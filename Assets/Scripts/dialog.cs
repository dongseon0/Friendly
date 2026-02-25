using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NavKeypad;    //Asset > Keypad > Scripts > KeypadModalController.cs
using nodedef;
using Newtonsoft.Json;

public class dialog : MonoBehaviour
{
    [Header("Load")]
    [SerializeField] private TextAsset scriptJson;  // Script json 파일 연결
    [SerializeField] private string startSceneId;
    [SerializeField] private string startNodeId;    //empty : [0]

    [Header("Refs")]
    [SerializeField] private UIController ui; // ui.ShowToast 만든 후 연결
    [SerializeField] private NavKeypad.Keypad keypad;
    [SerializeField] private NavKeypad.KeypadModalController modal;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    //Loaded Data
    public StoryData data { get; private set; }

    //Scene, node Lookup
    private readonly Dictionary<string, SceneDef> _scenesById = new();
    private readonly Dictionary<string, NodeDef> _nodesById = new();
    private readonly Dictionary<string, int> _nodeIndexById = new();
    private List<NodeDef> _currentNodes;
    private SceneDef _currentScene;

    //Flow control
    private string _nextNodeId;
    private string _pendingSceneID;
    private string _pendingSceneStartNodeId;

    private void Awake()
    {
        if (scriptJson != null)
        LoadFromTextAsset(scriptJson);
    }

    private void Start()
    {
        if (data == null) return;

        if (!string.IsNullOrEmpty(startSceneId))
        StartScene(startSceneId, startNodeId);
    }

    #region Loading
    public void LoadFromTextAsset(TextAsset json)
    {
        data = JsonConvert.DeserializeObject<StoryData>(json.text);
        BuildLookups();
    }

    private void BuildLookups()
    {
        _scenesById.Clear();

        if(data?.scenes == null)
        {
            Debug.LogError("StoryData.scenes is null");
            return;
        }

        foreach (var s in data.scenes)
        {
            if (string.IsNullOrEmpty(s.id)) continue;
            _scenesById[s.id] = s;
        }
    }

    #endregion

    #region scene entry
    public void StartScene(string sceneId, string nodeId = null)
    {
        if(!_scenesById.TryGetValue(sceneId, out var scene))
        {
            Debug.LogError($"Scene not found: {sceneId}");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(RunScene(scene, nodeId));
    }

    private IEnumerator RunScene(SceneDef scene, string startNodeOverride)
    {
        _currentScene = scene;
        _currentNodes = scene.nodes ?? new List<NodeDef>();

        // build node lookup for this scene
        _nodesById.Clear();
        _nodeIndexById.Clear();
        for (int i = 0; i < _currentNodes.Count; i++)
        {
            var n = _currentNodes[i];
            if (n == null || string.IsNullOrEmpty(n.id)) continue;
            _nodesById[n.id] = n;
            _nodeIndexById[n.id] = i;
        }

        // onEnter actions
        if (scene.onEnter != null && scene.onEnter.Count > 0)
            yield return RunCommands(scene.onEnter);

        // pick start node
        string nodeId = !string.IsNullOrEmpty(startNodeOverride)
            ? startNodeOverride
            : (_currentNodes.Count > 0 ? _currentNodes[0].id : null);

        yield return RunFlow(nodeId);
    }

    #endregion

    #region core flow loof
    private IEnumerator RunFlow(string nodeId)
    {
        while(!string.IsNullOrEmpty(nodeId))
        {
            if (!_nodesById.TryGetValue(nodeId, out var node) || node == null)
            {
                Debug.LogError($"Node not found in scene '{_currentScene?.id}': {nodeId}");
                yield break;
            }

            _nextNodeId = null;

            yield return RunNode(node);

            // scene transition requested?
            if (!string.IsNullOrEmpty(_pendingSceneId))
            {
                var nextSceneId = _pendingSceneId;
                var nextStartNode = _pendingSceneStartNodeId;

                _pendingSceneId = null;
                _pendingSceneStartNodeId = null;

                if (_scenesById.TryGetValue(nextSceneId, out var nextScene))
                {
                    yield return RunScene(nextScene, nextStartNode);
                    yield break; // RunScene 내부에서 다시 RunFlow를 시작하므로 여기서 종료
                }
                else
                {
                    Debug.LogError($"Next scene not found: {nextSceneId}");
                    yield break;
                }
            }

            nodeId = _nextNodeId;
        }
    } 

    private void Goto(string nextId) => _nextNodeId = nextId;

    private void GotoNextInScene(string currentNodeId)
    {
        if(_nodeIndexById.TryGetValue(currentNodeId, out var idx))
        {
            int nextIdx = idx+1;
            if (nextIdx>=0 && nextIdx < _currentNodes.Count)
                _nextNodeId = _currentNodes[nextIdx].id;
            else
                _nextNodeId = null;
        }
        else
        {
            _nextNodeId = null;
        }
    }

    private void GotoScene(string sceneId, string startNodeId = null)
    {
        _pendingSceneID = sceneId;
        _pendingSceneStartNodeId = startNodeId;
        _nextNodeId = null;
    }

    #endregion

    #region node dispatcher

    private IEnumerator RunNode(NodeDef node)
    {
        switch (node.type)
        {
            case NodeType.narration:
            {
                var n = (NarrationNode)node;
                yield return RunDialogueLike(null, Template(n.text));
                Goto(string.IsNullOrEmpty(node.next) ? NextIdOrNull(node.id) : node.next);
                break;
            }
            case NodeType.dialogue:
            {
                var d = (DialogueNode)node;
                yield return RunDialogueLike(Template(d.speaker), Template(d.text));
                Goto(string.IsNullOrEmpty(node.next) ? NextIdOrNull(node.id) : node.next);
                break;
            }
            case NodeType.pickup:
            {
                var p = (PickupNode)node;
                yield return RunPickup(p);
                // pickup은 JSON에 next가 없을 때가 많아서 “배열상 다음 노드”로 진행
                if (!string.IsNullOrEmpty(node.next)) Goto(node.next);
                else GotoNextInScene(node.id);
                break;
            }
            case NodeType.interaction:
            {
                var it = (InteractionNode)node;
                yield return RunInteraction(it);
                // whenInteract 안에서 dialogue next/goto 등이 발생하면 _nextNodeId가 설정될 것
                if (string.IsNullOrEmpty(_nextNodeId))
                {
                    if (!string.IsNullOrEmpty(node.next)) Goto(node.next);
                    else GotoNextInScene(node.id);
                }
                break;
            }
            case NodeType.action:
            {
                var a = (ActionNode)node;
                if (a.Do != null && a.Do.Count > 0)
                    yield return RunCommands(a.Do);

                if (!string.IsNullOrEmpty(node.next)) Goto(node.next);
                else GotoNextInScene(node.id);
                break;
            }
            case NodeType.choice:
            {
                var c = (ChoiceNode)node;
                yield return RunChoice(c);
                // RunChoice 내부에서 Goto를 결정
                break;
            }
            case NodeType.timeSkip:
            {
                var t = (TimeSkipNode)node;
                if (!string.IsNullOrEmpty(t.text))
                    yield return RunDialogueLike(null, Template(t.text));

                if (t.seconds > 0f)
                    yield return new WaitForSeconds(t.seconds);

                if (!string.IsNullOrEmpty(node.next)) Goto(node.next);
                else GotoNextInScene(node.id);
                break;
            }
            case NodeType.uiObjective:
            {
                var u = (UiObjectiveNode)node;
                if (ui != null) ui.ShowObjective(Template(u.text));
                else Debug.Log($"[Objective] {Template(u.text)}");

                // nextScene 필드는 NodeDef.cs에서 정확히 매핑돼 있어야 함 (아래 수정 목록 참고)
                var ns = GetNextScene(node);
                if (!string.IsNullOrEmpty(ns))
                    GotoScene(ns, node.nextNode);
                else
                    Goto(node.next); // fallback
                break;
            }
            case NodeType.ending:
            {
                var e = (EndingNode)node;
                // TODO: 엔딩 UI/씬 처리
                Debug.Log($"[ENDING] {e.endingId} {e.title}\n{e.text}");
                _nextNodeId = null;
                break;
            }
            default:
                Debug.LogError($"Unknown node type: {node.Type} ({node.id})");
                Goto(node.next);
                break;
        }
    }

    private string NextIdOrNull(string currentId)
    {
        if (_nodeIndexById.TryGetValue(currentId, out var idx))
        {
            int nextIdx = idx+1;
            return(nextIdx>= 0 && nextIdx < _currentNodes.Count) ? _currentNodes[nextIdx].id : null;
        }
        return null;
    }

    #endregion

    #region Nodehandlers

    private IEnumerator RunDialogueLike(string speaker, string text)
    {
        bool done = false;

        if(ui != null) ui.ShowDialogue(speaker, text, () => done = true);
        else { Debug.Log($"{speaker}: {text}"); done = true;}

        while (!done) yield return null;
    }

    private IEnumerator RunPickup(PickupNode node)
    {
        if (data == null || data.items == null)
        {
            Debug.LogError("Story data/items not loaded.");
            yield break;
        }

        if (!data.items.TryGetValue(node.itemId, out var item))
        {
            Debug.LogError($"Item not found: {node.itemId}");
            yield break;
        }
    
        // inventory add
        if (data.state.inventory == null) data.state.inventory = new List<string>();
        if (!data.state.inventory.Contains(node.itemId))
            data.state.inventory.Add(node.itemId);

        // flags/vars apply
        if (item.addFlags != null)
        {
            if (data.state.flags == null) data.state.flags = new Dictionary<string, bool>();
            foreach (var f in item.addFlags) data.state.flags[f] = true;
        }

        if (item.addVar != null)
        {
            if (data.state.vars == null) data.state.vars = new Dictionary<string, int>();
            foreach (var kv in item.addVar)
            {
                data.state.vars.TryGetValue(kv.Key, out int prev);
                data.state.vars[kv.Key] = prev + kv.Value;
            }
        }

        // randomly print inspect Line
        string line = PickRandomInspectLine(item);
        // and
        yield return RunDialogueLike(Template("{PC_NAME}"), Template(line));

        // Toast
        if (node.itemId == "PHOTO_BABY" && ui != null)
        {
            //Toast UI 만들어진 상태면 toast 출력됨
            ui.ShowToast("Safe Passcode Unlocked: 11985", 1.8f);
            yield return new WaitForSeconds(1.8f);
        }

        Debug.Log($Picked Up : {node.itemId});
    }

    private IEnumerator RunInteraction(InteractionNode node)
    {
        if (ui!=null) ui.ShowInteractHint(true, Template(node.target));

        while (true)
        {
            if(Input.GetKeyDown(interactKey) || Input.GetMouseButtonDown(0)) break;
            yield return null;
        }

        if (ui != null) ui.ShowInteractHint(false, Template(node.target));

        if(node.whenInteract != null && node.whenInteract.Count > 0)
            yield return RunCommands(node.whenInteract);
    }

    private IEnumerator RunChoice(ChoiceNode node)
    {
        if (node.choices == null || node.choices.Count == 0)
        {
            Debug.LogWarning($"Choice node has no options: {node.id}");
            if (!string.IsNullOrEmpty(node.next)) Goto(node.next);
            else GotoNextInScene(node.id);
            yield break;
        }

        bool done = false;
        int picked = -1;

        if (ui != null && ui.SupportsChoiceUI)
        {
            var texts = new List<string>();
            foreach (var c in node.choices) texts.Add(Template(c.text));
            ui.ShowChoice(Template(node.prompt), texts, idx => { picked = idx; done = true; });
        }
        else
        {
            // fallback: automatically pick first choice
            Debug.Log($"[CHOICE] {node.prompt} (fallback picks #0)");
            picked = 0;
            done = true;
        }

        while (!done) yield return null;

        if (picked < 0 || picked >= node.choices.Count) picked = 0;

        var opt = node.choices[picked];

        // apply effects
        if (opt.effects != null && opt.effects.Count > 0)
            yield return RunCommands(opt.effects);

        // go next
        if (!string.IsNullOrEmpty(opt.nextScene))
            GotoScene(opt.nextScene, opt.nextNode);
        else if (!string.IsNullOrEmpty(opt.next))
            Goto(opt.next);
        else
            GotoNextInScene(node.id);
    }

    #endregion

    #region Command Runner (whenInteract / onEnter / do / effects)

    private IEnumerator RunCommands(List<Command> cmds)
    {
        foreach (var cmd in cmds)
        {
            if(cmd == null) continue;

            switch(cmd.type)
            {
                case "setFlags" :
                {
                    if (data.state.flags == null) data.state.flags = new Dictionary<string, bool>();
                    data.state.flags[cmd.flag] = cmd.value ?? true;
                    break;
                }
                case "addVar":
                {
                    if (data.state.vars == null) data.state.vars = new Dictionary<string, int>();
                    data.state.vars.TryGetValue(cmd.var, out int prev);
                    data.state.vars[cmd.var] = prev + (cmd.delta ?? 0);
                    break;
                }
                case "narration":
                    yield return RunDialogueLike(null, Template(cmd.text));
                    break;
                case "dialogue":
                    yield return RunDialogueLike(Template(cmd.speaker), Template(cmd.text));
                    if (!string.IsNullOrEmpty(cmd.next))
                    {
                        Goto(cmd.next);
                        yield break; // 대사 액션이 next로 흐름을 끊는 경우
                    }
                    break;
                case "sound":
                    if (ui != null) ui.PlaySound(cmd.name);
                    else Debug.Log($"[SOUND] {cmd.name}");
                    break;
                case "video":
                    if (ui != null) yield return ui.PlayVideoAndWait(cmd.id);
                    else Debug.Log($"[VIDEO] {cmd.id} (no UI)");
                    break;
                case "light":
                    if (ui != null) ui.SetLight(cmd.mode, cmd.intensity ?? 1f);
                    break;
                case "vignette":
                    if (ui != null) ui.SetVignette(cmd.mode, cmd.strength ?? 0.5f);
                    break;
                case "branch":
                {
                    bool ok = EvalConditions(cmd.conditions);
                    var list = ok ? cmd.then : cmd.@else;
                    if (list != null && list.Count > 0)
                        yield return RunCommands(list);
                    break;
                }
                case "inputCode":
                    yield return RunInputCode(cmd);
                    break;
                case "goto":
                    Goto(cmd.next);
                    yield break;
                default:
                    Debug.LogWarning($"Unknown command type: {cmd.type}");
                    break;
            }
        }
    }

    private bool EvalConditions(List<Condition> conditions)
    {
        if (conditions == null || conditions.Count == 0) return true;
        if (data.state.flags == null) data.state.flags = new Dictionary<string, bool>();

        foreach (var c in conditions)
        {
            if (!string.IsNullOrEmpty(c.flag))
            {
                data.state.flags.TryGetValue(c.flag, out var has);
                if (!has) return false;
            }
            if (!string.IsNullOrEmpty(c.flagNot))
            {
                data.state.flags.TryGetValue(c.flagNot, out var has);
                if (has) return false;
            }
        }
        return true;
    }

    private IEnumerator RunInputCode(Command cmd)
    {
        if (keypad == null || modal == null)
        {
            Debug.LogError("Keypad/modal not assigned.");
            if (!string.IsNullOrEmpty(cmd.onCancel)) Goto(cmd.onCancel);
            yield break;
        }

        if (!int.TryParse(cmd.expected, out var combo))
        {
            Debug.LogError($"Invalid expected code: {cmd.expected}");
            if (!string.IsNullOrEmpty(cmd.onCancel)) Goto(cmd.onCancel);
            yield break;
        }

        // Keypad.cs에 SetCombo(int) 필요
        keypad.SetCombo(combo);

        modal.Open();

        bool done = false;
        string next = null;

        UnityAction granted = () => { done = true; next = cmd.onSuccess; };
        UnityAction denied  = () => { done = true; next = cmd.onFail; };
        UnityAction cancel  = () => { done = true; next = cmd.onCancel; };

        keypad.OnAccessGranted.AddListener(granted);
        keypad.OnAccessDenied.AddListener(denied);
        keypad.OnCanceled.AddListener(cancel);

        while (!done) yield return null;

        keypad.OnAccessGranted.RemoveListener(granted);
        keypad.OnAccessDenied.RemoveListener(denied);
        keypad.OnCanceled.RemoveListener(cancel);

        if (!string.IsNullOrEmpty(next))
        {
            Goto(next);
        }
    }

    #endregion

    #region Utilities
    private string Template(string s)
    {
        if (string.IsNullOrEmpty(s) || data?.meta?.variables == null) return s;

        // simple {KEY} replace
        foreach (var kv in data.meta.variables)
        {
            if (kv.Key == null) continue;
            string token = "{" + kv.Key + "}";
            if (s.Contains(token))
                s = s.Replace(token, kv.Value?.ToString() ?? "");
        }
        return s;
    }

    private string PickRandomInspectLine(ItemDef item)
    {
        if (item.onInspectLines == null || item.onInspectLines.Count == 0)
            return $"{item.name} obtained.";

        int idx = UnityEngine.Random.Range(0, item.onInspectLines.Count);
        return item.onInspectLines[idx];
    }

    private string GetNextScene(NodeDef node)
    {
        var prop = node.GetType().GetProperty("nextScene");
        if (prop != null)
            return prop.GetValue(node) as string;

        // fallback
        var prop2 = node.GetType().GetProperty("nextScene");
        if (prop2 != null)
            return prop2.GetValue(node) as string;

        return null;
    }

    #endregion
}

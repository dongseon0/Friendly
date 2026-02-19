using UnityEngine;

public class dialog
{
    private IEnumerator RunPickup(NodeDef node)
    {
        if (!data.items.TryGetValue(node.itemId, out var item))
        {
            Debug.LogError($"Item not found: {node.itemId}");
            Goto(node.next);
            yield break;
        }
    Flag
        // 1) 인벤토리 추가(중복 방지)
        if (!data.state.inventory.Contains(node.itemId))
            data.state.inventory.Add(node.itemId);

        // 2) 아이템 Flag (flags/vars)
        if (item.addFlags != null)
            foreach (var f in item.addFlags) data.state.flags[f] = true;

        if (item.addVar != null)
            foreach (var kv in item.addVar)
            {
                data.state.vars.TryGetValue(kv.Key, out int prev);
                data.state.vars[kv.Key] = prev + kv.Value;
            }

        // 3) onInspectLines 랜덤 1줄 선택
        string line = PickRandomInspectLine(item);

        // 4) 출력하고 다음으로
        bool done = false;
        ui.ShowDialogue(Template("{PC_NAME}"), Template(line), () => done = true);
        while (!done) yield return null;

        // 대사 이후 토스트
        if (node.itemId == "PHOTO_BABY")
        {
            //Toast UI 만들기
            ui.ShowToast("Safe Passcode Unlocked: 11985", 1.8f);
            yield return new WaitForSeconds(1.8f);
        }

        Goto(node.next);
    }

    private string PickRandomInspectLine(ItemDef item)
    {
        if (item.onInspectLines == null || item.onInspectLines.Count == 0)
            return $"{item.name} obtained.";

        int idx = UnityEngine.Random.Range(0, item.onInspectLines.Count);
        return item.onInspectLines[idx];
    }

    
}

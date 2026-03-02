using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ItemDataAutoFillOnCreate : UnityEditor.AssetModificationProcessor
{
    // StreamingAssets 안의 JSON 파일명 (네 파일명으로 변경)
    private const string JsonFileName = "SCRIPT_0209.json";

    // Game > Item 으로 생성되는 ItemData 에셋이 만들어지는 순간 호출됨
    static void OnWillCreateAsset(string assetPath)
    {
        // meta 파일 등 제외
        if (assetPath.EndsWith(".meta")) return;

        // Unity가 넘겨주는 경로는 보통 "Assets/..." 형태지만 ".asset" 확실히 체크
        string cleanPath = assetPath.Replace(".meta", "");
        if (!cleanPath.EndsWith(".asset")) return;

        // 먼저 에셋이 실제로 생성된 다음 LoadAssetAtPath가 가능하므로 한 프레임 뒤에 실행
        EditorApplication.delayCall += () =>
        {
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(cleanPath);
            if (itemData == null) return; // ItemData가 아니면 무시

            // 파일명으로 itemId 결정 (예: Assets/Items/S12.asset -> S12)
            string fileName = Path.GetFileNameWithoutExtension(cleanPath);
            string itemId = fileName;

            // JSON 로드
            Dictionary<string, ItemJsonData> jsonItems;
            try
            {
                jsonItems = ScriptLoader.LoadItems(JsonFileName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Item AutoFill] JSON 로드 실패: {e.Message}\n" +
                               $"StreamingAssets/{JsonFileName} 경로/파일 확인");
                return;
            }

            if (!jsonItems.ContainsKey(itemId))
            {
                Debug.LogWarning($"[Item AutoFill] JSON에 itemId '{itemId}' 없음. (asset: {cleanPath})");
                // itemId만이라도 넣어두고 끝낼지, 아예 아무것도 안할지 선택 가능
                Undo.RecordObject(itemData, "AutoFill ItemData (Set Id Only)");
                itemData.itemId = itemId;
                EditorUtility.SetDirty(itemData);
                AssetDatabase.SaveAssets();
                return;
            }

            // 값 채우기 (네가 이미 갖고 있던 FillItemData 그대로 활용)
            Undo.RecordObject(itemData, "AutoFill ItemData From JSON");
            itemData.itemId = itemId;
            ScriptLoader.FillItemData(itemData, itemId, jsonItems);

            EditorUtility.SetDirty(itemData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Item AutoFill] '{itemId}' 자동 채움 완료! (asset: {cleanPath})");
        };
    }
}
///////////////////<summary>///////////////////////
///This script automatically fills the fields of a newly created ItemData asset based on the corresponding item
///How to use:
///1st: Create a new ItemData asset (Right Click in Project Window -> Create -> Game -> Item).
///2nd: The script will automatically fill the new ItemData's fields based on the corresponding itemId in the JSON file (SCRIPT_0209.json).
///3rd: Make sure the JSON file is correctly placed in the StreamingAssets folder and contains the necessary item data with matching itemIds.
///4th: If the itemId from the asset's file name does not exist in the JSON, only the itemId field will be filled, and a warning will be logged.
///5th: Check the Console for logs about the auto-fill process and any potential issues with loading the JSON or missing itemIds.
///6th: After creation, you can further customize the ItemData asset in the Inspector as needed.
///7th: This script helps streamline the creation of ItemData assets by reducing manual data entry and ensuring consistency with the JSON data source.
///8th: Remember to save your project after creating new ItemData assets to ensure all changes are preserved.
///9th: If you encounter any issues, check the Console for error messages related to JSON loading or asset creation, and verify that the JSON file is correctly formatted and accessible.
///10th: This script is intended for use in the Unity Editor and will not affect runtime behavior. It is designed to enhance the workflow of creating ItemData assets by automating the population of fields based on a predefined JSON structure.
//////////////////<summary>//////////////////////
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ItemDataAutoFillOnCreate : UnityEditor.AssetModificationProcessor
{
    private const string JsonFileName = "SCRIPT_0209.json";

    static void OnWillCreateAsset(string assetPath)
    {
        // except meta files and non-asset files
        if (assetPath.EndsWith(".meta")) return;

        string cleanPath = assetPath.Replace(".meta", "");
        if (!cleanPath.EndsWith(".asset")) return;

        EditorApplication.delayCall += () =>
        {
            var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(cleanPath);
            if (itemData == null) return; // ignore if not ItemData

            // determine itemId from file name (without extension)
            string fileName = Path.GetFileNameWithoutExtension(cleanPath);
            string itemId = fileName;

            // load JSON and find matching itemId
            Dictionary<string, ItemJsonData> jsonItems;
            try
            {
                jsonItems = ScriptLoader.LoadItems(JsonFileName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Fall to load [Item AutoFill] JSON: {e.Message}\n" +
                               $"Check the address of file of StreamingAssets/{JsonFileName}");
                return;
            }

            if (!jsonItems.ContainsKey(itemId))
            {
                Debug.LogWarning($"There is no itemId at [Item AutoFill] Json. (asset: {cleanPath})");
                // itemId만이라도 넣어두고 끝낼지, 아예 아무것도 안할지 선택 가능
                Undo.RecordObject(itemData, "AutoFill ItemData (Set Id Only)");
                itemData.itemId = itemId;
                EditorUtility.SetDirty(itemData);
                AssetDatabase.SaveAssets();
                return;
            }

            // Fill ItemData from JSON
            Undo.RecordObject(itemData, "AutoFill ItemData From JSON");
            itemData.itemId = itemId;
            ScriptLoader.FillItemData(itemData, itemId, jsonItems);

            EditorUtility.SetDirty(itemData);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Item AutoFill] '{itemId}' 자동 채움 완료! (asset: {cleanPath})");
        };
    }
}
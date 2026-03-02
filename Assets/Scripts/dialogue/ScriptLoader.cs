using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ItemJsonData
{
    public string name;
    public string desc;
    public string[] onInspectLines;
    public string[] addFlags;
    public Dictionary<string, int> addVar;
}

[System.Serializable]
public class ScriptJsonRoot
{
    public Dictionary<string, ItemJsonData> items;
}

public class ScriptLoader
{
    public static Dictionary<string, ItemJsonData> LoadItems(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        string json = File.ReadAllText(path);

        var root = JsonConvert.DeserializeObject<ScriptJsonRoot>(json);
        return root.items;
    }

    public static void FillItemData(ItemData itemData, string itemKey, Dictionary<string, ItemJsonData> jsonItems)
    {
        if (!jsonItems.ContainsKey(itemKey))
        {
            Debug.LogError("Item not found in JSON: " + itemKey);
            return;
        }

        var jsonItem = jsonItems[itemKey];

        itemData.itemId = itemKey;
        itemData.displayName = jsonItem.name;
        itemData.description = jsonItem.desc;
    }
}
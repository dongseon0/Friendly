using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item", fileName = "Item_")]
public class ItemData : ScriptableObject
{
    public string itemId;
    public string displayName;
    [TextArea(3, 8)] public string description;
    public Sprite icon;
}
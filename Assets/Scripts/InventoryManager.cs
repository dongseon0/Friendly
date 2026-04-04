using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject inventoryPanel;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;             
    [SerializeField] private string playerMap = "Player";
    [SerializeField] private string uiMap = "UI";

    public static InventoryManager Instance;

    public event Action OnInventoryChanged;

    [SerializeField] private List<ItemData> items = new();
    [SerializeField] private ItemData debugItem;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;  
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (debugItem != null)
            AddItem(debugItem);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = FindFirstObjectByType<PlayerInput>();
        if (player != null)
        {
            playerInput = player;
            Debug.Log($"[Inventory] rebound PlayerInput={playerInput.name}");
        }
        else
        {
            Debug.LogWarning("[Inventory] PlayerInput not found on scene load");
        }

        if (playerInput)
            playerInput.SwitchCurrentActionMap(IsOpen ? uiMap : playerMap);
    }

    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    public IReadOnlyList<ItemData> Items => items;

    public void AddItem(ItemData item)
    {
        if (item == null) return;
        items.Add(item);
        OnInventoryChanged?.Invoke();
    }

    public void ToggleInventory()
    {
        if (!inventoryPanel) return;
        SetOpen(!inventoryPanel.activeSelf);
    }

   public void SetOpen(bool open)
    {
        Debug.Log($"[Inventory] REQUEST open={open} beforeActive={inventoryPanel.activeSelf} map={playerInput?.currentActionMap?.name}");

        inventoryPanel.SetActive(open);

        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        if (playerInput)
            playerInput.SwitchCurrentActionMap(open ? uiMap : playerMap);

        Debug.Log($"[Inventory] DONE open={open} afterActive={inventoryPanel.activeSelf} map={playerInput?.currentActionMap?.name}");
    }
}
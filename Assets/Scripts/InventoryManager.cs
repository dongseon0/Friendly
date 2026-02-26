using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    [Header("UI Root (Panel)")]
    public GameObject inventoryPanel;

    [Header("Input")]
    public PlayerInput playerInput;                 
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
        // 매 씬마다 PlayerInput 다시 연결 (DDOL 필수)
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) playerInput = player.GetComponent<PlayerInput>();

        // 씬 로드 직후 인벤이 열려있으면 UI맵 유지, 닫혀있으면 Player맵
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
        inventoryPanel.SetActive(open);
        
        Debug.Log($"[Inventory] open={open} map={playerInput?.currentActionMap?.name}");
        // 인벤 열면 멈추고 커서 풀기
        Time.timeScale = open ? 0f : 1f;
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;

        // Look/Move 입력 차단 = UI맵으로 스왑
        if (playerInput)
            playerInput.SwitchCurrentActionMap(open ? uiMap : playerMap);
    }
}
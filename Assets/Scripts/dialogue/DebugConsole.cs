using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject consoleRoot;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text logText;

    [Header("Refs")]
    [SerializeField] private dialog dialogSystem;

    private bool isOpen = false;
    private List<string> logs = new();

    private void Awake()
    {
        if (consoleRoot) consoleRoot.SetActive(false);

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void Update()
    {
        // Alt + Shift 토글
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleConsole();
        }

        if (!isOpen) return;

        // Enter 입력
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitCommand();
        }
    }

    private void ToggleConsole()
    {
        isOpen = !isOpen;

        if (consoleRoot)
            consoleRoot.SetActive(isOpen);

        if (isOpen && inputField)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    private void SubmitCommand()
    {
        if (inputField == null) return;

        string cmd = inputField.text.Trim();
        inputField.text = "";

        ExecuteCommand(cmd);
    }

    private void CloseConsole()
    {
        isOpen = false;

        if (consoleRoot)
            consoleRoot.SetActive(false);
    }

    private void ExecuteCommand(string cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return;

        Debug.Log($"[CMD] {cmd}");

        switch (cmd.ToLower())
        {
            case "/nextnodeadmin":
                if (dialogSystem == null)
                {
                    Debug.LogError("[CMD] dialogSystem not assigned");
                    return;
                }
                dialogSystem.SkipToNextNode();
                break;

            case "/nextsceneadmin":
                if (dialogSystem == null)
                {
                    Debug.LogError("[CMD] dialogSystem not assigned");
                    return;
                }
                dialogSystem.SkipToNextScene();
                break;

            case "/showlog":
                ShowLogs();
                break;

            case "/closethis":
                CloseConsole();
                break;

            default:
                Debug.LogWarning($"[CMD] Unknown command: {cmd}");
                break;
        }
    }

    // -------------------------
    // 로그 수집
    // -------------------------
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logs.Add(logString);

        if (logs.Count > 100)
            logs.RemoveAt(0);
    }

    private void ShowLogs()
    {
        if (logText == null)
        {
            Debug.LogWarning("[CMD] logText UI not assigned");
            return;
        }

        logText.text = string.Join("\n", logs);
    }
}

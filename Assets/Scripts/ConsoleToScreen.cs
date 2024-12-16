using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ConsoleToScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text consoleText; // Assign a UI Text element in the inspector.
    [SerializeField] private int maxMessages = 50; // Maximum number of messages to display.

    private Queue<string> messageQueue = new Queue<string>();

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string formattedMessage = string.Empty;

        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                formattedMessage = $"<color=red>{logString}</color>";
                break;
            case LogType.Warning:
                formattedMessage = $"<color=yellow>{logString}</color>";
                break;
            default:
                formattedMessage = logString;
                break;
        }

        messageQueue.Enqueue(formattedMessage);

        if (messageQueue.Count > maxMessages)
        {
            messageQueue.Dequeue();
        }

        consoleText.text = string.Join("\n", messageQueue);
    }
}

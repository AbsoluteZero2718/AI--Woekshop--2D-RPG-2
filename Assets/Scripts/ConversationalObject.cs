using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Attach to any object to turn it into a conversational partner.
/// Wire up UI references (or use the optional world-space speech bubble),
/// set an API key, and type prompts to chat naturally with the object.
/// </summary>
[DisallowMultipleComponent]
public class ConversationalObject : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string characterName = "Guide";
    [TextArea(3, 6)]
    [SerializeField] private string personality =
        "You are a friendly, curious object in a Unity scene. " +
        "Reply in 1–3 short sentences. Stay in character, ask follow-up questions when it fits, " +
        "and remember what the user said earlier in this chat.";

    [Header("LLM (OpenAI-compatible)")]
    [SerializeField] private string apiEndpoint = "https://api.openai.com/v1/chat/completions";
    [SerializeField] private string apiKey = "";
    [SerializeField] private string model = "gpt-4o-mini";
    [SerializeField] private int maxHistoryMessages = 20;
    [SerializeField] private bool logDetailedErrors = true;

    [Header("Chat UI")]
    [SerializeField] private TMP_InputField promptInput;
    [SerializeField] private TMP_Text chatLog;
    [SerializeField] private ScrollRect chatScrollRect;
    [SerializeField] private Button sendButton;
    [SerializeField] private string userLabel = "You";
    [SerializeField] private Color userColor = new Color(0.55f, 0.78f, 1f);
    [SerializeField] private Color objectColor = new Color(0.95f, 0.85f, 0.55f);

    [Header("World Feedback (optional)")]
    [SerializeField] private TMP_Text speechBubble;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float thinkPulseSpeed = 4f;
    [SerializeField] private float thinkPulseAmount = 0.04f;
    [SerializeField] private float speakBobSpeed = 6f;
    [SerializeField] private float speakBobAmount = 0.03f;

    [Header("Conversation Feel")]
    [SerializeField] private float minThinkDelay = 0.35f;
    [SerializeField] private float maxThinkDelay = 1.1f;
    [SerializeField] private float charactersPerSecond = 42f;
    [SerializeField] private bool greetOnStart = true;
    [TextArea(1, 3)]
    [SerializeField] private string greeting =
        "Hey there! I'm here if you want to talk — what's on your mind?";

    [Header("Events")]
    public UnityEvent<string> onUserMessageSent;
    public UnityEvent<string> onObjectResponseComplete;

    private readonly List<ChatMessage> conversationHistory = new List<ChatMessage>();
    private readonly StringBuilder chatBuffer = new StringBuilder();
    private Vector3 visualBaseScale = Vector3.one;
    private Vector3 visualBaseLocalPosition;
    private bool isBusy;
    private Coroutine activeConversationRoutine;

    [Serializable]
    private struct ChatMessage
    {
        public string role;
        public string content;

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    [Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessageDto[] messages;
    }

    [Serializable]
    private class ChatMessageDto
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class ChatResponse
    {
        public Choice[] choices;
    }

    [Serializable]
    private class Choice
    {
        public ChatMessageDto message;
    }

    [Serializable]
    private class ApiErrorResponse
    {
        public ApiErrorBody error;
    }

    [Serializable]
    private class ApiErrorBody
    {
        public string message;
        public string type;
    }

    private string ResolvedApiKey =>
        string.IsNullOrWhiteSpace(apiKey)
            ? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            : apiKey.Trim();

    private bool RequiresApiKey =>
        !apiEndpoint.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) &&
        !apiEndpoint.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase);

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        visualBaseScale = visualRoot.localScale;
        visualBaseLocalPosition = visualRoot.localPosition;

        if (sendButton != null)
            sendButton.onClick.AddListener(SubmitPrompt);

        if (promptInput != null)
            promptInput.onSubmit.AddListener(OnInputSubmit);
    }

    private void Start()
    {
        if (greetOnStart && !string.IsNullOrWhiteSpace(greeting))
            StartCoroutine(PlayGreeting());
    }

    private void OnDestroy()
    {
        if (sendButton != null)
            sendButton.onClick.RemoveListener(SubmitPrompt);

        if (promptInput != null)
            promptInput.onSubmit.RemoveListener(OnInputSubmit);
    }

    private void OnInputSubmit(string _)
    {
        SubmitPrompt();
    }

    public void SubmitPrompt()
    {
        if (promptInput == null || isBusy)
            return;

        string prompt = promptInput.text.Trim();
        if (string.IsNullOrEmpty(prompt))
            return;

        promptInput.text = string.Empty;
        promptInput.ActivateInputField();

        if (activeConversationRoutine != null)
            StopCoroutine(activeConversationRoutine);

        activeConversationRoutine = StartCoroutine(ConversationRoutine(prompt));
    }

    public void ClearConversation()
    {
        if (activeConversationRoutine != null)
        {
            StopCoroutine(activeConversationRoutine);
            activeConversationRoutine = null;
        }

        conversationHistory.Clear();
        isBusy = false;
        ResetVisualFeedback();
        chatBuffer.Clear();
        RefreshChatLog();

        if (speechBubble != null)
            speechBubble.text = string.Empty;
    }

    private IEnumerator PlayGreeting()
    {
        isBusy = true;
        SetInputInteractable(false);
        yield return TypeObjectReply(greeting, recordInHistory: false);
        isBusy = false;
        SetInputInteractable(true);
    }

    private IEnumerator ConversationRoutine(string userPrompt)
    {
        isBusy = true;
        SetInputInteractable(false);

        AppendToChatLog(userLabel, userPrompt, userColor);
        onUserMessageSent?.Invoke(userPrompt);

        conversationHistory.Add(new ChatMessage("user", userPrompt));
        TrimHistory();

        yield return ShowThinkingState();

        string reply = null;
        yield return RequestReply(result => reply = result);

        if (string.IsNullOrWhiteSpace(reply))
            reply = "Sorry — I lost my train of thought. Could you say that again?";

        conversationHistory.Add(new ChatMessage("assistant", reply));
        TrimHistory();

        yield return TypeObjectReply(reply, recordInHistory: true);

        isBusy = false;
        SetInputInteractable(true);
        activeConversationRoutine = null;

        if (promptInput != null)
            promptInput.ActivateInputField();
    }

    private IEnumerator RequestReply(Action<string> onComplete)
    {
        string resolvedKey = ResolvedApiKey;

        if (RequiresApiKey && string.IsNullOrWhiteSpace(resolvedKey))
        {
            onComplete?.Invoke(
                "I'd love to chat, but I need an API key first. " +
                "Add one on the ConversationalObject component in the Inspector " +
                "(or set the OPENAI_API_KEY environment variable).");
            yield break;
        }

        var payload = BuildRequestPayload();
        string json = JsonUtility.ToJson(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        if (logDetailedErrors)
            Debug.Log($"ConversationalObject: POST {apiEndpoint} (model: {model})");

        using (var request = new UnityWebRequest(apiEndpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrWhiteSpace(resolvedKey))
                request.SetRequestHeader("Authorization", "Bearer " + resolvedKey);

            yield return request.SendWebRequest();

            string responseBody = request.downloadHandler?.text ?? string.Empty;

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                LogApiFailure(request, responseBody);
                onComplete?.Invoke(BuildConnectionErrorMessage(request.error));
                yield break;
            }

            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                LogApiFailure(request, responseBody);
                onComplete?.Invoke(BuildProtocolErrorMessage(request.responseCode, responseBody));
                yield break;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                LogApiFailure(request, responseBody);
                onComplete?.Invoke("Hmm, I'm having trouble reaching my thoughts right now. Try again in a moment?");
                yield break;
            }

            string reply = ParseReply(responseBody);
            if (string.IsNullOrWhiteSpace(reply))
            {
                if (logDetailedErrors)
                    Debug.LogWarning($"ConversationalObject: HTTP 200 but no reply parsed. Body:\n{responseBody}");

                onComplete?.Invoke("Sorry — I got a response but couldn't understand it. Check the Console for details.");
                yield break;
            }

            onComplete?.Invoke(reply);
        }
    }

    private void LogApiFailure(UnityWebRequest request, string responseBody)
    {
        if (!logDetailedErrors)
            return;

        Debug.LogWarning(
            "ConversationalObject API request failed.\n" +
            $"URL: {apiEndpoint}\n" +
            $"Model: {model}\n" +
            $"Result: {request.result}\n" +
            $"HTTP: {request.responseCode}\n" +
            $"Error: {request.error}\n" +
            $"Body: {responseBody}");
    }

    private string BuildConnectionErrorMessage(string unityError)
    {
        if (apiEndpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            return
                "I can't reach the local LLM server. If you're using Ollama, make sure it's running " +
                "(try `ollama serve` in a terminal) and set Player Settings → Allow downloads over HTTP → " +
                "Always Allowed. Details are in the Console.";
        }

        return
            "I can't reach the API right now — check your internet connection and firewall, " +
            "then look at the Console for the exact error.";
    }

    private static string BuildProtocolErrorMessage(long responseCode, string responseBody)
    {
        string apiMessage = TryExtractApiErrorMessage(responseBody);

        switch (responseCode)
        {
            case 401:
                return string.IsNullOrEmpty(apiMessage)
                    ? "My API key doesn't look valid. Create a new key at platform.openai.com/api-keys and paste it into the Inspector."
                    : $"API key problem: {apiMessage}";

            case 402:
            case 429:
                return string.IsNullOrEmpty(apiMessage)
                    ? "The API is rate-limiting or out of credits. Add billing at platform.openai.com/account/billing."
                    : apiMessage;

            case 404:
                return string.IsNullOrEmpty(apiMessage)
                    ? "That API endpoint or model wasn't found. Check Api Endpoint and Model in the Inspector."
                    : apiMessage;

            default:
                return string.IsNullOrEmpty(apiMessage)
                    ? $"The API returned an error (HTTP {responseCode}). Check the Unity Console for details."
                    : $"API error (HTTP {responseCode}): {apiMessage}";
        }
    }

    private static string TryExtractApiErrorMessage(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            var parsed = JsonUtility.FromJson<ApiErrorResponse>(responseBody);
            if (!string.IsNullOrWhiteSpace(parsed?.error?.message))
                return parsed.error.message.Trim();
        }
        catch (Exception)
        {
            // Ignore malformed error payloads.
        }

        return null;
    }

    private ChatRequest BuildRequestPayload()
    {
        var messages = new List<ChatMessageDto>
        {
            new ChatMessageDto
            {
                role = "system",
                content = BuildSystemPrompt()
            }
        };

        foreach (ChatMessage message in conversationHistory)
        {
            messages.Add(new ChatMessageDto
            {
                role = message.role,
                content = message.content
            });
        }

        return new ChatRequest
        {
            model = model,
            messages = messages.ToArray()
        };
    }

    private string BuildSystemPrompt()
    {
        return
            $"{personality}\n\n" +
            $"Your name is {characterName}. " +
            "Speak in first person as this object. " +
            "Never mention that you are an AI, language model, or API. " +
            "Keep replies conversational, warm, and concise.";
    }

    private static string ParseReply(string responseJson)
    {
        try
        {
            var response = JsonUtility.FromJson<ChatResponse>(responseJson);
            if (response?.choices != null &&
                response.choices.Length > 0 &&
                response.choices[0].message != null)
            {
                return response.choices[0].message.content.Trim();
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"ConversationalObject failed to parse response: {exception.Message}");
        }

        return null;
    }

    private IEnumerator ShowThinkingState()
    {
        float delay = UnityEngine.Random.Range(minThinkDelay, maxThinkDelay);
        float elapsed = 0f;

        if (speechBubble != null)
            speechBubble.text = "…";

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;
            float pulse = Mathf.Sin(elapsed * thinkPulseSpeed) * thinkPulseAmount;
            visualRoot.localScale = visualBaseScale * (1f + pulse);
            yield return null;
        }

        visualRoot.localScale = visualBaseScale;
    }

    private IEnumerator TypeObjectReply(string reply, bool recordInHistory)
    {
        AppendToChatLog(characterName, string.Empty, objectColor, appendOnly: true);
        int lineStartIndex = chatBuffer.Length;

        if (speechBubble != null)
            speechBubble.text = string.Empty;

        float charDelay = charactersPerSecond > 0f ? 1f / charactersPerSecond : 0f;
        var typed = new StringBuilder();

        for (int i = 0; i < reply.Length; i++)
        {
            typed.Append(reply[i]);
            chatBuffer.Length = lineStartIndex;
            chatBuffer.Append(typed);
            RefreshChatLog();

            if (speechBubble != null)
                speechBubble.text = typed.ToString();

            float bob = Mathf.Sin(Time.time * speakBobSpeed) * speakBobAmount;
            visualRoot.localPosition = visualBaseLocalPosition + Vector3.up * bob;

            if (charDelay > 0f)
                yield return new WaitForSeconds(charDelay);
            else
                yield return null;
        }

        visualRoot.localPosition = visualBaseLocalPosition;

        if (recordInHistory)
            onObjectResponseComplete?.Invoke(reply);
    }

    private void AppendToChatLog(string speaker, string message, Color color, bool appendOnly = false)
    {
        if (!appendOnly && chatBuffer.Length > 0)
            chatBuffer.AppendLine().AppendLine();

        chatBuffer.Append("<color=#")
            .Append(ColorUtility.ToHtmlStringRGB(color))
            .Append("><b>")
            .Append(EscapeRichText(speaker))
            .Append(":</b></color> ");

        if (!string.IsNullOrEmpty(message))
            chatBuffer.Append(EscapeRichText(message));

        RefreshChatLog();
    }

    private void RefreshChatLog()
    {
        if (chatLog == null)
            return;

        chatLog.text = chatBuffer.ToString();

        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void SetInputInteractable(bool interactable)
    {
        if (promptInput != null)
            promptInput.interactable = interactable;

        if (sendButton != null)
            sendButton.interactable = interactable;
    }

    private void ResetVisualFeedback()
    {
        visualRoot.localScale = visualBaseScale;
        visualRoot.localPosition = visualBaseLocalPosition;
    }

    private void TrimHistory()
    {
        while (conversationHistory.Count > maxHistoryMessages)
            conversationHistory.RemoveAt(0);
    }

    private static string EscapeRichText(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}

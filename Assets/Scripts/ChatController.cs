using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements; 
using NativeWebSocket;

public class ChatController : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string websocketUrl = "ws://localhost:8080"; 

    private UIDocument _uiDocument;
    private VisualElement _loginScreen;
    private VisualElement _chatScreen;
    private TextField _nickInput;
    private TextField _messageInput;
    private ScrollView _chatHistory;

    // --- Datos ---
    private WebSocket websocket;
    private string _myNick = "";

    void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        
        _loginScreen = root.Q<VisualElement>("Login");      
        _nickInput = root.Q<TextField>("NickInput");      
        var joinBtn = root.Q<Button>("JoinBtn");            

        _chatScreen = root.Q<VisualElement>("ChatScreen"); 
        _chatHistory = root.Q<ScrollView>("ChatHistory"); 
        
        _messageInput = root.Q<TextField>("MessageBtn");    
        var sendBtn = root.Q<Button>("SendBtn");            

        if (joinBtn != null) joinBtn.clicked += OnJoinClicked;
        if (sendBtn != null) sendBtn.clicked += OnSendClicked;

        if (_loginScreen != null) _loginScreen.style.display = DisplayStyle.Flex;
        if (_chatScreen != null) _chatScreen.style.display = DisplayStyle.None;
    }

    private async void OnJoinClicked()
    {
        if (_nickInput == null || string.IsNullOrEmpty(_nickInput.value)) return;

        _myNick = _nickInput.value;

        if (_loginScreen != null) _loginScreen.style.display = DisplayStyle.None;
        if (_chatScreen != null) _chatScreen.style.display = DisplayStyle.Flex;

        await ConnectWebSocket();
    }

    private void OnSendClicked()
    {
        if (_messageInput == null || string.IsNullOrEmpty(_messageInput.value)) return;
        
        SendMessageToServer(_messageInput.value);
        
        _messageInput.value = "";
    }

    private void AddMessageToChat(string text)
    {
        if (_chatHistory == null) return;

        Label newMsg = new Label(text);
        newMsg.style.fontSize = 16;
        newMsg.style.marginBottom = 5;
        newMsg.style.color = Color.white; 

        _chatHistory.Add(newMsg);

        _chatHistory.schedule.Execute(() => 
        {
            _chatHistory.scrollOffset = new Vector2(0, float.MaxValue);
        });
    }


    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket(websocketUrl);

        websocket.OnOpen += () => Debug.Log("Conectado al servidor local!");
        websocket.OnError += (e) => Debug.LogError("Error: " + e);
        websocket.OnClose += (e) => Debug.Log("Cerrado");

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            AddMessageToChat(message);
        };

        await websocket.Connect();
        
        AddMessageToChat($"--- Te has unido como {_myNick} ---");
    }

    private async void SendMessageToServer(string text)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            string fullMessage = $"{_myNick}: {text}";
            await websocket.SendText(fullMessage);
        }
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null) websocket.DispatchMessageQueue();
        #endif
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null) await websocket.Close();
    }
}
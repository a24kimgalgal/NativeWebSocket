using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour {
    [Header("Configuració del WebSocket")]
    [SerializeField]
    private string websocketUrl = "ws://localhost:8080"; // Exemple de URL

    [Header("Tests des de l'editor")]
    [SerializeField]
    private string testMessage = "Hola des de Unity!";

    private TextElement messageDisplay;

    // Instància del client WebSocket.
    WebSocket websocket;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start() {
        // Inicialitza la connexió al WebSocket.
        await ConnectWebSocket();
    }

    private async Task ConnectWebSocket() {
        // Crea una nova instància de WebSocket amb la URL especificada.
        websocket = new WebSocket(websocketUrl);

        // Callback: Quan la connexió s'obre.
        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connectat!");
        };

        // Callback: Quan es rep un error.
        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        // Callback: Quan la connexió es tanca.
        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket tancat!");
        };

        // Callback: Quan es rep un missatge.
        websocket.OnMessage += (bytes) =>
        {
            // Converteix els bytes a una cadena.
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Missatge rebut: " + message);
        };

        // Inicia la connexió de manera asíncrona.
        await websocket.Connect();
    }

    public async void SendWebSocketMessage(string message) {
        if (websocket != null && websocket.State == WebSocketState.Open) {
            await websocket.SendText(message);
            Debug.Log("Missatge enviat: " + message);
        } else {
            Debug.LogWarning("No es pot enviar el missatge: connexió no oberta.");
        }
    }

    // Update is called once per frame
    void Update() {
        // Necessari per gestionar la cua de missatges en entorns WebGL i altres plataformes.
        if (websocket != null) {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnApplicationQuit() {
        // Tanca la connexió quan l'aplicació es tanca.
        if (websocket != null) {
            await websocket.Close();
        }
    }

    // Aquesta farà servir el camp serialitzat
    public void SendSerializedMessage() {
        if (string.IsNullOrEmpty(testMessage)) {
            Debug.LogWarning("testMessage està buit, no envio res.");
            return;
        }
        SendWebSocketMessage(testMessage);
    }

    // Afegeix l'opció al menú contextual de l'Inspector
    [ContextMenu("WebSocket/Enviar missatge de prova")]
    private void ContextSendTestMessage() {
        SendSerializedMessage();
    }
}
using CBS.Models;
using CBS.SignalR.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CBS
{
    public class CBSSignalRModule : CBSModule, ISignalRConnection
    {
        // Events
        public event Action<SignalRChatMessage> OnReceiveTextMessage;
        public event Action<SignalRChatMessage> OnReceiveStickerMessage;
        public event Action<SignalRUserTyping> OnUserTyping;
        public event Action<SignalRUserChatAction> OnUserJoinedChat;
        public event Action<SignalRUserChatAction> OnUserLeftChat;

        // Properties
        public bool IsConnected { get; private set; }
        public string ServerUrl { get; private set; }
        public string AccessToken { get; private set; }

        // Private fields
        private Dictionary<string, Action<bool>> PendingCallbacks { get; set; }
        private ICoroutineRunner CoroutineRunner { get; set; }
        private IProfile Profile { get; set; }

        protected override void OnInitialize()
        {
            PendingCallbacks = new Dictionary<string, Action<bool>>();
            CoroutineRunner = CBSModule.Get<CBSMainModule>();
            Profile = CBSModule.Get<CBSProfileModule>();
        }

        public void Connect(string serverUrl, string accessToken)
        {
            ServerUrl = serverUrl;
            AccessToken = accessToken;

            // Start connection process
            CoroutineRunner.StartCoroutine(ConnectToSignalR());
        }

        private IEnumerator ConnectToSignalR()
        {
            // For demo purposes, we'll simulate SignalR connection
            // In real implementation, you would use a SignalR client library
            
            Debug.Log("Connecting to SignalR server...");
            
            // Simulate connection delay
            yield return new WaitForSeconds(1f);
            
            IsConnected = true;
            Debug.Log("Connected to SignalR server");
            
            // Start listening for messages
            CoroutineRunner.StartCoroutine(ListenForMessages());
        }

        public void Disconnect()
        {
            IsConnected = false;
            Debug.Log("Disconnected from SignalR server");
        }

        public void JoinChat(string chatID)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot join chat: not connected to SignalR");
                return;
            }

            Debug.Log($"Joining chat: {chatID}");
            
            // In real implementation, this would send a SignalR message
            // For now, we'll simulate it
            CoroutineRunner.StartCoroutine(SendJoinChatRequest(chatID));
        }

        private IEnumerator SendJoinChatRequest(string chatID)
        {
            var request = new UnityWebRequest($"{ServerUrl}/chat/join", "POST");
            request.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            
            var requestData = new { ChatID = chatID };
            var jsonData = JsonUtility.ToJson(requestData);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully joined chat: {chatID}");
            }
            else
            {
                Debug.LogError($"Failed to join chat: {request.error}");
            }
        }

        public void LeaveChat(string chatID)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot leave chat: not connected to SignalR");
                return;
            }

            Debug.Log($"Leaving chat: {chatID}");
            
            // In real implementation, this would send a SignalR message
            CoroutineRunner.StartCoroutine(SendLeaveChatRequest(chatID));
        }

        private IEnumerator SendLeaveChatRequest(string chatID)
        {
            var request = new UnityWebRequest($"{ServerUrl}/chat/leave", "POST");
            request.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            request.SetRequestHeader("Content-Type", "application/json");
            
            var requestData = new { ChatID = chatID };
            var jsonData = JsonUtility.ToJson(requestData);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Successfully left chat: {chatID}");
            }
            else
            {
                Debug.LogError($"Failed to leave chat: {request.error}");
            }
        }

        public void SendTextMessage(SignalRTextMessageRequest request, Action<bool> callback = null)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot send message: not connected to SignalR");
                callback?.Invoke(false);
                return;
            }

            var messageId = Guid.NewGuid().ToString();
            if (callback != null)
            {
                PendingCallbacks[messageId] = callback;
            }

            CoroutineRunner.StartCoroutine(SendTextMessageRequest(request, messageId));
        }

        private IEnumerator SendTextMessageRequest(SignalRTextMessageRequest request, string messageId)
        {
            var webRequest = new UnityWebRequest($"{ServerUrl}/chat/sendText", "POST");
            webRequest.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            var jsonData = JsonUtility.ToJson(request);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Text message sent successfully: {messageId}");
                if (PendingCallbacks.ContainsKey(messageId))
                {
                    PendingCallbacks[messageId]?.Invoke(true);
                    PendingCallbacks.Remove(messageId);
                }
            }
            else
            {
                Debug.LogError($"Failed to send text message: {webRequest.error}");
                if (PendingCallbacks.ContainsKey(messageId))
                {
                    PendingCallbacks[messageId]?.Invoke(false);
                    PendingCallbacks.Remove(messageId);
                }
            }
        }

        public void SendStickerMessage(SignalRStickerMessageRequest request, Action<bool> callback = null)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot send sticker: not connected to SignalR");
                callback?.Invoke(false);
                return;
            }

            var messageId = Guid.NewGuid().ToString();
            if (callback != null)
            {
                PendingCallbacks[messageId] = callback;
            }

            CoroutineRunner.StartCoroutine(SendStickerMessageRequest(request, messageId));
        }

        private IEnumerator SendStickerMessageRequest(SignalRStickerMessageRequest request, string messageId)
        {
            var webRequest = new UnityWebRequest($"{ServerUrl}/chat/sendSticker", "POST");
            webRequest.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            var jsonData = JsonUtility.ToJson(request);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Sticker message sent successfully: {messageId}");
                if (PendingCallbacks.ContainsKey(messageId))
                {
                    PendingCallbacks[messageId]?.Invoke(true);
                    PendingCallbacks.Remove(messageId);
                }
            }
            else
            {
                Debug.LogError($"Failed to send sticker message: {webRequest.error}");
                if (PendingCallbacks.ContainsKey(messageId))
                {
                    PendingCallbacks[messageId]?.Invoke(false);
                    PendingCallbacks.Remove(messageId);
                }
            }
        }

        public void SendTypingIndicator(string chatID, bool isTyping)
        {
            if (!IsConnected)
            {
                Debug.LogWarning("Cannot send typing indicator: not connected to SignalR");
                return;
            }

            CoroutineRunner.StartCoroutine(SendTypingIndicatorRequest(chatID, isTyping));
        }

        private IEnumerator SendTypingIndicatorRequest(string chatID, bool isTyping)
        {
            var webRequest = new UnityWebRequest($"{ServerUrl}/chat/typing", "POST");
            webRequest.SetRequestHeader("Authorization", $"Bearer {AccessToken}");
            webRequest.SetRequestHeader("Content-Type", "application/json");
            
            var requestData = new { ChatID = chatID, IsTyping = isTyping };
            var jsonData = JsonUtility.ToJson(requestData);
            var bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Typing indicator sent: {isTyping}");
            }
            else
            {
                Debug.LogError($"Failed to send typing indicator: {webRequest.error}");
            }
        }

        private IEnumerator ListenForMessages()
        {
            while (IsConnected)
            {
                // In real implementation, this would be a WebSocket or long polling connection
                // For demo purposes, we'll simulate receiving messages
                
                yield return new WaitForSeconds(5f); // Check every 5 seconds
                
                // Simulate receiving a message occasionally
                if (UnityEngine.Random.Range(0f, 1f) < 0.1f) // 10% chance
                {
                    SimulateReceivedMessage();
                }
            }
        }

        private void SimulateReceivedMessage()
        {
            // Simulate receiving a text message
            var simulatedMessage = new SignalRChatMessage
            {
                MessageID = Guid.NewGuid().ToString(),
                ChatID = "global",
                Sender = new ChatMember
                {
                    ProfileID = "simulated_user",
                    DisplayName = "Simulated User",
                    Avatar = new AvatarInfo()
                },
                Content = "This is a simulated message from SignalR",
                ContentType = MessageContent.MESSAGE,
                Timestamp = DateTime.UtcNow
            };

            OnReceiveTextMessage?.Invoke(simulatedMessage);
        }

        protected override void OnLogout()
        {
            Disconnect();
            PendingCallbacks.Clear();
        }
    }
} 
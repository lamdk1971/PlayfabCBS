using CBS.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CBS.Example
{
    /// <summary>
    /// Example of how to use SignalR chat in Unity
    /// </summary>
    public class SignalRChatExample : MonoBehaviour
    {
        [Header("UI References")]
        public InputField messageInput;
        public Button sendButton;
        public Button sendStickerButton;
        public ScrollRect chatScrollRect;
        public Transform messageContainer;
        public GameObject messagePrefab;
        public Text typingIndicator;

        [Header("SignalR Settings")]
        public string signalRServerUrl = "https://your-signalr-server.com";
        public string accessToken = "your-access-token";

        private IChat chatInstance;
        private CBSSignalRModule signalRModule;
        private bool isTyping = false;
        private float typingTimer = 0f;
        private const float TYPING_TIMEOUT = 3f;

        void Start()
        {
            InitializeSignalR();
            SetupUI();
        }

        void Update()
        {
            // Handle typing timeout
            if (isTyping)
            {
                typingTimer += Time.deltaTime;
                if (typingTimer >= TYPING_TIMEOUT)
                {
                    StopTyping();
                }
            }
        }

        private void InitializeSignalR()
        {
            // Get SignalR module
            signalRModule = CBSModule.Get<CBSSignalRModule>();

            // Connect to SignalR server
            signalRModule.Connect(signalRServerUrl, accessToken);

            // Create chat instance
            var chatRequest = new ChatInstanceRequest
            {
                ChatID = "global",
                Access = ChatAccess.GROUP,
                LoadMessagesCount = 50,
                PrivateChatMembers = null
            };

            // Use SignalR chat instance instead of regular chat instance
            chatInstance = new SignalRChatInstance(chatRequest);
            
            // Subscribe to events
            chatInstance.OnNewMessage += OnNewMessageReceived;
            chatInstance.OnGetHistory += OnChatHistoryLoaded;

            // Load chat
            chatInstance.Load();
        }

        private void SetupUI()
        {
            // Setup send button
            sendButton.onClick.AddListener(SendMessage);
            sendStickerButton.onClick.AddListener(SendSticker);

            // Setup input field for typing indicator
            messageInput.onValueChanged.AddListener(OnMessageInputChanged);
            messageInput.onEndEdit.AddListener(OnMessageInputEndEdit);
        }

        private void OnMessageInputChanged(string text)
        {
            if (!isTyping)
            {
                StartTyping();
            }
            typingTimer = 0f;
        }

        private void OnMessageInputEndEdit(string text)
        {
            StopTyping();
        }

        private void StartTyping()
        {
            isTyping = true;
            typingTimer = 0f;
            
            // Send typing indicator via SignalR
            if (chatInstance is SignalRChatInstance signalRChat)
            {
                signalRChat.SendTypingIndicator(true);
            }
        }

        private void StopTyping()
        {
            isTyping = false;
            typingTimer = 0f;
            
            // Send typing indicator via SignalR
            if (chatInstance is SignalRChatInstance signalRChat)
            {
                signalRChat.SendTypingIndicator(false);
            }
        }

        private void SendMessage()
        {
            if (string.IsNullOrEmpty(messageInput.text))
                return;

            var request = new CBSSendTextMessageRequest
            {
                MessageBody = messageInput.text,
                Target = ChatTarget.ALL,
                Visibility = ChatAccess.GROUP,
                CustomData = null
            };

            chatInstance.SendMessage(request, (result) =>
            {
                if (result.IsSuccess)
                {
                    Debug.Log("Message sent successfully via SignalR");
                    messageInput.text = "";
                }
                else
                {
                    Debug.LogError($"Failed to send message: {result.Error?.Message}");
                }
            });
        }

        private void SendSticker()
        {
            var request = new CBSSendStickerMessageRequest
            {
                StickerID = "happy_sticker_001",
                Target = ChatTarget.ALL,
                Visibility = ChatAccess.GROUP,
                CustomData = null
            };

            chatInstance.SendSticker(request, (result) =>
            {
                if (result.IsSuccess)
                {
                    Debug.Log("Sticker sent successfully via SignalR");
                }
                else
                {
                    Debug.LogError($"Failed to send sticker: {result.Error?.Message}");
                }
            });
        }

        private void OnNewMessageReceived(ChatMessage message)
        {
            Debug.Log($"New message received: {message.ContentRawData} from {message.Sender.DisplayName}");
            
            // Create UI message
            CreateMessageUI(message);
            
            // Scroll to bottom
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnChatHistoryLoaded(List<ChatMessage> messages)
        {
            Debug.Log($"Chat history loaded: {messages.Count} messages");
            
            // Clear existing messages
            foreach (Transform child in messageContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create UI for each message
            foreach (var message in messages)
            {
                CreateMessageUI(message);
            }
        }

        private void CreateMessageUI(ChatMessage message)
        {
            if (messagePrefab == null) return;

            var messageGO = Instantiate(messagePrefab, messageContainer);
            var messageUI = messageGO.GetComponent<ChatMessageUI>();
            
            if (messageUI != null)
            {
                messageUI.SetMessage(message);
            }
        }

        void OnDestroy()
        {
            // Cleanup
            if (chatInstance != null)
            {
                chatInstance.OnNewMessage -= OnNewMessageReceived;
                chatInstance.OnGetHistory -= OnChatHistoryLoaded;
                chatInstance.Dispose();
            }
        }
    }

    /// <summary>
    /// UI component for displaying a chat message
    /// </summary>
    public class ChatMessageUI : MonoBehaviour
    {
        public Text senderNameText;
        public Text messageText;
        public Text timestampText;
        public Image avatarImage;
        public Image messageBubble;

        public void SetMessage(ChatMessage message)
        {
            if (senderNameText != null)
                senderNameText.text = message.Sender.DisplayName;
            
            if (messageText != null)
                messageText.text = message.GetMessageBody();
            
            if (timestampText != null)
                timestampText.text = message.CreationDateUTC.ToLocalTime().ToString("HH:mm");
            
            // Set bubble color based on sender
            if (messageBubble != null)
            {
                var isOwnMessage = message.Sender.ProfileID == CBSModule.Get<CBSProfileModule>().ProfileID;
                messageBubble.color = isOwnMessage ? Color.blue : Color.white;
            }
        }
    }
} 
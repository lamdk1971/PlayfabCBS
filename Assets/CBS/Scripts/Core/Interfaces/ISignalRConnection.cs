using CBS.Models;
using CBS.SignalR.Models;
using System;

namespace CBS
{
    public interface ISignalRConnection
    {
        // Events
        event Action<SignalRChatMessage> OnReceiveTextMessage;
        event Action<SignalRChatMessage> OnReceiveStickerMessage;
        event Action<SignalRUserTyping> OnUserTyping;
        event Action<SignalRUserChatAction> OnUserJoinedChat;
        event Action<SignalRUserChatAction> OnUserLeftChat;

        // Connection
        bool IsConnected { get; }
        void Connect(string serverUrl, string accessToken);
        void Disconnect();

        // Chat operations
        void JoinChat(string chatID);
        void LeaveChat(string chatID);
        void SendTextMessage(SignalRTextMessageRequest request, Action<bool> callback = null);
        void SendStickerMessage(SignalRStickerMessageRequest request, Action<bool> callback = null);
        void SendTypingIndicator(string chatID, bool isTyping);
    }
} 
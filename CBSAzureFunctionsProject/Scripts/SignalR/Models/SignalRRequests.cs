using CBS.Models;

namespace CBS.SignalR.Models
{
    /// <summary>
    /// Request for sending text message via SignalR
    /// </summary>
    public class SignalRTextMessageRequest
    {
        public string ChatID { get; set; }
        public string MessageBody { get; set; }
        public ChatTarget Target { get; set; }
        public ChatAccess Visibility { get; set; }
        public string CustomData { get; set; }
    }

    /// <summary>
    /// Request for sending sticker message via SignalR
    /// </summary>
    public class SignalRStickerMessageRequest
    {
        public string ChatID { get; set; }
        public string StickerID { get; set; }
        public ChatTarget Target { get; set; }
        public ChatAccess Visibility { get; set; }
        public string CustomData { get; set; }
    }

    /// <summary>
    /// Request for sending item message via SignalR
    /// </summary>
    public class SignalRItemMessageRequest
    {
        public string ChatID { get; set; }
        public string ItemInstanceID { get; set; }
        public ChatTarget Target { get; set; }
        public ChatAccess Visibility { get; set; }
        public string CustomData { get; set; }
    }

    /// <summary>
    /// SignalR chat message response
    /// </summary>
    public class SignalRChatMessage
    {
        public string MessageID { get; set; }
        public string ChatID { get; set; }
        public ChatMember Sender { get; set; }
        public string Content { get; set; }
        public MessageContent ContentType { get; set; }
        public DateTime Timestamp { get; set; }
        public string CustomData { get; set; }
    }

    /// <summary>
    /// User typing indicator
    /// </summary>
    public class SignalRUserTyping
    {
        public string ProfileID { get; set; }
        public string ChatID { get; set; }
        public bool IsTyping { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// User join/leave chat notification
    /// </summary>
    public class SignalRUserChatAction
    {
        public string ProfileID { get; set; }
        public string ChatID { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 
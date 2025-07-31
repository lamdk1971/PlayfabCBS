using CBS.Context;
using CBS.Models;
using CBS.SignalR.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

namespace CBS
{
    public class SignalRChatInstance : IChat
    {
        public event Action<ChatMessage> OnNewMessage;
        public event Action<List<ChatMessage>> OnGetHistory;

        public string ChatID { get; private set; }
        public string[] PrivateChatMembers { get; private set; }

        private int HistoryMaxCount { get; set; }
        private ICBSChat ChatModule { get; set; }
        private IProfile Profile { get; set; }
        private ChatAccess ChatType { get; set; }
        private ISignalRConnection SignalRConnection { get; set; }
        private List<ChatMessage> CacheMessages { get; set; }

        public SignalRChatInstance(ChatInstanceRequest request)
        {
            HistoryMaxCount = request.LoadMessagesCount;
            ChatModule = CBSModule.Get<CBSChatModule>();
            Profile = CBSModule.Get<CBSProfileModule>();
            SignalRConnection = CBSModule.Get<CBSSignalRModule>();
            ChatID = request.ChatID;
            ChatType = request.Access;
            PrivateChatMembers = request.PrivateChatMembers;
            CacheMessages = new List<ChatMessage>();
        }

        public void Load()
        {
            GetChatHistory();
            ConnectToSignalR();
        }

        private void ConnectToSignalR()
        {
            if (SignalRConnection != null)
            {
                // Subscribe to SignalR events
                SignalRConnection.OnReceiveTextMessage += HandleSignalRTextMessage;
                SignalRConnection.OnReceiveStickerMessage += HandleSignalRStickerMessage;
                SignalRConnection.OnUserTyping += HandleUserTyping;
                SignalRConnection.OnUserJoinedChat += HandleUserJoinedChat;
                SignalRConnection.OnUserLeftChat += HandleUserLeftChat;

                // Join chat room
                SignalRConnection.JoinChat(ChatID);
            }
        }

        private void HandleSignalRTextMessage(SignalRChatMessage signalRMessage)
        {
            if (signalRMessage.ChatID == ChatID)
            {
                // Convert SignalR message to ChatMessage
                var chatMessage = new ChatMessage
                {
                    MessageID = signalRMessage.MessageID,
                    ChatID = signalRMessage.ChatID,
                    ContentType = signalRMessage.ContentType,
                    State = MessageState.SENT,
                    Sender = signalRMessage.Sender,
                    CreationDateUTC = signalRMessage.Timestamp,
                    ContentRawData = signalRMessage.Content,
                    CustomData = signalRMessage.CustomData
                };

                // Add to cache
                CacheMessages.Add(chatMessage);

                // Trigger event
                OnNewMessage?.Invoke(chatMessage);
            }
        }

        private void HandleSignalRStickerMessage(SignalRChatMessage signalRMessage)
        {
            if (signalRMessage.ChatID == ChatID)
            {
                // Convert SignalR message to ChatMessage
                var chatMessage = new ChatMessage
                {
                    MessageID = signalRMessage.MessageID,
                    ChatID = signalRMessage.ChatID,
                    ContentType = signalRMessage.ContentType,
                    State = MessageState.SENT,
                    Sender = signalRMessage.Sender,
                    CreationDateUTC = signalRMessage.Timestamp,
                    ContentRawData = signalRMessage.Content,
                    CustomData = signalRMessage.CustomData
                };

                // Add to cache
                CacheMessages.Add(chatMessage);

                // Trigger event
                OnNewMessage?.Invoke(chatMessage);
            }
        }

        private void HandleUserTyping(SignalRUserTyping typingInfo)
        {
            if (typingInfo.ChatID == ChatID)
            {
                // Handle typing indicator
                Debug.Log($"User {typingInfo.ProfileID} is {(typingInfo.IsTyping ? "typing" : "not typing")} in chat {ChatID}");
            }
        }

        private void HandleUserJoinedChat(SignalRUserChatAction action)
        {
            if (action.ChatID == ChatID)
            {
                Debug.Log($"User {action.ProfileID} joined chat {ChatID}");
            }
        }

        private void HandleUserLeftChat(SignalRUserChatAction action)
        {
            if (action.ChatID == ChatID)
            {
                Debug.Log($"User {action.ProfileID} left chat {ChatID}");
            }
        }

        public void SetMaxMessagesCount(int maxMessagesToLoad)
        {
            HistoryMaxCount = maxMessagesToLoad;
        }

        public void SetUpdateIntervalMiliseconds(float updateInterval)
        {
            // Not needed for SignalR as it's real-time
        }

        public void SendMessage(CBSSendTextMessageRequest request, Action<CBSSendChatMessageResult> result = null)
        {
            if (SignalRConnection != null)
            {
                var signalRRequest = new SignalRTextMessageRequest
                {
                    ChatID = ChatID,
                    MessageBody = request.MessageBody,
                    Target = request.Target,
                    Visibility = request.Visibility,
                    CustomData = request.CustomData
                };

                SignalRConnection.SendTextMessage(signalRRequest, (success) =>
                {
                    if (success)
                    {
                        result?.Invoke(new CBSSendChatMessageResult
                        {
                            IsSuccess = true
                        });
                    }
                    else
                    {
                        result?.Invoke(new CBSSendChatMessageResult
                        {
                            IsSuccess = false,
                            Error = new CBSError { Message = "Failed to send message via SignalR" }
                        });
                    }
                });
            }
            else
            {
                // Fallback to existing system
                if (ChatType == ChatAccess.GROUP)
                {
                    ChatModule.SendTextMessageToGroupChat(ChatID, request, result);
                }
                else if (ChatType == ChatAccess.PRIVATE)
                {
                    ChatModule.SendTextMessageToPrivateChat(GetInterlocutorID(), request, result);
                }
            }
        }

        public void SendSticker(CBSSendStickerMessageRequest request, Action<CBSSendChatMessageResult> result = null)
        {
            if (SignalRConnection != null)
            {
                var signalRRequest = new SignalRStickerMessageRequest
                {
                    ChatID = ChatID,
                    StickerID = request.StickerID,
                    Target = request.Target,
                    Visibility = request.Visibility,
                    CustomData = request.CustomData
                };

                SignalRConnection.SendStickerMessage(signalRRequest, (success) =>
                {
                    if (success)
                    {
                        result?.Invoke(new CBSSendChatMessageResult
                        {
                            IsSuccess = true
                        });
                    }
                    else
                    {
                        result?.Invoke(new CBSSendChatMessageResult
                        {
                            IsSuccess = false,
                            Error = new CBSError { Message = "Failed to send sticker via SignalR" }
                        });
                    }
                });
            }
            else
            {
                // Fallback to existing system
                if (ChatType == ChatAccess.GROUP)
                {
                    ChatModule.SendStickerMessageToGroupChat(ChatID, request, result);
                }
                else if (ChatType == ChatAccess.PRIVATE)
                {
                    ChatModule.SendStickerMessageToPrivateChat(GetInterlocutorID(), request, result);
                }
            }
        }

        public void SendItem(CBSSendItemMessageRequest request, Action<CBSSendChatMessageResult> result = null)
        {
            // For now, fallback to existing system for item messages
            if (ChatType == ChatAccess.GROUP)
            {
                ChatModule.SendItemMessageToGroupChat(ChatID, request, result);
            }
            else if (ChatType == ChatAccess.PRIVATE)
            {
                ChatModule.SendItemMessageToPrivateChat(GetInterlocutorID(), request, result);
            }
        }

        public void GetChatHistory(Action<CBSGetMessagesFromChatResult> result = null)
        {
            // Use existing system to get chat history
            ChatModule.GetMessagesFromChat(ChatID, HistoryMaxCount, onGet =>
            {
                if (onGet.IsSuccess)
                {
                    CacheMessages = onGet.Messages;
                    OnGetHistory?.Invoke(CacheMessages);
                }
                result?.Invoke(onGet);
            });
        }

        private string GetInterlocutorID()
        {
            if (ChatType != ChatAccess.PRIVATE)
                return string.Empty;
            var profileID = Profile.ProfileID;
            var interlocutorID = PrivateChatMembers.Where(x => x != profileID).FirstOrDefault();
            return interlocutorID;
        }

        public void SendTypingIndicator(bool isTyping)
        {
            if (SignalRConnection != null)
            {
                SignalRConnection.SendTypingIndicator(ChatID, isTyping);
            }
        }

        public void Dispose()
        {
            if (SignalRConnection != null)
            {
                // Unsubscribe from events
                SignalRConnection.OnReceiveTextMessage -= HandleSignalRTextMessage;
                SignalRConnection.OnReceiveStickerMessage -= HandleSignalRStickerMessage;
                SignalRConnection.OnUserTyping -= HandleUserTyping;
                SignalRConnection.OnUserJoinedChat -= HandleUserJoinedChat;
                SignalRConnection.OnUserLeftChat -= HandleUserLeftChat;

                // Leave chat room
                SignalRConnection.LeaveChat(ChatID);
            }

            CacheMessages = null;
        }
    }
} 
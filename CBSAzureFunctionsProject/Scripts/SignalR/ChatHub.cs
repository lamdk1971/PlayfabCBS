using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CBS.Models;
using Newtonsoft.Json;

namespace CBS.SignalR
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatDataProvider _chatDataProvider;

        public ChatHub(ILogger<ChatHub> logger, IChatDataProvider chatDataProvider)
        {
            _logger = logger;
            _chatDataProvider = chatDataProvider;
        }

        public override async Task OnConnectedAsync()
        {
            var profileID = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(profileID))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{profileID}");
                _logger.LogInformation($"User {profileID} connected to chat hub");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var profileID = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(profileID))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{profileID}");
                _logger.LogInformation($"User {profileID} disconnected from chat hub");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a chat room
        /// </summary>
        public async Task JoinChat(string chatID)
        {
            var profileID = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(profileID))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatID}");
            _logger.LogInformation($"User {profileID} joined chat {chatID}");

            // Notify others in the chat
            await Clients.OthersInGroup($"chat_{chatID}").SendAsync("UserJoinedChat", new
            {
                ProfileID = profileID,
                ChatID = chatID,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        public async Task LeaveChat(string chatID)
        {
            var profileID = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(profileID))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatID}");
            _logger.LogInformation($"User {profileID} left chat {chatID}");

            // Notify others in the chat
            await Clients.OthersInGroup($"chat_{chatID}").SendAsync("UserLeftChat", new
            {
                ProfileID = profileID,
                ChatID = chatID,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Send a text message to a chat
        /// </summary>
        public async Task SendTextMessage(SignalRTextMessageRequest request)
        {
            var senderProfileID = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderProfileID))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Validate request
            if (string.IsNullOrEmpty(request.ChatID) || string.IsNullOrEmpty(request.MessageBody))
            {
                throw new ArgumentException("Invalid message request");
            }

            try
            {
                // Create chat message
                var chatMessage = new ChatMessage
                {
                    MessageID = Guid.NewGuid().ToString(),
                    ChatID = request.ChatID,
                    ContentType = MessageContent.MESSAGE,
                    Target = request.Target,
                    Visibility = request.Visibility,
                    State = MessageState.SENT,
                    CreationDateUTC = DateTime.UtcNow,
                    ContentRawData = request.MessageBody,
                    CustomData = request.CustomData
                };

                // Get sender information
                var senderMember = await GetChatMemberAsync(senderProfileID);
                chatMessage.Sender = senderMember;

                // Save to database (existing system)
                await SaveMessageToDatabase(chatMessage);

                // Broadcast to all users in the chat
                var signalRMessage = new SignalRChatMessage
                {
                    MessageID = chatMessage.MessageID,
                    ChatID = chatMessage.ChatID,
                    Sender = chatMessage.Sender,
                    Content = chatMessage.ContentRawData,
                    ContentType = chatMessage.ContentType,
                    Timestamp = chatMessage.CreationDateUTC,
                    CustomData = chatMessage.CustomData
                };

                await Clients.Group($"chat_{request.ChatID}").SendAsync("ReceiveTextMessage", signalRMessage);

                _logger.LogInformation($"Message sent to chat {request.ChatID} by {senderProfileID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to chat {request.ChatID}");
                throw;
            }
        }

        /// <summary>
        /// Send a sticker message to a chat
        /// </summary>
        public async Task SendStickerMessage(SignalRStickerMessageRequest request)
        {
            var senderProfileID = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(senderProfileID))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            // Validate request
            if (string.IsNullOrEmpty(request.ChatID) || string.IsNullOrEmpty(request.StickerID))
            {
                throw new ArgumentException("Invalid sticker request");
            }

            try
            {
                // Create sticker content
                var chatSticker = new ChatSticker { ID = request.StickerID };
                var stickerContent = JsonConvert.SerializeObject(chatSticker);

                // Create chat message
                var chatMessage = new ChatMessage
                {
                    MessageID = Guid.NewGuid().ToString(),
                    ChatID = request.ChatID,
                    ContentType = MessageContent.STICKER,
                    Target = request.Target,
                    Visibility = request.Visibility,
                    State = MessageState.SENT,
                    CreationDateUTC = DateTime.UtcNow,
                    ContentRawData = stickerContent,
                    CustomData = request.CustomData
                };

                // Get sender information
                var senderMember = await GetChatMemberAsync(senderProfileID);
                chatMessage.Sender = senderMember;

                // Save to database (existing system)
                await SaveMessageToDatabase(chatMessage);

                // Broadcast to all users in the chat
                var signalRMessage = new SignalRChatMessage
                {
                    MessageID = chatMessage.MessageID,
                    ChatID = chatMessage.ChatID,
                    Sender = chatMessage.Sender,
                    Content = stickerContent,
                    ContentType = chatMessage.ContentType,
                    Timestamp = chatMessage.CreationDateUTC,
                    CustomData = chatMessage.CustomData
                };

                await Clients.Group($"chat_{request.ChatID}").SendAsync("ReceiveStickerMessage", signalRMessage);

                _logger.LogInformation($"Sticker sent to chat {request.ChatID} by {senderProfileID}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending sticker to chat {request.ChatID}");
                throw;
            }
        }

        /// <summary>
        /// Send typing indicator
        /// </summary>
        public async Task SendTypingIndicator(string chatID, bool isTyping)
        {
            var profileID = Context.User?.Identity?.Name;
            if (string.IsNullOrEmpty(profileID))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            await Clients.OthersInGroup($"chat_{chatID}").SendAsync("UserTyping", new
            {
                ProfileID = profileID,
                ChatID = chatID,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Get chat member information
        /// </summary>
        private async Task<ChatMember> GetChatMemberAsync(string profileID)
        {
            // This would typically call your existing profile service
            // For now, returning a basic structure
            return new ChatMember
            {
                ProfileID = profileID,
                DisplayName = $"User_{profileID}", // This should come from your profile service
                Avatar = new AvatarInfo() // This should come from your profile service
            };
        }

        /// <summary>
        /// Save message to database using existing system
        /// </summary>
        private async Task SaveMessageToDatabase(ChatMessage message)
        {
            // This would call your existing ChatModule to save the message
            // For now, just logging
            _logger.LogInformation($"Saving message {message.MessageID} to database");
            
            // TODO: Implement actual database save using existing ChatModule
            // await ChatModule.SendProfileMessageToChatAsync(new FunctionSendChatMessageRequest
            // {
            //     SenderProfileID = message.Sender.ProfileID,
            //     ChatID = message.ChatID,
            //     ContentType = message.ContentType,
            //     MessageBody = message.ContentRawData,
            //     CustomData = message.CustomData,
            //     Visibility = message.Visibility
            // });
        }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CBS.SignalR.Models;
using CBS.Models;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CBS.SignalR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly ChatHub _chatHub;

        public ChatController(ILogger<ChatController> logger, ChatHub chatHub)
        {
            _logger = logger;
            _chatHub = chatHub;
        }

        /// <summary>
        /// Join a chat room
        /// </summary>
        [HttpPost("join")]
        public async Task<IActionResult> JoinChat([FromBody] JoinChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChatID))
                {
                    return BadRequest("ChatID is required");
                }

                // In real implementation, you would validate the user's access to this chat
                await _chatHub.JoinChat(request.ChatID);

                return Ok(new { Success = true, Message = $"Joined chat {request.ChatID}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining chat");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Leave a chat room
        /// </summary>
        [HttpPost("leave")]
        public async Task<IActionResult> LeaveChat([FromBody] LeaveChatRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChatID))
                {
                    return BadRequest("ChatID is required");
                }

                await _chatHub.LeaveChat(request.ChatID);

                return Ok(new { Success = true, Message = $"Left chat {request.ChatID}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving chat");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send a text message
        /// </summary>
        [HttpPost("sendText")]
        public async Task<IActionResult> SendTextMessage([FromBody] SignalRTextMessageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChatID) || string.IsNullOrEmpty(request.MessageBody))
                {
                    return BadRequest("ChatID and MessageBody are required");
                }

                await _chatHub.SendTextMessage(request);

                return Ok(new { Success = true, Message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending text message");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send a sticker message
        /// </summary>
        [HttpPost("sendSticker")]
        public async Task<IActionResult> SendStickerMessage([FromBody] SignalRStickerMessageRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChatID) || string.IsNullOrEmpty(request.StickerID))
                {
                    return BadRequest("ChatID and StickerID are required");
                }

                await _chatHub.SendStickerMessage(request);

                return Ok(new { Success = true, Message = "Sticker sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending sticker message");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Send typing indicator
        /// </summary>
        [HttpPost("typing")]
        public async Task<IActionResult> SendTypingIndicator([FromBody] TypingIndicatorRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ChatID))
                {
                    return BadRequest("ChatID is required");
                }

                await _chatHub.SendTypingIndicator(request.ChatID, request.IsTyping);

                return Ok(new { Success = true, Message = "Typing indicator sent" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending typing indicator");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get chat history (fallback to existing system)
        /// </summary>
        [HttpGet("history/{chatID}")]
        public async Task<IActionResult> GetChatHistory(string chatID, [FromQuery] int count = 50)
        {
            try
            {
                if (string.IsNullOrEmpty(chatID))
                {
                    return BadRequest("ChatID is required");
                }

                // This would call your existing ChatModule
                // For now, returning empty result
                var result = new
                {
                    Success = true,
                    Messages = new object[0],
                    Count = 0
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chat history");
                return StatusCode(500, new { Success = false, Message = "Internal server error" });
            }
        }
    }

    public class JoinChatRequest
    {
        public string ChatID { get; set; }
    }

    public class LeaveChatRequest
    {
        public string ChatID { get; set; }
    }

    public class TypingIndicatorRequest
    {
        public string ChatID { get; set; }
        public bool IsTyping { get; set; }
    }
} 
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace CBS.SignalR
{
    public interface IConnectionManager
    {
        Task AddConnectionAsync(string userId, string connectionId, string chatId);
        Task RemoveConnectionAsync(string connectionId);
        Task<List<string>> GetUserConnectionsAsync(string userId);
        Task<List<string>> GetChatUsersAsync(string chatId);
        Task<bool> IsUserInChatAsync(string userId, string chatId);
        Task<int> GetChatUserCountAsync(string chatId);
        Task<int> GetUserConnectionCountAsync(string userId);
    }

    public class ConnectionManager : IConnectionManager
    {
        private readonly ILogger<ConnectionManager> _logger;
        private readonly ConcurrentDictionary<string, UserConnectionInfo> _userConnections;
        private readonly ConcurrentDictionary<string, HashSet<string>> _chatUsers;

        public ConnectionManager(ILogger<ConnectionManager> logger)
        {
            _logger = logger;
            _userConnections = new ConcurrentDictionary<string, UserConnectionInfo>();
            _chatUsers = new ConcurrentDictionary<string, HashSet<string>>();
        }

        public async Task AddConnectionAsync(string userId, string connectionId, string chatId)
        {
            try
            {
                var userConnection = new UserConnectionInfo
                {
                    UserId = userId,
                    ConnectionId = connectionId,
                    ChatId = chatId,
                    ConnectedAt = DateTime.UtcNow
                };

                _userConnections.TryAdd(connectionId, userConnection);

                // Add user to chat
                _chatUsers.AddOrUpdate(chatId, 
                    new HashSet<string> { userId },
                    (key, existingSet) =>
                    {
                        existingSet.Add(userId);
                        return existingSet;
                    });

                _logger.LogInformation($"User {userId} connected to chat {chatId} with connection {connectionId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding connection for user {userId}");
                throw;
            }
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            try
            {
                if (_userConnections.TryRemove(connectionId, out var userConnection))
                {
                    // Remove user from chat if no other connections
                    var userConnections = await GetUserConnectionsAsync(userConnection.UserId);
                    if (userConnections.Count == 0)
                    {
                        if (_chatUsers.TryGetValue(userConnection.ChatId, out var chatUserSet))
                        {
                            chatUserSet.Remove(userConnection.UserId);
                        }
                    }

                    _logger.LogInformation($"User {userConnection.UserId} disconnected from chat {userConnection.ChatId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing connection {connectionId}");
                throw;
            }
        }

        public async Task<List<string>> GetUserConnectionsAsync(string userId)
        {
            var connections = new List<string>();
            
            foreach (var kvp in _userConnections)
            {
                if (kvp.Value.UserId == userId)
                {
                    connections.Add(kvp.Key);
                }
            }

            return connections;
        }

        public async Task<List<string>> GetChatUsersAsync(string chatId)
        {
            if (_chatUsers.TryGetValue(chatId, out var userSet))
            {
                return new List<string>(userSet);
            }

            return new List<string>();
        }

        public async Task<bool> IsUserInChatAsync(string userId, string chatId)
        {
            if (_chatUsers.TryGetValue(chatId, out var userSet))
            {
                return userSet.Contains(userId);
            }

            return false;
        }

        public async Task<int> GetChatUserCountAsync(string chatId)
        {
            if (_chatUsers.TryGetValue(chatId, out var userSet))
            {
                return userSet.Count;
            }

            return 0;
        }

        public async Task<int> GetUserConnectionCountAsync(string userId)
        {
            int count = 0;
            
            foreach (var kvp in _userConnections)
            {
                if (kvp.Value.UserId == userId)
                {
                    count++;
                }
            }

            return count;
        }

        public async Task<ConnectionStatistics> GetStatisticsAsync()
        {
            var stats = new ConnectionStatistics
            {
                TotalConnections = _userConnections.Count,
                TotalChats = _chatUsers.Count,
                TotalUsers = _chatUsers.Values.SelectMany(x => x).Distinct().Count(),
                ConnectedAt = DateTime.UtcNow
            };

            return stats;
        }
    }

    public class UserConnectionInfo
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
        public string ChatId { get; set; }
        public DateTime ConnectedAt { get; set; }
    }

    public class ConnectionStatistics
    {
        public int TotalConnections { get; set; }
        public int TotalChats { get; set; }
        public int TotalUsers { get; set; }
        public DateTime ConnectedAt { get; set; }
    }
} 
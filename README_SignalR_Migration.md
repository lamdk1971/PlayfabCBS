# SignalR Chat Migration Guide

## Tổng quan

Hướng dẫn này mô tả cách migrate hệ thống chat hiện tại sang SignalR để có real-time messaging trong khi vẫn giữ nguyên hệ thống lưu trữ lịch sử chat.

## Kiến trúc hệ thống

### Trước khi migrate (Hiện tại)
```
Unity Client → Azure Functions → Database
```

### Sau khi migrate (Với SignalR)
```
Unity Client → SignalR Hub → Real-time messaging
                ↓
            Azure Functions → Database (Lưu lịch sử)
```

## Các file đã tạo

### 1. Server-side (Azure Functions)

#### `CBSAzureFunctionsProject/Scripts/SignalR/ChatHub.cs`
- SignalR Hub chính cho chat
- Xử lý real-time messaging
- Tích hợp với hệ thống lưu trữ hiện tại

#### `CBSAzureFunctionsProject/Scripts/SignalR/Models/SignalRRequests.cs`
- Các model request/response cho SignalR
- Tương thích với cấu trúc ChatMessage hiện tại

#### `CBSAzureFunctionsProject/Scripts/SignalR/Controllers/ChatController.cs`
- REST API endpoints cho SignalR operations
- Fallback cho các operations không real-time

### 2. Client-side (Unity)

#### `Assets/CBS/Scripts/Core/Models/Chat/SignalRChatInstance.cs`
- Chat instance mới sử dụng SignalR
- Tương thích với interface IChat hiện tại
- Fallback về hệ thống cũ nếu SignalR không khả dụng

#### `Assets/CBS/Scripts/Core/Interfaces/ISignalRConnection.cs`
- Interface cho SignalR connection
- Định nghĩa các events và methods cần thiết

#### `Assets/CBS/Scripts/Core/CBSSignalRModule.cs`
- Module quản lý kết nối SignalR
- Xử lý authentication và connection management

#### `Assets/CBS/Scripts/Example/SignalRChatExample.cs`
- Ví dụ sử dụng SignalR chat
- UI implementation với typing indicators

## Thông tin User trong tin nhắn

### Cấu trúc ChatMessage hiện tại
```csharp
public class ChatMessage
{
    public ChatMember Sender;        // Thông tin người gửi
    public ChatMember TaggedProfile; // Thông tin người được tag
    // ... các field khác
}

public class ChatMember
{
    public string ProfileID;     // ID người dùng
    public string DisplayName;   // Tên hiển thị
    public AvatarInfo Avatar;    // Thông tin avatar
}
```

### SignalR Message
```csharp
public class SignalRChatMessage
{
    public ChatMember Sender;    // Đầy đủ thông tin user
    public string Content;       // Nội dung tin nhắn
    public DateTime Timestamp;   // Thời gian
    // ... các field khác
}
```

**✅ Kết luận: Bạn có thể lấy đầy đủ thông tin user từ tin nhắn SignalR!**

## Workflow hoạt động

### 1. Gửi tin nhắn
```
1. Client gửi tin nhắn qua SignalR
2. SignalR Hub nhận và broadcast
3. Đồng thời lưu vào database qua Azure Function
4. Client nhận confirmation
```

### 2. Nhận tin nhắn
```
1. SignalR Hub broadcast tin nhắn đến tất cả clients
2. Client nhận real-time với đầy đủ thông tin user
3. Hiển thị ngay lập tức
4. Lưu vào cache local
```

### 3. Load lịch sử
```
1. Client gọi API cũ để load lịch sử
2. Merge với tin nhắn real-time
3. Hiển thị đầy đủ
```

## Cách sử dụng

### 1. Khởi tạo SignalR Chat
```csharp
// Tạo chat instance với SignalR
var chatRequest = new ChatInstanceRequest
{
    ChatID = "global",
    Access = ChatAccess.GROUP,
    LoadMessagesCount = 50
};

var chatInstance = new SignalRChatInstance(chatRequest);
chatInstance.OnNewMessage += OnNewMessageReceived;
chatInstance.Load();
```

### 2. Gửi tin nhắn
```csharp
var request = new CBSSendTextMessageRequest
{
    MessageBody = "Hello SignalR!",
    Target = ChatTarget.ALL,
    Visibility = ChatAccess.GROUP
};

chatInstance.SendMessage(request, (result) =>
{
    if (result.IsSuccess)
        Debug.Log("Message sent via SignalR");
});
```

### 3. Nhận tin nhắn
```csharp
private void OnNewMessageReceived(ChatMessage message)
{
    // Có đầy đủ thông tin user
    var senderName = message.Sender.DisplayName;
    var senderID = message.Sender.ProfileID;
    var avatar = message.Sender.Avatar;
    
    Debug.Log($"Message from {senderName}: {message.GetMessageBody()}");
}
```

## Lợi ích

### ✅ Real-time Performance
- Tin nhắn được gửi và nhận ngay lập tức
- Không cần polling

### ✅ Giữ nguyên lịch sử
- Tất cả tin nhắn vẫn được lưu vào database
- Có thể load lịch sử như trước

### ✅ Đầy đủ thông tin user
- Mỗi tin nhắn chứa đầy đủ thông tin sender
- ProfileID, DisplayName, Avatar

### ✅ Backward Compatibility
- Có thể fallback về hệ thống cũ
- Không ảnh hưởng đến code hiện tại

### ✅ Gradual Migration
- Có thể migrate từng phần
- Test và deploy an toàn

## Deployment Steps

### Phase 1: Setup SignalR Infrastructure
1. Deploy SignalR Hub và Controllers
2. Setup authentication
3. Test basic connection

### Phase 2: Client Integration
1. Thêm SignalR modules vào Unity
2. Test real-time messaging
3. Verify user information

### Phase 3: Production Migration
1. Deploy với fallback
2. Monitor performance
3. Gradually switch users

### Phase 4: Optimization
1. Optimize connection pooling
2. Add error handling
3. Performance tuning

## Troubleshooting

### SignalR không kết nối
- Kiểm tra server URL và access token
- Verify network connectivity
- Check authentication

### Tin nhắn không hiển thị
- Kiểm tra event subscriptions
- Verify chat room membership
- Check message format

### Thông tin user bị thiếu
- Verify ChatMember population
- Check profile service integration
- Validate message structure

## Kết luận

Với implementation này, bạn có thể:
1. **Migrate chat sang SignalR** để có real-time messaging
2. **Giữ nguyên hệ thống lưu trữ** hiện tại
3. **Lấy đầy đủ thông tin user** từ tin nhắn nhận được
4. **Có fallback an toàn** về hệ thống cũ
5. **Gradual migration** mà không ảnh hưởng đến production

SignalR sẽ cung cấp performance tốt hơn cho real-time chat trong khi vẫn đảm bảo tính ổn định và tương thích với hệ thống hiện tại. 
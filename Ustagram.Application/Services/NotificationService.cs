using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;
using Ustagram.Infrastructure.Persistance;

namespace Ustagram.Application.Services;

public class NotificationService : INotificationService
{

    private readonly ApplicationDbContext _db;
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(ApplicationDbContext db, IUserService userService, IPostService postService, IHubContext<NotificationHub> hubContext)
    {
        _db = db;
        _userService = userService;
        _postService = postService;
        _hubContext = hubContext;
    }

    public async Task<string> CreateNotification(NotificationDTO model)
    {
        var newNotification = new Notification
        {
            Title = model.Title,
            Text = model.Text,
            Date = Convert.ToString(DateTime.UtcNow, CultureInfo.InvariantCulture),
            Type = model.Type,
            Reference_PostId = model.Reference_PostId,
            Reference_UserId = model.Reference_UserId,
            ReceiverId = model.Receiver
        };

        await _db.Notifications.AddAsync(newNotification);
        await _db.SaveChangesAsync();

        return "Data Created!";
    }

    public async Task<Notification> GetPostNotification(Guid id)
    {
        var data = await _db.Notifications.FindAsync(id);

        if (data == null)
        {
            return null;
        }

        return data;
    }

    public async Task<string> UpdateNotification(Guid id, NotificationDTO model)
    {
        var data = await _db.Notifications.FindAsync(id);

        if (data == null)
        {
            return null;
        }

        data.Date = Convert.ToString(DateTime.UtcNow, CultureInfo.InvariantCulture);
        data.Title = model.Title;
        data.Text = model.Text;
        data.Type = model.Type;
        data.ReceiverId = model.Receiver;
        data.Reference_PostId = model.Reference_PostId;
        data.Reference_UserId = model.Reference_UserId;

        await _db.SaveChangesAsync();
        return "Data Updated!";
    }

    public async Task<string> DeleteNotification(Guid id)
    {
        var data = await _db.Notifications.FindAsync(id);

        if (data == null)
        {
            return null;
        }

        _db.Notifications.Remove(data);
        await _db.SaveChangesAsync();

        return "Data Deleted!";
    }

    public async Task<List<Notification>> GetAllPNotifications()
    {
        var all = await _db.Notifications.ToListAsync();

        if (all == null)
        {
            return null;
        }

        return all;
    }

    public async Task<string> MarkAsRead(Guid id)
    {
        var notification = await _db.Notifications.FindAsync(id);

        if (notification == null)
        {
            return null;
        }
        
        notification.IsRead = true;
        await _db.SaveChangesAsync();

        return "Data Marked As Read";
    }
    
    
    public async Task NotifyCommentAsync(Guid postId, Guid commenterId, string commentText)
    {
        var post = await _postService.GetPostById(postId);
        if (post == null) return;

        var commenter = await _userService.GetUserById(commenterId);
        if (commenter == null) return;

        var notification = new Notification
        {
            Title = "New Comment",
            Text = $"{commenter.FullName} commented: {commentText}",
            Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Type = "comment",
            Reference_PostId = postId,
            Reference_UserId = commenterId,
            ReceiverId = post.UserId,
            IsRead = false
        };

        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
        
        await _hubContext.Clients.User(post.UserId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyLikeAsync(Guid postId, Guid likerId)
    {
        var post = await _postService.GetPostById(postId);
        if (post == null) return;

        var liker = await _userService.GetUserById(likerId);
        if (liker == null) return;

        var notification = new Notification
        {
            Title = "New Like",
            Text = $"{liker.FullName} liked your post",
            Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Type = "like",
            Reference_PostId = postId,
            Reference_UserId = likerId,
            ReceiverId = post.UserId,
            IsRead = false
        };

        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
        
        await _hubContext.Clients.User(post.UserId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task NotifyNewFollowerAsync(Guid userId, Guid followerId)
    {
        var follower = await _userService.GetUserById(followerId);
        if (follower == null) return;

        var notification = new Notification
        {
            Title = "New Follower",
            Text = $"{follower.FullName} started following you",
            Date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
            Type = "follow",
            Reference_UserId = followerId,
            ReceiverId = userId,
            IsRead = false
        };

        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
        
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task<List<Notification>> GetUserNotifications(Guid userId)
    {
        return await _db.Notifications
            .Where(n => n.ReceiverId == userId)
            .OrderByDescending(n => n.Date)
            .ToListAsync();
    }

    public async Task<int> GetUnreadNotificationCount(Guid userId)
    {
        return await _db.Notifications
            .CountAsync(n => n.ReceiverId == userId && !n.IsRead);
    }

    public async Task<string> MarkAllAsRead(Guid userId)
    {
        var unreadNotifications = await _db.Notifications
            .Where(n => n.ReceiverId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Count == 0)
        {
            return "No unread notifications found";
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _db.SaveChangesAsync();

        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("NotificationsRead");

        return $"Marked {unreadNotifications.Count} notifications as read";
    }
}
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.Application.Abstractions;

public interface INotificationService
{
    Task<string> CreateNotification(NotificationDTO NotificationDto);
    Task<Notification> GetPostNotification(Guid NotificationId);
    Task<string> UpdateNotification(Guid NotificationId, NotificationDTO NotificationDto);
    Task<string> DeleteNotification(Guid NotificationId);
    Task<List<Notification>> GetAllPNotifications();
    Task<string> MarkAsRead(Guid id);

    Task NotifyCommentAsync(Guid postId, Guid commenterId, string commentText);
    Task NotifyLikeAsync(Guid postId, Guid likerId);
    Task NotifyNewFollowerAsync(Guid userId, Guid followerId);
    
    Task<List<Notification>> GetUserNotifications(Guid userId);
    Task<int> GetUnreadNotificationCount(Guid userId);
    
    Task<string> MarkAllAsRead(Guid userId);
}
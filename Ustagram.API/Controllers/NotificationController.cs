using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ustagram.Application.Abstractions;
using Ustagram.Domain.DTOs;
using Ustagram.Domain.Model;

namespace Ustagram.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
// [Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService service, INotificationService notificationService)
    {
        _service = service;
        _notificationService = notificationService;
    }


    [HttpPost]
    public async Task<ActionResult<string>> CreateNotification([FromForm] NotificationDTO model)
    {
        var result = await _service.CreateNotification(model);

        return result;
    }

    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetAllNotifications()
    {
        return await _service.GetAllPNotifications();
    }
    
    
    [HttpGet]
    public async Task<ActionResult<List<Notification>>> GetMyNotifications()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return await _service.GetUserNotifications(userId);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetMyUnreadNotificationCount()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        return await _service.GetUnreadNotificationCount(userId);
    }

    [HttpPost("mark-as-read/{id}")]
    public async Task<ActionResult<string>> MarkNotificationAsRead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var notification = await _service.GetPostNotification(id);
        
        if (notification == null || notification.ReceiverId != userId)
        {
            return Unauthorized();
        }

        return await _service.MarkAsRead(id);
    }

    [HttpPost("mark-all-as-read")]
    public async Task<ActionResult<string>> MarkAllAsRead()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var notifications = await _service.GetUserNotifications(userId);
        
        foreach (var notification in notifications.Where(n => !n.IsRead))
        {
            await _service.MarkAsRead(notification.Id);
        }

        return "All notifications marked as read";
    }
}
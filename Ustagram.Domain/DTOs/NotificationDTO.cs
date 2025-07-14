namespace Ustagram.Domain.DTOs;

public class NotificationDTO
{
    public string Title { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
    public string Date { get; set; }
    public Guid Receiver { get; set; }
    public Guid? Reference_PostId { get; set; }
    public Guid? Reference_UserId { get; set; }
}
namespace Ustagram.Domain.Model;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Text { get; set; }
    public string Type { get; set; }
    public string Date { get; set; }
    public bool IsRead { get; set; } = false;
    public Guid ReceiverId { get; set; }
    public User Receiver { get; set; }
    public Guid? Reference_PostId { get; set; }
    public Guid? Reference_UserId { get; set; }
}
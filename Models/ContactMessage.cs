namespace e_store.Models;

public class ContactMessage
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool isRead { get; set; } = false;
    
}
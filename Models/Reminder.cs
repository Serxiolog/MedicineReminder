namespace MedicineReminder.Models;

public class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicineId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedTime { get; set; }
}

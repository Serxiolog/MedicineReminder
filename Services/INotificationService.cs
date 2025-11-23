using MedicineReminder.Models;

namespace MedicineReminder.Services;

public interface INotificationService
{
    Task ScheduleNotificationAsync(Reminder reminder, Medicine medicine);
    Task CancelNotificationAsync(int notificationId);
    Task RequestPermissionsAsync();
}

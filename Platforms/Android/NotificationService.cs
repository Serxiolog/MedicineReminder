using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using MedicineReminder.Models;

namespace MedicineReminder.Platforms.Android;

public class NotificationService : Services.INotificationService
{
    private const string ChannelId = "medicine_reminders";
    private const string ChannelName = "Напоминания о лекарствах";

    public NotificationService()
    {
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.High)
            {
                Description = "Уведомления о приёме лекарств"
            };

            var notificationManager = (NotificationManager?)global::Android.App.Application.Context.GetSystemService(Context.NotificationService);
            notificationManager?.CreateNotificationChannel(channel);
        }
    }

    public async Task RequestPermissionsAsync()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
                if (status != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.PostNotifications>();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка запроса разрешений: {ex.Message}");
        }
    }

    public async Task ScheduleNotificationAsync(Reminder reminder, Medicine medicine)
    {
        try
        {
            var intent = new Intent(global::Android.App.Application.Context, typeof(AlarmReceiver));
            intent.PutExtra("ReminderId", reminder.Id.ToString());
            intent.PutExtra("MedicineName", medicine.Name);
            intent.PutExtra("Dosage", medicine.Dosage);

            var notificationId = Math.Abs(reminder.Id.GetHashCode());
            var pendingIntent = PendingIntent.GetBroadcast(
                global::Android.App.Application.Context,
                notificationId,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            );

            var alarmManager = (AlarmManager?)global::Android.App.Application.Context.GetSystemService(Context.AlarmService);
            if (alarmManager != null)
            {
                var triggerTime = new DateTimeOffset(reminder.ScheduledTime).ToUnixTimeMilliseconds();
                
                if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent);
                }
                else
                {
                    alarmManager.SetExact(AlarmType.RtcWakeup, triggerTime, pendingIntent);
                }
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка планирования уведомления: {ex.Message}");
        }
    }

    public async Task CancelNotificationAsync(int notificationId)
    {
        try
        {
            var intent = new Intent(global::Android.App.Application.Context, typeof(AlarmReceiver));
            var pendingIntent = PendingIntent.GetBroadcast(
                global::Android.App.Application.Context,
                notificationId,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            );

            var alarmManager = (AlarmManager?)global::Android.App.Application.Context.GetSystemService(Context.AlarmService);
            alarmManager?.Cancel(pendingIntent);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отмены уведомления: {ex.Message}");
        }
    }
}

[BroadcastReceiver(Enabled = true)]
public class AlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null || intent == null) return;

        var medicineName = intent.GetStringExtra("MedicineName") ?? "Лекарство";
        var dosage = intent.GetStringExtra("Dosage") ?? "";

        var notificationIntent = new Intent(context, typeof(MainActivity));
        notificationIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);

        var pendingIntent = PendingIntent.GetActivity(
            context,
            0,
            notificationIntent,
            PendingIntentFlags.Immutable
        );

        var builder = new NotificationCompat.Builder(context, "medicine_reminders")
            .SetSmallIcon(Resource.Drawable.ic_call_answer)
            .SetContentTitle("Время принять лекарство")
            .SetContentText($"{medicineName} {dosage}")
            .SetPriority(NotificationCompat.PriorityHigh)
            .SetAutoCancel(true)
            .SetContentIntent(pendingIntent);

        var notificationManager = NotificationManagerCompat.From(context);
        var notificationId = new Random().Next();
        
        try
        {
            notificationManager.Notify(notificationId, builder.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отображения уведомления: {ex.Message}");
        }
    }
}

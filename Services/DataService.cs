using System.Text.Json;
using MedicineReminder.Models;

namespace MedicineReminder.Services;

public class DataService
{
    private readonly string _medicinesFilePath;
    private readonly string _remindersFilePath;
    private List<Medicine> _medicines = new();
    private List<Reminder> _reminders = new();

    public DataService()
    {
        var appDataPath = FileSystem.AppDataDirectory;
        _medicinesFilePath = Path.Combine(appDataPath, "medicines.json");
        _remindersFilePath = Path.Combine(appDataPath, "reminders.json");
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            if (File.Exists(_medicinesFilePath))
            {
                var json = File.ReadAllText(_medicinesFilePath);
                _medicines = JsonSerializer.Deserialize<List<Medicine>>(json) ?? new();
            }

            if (File.Exists(_remindersFilePath))
            {
                var json = File.ReadAllText(_remindersFilePath);
                _reminders = JsonSerializer.Deserialize<List<Reminder>>(json) ?? new();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки данных: {ex.Message}");
        }
    }

    private async Task SaveMedicinesAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_medicines, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_medicinesFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения лекарств: {ex.Message}");
        }
    }

    private async Task SaveRemindersAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_reminders, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_remindersFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения напоминаний: {ex.Message}");
        }
    }

    // CRUD для лекарств
    public async Task<List<Medicine>> GetAllMedicinesAsync()
    {
        return await Task.FromResult(_medicines.ToList());
    }

    public async Task<Medicine?> GetMedicineByIdAsync(Guid id)
    {
        return await Task.FromResult(_medicines.FirstOrDefault(m => m.Id == id));
    }

    public async Task AddMedicineAsync(Medicine medicine)
    {
        _medicines.Add(medicine);
        await SaveMedicinesAsync();
    }

    public async Task UpdateMedicineAsync(Medicine medicine)
    {
        var index = _medicines.FindIndex(m => m.Id == medicine.Id);
        if (index >= 0)
        {
            _medicines[index] = medicine;
            await SaveMedicinesAsync();
        }
    }

    public async Task DeleteMedicineAsync(Guid id)
    {
        var medicine = _medicines.FirstOrDefault(m => m.Id == id);
        if (medicine != null)
        {
            _medicines.Remove(medicine);
            await SaveMedicinesAsync();
            
            // Удаляем связанные напоминания
            _reminders.RemoveAll(r => r.MedicineId == id);
            await SaveRemindersAsync();
        }
    }

    // CRUD для напоминаний
    public async Task<List<Reminder>> GetAllRemindersAsync()
    {
        return await Task.FromResult(_reminders.ToList());
    }

    public async Task<List<Reminder>> GetUpcomingRemindersAsync()
    {
        var now = DateTime.Now;
        return await Task.FromResult(_reminders
            .Where(r => !r.IsCompleted && r.ScheduledTime >= now)
            .OrderBy(r => r.ScheduledTime)
            .ToList());
    }

    public async Task<List<Reminder>> GetRemindersByMedicineIdAsync(Guid medicineId)
    {
        return await Task.FromResult(_reminders
            .Where(r => r.MedicineId == medicineId)
            .OrderBy(r => r.ScheduledTime)
            .ToList());
    }

    public async Task AddReminderAsync(Reminder reminder)
    {
        _reminders.Add(reminder);
        await SaveRemindersAsync();
    }

    public async Task MarkReminderAsCompletedAsync(Guid reminderId)
    {
        var reminder = _reminders.FirstOrDefault(r => r.Id == reminderId);
        if (reminder != null)
        {
            reminder.IsCompleted = true;
            reminder.CompletedTime = DateTime.Now;
            await SaveRemindersAsync();
        }
    }

    public async Task GenerateRemindersForMedicineAsync(Medicine medicine)
    {
        // Удаляем старые напоминания для этого лекарства
        _reminders.RemoveAll(r => r.MedicineId == medicine.Id && r.ScheduledTime > DateTime.Now);

        if (!medicine.IsActive)
        {
            await SaveRemindersAsync();
            return;
        }

        var endDate = medicine.EndDate ?? DateTime.Now.AddMonths(3);
        var currentDate = medicine.StartDate;

        while (currentDate <= endDate)
        {
            if (medicine.SelectedDays.Count == 0 || medicine.SelectedDays.Contains(currentDate.DayOfWeek))
            {
                var scheduledTime = currentDate.Date.AddHours(medicine.Time.Hour).AddMinutes(medicine.Time.Minute);
                if (scheduledTime > DateTime.Now)
                {
                    var reminder = new Reminder
                    {
                        MedicineId = medicine.Id,
                        ScheduledTime = scheduledTime,
                        IsCompleted = false
                    };
                    _reminders.Add(reminder);
                }
            }
            currentDate = currentDate.AddDays(1);
        }

        await SaveRemindersAsync();
    }
}

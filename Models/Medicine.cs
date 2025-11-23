namespace MedicineReminder.Models;

public class Medicine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime Time { get; set; } = DateTime.Today.AddHours(9);
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime? EndDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<DayOfWeek> SelectedDays { get; set; } = new();
}

namespace IeltsTeachingAssistant.Services;

public interface INotificationService
{
    Task ScheduleClassReminderAsync(int classId, DateTime classTime, TimeSpan before);
    Task CancelReminderAsync(int classId);
    Task<List<string>> GetUpcomingRemindersAsync();
}

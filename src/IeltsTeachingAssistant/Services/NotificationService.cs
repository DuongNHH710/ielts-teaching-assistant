using Microsoft.Toolkit.Uwp.Notifications;

namespace IeltsTeachingAssistant.Services;

public class NotificationService : INotificationService
{
    public async Task ScheduleClassReminderAsync(int classId, DateTime classTime, TimeSpan before)
    {
        var alarmTime = classTime.Subtract(before);
        if (alarmTime <= DateTime.Now) return;

        new ToastContentBuilder()
            .AddArgument("action", "classReminder")
            .AddArgument("classId", classId.ToString())
            .AddText("Upcoming Class")
            .AddText($"You have a class starting in {before.TotalMinutes} minutes.")
            .Schedule(new DateTimeOffset(alarmTime));

        await Task.CompletedTask;
    }

    public async Task CancelReminderAsync(int classId)
    {
        // To cancel, we need to store and retrieve the notification ID.
        // Omitted for brevity in this scaffold.
        await Task.CompletedTask;
    }

    public async Task<List<string>> GetUpcomingRemindersAsync()
    {
        return await Task.FromResult(new List<string>());
    }
}

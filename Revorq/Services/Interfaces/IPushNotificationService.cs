namespace Revorq.API.Services.Interfaces;

public interface IPushNotificationService
{
    Task SendToUsersAsync(IEnumerable<int> userIds, string title, string body, IReadOnlyDictionary<string, string> data);
}

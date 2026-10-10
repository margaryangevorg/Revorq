using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Revorq.API.Services.Interfaces;
using Revorq.DAL.Repositories.Interfaces;

namespace Revorq.API.Services.Implementations;

public class FirebasePushNotificationService : IPushNotificationService
{
    private const string AndroidChannelId = "orders";
    private static readonly object InitLock = new();

    private readonly IConfiguration _config;
    private readonly IDeviceTokenRepository _deviceTokenRepository;
    private readonly ILogger<FirebasePushNotificationService> _logger;

    public FirebasePushNotificationService(
        IConfiguration config,
        IDeviceTokenRepository deviceTokenRepository,
        ILogger<FirebasePushNotificationService> logger)
    {
        _config = config;
        _deviceTokenRepository = deviceTokenRepository;
        _logger = logger;
    }

    private bool TryEnsureInitialized()
    {
        if (FirebaseApp.DefaultInstance is not null)
            return true;

        lock (InitLock)
        {
            if (FirebaseApp.DefaultInstance is not null)
                return true;

            var credentialPath = _config["Firebase:CredentialPath"];
            if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
            {
                _logger.LogWarning("Firebase credential file not found at '{Path}' — push notifications are disabled.", credentialPath);
                return false;
            }

            try
            {
                FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromFile(credentialPath) });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Firebase — push notifications are disabled.");
                return false;
            }
        }
    }

    public async Task SendToUsersAsync(IEnumerable<int> userIds, string title, string body, IReadOnlyDictionary<string, string> data)
    {
        try
        {
            await SendToUsersCoreAsync(userIds, title, body, data);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Push notification send failed — continuing without interrupting the caller.");
        }
    }

    private async Task SendToUsersCoreAsync(IEnumerable<int> userIds, string title, string body, IReadOnlyDictionary<string, string> data)
    {
        if (!TryEnsureInitialized())
            return;

        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return;

        var tokens = (await _deviceTokenRepository.GetByUserIdsAsync(ids))
            .Select(t => t.Token)
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
            return;

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification { Title = title, Body = body },
            Data = data,
            Android = new AndroidConfig { Notification = new AndroidNotification { ChannelId = AndroidChannelId } }
        };

        var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

        if (response.FailureCount == 0)
            return;

        var invalidTokens = new List<string>();
        for (var i = 0; i < response.Responses.Count; i++)
        {
            var result = response.Responses[i];
            if (result.IsSuccess)
                continue;

            if (result.Exception?.MessagingErrorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument)
                invalidTokens.Add(tokens[i]);
            else
                _logger.LogWarning(result.Exception, "Failed to send push notification to token {Token}", tokens[i]);
        }

        if (invalidTokens.Count == 0)
            return;

        foreach (var token in invalidTokens)
            await _deviceTokenRepository.DeleteByTokenAsync(token);

        await _deviceTokenRepository.SaveChangesAsync();
    }
}

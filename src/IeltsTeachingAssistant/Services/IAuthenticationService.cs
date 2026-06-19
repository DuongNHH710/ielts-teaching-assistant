namespace IeltsTeachingAssistant.Services;

public interface IAuthenticationService
{
    Task SetupPasswordAsync(string password);
    Task<bool> VerifyPasswordAsync(string password);
    bool IsPasswordSetup();

    Task<bool> SetupBiometricAsync();
    Task<bool> VerifyBiometricAsync();
    bool IsBiometricAvailable();
}

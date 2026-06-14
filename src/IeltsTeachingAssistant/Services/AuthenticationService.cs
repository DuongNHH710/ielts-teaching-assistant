using IeltsTeachingAssistant.Data;
using Microsoft.EntityFrameworkCore;
using Windows.Security.Credentials.UI;

namespace IeltsTeachingAssistant.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly AppDbContext _context;

    public AuthenticationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SetupPasswordAsync(string password)
    {
        var settings = await _context.Settings.FirstOrDefaultAsync() 
                       ?? new IeltsTeachingAssistant.Models.AppSettings();
        
        settings.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        if (settings.Id == 0)
        {
            _context.Settings.Add(settings);
        }
        else
        {
            _context.Settings.Update(settings);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<bool> VerifyPasswordAsync(string password)
    {
        var settings = await _context.Settings.FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(settings?.PasswordHash)) return false;

        return BCrypt.Net.BCrypt.Verify(password, settings.PasswordHash);
    }

    public bool IsPasswordSetup()
    {
        return !string.IsNullOrEmpty(_context.Settings.FirstOrDefault()?.PasswordHash);
    }

    public async Task<bool> SetupBiometricAsync()
    {
        if (!IsBiometricAvailable()) return false;

        var result = await UserConsentVerifier.RequestVerificationAsync("Verify your identity to enable biometric login for IELTS Assistant.");
        if (result == UserConsentVerificationResult.Verified)
        {
            var settings = await _context.Settings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.UseBiometric = true;
                await _context.SaveChangesAsync();
                return true;
            }
        }
        return false;
    }

    public async Task<bool> VerifyBiometricAsync()
    {
        if (!IsBiometricAvailable()) return false;

        var settings = await _context.Settings.FirstOrDefaultAsync();
        if (settings == null || !settings.UseBiometric) return false;

        var result = await UserConsentVerifier.RequestVerificationAsync("Verify your identity to login to IELTS Assistant.");
        return result == UserConsentVerificationResult.Verified;
    }

    public bool IsBiometricAvailable()
    {
        // Must be called from UI thread typically, synchronous check can block
        var status = UserConsentVerifier.CheckAvailabilityAsync().GetResults();
        return status == UserConsentVerifierAvailability.Available;
    }
}

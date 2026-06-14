using System.ComponentModel.DataAnnotations;

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents application settings. There is typically only one row in this table.
/// </summary>
public class AppSettings
{
    [Key]
    public int Id { get; set; }

    public string? GcpProjectId { get; set; }
    public string? GcpRegion { get; set; }
    public string? GcpCredentialsPath { get; set; }
    
    public string? AdminPassword { get; set; } // Hashed or plain text, depending on requirement. For simplicity, plain text is assumed for a local SQLite file but can be hashed.
    
    public string PreferredModel { get; set; } = "gemini-1.5-pro";
    
    public string Language { get; set; } = "en";
    public string Theme { get; set; } = "system";
    
    public string? PasswordHash { get; set; }
    public bool UseBiometric { get; set; }
}

namespace IeltsTeachingAssistant.Models;

/// <summary>
/// Represents the current status of a class.
/// </summary>
public enum ClassStatus
{
    Upcoming,
    Active,
    Completed
}

/// <summary>
/// Represents the delivery mode of a class.
/// </summary>
public enum ClassMode
{
    Online,
    Offline
}

/// <summary>
/// Represents the IELTS test type the class or student is preparing for.
/// </summary>
public enum TestType
{
    Academic,
    General,
    Both
}

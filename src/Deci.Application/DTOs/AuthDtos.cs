namespace Deci.Application.DTOs;

public record LoginRequest(string Email, string Password);

public record CompleteProfileRequest(string CurrentPassword, string NewEmail, string NewPassword, string ConfirmPassword, string Phone);

public record UserDto(
    int Id,
    string Email,
    string FullName,
    string? Phone,
    string? ProfileImageUrl,
    string Role,
    bool IsActive,
    bool ProfileCompleted,
    DateTime CreatedAt);

public record LoginResponse(string Token, UserDto User);

public record CreateUserRequest(string Email, string Password, string FullName, string? Phone, string Role);

public record UpdateUserRequest(string? Email, string? FullName, string? Phone, string? Password, string? Role, bool? IsActive);

public record UpdateProfileRequest(string? FullName, string? Phone, string? Email);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record SystemSettingsDto(
    string ShiftStart,
    string ShiftEnd,
    bool IpTrackingEnabled,
    bool SessionApprovalRequired);

public record UpdateSystemSettingsRequest(
    string? ShiftStart,
    string? ShiftEnd,
    bool? IpTrackingEnabled,
    bool? SessionApprovalRequired);

public record UserStatsDto(int TotalSessionsLogged, int TasksCompleted);

public record UserProfileDto(UserDto User, UserStatsDto? Stats);

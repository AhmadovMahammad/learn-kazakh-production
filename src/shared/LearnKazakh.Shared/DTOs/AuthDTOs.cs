namespace LearnKazakh.Shared.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserProfileDto UserProfileDto { get; set; } = null!;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { set; get; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class ForgetPasswordRequest
{
    public string Email { set; get; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string CurrentPassword { set; get; } = string.Empty;
    public string NewPassword { set; get; } = string.Empty;
}

public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
}
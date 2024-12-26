namespace MiracleLandBE.MinimalModels;

public class UserRegisterRequest
{
    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

}

public class UserLoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class PasswordChangeRequest
{
    public string? token { get; set; }
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}

public class GetAccountInfo
{
    public string? token { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Avt { get; set; }
}

public class UserAccountUpdate
{
    public string? token { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? AvatarContent { get; set; }

}
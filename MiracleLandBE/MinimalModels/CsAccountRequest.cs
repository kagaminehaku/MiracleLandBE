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
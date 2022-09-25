namespace Auth.Application.DTOs
{
    public record UserRegisterRequest(string Email, string Pwd, string Username);
    public record UserRegisterResponse(bool Success, IEnumerable<string>? Errors);

    public record UserLoginRequest(string Email, string Pwd);
    public record UserLoginResponse(bool Success, string? Token, DateTime? ExpireDate);
}

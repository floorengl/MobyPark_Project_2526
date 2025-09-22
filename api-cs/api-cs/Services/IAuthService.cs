public interface IAuthService
{
    Task<(bool Ok, string Message)> RegisterAsync(RegisterRequest req);
}
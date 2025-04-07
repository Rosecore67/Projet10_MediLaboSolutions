namespace IdentityAuthService.Service.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(string username);
    }
}

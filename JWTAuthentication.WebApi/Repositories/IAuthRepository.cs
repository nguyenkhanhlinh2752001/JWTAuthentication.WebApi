using JWTAuthentication.WebApi.Models;

namespace JWTAuthentication.WebApi.Repositories
{
    public interface IAuthRepository
    {
        Task<Response> Register(RegisterModel model);
    }
}

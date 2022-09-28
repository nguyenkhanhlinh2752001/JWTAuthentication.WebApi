using JWTAuthentication.WebApi.Models;

namespace JWTAuthentication.WebApi.Repositories
{
    public interface IAuthRepository
    {
        Task<Response> UserRegister(RegisterModel model);
        Task<Response> AdminRegister(RegisterModel model);
        Task<Response> Login(LoginModel model);
    }
}

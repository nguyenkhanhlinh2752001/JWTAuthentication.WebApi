using JWTAuthentication.WebApi.Models;
using Microsoft.AspNetCore.Identity;

namespace JWTAuthentication.WebApi.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly  UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthRepository(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<Response> Register(RegisterModel model)
        {
            var userExist = await _userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
                return new Response
                {
                    Message = "User already exists",
                    IsSuccess = false
                };
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };

            var rs = await _userManager.CreateAsync(user, model.Password);
            if (rs.Succeeded)
                return new Response
                {
                    Message = "Registed successfully",
                    IsSuccess = true
                };

            return new Response
            {
                Message = "Registed failed",
                IsSuccess = false,
                Errors = rs.Errors.Select(e => e.Description)
            };
        }
    }
}

using JWTAuthentication.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        public async Task<Response> AdminRegister(RegisterModel model)
        {
            var userExist = await _userManager.FindByEmailAsync(model.Email);
            if (userExist != null)
                return new Response
                {
                    Message = "Admin already exists",
                    IsSuccess = false
                };
            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var rs = await _userManager.CreateAsync(user, model.Password);
            if (!rs.Succeeded)
                return new Response
                {
                    Message = "Registed failed",
                    IsSuccess = false,
                    Errors = rs.Errors.Select(e => e.Description)
                };

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.User))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _userManager.AddToRoleAsync(user, UserRoles.User);

            return new Response
            {
                Message = "Register successfully",
                IsSuccess = true
            };
        }

        public async Task<Response> Login(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user!=null && await _userManager.CheckPasswordAsync(user, model.Password)){
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach(var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);
                return new Response
                {
                    Message = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    IsSuccess = true,
                };
            }
            return new Response
            {
                Message = "Unauthorized",
                IsSuccess = false
            };
        }

        public async Task<Response> UserRegister(RegisterModel model)
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

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}

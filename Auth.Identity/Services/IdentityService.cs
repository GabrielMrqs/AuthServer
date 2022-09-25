using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Identity.Configuration;
using Auth.Application.DTOs;

namespace Auth.Identity.Services
{
    public class IdentityService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtOptions _jwtOptions;

        public IdentityService(SignInManager<IdentityUser> signInManager,
                               UserManager<IdentityUser> userManager,
                               IOptions<JwtOptions> jwtOptions)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<UserRegisterResponse> RegisterUser(UserRegisterRequest user)
        {
            var identityUser = new IdentityUser
            {
                UserName = user.Username,
                Email = user.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, user.Pwd);

            if (result.Succeeded)
            {
                await _userManager.SetLockoutEnabledAsync(identityUser, false);
            }

            var errors = result.Errors.Select(x => x.Description);

            var response = new UserRegisterResponse(result.Succeeded, errors);

            return response;
        }

        public async Task<UserLoginResponse> Login(UserLoginRequest user)
        {
            var userLogin = await _userManager.FindByEmailAsync(user.Email);

            if (userLogin is null)
            {
                return new UserLoginResponse(false, null, null);
            }

            var result = await _signInManager.PasswordSignInAsync(userLogin, user.Pwd, false, true);

            if (result.Succeeded)
            {
                return await GenerateToken(user.Email);
            }

            var response = new UserLoginResponse(result.Succeeded, null, null);

            return response;
        }

        private async Task<UserLoginResponse> GenerateToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            var tokenClaims = await ObterClaims(user);

            var expireDate = DateTime.UtcNow.AddSeconds(_jwtOptions.Expiration);

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: tokenClaims,
                notBefore: DateTime.UtcNow,
                expires: expireDate,
                signingCredentials: _jwtOptions.SigningCredentials);

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new UserLoginResponse(true, token, expireDate);

            return response;
        }

        private async Task<IEnumerable<Claim>> ObterClaims(IdentityUser user)
        {
            var claims = await _userManager.GetClaimsAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, DateTime.UtcNow.ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()));

            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            return claims;
        }
    }
}

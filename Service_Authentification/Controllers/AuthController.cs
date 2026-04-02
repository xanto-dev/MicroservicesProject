using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Service_Authentification.Data;
using Service_Authentification.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service_Authentification.Controllers
{
    // Route alignée avec Ocelot : /api/authentification
    [Route("api/authentification")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AuthDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // 1. Chercher l'utilisateur par son email
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == login.Email);

            // 2. Vérifier si l'utilisateur existe et si le mot de passe correspond au hash
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.MotDePasse, user.MotDePasseHash))
            {
                return Unauthorized("Email ou mot de passe incorrect.");
            }

            // 3. Générer le Token JWT
            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(Utilisateur user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Clé JWT manquante");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Informations contenues dans le token (Claims)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // Le token expire dans 2 heures
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
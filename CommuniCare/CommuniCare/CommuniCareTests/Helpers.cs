using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommuniCare;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using MockQueryable.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace CommuniCareTest
{
    public static class Helpers
    {

        private const string SecretKey = "super_segredo_muito_forte_123456"; // a tua chave do appsettings.json
        private const string Issuer = "AMinhaAPI";
        private const string Audience = "AMinhaAPIUtilizadores";
        private const int ExpirationMinutes = 60; // Definir um valor de expiração de 60 minutos

        public static string GerarTokenJwt(int utilizadorId, string email, int tipoUtilizadorId)
        {
            // Usando as constantes diretamente ao invés de ler do IConfiguration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, utilizadorId.ToString()),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, tipoUtilizadorId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        // Para utilizador normal
        public static string GerarTokenUtilizador()
        {
            return GerarTokenJwt(1, "utilizador@teste.pt", 1);
        }

        // Para administrador
        public static string GerarTokenAdministrador()
        {
            return GerarTokenJwt(2, "admin@teste.pt", 2);
        }

    }




}

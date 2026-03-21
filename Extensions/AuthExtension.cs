using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CatalogAPI.Extensions
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var key = configuration["JWT_SECRET"];
            var issuer = configuration["Jwt:Issuer"];
            var audience = configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("JWT Key não configurada");

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("JWT Issuer não configurado");

            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("JWT Audience não configurado");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(key)
                            )
                    };
                });

            return services;
        }
    }
}

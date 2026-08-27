using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CatalogAPI.API.Extensions;

public static class BuilderExtension
{
    public static void AddBearerAuthentication(this WebApplicationBuilder builder)
    {
        // A SecretKey precisa ser a MESMA usada pela UsersAPI (emissora do token),
        // pois a assinatura HMAC (simetrica) e validada offline, sem request HTTP.
        var secretKey = builder.Configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException(
                "JwtSettings:SecretKey nao configurada. Necessaria para validar o JWT emitido pela UsersAPI.");

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorization();
    }

    public static void AddConfigureJsonStringEnumConverter(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}
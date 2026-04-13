using DocumentQnA.Application.Common.Behaviors;
using FluentValidation;
using MediatR;

namespace DocumentQnA.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.Documents.Commands.UploadDocumentCommand).Assembly));

        services.AddValidatorsFromAssembly(typeof(Application.Documents.Commands.UploadDocumentCommand).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? ""))
                };
            });

        services.AddAuthorization();
        return services;
    }
}
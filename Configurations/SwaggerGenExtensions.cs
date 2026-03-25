using Microsoft.OpenApi;

namespace CaManagement.Api.Configurations;

public static class SwaggerGenExtensions
{
    public static IServiceCollection AddCaManagementSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Chartered Accountant Management API",
                Version = "v1",
                Description =
                    "Reference PoC for .NET 10 / C# 14: in-memory ledgers, mock transactions, and audit summaries. " +
                    "No database or authentication — for learning only."
            });
            options.EnableAnnotations();
            var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(ServiceCollectionExtensions).Assembly.GetName().Name}.xml");
            if (File.Exists(xml))
                options.IncludeXmlComments(xml, includeControllerXmlComments: true);

        });
        return services;
    }
}

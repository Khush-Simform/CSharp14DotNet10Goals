using CaManagement.Api.Services;

namespace CaManagement.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCaManagementServices(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<CaDataStore>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}

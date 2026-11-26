using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using USBInfilteratorDetectorServiceAPI.Domain.Interfaces;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Notifications;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Repositories;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Security;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;
using USBInfilteratorDetectorServiceAPI.Services.Options;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure;

/// <summary>
/// Registers Infrastructure services (EF Core, security, notifications).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure dependencies to the service collection.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<NotificationOptions>(configuration.GetSection(NotificationOptions.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<AppCorsOptions>(configuration.GetSection(AppCorsOptions.SectionName));
        services.Configure<PortalOptions>(options =>
            options.PortalUrl = configuration[PortalOptions.PortalUrlKey] ?? options.PortalUrl);

        var superAdmins = configuration.GetSection(SuperAdminOptions.SectionName).Get<List<SuperAdminAccountOptions>>()
                          ?? new List<SuperAdminAccountOptions>();
        services.Configure<SuperAdminOptions>(options => options.Accounts = superAdmins);

        var emailEnabled = configuration.GetValue($"{NotificationOptions.SectionName}:Email:Enabled", false);
        if (emailEnabled)
        {
            services.AddOptions<EmailSettings>()
                .Bind(configuration.GetSection(EmailSettings.SectionName))
                .Validate(
                    settings =>
                        !string.IsNullOrWhiteSpace(settings.SmtpServer) &&
                        settings.Port > 0 &&
                        settings.Port <= 65535 &&
                        !string.IsNullOrWhiteSpace(settings.Username) &&
                        !string.IsNullOrWhiteSpace(settings.Password),
                    "Email SMTP configuration is incomplete or invalid.")
                .ValidateOnStart();
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IOrganizationApiCredentialRepository, OrganizationApiCredentialRepository>();
        services.AddScoped<IAuditorRepository, AuditorRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAllowedDeviceRepository, AllowedDeviceRepository>();
        services.AddScoped<IHrStaffRepository, HrStaffRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IHostRepository, HostRepository>();
        services.AddScoped<IUsbSessionRepository, UsbSessionRepository>();
        services.AddScoped<INotificationsLogRepository, NotificationsLogRepository>();

        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddSingleton<ICorpCodeGenerator, CorpCodeGenerator>();
        services.AddSingleton<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailNotificationSender, EmailNotificationSender>();
        services.AddHostedService<NotificationBackgroundService>();

        return services;
    }
}

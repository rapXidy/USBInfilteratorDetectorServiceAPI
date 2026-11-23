using Microsoft.Extensions.DependencyInjection;
using USBInfilteratorDetectorServiceAPI.Services.Auditors;
using USBInfilteratorDetectorServiceAPI.Services.Auth;
using USBInfilteratorDetectorServiceAPI.Services.Dashboard;
using USBInfilteratorDetectorServiceAPI.Services.Devices;
using USBInfilteratorDetectorServiceAPI.Services.HrStaff;
using USBInfilteratorDetectorServiceAPI.Services.Ingest;
using USBInfilteratorDetectorServiceAPI.Services.Onboarding;
using USBInfilteratorDetectorServiceAPI.Services.Organizations;
using USBInfilteratorDetectorServiceAPI.Services.Sessions;
using USBInfilteratorDetectorServiceAPI.Services.Subscriptions;

namespace USBInfilteratorDetectorServiceAPI.Services;

/// <summary>
/// Registers application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application-layer services to the DI container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<OnboardingService>();
        services.AddScoped<AuthService>();
        services.AddScoped<SuperAdminAuthService>();
        services.AddScoped<SuperAdminOrganizationService>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<AuditorUserService>();
        services.AddScoped<IngestService>();
        services.AddScoped<SessionService>();
        services.AddScoped<AlertService>();
        services.AddScoped<AllowedDeviceService>();
        services.AddScoped<HrStaffService>();
        services.AddScoped<SubscriptionService>();
        services.AddScoped<DashboardService>();
        return services;
    }
}

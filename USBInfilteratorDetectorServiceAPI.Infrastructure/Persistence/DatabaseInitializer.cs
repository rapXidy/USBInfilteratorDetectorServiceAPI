using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using USBInfilteratorDetectorServiceAPI.Domain.Entities;
using USBInfilteratorDetectorServiceAPI.Domain.Enums;
using USBInfilteratorDetectorServiceAPI.Services.Abstractions;

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence;

/// <summary>
/// Applies EF migrations at startup and optionally seeds development data.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Migrates the database and seeds a demo organization in Development.
    /// </summary>
    public static async Task MigrateAndSeedAsync(IServiceProvider services, IHostEnvironment environment, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        logger.LogInformation("Applying EF Core migrations...");
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(60));

        try
        {
            await db.Database.MigrateAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                "Timed out applying EF Core migrations. Ensure SQL Server is running and reachable at the configured connection string (Server=localhost).");
        }

        if (!environment.IsDevelopment())
            return;

        if (await db.Organizations.AnyAsync(x => x.CorpCode == "DEMO-001", cancellationToken))
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var now = DateTime.UtcNow;
        var orgId = Guid.NewGuid();
        var auditorId = Guid.NewGuid();

        // Dev-only seed credentials (override via User Secrets in real environments).
        const string demoPassword = "ChangeMe!DemoAdmin1";
        const string demoApiKey = "uid_demo_dev_api_key_do_not_use_prod";

        var organization = new Organization
        {
            Id = orgId,
            CorpCode = "DEMO-001",
            Name = "Demo Organization",
            Email = "demo@example.com",
            Phone = "+10000000000",
            OfficeAddress = "1 Demo Street",
            Sector = "Technology",
            CreatedAt = now,
            IsActive = true,
            OnboardingStatus = OnboardingStatus.Completed,
            OnboardingCompletedAt = now
        };

        var auditor = new Auditor
        {
            Id = auditorId,
            OrganizationId = orgId,
            Username = "admin",
            PasswordHash = hasher.Hash(demoPassword),
            DisplayName = "Demo Admin",
            Role = AuditorRole.Admin,
            IsActive = true,
            CreatedAt = now
        };

        var credential = new OrganizationApiCredential
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            ApiKeyHash = hasher.Hash(demoApiKey),
            ApiKeyPrefix = demoApiKey[..12],
            CreatedAt = now,
            IsActive = true
        };

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Plan = SubscriptionPlan.FreeTrial,
            Status = SubscriptionStatus.Active,
            StartsAt = now,
            EndsAt = now.AddDays(30),
            CreatedAt = now
        };

        await db.Organizations.AddAsync(organization, cancellationToken);
        await db.Auditors.AddAsync(auditor, cancellationToken);
        await db.OrganizationApiCredentials.AddAsync(credential, cancellationToken);
        await db.Subscriptions.AddAsync(subscription, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded DEMO-001 organization (admin / ChangeMe!DemoAdmin1).");
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence;

#nullable disable

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20261124070000_AddEmailNotificationDispatch")]
public partial class AddEmailNotificationDispatch : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Email",
            table: "Auditors",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "DestinationEmail",
            table: "NotificationsLogs",
            type: "nvarchar(256)",
            maxLength: 256,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<DateTime>(
            name: "LastAttemptedAt",
            table: "NotificationsLogs",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RetryCount",
            table: "NotificationsLogs",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_NotificationsLogs_Status_RetryCount_CreatedAt",
            table: "NotificationsLogs",
            columns: new[] { "Status", "RetryCount", "CreatedAt" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_NotificationsLogs_Status_RetryCount_CreatedAt",
            table: "NotificationsLogs");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "Auditors");

        migrationBuilder.DropColumn(
            name: "DestinationEmail",
            table: "NotificationsLogs");

        migrationBuilder.DropColumn(
            name: "LastAttemptedAt",
            table: "NotificationsLogs");

        migrationBuilder.DropColumn(
            name: "RetryCount",
            table: "NotificationsLogs");
    }
}

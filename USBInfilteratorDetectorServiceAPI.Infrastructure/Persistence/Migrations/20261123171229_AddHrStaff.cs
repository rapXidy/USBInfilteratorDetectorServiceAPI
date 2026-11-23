using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace USBInfilteratorDetectorServiceAPI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHrStaff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HrStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EmployeeNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WindowsUsername = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DateEmployed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateOfResignation = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfExit = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByAuditorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HrStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HrStaff_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HrStaff_OrganizationId_DateOfResignation",
                table: "HrStaff",
                columns: new[] { "OrganizationId", "DateOfResignation" });

            migrationBuilder.CreateIndex(
                name: "IX_HrStaff_OrganizationId_DeviceId",
                table: "HrStaff",
                columns: new[] { "OrganizationId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_HrStaff_OrganizationId_EmployeeNumber",
                table: "HrStaff",
                columns: new[] { "OrganizationId", "EmployeeNumber" },
                unique: true,
                filter: "[EmployeeNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HrStaff_OrganizationId_WindowsUsername",
                table: "HrStaff",
                columns: new[] { "OrganizationId", "WindowsUsername" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HrStaff");
        }
    }
}

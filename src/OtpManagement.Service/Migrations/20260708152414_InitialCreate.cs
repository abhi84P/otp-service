using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OtpManagement.Service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IdentifierStates",
                columns: table => new
                {
                    Identifier = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    FailedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    FrozenUntil = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentifierStates", x => x.Identifier);
                });

            migrationBuilder.CreateTable(
                name: "OtpRequests",
                columns: table => new
                {
                    RequestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Identifier = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    CodeHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpRequests", x => x.RequestId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OtpRequests_Identifier_Status",
                table: "OtpRequests",
                columns: ["Identifier", "Status"]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IdentifierStates");

            migrationBuilder.DropTable(
                name: "OtpRequests");
        }
    }
}

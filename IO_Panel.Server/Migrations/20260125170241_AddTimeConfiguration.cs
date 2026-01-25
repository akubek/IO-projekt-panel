using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IO_Panel.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimeZoneId = table.Column<string>(type: "TEXT", nullable: false),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    VirtualNowAtAppliedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeConfigurations_Id",
                table: "TimeConfigurations",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeConfigurations");
        }
    }
}

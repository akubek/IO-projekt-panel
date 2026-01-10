using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IO_Panel.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationsTriggerActionJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionJson",
                table: "Automations",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "TriggerJson",
                table: "Automations",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            // Best-effort data carry-over:
            // old LogicDefinition (free-form) becomes TriggerJson (still TEXT) so data isn't lost.
            migrationBuilder.Sql("""
                UPDATE Automations
                SET TriggerJson = COALESCE(NULLIF(LogicDefinition, ''), '{}'),
                    ActionJson = '{}'
            """);

            migrationBuilder.DropColumn(
                name: "LogicDefinition",
                table: "Automations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogicDefinition",
                table: "Automations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Automations
                SET LogicDefinition = COALESCE(NULLIF(TriggerJson, ''), '')
            """);

            migrationBuilder.DropColumn(
                name: "ActionJson",
                table: "Automations");

            migrationBuilder.DropColumn(
                name: "TriggerJson",
                table: "Automations");
        }
    }
}
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IO_Panel.Server.Migrations
{
    /// <inheritdoc />
    public partial class StoreDeviceStateHistoryRecordedAtAsUtcDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecordedAt",
                table: "DeviceStateHistory",
                newName: "RecordedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceStateHistory_DeviceId_RecordedAt",
                table: "DeviceStateHistory",
                newName: "IX_DeviceStateHistory_DeviceId_RecordedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RecordedAtUtc",
                table: "DeviceStateHistory",
                newName: "RecordedAt");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceStateHistory_DeviceId_RecordedAtUtc",
                table: "DeviceStateHistory",
                newName: "IX_DeviceStateHistory_DeviceId_RecordedAt");
        }
    }
}

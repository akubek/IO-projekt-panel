namespace IO_Panel.Server.Models;

public sealed record DeviceStateHistoryPoint(DateTimeOffset RecordedAt, double Value, string? Unit);
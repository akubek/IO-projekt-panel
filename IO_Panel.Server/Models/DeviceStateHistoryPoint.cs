namespace IO_Panel.Server.Models;

/// <summary>
/// Read model representing a single historical device measurement sampled at a timestamp.
/// </summary>
public sealed record DeviceStateHistoryPoint(DateTimeOffset RecordedAt, double Value, string? Unit);
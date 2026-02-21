namespace PackTrack.Models.Dtos;

/// <summary>
/// Lightweight DTOs for the Transport Dashboard ViewModel.
/// </summary>

public record FleetSummary(int Total, int Available, int OnRoad, int Maintenance);

public record ShipmentSummary(int InTransit, int Delivered, int Delayed);

public record TransportAlert
{
    public string Severity { get; init; } = "Info";   // Error | Warning | Info
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.Now;
}

public record DriverStats
{
    public string DriverId { get; init; } = string.Empty;
    public int TotalTrips { get; init; }
    public int OnTimeDeliveries { get; init; }
    public double OnTimePercent => TotalTrips > 0 ? Math.Round((double)OnTimeDeliveries / TotalTrips * 100, 1) : 0;
    public double AvgTripHours { get; init; }
}

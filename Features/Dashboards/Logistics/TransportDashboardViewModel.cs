using PackTrack.Models;
using PackTrack.Models.Dtos;
using PackTrack.Services;

namespace PackTrack.Features.Dashboards.Logistics;

/// <summary>
/// MVVM ViewModel for the Transport Dashboard.
/// Pure C# — no UI dependencies. Registered as Scoped in DI.
/// </summary>
public class TransportDashboardViewModel : ITransportDashboardViewModel
{
    private readonly LogisticsService _logistics;

    public TransportDashboardViewModel(LogisticsService logistics)
    {
        _logistics = logistics;
    }

    // ── State ────────────────────────────────────────────────────
    public bool IsLoading { get; private set; } = true;

    // ── Scalar KPIs ──────────────────────────────────────────────
    public int TotalFleetSize { get; private set; }
    public int ActiveShipmentCount { get; private set; }
    public int DeliveredCount { get; private set; }
    public double OnTimeDeliveryPercent { get; private set; }
    public double FleetUtilizationPercent { get; private set; }
    public int AlertCount => Alerts.Count;

    // ── Summaries ────────────────────────────────────────────────
    public FleetSummary Fleet { get; private set; } = new(0, 0, 0, 0);
    public ShipmentSummary ShipmentStats { get; private set; } = new(0, 0, 0);

    // ── Collections ──────────────────────────────────────────────
    public IEnumerable<Truck> Trucks { get; private set; } = Enumerable.Empty<Truck>();
    public IEnumerable<Shipment> Shipments { get; private set; } = Enumerable.Empty<Shipment>();
    public IEnumerable<Shipment> RecentDeliveries { get; private set; } = Enumerable.Empty<Shipment>();
    public List<TransportAlert> Alerts { get; private set; } = new();
    public List<DriverStats> DriverPerformance { get; private set; } = new();

    // ── Commands ─────────────────────────────────────────────────
    public void Load() => Refresh();

    public void Refresh()
    {
        IsLoading = true;
        try
        {
            // Fleet
            var allTrucks = _logistics.GetAllTrucks().ToList();
            Trucks = allTrucks;
            TotalFleetSize = allTrucks.Count;
            int available = allTrucks.Count(t => t.Status == "Available");
            int onRoad = allTrucks.Count(t => t.Status == "OnRoad");
            int maintenance = allTrucks.Count(t => t.Status == "Maintenance");
            Fleet = new FleetSummary(TotalFleetSize, available, onRoad, maintenance);
            FleetUtilizationPercent = TotalFleetSize > 0
                ? Math.Round((double)onRoad / TotalFleetSize * 100, 1)
                : 0;

            // Shipments
            var allShipments = _logistics.GetAllShipments().ToList();
            Shipments = allShipments;
            ActiveShipmentCount = allShipments.Count(s => s.Status == "InTransit");
            DeliveredCount = allShipments.Count(s => s.Status == "Delivered");
            int delayed = allShipments.Count(s =>
                s.Status == "InTransit" && DateTime.Now > s.ExpectedDelivery);
            ShipmentStats = new ShipmentSummary(ActiveShipmentCount, DeliveredCount, delayed);

            // On-time %
            var delivered = allShipments.Where(s => s.Status == "Delivered").ToList();
            if (delivered.Any())
            {
                int onTime = delivered.Count(s =>
                    s.ActualDelivery.HasValue && s.ActualDelivery.Value <= s.ExpectedDelivery);
                OnTimeDeliveryPercent = Math.Round((double)onTime / delivered.Count * 100, 1);
            }
            else
            {
                OnTimeDeliveryPercent = 100;
            }

            RecentDeliveries = _logistics.GetRecentShipments(10);

            // Driver stats
            DriverPerformance = allShipments
                .Where(s => !string.IsNullOrEmpty(s.DriverId))
                .GroupBy(s => s.DriverId)
                .Select(g =>
                {
                    var trips = g.ToList();
                    var completedTrips = trips.Where(s => s.Status == "Delivered").ToList();
                    int onTimeCount = completedTrips.Count(s =>
                        s.ActualDelivery.HasValue && s.ActualDelivery.Value <= s.ExpectedDelivery);
                    double avgHours = completedTrips.Any() && completedTrips.All(s => s.ActualDelivery.HasValue && s.DispatchTime.HasValue)
                        ? completedTrips.Average(s => (s.ActualDelivery!.Value - s.DispatchTime!.Value).TotalHours)
                        : 0;

                    return new DriverStats
                    {
                        DriverId = g.Key,
                        TotalTrips = trips.Count,
                        OnTimeDeliveries = onTimeCount,
                        AvgTripHours = Math.Round(avgHours, 1)
                    };
                })
                .OrderByDescending(d => d.TotalTrips)
                .ToList();

            // Alerts
            BuildAlerts(allShipments, allTrucks);
        }
        catch (Exception ex)
        {
            Alerts.Add(new TransportAlert
            {
                Severity = "Error",
                Title = "Data Load Failed",
                Description = ex.Message,
                Timestamp = DateTime.Now
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void MarkDeliveryComplete(int shipmentId)
    {
        _logistics.MarkDelivered(shipmentId);
        Refresh();
    }

    public byte[] ExportShipmentsCsv() => _logistics.ExportShipmentsCsv();

    // ── Private Helpers ──────────────────────────────────────────
    private void BuildAlerts(List<Shipment> shipments, List<Truck> trucks)
    {
        Alerts = new List<TransportAlert>();

        // Delayed shipments
        var delayed = shipments.Where(s =>
            s.Status == "InTransit" && DateTime.Now > s.ExpectedDelivery).ToList();
        foreach (var d in delayed)
        {
            Alerts.Add(new TransportAlert
            {
                Severity = "Error",
                Title = $"Shipment #{d.Id} OVERDUE",
                Description = $"Truck {d.TruckId} on route {d.RouteCode} — expected {d.ExpectedDelivery:MMM dd} but still in transit.",
                Timestamp = d.ExpectedDelivery
            });
        }

        // Trucks with no GPS
        foreach (var t in trucks.Where(t => string.IsNullOrEmpty(t.GPSDeviceId)))
        {
            Alerts.Add(new TransportAlert
            {
                Severity = "Warning",
                Title = $"Truck {t.TruckNumber} — No GPS",
                Description = "GPS device ID is missing. Live tracking unavailable.",
                Timestamp = DateTime.Now
            });
        }

        // Fleet fully utilized
        if (trucks.Any() && trucks.All(t => t.Status != "Available"))
        {
            Alerts.Add(new TransportAlert
            {
                Severity = "Warning",
                Title = "Fleet Fully Utilized",
                Description = "No trucks are currently available. New shipments will be queued.",
                Timestamp = DateTime.Now
            });
        }

        // Info — empty fleet
        if (!trucks.Any())
        {
            Alerts.Add(new TransportAlert
            {
                Severity = "Info",
                Title = "No Trucks Registered",
                Description = "Register trucks in the system to start tracking logistics.",
                Timestamp = DateTime.Now
            });
        }
    }
}

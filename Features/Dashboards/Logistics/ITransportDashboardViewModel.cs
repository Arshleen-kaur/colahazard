using PackTrack.Models;
using PackTrack.Models.Dtos;

namespace PackTrack.Features.Dashboards.Logistics;

/// <summary>
/// Contract for the Transport Dashboard ViewModel.
/// The Razor view binds exclusively to this interface — never to raw services.
/// </summary>
public interface ITransportDashboardViewModel
{
    // ── Scalar KPIs ──────────────────────────────────────────────
    int TotalFleetSize { get; }
    int ActiveShipmentCount { get; }
    int DeliveredCount { get; }
    double OnTimeDeliveryPercent { get; }
    double FleetUtilizationPercent { get; }
    int AlertCount { get; }
    bool IsLoading { get; }

    // ── Summaries ────────────────────────────────────────────────
    FleetSummary Fleet { get; }
    ShipmentSummary ShipmentStats { get; }

    // ── Collections ──────────────────────────────────────────────
    IEnumerable<Truck> Trucks { get; }
    IEnumerable<Shipment> Shipments { get; }
    IEnumerable<Shipment> RecentDeliveries { get; }
    List<TransportAlert> Alerts { get; }
    List<DriverStats> DriverPerformance { get; }

    // ── Commands ─────────────────────────────────────────────────
    void Load();
    void Refresh();
    void MarkDeliveryComplete(int shipmentId);
    byte[] ExportShipmentsCsv();
}

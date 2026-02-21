using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.Transport
{
    public class TransportDashboardService
    {
        private readonly LiteDbContext _context;

        public TransportDashboardService(LiteDbContext context)
        {
            _context = context;
        }

        public TransportDashboardResponse GetDashboard(string driverId)
        {
            // Implementation for transport dashboard
            return new TransportDashboardResponse
            {
                DriverId = driverId,
                // Add dashboard data
            };
        }
    }

    public class TransportDashboardResponse
    {
        public string DriverId { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string TruckId { get; set; } = string.Empty;
        public string TruckNumber { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public TodayStats TodayStats { get; set; } = new();
        public CurrentLocation CurrentLocation { get; set; } = new();
        public List<ActiveDelivery> ActiveDeliveries { get; set; } = new();
        public List<Alert> Alerts { get; set; } = new();
    }

    public class TodayStats
    {
        public int DeliveriesCompleted { get; set; }
        public int DeliveriesPending { get; set; }
        public double DistanceTraveled { get; set; }
        public double FuelConsumed { get; set; }
        public double HoursOnDuty { get; set; }
    }

    public class CurrentLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ActiveDelivery
    {
        public string TaskId { get; set; } = string.Empty;
        public string RetailerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public string Priority { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class Alert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}

using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.Transport
{
    public class LocationTrackingService
    {
        private readonly LiteDbContext _context;

        public LocationTrackingService(LiteDbContext context)
        {
            _context = context;
        }

        public LocationUpdateResponse UpdateLocation(LocationUpdateRequest request)
        {
            // Save GPS location to database
            var gpsLog = new GPSLog
            {
                TruckId = request.TruckId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Speed = request.Speed,
                Heading = request.Heading,
                Timestamp = request.Timestamp
            };

            // Save to database
            // _context.GPSLogs.Insert(gpsLog);

            return new LocationUpdateResponse
            {
                Success = true,
                LogId = $"GPS-{DateTime.Now:yyyyMMdd}-{new Random().Next(10000, 99999)}",
                NearbyDeliveries = new List<NearbyDelivery>(),
                RouteDeviation = false
            };
        }

        public List<GPSLog> GetLocationHistory(string truckId, DateTime startDate, DateTime endDate)
        {
            // Get location history from database
            return new List<GPSLog>();
        }
    }

    public class LocationUpdateRequest
    {
        public string TruckId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public double Accuracy { get; set; }
        public DateTime Timestamp { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsMoving { get; set; }
    }

    public class LocationUpdateResponse
    {
        public bool Success { get; set; }
        public string LogId { get; set; } = string.Empty;
        public List<NearbyDelivery> NearbyDeliveries { get; set; } = new();
        public bool RouteDeviation { get; set; }
        public List<string> GeofenceAlerts { get; set; } = new();
    }

    public class NearbyDelivery
    {
        public string TaskId { get; set; } = string.Empty;
        public double Distance { get; set; }
        public int Eta { get; set; }
    }

    public class GPSLog
    {
        public int Id { get; set; }
        public string TruckId { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Speed { get; set; }
        public double Heading { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

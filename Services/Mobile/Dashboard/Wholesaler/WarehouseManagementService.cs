using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.Wholesaler
{
    public class WarehouseManagementService
    {
        private readonly LiteDbContext _context;

        public WarehouseManagementService(LiteDbContext context)
        {
            _context = context;
        }

        public WarehouseStatusResponse GetWarehouseStatus(int wholesalerId)
        {
            return new WarehouseStatusResponse
            {
                WholesalerId = wholesalerId,
                WarehouseName = "Metro Distribution Center",
                TotalCapacity = 200000,
                CurrentStock = 125000,
                OccupancyPercentage = 62.5,
                AvailableSpace = 75000,
                Zones = new List<WarehouseZone>(),
                RecentActivity = new List<WarehouseActivity>(),
                Alerts = new List<WarehouseAlert>()
            };
        }

        public ZoneUpdateResponse UpdateZone(ZoneUpdateRequest request)
        {
            // Update zone information
            return new ZoneUpdateResponse
            {
                Success = true,
                Message = "Zone updated successfully"
            };
        }
    }

    public class WarehouseStatusResponse
    {
        public int WholesalerId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int TotalCapacity { get; set; }
        public int CurrentStock { get; set; }
        public double OccupancyPercentage { get; set; }
        public int AvailableSpace { get; set; }
        public List<WarehouseZone> Zones { get; set; } = new();
        public List<WarehouseActivity> RecentActivity { get; set; } = new();
        public List<WarehouseAlert> Alerts { get; set; } = new();
    }

    public class WarehouseZone
    {
        public string ZoneId { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentStock { get; set; }
        public double Occupancy { get; set; }
        public double Temperature { get; set; }
        public int Humidity { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> AssignedProducts { get; set; } = new();
    }

    public class WarehouseActivity
    {
        public DateTime Timestamp { get; set; }
        public string Activity { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Product { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
    }

    public class WarehouseAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Zone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }

    public class ZoneUpdateRequest
    {
        public int WholesalerId { get; set; }
        public string ZoneId { get; set; } = string.Empty;
        public int? CurrentStock { get; set; }
        public double? Temperature { get; set; }
        public int? Humidity { get; set; }
        public string? Status { get; set; }
    }

    public class ZoneUpdateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

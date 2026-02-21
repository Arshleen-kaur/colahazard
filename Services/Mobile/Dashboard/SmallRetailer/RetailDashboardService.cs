using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.SmallRetailer
{
    public class RetailDashboardService
    {
        private readonly LiteDbContext _context;

        public RetailDashboardService(LiteDbContext context)
        {
            _context = context;
        }

        public RetailerDashboardResponse GetDashboard(int retailerId)
        {
            return new RetailerDashboardResponse
            {
                RetailerId = retailerId,
                RetailerName = "SuperMart",
                OwnerName = "Retailer Owner"
            };
        }
    }

    public class RetailerDashboardResponse
    {
        public int RetailerId { get; set; }
        public string RetailerName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public RetailerLocation Location { get; set; } = new();
        public RetailerTodayStats TodayStats { get; set; } = new();
        public RetailerInventorySummary InventorySummary { get; set; } = new();
        public List<RetailerAlert> Alerts { get; set; } = new();
        public int PendingOrders { get; set; }
        public double ShelfOccupancy { get; set; }
        public DemandForecast DemandForecast { get; set; } = new();
    }

    public class RetailerLocation
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class RetailerTodayStats
    {
        public int SalesCount { get; set; }
        public decimal SalesRevenue { get; set; }
        public decimal AverageTransaction { get; set; }
        public string TopSellingProduct { get; set; } = string.Empty;
        public decimal StockValue { get; set; }
        public double ProfitMargin { get; set; }
    }

    public class RetailerInventorySummary
    {
        public int TotalUnits { get; set; }
        public Dictionary<string, int> Thick { get; set; } = new();
        public Dictionary<string, int> Thin { get; set; } = new();
        public Dictionary<string, int> RPET { get; set; } = new();
    }

    public class RetailerAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string ActionRequired { get; set; } = string.Empty;
    }

    public class DemandForecast
    {
        public Dictionary<string, int> NextWeek { get; set; } = new();
        public int Confidence { get; set; }
    }
}

using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.Wholesaler
{
    public class WholesaleDashboardService
    {
        private readonly LiteDbContext _context;

        public WholesaleDashboardService(LiteDbContext context)
        {
            _context = context;
        }

        public WholesalerDashboardResponse GetDashboard(int wholesalerId)
        {
            return new WholesalerDashboardResponse
            {
                WholesalerId = wholesalerId,
                WholesalerName = "Metro Distributors",
                OwnerName = "Wholesaler Owner"
            };
        }
    }

    public class WholesalerDashboardResponse
    {
        public int WholesalerId { get; set; }
        public string WholesalerName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public WholesalerLocation Location { get; set; } = new();
        public WholesalerTodayStats TodayStats { get; set; } = new();
        public WholesalerInventorySummary InventorySummary { get; set; } = new();
        public RetailerNetworkSummary RetailerNetwork { get; set; } = new();
        public CreditSummary CreditSummary { get; set; } = new();
        public List<WholesalerAlert> Alerts { get; set; } = new();
    }

    public class WholesalerLocation
    {
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
    }

    public class WholesalerTodayStats
    {
        public int OrdersReceived { get; set; }
        public int OrdersProcessed { get; set; }
        public int OrdersPending { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int DeliveriesCompleted { get; set; }
        public int DeliveriesPending { get; set; }
    }

    public class WholesalerInventorySummary
    {
        public int TotalUnits { get; set; }
        public decimal TotalValue { get; set; }
        public int WarehouseCapacity { get; set; }
        public double OccupancyPercentage { get; set; }
        public Dictionary<string, Dictionary<string, int>> ByBottleType { get; set; } = new();
    }

    public class RetailerNetworkSummary
    {
        public int TotalRetailers { get; set; }
        public int ActiveRetailers { get; set; }
        public int NewRetailersThisMonth { get; set; }
        public List<TopRetailer> TopRetailers { get; set; } = new();
    }

    public class TopRetailer
    {
        public int RetailerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MonthlyOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }

    public class CreditSummary
    {
        public decimal TotalCreditExtended { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OverdueAmount { get; set; }
        public double CreditUtilization { get; set; }
    }

    public class WholesalerAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}

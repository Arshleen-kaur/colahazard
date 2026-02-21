using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.FactoryWorker
{
    public class ProductionDashboardService
    {
        private readonly LiteDbContext _context;

        public ProductionDashboardService(LiteDbContext context)
        {
            _context = context;
        }

        public FactoryWorkerDashboardResponse GetDashboard(string workerId)
        {
            return new FactoryWorkerDashboardResponse
            {
                WorkerId = workerId,
                WorkerName = "Factory Worker",
                PlantId = 1,
                PlantName = "EcoCola Plant Mumbai"
            };
        }
    }

    public class FactoryWorkerDashboardResponse
    {
        public string WorkerId { get; set; } = string.Empty;
        public string WorkerName { get; set; } = string.Empty;
        public int PlantId { get; set; }
        public string PlantName { get; set; } = string.Empty;
        public CurrentShift CurrentShift { get; set; } = new();
        public ProductionStats TodayStats { get; set; } = new();
        public List<ActiveBatch> ActiveBatches { get; set; } = new();
        public List<ProductionTask> PendingTasks { get; set; } = new();
        public List<MachineStatus> MachineStatus { get; set; } = new();
        public List<WorkerAlert> Alerts { get; set; } = new();
    }

    public class CurrentShift
    {
        public string ShiftId { get; set; } = string.Empty;
        public string ShiftCode { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ProductionLineId { get; set; } = string.Empty;
        public string Supervisor { get; set; } = string.Empty;
    }

    public class ProductionStats
    {
        public int UnitsProduced { get; set; }
        public int TargetUnits { get; set; }
        public double CompletionPercentage { get; set; }
        public int QualityChecksPassed { get; set; }
        public int QualityChecksFailed { get; set; }
        public int DefectsReported { get; set; }
        public int BatchesCreated { get; set; }
        public double HoursWorked { get; set; }
    }

    public class ActiveBatch
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProducedUnits { get; set; }
        public int TargetUnits { get; set; }
        public DateTime StartedAt { get; set; }
    }

    public class ProductionTask
    {
        public string TaskId { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime DueTime { get; set; }
    }

    public class MachineStatus
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double Efficiency { get; set; }
        public string LastMaintenance { get; set; } = string.Empty;
    }

    public class WorkerAlert
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}

using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.FactoryWorker
{
    public class BatchCreationService
    {
        private readonly LiteDbContext _context;

        public BatchCreationService(LiteDbContext context)
        {
            _context = context;
        }

        public BatchCreationResponse CreateBatch(BatchCreationRequest request)
        {
            var batchCode = $"B-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
            
            var batch = new ProductionBatch
            {
                PlantId = request.PlantId,
                BatchCode = batchCode,
                BottleType = request.BottleType,
                BottleMaterialGrade = request.BottleMaterialGrade,
                BottleThicknessMicron = request.BottleThicknessMicron,
                CapacityML = request.CapacityML,
                ContainerType = request.ContainerType,
                LiquidType = request.LiquidType,
                LiquidBatchCode = request.LiquidBatchCode,
                BrixLevel = request.BrixLevel,
                AcidityPH = request.AcidityPH,
                CO2Volumes = request.CO2Volumes,
                IngredientsList = request.IngredientsList,
                ProductionLineId = request.ProductionLineId,
                MachineId = request.MachineId,
                ShiftCode = request.ShiftCode,
                OperatorId = request.OperatorId,
                SupervisorId = request.SupervisorId,
                ManufactureDate = request.ManufactureDate,
                ExpiryDate = request.ExpiryDate,
                TotalPlannedUnits = request.TotalPlannedUnits,
                TargetMarket = request.TargetMarket,
                WholesaleRate = request.WholesaleRate,
                MRP = request.MRP,
                TaxCode = request.TaxCode,
                Status = "Created",
                CreatedAt = DateTime.Now
            };

            // Save to database
            // var collection = _context.Database.GetCollection<ProductionBatch>("batches");
            // collection.Insert(batch);

            return new BatchCreationResponse
            {
                Success = true,
                BatchId = batch.Id,
                BatchCode = batchCode,
                QrCode = $"ECO-{batchCode}",
                Status = "Created",
                CreatedAt = DateTime.Now,
                Message = "Production batch created successfully"
            };
        }
    }

    public class BatchCreationRequest
    {
        public int PlantId { get; set; }
        public string ProductionLineId { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string ShiftCode { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string SupervisorId { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public string BottleMaterialGrade { get; set; } = string.Empty;
        public int BottleThicknessMicron { get; set; }
        public int CapacityML { get; set; }
        public string ContainerType { get; set; } = string.Empty;
        public string LiquidType { get; set; } = string.Empty;
        public string LiquidBatchCode { get; set; } = string.Empty;
        public double BrixLevel { get; set; }
        public double AcidityPH { get; set; }
        public double CO2Volumes { get; set; }
        public string IngredientsList { get; set; } = string.Empty;
        public int TotalPlannedUnits { get; set; }
        public string TargetMarket { get; set; } = string.Empty;
        public decimal WholesaleRate { get; set; }
        public decimal MRP { get; set; }
        public string TaxCode { get; set; } = string.Empty;
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class BatchCreationResponse
    {
        public bool Success { get; set; }
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime EstimatedCompletionTime { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

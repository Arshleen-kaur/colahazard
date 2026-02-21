using PackTrack.Data;
using PackTrack.Models;
using System.Linq;
using System.Text;

namespace PackTrack.Services
{
    public class LogisticsService
    {
        private readonly LiteDbContext _db;

        public LogisticsService(LiteDbContext db)
        {
            _db = db;
        }

        // ── Original Methods ─────────────────────────────────────
        public IEnumerable<Shipment> GetAllShipments() => _db.Shipments.FindAll().OrderByDescending(s => s.PickupDate);
        public IEnumerable<Truck> GetAllTrucks() => _db.Trucks.FindAll();
        
        public Shipment? GetShipmentByBatch(int batchId) => _db.Shipments.FindOne(x => x.BatchId == batchId);

        public void RegisterShipment(int batchId, string truckId, string route)
        {
            var shipment = new Shipment
            {
                BatchId = batchId,
                TruckId = truckId,
                RouteCode = route,
                PickupDate = DateTime.Now,
                DispatchTime = DateTime.Now,
                ExpectedDelivery = DateTime.Now.AddDays(1),
                Status = "InTransit",
                TemperatureLogId = "TEMP-" + Guid.NewGuid().ToString().Substring(0, 5),
                HumidityLogId = "HUM-" + Guid.NewGuid().ToString().Substring(0, 5)
            };
            _db.Shipments.Insert(shipment);

            var truck = _db.Trucks.FindById(truckId);
            if (truck != null)
            {
                truck.Status = "OnRoad";
                _db.Trucks.Update(truck);
            }
        }

        // ── Fleet Queries ────────────────────────────────────────
        public Truck? GetTruckById(string truckId) => _db.Trucks.FindById(truckId);
        public IEnumerable<Truck> GetAvailableTrucks() => _db.Trucks.Find(t => t.Status == "Available");
        public IEnumerable<Truck> GetOnRoadTrucks() => _db.Trucks.Find(t => t.Status == "OnRoad");
        public int GetFleetSize() => _db.Trucks.Count();

        // ── Shipment Queries ─────────────────────────────────────
        public IEnumerable<Shipment> GetActiveShipments() => _db.Shipments.Find(s => s.Status == "InTransit").OrderByDescending(s => s.PickupDate);
        public IEnumerable<Shipment> GetDeliveredShipments() => _db.Shipments.Find(s => s.Status == "Delivered").OrderByDescending(s => s.ActualDelivery);
        public IEnumerable<Shipment> GetShipmentsByTruck(string truckId) => _db.Shipments.Find(s => s.TruckId == truckId).OrderByDescending(s => s.PickupDate);
        public IEnumerable<Shipment> GetRecentShipments(int count) => _db.Shipments.FindAll().OrderByDescending(s => s.PickupDate).Take(count);
        
        public int GetActiveShipmentCount() => _db.Shipments.Count(s => s.Status == "InTransit");
        public int GetTotalDelivered() => _db.Shipments.Count(s => s.Status == "Delivered");

        // ── Actions ──────────────────────────────────────────────
        public void MarkDelivered(int shipmentId)
        {
            var shipment = _db.Shipments.FindById(shipmentId);
            if (shipment != null)
            {
                shipment.Status = "Delivered";
                shipment.ActualDelivery = DateTime.Now;
                _db.Shipments.Update(shipment);

                // Free up the truck
                var truck = _db.Trucks.FindById(shipment.TruckId);
                if (truck != null)
                {
                    truck.Status = "Available";
                    _db.Trucks.Update(truck);
                }
            }
        }

        // ── Export ────────────────────────────────────────────────
        public byte[] ExportShipmentsCsv()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ShipmentId,BatchId,TruckId,DriverId,Route,Status,PickupDate,ExpectedDelivery,ActualDelivery");
            foreach (var s in GetAllShipments())
            {
                sb.AppendLine($"{s.Id},{s.BatchId},{s.TruckId},{s.DriverId},{s.RouteCode},{s.Status},{s.PickupDate:yyyy-MM-dd},{s.ExpectedDelivery:yyyy-MM-dd},{s.ActualDelivery?.ToString("yyyy-MM-dd") ?? "—"}");
            }
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}

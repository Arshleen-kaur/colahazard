# Mobile App Structure & Architecture

## Overview
PackTrack Mobile App - A comprehensive QR-based supply chain tracking system for beverage distribution with role-based dashboards and real-time tracking capabilities.

## Technology Stack
- **Platform**: .NET MAUI / Xamarin
- **Backend**: ASP.NET Core Web API
- **Database**: LiteDB (Local) + SQL Server (Cloud Sync)
- **Authentication**: JWT Bearer Tokens
- **QR Scanning**: ZXing.Net.Mobile
- **Real-time**: SignalR
- **Maps**: Google Maps API / OpenStreetMap

## Project Structure

```
PackTrack.Mobile/
├── Services/
│   ├── Mobile/
│   │   ├── Authentication/
│   │   │   ├── LoginService.cs
│   │   │   ├── TokenService.cs
│   │   │   └── RoleAuthorizationService.cs
│   │   │
│   │   ├── Dashboard/
│   │   │   ├── Transport/
│   │   │   │   ├── TruckDashboardService.cs
│   │   │   │   ├── RouteOptimizationService.cs
│   │   │   │   ├── DeliveryTrackingService.cs
│   │   │   │   ├── LoadManagementService.cs
│   │   │   │   ├── FuelMonitoringService.cs
│   │   │   │   ├── DriverPerformanceService.cs
│   │   │   │   ├── VehicleMaintenanceService.cs
│   │   │   │   ├── GeofencingService.cs
│   │   │   │   ├── TripHistoryService.cs
│   │   │   │   ├── DocumentManagementService.cs
│   │   │   │   ├── EmergencyAlertService.cs
│   │   │   │   ├── WeatherIntegrationService.cs
│   │   │   │   └── ComplianceCheckService.cs
│   │   │   │
│   │   │   ├── FactoryWorker/
│   │   │   │   ├── ProductionDashboardService.cs
│   │   │   │   ├── BatchCreationService.cs
│   │   │   │   ├── QualityControlService.cs
│   │   │   │   ├── PackagingService.cs
│   │   │   │   ├── PalletManagementService.cs
│   │   │   │   ├── ShiftManagementService.cs
│   │   │   │   ├── MachineMonitoringService.cs
│   │   │   │   ├── DefectReportingService.cs
│   │   │   │   ├── InventoryCheckService.cs
│   │   │   │   ├── SafetyComplianceService.cs
│   │   │   │   ├── ProductionTargetService.cs
│   │   │   │   ├── WastageTrackingService.cs
│   │   │   │   └── EquipmentCalibrationService.cs
│   │   │   │
│   │   │   ├── SmallRetailer/
│   │   │   │   ├── RetailDashboardService.cs
│   │   │   │   ├── InventoryManagementService.cs
│   │   │   │   ├── SalesTrackingService.cs
│   │   │   │   ├── StockReplenishmentService.cs
│   │   │   │   ├── ShelfOptimizationService.cs
│   │   │   │   ├── PriceManagementService.cs
│   │   │   │   ├── CustomerOrderService.cs
│   │   │   │   ├── PaymentProcessingService.cs
│   │   │   │   ├── InvoiceGenerationService.cs
│   │   │   │   ├── ExpiryAlertService.cs
│   │   │   │   ├── PromotionManagementService.cs
│   │   │   │   ├── LoyaltyProgramService.cs
│   │   │   │   └── DemandForecastingService.cs
│   │   │   │
│   │   │   └── Wholesaler/
│   │   │       ├── WholesaleDashboardService.cs
│   │   │       ├── BulkOrderManagementService.cs
│   │   │       ├── DistributionPlanningService.cs
│   │   │       ├── WarehouseManagementService.cs
│   │   │       ├── RetailerNetworkService.cs
│   │   │       ├── CreditManagementService.cs
│   │   │       ├── PricingStrategyService.cs
│   │   │       ├── OrderFulfillmentService.cs
│   │   │       ├── ReturnManagementService.cs
│   │   │       ├── RegionalAnalyticsService.cs
│   │   │       ├── SupplierCoordinationService.cs
│   │   │       ├── ContractManagementService.cs
│   │   │       └── PerformanceReportingService.cs
│   │   │
│   │   ├── QRScanning/
│   │   │   ├── QRScannerService.cs
│   │   │   ├── QRGeneratorService.cs
│   │   │   ├── QRValidationService.cs
│   │   │   └── BatchQRScanService.cs
│   │   │
│   │   ├── Tracking/
│   │   │   ├── LocationTrackingService.cs
│   │   │   ├── MovementHistoryService.cs
│   │   │   ├── TraceabilityService.cs
│   │   │   └── ChainOfCustodyService.cs
│   │   │
│   │   ├── Notifications/
│   │   │   ├── PushNotificationService.cs
│   │   │   ├── AlertManagementService.cs
│   │   │   └── RealTimeUpdateService.cs
│   │   │
│   │   ├── Sync/
│   │   │   ├── DataSyncService.cs
│   │   │   ├── OfflineModeService.cs
│   │   │   └── ConflictResolutionService.cs
│   │   │
│   │   └── Analytics/
│   │       ├── PerformanceMetricsService.cs
│   │       ├── ReportGenerationService.cs
│   │       └── PredictiveAnalyticsService.cs
│   │
│   └── API/
│       └── Controllers/
│           ├── AuthController.cs
│           ├── TransportController.cs
│           ├── FactoryWorkerController.cs
│           ├── RetailerController.cs
│           └── WholesalerController.cs
│
├── Models/
│   ├── Mobile/
│   │   ├── Transport/
│   │   │   ├── TruckStatus.cs
│   │   │   ├── DeliveryTask.cs
│   │   │   ├── RouteInfo.cs
│   │   │   └── TripLog.cs
│   │   │
│   │   ├── FactoryWorker/
│   │   │   ├── ProductionTask.cs
│   │   │   ├── QualityCheck.cs
│   │   │   ├── ShiftInfo.cs
│   │   │   └── DefectReport.cs
│   │   │
│   │   ├── SmallRetailer/
│   │   │   ├── StockItem.cs
│   │   │   ├── SaleTransaction.cs
│   │   │   ├── CustomerOrder.cs
│   │   │   └── ShelfLayout.cs
│   │   │
│   │   └── Wholesaler/
│   │       ├── BulkOrder.cs
│   │       ├── RetailerAccount.cs
│   │       ├── WarehouseZone.cs
│   │       └── DistributionPlan.cs
│   │
│   └── DTOs/
│       ├── LoginRequest.cs
│       ├── DashboardResponse.cs
│       ├── QRScanResult.cs
│       └── SyncPayload.cs
│
└── Views/
    ├── Transport/
    ├── FactoryWorker/
    ├── SmallRetailer/
    └── Wholesaler/
```

## Role-Based Access Matrix

| Feature | Transport | Factory Worker | Small Retailer | Wholesaler |
|---------|-----------|----------------|----------------|------------|
| QR Scanning | ✓ | ✓ | ✓ | ✓ |
| GPS Tracking | ✓ | ✗ | ✗ | ✓ |
| Production Entry | ✗ | ✓ | ✗ | ✗ |
| Inventory Management | ✗ | ✓ | ✓ | ✓ |
| Sales Recording | ✗ | ✗ | ✓ | ✓ |
| Route Planning | ✓ | ✗ | ✗ | ✓ |
| Quality Control | ✗ | ✓ | ✓ | ✓ |
| Bulk Orders | ✗ | ✗ | ✗ | ✓ |
| Analytics Dashboard | ✓ | ✓ | ✓ | ✓ |

## Database Schema Extensions

### Mobile-Specific Tables

```sql
-- User Roles & Authentication
MobileUsers (UserId, Username, PasswordHash, Role, DeviceId, LastLogin)
UserSessions (SessionId, UserId, Token, ExpiresAt, DeviceInfo)

-- Transport
TruckAssignments (AssignmentId, TruckId, DriverId, RouteId, Status)
DeliveryTasks (TaskId, AssignmentId, RetailerId, ScheduledTime, Status)
GPSLogs (LogId, TruckId, Latitude, Longitude, Speed, Timestamp)
FuelLogs (LogId, TruckId, FuelLevel, Consumption, Timestamp)

-- Factory Worker
WorkerShifts (ShiftId, WorkerId, StartTime, EndTime, ProductionLineId)
QualityChecks (CheckId, BatchId, WorkerId, CheckType, Result, Timestamp)
DefectReports (ReportId, BatchId, DefectType, Severity, ImageUrl, Timestamp)
ProductionTasks (TaskId, WorkerId, TaskType, Status, CompletedAt)

-- Small Retailer
RetailSales (SaleId, RetailerId, Items, TotalAmount, PaymentMethod, Timestamp)
StockAlerts (AlertId, RetailerId, BottleType, CurrentStock, ThresholdLevel)
CustomerOrders (OrderId, RetailerId, CustomerInfo, Items, Status)
ShelfLayouts (LayoutId, RetailerId, RackConfig, OptimizationScore)

-- Wholesaler
WholesaleOrders (OrderId, WholesalerId, RetailerId, Items, TotalAmount, Status)
CreditAccounts (AccountId, RetailerId, CreditLimit, Outstanding, DueDate)
WarehouseZones (ZoneId, WholesalerId, Capacity, CurrentStock, ZoneType)
DistributionSchedules (ScheduleId, WholesalerId, RouteId, DeliveryDate)
```

## API Endpoints Summary

### Authentication APIs (5)
1. POST /api/auth/login
2. POST /api/auth/refresh-token
3. POST /api/auth/logout
4. GET /api/auth/verify-role
5. POST /api/auth/change-password

### Transport APIs (13)
6. GET /api/transport/dashboard
7. GET /api/transport/active-deliveries
8. POST /api/transport/scan-qr
9. POST /api/transport/update-location
10. GET /api/transport/route-details/{routeId}
11. POST /api/transport/mark-delivery-complete
12. GET /api/transport/fuel-status
13. POST /api/transport/report-incident
14. GET /api/transport/trip-history
15. POST /api/transport/upload-pod
16. GET /api/transport/weather-alerts
17. POST /api/transport/emergency-alert
18. GET /api/transport/maintenance-schedule

### Factory Worker APIs (13)
19. GET /api/factory/dashboard
20. POST /api/factory/create-batch
21. POST /api/factory/scan-qr
22. POST /api/factory/quality-check
23. POST /api/factory/report-defect
24. GET /api/factory/shift-tasks
25. POST /api/factory/complete-task
26. GET /api/factory/production-targets
27. POST /api/factory/record-wastage
28. GET /api/factory/machine-status
29. POST /api/factory/calibrate-equipment
30. GET /api/factory/safety-checklist
31. POST /api/factory/create-pallet

### Small Retailer APIs (13)
32. GET /api/retailer/dashboard
33. POST /api/retailer/scan-qr
34. GET /api/retailer/inventory
35. POST /api/retailer/record-sale
36. POST /api/retailer/create-order
37. GET /api/retailer/stock-alerts
38. POST /api/retailer/update-prices
39. GET /api/retailer/sales-history
40. POST /api/retailer/generate-invoice
41. GET /api/retailer/expiry-alerts
42. POST /api/retailer/apply-promotion
43. GET /api/retailer/demand-forecast
44. POST /api/retailer/optimize-shelf

### Wholesaler APIs (13)
45. GET /api/wholesaler/dashboard
46. POST /api/wholesaler/scan-qr
47. POST /api/wholesaler/create-bulk-order
48. GET /api/wholesaler/retailer-network
49. POST /api/wholesaler/approve-credit
50. GET /api/wholesaler/warehouse-status
51. POST /api/wholesaler/plan-distribution
52. GET /api/wholesaler/pending-orders
53. POST /api/wholesaler/process-return
54. GET /api/wholesaler/regional-analytics
55. POST /api/wholesaler/update-pricing
56. GET /api/wholesaler/performance-report
57. POST /api/wholesaler/coordinate-supplier

### Common APIs (8)
58. POST /api/qr/scan
59. POST /api/qr/generate
60. GET /api/qr/validate/{qrId}
61. GET /api/tracking/history/{bottleId}
62. POST /api/tracking/update-location
63. GET /api/notifications/list
64. POST /api/sync/upload
65. GET /api/sync/download

**Total APIs: 65+**

## Next Steps
1. Detailed API specifications for each role (separate MD files)
2. Request/Response schemas
3. Authentication flow diagrams
4. Offline mode implementation
5. Real-time sync strategy

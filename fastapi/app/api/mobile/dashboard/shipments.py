# app/api/mobile/dashboard/wholesaler/shipments.py

from fastapi import APIRouter, Depends, Query, Path, Body, UploadFile, File
from fastapi.responses import HTMLResponse
from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date

from app.config.database import get_db
from app.core.dependencies import get_current_user
from app.core.roles import require_role
from app.core.pagination import paginate
from app.schemas.common import SuccessResponse
from app.schemas.wholesaler_shipments import (
    ShipmentCreate,
    ShipmentUpdate,
    ShipmentResponse,
    DriverAssignmentRequest,
    VehicleAssignmentRequest,
    PalletAssignmentRequest,
    DelayReportRequest,
    DamageReportRequest,
    DeliveryProofRequest,
    ShipmentAnalyticsResponse,
    RouteEfficiencyResponse,
    FuelUsageSummaryResponse
)
from app.services.dashboard.wholesaler_service import WholesalerService

router = APIRouter()


# =========================================================
# 🚛 CREATE SHIPMENT
# =========================================================
@router.post("/shipments", response_model=ShipmentResponse)
async def create_shipment(
    payload: ShipmentCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.create_shipment(payload, wholesaler_id=current_user.id)


# =========================================================
# ✏️ UPDATE SHIPMENT
# =========================================================
@router.put("/shipments/{shipment_id}", response_model=ShipmentResponse)
async def update_shipment(
    shipment_id: str,
    payload: ShipmentUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.update_shipment(shipment_id, payload)


# =========================================================
# ❌ CANCEL SHIPMENT
# =========================================================
@router.post("/shipments/{shipment_id}/cancel", response_model=SuccessResponse)
async def cancel_shipment(
    shipment_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.cancel_shipment(shipment_id, reason)
    return {"message": "Shipment cancelled successfully"}


# =========================================================
# 👨‍✈️ ASSIGN DRIVER
# =========================================================
@router.post("/shipments/assign-driver", response_model=SuccessResponse)
async def assign_driver(
    payload: DriverAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.assign_driver(payload.shipment_id, payload.driver_id)
    return {"message": "Driver assigned successfully"}


# =========================================================
# 🚚 ASSIGN VEHICLE
# =========================================================
@router.post("/shipments/assign-vehicle", response_model=SuccessResponse)
async def assign_vehicle(
    payload: VehicleAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.assign_vehicle(payload.shipment_id, payload.vehicle_id)
    return {"message": "Vehicle assigned successfully"}


# =========================================================
# 📦 ADD PALLETS TO SHIPMENT
# =========================================================
@router.post("/shipments/add-pallets", response_model=SuccessResponse)
async def add_pallets(
    payload: PalletAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.add_pallets(payload.shipment_id, payload.pallet_ids)
    return {"message": "Pallets added successfully"}


# =========================================================
# ▶️ START SHIPMENT
# =========================================================
@router.post("/shipments/{shipment_id}/start", response_model=SuccessResponse)
async def start_shipment(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.start_shipment(shipment_id)
    return {"message": "Shipment started successfully"}


# =========================================================
# 🚚 MARK IN TRANSIT
# =========================================================
@router.post("/shipments/{shipment_id}/in-transit", response_model=SuccessResponse)
async def mark_in_transit(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.mark_in_transit(shipment_id)
    return {"message": "Shipment marked as in transit"}


# =========================================================
# 📬 MARK DELIVERED
# =========================================================
@router.post("/shipments/{shipment_id}/delivered", response_model=SuccessResponse)
async def mark_delivered(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.mark_delivered(shipment_id)
    return {"message": "Shipment delivered successfully"}


# =========================================================
# ⏳ REPORT DELAY
# =========================================================
@router.post("/shipments/delay", response_model=SuccessResponse)
async def report_delay(
    payload: DelayReportRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.report_delay(payload)
    return {"message": "Delay reported successfully"}


# =========================================================
# ⚠️ REPORT DAMAGE
# =========================================================
@router.post("/shipments/damage", response_model=SuccessResponse)
async def report_damage(
    payload: DamageReportRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.report_damage(payload)
    return {"message": "Damage reported successfully"}


# =========================================================
# 📸 CAPTURE DELIVERY PROOF
# =========================================================
@router.post("/shipments/proof", response_model=SuccessResponse)
async def capture_delivery_proof(
    payload: DeliveryProofRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.capture_delivery_proof(payload)
    return {"message": "Delivery proof captured successfully"}


# =========================================================
# 📍 LIVE LOCATION TRACKING
# =========================================================
@router.get("/shipments/{shipment_id}/live-location")
async def live_location(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_live_location(shipment_id)


# =========================================================
# 📜 SHIPMENT STATUS HISTORY
# =========================================================
@router.get("/shipments/{shipment_id}/history")
async def shipment_history(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_shipment_history(shipment_id)


# =========================================================
# 📊 SHIPMENT ANALYTICS
# =========================================================
@router.get("/shipments/analytics", response_model=ShipmentAnalyticsResponse)
async def shipment_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_shipment_analytics(current_user.id)


# =========================================================
# 🛣 ROUTE EFFICIENCY
# =========================================================
@router.get("/shipments/route-efficiency", response_model=RouteEfficiencyResponse)
async def route_efficiency(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_route_efficiency(current_user.id)


# =========================================================
# ⛽ FUEL USAGE SUMMARY
# =========================================================
@router.get("/shipments/fuel-summary", response_model=FuelUsageSummaryResponse)
async def fuel_summary(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_fuel_usage_summary(current_user.id)


# =========================================================
# 📤 EXPORT SHIPMENTS
# =========================================================
@router.get("/shipments/export")
async def export_shipments(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.export_shipments(current_user.id, format)


# =========================================================
# 📥 BULK SHIPMENT UPLOAD
# =========================================================
@router.post("/shipments/bulk-upload")
async def bulk_upload_shipments(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.bulk_upload_shipments(file, current_user.id)


# =========================================================
# 🌐 SHIPMENTS DASHBOARD (HTML)
# =========================================================
@router.get("/shipments/dashboard", response_class=HTMLResponse)
async def shipments_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Wholesaler Shipments Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #06b6d4; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #06b6d4;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #0891b2;
        }
    </style>
</head>
<body>

<h1>🚛 Wholesaler Shipments Dashboard</h1>

<div class="card">
    <h2>Shipment Analytics</h2>
    <button onclick="alert('GET /shipments/analytics')">View Analytics</button>
</div>

<div class="card">
    <h2>Route Efficiency</h2>
    <button onclick="alert('GET /shipments/route-efficiency')">View Efficiency</button>
</div>

<div class="card">
    <h2>Fuel Summary</h2>
    <button onclick="alert('GET /shipments/fuel-summary')">View Fuel Usage</button>
</div>

<script>
    console.log("Wholesaler shipments dashboard loaded.");
</script>

</body>
</html>
"""
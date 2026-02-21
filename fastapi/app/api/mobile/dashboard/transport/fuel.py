from fastapi import APIRouter, Depends, Query, Path, Body, UploadFile, File
from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date

from app.config.database import get_db
from app.core.dependencies import get_current_user
from app.core.roles import require_role
from app.core.pagination import paginate
from app.schemas.common import SuccessResponse
from app.schemas.fuel import (
    FuelLogCreate,
    FuelLogUpdate,
    FuelLogResponse,
    FuelStatsResponse,
    FuelEfficiencyResponse,
    FuelAnomalyResponse,
    FuelApprovalRequest
)
from app.services.dashboard.transport_service import TransportService

router = APIRouter()


# ---------------------------------------------------------
# ⛽ CREATE FUEL LOG
# ---------------------------------------------------------
@router.post("/fuel/log", response_model=FuelLogResponse)
async def create_fuel_log(
    payload: FuelLogCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.create_fuel_log(payload, current_user.id)


# ---------------------------------------------------------
# ✏️ UPDATE FUEL LOG
# ---------------------------------------------------------
@router.put("/fuel/{fuel_id}", response_model=FuelLogResponse)
async def update_fuel_log(
    fuel_id: str = Path(...),
    payload: FuelLogUpdate = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.update_fuel_log(fuel_id, payload)


# ---------------------------------------------------------
# 📜 GET DRIVER FUEL HISTORY
# ---------------------------------------------------------
@router.get("/fuel/history", response_model=List[FuelLogResponse])
async def get_fuel_history(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)

    logs = service.get_driver_fuel_logs(
        driver_id=current_user.id,
        start_date=start_date,
        end_date=end_date
    )

    return paginate(logs, page, limit)


# ---------------------------------------------------------
# 🚛 TRIP-WISE FUEL REPORT
# ---------------------------------------------------------
@router.get("/fuel/trip/{shipment_id}", response_model=List[FuelLogResponse])
async def fuel_by_trip(
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.get_fuel_by_shipment(shipment_id)


# ---------------------------------------------------------
# 📊 DRIVER FUEL STATISTICS
# ---------------------------------------------------------
@router.get("/fuel/stats", response_model=FuelStatsResponse)
async def driver_fuel_stats(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.calculate_driver_fuel_stats(current_user.id)


# ---------------------------------------------------------
# 📈 FUEL EFFICIENCY ANALYSIS
# ---------------------------------------------------------
@router.get("/fuel/efficiency", response_model=FuelEfficiencyResponse)
async def fuel_efficiency(
    vehicle_id: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.calculate_fuel_efficiency(vehicle_id)


# ---------------------------------------------------------
# ⚠️ FUEL ANOMALY DETECTION
# ---------------------------------------------------------
@router.get("/fuel/anomalies", response_model=List[FuelAnomalyResponse])
async def detect_fuel_anomalies(
    threshold: float = Query(15.0),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.detect_fuel_anomalies(
        driver_id=current_user.id,
        threshold=threshold
    )


# ---------------------------------------------------------
# 📤 BULK FUEL UPLOAD (CSV)
# ---------------------------------------------------------
@router.post("/fuel/bulk-upload")
async def bulk_upload_fuel_logs(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.bulk_upload_fuel(file, current_user.id)


# ---------------------------------------------------------
# 🧾 FUEL EXPENSE REPORT
# ---------------------------------------------------------
@router.get("/fuel/report")
async def fuel_expense_report(
    start_date: date,
    end_date: date,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.generate_fuel_report(
        driver_id=current_user.id,
        start_date=start_date,
        end_date=end_date
    )


# ---------------------------------------------------------
# 🔄 FUEL APPROVAL REQUEST
# ---------------------------------------------------------
@router.post("/fuel/approve", response_model=SuccessResponse)
async def approve_fuel_log(
    payload: FuelApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport", "admin"])
    service = TransportService(db)
    service.approve_fuel_log(payload.fuel_id, current_user.id)
    return {"message": "Fuel log approved successfully"}


# ---------------------------------------------------------
# 📡 IOT FUEL SYNC
# ---------------------------------------------------------
@router.post("/fuel/iot-sync")
async def fuel_iot_sync(
    vehicle_id: str,
    fuel_level: float,
    timestamp: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.sync_iot_fuel_data(
        vehicle_id=vehicle_id,
        fuel_level=fuel_level,
        timestamp=timestamp
    )


# ---------------------------------------------------------
# 📊 DASHBOARD SUMMARY
# ---------------------------------------------------------
@router.get("/fuel/dashboard-summary")
async def fuel_dashboard_summary(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.get_fuel_dashboard_summary(current_user.id)


# ---------------------------------------------------------
# 📤 EXPORT FUEL DATA
# ---------------------------------------------------------
@router.get("/fuel/export")
async def export_fuel_data(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])
    service = TransportService(db)
    return service.export_fuel_logs(current_user.id, format)
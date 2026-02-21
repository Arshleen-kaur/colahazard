# app/api/mobile/dashboard/factory/batches.py

from fastapi import APIRouter, Depends, Query, Path, Body
from fastapi.responses import HTMLResponse
from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date

from app.config.database import get_db
from app.core.dependencies import get_current_user
from app.core.roles import require_role
from app.core.pagination import paginate
from app.schemas.batch import (
    BatchCreate,
    BatchUpdate,
    BatchResponse,
    BatchStatusUpdate,
    QualityCheckRequest,
    DefectReportRequest
)
from app.schemas.common import SuccessResponse
from app.services.dashboard.factory_service import FactoryService

router = APIRouter()

# =========================================================
# 🏭 CREATE BATCH
# =========================================================
@router.post("/batches", response_model=BatchResponse)
async def create_batch(
    payload: BatchCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.create_batch(payload)


# =========================================================
# 📦 GET ALL BATCHES
# =========================================================
@router.get("/batches", response_model=List[BatchResponse])
async def get_batches(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    status: Optional[str] = None,
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)

    batches = service.get_batches(
        status=status,
        start_date=start_date,
        end_date=end_date
    )

    return paginate(batches, page, limit)


# =========================================================
# 🔍 GET BATCH DETAILS
# =========================================================
@router.get("/batch/{batch_id}", response_model=BatchResponse)
async def get_batch(
    batch_id: str = Path(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.get_batch_by_id(batch_id)


# =========================================================
# ✏️ UPDATE BATCH
# =========================================================
@router.put("/batch/{batch_id}", response_model=BatchResponse)
async def update_batch(
    batch_id: str,
    payload: BatchUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.update_batch(batch_id, payload)


# =========================================================
# 🔄 UPDATE BATCH STATUS
# =========================================================
@router.patch("/batch/status", response_model=SuccessResponse)
async def update_batch_status(
    payload: BatchStatusUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    service.update_batch_status(payload.batch_id, payload.status)
    return {"message": "Batch status updated successfully"}


# =========================================================
# ❌ CLOSE BATCH
# =========================================================
@router.post("/batch/{batch_id}/close", response_model=SuccessResponse)
async def close_batch(
    batch_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    service.close_batch(batch_id)
    return {"message": "Batch closed successfully"}


# =========================================================
# 📷 GENERATE QR FOR BATCH
# =========================================================
@router.get("/batch/{batch_id}/generate-qr")
async def generate_batch_qr(
    batch_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.generate_batch_qr(batch_id)


# =========================================================
# 🧪 QUALITY CHECK
# =========================================================
@router.post("/batch/{batch_id}/quality-check", response_model=SuccessResponse)
async def quality_check(
    batch_id: str,
    payload: QualityCheckRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    service.perform_quality_check(batch_id, payload)
    return {"message": "Quality check recorded"}


# =========================================================
# ⚠️ REPORT DEFECT
# =========================================================
@router.post("/batch/{batch_id}/defect", response_model=SuccessResponse)
async def report_defect(
    batch_id: str,
    payload: DefectReportRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    service.report_defect(batch_id, payload)
    return {"message": "Defect recorded"}


# =========================================================
# 📊 BATCH ANALYTICS
# =========================================================
@router.get("/batch-analytics")
async def batch_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.get_batch_analytics()


# =========================================================
# 📈 SHIFT REPORT
# =========================================================
@router.get("/shift-report")
async def shift_report(
    shift: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.get_shift_report(shift)


# =========================================================
# 🛠 MAINTENANCE LOG
# =========================================================
@router.get("/maintenance-log")
async def maintenance_log(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.get_maintenance_logs()


# =========================================================
# 📤 EXPORT BATCH DATA
# =========================================================
@router.get("/batches/export")
async def export_batches(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["factory"])
    service = FactoryService(db)
    return service.export_batches(format)


# =========================================================
# 🌐 SIMPLE HTML DASHBOARD (Embedded)
# =========================================================
@router.get("/dashboard", response_class=HTMLResponse)
async def factory_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Factory Batch Dashboard</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background: #111;
            color: #fff;
            padding: 40px;
        }
        h1 {
            color: #00ffcc;
        }
        .card {
            background: #1c1c1c;
            padding: 20px;
            margin: 20px 0;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,255,204,0.2);
        }
        button {
            background: #00ffcc;
            border: none;
            padding: 10px 20px;
            cursor: pointer;
            border-radius: 5px;
            font-weight: bold;
        }
        button:hover {
            background: #00ccaa;
        }
    </style>
</head>
<body>

<h1>🏭 Factory Batch Dashboard</h1>

<div class="card">
    <h2>Create Batch</h2>
    <button onclick="alert('Call POST /batches API')">Create New Batch</button>
</div>

<div class="card">
    <h2>View Batches</h2>
    <button onclick="alert('Call GET /batches API')">Load Batches</button>
</div>

<div class="card">
    <h2>Generate QR</h2>
    <button onclick="alert('Call /generate-qr endpoint')">Generate QR Code</button>
</div>

<div class="card">
    <h2>Analytics</h2>
    <button onclick="alert('Call /batch-analytics')">View Analytics</button>
</div>

<script>
    console.log("Factory Dashboard Loaded");
</script>

</body>
</html>
"""
# app/api/mobile/dashboard/retailer/returns.py

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
from app.schemas.returns import (
    ReturnCreate,
    ReturnUpdate,
    ReturnResponse,
    ReturnApprovalRequest,
    RefundProcessRequest,
    ReplacementRequest,
    ReturnAnalyticsResponse,
    ReturnReasonStats
)
from app.services.dashboard.retailer_service import RetailerService

router = APIRouter()


# =========================================================
# 🔁 CREATE RETURN REQUEST
# =========================================================
@router.post("/returns", response_model=ReturnResponse)
async def create_return(
    payload: ReturnCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.create_return(payload, retailer_id=current_user.id)


# =========================================================
# ✏️ UPDATE RETURN
# =========================================================
@router.put("/returns/{return_id}", response_model=ReturnResponse)
async def update_return(
    return_id: str,
    payload: ReturnUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.update_return(return_id, payload)


# =========================================================
# ❌ CANCEL RETURN
# =========================================================
@router.post("/returns/{return_id}/cancel", response_model=SuccessResponse)
async def cancel_return(
    return_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.cancel_return(return_id, reason)
    return {"message": "Return cancelled successfully"}


# =========================================================
# 🔍 GET RETURN DETAILS
# =========================================================
@router.get("/returns/{return_id}", response_model=ReturnResponse)
async def get_return(
    return_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_return_by_id(return_id)


# =========================================================
# 📋 LIST RETURNS
# =========================================================
@router.get("/returns", response_model=List[ReturnResponse])
async def list_returns(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    status: Optional[str] = None,
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    reason: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)

    returns = service.get_returns(
        retailer_id=current_user.id,
        status=status,
        start_date=start_date,
        end_date=end_date,
        reason=reason
    )

    return paginate(returns, page, limit)


# =========================================================
# ✅ APPROVE RETURN
# =========================================================
@router.post("/returns/approve", response_model=SuccessResponse)
async def approve_return(
    payload: ReturnApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer", "admin"])
    service = RetailerService(db)
    service.approve_return(payload.return_id)
    return {"message": "Return approved successfully"}


# =========================================================
# ❌ REJECT RETURN
# =========================================================
@router.post("/returns/reject", response_model=SuccessResponse)
async def reject_return(
    payload: ReturnApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer", "admin"])
    service = RetailerService(db)
    service.reject_return(payload.return_id)
    return {"message": "Return rejected successfully"}


# =========================================================
# 💰 PROCESS REFUND
# =========================================================
@router.post("/returns/refund", response_model=SuccessResponse)
async def process_refund(
    payload: RefundProcessRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.process_return_refund(payload)
    return {"message": "Refund processed successfully"}


# =========================================================
# 🔄 REPLACE PRODUCT
# =========================================================
@router.post("/returns/replace", response_model=SuccessResponse)
async def replace_product(
    payload: ReplacementRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.process_replacement(payload)
    return {"message": "Replacement processed successfully"}


# =========================================================
# 📦 RESTOCK INVENTORY AFTER RETURN
# =========================================================
@router.post("/returns/{return_id}/restock", response_model=SuccessResponse)
async def restock_inventory(
    return_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.restock_inventory(return_id)
    return {"message": "Inventory restocked successfully"}


# =========================================================
# 📊 RETURN ANALYTICS
# =========================================================
@router.get("/returns/analytics", response_model=ReturnAnalyticsResponse)
async def return_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_return_analytics(current_user.id)


# =========================================================
# 📈 RETURN REASON STATISTICS
# =========================================================
@router.get("/returns/reason-stats", response_model=ReturnReasonStats)
async def return_reason_stats(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_return_reason_stats(current_user.id)


# =========================================================
# 👤 CUSTOMER RETURN HISTORY
# =========================================================
@router.get("/returns/customer/{customer_id}")
async def customer_return_history(
    customer_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_customer_return_history(customer_id)


# =========================================================
# 📤 EXPORT RETURN DATA
# =========================================================
@router.get("/returns/export")
async def export_returns(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.export_returns(current_user.id, format)


# =========================================================
# 📥 BULK RETURN UPLOAD
# =========================================================
@router.post("/returns/bulk-upload")
async def bulk_upload_returns(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.bulk_upload_returns(file, current_user.id)


# =========================================================
# 🌐 RETURNS DASHBOARD (HTML)
# =========================================================
@router.get("/returns/dashboard", response_class=HTMLResponse)
async def returns_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Retailer Returns Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #ef4444; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #ef4444;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #dc2626;
        }
    </style>
</head>
<body>

<h1>🔁 Retailer Returns Dashboard</h1>

<div class="card">
    <h2>Return Analytics</h2>
    <button onclick="alert('GET /returns/analytics')">View Analytics</button>
</div>

<div class="card">
    <h2>Reason Breakdown</h2>
    <button onclick="alert('GET /returns/reason-stats')">View Stats</button>
</div>

<div class="card">
    <h2>Customer History</h2>
    <button onclick="alert('GET /returns/customer/{id}')">Check History</button>
</div>

<script>
    console.log("Returns dashboard loaded.");
</script>

</body>
</html>
"""
# app/api/mobile/dashboard/wholesaler/network.py

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
from app.schemas.wholesaler_network import (
    RetailerCreate,
    RetailerUpdate,
    RetailerResponse,
    RetailerApprovalRequest,
    TerritoryAssignmentRequest,
    SalesManagerAssignmentRequest,
    CreditLimitRequest,
    NetworkAnalyticsResponse,
    RetailerPerformanceResponse,
    NetworkGrowthResponse
)
from app.services.dashboard.wholesaler_service import WholesalerService

router = APIRouter()


# =========================================================
# 🏪 ADD RETAILER TO NETWORK
# =========================================================
@router.post("/network/retailers", response_model=RetailerResponse)
async def add_retailer(
    payload: RetailerCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.add_retailer(payload, wholesaler_id=current_user.id)


# =========================================================
# ✏️ UPDATE RETAILER
# =========================================================
@router.put("/network/retailers/{retailer_id}", response_model=RetailerResponse)
async def update_retailer(
    retailer_id: str,
    payload: RetailerUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.update_retailer(retailer_id, payload)


# =========================================================
# ❌ REMOVE RETAILER
# =========================================================
@router.delete("/network/retailers/{retailer_id}", response_model=SuccessResponse)
async def remove_retailer(
    retailer_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler", "admin"])
    service = WholesalerService(db)
    service.remove_retailer(retailer_id)
    return {"message": "Retailer removed from network successfully"}


# =========================================================
# ✅ APPROVE RETAILER
# =========================================================
@router.post("/network/retailers/approve", response_model=SuccessResponse)
async def approve_retailer(
    payload: RetailerApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler", "admin"])
    service = WholesalerService(db)
    service.approve_retailer(payload.retailer_id)
    return {"message": "Retailer approved successfully"}


# =========================================================
# 🚫 SUSPEND RETAILER
# =========================================================
@router.post("/network/retailers/{retailer_id}/suspend", response_model=SuccessResponse)
async def suspend_retailer(
    retailer_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.suspend_retailer(retailer_id, reason)
    return {"message": "Retailer suspended successfully"}


# =========================================================
# 🌍 ASSIGN TERRITORY
# =========================================================
@router.post("/network/assign-territory", response_model=SuccessResponse)
async def assign_territory(
    payload: TerritoryAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.assign_territory(payload)
    return {"message": "Territory assigned successfully"}


# =========================================================
# 👔 ASSIGN SALES MANAGER
# =========================================================
@router.post("/network/assign-sales-manager", response_model=SuccessResponse)
async def assign_sales_manager(
    payload: SalesManagerAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.assign_sales_manager(payload)
    return {"message": "Sales manager assigned successfully"}


# =========================================================
# 📋 LIST NETWORK RETAILERS
# =========================================================
@router.get("/network/retailers", response_model=List[RetailerResponse])
async def list_retailers(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    region: Optional[str] = None,
    status: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)

    retailers = service.get_retailers(
        wholesaler_id=current_user.id,
        region=region,
        status=status
    )

    return paginate(retailers, page, limit)


# =========================================================
# 📊 RETAILER PERFORMANCE
# =========================================================
@router.get("/network/retailer-performance/{retailer_id}", response_model=RetailerPerformanceResponse)
async def retailer_performance(
    retailer_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_retailer_performance(retailer_id)


# =========================================================
# 💳 SET CREDIT LIMIT
# =========================================================
@router.post("/network/credit-limit", response_model=SuccessResponse)
async def set_credit_limit(
    payload: CreditLimitRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.set_credit_limit(payload)
    return {"message": "Credit limit updated successfully"}


# =========================================================
# 💰 OUTSTANDING BALANCE
# =========================================================
@router.get("/network/outstanding-balances")
async def outstanding_balances(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_outstanding_balances(current_user.id)


# =========================================================
# 📢 BROADCAST MESSAGE TO NETWORK
# =========================================================
@router.post("/network/broadcast", response_model=SuccessResponse)
async def broadcast_message(
    message: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.broadcast_message(current_user.id, message)
    return {"message": "Broadcast sent successfully"}


# =========================================================
# 📈 NETWORK ANALYTICS
# =========================================================
@router.get("/network/analytics", response_model=NetworkAnalyticsResponse)
async def network_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_network_analytics(current_user.id)


# =========================================================
# 📊 NETWORK GROWTH STATS
# =========================================================
@router.get("/network/growth", response_model=NetworkGrowthResponse)
async def network_growth(
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_network_growth(current_user.id, start_date, end_date)


# =========================================================
# 📤 EXPORT NETWORK DATA
# =========================================================
@router.get("/network/export")
async def export_network(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.export_network(current_user.id, format)


# =========================================================
# 📥 BULK NETWORK UPLOAD
# =========================================================
@router.post("/network/bulk-upload")
async def bulk_upload_network(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.bulk_upload_network(file, current_user.id)


# =========================================================
# 🌐 NETWORK DASHBOARD (HTML)
# =========================================================
@router.get("/network/dashboard", response_class=HTMLResponse)
async def network_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Wholesaler Network Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #f97316; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #f97316;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #ea580c;
        }
    </style>
</head>
<body>

<h1>🌍 Wholesaler Network Dashboard</h1>

<div class="card">
    <h2>Network Analytics</h2>
    <button onclick="alert('GET /network/analytics')">View Analytics</button>
</div>

<div class="card">
    <h2>Outstanding Balances</h2>
    <button onclick="alert('GET /network/outstanding-balances')">View Balances</button>
</div>

<div class="card">
    <h2>Growth Stats</h2>
    <button onclick="alert('GET /network/growth')">View Growth</button>
</div>

<script>
    console.log("Wholesaler network dashboard loaded.");
</script>

</body>
</html>
"""
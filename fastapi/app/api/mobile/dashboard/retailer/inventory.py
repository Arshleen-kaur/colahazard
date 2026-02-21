# app/api/mobile/dashboard/retailer/inventory.py

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
from app.schemas.inventory import (
    InventoryCreate,
    InventoryUpdate,
    InventoryResponse,
    StockAdjustmentRequest,
    StockTransferRequest,
    ReorderSuggestionResponse,
    InventoryAnalyticsResponse
)
from app.services.dashboard.retailer_service import RetailerService

router = APIRouter()


# =========================================================
# 📦 ADD INVENTORY ITEM
# =========================================================
@router.post("/inventory", response_model=InventoryResponse)
async def add_inventory(
    payload: InventoryCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.add_inventory(payload, retailer_id=current_user.id)


# =========================================================
# ✏️ UPDATE INVENTORY ITEM
# =========================================================
@router.put("/inventory/{inventory_id}", response_model=InventoryResponse)
async def update_inventory(
    inventory_id: str,
    payload: InventoryUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.update_inventory(inventory_id, payload)


# =========================================================
# ❌ DELETE INVENTORY ITEM
# =========================================================
@router.delete("/inventory/{inventory_id}", response_model=SuccessResponse)
async def delete_inventory(
    inventory_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer", "admin"])
    service = RetailerService(db)
    service.delete_inventory(inventory_id)
    return {"message": "Inventory item deleted successfully"}


# =========================================================
# 📋 LIST INVENTORY
# =========================================================
@router.get("/inventory", response_model=List[InventoryResponse])
async def list_inventory(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    category: Optional[str] = None,
    low_stock: Optional[bool] = None,
    expiry_before: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)

    items = service.get_inventory(
        retailer_id=current_user.id,
        category=category,
        low_stock=low_stock,
        expiry_before=expiry_before
    )

    return paginate(items, page, limit)


# =========================================================
# 🔍 GET INVENTORY DETAILS
# =========================================================
@router.get("/inventory/{inventory_id}", response_model=InventoryResponse)
async def get_inventory(
    inventory_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_inventory_by_id(inventory_id)


# =========================================================
# ⚠️ LOW STOCK ALERTS
# =========================================================
@router.get("/inventory/alerts/low-stock")
async def low_stock_alerts(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_low_stock_alerts(current_user.id)


# =========================================================
# 🗓 EXPIRY TRACKING
# =========================================================
@router.get("/inventory/alerts/expiry")
async def expiry_alerts(
    days: int = Query(7),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_expiry_alerts(current_user.id, days)


# =========================================================
# 📷 SCAN PRODUCT QR
# =========================================================
@router.post("/inventory/scan")
async def scan_product(
    qr_code: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.scan_product(qr_code, current_user.id)


# =========================================================
# 🔄 STOCK ADJUSTMENT
# =========================================================
@router.post("/inventory/adjust", response_model=SuccessResponse)
async def adjust_stock(
    payload: StockAdjustmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.adjust_stock(payload)
    return {"message": "Stock adjusted successfully"}


# =========================================================
# 🔁 STOCK TRANSFER
# =========================================================
@router.post("/inventory/transfer", response_model=SuccessResponse)
async def transfer_stock(
    payload: StockTransferRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.transfer_stock(payload)
    return {"message": "Stock transferred successfully"}


# =========================================================
# 📊 INVENTORY ANALYTICS
# =========================================================
@router.get("/inventory/analytics", response_model=InventoryAnalyticsResponse)
async def inventory_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_inventory_analytics(current_user.id)


# =========================================================
# 🔮 REORDER SUGGESTIONS
# =========================================================
@router.get("/inventory/reorder", response_model=List[ReorderSuggestionResponse])
async def reorder_suggestions(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_reorder_suggestions(current_user.id)


# =========================================================
# 📤 EXPORT INVENTORY DATA
# =========================================================
@router.get("/inventory/export")
async def export_inventory(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.export_inventory(current_user.id, format)


# =========================================================
# 📥 BULK UPLOAD INVENTORY
# =========================================================
@router.post("/inventory/bulk-upload")
async def bulk_upload_inventory(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.bulk_upload_inventory(file, current_user.id)


# =========================================================
# 🌐 RETAILER INVENTORY DASHBOARD (HTML)
# =========================================================
@router.get("/inventory/dashboard", response_class=HTMLResponse)
async def inventory_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Retailer Inventory Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #22c55e; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #22c55e;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #16a34a;
        }
    </style>
</head>
<body>

<h1>🏪 Retailer Inventory Dashboard</h1>

<div class="card">
    <h2>Low Stock Alerts</h2>
    <button onclick="alert('GET /inventory/alerts/low-stock')">Check Alerts</button>
</div>

<div class="card">
    <h2>Expiry Alerts</h2>
    <button onclick="alert('GET /inventory/alerts/expiry')">Check Expiry</button>
</div>

<div class="card">
    <h2>Reorder Suggestions</h2>
    <button onclick="alert('GET /inventory/reorder')">View Suggestions</button>
</div>

<div class="card">
    <h2>Analytics</h2>
    <button onclick="alert('GET /inventory/analytics')">View Analytics</button>
</div>

<script>
    console.log("Retailer inventory dashboard loaded.");
</script>

</body>
</html>
"""
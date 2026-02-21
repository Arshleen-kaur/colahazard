# app/api/mobile/dashboard/wholesaler/orders.py

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
from app.schemas.wholesaler_orders import (
    OrderCreate,
    OrderUpdate,
    OrderResponse,
    OrderApprovalRequest,
    TruckAssignmentRequest,
    PartialFulfillmentRequest,
    PaymentTrackingRequest,
    OrderAnalyticsResponse,
    RevenueSummaryResponse,
    TopRetailerResponse
)
from app.services.dashboard.wholesaler_service import WholesalerService

router = APIRouter()


# =========================================================
# 📦 CREATE BULK ORDER
# =========================================================
@router.post("/orders", response_model=OrderResponse)
async def create_order(
    payload: OrderCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.create_order(payload, wholesaler_id=current_user.id)


# =========================================================
# ✏️ UPDATE ORDER
# =========================================================
@router.put("/orders/{order_id}", response_model=OrderResponse)
async def update_order(
    order_id: str,
    payload: OrderUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.update_order(order_id, payload)


# =========================================================
# ❌ CANCEL ORDER
# =========================================================
@router.post("/orders/{order_id}/cancel", response_model=SuccessResponse)
async def cancel_order(
    order_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.cancel_order(order_id, reason)
    return {"message": "Order cancelled successfully"}


# =========================================================
# ✅ APPROVE ORDER
# =========================================================
@router.post("/orders/approve", response_model=SuccessResponse)
async def approve_order(
    payload: OrderApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler", "admin"])
    service = WholesalerService(db)
    service.approve_order(payload.order_id)
    return {"message": "Order approved successfully"}


# =========================================================
# ❌ REJECT ORDER
# =========================================================
@router.post("/orders/reject", response_model=SuccessResponse)
async def reject_order(
    payload: OrderApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler", "admin"])
    service = WholesalerService(db)
    service.reject_order(payload.order_id)
    return {"message": "Order rejected successfully"}


# =========================================================
# 🚛 ASSIGN TRUCK TO ORDER
# =========================================================
@router.post("/orders/assign-truck", response_model=SuccessResponse)
async def assign_truck(
    payload: TruckAssignmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.assign_truck(payload.order_id, payload.truck_id)
    return {"message": "Truck assigned successfully"}


# =========================================================
# 📦 PARTIAL FULFILLMENT
# =========================================================
@router.post("/orders/partial-fulfillment", response_model=SuccessResponse)
async def partial_fulfillment(
    payload: PartialFulfillmentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.partial_fulfill_order(payload)
    return {"message": "Partial fulfillment recorded"}


# =========================================================
# 🔄 UPDATE ORDER STATUS
# =========================================================
@router.patch("/orders/{order_id}/status", response_model=SuccessResponse)
async def update_order_status(
    order_id: str,
    status: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.update_order_status(order_id, status)
    return {"message": "Order status updated successfully"}


# =========================================================
# 📊 LIST ORDERS
# =========================================================
@router.get("/orders", response_model=List[OrderResponse])
async def list_orders(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    status: Optional[str] = None,
    retailer_id: Optional[str] = None,
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)

    orders = service.get_orders(
        wholesaler_id=current_user.id,
        status=status,
        retailer_id=retailer_id,
        start_date=start_date,
        end_date=end_date
    )

    return paginate(orders, page, limit)


# =========================================================
# 🔍 GET ORDER DETAILS
# =========================================================
@router.get("/orders/{order_id}", response_model=OrderResponse)
async def get_order(
    order_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_order_by_id(order_id)


# =========================================================
# 💰 PAYMENT TRACKING
# =========================================================
@router.post("/orders/payment", response_model=SuccessResponse)
async def track_payment(
    payload: PaymentTrackingRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.track_payment(payload)
    return {"message": "Payment updated successfully"}


# =========================================================
# 📈 ORDER ANALYTICS
# =========================================================
@router.get("/orders/analytics", response_model=OrderAnalyticsResponse)
async def order_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_order_analytics(current_user.id)


# =========================================================
# 📊 REVENUE SUMMARY
# =========================================================
@router.get("/orders/revenue-summary", response_model=RevenueSummaryResponse)
async def revenue_summary(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_revenue_summary(current_user.id)


# =========================================================
# 🏆 TOP RETAILERS
# =========================================================
@router.get("/orders/top-retailers", response_model=List[TopRetailerResponse])
async def top_retailers(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.get_top_retailers(current_user.id)


# =========================================================
# 📤 EXPORT ORDERS
# =========================================================
@router.get("/orders/export")
async def export_orders(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.export_orders(current_user.id, format)


# =========================================================
# 📥 BULK ORDER UPLOAD
# =========================================================
@router.post("/orders/bulk-upload")
async def bulk_upload_orders(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.bulk_upload_orders(file, current_user.id)


# =========================================================
# 🌐 WHOLESALER ORDERS DASHBOARD (HTML)
# =========================================================
@router.get("/orders/dashboard", response_class=HTMLResponse)
async def orders_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Wholesaler Orders Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #6366f1; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #6366f1;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #4f46e5;
        }
    </style>
</head>
<body>

<h1>📦 Wholesaler Orders Dashboard</h1>

<div class="card">
    <h2>Order Analytics</h2>
    <button onclick="alert('GET /orders/analytics')">View Analytics</button>
</div>

<div class="card">
    <h2>Revenue Summary</h2>
    <button onclick="alert('GET /orders/revenue-summary')">View Revenue</button>
</div>

<div class="card">
    <h2>Top Retailers</h2>
    <button onclick="alert('GET /orders/top-retailers')">View Top Retailers</button>
</div>

<script>
    console.log("Wholesaler orders dashboard loaded.");
</script>

</body>
</html>
"""
# app/api/mobile/dashboard/retailer/sales.py

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
from app.schemas.sales import (
    SaleCreate,
    SaleUpdate,
    SaleResponse,
    RefundRequest,
    POSQuickSaleRequest,
    SalesAnalyticsResponse,
    PaymentBreakdownResponse,
    TopProductResponse
)
from app.services.dashboard.retailer_service import RetailerService

router = APIRouter()


# =========================================================
# 💳 CREATE SALE
# =========================================================
@router.post("/sales", response_model=SaleResponse)
async def create_sale(
    payload: SaleCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.create_sale(payload, retailer_id=current_user.id)


# =========================================================
# ✏️ UPDATE SALE
# =========================================================
@router.put("/sales/{sale_id}", response_model=SaleResponse)
async def update_sale(
    sale_id: str,
    payload: SaleUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.update_sale(sale_id, payload)


# =========================================================
# ❌ CANCEL SALE
# =========================================================
@router.post("/sales/{sale_id}/cancel", response_model=SuccessResponse)
async def cancel_sale(
    sale_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.cancel_sale(sale_id, reason)
    return {"message": "Sale cancelled successfully"}


# =========================================================
# 🔍 GET SALE DETAILS
# =========================================================
@router.get("/sales/{sale_id}", response_model=SaleResponse)
async def get_sale(
    sale_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_sale_by_id(sale_id)


# =========================================================
# 📋 LIST SALES
# =========================================================
@router.get("/sales", response_model=List[SaleResponse])
async def list_sales(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    payment_method: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)

    sales = service.get_sales(
        retailer_id=current_user.id,
        start_date=start_date,
        end_date=end_date,
        payment_method=payment_method
    )

    return paginate(sales, page, limit)


# =========================================================
# ⚡ POS QUICK SALE
# =========================================================
@router.post("/sales/pos", response_model=SaleResponse)
async def pos_quick_sale(
    payload: POSQuickSaleRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.create_pos_sale(payload, current_user.id)


# =========================================================
# 📷 SCAN SALE VIA QR
# =========================================================
@router.post("/sales/scan")
async def scan_sale(
    qr_code: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.scan_sale_qr(qr_code, current_user.id)


# =========================================================
# 🔄 PROCESS REFUND
# =========================================================
@router.post("/sales/refund", response_model=SuccessResponse)
async def process_refund(
    payload: RefundRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.process_refund(payload)
    return {"message": "Refund processed successfully"}


# =========================================================
# 📊 DAILY SALES SUMMARY
# =========================================================
@router.get("/sales/summary")
async def sales_summary(
    date_filter: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_sales_summary(current_user.id, date_filter)


# =========================================================
# 📈 SALES ANALYTICS
# =========================================================
@router.get("/sales/analytics", response_model=SalesAnalyticsResponse)
async def sales_analytics(
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_sales_analytics(current_user.id, start_date, end_date)


# =========================================================
# 🏆 TOP PRODUCTS
# =========================================================
@router.get("/sales/top-products", response_model=List[TopProductResponse])
async def top_products(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_top_products(current_user.id)


# =========================================================
# 💰 PAYMENT BREAKDOWN
# =========================================================
@router.get("/sales/payment-breakdown", response_model=PaymentBreakdownResponse)
async def payment_breakdown(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_payment_breakdown(current_user.id)


# =========================================================
# 👤 CUSTOMER PURCHASE HISTORY
# =========================================================
@router.get("/sales/customer/{customer_id}")
async def customer_purchase_history(
    customer_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_customer_purchase_history(customer_id)


# =========================================================
# 📤 EXPORT SALES DATA
# =========================================================
@router.get("/sales/export")
async def export_sales(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.export_sales(current_user.id, format)


# =========================================================
# 📥 BULK SALES UPLOAD
# =========================================================
@router.post("/sales/bulk-upload")
async def bulk_upload_sales(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.bulk_upload_sales(file, current_user.id)


# =========================================================
# 🌐 SALES DASHBOARD (HTML)
# =========================================================
@router.get("/sales/dashboard", response_class=HTMLResponse)
async def sales_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Retailer Sales Dashboard</title>
    <style>
        body {
            background: #111827;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #f59e0b; }
        .card {
            background: #1f2937;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #f59e0b;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #d97706;
        }
    </style>
</head>
<body>

<h1>💳 Retailer Sales Dashboard</h1>

<div class="card">
    <h2>Today's Summary</h2>
    <button onclick="alert('GET /sales/summary')">View Summary</button>
</div>

<div class="card">
    <h2>Analytics</h2>
    <button onclick="alert('GET /sales/analytics')">View Analytics</button>
</div>

<div class="card">
    <h2>Top Products</h2>
    <button onclick="alert('GET /sales/top-products')">View Top Products</button>
</div>

<div class="card">
    <h2>Payment Breakdown</h2>
    <button onclick="alert('GET /sales/payment-breakdown')">View Payments</button>
</div>

<script>
    console.log("Retailer sales dashboard loaded.");
</script>

</body>
</html>
"""
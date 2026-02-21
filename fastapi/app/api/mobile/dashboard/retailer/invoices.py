# app/api/mobile/dashboard/retailer/invoices.py

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
from app.schemas.invoices import (
    InvoiceCreate,
    InvoiceUpdate,
    InvoiceResponse,
    InvoiceLineItemRequest,
    InvoicePaymentRequest,
    InvoiceAnalyticsResponse,
    PaymentHistoryResponse
)
from app.services.dashboard.retailer_service import RetailerService

router = APIRouter()


# =========================================================
# 🧾 CREATE INVOICE
# =========================================================
@router.post("/invoices", response_model=InvoiceResponse)
async def create_invoice(
    payload: InvoiceCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.create_invoice(payload, retailer_id=current_user.id)


# =========================================================
# ✏️ UPDATE INVOICE
# =========================================================
@router.put("/invoices/{invoice_id}", response_model=InvoiceResponse)
async def update_invoice(
    invoice_id: str,
    payload: InvoiceUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.update_invoice(invoice_id, payload)


# =========================================================
# 🔍 GET INVOICE DETAILS
# =========================================================
@router.get("/invoices/{invoice_id}", response_model=InvoiceResponse)
async def get_invoice(
    invoice_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_invoice_by_id(invoice_id)


# =========================================================
# 📋 LIST INVOICES
# =========================================================
@router.get("/invoices", response_model=List[InvoiceResponse])
async def list_invoices(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    status: Optional[str] = None,
    start_date: Optional[date] = None,
    end_date: Optional[date] = None,
    customer_id: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)

    invoices = service.get_invoices(
        retailer_id=current_user.id,
        status=status,
        start_date=start_date,
        end_date=end_date,
        customer_id=customer_id
    )

    return paginate(invoices, page, limit)


# =========================================================
# 🧾 GENERATE INVOICE FROM SALE
# =========================================================
@router.post("/invoices/generate-from-sale/{sale_id}", response_model=InvoiceResponse)
async def generate_invoice_from_sale(
    sale_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.generate_invoice_from_sale(sale_id)


# =========================================================
# ➕ ADD LINE ITEM
# =========================================================
@router.post("/invoices/{invoice_id}/add-item", response_model=SuccessResponse)
async def add_line_item(
    invoice_id: str,
    payload: InvoiceLineItemRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.add_invoice_item(invoice_id, payload)
    return {"message": "Line item added successfully"}


# =========================================================
# ➖ REMOVE LINE ITEM
# =========================================================
@router.delete("/invoices/{invoice_id}/remove-item/{item_id}", response_model=SuccessResponse)
async def remove_line_item(
    invoice_id: str,
    item_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.remove_invoice_item(invoice_id, item_id)
    return {"message": "Line item removed successfully"}


# =========================================================
# 💸 APPLY DISCOUNT
# =========================================================
@router.post("/invoices/{invoice_id}/discount", response_model=SuccessResponse)
async def apply_discount(
    invoice_id: str,
    amount: float = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.apply_discount(invoice_id, amount)
    return {"message": "Discount applied successfully"}


# =========================================================
# 🧮 APPLY TAX
# =========================================================
@router.post("/invoices/{invoice_id}/tax", response_model=SuccessResponse)
async def apply_tax(
    invoice_id: str,
    tax_percentage: float = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.apply_tax(invoice_id, tax_percentage)
    return {"message": "Tax applied successfully"}


# =========================================================
# 💰 MARK AS PAID
# =========================================================
@router.post("/invoices/{invoice_id}/mark-paid", response_model=SuccessResponse)
async def mark_invoice_paid(
    invoice_id: str,
    payload: InvoicePaymentRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.mark_invoice_paid(invoice_id, payload)
    return {"message": "Invoice marked as paid"}


# =========================================================
# ❌ CANCEL INVOICE
# =========================================================
@router.post("/invoices/{invoice_id}/cancel", response_model=SuccessResponse)
async def cancel_invoice(
    invoice_id: str,
    reason: str = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.cancel_invoice(invoice_id, reason)
    return {"message": "Invoice cancelled successfully"}


# =========================================================
# 📥 DOWNLOAD INVOICE PDF
# =========================================================
@router.get("/invoices/{invoice_id}/download")
async def download_invoice_pdf(
    invoice_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.download_invoice_pdf(invoice_id)


# =========================================================
# 📧 SEND INVOICE (EMAIL TRIGGER)
# =========================================================
@router.post("/invoices/{invoice_id}/send", response_model=SuccessResponse)
async def send_invoice(
    invoice_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    service.send_invoice(invoice_id)
    return {"message": "Invoice sent successfully"}


# =========================================================
# ⏰ OVERDUE INVOICES
# =========================================================
@router.get("/invoices/overdue")
async def overdue_invoices(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_overdue_invoices(current_user.id)


# =========================================================
# 📊 INVOICE ANALYTICS
# =========================================================
@router.get("/invoices/analytics", response_model=InvoiceAnalyticsResponse)
async def invoice_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_invoice_analytics(current_user.id)


# =========================================================
# 📜 PAYMENT HISTORY
# =========================================================
@router.get("/invoices/{invoice_id}/payments", response_model=List[PaymentHistoryResponse])
async def payment_history(
    invoice_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.get_payment_history(invoice_id)


# =========================================================
# 📤 EXPORT INVOICE DATA
# =========================================================
@router.get("/invoices/export")
async def export_invoices(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.export_invoices(current_user.id, format)


# =========================================================
# 📥 BULK INVOICE UPLOAD
# =========================================================
@router.post("/invoices/bulk-upload")
async def bulk_upload_invoices(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["retailer"])
    service = RetailerService(db)
    return service.bulk_upload_invoices(file, current_user.id)


# =========================================================
# 🌐 INVOICE DASHBOARD (HTML)
# =========================================================
@router.get("/invoices/dashboard", response_class=HTMLResponse)
async def invoice_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Retailer Invoice Dashboard</title>
    <style>
        body {
            background: #111827;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #3b82f6; }
        .card {
            background: #1f2937;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #3b82f6;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #2563eb;
        }
    </style>
</head>
<body>

<h1>🧾 Retailer Invoice Dashboard</h1>

<div class="card">
    <h2>Create Invoice</h2>
    <button onclick="alert('POST /invoices')">Create</button>
</div>

<div class="card">
    <h2>Overdue Invoices</h2>
    <button onclick="alert('GET /invoices/overdue')">Check Overdue</button>
</div>

<div class="card">
    <h2>Invoice Analytics</h2>
    <button onclick="alert('GET /invoices/analytics')">View Analytics</button>
</div>

<script>
    console.log("Invoice dashboard loaded.");
</script>

</body>
</html>
"""
from fastapi import APIRouter, Depends, HTTPException, UploadFile, File
from sqlalchemy.orm import Session
from typing import List
from datetime import datetime

from app.core.dependencies import get_db, get_current_user
from app.core.roles import RoleChecker
from app.schemas.shipment import DeliveryUpdate, DeliveryResponse
from app.services.dashboard.transport_service import TransportService

router = APIRouter()

# Only transport drivers allowed
allow_driver = RoleChecker(["transport_driver"])


@router.post("/delivery/start/{shipment_id}", response_model=DeliveryResponse)
async def start_delivery(
    shipment_id: str,
    db: Session = Depends(get_db),
    user=Depends(get_current_user),
    _: bool = Depends(allow_driver),
):
    """
    Mark shipment as Out for Delivery.
    """
    service = TransportService(db)
    shipment = service.start_delivery(shipment_id, user.id)

    if not shipment:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return shipment


@router.post("/delivery/complete/{shipment_id}", response_model=DeliveryResponse)
async def complete_delivery(
    shipment_id: str,
    payload: DeliveryUpdate,
    db: Session = Depends(get_db),
    user=Depends(get_current_user),
    _: bool = Depends(allow_driver),
):
    """
    Complete delivery with geo-coordinates and timestamp.
    """
    service = TransportService(db)

    updated = service.complete_delivery(
        shipment_id=shipment_id,
        delivered_at=datetime.utcnow(),
        latitude=payload.latitude,
        longitude=payload.longitude,
        receiver_name=payload.receiver_name,
    )

    if not updated:
        raise HTTPException(status_code=400, detail="Delivery failed")

    return updated


@router.post("/delivery/proof/{shipment_id}")
async def upload_delivery_proof(
    shipment_id: str,
    file: UploadFile = File(...),
    db: Session = Depends(get_db),
    user=Depends(get_current_user),
    _: bool = Depends(allow_driver),
):
    """
    Upload signature or image proof of delivery.
    """
    service = TransportService(db)
    proof_url = await service.save_delivery_proof(shipment_id, file)

    return {"message": "Proof uploaded", "proof_url": proof_url}


@router.get("/delivery/status/{shipment_id}")
async def delivery_status(
    shipment_id: str,
    db: Session = Depends(get_db),
    user=Depends(get_current_user),
):
    """
    Get live delivery status for shipment.
    """
    service = TransportService(db)
    status = service.get_delivery_status(shipment_id)

    if not status:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return status 
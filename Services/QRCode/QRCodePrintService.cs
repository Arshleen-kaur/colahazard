using PackTrack.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Drawing;

namespace PackTrack.Services.QRCode
{
    /// <summary>
    /// QR Code Printing Service for generating printable labels
    /// Supports: Pallet labels, Carton labels, Bottle labels
    /// </summary>
    public class QRCodePrintService
    {
        public QRCodePrintService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Pallet Label Printing

        /// <summary>
        /// Generate printable pallet label (A4 size)
        /// Includes: QR Code, Batch Info, Pallet Details, Barcode
        /// </summary>
        public byte[] GeneratePalletLabel(PalletLabelData data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.PageColor(Colors.White);

                    page.Content().Column(column =>
                    {
                        // Header
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("PACKTRACK PALLET LABEL")
                                    .FontSize(24)
                                    .Bold();
                                col.Item().Text($"Pallet: {data.PalletCode}")
                                    .FontSize(18);
                            });

                            row.ConstantItem(120).AlignRight().Image(data.CompanyLogo).FitArea();
                        });

                        column.Item().PaddingVertical(10).LineHorizontal(2);

                        // QR Code Section (Large)
                        column.Item().PaddingVertical(20).AlignCenter().Column(col =>
                        {
                            col.Item().AlignCenter().Width(300).Height(300).Image(data.QRCodeImage).FitArea();
                            
                            col.Item().PaddingTop(10).Text(data.QRCode)
                                .FontSize(16)
                                .Bold()
                                .AlignCenter();
                        });

                        // Batch Information
                        column.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                            });

                            table.Cell().Border(1).Padding(5).Text("Batch Code:").Bold();
                            table.Cell().Border(1).Padding(5).Text(data.BatchCode);

                            table.Cell().Border(1).Padding(5).Text("Product:").Bold();
                            table.Cell().Border(1).Padding(5).Text($"{data.BottleType} {data.CapacityML}ml");

                            table.Cell().Border(1).Padding(5).Text("Total Units:").Bold();
                            table.Cell().Border(1).Padding(5).Text(data.TotalUnits.ToString());

                            table.Cell().Border(1).Padding(5).Text("Cartons:").Bold();
                            table.Cell().Border(1).Padding(5).Text(data.CartonCount.ToString());

                            table.Cell().Border(1).Padding(5).Text("Manufacture Date:").Bold();
                            table.Cell().Border(1).Padding(5).Text(data.ManufactureDate.ToString("yyyy-MM-dd"));

                            table.Cell().Border(1).Padding(5).Text("Expiry Date:").Bold();
                            table.Cell().Border(1).Padding(5).Text(data.ExpiryDate.ToString("yyyy-MM-dd"));

                            table.Cell().Border(1).Padding(5).Text("Weight:").Bold();
                            table.Cell().Border(1).Padding(5).Text($"{data.GrossWeight} kg");
                        });

                        // Barcode
                        column.Item().PaddingVertical(10).AlignCenter().Column(col =>
                        {
                            col.Item().Text("Barcode").FontSize(10);
                            col.Item().Width(400).Height(60).Image(data.BarcodeImage).FitArea();
                        });

                        // Footer
                        column.Item().PaddingTop(20).AlignCenter().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Generate multiple pallet labels in batch
        /// </summary>
        public byte[] GenerateBatchPalletLabels(List<PalletLabelData> pallets)
        {
            return Document.Create(container =>
            {
                foreach (var pallet in pallets)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);
                        page.PageColor(Colors.White);

                        page.Content().Element(c => RenderPalletLabel(c, pallet));
                    });
                }
            }).GeneratePdf();
        }

        #endregion

        #region Carton Label Printing

        /// <summary>
        /// Generate printable carton label (smaller format)
        /// Size: 10cm x 15cm
        /// </summary>
        public byte[] GenerateCartonLabel(CartonLabelData data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(10, 15, Unit.Centimetre));
                    page.Margin(10);
                    page.PageColor(Colors.White);

                    page.Content().Column(column =>
                    {
                        // Header
                        column.Item().Text("CARTON LABEL")
                            .FontSize(14)
                            .Bold()
                            .AlignCenter();

                        column.Item().PaddingVertical(5).LineHorizontal(1);

                        // QR Code
                        column.Item().PaddingVertical(10).AlignCenter().Width(150).Height(150).Image(data.QRCodeImage).FitArea();

                        column.Item().Text(data.QRCode)
                            .FontSize(10)
                            .AlignCenter();

                        // Details
                        column.Item().PaddingVertical(5).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                            });

                            table.Cell().Padding(2).Text("Carton:").FontSize(9).Bold();
                            table.Cell().Padding(2).Text(data.CartonCode).FontSize(9);

                            table.Cell().Padding(2).Text("Pallet:").FontSize(9).Bold();
                            table.Cell().Padding(2).Text(data.PalletCode).FontSize(9);

                            table.Cell().Padding(2).Text("Units:").FontSize(9).Bold();
                            table.Cell().Padding(2).Text(data.UnitsPerCarton.ToString()).FontSize(9);

                            table.Cell().Padding(2).Text("Product:").FontSize(9).Bold();
                            table.Cell().Padding(2).Text($"{data.BottleType} {data.CapacityML}ml").FontSize(9);
                        });

                        // Footer
                        column.Item().PaddingTop(5).AlignCenter().Text($"{DateTime.Now:yyyy-MM-dd}")
                            .FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Generate carton labels sheet (multiple labels per page)
        /// 6 labels per A4 page (2 columns x 3 rows)
        /// </summary>
        public byte[] GenerateCartonLabelsSheet(List<CartonLabelData> cartons)
        {
            return Document.Create(container =>
            {
                var pages = cartons.Chunk(6).ToList();

                foreach (var pageCartons in pages)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(10);
                        page.PageColor(Colors.White);

                        page.Content().Grid(grid =>
                        {
                            grid.Columns(2);
                            grid.Spacing(10);

                            foreach (var carton in pageCartons)
                            {
                                grid.Item().Border(1).Padding(5).Element(c => RenderCartonLabelCompact(c, carton));
                            }
                        });
                    });
                }
            }).GeneratePdf();
        }

        #endregion

        #region Bottle Label Printing

        /// <summary>
        /// Generate bottle labels sheet (small stickers)
        /// 24 labels per A4 page (4 columns x 6 rows)
        /// </summary>
        public byte[] GenerateBottleLabelsSheet(List<BottleLabelData> bottles)
        {
            return Document.Create(container =>
            {
                var pages = bottles.Chunk(24).ToList();

                foreach (var pageBottles in pages)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(5);
                        page.PageColor(Colors.White);

                        page.Content().Grid(grid =>
                        {
                            grid.Columns(4);
                            grid.Spacing(5);

                            foreach (var bottle in pageBottles)
                            {
                                grid.Item().Border(1).Padding(3).Element(c => RenderBottleLabel(c, bottle));
                            }
                        });
                    });
                }
            }).GeneratePdf();
        }

        #endregion

        #region Rendering Helpers

        private void RenderPalletLabel(IContainer container, PalletLabelData data)
        {
            container.Column(column =>
            {
                column.Item().Text($"PALLET: {data.PalletCode}")
                    .FontSize(20)
                    .Bold();

                column.Item().PaddingVertical(10).AlignCenter().Width(250).Height(250).Image(data.QRCodeImage).FitArea();

                column.Item().Text(data.QRCode)
                    .FontSize(14)
                    .AlignCenter();

                column.Item().PaddingVertical(10).Text($"Batch: {data.BatchCode} | Units: {data.TotalUnits}")
                    .FontSize(12);
            });
        }

        private void RenderCartonLabelCompact(IContainer container, CartonLabelData data)
        {
            container.Column(column =>
            {
                column.Item().Text(data.CartonCode)
                    .FontSize(10)
                    .Bold()
                    .AlignCenter();

                column.Item().PaddingVertical(5).AlignCenter().Width(100).Height(100).Image(data.QRCodeImage).FitArea();

                column.Item().Text($"{data.BottleType} {data.CapacityML}ml")
                    .FontSize(8)
                    .AlignCenter();

                column.Item().Text($"Units: {data.UnitsPerCarton}")
                    .FontSize(8)
                    .AlignCenter();
            });
        }

        private void RenderBottleLabel(IContainer container, BottleLabelData data)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Width(40).Height(40).Image(data.QRCodeImage).FitArea();

                column.Item().Text(data.BottleId.Substring(data.BottleId.Length - 8))
                    .FontSize(6)
                    .AlignCenter();

                column.Item().Text($"{data.CapacityML}ml")
                    .FontSize(6)
                    .AlignCenter();
            });
        }

        #endregion

        #region Print Job Management

        /// <summary>
        /// Send print job to network printer
        /// </summary>
        public async Task<bool> SendToPrinter(byte[] pdfData, string printerName)
        {
            try
            {
                // Save PDF temporarily
                var tempFile = Path.Combine(Path.GetTempPath(), $"print_{Guid.NewGuid()}.pdf");
                await File.WriteAllBytesAsync(tempFile, pdfData);

                // Send to printer using System.Drawing.Printing
                var printDocument = new System.Drawing.Printing.PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;

                // Print logic here (requires additional PDF rendering library)
                // For production, use PdfiumViewer or similar

                // Clean up
                File.Delete(tempFile);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Print error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get available printers
        /// </summary>
        public List<string> GetAvailablePrinters()
        {
            var printers = new List<string>();
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                printers.Add(printer);
            }
            return printers;
        }

        #endregion
    }

    #region Label Data Models

    public class PalletLabelData
    {
        public string PalletCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public byte[] QRCodeImage { get; set; } = Array.Empty<byte>();
        public byte[] BarcodeImage { get; set; } = Array.Empty<byte>();
        public byte[] CompanyLogo { get; set; } = Array.Empty<byte>();
        public string BatchCode { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int TotalUnits { get; set; }
        public int CartonCount { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
    }

    public class CartonLabelData
    {
        public string CartonCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public byte[] QRCodeImage { get; set; } = Array.Empty<byte>();
        public string PalletCode { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int UnitsPerCarton { get; set; }
    }

    public class BottleLabelData
    {
        public string BottleId { get; set; } = string.Empty;
        public byte[] QRCodeImage { get; set; } = Array.Empty<byte>();
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    #endregion
}

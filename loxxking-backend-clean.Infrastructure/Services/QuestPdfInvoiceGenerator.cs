using loxxking_backend_clean.Domain.Entities.Invoices;
using loxxking_backend_clean.Domain.Entities.Orders;
using loxxking_backend_clean.Shared.Resources;
using Microsoft.Extensions.Localization;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace loxxking_backend_clean.Infrastructure.Services;

public class QuestPdfInvoiceGenerator : IInvoicePdfGenerator
{
    private readonly IStringLocalizer<SharedResource>? _localizer;

    static QuestPdfInvoiceGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "NotoSansArabic-Regular.ttf");
        if (File.Exists(fontPath))
        {
            FontManager.RegisterFont(File.OpenRead(fontPath));
        }
    }

    public QuestPdfInvoiceGenerator(IStringLocalizer<SharedResource>? localizer = null)
    {
        _localizer = localizer;
    }

    public Task<byte[]> GeneratePdfAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        return Task.FromResult(GeneratePdf(invoice));
    }

    private byte[] GeneratePdf(Invoice invoice)
    {
        var lang = invoice.Order?.Customer?.PreferredLanguage ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        var isArabic = lang.StartsWith("ar", StringComparison.OrdinalIgnoreCase);

        string T(string key, string enFallback, string arFallback)
        {
            if (_localizer != null)
            {
                var val = _localizer[key];
                if (!val.ResourceNotFound && !string.IsNullOrWhiteSpace(val.Value))
                    return val.Value;
            }
            return isArabic ? arFallback : enFallback;
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Noto Sans Arabic").FontSize(12));
                
                if (isArabic)
                    page.ContentFromRightToLeft();
                else
                    page.ContentFromLeftToRight();

                page.Header()
                    .AlignCenter()
                    .Text($"{T("Invoice_Header", "INVOICE", "ÙØ§ØªÙˆØ±Ø©")}: {invoice.InvoiceNumber}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col2 =>
                            {
                                col2.Item().Text($"{T("Invoice_Number", "Invoice Number", "Ø±Ù‚Ù… Ø§Ù„ÙØ§ØªÙˆØ±Ø©")}: {invoice.InvoiceNumber}");
                                col2.Item().Text($"{T("Invoice_Date", "Date", "Ø§Ù„ØªØ§Ø±ÙŠØ®")}: {invoice.IssuedAt:dd/MM/yyyy HH:mm}");
                                col2.Item().Text($"{T("Invoice_Order", "Order", "Ø§Ù„Ø·Ù„Ø¨")}: {invoice.Order?.OrderNumber ?? invoice.OrderId.ToString()[..8]}");
                                if (invoice.Order?.Country != null)
                                    col2.Item().Text($"{T("Invoice_Country", "Country", "Ø§Ù„Ø¯ÙˆÙ„Ø©")}: {invoice.Order.Country.Name}");
                            });

                            row.RelativeItem().Column(col2 =>
                            {
                                col2.Item().Text(T("Invoice_CustomerInfo", "Customer Information:", "Ø¨ÙŠØ§Ù†Ø§Øª Ø§Ù„Ø¹Ù…ÙŠÙ„:")).Bold();
                                if (invoice.Order?.Customer != null)
                                {
                                    col2.Item().Text($"{T("Invoice_CustomerName", "Name", "Ø§Ù„Ø§Ø³Ù…")}: {invoice.Order.Customer.Name}");
                                    col2.Item().Text($"{T("Invoice_CustomerEmail", "Email", "Ø§Ù„Ø¨Ø±ÙŠØ¯ Ø§Ù„Ø¥Ù„ÙƒØªØ±ÙˆÙ†ÙŠ")}: {invoice.Order.Customer.Email}");
                                    col2.Item().Text($"{T("Invoice_CustomerPhone", "Phone", "Ø§Ù„Ù‡Ø§ØªÙ")}: {invoice.Order.Customer.Phone}");
                                }
                                else if (invoice.Order != null)
                                {
                                    var guestName = invoice.Order.GuestName ?? invoice.Order.Phone ?? (isArabic ? "Ø²Ø§Ø¦Ø±" : "Guest");
                                    var guestPhone = invoice.Order.GuestPhone ?? invoice.Order.Phone ?? "-";
                                    var guestAddr = invoice.Order.GuestAddress ?? invoice.Order.Address ?? "-";
                                    col2.Item().Text($"{T("Invoice_CustomerName", "Name", "Ø§Ù„Ø§Ø³Ù…")}: {guestName}");
                                    col2.Item().Text($"{T("Invoice_CustomerPhone", "Phone", "Ø§Ù„Ù‡Ø§ØªÙ")}: {guestPhone}");
                                    col2.Item().Text($"{T("Invoice_CustomerAddress", "Address", "Ø§Ù„Ø¹Ù†ÙˆØ§Ù†")}: {guestAddr}");
                                }
                                else
                                {
                                    col2.Item().Text(T("Invoice_GuestOrder", "Guest Order", "Ø·Ù„Ø¨ Ø²Ø§Ø¦Ø±"));
                                }
                            });
                        });


                        col.Item().PaddingTop(1, Unit.Centimetre).LineHorizontal(1);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(T("Invoice_TableProduct", "Product", "Ø§Ù„Ù…Ù†ØªØ¬")).Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(T("Invoice_TableQty", "Qty", "Ø§Ù„ÙƒÙ…ÙŠØ©")).Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(T("Invoice_TableUnitPrice", "Unit Price", "Ø³Ø¹Ø± Ø§Ù„ÙˆØ­Ø¯Ø©")).Bold().AlignRight();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text(T("Invoice_TableTotal", "Total", "Ø§Ù„Ø¥Ø¬Ù…Ø§Ù„ÙŠ")).Bold().AlignRight();
                            });

                            if (invoice.Order?.OrderItems != null && invoice.Order.OrderItems.Any())
                            {
                                foreach (var item in invoice.Order.OrderItems)
                                {
                                    var totalPrice = item.Quantity * item.PriceAtOrder;
                                    var productName = isArabic 
                                        ? (!string.IsNullOrWhiteSpace(item.Product?.NameAr) ? item.Product.NameAr : (item.Product?.NameEn ?? "Ù…Ù†ØªØ¬"))
                                        : (!string.IsNullOrWhiteSpace(item.Product?.NameEn) ? item.Product.NameEn : (item.Product?.NameAr ?? "Product"));
                                    
                                    table.Cell().Padding(5).Text(productName);
                                    table.Cell().Padding(5).Text(item.Quantity.ToString()).AlignRight();
                                    table.Cell().Padding(5).Text($"{item.PriceAtOrder:C}").AlignRight();
                                    table.Cell().Padding(5).Text($"{totalPrice:C}").AlignRight();
                                }
                            }
                            else
                            {
                                table.Cell().Padding(5).Text(T("Invoice_NoItems", "No items loaded", "Ù„Ø§ ØªÙˆØ¬Ø¯ Ø¹Ù†Ø§ØµØ±"));
                                table.Cell().Padding(5).Text("-");
                                table.Cell().Padding(5).Text("-");
                                table.Cell().Padding(5).Text("-");
                            }
                        });

                        col.Item().PaddingTop(1, Unit.Centimetre)
                            .AlignRight()
                            .Text($"{T("Invoice_TotalAmount", "Total Amount", "Ø§Ù„Ù…Ø¨Ù„Øº Ø§Ù„Ø¥Ø¬Ù…Ø§Ù„ÙŠ")}: {invoice.TotalAmount:C}")
                            .SemiBold().FontSize(16).FontColor(Colors.Green.Darken2);
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(T("Invoice_ThankYou", "Thank you for your business!", "Ø´ÙƒØ±Ø§Ù‹ Ù„ØªØ¹Ø§Ù…Ù„ÙƒÙ… Ù…Ø¹Ù†Ø§!"))
                    .FontSize(10).FontColor(Colors.Grey.Medium);
            });
        });

        return document.GeneratePdf();
    }
}

using FinTrackWebApi.Services.DocumentService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class PdfDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".pdf";
        public string MimeType => "application/pdf";

        public async Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (data is TransactionsRaportModel reportData)
            {
                var pdfDocument = new TransactionReportPdfDocument(reportData);

                byte[] pdfBytes = await Task.Run(() => pdfDocument.GeneratePdf());
                return pdfBytes;
            }
            else
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for PDF generation. Expected TransactionsRaportModel.",
                    nameof(data)
                );
            }
        }
    }

    public class TransactionReportPdfDocument : IDocument
    {
        private readonly TransactionsRaportModel _data;

        public TransactionReportPdfDocument(TransactionsRaportModel data)
        {
            _data = data;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.MarginVertical(40);
                page.MarginHorizontal(35);

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });

                page.Header()
                    .PaddingBottom(15)
                    .Column(col =>
                    {
                        col.Item().Text(_data.ReportTitle).SemiBold().FontSize(18).AlignCenter();
                        col.Item()
                            .Text($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(9)
                            .AlignCenter();
                        if (!string.IsNullOrWhiteSpace(_data.Description))
                        {
                            col.Item().PaddingTop(10).Text("Description:").FontSize(10).SemiBold();
                            col.Item().Text(_data.Description).FontSize(10);
                        }
                    });

                page.Content()
                    .PaddingVertical(5)
                    .Column(col =>
                    {
                        col.Item()
                            .PaddingBottom(5)
                            .Text("Transaction Details")
                            .SemiBold()
                            .FontSize(14);

                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25); // # (Sıra No)
                                    columns.RelativeColumn(2f); // Account Name
                                    columns.RelativeColumn(1.5f); // Category
                                    columns.RelativeColumn(1f); // Amount (Sağa hizalı olacak)
                                    columns.RelativeColumn(2.5f); // Description
                                    columns.ConstantColumn(75); // Transaction Date
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("#");
                                    header.Cell().Element(HeaderCellStyle).Text("Account Name");
                                    header.Cell().Element(HeaderCellStyle).Text("Category");
                                    header
                                        .Cell()
                                        .Element(HeaderCellStyle)
                                        .AlignRight()
                                        .Text("Amount");
                                    header.Cell().Element(HeaderCellStyle).Text("Description");
                                    header.Cell().Element(HeaderCellStyle).Text("Transaction");

                                    static IContainer HeaderCellStyle(IContainer c) =>
                                        c.DefaultTextStyle(x => x.SemiBold().FontSize(8))
                                            .PaddingVertical(4)
                                            .PaddingHorizontal(2)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Medium);
                                });

                                if (_data.Items != null && _data.Items.Any())
                                {
                                    int index = 1;
                                    foreach (var item in _data.Items)
                                    {
                                        table
                                            .Cell()
                                            .Element(DataCellStyle)
                                            .Text(index++.ToString());
                                        table.Cell().Element(DataCellStyle).Text(item.AccountName);
                                        table.Cell().Element(DataCellStyle).Text(item.CategoryName);
                                        table
                                            .Cell()
                                            .Element(DataCellStyle)
                                            .AlignRight()
                                            .Text(item.Amount.ToString("N2"));
                                        table.Cell().Element(DataCellStyle).Text(item.Description);
                                        table
                                            .Cell()
                                            .Element(DataCellStyle)
                                            .Text(item.TransactionDateUtc.ToString("yyyy-MM-dd"));

                                        static IContainer DataCellStyle(IContainer c) =>
                                            c.BorderBottom(1)
                                                .BorderColor(Colors.Grey.Lighten2)
                                                .PaddingVertical(3)
                                                .PaddingHorizontal(2)
                                                .DefaultTextStyle(x => x.FontSize(7));
                                    }
                                }
                                else
                                {
                                    table
                                        .Cell()
                                        .ColumnSpan(6)
                                        .PaddingTop(10)
                                        .AlignCenter()
                                        .Text("No transactions found.")
                                        .FontSize(9);
                                }
                            });

                        if (_data.Items != null && _data.Items.Any())
                        {
                            col.Item()
                                .PaddingTop(10)
                                .AlignRight()
                                .Text(txt =>
                                {
                                    txt.Span("Total Transactions Count: ").SemiBold().FontSize(10);
                                    txt.Span(_data.TotalCount.ToString()).FontSize(10);
                                });
                        }
                    });
            });
        }
    }
}

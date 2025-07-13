using FinTrackWebApi.Services.DocumentService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinTrackWebApi.Services.DocumentService.Generations.Budget
{
    public class PdfDocumentGenerator_Budget : IDocumentGenerator
    {
        public string FileExtension => ".pdf";
        public string MimeType => "application/pdf";

        public async Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (data is BudgetReportModel reportData)
            {
                var pdfDocument = new BudgetReportPdfDocument(reportData);

                byte[] pdfBytes = await Task.Run(() => pdfDocument.GeneratePdf());
                return pdfBytes;
            }
            else
            {
                throw new ArgumentException(
                    "Unsupported data type for PDF generation.",
                    nameof(data)
                );
            }
        }
    }

    public class BudgetReportPdfDocument : IDocument
    {
        private readonly BudgetReportModel _data;

        public BudgetReportPdfDocument(BudgetReportModel data)
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
                        col.Item().PaddingTop(10).Text("Description:").FontSize(10).SemiBold();
                        col.Item().Text(_data.Description).FontSize(10);
                    });

                page.Content()
                    .PaddingVertical(5)
                    .Column(col =>
                    {
                        col.Item().PaddingBottom(5).Text("Budget Details").SemiBold().FontSize(14);

                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25); // #
                                    columns.RelativeColumn(1.5f); // Name
                                    columns.RelativeColumn(2.0f); // Description
                                    columns.RelativeColumn(1.5f); // Category
                                    columns.ConstantColumn(50); // Type
                                    columns.ConstantColumn(65); // Start Date
                                    columns.ConstantColumn(65); // End Date
                                    columns.ConstantColumn(65); // Created At
                                    columns.ConstantColumn(65); // Updated At
                                    columns.RelativeColumn(1.2f); // AllocatedAmount
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("#");
                                    header.Cell().Element(HeaderCellStyle).Text("Name");
                                    header.Cell().Element(HeaderCellStyle).Text("Description");
                                    header.Cell().Element(HeaderCellStyle).Text("Category");
                                    header.Cell().Element(HeaderCellStyle).Text("Start");
                                    header.Cell().Element(HeaderCellStyle).Text("End");
                                    header.Cell().Element(HeaderCellStyle).Text("Created");
                                    header.Cell().Element(HeaderCellStyle).Text("Updated");
                                    header
                                        .Cell()
                                        .Element(HeaderCellStyle)
                                        .AlignRight()
                                        .Text("Allocated");

                                    static IContainer HeaderCellStyle(IContainer container) =>
                                        container
                                            .DefaultTextStyle(x => x.SemiBold().FontSize(7))
                                            .PaddingVertical(3)
                                            .PaddingHorizontal(2)
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Medium);
                                });

                                int index = 1;
                                foreach (var item in _data.Items)
                                {
                                    table.Cell().Element(DataCellStyle).Text(index++.ToString());
                                    table.Cell().Element(DataCellStyle).Text(item.Name);
                                    table.Cell().Element(DataCellStyle).Text(item.Description);
                                    table.Cell().Element(DataCellStyle).Text(item.Category);
                                    table
                                        .Cell()
                                        .Element(DataCellStyle)
                                        .Text(item.StartDate.ToString("yyyy-MM-dd"));
                                    table
                                        .Cell()
                                        .Element(DataCellStyle)
                                        .Text(item.EndDate.ToString("yyyy-MM-dd"));
                                    table
                                        .Cell()
                                        .Element(DataCellStyle)
                                        .Text(item.CreatedAt.ToString("yyyy-MM-dd"));
                                    table
                                        .Cell()
                                        .Element(DataCellStyle)
                                        .Text(
                                            item.UpdatedAt == DateTime.MinValue
                                            || item.UpdatedAt == default
                                                ? "-"
                                                : item.UpdatedAt.ToString("yyyy-MM-dd")
                                        );
                                    table
                                        .Cell()
                                        .Element(DataCellStyle)
                                        .AlignRight()
                                        .Text(item.AllocatedAmount.ToString());

                                    static IContainer DataCellStyle(IContainer container) =>
                                        container
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(2)
                                            .PaddingHorizontal(2)
                                            .DefaultTextStyle(x => x.FontSize(6));
                                }
                            });
                    });
            });
        }
    }
}

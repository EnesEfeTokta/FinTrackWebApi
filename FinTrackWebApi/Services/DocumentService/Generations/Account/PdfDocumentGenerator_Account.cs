using FinTrackWebApi.Services.DocumentService.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FinTrackWebApi.Services.DocumentService.Generations.Account
{
    public class PdfDocumentGenerator_Account : IDocumentGenerator
    {
        public string FileExtension => ".pdf";
        public string MimeType => "application/pdf";

        public async Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (data is AccountReportModel accountReport)
            {
                var pdfDocument = new AccountReportPdfDocument(accountReport);

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

    public class AccountReportPdfDocument : IDocument
    {
        private readonly AccountReportModel _data;

        public AccountReportPdfDocument(AccountReportModel data)
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
                        col.Item().PaddingBottom(5).Text("Account Details").SemiBold().FontSize(14);

                        col.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(25); // #
                                    columns.RelativeColumn(1.5f); // Name
                                    columns.RelativeColumn(1.5f); // Type
                                    columns.ConstantColumn(65); // Created At
                                    columns.ConstantColumn(65); // Updated At
                                    columns.RelativeColumn(1.2f); // Balance
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("#");
                                    header.Cell().Element(HeaderCellStyle).Text("Name");
                                    header.Cell().Element(HeaderCellStyle).Text("Type");
                                    header.Cell().Element(HeaderCellStyle).Text("Created");
                                    header.Cell().Element(HeaderCellStyle).Text("Updated");
                                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Balance");

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
                                    table.Cell().Element(DataCellStyle).Text(item.Type.ToString());
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
                                        .Text(item.Balance.ToString());

                                    static IContainer DataCellStyle(IContainer container) =>
                                        container
                                            .BorderBottom(1)
                                            .BorderColor(Colors.Grey.Lighten2)
                                            .PaddingVertical(2)
                                            .PaddingHorizontal(2)
                                            .DefaultTextStyle(x => x.FontSize(6));
                                }
                            });

                        col.Item().PaddingTop(20);

                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10)
                            .Column(summaryCol =>
                        {
                            summaryCol.Item().Text("Report Summary").SemiBold().FontSize(12);
                            summaryCol.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            summaryCol.Item().Text(txt =>
                            {
                                txt.Span("Total Accounts: ").SemiBold();
                                txt.Span(_data.AccountCount.ToString());
                            });
                            summaryCol.Item().Text(txt =>
                            {
                                txt.Span("Total Balance: ").SemiBold();
                                txt.Span($"{_data.TotalBalance:N2}");
                            });
                        });
                    });
            });
        }
    }
}

using FinTrackWebApi.Services.DocumentService.Models;
using ClosedXML.Excel;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class XlsxDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".xlsx";
        public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private const int NumberOfColumns = 6;

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is TransactionsRaportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for XLSX generation. Expected TransactionsRaportModel.", nameof(data));
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Transaction Report");

                int currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = reportData.ReportTitle;
                worksheet.Range(currentRow, 1, currentRow, NumberOfColumns).Merge().Style
                    .Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;

                if (!string.IsNullOrWhiteSpace(reportData.Description))
                {
                    worksheet.Cell(currentRow, 1).Value = "Description:";
                    worksheet.Cell(currentRow, 1).Style.Font.SetBold(true);
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = reportData.Description;
                    worksheet.Range(currentRow, 1, currentRow, NumberOfColumns).Merge().Style
                        .Alignment.SetWrapText(true)
                        .Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                    currentRow++;
                }

                worksheet.Cell(currentRow, 1).Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Range(currentRow, 1, currentRow, NumberOfColumns).Merge().Style
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left)
                    .Font.SetItalic(true);
                currentRow++;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Transaction Details";
                worksheet.Cell(currentRow, 1).Style.Font.SetBold(true).Font.SetFontSize(14);
                currentRow++;

                int headerRow = currentRow;
                worksheet.Cell(headerRow, 1).Value = "#";
                worksheet.Cell(headerRow, 2).Value = "Account Name";
                worksheet.Cell(headerRow, 3).Value = "Category";
                worksheet.Cell(headerRow, 4).Value = "Amount";
                worksheet.Cell(headerRow, 5).Value = "Description";
                worksheet.Cell(headerRow, 6).Value = "Transaction Date";

                var headerRange = worksheet.Range(headerRow, 1, headerRow, NumberOfColumns);
                headerRange.Style.Font.SetBold(true);
                headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                worksheet.Cell(headerRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                currentRow++;

                if (reportData.Items != null && reportData.Items.Any())
                {
                    int index = 1;
                    foreach (var item in reportData.Items)
                    {
                        worksheet.Cell(currentRow, 1).Value = index++;
                        worksheet.Cell(currentRow, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        worksheet.Cell(currentRow, 2).Value = item.AccountName;
                        worksheet.Cell(currentRow, 3).Value = item.CategoryName;

                        worksheet.Cell(currentRow, 4).Value = item.Amount;
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                        worksheet.Cell(currentRow, 5).Value = item.Description;

                        worksheet.Cell(currentRow, 6).Value = item.TransactionDateUtc;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "yyyy-MM-dd";
                        worksheet.Cell(currentRow, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        currentRow++;
                    }

                    var dataRange = worksheet.Range(headerRow + 1, 1, currentRow - 1, NumberOfColumns);
                    dataRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    dataRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

                    for (int i = headerRow + 1; i < currentRow; i++)
                    {
                        worksheet.Row(i).AdjustToContents();
                        worksheet.Row(i).Cells(1, NumberOfColumns).Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                    }
                }
                else
                {
                    worksheet.Cell(currentRow, 1).Value = "No transaction details found.";
                    worksheet.Range(currentRow, 1, currentRow, NumberOfColumns).Merge().Style
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                    currentRow++;
                }

                currentRow++;
                worksheet.Cell(currentRow, NumberOfColumns - 1).Value = "Total Transactions Count:";
                worksheet.Cell(currentRow, NumberOfColumns - 1).Style.Font.SetBold(true)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                worksheet.Cell(currentRow, NumberOfColumns).Value = reportData.TotalCount;
                worksheet.Cell(currentRow, NumberOfColumns).Style.Font.SetBold(true)
                    .NumberFormat.Format = "#,##0";
                worksheet.Cell(currentRow, NumberOfColumns).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);


                worksheet.Columns(1, NumberOfColumns).AdjustToContents();
                worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 20); // Account Name min genişlik
                worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 30); // Description min genişlik
                worksheet.Column(5).Style.Alignment.SetWrapText(true);


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return Task.FromResult(stream.ToArray());
                }
            }
        }
    }
}
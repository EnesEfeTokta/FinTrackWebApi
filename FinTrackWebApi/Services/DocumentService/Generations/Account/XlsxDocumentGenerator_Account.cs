using ClosedXML.Excel;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Account
{
    public class XlsxDocumentGenerator_Account : IDocumentGenerator
    {
        public string FileExtension => ".xlsx";
        public string MimeType =>
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is AccountReportModel accountReport))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for XLSX generation. Expected AccountReportModel.",
                    nameof(data)
                );
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Account Report");

                int currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = accountReport.ReportTitle;
                worksheet
                    .Range(currentRow, 1, currentRow, 6)
                    .Merge()
                    .Style.Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Description:";
                worksheet.Cell(currentRow, 1).Style.Font.SetBold(true);
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = accountReport.Description;
                worksheet
                    .Range(currentRow, 1, currentRow, 6)
                    .Merge()
                    .Style.Alignment.SetWrapText(true);
                currentRow++;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Account Details";
                worksheet.Cell(currentRow, 1).Style.Font.SetBold(true).Font.SetFontSize(14);
                currentRow++;

                int headerRow = currentRow;
                worksheet.Cell(headerRow, 1).Value = "#";
                worksheet.Cell(headerRow, 2).Value = "Name";
                worksheet.Cell(headerRow, 3).Value = "Type";
                worksheet.Cell(headerRow, 4).Value = "Created At";
                worksheet.Cell(headerRow, 5).Value = "Updated At";
                worksheet.Cell(headerRow, 6).Value = "Balance";

                var headerRange = worksheet.Range(headerRow, 1, headerRow, 6);
                headerRange.Style.Font.SetBold(true);
                headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                worksheet
                    .Cell(headerRow, 6)
                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                currentRow++;

                if (accountReport.Items != null && accountReport.Items.Any())
                {
                    int index = 1;
                    foreach (var item in accountReport.Items)
                    {
                        worksheet.Cell(currentRow, 1).Value = index++;
                        worksheet.Cell(currentRow, 2).Value = item.Name;
                        worksheet.Cell(currentRow, 3).Value = item.Type?.ToString();
                        worksheet.Cell(currentRow, 4).Value = item.CreatedAt;
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "yyyy-MM-dd";

                        if (item.UpdatedAt != DateTime.MinValue && item.UpdatedAt != default)
                        {
                            worksheet.Cell(currentRow, 5).Value = item.UpdatedAt;
                            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "yyyy-MM-dd";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 5).Value = "-";
                        }

                        worksheet.Cell(currentRow, 6).Value = item.Balance;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 6).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);


                        currentRow++;
                    }

                    var dataRange = worksheet.Range(headerRow + 1, 1, currentRow - 1, 6);
                    dataRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    dataRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                }
                else
                {
                    worksheet.Cell(currentRow, 1).Value = "No account details found.";
                    worksheet.Range(currentRow, 1, currentRow, 6).Merge();
                }

                currentRow++;
                int summaryRowStart = currentRow;

                worksheet.Cell(currentRow, 5).Value = "Total Accounts:";
                worksheet.Cell(currentRow, 5).Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                worksheet.Cell(currentRow, 6).Value = accountReport.AccountCount;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0";
                currentRow++;

                worksheet.Cell(currentRow, 5).Value = "Total Balance:";
                worksheet.Cell(currentRow, 5).Style.Font.SetBold(true).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
                worksheet.Cell(currentRow, 6).Value = accountReport.TotalBalance;
                worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";

                var summaryRange = worksheet.Range(summaryRowStart, 5, currentRow, 6);
                summaryRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);

                worksheet.Columns(1, 6).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return Task.FromResult(stream.ToArray());
                }
            }
        }
    }
}
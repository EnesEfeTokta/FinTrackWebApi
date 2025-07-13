using ClosedXML.Excel;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Budget
{
    public class XlsxDocumentGenerator_Budget : IDocumentGenerator
    {
        public string FileExtension => ".xlsx";
        public string MimeType =>
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is BudgetReportModel reportData))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for XLSX generation. Expected BudgetReportModel.",
                    nameof(data)
                );
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Budget Report");

                int currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = reportData.ReportTitle;
                worksheet
                    .Range(currentRow, 1, currentRow, 10)
                    .Merge()
                    .Style.Font.SetBold(true)
                    .Font.SetFontSize(16)
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Description:";
                worksheet.Cell(currentRow, 1).Style.Font.SetBold(true);
                currentRow++;
                worksheet.Cell(currentRow, 1).Value = reportData.Description;
                worksheet
                    .Range(currentRow, 1, currentRow, 10)
                    .Merge()
                    .Style.Alignment.SetWrapText(true);
                currentRow++;
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Budget Details";
                worksheet.Cell(currentRow, 1).Style.Font.SetBold(true).Font.SetFontSize(14);
                currentRow++;

                int headerRow = currentRow;
                worksheet.Cell(headerRow, 1).Value = "#";
                worksheet.Cell(headerRow, 2).Value = "Name";
                worksheet.Cell(headerRow, 3).Value = "Description";
                worksheet.Cell(headerRow, 4).Value = "Category";
                worksheet.Cell(headerRow, 6).Value = "Start Date";
                worksheet.Cell(headerRow, 7).Value = "End Date";
                worksheet.Cell(headerRow, 8).Value = "Created At";
                worksheet.Cell(headerRow, 9).Value = "Updated At";
                worksheet.Cell(headerRow, 10).Value = "Allocated Amount";

                var headerRange = worksheet.Range(headerRow, 1, headerRow, 10);
                headerRange.Style.Font.SetBold(true);
                headerRange.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                headerRange.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                headerRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                headerRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                worksheet
                    .Cell(headerRow, 10)
                    .Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                currentRow++;

                if (reportData.Items != null && reportData.Items.Any())
                {
                    int index = 1;
                    foreach (var item in reportData.Items)
                    {
                        worksheet.Cell(currentRow, 1).Value = index++;
                        worksheet.Cell(currentRow, 2).Value = item.Name;
                        worksheet.Cell(currentRow, 3).Value = item.Description;
                        worksheet.Cell(currentRow, 4).Value = item.Category;

                        worksheet.Cell(currentRow, 6).Value = item.StartDate;
                        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "yyyy-MM-dd";
                        worksheet.Cell(currentRow, 7).Value = item.EndDate;
                        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "yyyy-MM-dd";
                        worksheet.Cell(currentRow, 8).Value = item.CreatedAt;
                        worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "yyyy-MM-dd";

                        if (item.UpdatedAt != DateTime.MinValue && item.UpdatedAt != default)
                        {
                            worksheet.Cell(currentRow, 9).Value = item.UpdatedAt;
                            worksheet.Cell(currentRow, 9).Style.NumberFormat.Format = "yyyy-MM-dd";
                        }
                        else
                        {
                            worksheet.Cell(currentRow, 9).Value = "-";
                        }

                        worksheet.Cell(currentRow, 10).Value = item.AllocatedAmount;
                        worksheet.Cell(currentRow, 10).Style.NumberFormat.Format = string.Format(
                            "#,##0.00 \"{0}\""
                        );

                        currentRow++;
                    }

                    var dataRange = worksheet.Range(headerRow + 1, 1, currentRow - 1, 10);
                    dataRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
                    dataRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);
                }
                else
                {
                    worksheet.Cell(currentRow, 1).Value = "No budget details found.";
                    worksheet.Range(currentRow, 1, currentRow, 10).Merge();
                }

                worksheet.Columns(1, 10).AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return Task.FromResult(stream.ToArray());
                }
            }
        }
    }
}

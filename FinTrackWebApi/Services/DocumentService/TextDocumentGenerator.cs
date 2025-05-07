using System.Text;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService
{
    public class TextDocumentGenerator : IDocumentGenerator
    {
        public string FileExtension => ".txt";
        public string MimeType => "text/plain";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is BudgetReportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Text generation. Expected BudgetReportModel.", nameof(data));
            }

            StringBuilder sb = new StringBuilder();

            // --- Başlık Bilgileri ---
            sb.AppendLine($"==================================================");
            sb.AppendLine($"          {reportData.ReportTitle}");
            sb.AppendLine($"==================================================");

            sb.AppendLine($"\nDescription:");
            sb.AppendLine(reportData.Description);
            sb.AppendLine();

            sb.AppendLine("Budget Details:");

            sb.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------");
            sb.AppendFormat("| {0,-3} | {1,-20} | {2,-25} | {3,-15} | {4,-10} | {5,-10} | {6,-10} | {7,-10} | {8,-10} | {9,15} |\n",
                          "#", "Name", "Description", "Category", "Type", "Start", "End", "Created", "Updated", "Allocated");
            sb.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------");

            if (reportData.Items != null && reportData.Items.Any())
            {
                int index = 1;
                foreach (var item in reportData.Items)
                {
                    string allocatedStr = item.AllocatedAmount.ToString();
                    string updatedAtStr = item.UpdatedAt == DateTime.MinValue || item.UpdatedAt == default ? "-" : item.UpdatedAt.ToString("yyyy-MM-dd");

                    string name = Truncate(item.Name, 20);
                    string description = Truncate(item.Description, 25);
                    string category = Truncate(item.Category, 15);

                    sb.AppendFormat("| {0,-3} | {1,-20} | {2,-25} | {3,-15} | {4,-10} | {5,-10:yyyy-MM-dd} | {6,-10:yyyy-MM-dd} | {7,-10:yyyy-MM-dd} | {8,-10} | {9,15} |\n",
                                  index++,
                                  name,
                                  description,
                                  category,
                                  item.Type,
                                  item.StartDate,
                                  item.EndDate,
                                  item.CreatedAt,
                                  updatedAtStr,
                                  allocatedStr);
                }
                sb.AppendLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------");

            }
            else
            {
                sb.AppendLine("No budget details found.");
            }


            sb.AppendLine("\n--- End of Report ---");

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }
    }
}
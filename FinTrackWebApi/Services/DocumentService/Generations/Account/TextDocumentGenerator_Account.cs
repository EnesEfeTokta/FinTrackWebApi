using FinTrackWebApi.Services.DocumentService.Models;
using System.Text;

namespace FinTrackWebApi.Services.DocumentService.Generations.Account
{
    public class TextDocumentGenerator_Account : IDocumentGenerator
    {
        public string FileExtension => ".txt";
        public string MimeType => "text/plain";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is AccountReportModel accountReport))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Text generation. Expected AccountReportModel.", nameof(data));
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("==================================================");
            sb.AppendLine($"          {accountReport.ReportTitle}");
            sb.AppendLine("==================================================");

            sb.AppendLine($"\nDescription:");
            sb.AppendLine(accountReport.Description);
            sb.AppendLine();

            sb.AppendLine("Account Details:");

            string header = string.Format("| {0,-4} | {1,-25} | {2,-15} | {3,-12} | {4,-12} | {5,15} |",
                                          "#", "Name", "Type", "Created", "Updated", "Balance");
            sb.AppendLine(new string('-', header.Length));
            sb.AppendLine(header);
            sb.AppendLine(new string('-', header.Length));

            if (accountReport.Items != null && accountReport.Items.Any())
            {
                int index = 1;
                foreach (var item in accountReport.Items)
                {
                    string updatedAtStr = (item.UpdatedAt == DateTime.MinValue || item.UpdatedAt == default)
                                          ? "-" : item.UpdatedAt.ToString("yyyy-MM-dd");

                    string row = string.Format("| {0,-4} | {1,-25} | {2,-15} | {3,-12:yyyy-MM-dd} | {4,-12} | {5,15:N2} |",
                                               index++,
                                               Truncate(item.Name, 25),
                                               item.Type?.ToString() ?? "N/A",
                                               item.CreatedAt,
                                               updatedAtStr,
                                               item.Balance);
                    sb.AppendLine(row);
                }
                sb.AppendLine(new string('-', header.Length));
            }
            else
            {
                sb.AppendLine("No account details found.");
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
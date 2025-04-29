using DocumentFormat.OpenXml; // OpenXml temel türleri için
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Threading.Tasks;
using System; // ArgumentException için

namespace FinTrackWebApi.Services.DocumentService // Veya Services.Generation
{
    public class WordDocumentGenerator : IDocumentGenerator
    {
        public string FileExtension => ".docx";
        public string MimeType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            // OpenXML SDK kullanarak .docx oluşturma mantığı
            using (MemoryStream mem = new MemoryStream())
            {
                // Word belgesi oluştur - Sadece stream ve belge tipi (WordprocessingDocumentType.Document) yeterli
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document))
                {
                    // Ana belge bölümünü ekle
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

                    // Yeni bir belge içeriği oluştur
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    // Veri modelini kullanarak içerik ekle
                    if (data is BudgetReportModel reportData) // Kendi veri modelinizi kullanın
                    {
                        // Başlık Ekleme (Örnek - Stil ile)
                        Paragraph titlePara = new Paragraph(
                            new Run(
                                new RunProperties(new Bold()), // Kalın yap
                                new Text($"Report: {reportData.ReportTitle}")
                            )
                        );
                        body.AppendChild(titlePara);

                        // Özet Ekleme
                        Paragraph summaryPara = new Paragraph(
                            new Run(
                                new Text(reportData.Description)
                            )
                        );
                        body.AppendChild(summaryPara);
                    }
                    else if (data is string stringData) // Eğer veri basit bir string ise
                    {
                        body.AppendChild(new Paragraph(new Run(new Text(stringData))));
                    }
                    else
                    {
                        throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Word generation.", nameof(data));
                    }
                }

                return Task.FromResult(mem.ToArray());
            }
        }
    }
}
using FinTrackWebApi.Services.DocumentService.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FinTrackWebApi.Services.DocumentService.Generations.Account
{
    public class XmlDocumentGenerator_Account : IDocumentGenerator
    {
        public string FileExtension => ".xml";
        public string MimeType => "application/xml";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is AccountReportModel accountReport))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for XML generation. Expected AccountReportModel.", nameof(data));
            }

            var serializer = new XmlSerializer(typeof(AccountReportModel));

            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false,
            };

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(xmlWriter, accountReport, namespaces);
                }
                return Task.FromResult(memoryStream.ToArray());
            }
        }
    }
}
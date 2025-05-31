namespace FinTrackWebApi.Services.DocumentService
{
    public interface IDocumentGenerationService
    {
        Task<(byte[] Content, string MimeType, string FileName)> GenerateDocumentAsync<TData>(TData data, DocumentFormat format, DocumentType type, string baseFileName) where TData : class;
    }

    public enum DocumentFormat
    {
        Pdf,
        Word,
        Text,
        Markdown,
        Xml,
        Xlsx
    }

    public enum DocumentType
    {
        Budget,
        Transaction,
        Account
    }
}

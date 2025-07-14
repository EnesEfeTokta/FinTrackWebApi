namespace FinTrackWebApi.Enums
{
    public enum VideoStorageType
    {
        FileSystem, // Dosya sisteminde şifresiz
        AzureBlob, // Azure Blob'da şifresiz (kullanıyorsanız)
        EncryptedFileSystem, // Dosya sisteminde şifreli
    }
}

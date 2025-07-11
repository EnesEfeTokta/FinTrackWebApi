namespace FinTrackWebApi.Enums
{
    public enum VideoStatusType
    {
        PendingApproval, // Onay Bekliyor
        ApprovedAndQueuedForEncryption, // Operatör Onayladı, Şifreleme Kuyruğunda
        ProcessingEncryption, // Şifreleniyor
        Encrypted, // Başarıyla Şifrelendi (ve anahtar gönderildi)
        Rejected, // Reddedildi
        EncryptionFailed, // Şifreleme Başarısız Oldu
        ProcessingError, // Genel Bir İşlem Hatası
    }
}

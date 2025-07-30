namespace FinTrackWebApi.Enums
{
    public enum DebtStatusType
    {
        PendingBorrowerAcceptance,    // Borç Alan Onayı Bekliyor
        AcceptedPendingVideoUpload,   // Borçlu Kabul Etti, Video Yüklemesi Bekleniyor (YENİ)
        PendingOperatorApproval,      // Operatör Onayı Bekliyor
        Active,                       // Aktif Borç
        PaymentConfirmationPending,   // Ödeme Onayı Bekliyor
        Paid,                         // Ödendi
        Defaulted,                    // Vadesi Geçmiş/Ödenmemiş
        RejectedByBorrower,           // Borç Alan Tarafından Reddedildi
        RejectedByOperator,           // Operatör Tarafından Reddedildi
        CancelledByLender,            // Borç Veren Tarafından İptal Edildi
    }
}
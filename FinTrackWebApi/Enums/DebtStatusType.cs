namespace FinTrackWebApi.Enums
{
    public enum DebtStatusType
    {
        PendingBorrowerAcceptance, // Borç Alan Onayı Bekliyor
        PendingOperatorApproval, // Operatör Onayı Bekliyor (eğer varsa)
        Active, // Aktif Borç
        PaymentConfirmationPending, // Ödeme Onayı Bekliyor
        Paid, // Ödendi
        Defaulted, // Vadesi Geçmiş/Ödenmemiş
        RejectedByBorrower, // Borç Alan Tarafından Reddedildi
        RejectedByOperator, // Operatör Tarafından Reddedildi (eğer varsa)
        CancelledByLender, // Borç Veren Tarafından İptal Edildi
    }
}

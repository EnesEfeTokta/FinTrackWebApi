using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Dtos.DebtDtos
{
    // Video için ve borcun güncellenmesi sadece Dorç statusunu değiştirmek için kullanılacak.
    // Nedeni ise Önceden video ve operatör onayı işlemleri geçersiz hale gelir.
    public class DebtUpdateDto
    {
        public DebtStatusType Status { get; set; }
    }
}

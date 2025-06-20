﻿using System.ComponentModel.DataAnnotations;

namespace FinTrackWebApi.Dtos
{
    public class UserMembershipDto
    {
        public int UserMembershipId { get; set; }
        public int PlanId { get; set; }
        public string? PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public bool AutoRenew { get; set; }
    }

    public class SubscriptionRequestDto
    {
        [Required]
        public int PlanId { get; set; }
        public bool AutoRenew { get; set; } = true; // Varsayılan olarak otomatik yenileme açık olabilir
        // Ödeme yöntemi token'ı gibi ek bilgiler buraya gelebilir
        // public string? PaymentMethodToken { get; set; }
    }
}

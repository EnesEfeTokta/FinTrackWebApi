namespace FinTrackWebApi.Dtos
{
    public class UserSettingsDto
    {
        public string Theme { get; set; } = "light";
        public string Language { get; set; } = "tr";
        public string Currency { get; set; } = "TRY";
        public bool Notification { get; set; } = true;
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    }
}

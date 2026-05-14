namespace InventurApp.Models
{
    public class UserAccount
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Benutzer";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? PasswordChangedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public int FailedLoginCount { get; set; }
    }
}

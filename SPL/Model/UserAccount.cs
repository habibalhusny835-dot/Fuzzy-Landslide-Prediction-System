using SQLite;

namespace SPL.Model
{
    public class UserAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string FaceData { get; set; } = string.Empty;

        public string FaceImagePath { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
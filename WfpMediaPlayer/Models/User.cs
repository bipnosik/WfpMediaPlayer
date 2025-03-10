using System;

namespace  WfpMediaPlayer.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } // Для примера – хранится как текст
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string Role { get; set; } // "User" или "Admin"
    }
}

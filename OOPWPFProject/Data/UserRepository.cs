using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OOPWPFProject.Models;

namespace OOPWPFProject.Data
{
    public class UserRepository
    {
        private static readonly string FilePath = Path.Combine("Data", "users.json");
        private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };

        public static string HashPassword(string password)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password + "cinema_salt_2026"));
            return Convert.ToBase64String(bytes);
        }
        private List<User> ReadAll()
        {
            if (!File.Exists(FilePath)) return new List<User>();
            try
            {
                string json = File.ReadAllText(FilePath, Encoding.UTF8);
                return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
            }
            catch { return new List<User>(); }
        }

        private void WriteAll(List<User> users) 
        {
            Directory.CreateDirectory("Data");
            File.WriteAllText(FilePath, JsonSerializer.Serialize(users, JsonOpts), Encoding.UTF8);
        }

        public void EnsureAdminExists()
        {
            var users = ReadAll();
            if (users.Any(u => u.Role == "admin")) return;

            users.Add(new User
            {
                Id = 1,
                FullName = "Адміністратор",
                Phone = "0000000000",
                Email = "admin@cinema.ua",
                PasswordHash = HashPassword("admin123"),
                Role = "admin"
            });
            WriteAll(users);
        }

        public User Register(string fullName, string phone, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentNullException("Введіть ваше ПІБ.");
            if (string.IsNullOrWhiteSpace(phone)) throw new ArgumentNullException("Введіть ваш номер телефону.");
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException("Введіть вашу електронну адресу.");
            if (password.Length < 8) throw new ArgumentNullException("Пароль має містити щонайменше 8 символів.");

            var users = ReadAll();

            if (users.Any(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Користувач з такою адресою вже існує.");
            if (users.Any(u => u.Phone.Equals(phone.Trim(), StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Користувач з таким телефоном вже існує.");

            int nextUserId = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
            var user = new User
            {
                Id = nextUserId,
                FullName = fullName.Trim(),
                Phone = phone.Trim(),
                Email = email.Trim(),
                PasswordHash = HashPassword(password),
                Role = "user"
            };
            users.Add(user);
            WriteAll(users);
            return user;
        }

        public User? Login(string emailOrPhone, string password)
        {
            string hash = HashPassword(password);
            string key = emailOrPhone.Trim();

            return ReadAll().FirstOrDefault(u => (u.Email == key || u.Phone == emailOrPhone.Trim()) && u.PasswordHash == hash);
        }

        public List<User> GetAll() => ReadAll();
    }
}

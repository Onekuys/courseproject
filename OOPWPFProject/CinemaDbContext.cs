using Microsoft.EntityFrameworkCore;
using System.IO;

namespace OOPWPFProject
{ 
    public class CinemaDbContext : DbContext
    {
        // Створюємо таблиці для фільмів, сеансів та бронювань
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Showtime> Showtimes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }
            optionsBuilder.UseSqlite("Data Source=Data/cinema.db");
        }
    }
}
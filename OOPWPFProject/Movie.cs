using System.Collections.Generic;

namespace OOPWPFProject
{
    public class Movie
    {
        // Первинний ключ для бази даних
        public int Id { get; set; }
        public string Title { get; set; }
        public int DurationMinutes { get; set; }

        // Navigation Property
        public List<Showtime> Showtimes { get; set; } = new List<Showtime>();
    }
}
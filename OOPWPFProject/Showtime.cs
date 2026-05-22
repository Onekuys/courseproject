using System;
using System.Collections.Generic;

namespace OOPWPFProject
{
    public class Showtime
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Format { get; set; } 
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        public override string ToString()
        {
            return $"{Time:hh\\:mm} — {Format}";
        }
    }
}
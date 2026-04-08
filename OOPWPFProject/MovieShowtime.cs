using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject
{
        public class MovieShowtime
        {
            public string MovieTitle { get; set; }
            public TimeSpan Showtime { get; set; }
            public int SeatNumber { get; set; }
            public string? MovieFormat { get; set; }
            public string? AdditionalWishes { get; set; }

            public MovieShowtime()
            {
               
            }


        public MovieShowtime(string movieTitle, TimeSpan showtime, int seatNumber, string? movieFormat, string? additionalWishes)
            {
                if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
                if (seatNumber < 1) throw new ArgumentException("Номер місця повинен бути додатнім числом.");
                MovieTitle = movieTitle;
                Showtime = showtime;
                SeatNumber = seatNumber;
                MovieFormat = movieFormat;
                AdditionalWishes = additionalWishes;
            }
        public string DisplayInfo()
        {
            return $"Назва: {MovieTitle}, Час: {Showtime}, Місце: {SeatNumber}, Формат: {MovieFormat}, Додаткові побажання: {AdditionalWishes}";
        }
        public MovieShowtime Clone()
        {
            return new MovieShowtime(MovieTitle, Showtime, SeatNumber, MovieFormat, AdditionalWishes);
        }
    }
}

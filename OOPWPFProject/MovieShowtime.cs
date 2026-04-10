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

            private string? format;
            public string? Format
            {
                get {
                    return format;
                }
                set
                {
                    format = value;
                }
            }
            public string? AdditionalWishes { get; set; }

            public MovieShowtime() { }


        public MovieShowtime(string movieTitle, TimeSpan showtime, int seatNumber, string? movieFormat, string? additionalWishes)
            {
                if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
                if (seatNumber < 1) throw new ArgumentException("Номер місця повинен бути додатнім числом.");
                MovieTitle = movieTitle;
                Showtime = showtime;
                SeatNumber = seatNumber;
                Format = movieFormat;
                AdditionalWishes = additionalWishes;
            }
        public string DisplayInfo()
        {
            return $"Назва: {MovieTitle}, Час: {Showtime}, Місце: {SeatNumber}, Формат: {Format}, Додаткові побажання: {AdditionalWishes}";
        }
        public override string ToString()
        {
            return DisplayInfo();
        }
        public MovieShowtime Clone()
        {
            return new MovieShowtime(MovieTitle, Showtime, SeatNumber, Format, AdditionalWishes);
        }

        public string SeatSummary
        {
            get { return $"Формат: {Format}, Місце: {SeatNumber}"; } 
        }
        public string IsPremiumFormat
        {
            get
            {
                if (Format == "IMAX") return $"Так";
                return $"Ні";
            }
        }
    }
}

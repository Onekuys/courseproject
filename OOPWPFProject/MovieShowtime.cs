using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject
{
        public struct MovieShowtime
        {
            public string MovieTitle { get; set; }
            public TimeSpan Showtime { get; set; }
            public int SeatNumber { get; set; }
            public string? MovieFormat { get; set; }
            public string? AdditionalWishes { get; set; }

            public MovieShowtime(string movieTitle, TimeSpan showtime, int seatNumber, string? movieFormat, string? additionalWishes)
            {
                MovieTitle = movieTitle;
                Showtime = showtime;
                SeatNumber = seatNumber;
                MovieFormat = movieFormat;
                AdditionalWishes = additionalWishes;
            }
        }

    }

using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject
{
    public class Reservation
    {
        public string MovieTitle { get; set; }
        public TimeSpan ShowTime { get; set; }
        public int SeatNumber { get; set; }

        public Reservation(string movieTitle, TimeSpan showtime, int seatNumber)
        {
            if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
            if (seatNumber < 1) throw new ArgumentException("Номер місця повинен бути більше 0.");
            MovieTitle = movieTitle;
            ShowTime = showtime;
            SeatNumber = seatNumber;
        }

        public virtual string GetDetails()
        {
            return $"Назва: {MovieTitle}, Час сеансу: {ShowTime:hh\\:mm}, Номер місця: {SeatNumber}";
        }
        public string Details => GetDetails();
    }

    public class StandardReservation: Reservation
    {
        private string? format;
        public string? Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
            }
        }

        public StandardReservation(string title, TimeSpan time, int seat, string? format): base(title, time, seat)
        {
            Format = format;
        }
        public override string GetDetails()
        {
            return $"Формат: {Format}";
        }

    }
    public class VIPReservation: Reservation
    {
        public bool LoungeAccess {  get; set; }
        public bool ComplementarySnacks { get; set; }

        public VIPReservation(string title, TimeSpan time, int seat, bool lounge, bool snacks) : base(title, time, seat)
        {
            LoungeAccess = lounge;
            ComplementarySnacks = snacks;
        }
        public override string GetDetails()
        {
            string lounge = LoungeAccess ? "Так" : "Ні";
            string snacks = ComplementarySnacks ? "Так" : "Ні";
            return $"VIP-зона: {LoungeAccess}, Закуски: {ComplementarySnacks}";
        }
    }

    
}

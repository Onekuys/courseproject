using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OOPWPFProject
{
    public abstract class AbstractReservation
    {
        public string MovieTitle { get; set; }
        public TimeSpan ShowTime { get; set; }
        public List<int> SeatNumbers { get; set; }
        public string SeatDisplay => string.Join(", ", SeatNumbers);
        public string Format { get; set; }
        public bool IsCanceled { get; set; }

    }

    [JsonDerivedType(typeof(Reservation), typeDiscriminator: "Standard")]
    [JsonDerivedType(typeof(VIPReservation), typeDiscriminator: "VIP")]
    public class Reservation : AbstractReservation, ICancelable
    {
        public Reservation() { }
        public Reservation(string movieTitle, TimeSpan showtime, int seatNumber, string format, bool isCanceled = false)
        {
            if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
            if (seatNumber < 1 || seatNumber > 30) throw new ArgumentException("Номер місця не може бути порожнім.");
            MovieTitle = movieTitle;
            ShowTime = showtime;
            SeatNumbers = new List<int> { seatNumber };
            Format = format;
            IsCanceled = isCanceled;
        }
        // Конструктор для групових бронювань
        public Reservation(string movieTitle, TimeSpan showtime, List<int> seatNumber, string format, bool isCanceled = false)
        {
            if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
            if (seatNumber.Count == 0) throw new ArgumentException("Номер місця не може бути порожнім.");
            MovieTitle = movieTitle;
            ShowTime = showtime;
            SeatNumbers = new List<int> (seatNumber);
            Format = format;
            IsCanceled = isCanceled;
        }

        // Перевантаження оператора +
        public static Reservation operator +(Reservation r1, Reservation r2)
        {
            if ((r1.MovieTitle == r2.MovieTitle) && (r1.ShowTime == r2.ShowTime) && (r1.Format == r2.Format)){
                var groupedSeats = new List<int> { };
                groupedSeats.AddRange(r1.SeatNumbers);
                groupedSeats.AddRange(r2.SeatNumbers);
                groupedSeats = groupedSeats.Distinct().OrderBy(x => x).ToList();
                

                // Перевірка на VIP
                bool isVip1 = r1 is VIPReservation;
                bool isVip2 = r2 is VIPReservation;

                if (isVip1 || isVip2)
                {
                    var vip1 = r1 as VIPReservation;
                    var vip2 = r2 as VIPReservation;

                    bool combinedLounge = (vip1?.LoungeAccess == true) && (vip2?.LoungeAccess == true);
                    bool combinedSnacks = (vip1?.ComplementarySnacks == true) && (vip2?.ComplementarySnacks == true);

                    string? combinedWishes = $"{vip1?.AdditionalWishes} {vip2?.AdditionalWishes}".Trim();

                    return new VIPReservation(r1.MovieTitle, r1.ShowTime, groupedSeats, r1.Format, combinedLounge, combinedSnacks, combinedWishes);
                }
                return new Reservation(r1.MovieTitle, r1.ShowTime, groupedSeats, r1.Format);
            }
            else
            {
                throw new ArgumentException("Сеанси не збігаються!");
            }
        }

        // Перевантаження операторів > та <
        public static bool operator >(Reservation r1, Reservation r2)
        {
            return r1.ShowTime > r2.ShowTime;
        }
        public static bool operator <(Reservation r1, Reservation r2)
        {
            return r1.ShowTime < r2.ShowTime;
        }

        // Перевантаження операторі == та !=

        public static bool operator ==(Reservation r1, Reservation r2)
        {
            if (ReferenceEquals(r1, r2)) return true;
            if (r1 is null || r2 is null) return false;

            return r1.ShowTime == r2.ShowTime && r1.SeatNumbers.Intersect(r2.SeatNumbers).Any();
        }

        public static bool operator !=(Reservation r1, Reservation r2)
        {
            return !(r1 == r2);
        }

        public override bool Equals(object obj)
        {
            if (obj is Reservation other)
                return this == other;
            return false;
        }

        public override int GetHashCode()
        {
            return ShowTime.GetHashCode();
        }



        public virtual string GetReservationDetails()
        {
            return IsCanceled ? $"[СКАСОВАНО] Формат: {Format}" : $"Формат: {Format}";
        }
        public string Details => GetReservationDetails();

        public void Cancel()
        {
            IsCanceled = true;
        }
    }
    public class VIPReservation : Reservation
    {
        public bool LoungeAccess { get; set; }
        public bool ComplementarySnacks { get; set; }
        public string AdditionalWishes { get; set; }

        public VIPReservation() { }
        public VIPReservation(string title, TimeSpan time, int seat, string format, bool lounge, bool snacks, string wishes, bool isCanceled = false): base(title, time, seat, format, isCanceled)
        {
            LoungeAccess = lounge;
            ComplementarySnacks = snacks;
            AdditionalWishes = wishes;
        }
        // Конструктор для групових VIP-бронювань
        public VIPReservation(string title, TimeSpan time, List<int> seats, string format, bool lounge, bool snacks, string wishes, bool isCanceled = false) : base(title, time, seats, format, isCanceled) 
        {
            LoungeAccess = lounge;
            ComplementarySnacks = snacks;
            AdditionalWishes = wishes;
        }
        public override string GetReservationDetails()
        {
            string lounge = LoungeAccess ? "Так" : "Ні";
            string snacks = ComplementarySnacks ? "Так" : "Ні";
            string wishes = string.IsNullOrWhiteSpace(AdditionalWishes) ? "-" : AdditionalWishes ;
            return IsCanceled ? $"[СКАСОВАНО] Формат: {Format}, VIP-зона: {lounge}, Закуски: {snacks}, Додатково:{wishes}" : $"Формат: {Format}, VIP-зона: {lounge}, Закуски: {snacks}, Додатково:{wishes}";
        }
    }

    
}

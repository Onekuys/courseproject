using System;
using System.Windows.Documents;
using OOPWPFProject.Helpers;

namespace OOPWPFProject.Models
{
    // Клас для бронювання квитків
    public class Reservation : BookingBase
    {
        // Поля для зберігання інформації про бронювання
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; } = null!;
        public int SeatNumber { get; set; }
        public bool IsPaid { get; set; }
        public bool? LoungeAccess { get; set; }
        public bool? ComplementarySnacks { get; set; }
        public string? AdditionalWishes { get; set; }

        // Конструктори
        public Reservation() { }

        public Reservation(Showtime showtime, int seatNumber, bool? lounge = null, bool? snacks = null, string? wishes = null, int? userId = null)
        {
            if (seatNumber < 1 || seatNumber > 72) throw new ArgumentException("Номер місця має бути від 1 до 72 включно.");
            Showtime = showtime;
            ShowtimeId = showtime.Id;
            SeatNumber = seatNumber;
            IsCanceled = false;
            LoungeAccess = lounge;
            ComplementarySnacks = snacks;
            AdditionalWishes = wishes;
            UserId = userId;

        }
        // Обчислювальні властивості для DataGrid
        public string MovieTitle => Showtime?.Movie?.Title ?? "-";
        public TimeSpan ShowTime => Showtime?.Time ?? TimeSpan.Zero;
        public DateTime ShowDate => Showtime?.Date ?? DateTime.MinValue;
        public string Format => Showtime?.Format ?? "-";
        public string SeatDisplay => SeatNumber.ToString();
        public bool IsVip => LoungeAccess == true || ComplementarySnacks == true || SeatNumber > 60;

        public override decimal GetPrice() => IsVip ? 250m : 150m;
        public decimal Price => GetPrice();

        // Властивість для відображення деталей для DataGrid
        public string Details
        {
            get
            {
                string status = IsCanceled ? "[СКАСОВАНО] " : "";
                string paid = IsPaid ? "[ОПЛАЧЕНО]" : "";
                string type = IsVip ? "VIP-місце" : "Стандарт";
                string lounge = LoungeAccess == true ? "Так " : "Ні";
                string snacks = ComplementarySnacks == true ? "Так" : "Ні";
                string wishes = string.IsNullOrWhiteSpace(AdditionalWishes) ? "" : AdditionalWishes;
                return $"{status}{paid} Лаундж: {lounge} | Закуски: {snacks} | Побажання: {wishes}";
            }
        }
        // Реалізація ICancellable
        public void Cancel()
        {
            IsCanceled = true;
        }
        // Метод для отримання повної інформації про бронювання
        public override string GetReservationDetails()
        {
            return $"{MovieTitle} | {ShowDate:dd.MM.yyyy} {ShowTime:hh\\:mm} | Місце: {SeatDisplay} | {Format}";
        }
    }
}
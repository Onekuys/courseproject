using System;

namespace OOPWPFProject
{
    public class Reservation
    {
        public int Id { get; set; }
        public int ShowtimeId { get; set; }
        public Showtime Showtime { get; set; }
        public int SeatNumber { get; set; }
        public bool IsVip => SeatNumber > 60;
        public bool IsCanceled { get; set; }
        public bool? LoungeAccess { get; set; }
        public bool? ComplementarySnacks { get; set; }
        public string? AdditionalWishes { get; set; }
        public string Details
        {
            get
            {
                string status = IsCanceled ? "[СКАСОВАНО] " : "";
                string type = IsVip ? "VIP-місце" : "Стандарт";
                string wishes = string.IsNullOrWhiteSpace(AdditionalWishes) ? "-" : AdditionalWishes;
                return $"{status}Тип: {type}, Закуски: ({(ComplementarySnacks == true ? "Так" : "Ні")}, Дод. побажання: {wishes})";
            }
        }
    }
}
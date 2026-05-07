using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOPWPFProject
{
    public abstract class AbstractReservation
    {
        public string MovieTitle { get; set; }
        public TimeSpan ShowTime { get; set; }
        public int SeatNumber { get; set; }
        public string Format { get; set; }
        public bool IsCanceled { get; set; }

    }
    
    
    
    public class Reservation : AbstractReservation, ICancelable
    {

        public Reservation(string movieTitle, TimeSpan showtime, int seatNumber, string format, bool isCanceled = false)
        {
            if (string.IsNullOrWhiteSpace(movieTitle)) throw new ArgumentException("Назва фільму не може бути порожньою.");
            if (seatNumber < 1) throw new ArgumentException("Номер місця повинен бути більше 0.");
            MovieTitle = movieTitle;
            ShowTime = showtime;
            SeatNumber = seatNumber;
            Format = format;
            IsCanceled = isCanceled;
        }

        public virtual string GetReservationDetails()
        {
            return IsCanceled ? $"[СКАСОВАНО] Формат: {Format}" 
                : $"Формат: {Format}";
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

        public VIPReservation(string title, TimeSpan time, int seat, string format, bool lounge, bool snacks, string wishes, bool isCanceled = false): base(title, time, seat, format, isCanceled)
        {
            LoungeAccess = lounge;
            ComplementarySnacks = snacks;
            AdditionalWishes = wishes;
        }
        public override string GetReservationDetails()
        {
            string lounge = LoungeAccess ? "Так" : "Ні";
            string snacks = ComplementarySnacks ? "Так" : "Ні";
            return IsCanceled ? $"[СКАСОВАНО] Формат: {Format}, VIP-зона: {LoungeAccess}, Закуски: {ComplementarySnacks}, Додатково:{AdditionalWishes}" : $"Формат: {Format}, VIP-зона: {LoungeAccess}, Закуски: {ComplementarySnacks}, Додатково:{AdditionalWishes}";
        }
    }

    
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using OOPWPFProject.Models;
using System.Threading.Tasks;

namespace OOPWPFProject.ViewModels
{
    public class ReservationViewModel : INotifyPropertyChanged
    {
        private readonly Reservation reservation;

        public ReservationViewModel(Reservation r)
        {
            reservation = r;
        }

        public Reservation Source => reservation;
        public int Id => reservation.Id;
        public string MovieTitle => reservation.Showtime?.Movie?.Title ?? "-";
        public TimeSpan ShowTime => reservation.ShowTime;
        public string Format => reservation.Showtime?.Format ?? "-";
        public int SeatNumber => reservation.SeatNumber;
        public string SeatDisplay => $"{reservation.SeatDisplay}";
        public bool IsVip => reservation.IsVip;
        public string TypeLabel => reservation.IsVip ? "VIP" : "Стандарт";
        public decimal Price => reservation.Price;
        public bool IsCanceled
        {
            get => reservation.IsCanceled;
            set
            {
                if (reservation.IsCanceled != value)
                {
                    reservation.IsCanceled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Details));
                }
            }
        }
        public bool IsPaid
        {
            get => reservation.IsPaid;
            set
            {
                if (reservation.IsPaid != value)
                {
                    reservation.IsPaid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PaidLabel));
                }
            }
        }
        public string Details => reservation.Details;
        public bool? LoungeAccess => reservation.LoungeAccess;
        public bool? ComplementarySnacks => reservation.ComplementarySnacks;
        public string? AdditionalWishes => reservation.AdditionalWishes;
        public string LoungeDisplay => reservation.LoungeAccess == true ? "Так" : "Ні";
        public string SnacksDisplay => reservation.ComplementarySnacks == true ? "Так" : "Ні";

        public string PaidLabel => reservation.IsPaid ? "Оплачено" : "Очікує оплати";
        
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

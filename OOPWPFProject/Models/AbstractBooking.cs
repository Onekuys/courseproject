using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OOPWPFProject.Helpers;

namespace OOPWPFProject.Models
{
    // Абстрактний базовий клас для бронювань
    public abstract class BookingBase : ICancelable
    {
        // Спільні поля для всіх типів бронювань
        public int Id { get; set; }
        public bool IsCanceled { get; set; }
        public int? UserId { get; set; }

        // Спільна реалізація Cancel()
        public virtual void Cancel()
        {
            IsCanceled = true;
        }

        // Абстрактні методи
        public abstract decimal GetPrice();
        public abstract string GetReservationDetails();
    }
}

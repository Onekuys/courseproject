using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace OOPWPFProject
{
    public class EntityManager
    {
        private readonly CinemaDbContext _db;
        public EntityManager()
        {
            _db = new CinemaDbContext();
        }
        public void AddReservation(Reservation reservation)
        {
            _db.Reservations.Add(reservation);
            _db.SaveChanges();
        }
        public void CancelReservation(Reservation reservation)
        {
            reservation.IsCanceled = true;
            _db.SaveChanges();
        }
        public List<Reservation> GetAllReservations()
        {
            return _db.Reservations.Include(r => r.Showtime).ThenInclude(s => s.Movie).Where(r => !r.IsCanceled).ToList();
        }
    }
}
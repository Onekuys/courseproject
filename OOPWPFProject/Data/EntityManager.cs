using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OOPWPFProject.Models;

namespace OOPWPFProject.Data
{
    public class EntityManager
    {
        private readonly CinemaDbContext _db;

        public EntityManager()
        {
            _db = new CinemaDbContext();
            _db.Database.EnsureCreated();
            SeedData();
        }

        // SEED - текстові поля
        private void SeedData()
        {
            if (_db.Movies.Any()) return; // Вже є дані в БД

            var movies = new List<Movie>()
            {
                new Movie { Title = "Дюна", DurationMinutes = 166 },
                new Movie { Title = "Тачки", DurationMinutes = 117},
                new Movie { Title = "Назад у майбутнє", DurationMinutes = 116}
            };
            _db.Movies.AddRange(movies);
            _db.SaveChanges();

            var today = DateTime.Today;
            var showtimes = new List<Showtime>
            {
                new Showtime { MovieId = movies[0].Id, Date = today, Time = new TimeSpan(9, 50, 0), Format = "IMAX" },
                new Showtime { MovieId = movies[0].Id, Date = today, Time = new TimeSpan(14, 20, 0), Format = "2D" },
                new Showtime { MovieId = movies[1].Id, Date = today, Time = new TimeSpan(12, 15, 0), Format = "2D" },
                new Showtime { MovieId = movies[1].Id, Date = today, Time = new TimeSpan(17, 10, 0), Format = "3D" },
                new Showtime { MovieId = movies[2].Id, Date = today, Time = new TimeSpan(11, 0, 0), Format = "3D" },
                new Showtime { MovieId = movies[2].Id, Date = today, Time = new TimeSpan(16, 30, 0), Format = "IMAX" },
            };
            _db.Showtimes.AddRange(showtimes);
            _db.SaveChanges();
        }

        //--------------
        // MOVIES
        //--------------
        public List<Movie> GetAllMovies()
        {
            return _db.Movies.OrderBy(m => m.Title).ToList();
        }
        public void AddMovie(Movie movie)
        {
            _db.Movies.Add(movie);
            _db.SaveChanges();
        }

        public List<Showtime> GetShowtimesForMovie(int movieId, DateTime date)
        {
            return _db.Showtimes.Where(s => s.MovieId == movieId && s.Date.Date == date.Date).AsEnumerable().OrderBy(s => s.Time).ToList();
        }

        //--------------
        // SHOWTIMES
        //--------------
        public void AddShowtime(Showtime showtime)
        {
            _db.Showtimes.Add(showtime);
            _db.SaveChanges();
        }

        public List<int> GetReservedSeatsForShowtime(int showtimeId)
        {
            return _db.Reservations.Where(r => r.ShowtimeId == showtimeId && !r.IsCanceled).Select(r => r.SeatNumber).ToList();
        }
        public List<Showtime> GetAllShowtimes()
        {
            return _db.Showtimes.Include(s => s.Movie).OrderBy(s => s.Movie).AsEnumerable().OrderBy(s => s.Date).ThenBy(s => s.Time).ToList();
        }

        //--------------
        // RESERVATIONS
        //--------------
        public void AddReservation(IEnumerable<Reservation> reservation)
        {
            _db.Reservations.AddRange(reservation);
            _db.SaveChanges();
        }
        public void AddReservations(IEnumerable<Reservation> reservations)
        {
            _db.Reservations.AddRange(reservations);
            _db.SaveChanges();
        }


        public void RemoveReservation(Reservation reservation)
        {
            var connected = _db.Reservations.Find(reservation.Id);
            if (connected != null)
            {
                _db.Reservations.Remove(connected);
                _db.SaveChanges();
            }
        }
        public void CancelReservation(Reservation reservation) 
        {
            var connected = _db.Reservations.Find(reservation.Id);
            if (connected != null)
            {
                connected.IsCanceled = true;
                _db.SaveChanges();
            }
        }
        public void PayReservation(Reservation reservation)
        {
            var paidRes = _db.Reservations.Find(reservation.Id);
            if (paidRes == null) return;
            paidRes.IsPaid = true;
            _db.SaveChanges();
        }


        public List<Reservation> GetAllReservations() 
        {
            return _db.Reservations.Include(r => r.Showtime).ThenInclude(s => s.Movie).ToList();
        }

        public List<Reservation> GetAllReservations(DateTime date)
        {   
            return _db.Reservations.Include(r => r.Showtime).ThenInclude(s => s.Movie).Where(r => r.Showtime.Date.Date == date.Date).AsEnumerable().OrderBy(r => r.Showtime.Time).ToList();
        }

        public List<Reservation> GetReservationsByUser(int userId)
        {
            return _db.Reservations.Include(r => r.Showtime).ThenInclude(s => s.Movie).Where(r => r.UserId == userId).AsEnumerable().OrderByDescending(r => r.Showtime.Date).ThenBy(r => r.Showtime.Time).ToList();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using OOPWPFProject.Data;
using OOPWPFProject.Helpers;
using OOPWPFProject.Models;
using OOPWPFProject.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Navigation;

namespace OOPWPFProject.Data
{
    public class EntityManager
    {
        private readonly CinemaDbContext _db;
        private readonly UserRepository _userRepository = new();

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
                new Movie { Title = "Назад у майбутнє", DurationMinutes = 116},
                new Movie { Title = "Оппенгеймер", DurationMinutes = 180},
                new Movie { Title = "Майкл", DurationMinutes = 127}
            };
            _db.Movies.AddRange(movies);
            _db.SaveChanges();

            var today = DateTime.Today;
            var showtimes = new List<Showtime>
            {
                new Showtime { MovieId = movies[0].Id, Date = today, Time = new TimeSpan(9, 50, 0), Format = "2D" },
                new Showtime { MovieId = movies[0].Id, Date = today, Time = new TimeSpan(14, 30, 0), Format = "3D" },
                new Showtime { MovieId = movies[0].Id, Date = today, Time = new TimeSpan(18, 20, 0), Format = "IMAX" },
                new Showtime { MovieId = movies[0].Id, Date = new DateTime(2026, 5, 27), Time = new TimeSpan(18, 20, 0), Format = "IMAX" },

                new Showtime { MovieId = movies[1].Id, Date = today, Time = new TimeSpan(12, 15, 0), Format = "2D" },
                new Showtime { MovieId = movies[1].Id, Date = today, Time = new TimeSpan(15, 10, 0), Format = "3D" },
                new Showtime { MovieId = movies[1].Id, Date = today, Time = new TimeSpan(18, 5, 0), Format = "2D" },

                new Showtime { MovieId = movies[2].Id, Date = today, Time = new TimeSpan(10, 0, 0), Format = "2D" },
                new Showtime { MovieId = movies[2].Id, Date = today, Time = new TimeSpan(14, 50, 0), Format = "3D" },
                new Showtime { MovieId = movies[2].Id, Date = today, Time = new TimeSpan(18, 20, 0), Format = "3D" },

                new Showtime { MovieId = movies[3].Id, Date = today, Time = new TimeSpan(11, 0, 0), Format = "3D" },
                new Showtime { MovieId = movies[3].Id, Date = today, Time = new TimeSpan(16, 30, 0), Format = "IMAX" },
                new Showtime { MovieId = movies[3].Id, Date = today, Time = new TimeSpan(20, 0, 0), Format = "3D" },

                new Showtime { MovieId = movies[4].Id, Date = today, Time = new TimeSpan(9, 30, 0), Format = "2D" },
                new Showtime { MovieId = movies[4].Id, Date = today, Time = new TimeSpan(12, 50, 0), Format = "3D" },
                new Showtime { MovieId = movies[4].Id, Date = today, Time = new TimeSpan(16, 10, 0), Format = "2D" },
            };
            _db.Showtimes.AddRange(showtimes);
            _db.SaveChanges();
        }
        //-------------
        //    USER
        //-------------
        private ReservationViewModel Enrich(Reservation r)
        {
            if (!r.UserId.HasValue)
                return new ReservationViewModel(r);

            var user = _userRepository.GetAll().FirstOrDefault(u => u.Id == r.UserId.Value);
            return user == null ? new ReservationViewModel(r) : new ReservationViewModel(r, user.FullName, user.Phone, user.Email);
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
            return _db.Showtimes
                .Where(s => s.MovieId == movieId && s.Date.Date == date.Date)
                .AsEnumerable().OrderBy(s => s.Time).ToList();
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
            return _db.Reservations
                .Where(r => r.ShowtimeId == showtimeId && !r.IsCanceled)
                .Select(r => r.SeatNumber).ToList();
        }
        public List<Showtime> GetAllShowtimes()
        {
            return _db.Showtimes
                .Include(s => s.Movie).OrderBy(s => s.Movie)
                .AsEnumerable().OrderBy(s => s.Date).ThenBy(s => s.Time).ToList();
        }

        // [ADMIN] Отримати сеанси за датою для таблиці керування
        public List<Showtime> GetShowtimesForDate(DateTime date)
        {
            return _db.Showtimes
                .Include(s => s.Movie)
                .Include(s => s.Reservations)
                .Where(s => s.Date.Date == date.Date)
                .AsEnumerable()
                .OrderBy(s => s.Time)
                .ToList();
        }

        // [ADMIN] Видалення сеансу. Якщо є активні бронювання - кидає виняток.
        public void RemoveShowtime(Showtime showtime)
        {
            var connected = _db.Showtimes
                .Include(s => s.Reservations)
                .FirstOrDefault(s => s.Id == showtime.Id);

            if (connected == null) return;

            bool hasActiveReservations = connected.Reservations.Any(r => !r.IsCanceled);
            if (hasActiveReservations)
                throw new InvalidOperationException(
                    "Не можна видалити сеанс: є активні бронювання. Спочатку скасуйте їх.");

            _db.Showtimes.Remove(connected);
            _db.SaveChanges();
        }

        // [ADMIN] Оновлення часу та формату сеансу
        public void UpdateShowtime(Showtime showtime, TimeSpan newTime, string newFormat)
        {
            var connected = _db.Showtimes.Find(showtime.Id);
            if (connected == null)
                throw new InvalidOperationException("Сеанс не знайдено.");

            // Перевірка дубліката (виключаємо себе)
            bool duplicate = _db.Showtimes.Any(s =>
                s.Id != showtime.Id &&
                s.MovieId == showtime.MovieId &&
                s.Date.Date == showtime.Date.Date &&
                s.Time == newTime);

            if (duplicate)
                throw new InvalidOperationException("Сеанс для цього фільму в цей час вже існує.");

            connected.Time = newTime;
            connected.Format = newFormat;
            _db.SaveChanges();
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
        public void CancelReservation(ICancelable item)
        {
            if (item is not Reservation reservation) return;
            var connected = _db.Reservations.Find(reservation.Id);
            if (connected != null)
            {
                connected.IsCanceled = true;
                connected.Cancel();
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
            return _db.Reservations
                .Include(r => r.Showtime)
                .ThenInclude(s => s.Movie)
                .ToList();
        }

        public List<Reservation> GetAllReservations(DateTime date)
        {
            _db.ChangeTracker.Clear();
            return _db.Reservations
                .Include(r => r.Showtime).ThenInclude(s => s.Movie)
                .Where(r => r.Showtime.Date.Date == date.Date)
                .AsEnumerable()
                .OrderBy(r => r.Showtime.Time)
                .ThenBy(r => r.SeatNumber)
                .ToList();
        }

        public List<Reservation> GetReservationsByUser(int userId)
        {
            return _db.Reservations
                .Include(r => r.Showtime)
                .ThenInclude(s => s.Movie)
                .Where(r => r.UserId == userId)
                .AsEnumerable()
                .OrderByDescending(r => r.Showtime.Date)
                .ThenBy(r => r.Showtime.Time)
                .ToList();
        }

        //----------------
        //   STATISTICS
        //----------------
        // Методи

        public AdminStats GetAdminStats(DateTime date)
        {
            _db.ChangeTracker.Clear();
            // Витягуємо всі бронювання на вказану дату
            var reservations = _db.Reservations.Include(r => r.Showtime).ThenInclude(s => s.Movie).Where(r => r.Showtime.Date.Date == date.Date).ToList();

            var active = reservations.Where(r => !r.IsCanceled).ToList();

            // Створюємо топ фільмів за кількістю проданих квитків та загальним доходом
            var revenueMap = new Dictionary<string, (int ticketsCount, decimal TotalRevenue)>();
            foreach (var r in active)
            {
                var title = r.Showtime.Movie.Title;
                decimal ticketPrice = r.SeatNumber > 60 ? 250m : 150m;

                if (revenueMap.ContainsKey(title))
                {
                    var current = revenueMap[title];
                    revenueMap[title] = (current.ticketsCount + 1, current.TotalRevenue + ticketPrice);
                }
                else
                {
                    revenueMap[title] = (1, ticketPrice);
                }
            }

            // Створюємо список для відображення в UI
            var movieRevenues = new List<MovieRevenue>();
            foreach (var pair in revenueMap)
            {
                movieRevenues.Add(new MovieRevenue(pair.Key, pair.Value.ticketsCount, pair.Value.TotalRevenue));
            }

            movieRevenues.Sort((a, b) => b.Revenue.CompareTo(a.Revenue));


            // Фільтруємо сеанси на вказану дату
            var selectedDate = date.Date;
            var showtimes = _db.Showtimes.Include(s => s.Movie).Where(s => s.Date.Date == selectedDate).ToList();

            // Рахуємо відсоток проданих місць для кожного сеансу
            var showtimePercentBought = new List<ShowtimePercentBought>();
            foreach (var s in showtimes)
            {
                int reserved = 0;
                foreach (var r in reservations)
                {
                    if (r.ShowtimeId == s.Id && !r.IsCanceled) reserved++;
                }

                double percent = Math.Round(reserved * 100.0 / 70, 1);

                showtimePercentBought.Add(new ShowtimePercentBought(s.Movie.Title, s.Time, s.Format, reserved, 70, percent));
            }
            // Сортуємо сеанси за відсотком проданих місць
            showtimePercentBought.Sort((a, b) => b.Percent.CompareTo(a.Percent));


            // Рахуємо загальний дохід, кількість VIP та оплачений квитків
            decimal totalRevenue = 0m;
            decimal vipRevenue = 0m;
            decimal paidRevenue = 0m;
            int vipTickets = 0;
            int paidTickets = 0;

            foreach (var r in active)
            {
                decimal price = r.SeatNumber > 60 ? 250m : 150m;
                totalRevenue += price;

                if (r.SeatNumber > 60)
                {
                    vipRevenue += price;
                    vipTickets++;
                }
                if (r.IsPaid)
                {
                    paidRevenue += price;
                    paidTickets++;
                }
            }

            double avgCapacity = showtimes.Count == 0 ? 0 : Math.Round(active.Count * 100.0 / (showtimes.Count * 70), 1);

            return new AdminStats(
                TotalRevenue: totalRevenue, 
                VipRevenue: vipRevenue,
                PaidRevenue: paidRevenue,
                TotalTickets: active.Count,
                VipTickets: vipTickets,
                PaidTickets: paidTickets,
                CanceledCount: reservations.Count - active.Count,
                AvgCapacity: avgCapacity,
                MovieRevenues: movieRevenues,
                ShowtimeLoads: showtimePercentBought
            );
        }

    }

}
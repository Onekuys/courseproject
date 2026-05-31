using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OOPWPFProject.Models;
using OOPWPFProject.Data;
using OOPWPFProject.Helpers;

namespace OOPWPFProject.Views
{
    // Логіка для вікна перегляду сеансів
    public partial class BrowseWindow : Window
    {
        private readonly EntityManager _manager = new();
        private List<ShowtimeDisplay> allShowtimes = new();

        public BrowseWindow()
        {
            InitializeComponent();
            LoadShowtimes();
            RefreshUserUI();
        }

        // Завантажує сеанси та оновлює фільтри
        private void LoadShowtimes()
        {
            var showtimes = _manager.GetAllShowtimes();
            allShowtimes = showtimes.Select(s => new ShowtimeDisplay(s,
                70 - _manager.GetReservedSeatsForShowtime(s.Id).Count)).ToList();

            var movies = allShowtimes.Select(s => s.Movie).DistinctBy(m => m.Id).ToList();
            movies.Insert(0, new Movie { Id = 0, Title = "Усі фільми" });
            MovieFilter.ItemsSource = movies;
            MovieFilter.SelectedIndex = 0;

            ApplyFilter();
        }

        // Застосовує вибрані фільтри до списку сеансів
        private void ApplyFilter()
        {
            if (MovieFilter == null || DateInput == null || ShowtimesGrid == null) return;

            var filtered = allShowtimes.AsEnumerable();

            if (DateInput.SelectedDate.HasValue)
            {
                filtered = filtered.Where(s => s.Date.Date == DateInput.SelectedDate.Value.Date);
            }
            if (MovieFilter.SelectedItem is Movie movie && movie.Id != 0)
            {
                filtered = filtered.Where(s => s.Movie?.Id == movie.Id);
            }

            ShowtimesGrid.ItemsSource = filtered.ToList();
            StatusBar.Text = $"Знайдено сеансів: {filtered.Count()}";
        }

        // Оновлює інтерфейс користувача залежно від статусу входу
        public void RefreshUserUI()
        {
            var user = App.CurrentUser;
            if (user == null)
            {
                UserLabel.Text = "Гість";
                LoginBtn.Visibility = System.Windows.Visibility.Visible;
                AdminBtn.Visibility = System.Windows.Visibility.Collapsed;
                MyTicketsBtn.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (user.Role == "admin")
            {
                UserLabel.Text = $"Адм. {user.FullName}";
                LoginBtn.Content = "Вийти";
                LoginBtn.Visibility = System.Windows.Visibility.Visible;
                AdminBtn.Visibility = System.Windows.Visibility.Visible;
                MyTicketsBtn.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                UserLabel.Text = $"Кор. {user.FullName}";
                LoginBtn.Content = "Вийти";
                LoginBtn.Visibility = System.Windows.Visibility.Visible;
                AdminBtn.Visibility = System.Windows.Visibility.Collapsed;
                MyTicketsBtn.Visibility = System.Windows.Visibility.Visible;
            }
        }

        // Обробники подій для фільтрів та кнопок
        private void DateInput_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
        private void MovieFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();
        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DateInput.SelectedDate = DateTime.Today;
            MovieFilter.SelectedIndex = 0;
        }

        private void ShowtimesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BookBtn.IsEnabled = ShowtimesGrid.SelectedItem is ShowtimeDisplay;
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser != null)
            {
                App.CurrentUser = null;
                LoginBtn.Content = "Увійти/Реєстрація";
                RefreshUserUI();
                return;
            }
            var window = new LoginWindow();
            window.Owner = this;
            window.ShowDialog();
            RefreshUserUI();
        }
        private void AdminBtn_Click(object sender, RoutedEventArgs e)
        {
            var admin = new MainWindow();
            admin.Show();
        }

        private void MyTicketsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;
            var window = new ClientWindow(App.CurrentUser);
            window.Owner = this;
            window.ShowDialog();
        }

        // Обробник для кнопки бронювання, який перевіряє вибір сеансу та статус користувача
        private void BookBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ShowtimesGrid.SelectedItem is not ShowtimeDisplay sd) return;

            if (App.CurrentUser == null)
            {
                var login = new LoginWindow();
                login.Owner = this;
                login.ShowDialog();
                RefreshUserUI();

                if (App.CurrentUser == null) return;
            }

            var booking = new AddReservationWindow(new EntityManager(), sd.Source, App.CurrentUser);
            booking.Owner = this;
            booking.ShowDialog();
            LoadShowtimes();
        }

        // Внутрішній клас для відображення інформації про сеанс разом з кількістю вільних місць
        public class ShowtimeDisplay
        {
            public Showtime Source { get; }
            public Movie Movie => Source.Movie;
            public DateTime Date => Source.Date;
            public TimeSpan Time => Source.Time;
            public string Format => Source.Format;
            public int FreeSeats { get; }

            public ShowtimeDisplay(Showtime source, int freeSeats)
            {
                Source = source;
                FreeSeats = freeSeats;
            }

        }
    }

}

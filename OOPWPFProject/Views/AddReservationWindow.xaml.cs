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
    public partial class AddReservationWindow : Window
    {

        private readonly EntityManager _manager;
        private readonly User? _currentUser;

        private const int ROWS = 7;
        private const int COLUMNS = 10;
        private const int SEATS_PER_ROW = 10;
        private const int TOTAL_SEATS = ROWS * SEATS_PER_ROW;


        private readonly HashSet<int> _selectedSeats = new();
        private List<int> _reservedSeats = new();

        private readonly Dictionary<int, Button> _seatButtons = new();

        public AddReservationWindow(EntityManager manager, Showtime? preselectedShowtime = null, User? currentUser = null)
        {
            InitializeComponent();
            _manager = manager;
            _currentUser = currentUser;
            LoadMovies();
            BuildSeatGrid();

            if (preselectedShowtime != null) 
            {
                PreloadShowtime(preselectedShowtime);
            }
        }

        private void PreloadShowtime(Showtime showtime)
        {
            DateInput.SelectedDate = showtime.Date;
        
            var movies = MovieComboBox.ItemsSource as List<Movie>;
            var movie = movies?.FirstOrDefault(m => m.Id == showtime.MovieId);
            if (movie != null) MovieComboBox.SelectedItem = movie;

            MovieComboBox.SelectedItem = movie;
            LoadShowtimes();

            Dispatcher.InvokeAsync(() =>
            {
                var items = ShowtimeComboBox.ItemsSource as List<Showtime>;
                var st = items?.FirstOrDefault(s => s.Id == showtime.Id);
                if (st == null) return;

                ShowtimeComboBox.SelectedItem = st;

                _reservedSeats = _manager.GetReservedSeatsForShowtime(st.Id) ?? new List<int>();
                RefreshSeatStyles();
                UpdateSelectionLabel();
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void LoadMovies()
        {
            MovieComboBox.ItemsSource = _manager.GetAllMovies();
        }

        // ---------------------
        // Створення схеми залу
        // ---------------------

        private void BuildSeatGrid()
        {
            _seatButtons.Clear();
            var rows = new List<UIElement>();

            for (int row = 0; row < ROWS; row++)
            {
                var rowPanel = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 2, 0, 2)
                };

                var rowLabel = new TextBlock
                {
                    Text = $"{row+1} Ряд",
                    Width = 60,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontWeight = FontWeights.SemiBold,
                    TextAlignment = TextAlignment.Center,
                };
                rowPanel.Children.Add(rowLabel);

                bool isLastRow = (row == ROWS - 1);

                for (int seat = 1; seat <= SEATS_PER_ROW; seat++)
                {
                    int seatNumber = row * SEATS_PER_ROW + seat;

                    Style seatStyle = isLastRow ? (FindResource("SeatVIP") as Style) : (FindResource("SeatFree") as Style);

                    var seatBtn = new Button
                    {
                        Content = seat.ToString(),
                        Tag = seatNumber,
                        Style= seatStyle
                    };
                    seatBtn.Click += SeatButton_Click;
                    _seatButtons.Add(seatNumber, seatBtn);
                    rowPanel.Children.Add(seatBtn);
                }
                rows.Add(rowPanel);
            }
            SeatsPanel.ItemsSource = rows;
        }

        private void SeatButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            int seatNumber = (int)btn.Tag;
            if (_reservedSeats.Contains(seatNumber)) return;

            if (_selectedSeats.Contains(seatNumber))
            {
                // Знімаємо вибір
                _selectedSeats.Remove(seatNumber);
                btn.Style = GetDefaultStyle(seatNumber);
            }
            else
            {
                // Додаємо вибір
                _selectedSeats.Add(seatNumber);
                btn.Style = FindResource("SeatSelected") as Style;
            }

            UpdateSelectionLabel();
            UpdateVipOptionsViability();
        }

        private Style? GetDefaultStyle(int seatNumber) => seatNumber > 60 ? FindResource("SeatVIP") as Style : FindResource("SeatFree") as Style;


        private void RefreshSeatStyles()
        {
            foreach (var (num, btn) in _seatButtons)
            {
                if (_reservedSeats.Contains(num))
                    btn.Style = FindResource("SeatTaken") as Style;
                else if (_selectedSeats.Contains(num))
                    btn.Style = FindResource("SeatSelected") as Style;
                else
                    btn.Style = GetDefaultStyle(num);
            }
        }

        private void UpdateSelectionLabel()
        {
            if (_selectedSeats.Count == 0)
            {
                SelectedSeatLabel.Text = "Місця не обрано";
                SelectedSeatLabel.Foreground = System.Windows.Media.Brushes.Gray;
                SubmitButton.IsEnabled = false;
            }
            else
            {
                var nums = string.Join(", ", _selectedSeats.OrderBy(s => s));
                decimal total = _selectedSeats.Sum(s => s > 60 ? 250m : 150m);
                SelectedSeatLabel.Text = $"Обрано місця: {nums} | Сума: {total} грн";
                SelectedSeatLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
                SubmitButton.IsEnabled = true;
            }
        }

        private void UpdateVipOptionsViability()
        {
            bool hasVip = _selectedSeats.Any(s => s > 60);
            LoungeYes.IsEnabled = hasVip;
            LoungeNo.IsEnabled = hasVip;
            WishesInput.IsEnabled = hasVip;
            if (!hasVip) LoungeNo.IsChecked = true;

        }


        // ---------------------------
        // Каскадне оновлення ComboBox
        // ---------------------------

        private void MovieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ResetShowtimeAndSeat();

            if (MovieComboBox.SelectedItem is Movie && DateInput.SelectedDate.HasValue) LoadShowtimes();
        }
        private void DateInput_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ResetShowtimeAndSeat();

            if (MovieComboBox.SelectedItem is not Movie) return;
            if (DateInput.SelectedDate.HasValue) LoadShowtimes();
        }

        private void LoadShowtimes()
        {
            var movie = (Movie)MovieComboBox.SelectedItem;
            var date = DateInput.SelectedDate!.Value;

            var showtimes = _manager.GetShowtimesForMovie(movie.Id, date) ?? new List<Showtime>();

            if (showtimes == null)
            {
                showtimes = new List<Showtime>();
            }

            ShowtimeComboBox.ItemsSource = showtimes;

            if (showtimes.Count > 0) ShowtimeComboBox.IsEnabled = true;
        }

        private void ShowtimeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            ResetSeatSelection();

            if (ShowtimeComboBox.SelectedItem is not Showtime showtime) return;

            _reservedSeats = _manager.GetReservedSeatsForShowtime(showtime.Id) ?? new List<int>();
            RefreshSeatStyles();
            SeatsPanel.IsEnabled = true;
        }


        // -------------
        // Reset методи
        // -------------

        private void ResetShowtimeAndSeat()
        {
            ShowtimeComboBox.ItemsSource = null;
            ShowtimeComboBox.IsEnabled = false;
            ResetSeatSelection();
        }
        private void ResetSeatSelection()
        {
            _selectedSeats.Clear();
            _reservedSeats = new List<int>();
            SeatsPanel.IsEnabled = false;
            SelectedSeatLabel.Text = "Місце не обрано";
            SelectedSeatLabel.Foreground = System.Windows.Media.Brushes.Gray;
            UpdateVipOptionsViability();
            SubmitButton.IsEnabled = false;
            RefreshSeatStyles();
        }


        // ---------------------
        // Збереження бронювання
        // ---------------------


        private void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            if (ShowtimeComboBox.SelectedItem is not Showtime showtime || _selectedSeats.Count == 0)
            {
                MessageBox.Show("Оберіть сеанс та хоча б одне місце.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool hasLounge = LoungeYes.IsChecked == true;
            bool hasSnacks = SnacksYes.IsChecked == true;
            string? wishes = string.IsNullOrWhiteSpace(WishesInput.Text) ? null : WishesInput.Text;
            int? userId = _currentUser?.Id;

            try
            {
                var reservations = _selectedSeats.OrderBy(s => s).Select(seatNum => new Reservation(showtime, seatNum, hasLounge, hasSnacks, wishes, userId)).ToList();
                _manager.AddReservations(reservations);
                foreach(var r in reservations) Logger.Log("Додано", r.GetReservationDetails());
                string seats = string.Join(", ", _selectedSeats.OrderBy(s => s));
                decimal total = reservations.Sum(r => r.Price);

                MessageBox.Show($"Заброньовано місця: {seats}\nЗагальна сума: {total} грн", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                ResetSeatSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            MovieComboBox.SelectedIndex = -1;
            DateInput.SelectedDate = DateTime.Today;
            ShowtimeComboBox.ItemsSource = null;
            ShowtimeComboBox.IsEnabled = false;
            WishesInput.Clear();
            LoungeNo.IsChecked = true;
            SnacksNo.IsChecked = true;
            ResetSeatSelection();
        }
    }
}

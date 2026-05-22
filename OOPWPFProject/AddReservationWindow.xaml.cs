using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OOPWPFProject
{
    public partial class AddReservationWindow : Window
    {

        private readonly EntityManager _manager;

        private const int ROWS = 7;
        private const int COLUMNS = 10;
        private const int SEATS_PER_ROW = 10;
        private const int TOTAL_SEATS = ROWS * SEATS_PER_ROW;


        private int? _selectedSeat = null;
        private List<int> _reservedSeats = new();

        private readonly Dictionary<int, Button> _seatButtons = new();

        public AddReservationWindow(EntityManager manager)
        {
            InitializeComponent();
            _manager = manager;
            LoadMovies();
            BuildSeatGrid();
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
            if (sender is not Button seat) return;
            int seatNumber = (int)seat.Tag;

            // Змінюємо стиль попередньо вибраного місця назад на вільний/зайнятий
            if (_selectedSeat.HasValue && _seatButtons.TryGetValue(_selectedSeat.Value, out var previous)){
                if (_reservedSeats.Contains(_selectedSeat.Value))
                {
                    previous.Style = FindResource("SeatTaken") as Style;
                }
                else if (_selectedSeat.Value >= 61 && _selectedSeat.Value <= 70)
                {
                    previous.Style = FindResource("SeatVIP") as Style; 
                }
                else
                {
                    previous.Style = FindResource("SeatFree") as Style;

                }
            }
            // Якщо натиснули на вже вибране місце - знімаємо вибір
            if (_selectedSeat == seatNumber)
            {
                _selectedSeat = null;
                SelectedSeatLabel.Text = "Місце не обрано";
                SelectedSeatLabel.Foreground = System.Windows.Media.Brushes.Gray;
                SubmitButton.IsEnabled = false;
                UpdateVipOptionsViability();
                return;
            }

            _selectedSeat = seatNumber;
            seat.Style = FindResource ("SeatSelected") as Style;

            int row = (seatNumber - 1) / SEATS_PER_ROW;
            int column = (seatNumber - 1) % SEATS_PER_ROW + 1;
            string rowLetter = $"{row + 1} Ряд";
            SelectedSeatLabel.Text = $"Обрано: {rowLetter}, Місце {column} (#{seatNumber})";

            SelectedSeatLabel.Foreground = System.Windows.Media.Brushes.DarkGreen;
            SubmitButton.IsEnabled = true;

            UpdateVipOptionsViability();
        }

        private void RefreshSeatStyles()
        {
            foreach (var (seatNumber, btn) in _seatButtons)
            {
                if (seatNumber == _selectedSeat)
                {
                    btn.Style = FindResource("SeatSelected") as Style;
                }
                else if (_reservedSeats.Contains(seatNumber))
                {
                    btn.Style = FindResource("SeatTaken") as Style;
                }
                else
                {
                    if (seatNumber >= 61 && seatNumber <=70) btn.Style = FindResource("SeatVIP") as Style;
                    else btn.Style = FindResource("SeatFree") as Style;
                }
                UpdateVipOptionsViability();
            }
        }

        private void UpdateVipOptionsViability()
        {
            bool isVipSeat = _selectedSeat.HasValue && _selectedSeat.Value > 60 && _selectedSeat.Value < 71;
            bool isVipSnacks = SnacksYes.IsChecked == true;

            LoungeYes.IsEnabled = isVipSeat;
            LoungeNo.IsEnabled = isVipSeat;

            if (!isVipSeat)
            {
                LoungeNo.IsChecked = true;
            }
        }


        // ---------------------------
        // Каскадне оновлення ComboBox
        // ---------------------------

        private void MovieComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ResetShowtimeAndSeat();

            if (MovieComboBox.SelectedItem is not Movie) return;

            if (DateInput.SelectedDate.HasValue) LoadShowtimes();
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
            Movie movie = (Movie)MovieComboBox.SelectedItem;
            DateTime date = DateInput.SelectedDate!.Value;


            var showtimes = _manager.GetShowtimesForMovie(movie.Id, date);

            if (showtimes == null)
            {
                showtimes = new List<Showtime>();
            }

            ShowtimeComboBox.ItemsSource = showtimes;

            if (showtimes.Count > 0) ShowtimeComboBox.IsEnabled = true;
            if (showtimes.Count == 0) ShowtimeComboBox.IsEnabled = false;
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
            _selectedSeat = null;
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
            if (ShowtimeComboBox.SelectedItem is not Showtime showtime || !_selectedSeat.HasValue)
            {
                MessageBox.Show("Оберіть сеанс та місце.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool hasLounge = LoungeYes.IsChecked == true;
            bool hasSnacks = SnacksYes.IsChecked == true;
            string? wishes = string.IsNullOrWhiteSpace(WishesInput.Text) ? null : WishesInput.Text;

            try
            {
                var reservation = new Reservation(showtime, _selectedSeat.Value, hasLounge, hasSnacks, wishes);
                _manager.AddReservation(reservation);
                Logger.Log("Додано", reservation.GetReservationDetails());

                MessageBox.Show("Бронювання успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
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

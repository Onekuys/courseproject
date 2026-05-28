using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OOPWPFProject.Data;
using OOPWPFProject.Helpers;
using OOPWPFProject.Models;

namespace OOPWPFProject.Views
{
    public partial class ManageShowtimesWindow : Window
    {
        private readonly EntityManager _manager = new();
        private Showtime? _selectedShowtime;

        public event EventHandler? ShowtimesUpdated;

        public ManageShowtimesWindow()
        {
            InitializeComponent();
            MovieCombo.ItemsSource = _manager.GetAllMovies();
            MovieCombo.SelectedIndex = 0;
            FormatCombo.SelectedIndex = 0;
            LoadShowtimes();
        }

        // Завантажуємо сеанси за обраною датою
        private void LoadShowtimes()
        {
            if (ShowtimesGrid == null) return;
            var date = DateFilter.SelectedDate ?? DateTime.Today;

            // Отримуємо сеанси разом з кількістю активних бронювань
            var rawShowtimes = _manager.GetShowtimesForDate(date) ?? Enumerable.Empty<Showtime>();
            var showtimes = rawShowtimes.Select(s => new ShowtimeRow(s)).ToList();

            ShowtimesGrid.ItemsSource = showtimes;
            StatusBar.Text = $"Знайдено сеансів: {showtimes.Count}";
            ClearForm();
        }

        private void DateFilter_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => LoadShowtimes();

        private void ShowtimesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShowtimesGrid.SelectedItem is not ShowtimeRow row)
            {
                _selectedShowtime = null;
                EditBtn.IsEnabled = false;
                DeleteBtn.IsEnabled = false;
                return;
            }

            _selectedShowtime = row.Source;

            // Заповнюємо форму даними обраного сеансу
            MovieCombo.SelectedItem = MovieCombo.Items.Cast<Movie>().FirstOrDefault(m => m.Id == _selectedShowtime.MovieId);
            DateInput.SelectedDate = _selectedShowtime.Date;
            TimeInput.Text = _selectedShowtime.Time.ToString(@"hh\:mm");
            FormatCombo.SelectedItem = FormatCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Content?.ToString() == _selectedShowtime.Format);

            EditBtn.IsEnabled = true;
            DeleteBtn.IsEnabled = true;
            StatusBar.Text = $"Обрано: {_selectedShowtime.Movie?.Title} о {_selectedShowtime.Time:hh\\:mm}";
        }

        // Додавання нового сеансу
        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseForm(out var movie, out var date, out var time, out var format)) return;

            try
            {
                var showtime = new Showtime
                {
                    MovieId = movie!.Id,
                    Date = date,
                    Time = time,
                    Format = format
                };

                // Перевіряємо наявність дублікатів
                if (_manager.GetAllShowtimes()?.Any(s => s.MovieId == showtime.MovieId && s.Date == showtime.Date && s.Time == showtime.Time) == true)
                {
                    MessageBox.Show("Сеанс з таким фільмом, датою та часом вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _manager.AddShowtime(showtime);
                Logger.Log("Адмін:сеанс", $"Додано: {movie.Title} {date:dd.MM.yyyy} {time:hh\\:mm} {format}");
                ShowtimesUpdated?.Invoke(this, EventArgs.Empty);
                LoadShowtimes();
                StatusBar.Text = "Сеанс успішно додано.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Редагування обраного сеансу
        private void EditBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShowtime == null) return;
            if (!TryParseForm(out _, out _, out var time, out var format)) return;

            try
            {
                _manager.UpdateShowtime(_selectedShowtime, time, format);
                Logger.Log("Адмін:сеанс", $"Змінено #{_selectedShowtime.Id}: час={time:hh\\:mm} формат={format}");
                ShowtimesUpdated?.Invoke(this, EventArgs.Empty);
                LoadShowtimes();
                StatusBar.Text = "Сеанс успішно оновлено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Видалення сеансу — тільки якщо немає активних бронювань
        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShowtime == null) return;

            var confirm = MessageBox.Show(
                $"Видалити сеанс?\n{_selectedShowtime.Movie?.Title} о {_selectedShowtime.Time:hh\\:mm}", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                _manager.RemoveShowtime(_selectedShowtime);
                Logger.Log("Адмін:сеанс", $"Видалено #{_selectedShowtime.Id}");
                ShowtimesUpdated?.Invoke(this, EventArgs.Empty);
                LoadShowtimes();
                StatusBar.Text = "Сеанс видалено.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Парсинг і валідація форми
        private bool TryParseForm(out Movie? movie, out DateTime date, out TimeSpan time, out string format)
        {
            movie = null;
            date = DateTime.Today;
            time = TimeSpan.Zero;
            format = "";

            if (MovieCombo.SelectedItem is not Movie m)
            {
                MessageBox.Show("Оберіть фільм.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!DateInput.SelectedDate.HasValue)
            {
                MessageBox.Show("Оберіть дату.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!TimeSpan.TryParseExact(TimeInput.Text.Trim(), @"hh\:mm", null, out time))
            {
                MessageBox.Show("Час введено невірно. Формат: ГГ:ХХ (наприклад 14:30).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (FormatCombo.SelectedItem is not ComboBoxItem fi)
            {
                MessageBox.Show("Оберіть формат.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            movie = m;
            date = DateInput.SelectedDate.Value;
            format = fi.Content?.ToString() ?? "2D";
            return true;
        }

        private void ClearForm()
        {
            _selectedShowtime = null;
            TimeInput.Text = "";
            MovieCombo.SelectedIndex = 0;
            FormatCombo.SelectedIndex = 0;
            DateInput.SelectedDate = DateFilter.SelectedDate ?? DateTime.Today;
            EditBtn.IsEnabled = false;
            DeleteBtn.IsEnabled = false;
        }

        // Обгортка для DataGrid 
        public class ShowtimeRow
        {
            public Showtime Source { get; }
            public int Id => Source.Id;
            public Movie? Movie => Source.Movie;
            public DateTime Date => Source.Date;
            public TimeSpan Time => Source.Time;
            public string Format => Source.Format;
            // Підраховуємо активні (не скасовані) бронювання
            public int ActiveReservationsCount => Source.Reservations?.Count(r => !r.IsCanceled) ?? 0;

            public ShowtimeRow(Showtime source) => Source = source;
        }
    }
}
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOPWPFProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<MovieShowtime> bookings = new ObservableCollection<MovieShowtime>();
        public MainWindow()
        {
            InitializeComponent();
            bookings = new ObservableCollection<MovieShowtime>();
            BookingsDataGrid.ItemsSource = bookings;
        }

        // Метод для обробки кліку на кнопку "Забронювати"
        public void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string movieTitleText = MovieTitleInput.Text;
            string showTimeText= ShowtimeInput.Text;
            string seatNumberText = SeatNumberInput.Text;

            // Перевірка на заповнення обов'язкових полів
            if (string.IsNullOrWhiteSpace(movieTitleText) || string.IsNullOrWhiteSpace(showTimeText) || string.IsNullOrWhiteSpace(seatNumberText))
            {
                MessageBox.Show("Помилка: Будь ласка, заповніть всі обов'язкові поля (Назва фільму, Час сеансу, Номер місця)!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Перевірка формату часу та номера місця
            if (!TimeSpan.TryParse(showTimeText, out TimeSpan showtime))
            {
                MessageBox.Show("Помилка: Некоректний формат часу, будь ласка, заповніть поле у форматі ЧЧ:ММ!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(seatNumberText, out int seatnumber) || seatnumber < 1 )
            {
                MessageBox.Show("Помилка: Некоректний номер місця. ", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Переірка формату та додаткової інформації на випадок, якщо користувач залишив ці поля порожніми
            string? format = FormatInput.SelectedItem is ComboBoxItem selectedItem ? selectedItem.Content.ToString() : null;

            string? wishes = AdditionalInfoInput.Text;
            if (string.IsNullOrWhiteSpace(wishes)) wishes = null;

            MovieShowtime newBooking = new MovieShowtime(movieTitleText, showtime, seatnumber, format, wishes);
            bookings.Add(newBooking);
        }

        // Метод для обробки кліку на кнопку "Очистити"
        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            MovieTitleInput.Clear();
            ShowtimeInput.Clear();
            SeatNumberInput.Clear();
            FormatInput.SelectedIndex = -1; 
            AdditionalInfoInput.Clear();
        }
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is MovieShowtime selectedBooking)
            {
                bookings.Remove(selectedBooking);
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть запис для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        }
}
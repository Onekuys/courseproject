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
        private EntityManager<Reservation> _bookings;

        public AddReservationWindow(EntityManager<Reservation> bookings)
        {
            InitializeComponent();
            _bookings = bookings;
        }


        private void AddToList(Reservation newReservation)
        {
            _bookings.Add(newReservation);
        }

        // Метод для обробки кліку на кнопку "Забронювати"
        public void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string movieTitleText = MovieTitleInput.Text;
            string showTimeText = ShowtimeInput.Text;
            string seatNumberText = SeatNumberInput.Text;

            // Перевірка на заповнення обов'язкових полів
            if (string.IsNullOrWhiteSpace(movieTitleText) || string.IsNullOrWhiteSpace(showTimeText) || string.IsNullOrWhiteSpace(seatNumberText) || FormatInput.SelectedItem == null || (Lounge1Input.IsChecked == false && Lounge2Input.IsChecked == false) || (Snacks1Input.IsChecked == false && Snacks2Input.IsChecked == false))
            {
                MessageBox.Show("Помилка: Будь ласка, заповніть всі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Перевірка формату часу та номера місця
            if (!TimeSpan.TryParse(showTimeText, out TimeSpan showtime))
            {
                MessageBox.Show("Помилка: Некоректний формат часу, будь ласка, заповніть поле у форматі ЧЧ:ММ!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(seatNumberText, out int seatnumber) || seatnumber < 1)
            {
                MessageBox.Show("Помилка: Некоректний номер місця. ", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Переірка формату та додаткової інформації на випадок, якщо користувач залишив ці поля порожніми
            string? format = FormatInput.SelectedItem is ComboBoxItem selectedItem ? selectedItem.Content.ToString() : null;

            string? wishes = AdditionalInfoInput.Text;
            if (string.IsNullOrWhiteSpace(wishes)) wishes = null;

            // Перевірка на VIP
            bool hasLounge = Lounge1Input.IsChecked == true;
            bool hasSnacks = Snacks1Input.IsChecked == true;

            Reservation newBooking;

            try
            {
                if (hasLounge || hasSnacks)
                {
                    newBooking = new VIPReservation(movieTitleText, showtime, seatnumber, format, hasLounge, hasSnacks, wishes);

                    AddToList(newBooking);
                    MessageBox.Show("Запис успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    newBooking = new Reservation(movieTitleText, showtime, seatnumber, format);
                    AddToList(newBooking);
                    MessageBox.Show("Запис успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Помилка вводу", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        // Метод для обробки кліку на кнопку "Очистити"
        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            MovieTitleInput.Clear();
            ShowtimeInput.Clear();
            SeatNumberInput.Clear();
            FormatInput.SelectedIndex = -1;
        }

    }
}

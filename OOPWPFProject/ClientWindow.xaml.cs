using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OOPWPFProject
{
    public partial class ClientWindow : Window
    {
        private readonly EntityManager _manager = new();
        private readonly TicketRepository _tickets = new();
        private readonly User _user;

        private ReservationViewModel? _selected;

        public ClientWindow(User user)
        {
            InitializeComponent();
            _user = user;
            WelcomeLabel.Text = $"{user.FullName}";
            LoadTickets();
        }

        // Завантаження 

        private void LoadTickets()
        {
            var raw = _manager.GetReservationsByUser(_user.Id);
            var res = raw.Select(r => new ReservationViewModel(r)).ToList();
            TicketsGrid.ItemsSource = res;

            StatTotal.Text = res.Count.ToString();
            StatPaid.Text = res.Count(r => r.IsPaid).ToString();
            StatSpent.Text = res.Where(r => r.IsPaid).Sum(r => r.Price).ToString("0.00");

            StatusBar.Text = $"Завантажено: {res.Count} квитків";
            UpdateButtons();
        }

        // Вибір рядка

        private void TicketsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selected = TicketsGrid.SelectedItem as ReservationViewModel;
            UpdateButtons();

            if (_selected == null)
            {
                SelectionInfo.Text = "Оберіть квиток зі списку";
            }
            else
            {
                SelectionInfo.Text = $"{_selected.MovieTitle} | Місце {_selected.SeatNumber} | {_selected.Price} грн";
            }
        }

        private void UpdateButtons()
        {
            PayBtn.IsEnabled = _selected != null && !_selected.IsCanceled && !_selected.IsPaid;
            CancelBtn.IsEnabled = _selected != null && !_selected.IsCanceled;
        }

        // Оплата 

        private void PayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            // Імітація оплати
            var result = MessageBox.Show(
                $"Оплатити квиток?\n\n" +
                $"Фільм:  {_selected.MovieTitle}\n" +
                $"Місце:  {_selected.SeatNumber} ({_selected.TypeLabel})\n" +
                $"Сума:   {_selected.Price} грн\n\n" +
                $"Підтвердити оплату?",
                "Підтвердження оплати",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _manager.PayReservation(_selected.Source);
            _tickets.SaveTicket(_user, _selected.Source);
            Logger.Log("Оплата", $"{_user.FullName}: {_selected.Source.GetReservationDetails()}");

            ShowPaymentConfirmation(_selected);
            LoadTickets();
        }

        private void ShowPaymentConfirmation(ReservationViewModel vm)
        {
            MessageBox.Show(
                $"  Оплата прийнята!\n\n" +
                $"--------------------------\n" +
                $"  КВИТОК ОПЛАЧЕНО\n" +
                $"--------------------------\n" +
                $"Клієнт:   {_user.FullName}\n" +
                $"Телефон:  {_user.Phone}\n" +
                $"Фільм:    {vm.MovieTitle}\n" +
                $"Час:      {vm.ShowTime:hh\\:mm}\n" +
                $"Формат:   {vm.Format}\n" +
                $"Місце:    {vm.SeatNumber} ({vm.TypeLabel})\n" +
                $"Сума:     {vm.Price} грн\n" +
                $"--------------------------\n" +
                $"Дані збережено у tickets.json",
                "Оплата успішна",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Скасування 

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_selected == null) return;

            var result = MessageBox.Show(
                $"Скасувати бронювання?\n{_selected.MovieTitle}, місце {_selected.SeatNumber}",
                "Скасування", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _manager.CancelReservation(_selected.Source);
            Logger.Log("Клієнт:скасування", $"{_user.FullName}: {_selected.Source.GetReservationDetails()}");
            LoadTickets();

            MessageBox.Show("Бронювання скасовано.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Кнопка оновлення 

        private void RefreshBtn_Click(object sender, RoutedEventArgs e) => LoadTickets();
    }
}
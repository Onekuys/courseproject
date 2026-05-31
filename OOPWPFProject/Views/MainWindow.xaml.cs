using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using OOPWPFProject.Helpers;
using OOPWPFProject.ViewModels;
using OOPWPFProject.Data;

namespace OOPWPFProject.Views
{
    // Вікно адмін-панелі для керування бронюваннями та перегляду статистики
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainWindowViewModel();
            DataContext = _vm;

            _vm.AddRequested += OnAddRequested;
            _vm.ConfirmationRequested += OnConfirmationRequested;
            _vm.MessageRequested += OnMessageRequested;

            _vm.Reservations.CollectionChanged += (_, _) => UpdateStats();
            UpdateStats();
        }

        // Оновлення статистики на вкладці "Статистика"
        private void UpdateStats()
        {
            var list = _vm.Reservations;
            StatTotal.Text = list.Count.ToString();
            StatVip.Text = list.Count(r => r.IsVip && !r.IsCanceled).ToString();
            StatImax.Text = list.Count(r => r.Format == "IMAX" && !r.IsCanceled).ToString();
        }

        // Фільтр-чіпи
        private void FilterChip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clicked) return;

            if (clicked.Parent is StackPanel panel)
            {
                foreach (var child in panel.Children.OfType<ToggleButton>())
                    child.IsChecked = child == clicked;
            }

            _vm.ActiveFilter = clicked.Tag?.ToString() ?? "Всі";
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }  

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.LoadReservations();
            UpdateStats();
        }

        // [ADMIN] Відкриття вікна керування сеансами
        private void ManageShowtimesBtn_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageShowtimesWindow();
            window.Owner = this;
            window.ShowtimesUpdated += (s, args) =>
            {
                _vm.LoadReservations();
                UpdateStats();
            };

            window.ShowDialog();

            _vm.LoadReservations();
            UpdateStats();
        }



        // Перемикач вкладок
        private void TabRecord_Click(object sender, RoutedEventArgs e)
        {
            TabRecord.IsChecked = true;
            TabStats.IsChecked = false;
            TabRecord.Background = System.Windows.Media.Brushes.White;
            TabRecord.BorderThickness = new Thickness(0, 0, 0, 2);
            TabRecord.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A6FC4"));
            TabStats.Background = System.Windows.Media.Brushes.Transparent;
            TabStats.BorderThickness = new Thickness(0);
            TabStats.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));
            PanelRecord.Visibility = Visibility.Visible;
            PanelStats.Visibility = Visibility.Collapsed;
        }

        private void TabStats_Click(object sender, RoutedEventArgs e)
        {
            TabStats.IsChecked = true;
            TabRecord.IsChecked = false;
            TabStats.Background = System.Windows.Media.Brushes.White;
            TabStats.BorderThickness = new Thickness(0, 0, 0, 2);
            TabStats.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A6FC4"));
            TabRecord.Background = System.Windows.Media.Brushes.Transparent;
            TabRecord.BorderThickness = new Thickness(0);
            TabRecord.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#9CA3AF"));
            PanelRecord.Visibility = Visibility.Collapsed;
            PanelStats.Visibility = Visibility.Visible;
        }


        // Обробники для ViewModel
        private void OnAddRequested(object? sender, EventArgs e)
        {
            var addWindow = new AddReservationWindow(new EntityManager());
            addWindow.Owner = this;
            addWindow.ShowDialog();
            _vm.LoadReservations();
            UpdateStats();
        }

        private void OnConfirmationRequested(object? sender, string message)
        {
            MessageBox.Show(message, "Підтвердження", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnMessageRequested(object? sender, string message)
        {
            MessageBox.Show(message, "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logger.Log("Система", "Вікно закрито");
        }
    }
}
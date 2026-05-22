using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OOPWPFProject
{
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

        // Статистика
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
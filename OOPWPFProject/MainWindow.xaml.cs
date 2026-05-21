using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOPWPFProject
{
    // <summary>
    // Interaction logic for MainWindow.xaml
    // </summary>
    public partial class MainWindow : Window
    {
        private EntityManager _manager = new EntityManager();

        public MainWindow()
        {
            InitializeComponent();
            RefreshGrid(DateTime.Today);
        }

        private void RefreshGrid(DateTime date)
        {
            BookingsDataGrid.ItemsSource = null;
            BookingsDataGrid.ItemsSource = _manager.GetAllReservations(date);
        }
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is not Reservation selected) return;
            
            if (selected.IsCanceled)
            {
                MessageBox.Show("Неможливо видалити скасоване бронювання.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var confirm = MessageBox.Show("Видалити бронювання?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                _manager.RemoveReservation(selected);
                RefreshGrid(DateTime.Today);
            }
        }

        private void CancelRecord_Click(Object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is not Reservation selected) return;

            if (selected.IsCanceled)
            {
                MessageBox.Show("Бронювання вже скасоване.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var confirm = MessageBox.Show("Скасувати бронювання?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                _manager.CancelReservation(selected);
                RefreshGrid(DateTime.Today);
            }
            MessageBox.Show("Бронювання успішно скасоване.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Warning);
        }


        private void OpenAddWindow_Click(object sender, RoutedEventArgs e)
        {
            AddReservationWindow addWindow = new AddReservationWindow(_manager);
            addWindow.ShowDialog();
            RefreshGrid(DateTime.Today);
        }



        // ------------------
        // Треба реалізувати
        // ------------------

        private void SortRecords_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Сортування по полю з SortComboBox
        }

        private void SearchName_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Фільтрація по назві фільму з SearchNameInput
        }

        private void ExportGrouped_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Експорт згрупованих даних (лише для Admin)
        }

        private void GroupRecords_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Групування виділених записів
        }

        private void CompareEquality_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Перевірка збігу часу і місця для двох обраних записів
        }

        private void CompareTime_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Порівняння часу сеансів для двох обраних записів
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // TODO: Зберегти стан / підтвердити вихід при необхідності
        }

    }

}
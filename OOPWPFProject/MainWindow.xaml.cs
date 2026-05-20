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
        private EntityManager bookingsManager = new EntityManager();

        public MainWindow()
        {
            InitializeComponent();
            RefreshGrid(DateTime.Today);
        }

        private void RefreshGrid(DateTime date)
        {
            BookingsDataGrid.ItemsSource = bookingsManager.GetAllReservationsByDate(date);
        }
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            var selected = BookingsDataGrid.SelectedItem as Reservation;
            if (selected != null && !selected.IsCanceled)
            {
                bookingsManager.RemoveReservation(selected);
            }
        }
        private void CancelRecord_Click(Object sender, RoutedEventArgs e)
        {
            var selected = BookingsDataGrid.SelectedItem as Reservation;
            if (selected != null && !selected.IsCanceled)
            {
                bookingsManager.CancelReservation(selected);
                RefreshGrid(DateTime.Today);
                MessageBox.Show("Бронювання успішно скасовано в базі даних!");
            }
        }
        private void OpenAddWindow_Click(object sender, RoutedEventArgs e)
        {
            AddReservationWindow addWindow = new AddReservationWindow(bookingsManager);
            addWindow.ShowDialog();
            RefreshGrid(DateTime.Today);
        }
    }

}
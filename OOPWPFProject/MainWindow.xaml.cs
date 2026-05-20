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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private EntityManager _entityManager = new EntityManager();

        public MainWindow()
        {
            InitializeComponent();
            //RefreshGrid();
        }

        //private void RefreshGrid()
        //{
        //    BookingsDataGrid.ItemsSource = _entityManager.GetAllReservations();
        //}
        //private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        //{
        //    var selected = BookingsDataGrid.SelectedItem as Reservation;
        //    if (selected != null && !selected.IsCanceled)
        //    {
        //        _entityManager.RemoveReservation(selected);
        //    }
        //}
        //private void CancelRecord_Click(Object sender, RoutedEventArgs e)
        //{
        //    var selected = BookingsDataGrid.SelectedItem as Reservation;
        //    if (selected != null && !selected.IsCanceled)
        //    {
        //        _entityManager.CancelReservation(selected);
        //        RefreshGrid();
        //        MessageBox.Show("Бронювання успішно скасовано в базі даних!");
        //    }
        //}
    }

}
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
        
        private EntityManager _bookings;

        public AddReservationWindow(EntityManager bookingsManager)
        {
            InitializeComponent();
            _bookings = bookingsManager;
        }

        // Метод для обробки кліку на кнопку "Забронювати"
        public void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            
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

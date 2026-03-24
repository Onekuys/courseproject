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
        public MainWindow()
        {
            InitializeComponent();
        }

        public void AddRecord_Click(object sender, RoutedEventArgs e)
        {
            string movieTitle = MovieTitleInput.Text;
            string showTime = ShowtimeInput.Text;
            string seatNumber = SeatNumberInput.Text;

            string format = FormatInput.SelectedItem is ComboBoxItem selectedItem ? selectedItem.Content.ToString() : "Не вказано";
            string additionalInfo = AdditionalInfoInput.Text;

            if (string.IsNullOrWhiteSpace(movieTitle) || string.IsNullOrWhiteSpace(showTime) || string.IsNullOrWhiteSpace(seatNumber))
            {
                MessageBox.Show("Будь ласка, заповніть всі обов'язкові поля (назва фільму, час показу, номер місця).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StringBuilder recordBuilder = new StringBuilder();
            recordBuilder.AppendLine("Бронювання: ");
            recordBuilder.AppendLine($"Назва фільму: {movieTitle}");
            recordBuilder.AppendLine($"Час сеансу: {showTime}");
            recordBuilder.AppendLine($"Номер місця: {seatNumber}");
            recordBuilder.AppendLine($"Формат: {format}");
            recordBuilder.AppendLine($"Побажання: {(string.IsNullOrWhiteSpace(additionalInfo) ? "Не вказано" : additionalInfo)}");
            recordBuilder.AppendLine("---------------------");
            recordBuilder.AppendLine();

            if (ResultDisplay.Text == "Записи відсутні." || string.IsNullOrWhiteSpace(ResultDisplay.Text))
            {
                ResultDisplay.Text = recordBuilder.ToString();
            }
            else
            {
                ResultDisplay.Text += recordBuilder.ToString();
            }
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            MovieTitleInput.Clear();
            ShowtimeInput.Clear();
            SeatNumberInput.Clear();
            FormatInput.SelectedIndex = -1; 
            AdditionalInfoInput.Clear();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
using OOPWPFProject.Models;
using OOPWPFProject.Data;
using OOPWPFProject.Helpers;
using System.Windows;

namespace OOPWPFProject.Views
{
    public partial class LoginWindow : Window
    {
        private readonly UserRepository repository = new();
        private bool isRegister = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        // Перемикач режиму

        private void ToggleBtn_Click(object sender, RoutedEventArgs e)
        {
            isRegister = !isRegister;
            ErrorLabel.Visibility = Visibility.Collapsed;

            if (isRegister)
            {
                TitleLabel.Text = "Реєстрація";
                SubmitBtn.Content = "Зареєструватись";
                ToggleBtn.Content = "Вже маєте акаунт? Увійти";


                LblName.Visibility = LblPhone.Visibility = LblRegEmail.Visibility = Visibility.Visible;
                FullNameBox.Visibility = PhoneBox.Visibility = RegEmailBox.Visibility = Visibility.Visible;

                LblLogin.Visibility = LoginBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                TitleLabel.Text = "Вхід";
                SubmitBtn.Content = "Увійти";
                ToggleBtn.Content = "Немає акаунту? Зареєструватись";

                LblName.Visibility = LblPhone.Visibility = LblRegEmail.Visibility = Visibility.Collapsed;
                FullNameBox.Visibility = PhoneBox.Visibility = RegEmailBox.Visibility = Visibility.Collapsed;

                LblLogin.Visibility = LoginBox.Visibility = Visibility.Visible;
            }
        }

        // Відправка форми

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            ErrorLabel.Visibility = Visibility.Collapsed;
            string password = PasswordBox.Password;

            try
            {
                User user;
                if (isRegister)
                {
                    user = repository.Register(FullNameBox.Text.Trim(), PhoneBox.Text.Trim(), RegEmailBox.Text.Trim(), password);
                    Logger.Log("Реєстрація", $"{user.FullName} ({user.Email})");
                }
                else
                {
                    user = repository.Login(LoginBox.Text.Trim(), password) ?? throw new System.Exception("Невірний логін або пароль.");
                    Logger.Log("Вхід", $"{user.FullName} [{user.Role}]");
                }

                App.CurrentUser = user;
                DialogResult = true;
                Close();
            }
            catch (System.Exception ex)
            {
                ErrorLabel.Text = ex.Message;
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }
    }
}
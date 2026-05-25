using System.Configuration;
using System.Data;
using System.Windows;

namespace OOPWPFProject
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new UserRepository().EnsureAdminExists();
        }
    }

}

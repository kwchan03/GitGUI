using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitGUI.Pages
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void GitHubLoginButton_Click(object sender, RoutedEventArgs e)
        {
            //var loginPage = new GitHubLoginPage();

            //// Navigate your Frame to the GitHubLoginPage
            //// Assume your Frame is named "MainFrame" and is accessible via parent window
            //((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(loginPage);

            //try
            //{
            //    // Await the token once the GitHubLoginPage has finished its flow
            //    string token = await loginPage.GetGitHubAccessTokenAsync();

            //    // Process token, then navigate back or forward to another Page
            //    MessageBox.Show($"Token: {token}", "Success");
            //    // For example, navigate back to a DashboardPage:
            //    ((MainWindow)Application.Current.MainWindow).MainFrame.Navigate(new OperationPage());
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"OAuth failed: {ex.Message}", "Error");
            //    // Optionally navigate back:
            //    ((MainWindow)Application.Current.MainWindow).MainFrame.GoBack();
            //}
        }
    }
}

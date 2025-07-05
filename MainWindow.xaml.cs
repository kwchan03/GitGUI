using System.Windows;

namespace GitGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
        }

        public MainWindow(Pages.OperationPage operationPage)
        {
            InitializeComponent();
            MainFrame.Navigate(operationPage);
        }

    }
}
using System.Windows.Controls;

namespace GitGUI.Pages
{
    /// <summary>
    /// Interaction logic for PRListPage.xaml
    /// </summary>
    public partial class PullRequestPage : Page
    {
        public PullRequestPage()
        {
            InitializeComponent();
        }

        // Event handler for the SelectionChanged event of the ListBox
        private void ChangedFile_Selected(object sender, SelectionChangedEventArgs e)
        {
            // Add your logic here to handle the file selection change
            // Example: Update the diff viewer based on the selected file
        }
    }
}

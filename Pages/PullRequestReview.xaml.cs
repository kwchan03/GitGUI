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
    /// Interaction logic for PullRequestReview.xaml
    /// </summary>
    public partial class PullRequestReview : Page
    {
        public PullRequestReview()
        {
            InitializeComponent();
        }

        // Add the missing event handler for FileListBox_SelectionChanged
        private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle the selection change logic here
            // Example: Display the selected file's diff
            if (FileListBox.SelectedItem != null)
            {
                var selectedFile = FileListBox.SelectedItem.ToString();
                // Logic to update OldFileLines and NewFileLines based on selectedFile
            }
        }
    }
}

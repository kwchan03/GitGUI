using GitGUI.ViewModels;
using System.Windows.Controls;

namespace GitGUI.Pages
{
    /// <summary>  
    /// Interaction logic for Operation.xaml  
    /// </summary>  
    public partial class OperationPage : Page
    {
        public OperationPage(OperationViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}

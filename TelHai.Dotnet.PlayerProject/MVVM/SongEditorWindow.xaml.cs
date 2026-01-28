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
using System.Windows.Shapes;

namespace TelHai.Dotnet.PlayerProject.MVVM
{
    /// <summary>
    /// Interaction logic for SongEditorWindow.xaml
    /// </summary>
    public partial class SongEditorWindow : Window
    {
        public SongEditorWindow(SongEditorViewModel vm)
        {
            InitializeComponent();

            // 1. Connect the View to the ViewModel
            DataContext = vm;

            // 2. Listen for the "Close" signal from the ViewModel
            vm.CloseRequested += (success) =>
            {
                DialogResult = success; // true = Saved, false = Cancelled
                Close();
            };
        }
    }
}

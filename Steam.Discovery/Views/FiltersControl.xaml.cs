using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Steam.Discovery.Models;

namespace Steam.Discovery.Views
{
    /// <summary>
    /// Interaction logic for Filters.xaml
    /// </summary>
    public partial class FiltersControl : UserControl
    {
        public FiltersControl()
        {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            AppMessenger.SendMessage(AppAction.DoesntHaveTagsFocused);
        }

        private void TextBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            AppMessenger.SendMessage(AppAction.HasTagsFocused);
        }
    }
}

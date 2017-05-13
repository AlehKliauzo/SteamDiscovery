using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using Steam.Discovery.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Steam.Discovery.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<Message>(this, OnMessageReceived);
        }

        private void OnMessageReceived(Message message)
        {
            if (message == Message.GamesListChanged && GamesList.Items.Count > 0)
            {
                GamesList.ScrollIntoView(GamesList.Items[0]);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var grid = (FrameworkElement)sender;
            var tag = (string)grid.Tag;
            var url = "http://store.steampowered.com/app/" + tag;
            Process.Start(url);
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Messenger.Default.Send<Message>(Message.AppClosing);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<Message>(Message.DoesntHaveTagsFocused);
        }

        private void TextBox_GotFocus_1(object sender, RoutedEventArgs e)
        {
            Messenger.Default.Send<Message>(Message.HasTagsFocused);
        }
    }
}

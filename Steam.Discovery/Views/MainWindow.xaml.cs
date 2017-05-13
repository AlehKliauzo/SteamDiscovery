using MahApps.Metro.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Steam.Discovery.Models;

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
            AppMessenger.RegisterForMessage(this, OnMessageReceived);
        }

        private void OnMessageReceived(Message message)
        {
            //if (message.Action == AppAction.GamesListChanged && GamesList.Items.Count > 0)
            //{
            //    GamesList.ScrollIntoView(GamesList.Items[0]);
            //}
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
            AppMessenger.SendMessage(AppAction.AppClosing);
        }
    }
}

using GalaSoft.MvvmLight;
using Steam.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Steam.Discovery.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Games = new ObservableCollection<Game>();
            LoadGames();

            if (IsInDesignMode)
            {
                Games.Add(new Game { Id = "1", Name = "Half Life 2" });
            }
        }

        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get { return _games; }
            private set { _games = value; RaisePropertyChanged(() => Games); }
        }

        private void LoadGames()
        {
            Task.Run(() =>
            {
                var games = GamesSerializer.Load().OrderByDescending(x => x.WilsonScore).Take(20).ToList();
                Games = new ObservableCollection<Game>(games);
            }
            );
        }
    }
}
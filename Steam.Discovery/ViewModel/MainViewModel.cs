using GalaSoft.MvvmLight;
using Steam.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Steam.Discovery.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private List<Game> _allGames;

        public MainViewModel()
        {
            Games = new ObservableCollection<Game>();
            LoadGames();
        }

        #region Properties

        private bool _isReleasedAfterFilterEnabled;
        public bool IsReleasedAfterFilterEnabled
        {
            get { return _isReleasedAfterFilterEnabled; }
            set
            {
                _isReleasedAfterFilterEnabled = value;
                RaisePropertyChanged(() => IsReleasedAfterFilterEnabled);
                FiltersChanged();
            }
        }

        private string _releasedAfter;
        public string ReleasedAfter
        {
            get { return _releasedAfter; }
            set
            {
                _releasedAfter = value;
                RaisePropertyChanged(() => ReleasedAfter);
                FiltersChanged();
            }
        }

        private bool _isMoreThanXReviewsFilterEnabled;
        public bool IsMoreThanXReviewsFilterEnabled
        {
            get { return _isMoreThanXReviewsFilterEnabled; }
            set
            {
                _isMoreThanXReviewsFilterEnabled = value;
                RaisePropertyChanged(() => IsMoreThanXReviewsFilterEnabled);
                FiltersChanged();
            }
        }

        private string _moreThanXReviews;
        public string MoreThanXReviews
        {
            get { return _moreThanXReviews; }
            set
            {
                _moreThanXReviews = value;
                RaisePropertyChanged(() => MoreThanXReviews);
                FiltersChanged();
            }
        }

        private bool _isDoesntHaveTagsFilterEnabled;
        public bool IsDoesntHaveTagsFilterEnabled
        {
            get { return _isDoesntHaveTagsFilterEnabled; }
            set
            {
                _isDoesntHaveTagsFilterEnabled = value;
                RaisePropertyChanged(() => IsDoesntHaveTagsFilterEnabled);
                FiltersChanged();
            }
        }

        private string _doesntHaveTags;
        public string DoesntHaveTags
        {
            get { return _doesntHaveTags; }
            set
            {
                _doesntHaveTags = value;
                RaisePropertyChanged(() => DoesntHaveTags);
                FiltersChanged();
            }
        }

        private ObservableCollection<Game> _games;
        public ObservableCollection<Game> Games
        {
            get { return _games; }
            private set
            {
                _games = value;
                RaisePropertyChanged(() => Games);
            }
        }

        #endregion

        private void FiltersChanged()
        {
            IEnumerable<Game> games = _allGames;

            if (IsReleasedAfterFilterEnabled && DateTime.TryParse(ReleasedAfter, out DateTime releaseDate))
            {
                games = games.Where(x => x.ReleaseDate > releaseDate);
            }

            if (IsMoreThanXReviewsFilterEnabled && int.TryParse(MoreThanXReviews, out int reviewsCount))
            {
                games = games.Where(x => x.AllTotalReviews > reviewsCount);
            }

            if (IsDoesntHaveTagsFilterEnabled)
            {
                var tags = DoesntHaveTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                           Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                foreach (var tag in tags)
                {
                    games = games.Where(x => x.Tags.All(y => y != tag));
                }
            }

            var filteredGames = games.OrderByDescending(x => x.WilsonScore).Take(20).ToList();
            Games = new ObservableCollection<Game>(filteredGames);
        }

        private void LoadGames()
        {
            Task.Run(() =>
            {
                _allGames = GamesSerializer.Load();
                var games = _allGames.OrderByDescending(x => x.WilsonScore).Take(20).ToList();
                Games = new ObservableCollection<Game>(games);
            }
            );
        }
    }
}
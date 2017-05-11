using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Steam.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam.Discovery.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private List<Game> _allGames;
        private List<Game> _filteredGames;
        private bool _updatesSuspended;

        public MainViewModel()
        {
            Load();
            Messenger.Default.Register<Message>(this, OnMessageReceived);
        }

        #region Properties

        private bool _isNameContainsFilterEnabled;
        public bool IsNameContainsFilterEnabled
        {
            get { return _isNameContainsFilterEnabled; }
            set
            {
                _isNameContainsFilterEnabled = value;
                RaisePropertyChanged(() => IsNameContainsFilterEnabled);
                FiltersChanged();
            }
        }

        private string _nameContains;
        public string NameContains
        {
            get { return _nameContains; }
            set
            {
                _nameContains = value;
                RaisePropertyChanged(() => NameContains);
                FiltersChanged();
            }
        }

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

        private bool _isHasTagsFilterEnabled;
        public bool IsHasTagsFilterEnabled
        {
            get { return _isHasTagsFilterEnabled; }
            set
            {
                _isHasTagsFilterEnabled = value;
                RaisePropertyChanged(() => IsHasTagsFilterEnabled);
                FiltersChanged();
            }
        }

        private string _hasTags;
        public string HasTags
        {
            get { return _hasTags; }
            set
            {
                _hasTags = value;
                RaisePropertyChanged(() => HasTags);
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

        private string _resultsCount;
        public string ResultsCount
        {
            get { return _resultsCount; }
            set
            {
                _resultsCount = value;
                RaisePropertyChanged(() => ResultsCount);
            }
        }

        #endregion

        private void FiltersChanged()
        {
            if (_updatesSuspended)
                return;

            IEnumerable<Game> games = _allGames;

            if(IsNameContainsFilterEnabled)
            {
                games = games.Where(x => x.Name.IndexOf(NameContains, StringComparison.InvariantCultureIgnoreCase) != -1);
            }

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
                    games = games.Where(x => x.Tags.All(y => !y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            if(IsHasTagsFilterEnabled)
            {
                var tags = HasTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                           Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                foreach(var tag in tags)
                {
                    games = games.Where(x => x.Tags.Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            _filteredGames = games.OrderByDescending(x => x.WilsonScore).ToList();
            var page = _filteredGames.Take(10).ToList();
            Games = new ObservableCollection<Game>(page);

            Messenger.Default.Send<string>("Games list changed");
            //GamesChanged();

            ResultsCount = _filteredGames.Count.ToString();
        }

        private void Load()
        {
            Task.Run(() =>
            {
                var settings = Serializer.LoadSettings();
                _allGames = Serializer.LoadGames();

                LoadSettings(settings);
                //var games = _allGames.OrderByDescending(x => x.WilsonScore).Take(20).ToList();
                //Games = new ObservableCollection<Game>(games);
            }
            );
        }

        private void LoadSettings(Settings settings)
        {
            _updatesSuspended = true;

            IsNameContainsFilterEnabled = settings.IsNameContainsFilterEnabled;
            NameContains = settings.NameContains;
            IsReleasedAfterFilterEnabled = settings.IsReleasedAfterFilterEnabled;
            ReleasedAfter = settings.ReleasedAfter;
            IsMoreThanXReviewsFilterEnabled = settings.IsMoreThanXReviewsFilterEnabled;
            MoreThanXReviews = settings.MoreThanXReviews;
            IsDoesntHaveTagsFilterEnabled = settings.IsDoesntHaveTagsFilterEnabled;
            DoesntHaveTags = settings.DoesntHaveTags;
            IsHasTagsFilterEnabled = settings.IsHasTagsFilterEnabled;
            HasTags = settings.HasTags; ;

            _updatesSuspended = false;
            FiltersChanged();
        }

        private void SaveSettings()
        {
            var settings = new Settings();
            settings.IsNameContainsFilterEnabled = IsNameContainsFilterEnabled;
            settings.NameContains = NameContains;
            settings.IsReleasedAfterFilterEnabled = IsReleasedAfterFilterEnabled;
            settings.ReleasedAfter = ReleasedAfter;
            settings.IsMoreThanXReviewsFilterEnabled = IsMoreThanXReviewsFilterEnabled;
            settings.MoreThanXReviews = MoreThanXReviews;
            settings.IsDoesntHaveTagsFilterEnabled = IsDoesntHaveTagsFilterEnabled;
            settings.DoesntHaveTags = DoesntHaveTags;
            settings.IsHasTagsFilterEnabled = IsHasTagsFilterEnabled;
            settings.HasTags = HasTags;
            Serializer.SaveSettings(settings);
        }

        private void OnMessageReceived(Message message)
        {
            if(message == Message.AppClosing)
            {
                SaveSettings();
            }
        }
    }
}
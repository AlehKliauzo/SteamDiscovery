using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Steam.Common;
using Steam.Discovery.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Steam.Discovery.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int gamesPerPage = 10;
        private List<Game> _allGames;
        private List<Game> _filteredGames;
        private bool _updatesSuspended;

        private TagsWindow _tagsWindow;
        private TagsViewModel _tagsViewModel;

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

        private List<Game> _games;
        public List<Game> Games
        {
            get { return _games; }
            private set
            {
                _games = value;
                RaisePropertyChanged(() => Games);
            }
        }

        private int _page;
        public int Page
        {
            get { return _page; }
            set
            {
                _page = value;
                RaisePropertyChanged(() => Page);
            }
        }

        private int _pagesCount;
        public int PagesCount
        {
            get { return _pagesCount; }
            set
            {
                _pagesCount = value;
                RaisePropertyChanged(() => PagesCount);
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

        #region Commands

        private RelayCommand _nextPageCommand;
        public RelayCommand NextPageCommand
        {
            get { return _nextPageCommand ?? (_nextPageCommand = new RelayCommand(NextPage)); }
        }

        private void NextPage()
        {
            ShowPage(Page + 1);
        }

        private RelayCommand _previousPageCommand;
        public RelayCommand PreviousPageCommand
        {
            get { return _previousPageCommand ?? (_previousPageCommand = new RelayCommand(PreviousPage)); }
        }

        private void PreviousPage()
        {
            ShowPage(Page - 1);
        }

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand
        {
            get { return _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag)); }
        }

        private void AddTag(string tag)
        {
            var textToAdd = tag + ", ";

            if (_wasHasTagsControlLastFocused)
            {
                HasTags += textToAdd;
            }
            else
            {
                DoesntHaveTags += textToAdd;
            }
        }

        private RelayCommand _showTagsWindowCommand;
        public RelayCommand ShowTagsWindowCommand
        {
            get { return _showTagsWindowCommand ?? (_showTagsWindowCommand = new RelayCommand(ShowTagsWindow)); }
        }

        private void ShowTagsWindow()
        {
            if (_tagsWindow == null)
            {
                _tagsWindow = new TagsWindow();
                _tagsWindow.DataContext = _tagsViewModel;
            }

            _tagsWindow.Show();
        }

        #endregion

        private void FiltersChanged()
        {
            if (_updatesSuspended)
                return;

            IEnumerable<Game> games = _allGames;

            if (IsNameContainsFilterEnabled)
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

            if (IsHasTagsFilterEnabled)
            {
                var tags = HasTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                           Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                foreach (var tag in tags)
                {
                    games = games.Where(x => x.Tags.Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            var previouslyFilteredGames = _filteredGames;

            _filteredGames = games.OrderByDescending(x => x.WilsonScore).ToList();
            ResultsCount = _filteredGames.Count.ToString();

            if (previouslyFilteredGames != null && previouslyFilteredGames.Count == _filteredGames.Count)
            {
                return;
            }

            UpdatePaging();
            ShowPage(1);
        }

        private void UpdatePaging()
        {
            var totalPages = Math.Ceiling((double)_filteredGames.Count / (double)gamesPerPage);
            PagesCount = (int)totalPages;
        }

        private void ShowPage(int page)
        {
            if (PagesCount == 0)
            {
                Page = 0;
                Games = new List<Game>();
                return;
            }

            if (page < 1 || page > PagesCount)
            {
                Games = new List<Game>();
                return;
            }

            Page = page;

            var skip = (page - 1) * gamesPerPage;

            var games = _filteredGames.Skip(skip).Take(gamesPerPage).ToList();

            Games = new List<Game>(games);

            Messenger.Default.Send<Message>(Message.GamesListChanged);
        }

        private void Load()
        {
            Task.Run(() =>
            {
                var settings = Serializer.LoadSettings();
                _allGames = Serializer.LoadGames();

                LoadSettings(settings);

                Dictionary<string, int> tags = new Dictionary<string, int>();

                foreach (var game in _allGames)
                {
                    foreach (var tag in game.Tags)
                    {
                        if (tags.ContainsKey(tag))
                        {
                            tags[tag]++;
                        }
                        else
                        {
                            tags.Add(tag, 1);
                        }
                    }
                }

                var tagsList = tags.Select(x => new Tag { Name = x.Key, GamesCount = x.Value }).ToList();
                _tagsViewModel = new TagsViewModel(tagsList);

            });
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

        private bool _wasHasTagsControlLastFocused = false;

        private void OnMessageReceived(Message message)
        {
            switch (message)
            {
                case Message.AppClosing:
                    SaveSettings();
                    break;
                case Message.HasTagsFocused:
                    _wasHasTagsControlLastFocused = true;
                    break;
                case Message.DoesntHaveTagsFocused:
                    _wasHasTagsControlLastFocused = false;
                    break;
            }
        }
    }
}
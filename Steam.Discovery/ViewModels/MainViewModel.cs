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

        public MainViewModel()
        {
            Load();
            Messenger.Default.Register<Message>(this, OnMessageReceived);
        }

        #region Properties

        private FiltersViewModel _filters;
        public FiltersViewModel Filters
        {
            get { return _filters; }
            private set
            {
                _filters = value;
                RaisePropertyChanged(() => Filters);
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

        #endregion

        private void FiltersChanged()
        {
            if (_updatesSuspended)
                return;

            IEnumerable<Game> games = _allGames;
            var filters = Filters.GetFilters();

            if (filters.IsNameContainsFilterEnabled)
            {
                games = games.Where(x => x.Name.IndexOf(filters.NameContains, StringComparison.InvariantCultureIgnoreCase) != -1);
            }

            if (filters.IsReleasedAfterFilterEnabled && DateTime.TryParse(filters.ReleasedAfter, out DateTime releaseDate))
            {
                games = games.Where(x => x.ReleaseDate > releaseDate);
            }

            if (filters.IsMoreThanXReviewsFilterEnabled && int.TryParse(filters.MoreThanXReviews, out int reviewsCount))
            {
                games = games.Where(x => x.AllTotalReviews > reviewsCount);
            }

            if (filters.IsDoesntHaveTagsFilterEnabled)
            {
                var tags = filters.DoesntHaveTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                           Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

                foreach (var tag in tags)
                {
                    games = games.Where(x => x.Tags.All(y => !y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            if (filters.IsHasTagsFilterEnabled)
            {
                var tags = filters.HasTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
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

        private async void Load()
        {
            await Task.Run(() =>
            {
                var settings = Serializer.LoadSettings();
                _allGames = Serializer.LoadGames();

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
                Filters = new FiltersViewModel(tagsList);
                Filters.ApplyFilters(settings);
            });
        }

        private void OnMessageReceived(Message message)
        {
            switch (message)
            {
                case Message.FiltersChanged:
                    FiltersChanged();
                    break;
            }
        }
    }
}
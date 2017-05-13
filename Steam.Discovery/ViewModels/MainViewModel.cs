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
using Steam.Discovery.Models;

namespace Steam.Discovery.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int gamesPerPage = 10;
        private List<Game> _allGames;
        private List<Game> _filteredGames;

        public MainViewModel()
        {
            Load();
            AppMessenger.RegisterForMessage(this, OnMessageReceived);
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
            //TODO: compare filter settings to previous
            var filters = Filters.GetFilters();
            var games = ApplyHardFilters(_allGames, filters);
            games = ApplySoftFilters(games, filters);

            _filteredGames = games.OrderByDescending(x => x.TotalScore).ToList();
            //_filteredGames = games.OrderByDescending(x => x.PreferenceScore).ThenByDescending(x => x.WilsonScore).ToList();
            ResultsCount = _filteredGames.Count.ToString();

            UpdatePaging();
            ShowPage(1);
        }

        private List<Game> ApplyHardFilters(List<Game> source, Filters filters)
        {
            IEnumerable<Game> games = source;

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
                var tags = SplitStringOnCommas(filters.DoesntHaveTags);

                foreach (var tag in tags)
                {
                    games = games.Where(x => x.Tags.All(y => !y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            if (filters.IsHasTagsFilterEnabled)
            {
                var tags = SplitStringOnCommas(filters.HasTags);

                foreach (var tag in tags)
                {
                    games = games.Where(x => x.Tags.Any(y => y.Equals(tag, StringComparison.InvariantCultureIgnoreCase)));
                }
            }

            return games.ToList();
        }

        private List<string> SplitStringOnCommas(string text)
        {
            var entries = text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return entries;
        }

        private List<Game> ApplySoftFilters(List<Game> games, Filters filters)
        {
            var preferences = new Dictionary<string, double>();

            if (string.IsNullOrEmpty(filters.SoftTags))
            {
                return games;
            }

            var allTags = SplitStringOnCommas(filters.SoftTags);

            foreach (var tag in allTags)
            {
                var lastWhitespace = tag.LastIndexOf(' ');

                if (lastWhitespace == -1)
                {
                    continue;
                }

                var tagName = tag.Substring(0, lastWhitespace);
                var tagValue = tag.Substring(lastWhitespace);

                if (double.TryParse(tagValue, out double value))
                {
                    preferences.Add(tagName, value / 100.0);
                }
            }

            foreach (var game in games)
            {
                var tags = game.Tags;
                var tagWeight = 0.3 + 1.0 / tags.Count;
                var score = 0.0;

                foreach (var tag in tags)
                {
                    if (preferences.ContainsKey(tag))
                    {
                        score += tagWeight * preferences[tag];
                    }
                }

                game.PreferenceScore = score;
                game.TotalScore = game.WilsonScore * game.PreferenceScore;
            }

            return games;
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

            AppMessenger.SendMessage(AppAction.GamesListChanged);
        }

        private async void Load()
        {
            await Task.Run(() =>
            {
                var settings = Serializer.LoadSettings();
                _allGames = Serializer.LoadGames().Where(x => x.Tags.Count >= 5 && x.WilsonScore > 70).ToList();

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
            switch (message.Action)
            {
                case AppAction.FiltersChanged:
                    FiltersChanged();
                    break;
            }
        }
    }
}
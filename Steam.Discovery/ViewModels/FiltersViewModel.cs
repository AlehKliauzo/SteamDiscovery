using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Steam.Common;
using Steam.Discovery.Models;
using Steam.Discovery.Views;

namespace Steam.Discovery.ViewModels
{
    public class FiltersViewModel : ViewModelBase
    {
        private bool _updatesSuspended;
        private bool _wasHasTagsControlLastFocused = false;

        private TagsWindow _tagsWindow;
        private readonly TagsViewModel _tagsViewModel;

        public FiltersViewModel(List<Tag> tags)
        {
            SelectedTab = SelectedTab.Filters;
            _tagsViewModel = new TagsViewModel(tags);

            AppMessenger.RegisterForMessage(this, OnMessageReceived);
        }

        #region Properties 

        private SelectedTab _selectedTab;
        public SelectedTab SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                RaisePropertyChanged(() => SelectedTab);
            }
        }

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

        private bool _isNameDoesntContainFilterEnabled;
        public bool IsNameDoesntContainFilterEnabled
        {
            get { return _isNameDoesntContainFilterEnabled; }
            set
            {
                _isNameDoesntContainFilterEnabled = value;
                RaisePropertyChanged(() => IsNameDoesntContainFilterEnabled);
                FiltersChanged();
            }
        }

        private string _nameDoesntContain;
        public string NameDoesntContain
        {
            get { return _nameDoesntContain; }
            set
            {
                _nameDoesntContain = value;
                RaisePropertyChanged(() => NameDoesntContain);
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

        private bool _isHasMoreThanXTagsFilterEnabled;
        public bool IsHasMoreThanXTagsFilterEnabled
        {
            get { return _isHasMoreThanXTagsFilterEnabled; }
            set
            {
                _isHasMoreThanXTagsFilterEnabled = value;
                RaisePropertyChanged(() => IsHasMoreThanXTagsFilterEnabled);
                FiltersChanged();
            }
        }

        private string _moreThanXTags;
        public string MoreThanXTags
        {
            get { return _moreThanXTags; }
            set
            {
                _moreThanXTags = value;
                RaisePropertyChanged(() => MoreThanXTags);
                FiltersChanged();
            }
        }

        private bool _isGameScoreHigherThanXFilterEnabled;
        public bool IsGameScoreHigherThanXFilterEnabled
        {
            get { return _isGameScoreHigherThanXFilterEnabled; }
            set
            {
                _isGameScoreHigherThanXFilterEnabled = value;
                RaisePropertyChanged(() => IsGameScoreHigherThanXFilterEnabled);
                FiltersChanged();
            }
        }

        private string _gameScoreHigherThanX;
        public string GameScoreHigherThanX
        {
            get { return _gameScoreHigherThanX; }
            set
            {
                _gameScoreHigherThanX = value;
                RaisePropertyChanged(() => GameScoreHigherThanX);
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

        private string _tagsPriority;
        public string TagsPriority
        {
            get { return _tagsPriority; }
            set
            {
                _tagsPriority = value;
                RaisePropertyChanged(() => TagsPriority);
                FiltersChanged();
            }
        }

        #endregion

        #region Commands

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

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand
        {
            get { return _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag)); }
        }

        private void AddTag(string tag)
        {
            if (SelectedTab == SelectedTab.Filters)
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
            else
            {
                var textToAdd = tag + " 0, ";
                TagsPriority += textToAdd;
            }
        }

        #endregion

        public void ApplyFilters(Filters settings)
        {
            _updatesSuspended = true;

            IsNameContainsFilterEnabled = settings.IsNameContainsFilterEnabled;
            NameContains = settings.NameContains;
            IsNameDoesntContainFilterEnabled = settings.IsNameDoesntContainFilterEnabled;
            NameDoesntContain = settings.NameDoesntContain;
            IsReleasedAfterFilterEnabled = settings.IsReleasedAfterFilterEnabled;
            ReleasedAfter = settings.ReleasedAfter;
            IsHasMoreThanXTagsFilterEnabled = settings.IsMoreThanXTagsFilterEnabled;
            MoreThanXTags = settings.MoreThanXTags;
            IsGameScoreHigherThanXFilterEnabled = settings.IsGameScoreHigherThanXFilterEnabled;
            GameScoreHigherThanX = settings.GameScoreHigherThanX;
            IsMoreThanXReviewsFilterEnabled = settings.IsMoreThanXReviewsFilterEnabled;
            MoreThanXReviews = settings.MoreThanXReviews;
            IsHasTagsFilterEnabled = settings.IsHasTagsFilterEnabled;
            HasTags = settings.HasTags;
            IsDoesntHaveTagsFilterEnabled = settings.IsDoesntHaveTagsFilterEnabled;
            DoesntHaveTags = settings.DoesntHaveTags;

            TagsPriority = settings.TagsPriority;

            _updatesSuspended = false;
            FiltersChanged();
        }

        public Filters GetFilters()
        {
            var settings = new Filters();
            settings.IsNameContainsFilterEnabled = IsNameContainsFilterEnabled;
            settings.NameContains = NameContains;
            settings.IsNameDoesntContainFilterEnabled = IsNameDoesntContainFilterEnabled;
            settings.NameDoesntContain = NameDoesntContain;
            settings.IsReleasedAfterFilterEnabled = IsReleasedAfterFilterEnabled;
            settings.ReleasedAfter = ReleasedAfter;
            settings.IsMoreThanXReviewsFilterEnabled = IsMoreThanXReviewsFilterEnabled;
            settings.MoreThanXReviews = MoreThanXReviews;
            settings.IsMoreThanXTagsFilterEnabled = IsHasMoreThanXTagsFilterEnabled;
            settings.MoreThanXTags = MoreThanXTags;
            settings.IsGameScoreHigherThanXFilterEnabled = IsGameScoreHigherThanXFilterEnabled;
            settings.GameScoreHigherThanX = GameScoreHigherThanX;
            settings.IsHasTagsFilterEnabled = IsHasTagsFilterEnabled;
            settings.HasTags = HasTags;
            settings.IsDoesntHaveTagsFilterEnabled = IsDoesntHaveTagsFilterEnabled;
            settings.DoesntHaveTags = DoesntHaveTags;
            settings.TagsPriority = TagsPriority;
            return settings;
        }

        private void SaveFilters()
        {
            var filters = GetFilters();
            Serializer.SaveSettings(filters);
        }

        private void FiltersChanged()
        {
            if (_updatesSuspended)
                return;

            AppMessenger.SendMessage(AppAction.FiltersChanged);
        }

        private void OnMessageReceived(Message message)
        {
            switch (message.Action)
            {
                case AppAction.AppClosing:
                    SaveFilters();
                    break;
                case AppAction.HasTagsFocused:
                    _wasHasTagsControlLastFocused = true;
                    break;
                case AppAction.DoesntHaveTagsFocused:
                    _wasHasTagsControlLastFocused = false;
                    break;
                case AppAction.TagSelected:
                    AddTag((string)message.Data);
                    break;
            }
        }
    }
}

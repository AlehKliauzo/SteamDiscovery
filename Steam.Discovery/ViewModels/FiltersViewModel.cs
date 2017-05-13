using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Steam.Common;
using Steam.Discovery.Views;

namespace Steam.Discovery.ViewModels
{
    public class FiltersViewModel : ViewModelBase
    {
        private bool _updatesSuspended;
        private bool _wasHasTagsControlLastFocused = false;

        private TagsWindow _tagsWindow;
        private TagsViewModel _tagsViewModel;

        public FiltersViewModel(List<Tag> tags)
        {
            _tagsViewModel = new TagsViewModel(tags);
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

        #endregion

        public void ApplyFilters(Filters settings)
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
            HasTags = settings.HasTags;

            _updatesSuspended = false;
            FiltersChanged();
        }

        public Filters GetFilters()
        {
            var settings = new Filters();
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

            Messenger.Default.Send<Message>(Message.FiltersChanged);
        }

        private void OnMessageReceived(Message message)
        {
            switch (message)
            {
                case Message.AppClosing:
                    SaveFilters();
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

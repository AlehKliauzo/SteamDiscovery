using GalaSoft.MvvmLight;
using Steam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Steam.Discovery.Models;

namespace Steam.Discovery.ViewModels
{
    public class TagsViewModel : ViewModelBase
    {
        private readonly List<Tag> _allTags;

        public TagsViewModel(List<Tag> tags)
        {
            _allTags = tags;
            Tags = _allTags.OrderByDescending(x => x.GamesCount).ToList();
        }

        #region Properties

        private string _nameFilter;
        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                _nameFilter = value;
                RaisePropertyChanged(() => NameFilter);
                FilterChanged();
            }
        }

        private List<Tag> _tags;
        public List<Tag> Tags
        {
            get { return _tags; }
            set
            {
                _tags = value;
                RaisePropertyChanged(() => Tags);
            }
        }

        #endregion

        #region Commands

        private RelayCommand<string> _addTagCommand;
        public RelayCommand<string> AddTagCommand
        {
            get { return _addTagCommand ?? (_addTagCommand = new RelayCommand<string>(AddTag)); }
        }

        private void AddTag(string tag)
        {
            AppMessenger.SendMessage(AppAction.TagSelected, tag);
        }

        #endregion

        private void FilterChanged()
        {
            var name = NameFilter;
            Tags = _allTags.Where(x => x.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1).
                            OrderByDescending(x => x.GamesCount).ToList();

        }
    }
}

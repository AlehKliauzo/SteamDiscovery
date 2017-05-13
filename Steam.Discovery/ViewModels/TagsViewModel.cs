using GalaSoft.MvvmLight;
using Steam.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Discovery.ViewModels
{
    public class TagsViewModel : ViewModelBase
    {
        private List<Tag> _allTags;

        public TagsViewModel()
        {

        }

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

        private void FilterChanged()
        {
            var name = NameFilter;
            Tags = _allTags.Where(x => x.Name.IndexOf(name, StringComparison.InvariantCultureIgnoreCase) != -1).
                            OrderByDescending(x => x.GamesCount).ToList();

        }
    }
}

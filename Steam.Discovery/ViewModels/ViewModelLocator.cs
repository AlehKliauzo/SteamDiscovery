using System.Collections.Generic;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using Steam.Common;

namespace Steam.Discovery.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public TagsViewModel Tags
        {
            get { return new TagsViewModel(new List<Tag>());}
        }

        public FiltersViewModel Filters
        {
            get { return new FiltersViewModel(new List<Tag>());}
        }
    }
}
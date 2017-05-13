using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Discovery.ViewModels
{
    public class TagSympathyViewModel : ViewModelBase
    {
        public string Name { get; set; }

        private string _sympathy;
        public string Sympathy
        {
            get { return _sympathy; }
            set
            {
                _sympathy = value;
                RaisePropertyChanged(() => Sympathy);
                TagSympathyChanged();
            }
        }

        private void TagSympathyChanged()
        {
            Messenger.Default.Send<Message>(Message.TagSympathyChanged);
        }
    }
}

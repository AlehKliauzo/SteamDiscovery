using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Steam.Discovery.Models;

namespace Steam.Discovery.Converters
{
    public class SelectedTabToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tab = (SelectedTab) value;
            return (int) tab;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tab = (int)value;
            return (SelectedTab)tab;
        }
    }
}

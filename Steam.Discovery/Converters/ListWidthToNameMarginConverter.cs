using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Steam.Discovery.Converters
{
    class ListWidthToNameMarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var gamesListWidth = (double)values[0];
            var textBlockWidth = (double)values[1];

            var difference = gamesListWidth - textBlockWidth;
            if(difference < 180)
            {
                return new Thickness(5, 20, 5, 5);
            }
            else
            {
                return new Thickness(5, 0, 5, 5);
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

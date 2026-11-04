using System;
using System.Globalization;

namespace OWCE.Converters
{
    public class SelectedButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                if (isSelected)
                {
                    return OWCE.Themes.ThemeManager.GetStyle("SelectedRoundButtonStyle");
                }
            }

            return OWCE.Themes.ThemeManager.GetStyle("RoundButtonStyle");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

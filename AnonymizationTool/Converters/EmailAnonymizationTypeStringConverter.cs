using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using static AnonymizationTool.Settings.IEmailSettings;

namespace AnonymizationTool.Converters
{
    public class EmailAnonymizationTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || !(value is AnonymizationType))
            {
                return null;
            }

            switch((AnonymizationType)value)
            {
                case AnonymizationType.Random:
                    return "Zufällige Zeichenfolge";

                case AnonymizationType.FirstnameLastname:
                    return "vorname.nachname";

                case AnonymizationType.FLastname:
                    return "v.nachname";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

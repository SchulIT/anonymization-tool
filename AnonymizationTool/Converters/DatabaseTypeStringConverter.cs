using AnonymizationTool.Data;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AnonymizationTool.Converters
{
    public class DatabaseTypeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null || !(value is DatabaseType))
            {
                return null;
            }

            switch((DatabaseType)value)
            {
                case DatabaseType.Access:
                    return "Access";

                case DatabaseType.MSSQL:
                    return "MSSQL Server";

                case DatabaseType.MySQL:
                    return "MySQL/MariaDB";

                case DatabaseType.SQLite:
                    return "SQLite";
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

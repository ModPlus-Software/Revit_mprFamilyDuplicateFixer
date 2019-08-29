namespace mprFamilyDuplicateFixer.View
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <inheritdoc />
    public class BooleanMultiConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool b && b)
                    continue;
                return false;
            }

            return true;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

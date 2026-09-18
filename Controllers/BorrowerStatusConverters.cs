using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using CoinTrace.WPF.Models;

namespace CoinTrace.WPF.Converters
{
    /// <summary>Maps a BorrowerStatus to the solid color its badge is filled with.</summary>
    public class BorrowerStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as BorrowerStatus? ?? BorrowerStatus.Active;
            string hex = status switch
            {
                BorrowerStatus.Active => "#12B76A",    // green
                BorrowerStatus.PastDue => "#F79009",   // amber
                BorrowerStatus.Defaulted => "#E5484D", // red
                BorrowerStatus.PaidOff => "#6B7280",   // gray
                _ => "#6B7280"
            };
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }

    /// <summary>Maps a BorrowerStatus to its display label.</summary>
    public class BorrowerStatusToLabelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as BorrowerStatus? ?? BorrowerStatus.Active;
            return status switch
            {
                BorrowerStatus.Active => "Active",
                BorrowerStatus.PastDue => "Past due",
                BorrowerStatus.Defaulted => "Defaulted",
                BorrowerStatus.PaidOff => "Paid off",
                _ => status.ToString()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}

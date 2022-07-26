using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Auction.Desktop.ViewModel
{
    /// <summary>
    /// logikai értéket egy tárgyra vonatkozó Licitálás aktívságát jelző szövegre konvertálja az alábbi megfeleltetéssel:
    ///  True->"a Licit Aktív, False->"a Licit már/még NEM aktív"
    /// </summary>
    public class BooleanToIsActiveConverter : IValueConverter
    {
        public object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            if (!(value is Boolean))
                return Binding.DoNothing;

            try
            {
                if ((bool)value)
                {
                    return "a Licit Aktív";
                }
                else
                {
                    return "a Licit már/még NEM aktív";
                }

            }
            catch
            {
                return Binding.DoNothing;
            }
        }

        public object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

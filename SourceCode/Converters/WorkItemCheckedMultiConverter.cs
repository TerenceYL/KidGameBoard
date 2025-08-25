using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace KidGameBoard.Converters
{
    public class WorkItemCheckedMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var checkedIds = values[0] as ObservableCollection<string>;
            var workItemId = values[1] as string;
            return checkedIds != null && workItemId != null && checkedIds.Contains(workItemId);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}
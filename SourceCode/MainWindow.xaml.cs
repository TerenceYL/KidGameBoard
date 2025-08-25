using KidGameBoard.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using KidGameBoard.Views;

namespace KidGameBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            MainContent.Content = new DailyRecordMaintain(); // 預設顯示
        }

        private void BaseInfo_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new BaseInfoMaintain();
        }

        private void DailyRecord_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DailyRecordMaintain();
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new NewReportViewer();
        }

        private void PointRedemption_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new PointRedemption();
        }
    }
}
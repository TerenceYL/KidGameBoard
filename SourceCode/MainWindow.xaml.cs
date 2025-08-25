using KidGameBoard.ViewModels;
using KidGameBoard.Views;
using System.Windows;

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
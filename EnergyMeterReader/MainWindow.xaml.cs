using System.Windows;

namespace EnergyMeter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //var database = new Services.Database(@"G:\Subject\IOT4.0\ServerData\Session.sdf");
            InitializeComponent();
            DataContext = new MainWindowVM();
            Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, System.EventArgs e)
        {
            (DataContext as MainWindowVM).Close();
        }
    }
}

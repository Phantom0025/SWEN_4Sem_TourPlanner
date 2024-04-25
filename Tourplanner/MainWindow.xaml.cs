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
using TourPlanner.BL;
using TourPlanner.DAL;
using TourPlanner.UI;

namespace Tourplanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var dbContext = new AppDbContext();
            ITourService tourService = new TourService(dbContext);
            ITourLogService tourLogService = new TourLogService(dbContext);
            DataContext = new MainViewModel(dbContext, tourService, tourLogService);
        }
    }
}
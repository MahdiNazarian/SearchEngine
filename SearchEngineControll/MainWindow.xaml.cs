using BussinessLogic.Crawler;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SearchEngineControll
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly SearchEngineDbContext _context;
        public MainWindow(SearchEngineDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void AddSeedButton_Click(object sender, RoutedEventArgs e)
        {
            string link = Seed.Text;
            CrawlerBLL bll = new CrawlerBLL(_context);
            bll.AddSeed(link);
        }
    }
}

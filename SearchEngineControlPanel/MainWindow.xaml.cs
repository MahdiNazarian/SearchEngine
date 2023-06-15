using BussinessLogic.Crawler;
using BussinessLogic.Indexer;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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

namespace SearchEngineControlPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///             //HttpClientHandler handler = new HttpClientHandler()
    //{
    //    UseDefaultCredentials = true,
    //    AllowAutoRedirect = false,
    //};
    //HttpClient client = new HttpClient(handler);
    //using HttpResponseMessage response = await client.GetAsync("https://www.africau.edu/images/default/sample.pdf");
    //response.EnsureSuccessStatusCode();
    //string responseBody = await response.Content.ReadAsStringAsync();
    public partial class MainWindow : Window
    {
        private SearchEngineDbContext _context;
        private List<LinksDataGridModel> dataGridModels;
        private CancellationTokenSource CrawlerTaskCancellationTokenSource;
        private CancellationToken CrawlerTaskCancellationToken;
        private CancellationTokenSource IndexerTaskCancellationTokenSource;
        private CancellationToken IndexerTaskCancellationToken;
        private string connectionString = "Server=LAPTOP-4L8IOKSN\\MSSQLSERVER2;Database=search_engine;Trusted_Connection=True;MultipleActiveResultSets=true";
        public MainWindow()
        {
            InitializeComponent();
            DbContextOptions<SearchEngineDbContext> options = new DbContextOptionsBuilder<SearchEngineDbContext>()
                .UseSqlServer(connectionString).Options;
            _context = new SearchEngineDbContext(options);
            SetLinksDataGridItems();
            CrawlerBLL CrawlerBLL = new CrawlerBLL(_context);
            IndexerBLL IndexerBLL = new IndexerBLL(_context);
            CrawlerTaskCancellationTokenSource = new CancellationTokenSource();
            CrawlerTaskCancellationToken = CrawlerTaskCancellationTokenSource.Token;
            IndexerTaskCancellationTokenSource = new CancellationTokenSource();
            IndexerTaskCancellationToken = IndexerTaskCancellationTokenSource.Token;
        }

        private async void SeedAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (Uri.IsWellFormedUriString(SeedTextBox.Text, UriKind.Absolute))
            {
                SeedAddButton.IsEnabled = false;
                SeedTextBox.IsEnabled = false;
                AddSeedLoading.Visibility = Visibility.Visible;
                try
                {
                    CrawlerBLL Bll = new CrawlerBLL(_context);
                    Links result = await Bll.AddSeed(SeedTextBox.Text);
                    if(result != null)
                    {
                        MessageBox.Show("ثبت موفقیت آمیز بود");
                        SetLinksDataGridItems();
                    }
                    else
                    {
                        MessageBox.Show("ثبت ناموفق");
                    }
                } 
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                SeedAddButton.IsEnabled = true;
                SeedTextBox.IsEnabled = true;
                AddSeedLoading.Visibility = Visibility.Hidden;
            }
            else
            {
                MessageBox.Show("فرمت لینک وارد شده اشتباه است");
            }
        }
        private void SetLinksDataGridItems()
        {
            dataGridModels = new List<LinksDataGridModel>();
            List<Links> links = _context.links.ToList();
            foreach (var link in links)
            {
                dataGridModels.Add(new LinksDataGridModel(link));
            }
            LinksList.ItemsSource = dataGridModels;
            LinksList.Items.Refresh();
        }

        private async void StartCrawl_Click(object sender, RoutedEventArgs e)
        {
            DbContextOptions<SearchEngineDbContext> options = new DbContextOptionsBuilder<SearchEngineDbContext>()
                .UseSqlServer(connectionString).Options;
            SearchEngineDbContext crawlDbContext = new SearchEngineDbContext(options);
            try
            {
                CrawlerBLL Bll = new CrawlerBLL(crawlDbContext);
                StartCrawl.IsEnabled = false;
                StopCrawl.IsEnabled = true;
                CrawlLoading.Visibility = Visibility.Visible;
                Task crawlTask = Task.Run(async () => { await Bll.ExtractLinks(CrawlerTaskCancellationToken); }, CrawlerTaskCancellationToken)
                    .ContinueWith((parent) =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            StartCrawl.IsEnabled = true;
                            StopCrawl.IsEnabled = false;
                            CrawlLoading.Visibility = Visibility.Hidden;
                        });
                        if (parent.Exception != null)
                        {
                            MessageBox.Show(parent.Exception.Message);
                        }
                        else if(parent.IsCompletedSuccessfully)
                        {
                            MessageBox.Show("تمام لینک ها خزش شدند");
                        }else if (parent.IsCanceled)
                        {
                            MessageBox.Show("عملیات با موفقیت متوقف شد");
                        }
                    });
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                StartCrawl.IsEnabled = true;
                StopCrawl.IsEnabled = false;
                CrawlLoading.Visibility = Visibility.Hidden;
            }
        }

        private void StopCrawl_Click(object sender, RoutedEventArgs e)
        {
            CrawlerTaskCancellationTokenSource.Cancel();
            StopCrawl.IsEnabled = false;
        }

        private void RefreshLinksListButton_Click(object sender, RoutedEventArgs e)
        {
            SetLinksDataGridItems();
        }

        private async void StartIndexing_Click(object sender, RoutedEventArgs e)
        {
            DbContextOptions<SearchEngineDbContext> options = new DbContextOptionsBuilder<SearchEngineDbContext>()
                .UseSqlServer(connectionString).Options;
            SearchEngineDbContext indexDbContext = new SearchEngineDbContext(options);
            try
            {
                IndexerBLL Bll = new IndexerBLL(indexDbContext);
                StartIndexing.IsEnabled = false;
                StopIndexing.IsEnabled = true;
                IndexLoading.Visibility = Visibility.Visible;
                Task indexTask = Task.Run(async () => await Bll.Indexer(IndexerTaskCancellationToken), IndexerTaskCancellationToken)
                    .ContinueWith((parent) =>
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            StartIndexing.IsEnabled = true;
                            StopIndexing.IsEnabled = false;
                            IndexLoading.Visibility = Visibility.Hidden;
                        });
                        if (parent.Exception != null)
                        {
                            MessageBox.Show(parent.Exception.Message);
                        }
                        else if (parent.IsCompletedSuccessfully)
                        {
                            MessageBox.Show("تمام لینک ها نمایه سازی شدند");
                        }
                        else if (parent.IsCanceled)
                        {
                            MessageBox.Show("عملیات با موفقیت متوقف شد");
                        }
                    });
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                StartIndexing.IsEnabled = true;
                StopIndexing.IsEnabled = false;
                IndexLoading.Visibility = Visibility.Hidden;
            }
        }

        private void StopIndexing_Click(object sender, RoutedEventArgs e)
        {
            IndexerTaskCancellationTokenSource.Cancel();
            StopIndexing.IsEnabled = false;
        }

    }
}

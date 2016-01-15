namespace BulkBlockEditor
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            (this.DataContext as MainViewModel).RequestClose += ((o, e) => this.Close());
        }
    }
}
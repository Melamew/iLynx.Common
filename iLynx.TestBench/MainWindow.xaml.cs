namespace iLynx.TestBench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(HellViewModel viewModel)
            : this()
        {
            DataContext = viewModel;
        }
    }
}

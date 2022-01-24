using GrimMerger.ViewModels;
using GrimMerger.Views.Base;

namespace GrimMerger.Views
{
    public partial class MainWindow : RMainWindow
    {
        public MainWindow() : base()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}

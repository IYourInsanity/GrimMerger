using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;

            var scrollViewer = FindScrollViewer(listBox);

            scrollViewer.ScrollChanged += (o, args) =>
            {
                if (args.ExtentHeightChange > 0)
                    scrollViewer.ScrollToBottom();
            };
        }

        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            var queue = new Queue<DependencyObject>(new[] { root });

            do
            {
                var item = queue.Dequeue();

                if (item is ScrollViewer)
                    return (ScrollViewer)item;

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(item, i));
            } while (queue.Count > 0);

            return default(ScrollViewer);
        }
    }
}

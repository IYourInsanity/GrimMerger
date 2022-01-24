using System;
using System.Windows;
using System.Windows.Input;
using RubyUIExtension.Interfaces.ViewModels;
using RubyUIExtension.Interfaces.Windows;

namespace RubyUIExtension.Windows
{
    public abstract class RWindow : Window, IRWindow
    {

    }

    public abstract class RWindow<TViewModel> : RWindow, IRWindow<TViewModel>
        where TViewModel : class, IRWindowViewModel
    {
        protected TViewModel? Model => DataContext as TViewModel;

        #region Constructors

        protected RWindow() : base()
        {
            this.Loaded += Window_Loaded;
        }

        #endregion

        #region Default event implementation

        protected virtual void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Model?.Initialize(this);
        }

        protected virtual void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                UpdateSize();
                return;
            }

            this.DragMove();
        }

        protected virtual void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected virtual void Resize_Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateSize();
        }

        protected virtual void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            if (Model?.Exit() == true)
            {
                Environment.Exit(0);
            }
        }

        #endregion


        private void UpdateSize()
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

    }
}

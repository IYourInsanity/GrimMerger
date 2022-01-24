using System;
using System.Threading.Tasks;
using System.Windows;

namespace RubyUIExtension.Helpers
{
    public static class UIHelper
    {
        public static void UpdateUI(Action action)
        {
            if ((action == null) || (Application.Current == null))
            {
                return;
            }

            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                try
                {
                    dispatcher.Invoke(action);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}

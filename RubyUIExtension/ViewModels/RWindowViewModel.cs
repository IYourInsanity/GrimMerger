using RubyUIExtension.Common;
using RubyUIExtension.Interfaces.ViewModels;
using RubyUIExtension.Interfaces.Windows;

namespace RubyUIExtension.ViewModels
{
    public abstract class RWindowViewModel<TViewModel> : BindingSource, IRWindowViewModel
        where TViewModel : class, IRWindowViewModel
    {
        protected IRWindow? window;
        
        public virtual void Initialize(IRWindow? window)
        {
            this.window = window;
        }

        public abstract bool Exit();
    }
}

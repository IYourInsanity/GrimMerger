using RubyUIExtension.Interfaces.ViewModels;

namespace RubyUIExtension.Interfaces.Windows
{
    public interface IRWindow 
    {

    }
    public interface IRWindow<TViewModel> : IRWindow
        where TViewModel : class, IRWindowViewModel
    {

    }
}

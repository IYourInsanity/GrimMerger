using RubyUIExtension.Interfaces.Windows;

namespace RubyUIExtension.Interfaces.ViewModels
{
    public interface IRWindowViewModel
    {
        void Initialize(IRWindow? window);

        bool Exit();
    }
}

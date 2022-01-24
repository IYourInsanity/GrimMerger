using RubyUIExtension.Interfaces.ViewModels;

namespace GrimMerger.Interfaces
{
    public interface IMainViewModel : IRWindowViewModel
    {
        void OpenFolderDialog();
        void MergeMods();
        bool CanMergeMods();

    }
}

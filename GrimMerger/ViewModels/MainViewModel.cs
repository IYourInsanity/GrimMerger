using System;
using GrimMerger.BusinessModels;
using GrimMerger.Interfaces;
using RubyUIExtension.Common;
using RubyUIExtension.ViewModels;

using WinForm = System.Windows.Forms;

namespace GrimMerger.ViewModels
{
    public sealed class MainViewModel : RWindowViewModel<IMainViewModel>, IMainViewModel
    {
        private MainBusinessModel _mainBM;


        #region Properties

        public string PathToGameFolder
        {
            get => _mainBM.pathToGameFolder;
            set => SetValue(ref _mainBM.pathToGameFolder, value, nameof(PathToGameFolder));
        }

        #endregion

        #region Commands

        private RCommand openFolderDialogCommand;
        public RCommand OpenFolderDialogCommand => RCommand.CreateCommand(ref openFolderDialogCommand, OpenFolderDialog);

        private RCommand mergeModsCommand;
        public RCommand MergeModsCommand => RCommand.CreateCommand(ref mergeModsCommand, MergeMods, CanMergeMods);

        #endregion

        #region Constructors

        public MainViewModel() : base()
        {
            _mainBM = new MainBusinessModel();
        }

        #endregion

        #region Implementation of Abstraction

        public override void UpdateAllValue()
        {
            UpdateValue(nameof(PathToGameFolder));
        }

        #endregion

        #region Overrides of RWindowViewModel


        public override bool Exit()
        {
            return true;
        }

        #endregion

        #region Implementation of IMainViewModel

        public void OpenFolderDialog()
        {
            var openFolderDialog = new WinForm.FolderBrowserDialog()
            {
                InitialDirectory = Environment.CurrentDirectory
            };

            if (openFolderDialog.ShowDialog() != WinForm.DialogResult.OK) 
                return;

            var selectedPath = openFolderDialog.SelectedPath;

            if (_mainBM.VerifiedPath(selectedPath))
            {
                PathToGameFolder = selectedPath;
            }
            else
            {
                //IMPLEMENT MESSAGE BOX
            }
        }

        public void MergeMods()
        {

        }

        public bool CanMergeMods()
        {
            return true;
        }

        #endregion

    }
}

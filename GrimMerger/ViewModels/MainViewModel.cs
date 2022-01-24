using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using GrimMerger.BusinessModels;
using GrimMerger.Interfaces;
using GrimMerger.Models;
using RubyUIExtension.Common;
using RubyUIExtension.Helpers;
using RubyUIExtension.Interfaces.Windows;
using RubyUIExtension.ViewModels;

using WinForm = System.Windows.Forms;

namespace GrimMerger.ViewModels
{
    public sealed class MainViewModel : RWindowViewModel<IMainViewModel>, IMainViewModel
    {
        private MainBusinessModel _mainBM;


        #region Properties

        public string PathToFolder
        {
            get => _mainBM.pathToFolder;
            set => SetValue(ref _mainBM.pathToFolder, value, nameof(PathToFolder));
        }

        public string PathToExe
        {
            get => _mainBM.pathToExe;
            set => SetValue(ref _mainBM.pathToExe, value, nameof(PathToExe));
        }

        public ObservableCollection<string> MessageCollection => _mainBM.messageCollection;
        public ObservableCollection<Mod> ModCollection => _mainBM.modCollection;

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
            _mainBM = new MainBusinessModel(UpdateAllValue);
        }

        #endregion

        #region Implementation of Abstraction

        public override void UpdateAllValue()
        {
            UpdateValue(nameof(PathToFolder));

            UpdateValue(nameof(MessageCollection));
            UpdateValue(nameof(ModCollection));
        }

        #endregion

        #region Overrides of RWindowViewModel

        public override void Initialize(IRWindow? window)
        {
            base.Initialize(window);
            _mainBM.Initialize();
        }

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
                InitialDirectory = PathToFolder.Equals(string.Empty) ? Environment.CurrentDirectory : PathToFolder,
            };

            if (openFolderDialog.ShowDialog() != WinForm.DialogResult.OK) 
                return;

            var selectedPath = openFolderDialog.SelectedPath;

            if (_mainBM.VerifiedPath(selectedPath))
            {
                PathToFolder = selectedPath;
                PathToExe = _mainBM.SearchExeFile(selectedPath);
            }
            else
            {
                //IMPLEMENT MESSAGE BOX
            }
        }

        public void MergeMods()
        {
            _mainBM.MergeMods(AddMessage);
        }

        public bool CanMergeMods()
        {
            return true;
        }

        #endregion

        private void AddMessage(CLMessage message)
        {
            UIHelper.UpdateUI(() =>
            {
                MessageCollection.Add(message.Value);
            });
        }

    }
}

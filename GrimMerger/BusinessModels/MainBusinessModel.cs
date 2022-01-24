using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GrimMerger.Models;
using RubyUIExtension.Helpers;

namespace GrimMerger.BusinessModels
{
    internal class MainBusinessModel
    {
        private const string EXECUTABLE_EXTENSION = ".exe";
        private const string ARCHIVE_TOOL_FILENAME = "ArchiveTool";
        private const string GRIM_DAWN_FILENAME = "Grim Dawn";

        private const string PATH_MOD_PART = "mods";

        #region Fields

        private Action updateAllValueAction;

        private readonly HashSet<string> _fileNameHashSet;

        internal string pathToFolder;
        internal string pathToExe;
        internal string pathToModFolder;

        internal ObservableCollection<string> messageCollection;
        internal ObservableCollection<Mod> modCollection;

        #endregion

        #region Constructors

        internal MainBusinessModel(Action updateAllValueAction)
        {
            this.updateAllValueAction = updateAllValueAction;
            _fileNameHashSet = new HashSet<string>
            {
                ARCHIVE_TOOL_FILENAME
            };

            messageCollection = new ObservableCollection<string>();
            modCollection = new ObservableCollection<Mod>();
        }

        #endregion

        #region Implementation of Inner logic

        internal async void Initialize()
        {
            await Task.Run(() =>
            {
                var root = Path.GetPathRoot(Environment.CurrentDirectory);

                var foldersToSearch = new string[]
                {
                    Path.Combine(root, "Games"),
                    Path.Combine(root, "Program Files"),
                    Path.Combine(root, "Program Files (x86)")
                };

                SearchFolder(foldersToSearch);
                PrepareTempModFolderAndMods();

                updateAllValueAction();
            });
        }

        internal bool VerifiedPath(string? path)
        {
            if (path == null)
                return false;

            var executableFiles = Directory.GetFiles(path).Where(fileName =>
            {
                var ext = Path.GetExtension(fileName);

                return ext.Equals(EXECUTABLE_EXTENSION, StringComparison.OrdinalIgnoreCase);
            }).Select(Path.GetFileNameWithoutExtension).ToList();

            return _fileNameHashSet.Any(_ => executableFiles.Any(__ => __ != null && __.Equals(_)));
        }

        #endregion

        internal bool SearchFolder(string[] foldersToSearch)
        {
            var result = false;

            pathToFolder = string.Empty;
            pathToExe = string.Empty;

            foreach (var folderToSearch in foldersToSearch)
            {
                try
                {
                    var fileName = SearchExeFile(folderToSearch);

                    if (fileName != string.Empty)
                    {
                        pathToExe = fileName;
                        pathToFolder = folderToSearch;
                        pathToModFolder = Path.Combine(folderToSearch, PATH_MOD_PART);

                        result = true;
                    }
                    else
                    {
                        result = SearchFolder(Directory.GetDirectories(folderToSearch));
                    }

                    if(result)
                        break;
                }
                catch 
                {
                    //skip
                }

            }

            return result;
        }

        internal string SearchExeFile(string path)
        {
            var fileName = Directory.GetFiles(path)
                .Where(_ => Path.GetExtension(_).Equals(EXECUTABLE_EXTENSION, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(_ => Path.GetFileNameWithoutExtension(_).Contains(GRIM_DAWN_FILENAME));

            return fileName ?? string.Empty;

        }

        private void PrepareTempModFolderAndMods()
        {
            var tempDirectory = Path.Combine(Environment.CurrentDirectory, "Temp");

            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }

            Directory.CreateDirectory(tempDirectory);
            Directory.CreateDirectory(Path.Combine(tempDirectory, "source"));
            Directory.CreateDirectory(Path.Combine(tempDirectory, "database", "templates"));

            var directoryInfo = new DirectoryInfo(pathToModFolder);

            foreach (var modInfo in directoryInfo.GetDirectories())
            {
                UIHelper.UpdateUI(() =>
                {
                    modCollection.Add(new Mod(modInfo.FullName, modInfo.Name, true));
                });
            }
        }

    }
}

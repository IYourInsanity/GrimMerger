using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrimMerger.BusinessModels
{
    internal class MainBusinessModel
    {
        private const string EXECUTABLE_EXTENSION = ".exe";
        private const string ARCHIVE_TOOL_FILENAME = "ArchiveTool";
        private const string GRIM_DAWN_FILENAME = "Grim Dawn";

        #region Fields

        private Action updateAllValueAction;

        private readonly HashSet<string> _fileNameHashSet;

        internal string pathToFolder;
        internal string pathToExe;

        #endregion

        #region Constructors

        internal MainBusinessModel(Action updateAllValueAction)
        {
            this.updateAllValueAction = updateAllValueAction;
            _fileNameHashSet = new HashSet<string>
            {
                ARCHIVE_TOOL_FILENAME
            };
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

                SearchFolder(foldersToSearch, out pathToFolder, out pathToExe);

            }).ContinueWith(_ =>
            {
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

        internal bool SearchFolder(string[] foldersToSearch, out string pathToFolder, out string pathToExe)
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

                        result = true;
                    }
                    else
                    {
                        result = SearchFolder(Directory.GetDirectories(folderToSearch), out pathToFolder, out pathToExe);
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

    }
}

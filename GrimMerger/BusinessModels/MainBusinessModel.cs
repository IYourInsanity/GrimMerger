using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrimMerger.BusinessModels
{
    internal class MainBusinessModel
    {
        private const string EXECUTABLE_EXTENSION = "exe";
        private const string ARCHIVE_TOOL_FILENAME = "ArchiveTool";



        #region Fields

        private readonly HashSet<string> _fileNameHashSet = new()
        {
            ARCHIVE_TOOL_FILENAME
        };

        internal string pathToGameFolder;

        #endregion

        #region Implementation of Inner logic

        internal bool VerifiedPath(string? path)
        {
            if (path == null)
                return false;

            var executableFiles = Directory.GetFiles(path).Where(fileName =>
            {
                var ext = Path.GetExtension(fileName);

                return ext.Equals(EXECUTABLE_EXTENSION, StringComparison.OrdinalIgnoreCase);
            }).Select(_ => _);

            return executableFiles.ToHashSet().SetEquals(_fileNameHashSet);
        }

        #endregion



    }
}

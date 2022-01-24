using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrimMerger.Enums;
using GrimMerger.Extensions;
using GrimMerger.Models;
using RubyUIExtension.Helpers;

namespace GrimMerger.BusinessModels
{
    internal class MainBusinessModel
    {
        private const string EXECUTABLE_EXTENSION = ".exe";
        private const string DATABASE_EXTENSION = ".arz";
        private const string PACKAGE_EXTENSION = ".arc";


        private const string ARCHIVE_TOOL_FILENAME = "ArchiveTool";
        private const string GRIM_DAWN_FILENAME = "Grim Dawn";

        private const string PATH_MOD_PART = "mods";

        #region Fields

        private Action updateAllValueAction;

        private readonly HashSet<string> _fileNameHashSet;
        private readonly Lazy<string> tempFolder = new Lazy<string>(() => Path.Combine(Environment.CurrentDirectory, "Temp"));

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

                var directoryInfo = new DirectoryInfo(pathToModFolder);

                foreach (var modInfo in directoryInfo.GetDirectories())
                {
                    UIHelper.UpdateUI(() =>
                    {
                        modCollection.Add(new Mod(modInfo.FullName, modInfo.Name, true));
                    });
                }

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

        internal async void MergeMods(Action<CLMessage> callbackVisual)
        {
            void ProcessMessage(CLMessage message)
            {
                callbackVisual(message);
            }

            await Task.Factory.StartNew(async() =>
            {
                using (var cancellationToken = new CancellationTokenSource())
                {
                    callbackVisual(CLMessage.Build("Create command line worker"));

                    using (var worker = CommandLineWorker.Build())
                    {
                        worker.OnMessageObtain += ProcessMessage;

                        worker.RegisterMessage(CLMessage.Build("Start listening"));
                        await worker.StartAsync(pathToFolder, cancellationToken.Token);
                        worker.RegisterMessage(CLMessage.Build("Copy files to temp folders"));

                        var tempFolderValue = tempFolder.Value;

                        foreach (var mod in modCollection)
                        {
                            if (mod.IsActive)
                            {
                                var modPath = mod.Path;
                                DirectoryCopy(modPath, tempFolderValue, true);

                                var resources = "resources";
                                var resourceFolder = new DirectoryInfo(Path.Combine(modPath, resources));
                                var tempResourceFolder = Path.Combine(tempFolderValue, resources);

                                worker.RegisterMessage(CLMessage.Build("Extract resources"));
                                foreach (var file in resourceFolder.GetFiles())
                                {
                                    if (file.Extension.Equals(PACKAGE_EXTENSION))
                                    {
                                        ExtractArc(worker, file.FullName, tempResourceFolder);

                                        var name = file.Name;
                                        var clearName = name.Substring(0, name.Length - 4);
                                        var parent = Path.Combine(resourceFolder.FullName, clearName);
                                        var direct = Path.Combine(tempFolderValue, "source", name);

                                        worker.RegisterMessage(CLMessage.Build("Package files"));
                                        worker.RegisterMessage(CLMessage.Build(CLMessageType.PackFiles, parent, direct));

                                        //var message = CLMessage.Build();
                                        //new Message { Command = "Pack", Args = new string[] { combinedDir + @"\source\" + @file.Name.Substring(0, file.Name.Length - 4), @file.Name.Substring(0, file.Name.Length - 4), combinedDir + @"\resources\" + file.Name } }

                                        //worker.Messages.Enqueue();
                                    }
                                }

                                var database = "database";
                                var databaseFolder = new DirectoryInfo(Path.Combine(modPath, database));
                                var tempDatabaseFolder = Path.Combine(tempFolderValue, database);

                                worker.RegisterMessage(CLMessage.Build("Extract databases"));
                                foreach (var file in databaseFolder.GetFiles())
                                {
                                    switch (file.Extension)
                                    {
                                        case PACKAGE_EXTENSION:
                                            ExtractArc(worker, file.FullName, tempDatabaseFolder);
                                            break;
                                        case DATABASE_EXTENSION:
                                            ExtractArz(worker, file.FullName, tempDatabaseFolder);
                                            break;
                                    }
                                }
                            }
                        }
                        
                        await Task.Delay(TimeSpan.FromSeconds(30));

                        worker.RegisterMessage(CLMessage.Build("Build database"));
                        worker.RegisterMessage(CLMessage.Build(CLMessageType.PackDatabase, tempFolderValue, pathToFolder));

                        if (worker.Wait())
                        {
                            cancellationToken.Cancel();

                            worker.Stop();
                            worker.OnMessageObtain -= ProcessMessage;
                        }
                    }
                }
            });

            PrepareTempModFolder();

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

        private void PrepareTempModFolder()
        {
            var tempFolderValue = tempFolder.Value;

            if (Directory.Exists(tempFolderValue))
            {
                Directory.Delete(tempFolderValue, true);
            }

            Directory.CreateDirectory(tempFolderValue);
            Directory.CreateDirectory(Path.Combine(tempFolderValue, "source"));
            Directory.CreateDirectory(Path.Combine(tempFolderValue, "database", "templates"));
        }

        private void DirectoryCopy(string modFolder, string tempFolder, bool copySubFolders)
        {
            var folderInfo = new DirectoryInfo(modFolder);
            var foldersInfo = folderInfo.GetDirectories();
            var files = folderInfo.GetFiles();

            Directory.CreateDirectory(tempFolder);

            foreach (var file in files)
            {
                switch (file.Extension)
                {
                    case PACKAGE_EXTENSION:
                    case DATABASE_EXTENSION:
                        continue;
                }

                var tempPath = Path.Combine(tempFolder, file.Name);

                try
                {
                    file.CopyTo(tempPath, false);
                }
                catch
                {
                    file.CopyTo(tempPath, true);
                }
            }

            if (copySubFolders == false)
                return;

            foreach (var folder in foldersInfo)
            {
                var tempPath = Path.Combine(tempFolder, folder.Name);
                DirectoryCopy(folder.FullName, tempPath, copySubFolders);
            }
        }

        private void ExtractArc(CommandLineWorker worker, string source, string destination)
        {
            Directory.CreateDirectory(destination);
            worker.RegisterMessage(CLMessage.Build(CLMessageType.ExtractFiles, new[] {source, destination}));
        }

        private void ExtractArz(CommandLineWorker worker, string source, string destination)
        {
            Directory.CreateDirectory(destination);
            worker.RegisterMessage(CLMessage.Build(CLMessageType.ExtractDatabase, new[] { source, destination }));
        }
    }
}

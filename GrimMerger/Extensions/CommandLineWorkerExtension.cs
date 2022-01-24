using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrimMerger.Constants;
using GrimMerger.Enums;
using GrimMerger.Models;

namespace GrimMerger.Extensions
{
    internal static class CommandLineWorkerExtension
    {
        internal static async Task<bool> StartAsync(this CommandLineWorker worker, string pathToDir, CancellationToken token)
        {
            var commandPrompt = worker.CommandPrompt;
            var result = commandPrompt.Start();

            if (result)
            {
                worker.ProcessMessageToVisual(CommandLineMessages.Start);

                commandPrompt.BeginOutputReadLine();
                commandPrompt.OutputDataReceived += worker.OutputDataReceived;
                commandPrompt.ErrorDataReceived += worker.ErrorDataReceived;

                var standardInput = commandPrompt.StandardInput;

                await standardInput.WriteLineAsync(CommandLineMessages.Logic.GoToDir.Forge(pathToDir));
                await standardInput.FlushAsync();

                await Task.Factory.StartNew(() => worker.ProcessMessageToCommandPrompt(token), token);
            }

            return result;
        }

        internal static void Stop(this CommandLineWorker worker)
        {
            worker.ProcessMessageToVisual(CommandLineMessages.Stop);

            var commandPrompt = worker.CommandPrompt;

            commandPrompt.OutputDataReceived -= worker.OutputDataReceived;
            commandPrompt.ErrorDataReceived -= worker.ErrorDataReceived;

            worker.Dispose();
        }

        internal static void RegisterMessage(this CommandLineWorker worker, CLMessage message)
        {
            worker.Messages.Enqueue(message);
        }

        internal static bool Wait(this CommandLineWorker worker)
        {
            return worker.ManualResetEvent.WaitOne();
        }

        internal static async void ProcessMessageToCommandPrompt(this CommandLineWorker worker, CancellationToken token)
        {
            const int MAX_REPEAT_TO_SLEEP = 1000;

            var awaitTime = TimeSpan.FromMilliseconds(100);
            var standardInput = worker.CommandPrompt.StandardInput;
            var messages = worker.Messages;
            var message = default(CLMessage);
            var repeat = 0;

            while (token.IsCancellationRequested == false)
            {
                try
                {
                    if (messages.TryDequeue(out message))
                    {
                        repeat = 0;
                        var arguments = message.Arguments;
                        var type = message.Type;

                        switch (type)
                        {
                            case CLMessageType.VisualToView:
                                worker.ProcessMessageToVisual(message.Value);
                                break;
                            case CLMessageType.ExtractFiles:

                                //arguments[0] - From
                                //arguments[1] - To
                                var extractFiles = CommandLineMessages.Logic.ExtractFiles.Forge(arguments[0], arguments[1]);

                                standardInput.ProcessMessage(extractFiles);
                                break;
                            case CLMessageType.ExtractDatabase:

                                //arguments[0] - From
                                //arguments[1] - To
                                var extractDatabase = CommandLineMessages.Logic.ExtractDatabase.Forge(arguments[0], arguments[1]);

                                standardInput.ProcessMessage(extractDatabase);
                                break;
                            case CLMessageType.PackFiles:

                                //arguments[0] - To
                                //arguments[1] - ParentDir
                                var packFilesMessage = CommandLineMessages.Logic.PackFiles.Forge(arguments[0], arguments[1]);

                                standardInput.ProcessMessage(packFilesMessage);
                                break;
                            case CLMessageType.PackDatabase:

                                //arguments[0] - ModDir
                                //arguments[1] - GameDir

                                var packDatabaseMessage = CommandLineMessages.Logic.PackDatabase.Forge(Directory.GetCurrentDirectory(),
                                                                                                             arguments[0],
                                                                                                             arguments[0],
                                                                                                             arguments[1],
                                                                                                             arguments[0]);
                                standardInput.ProcessMessage(packDatabaseMessage);
                                
                                break;
                            default:
                                worker.ProcessMessageToVisual(CommandLineMessages.CouldNotProcessMessageToCommandPrompt);
                                break;
                        }
                    }

                    await Task.Delay(awaitTime, token);

                    if (repeat > MAX_REPEAT_TO_SLEEP)
                    {
                        worker.ManualResetEvent.Set();
                    }
                    else
                    {
                        repeat++;
                    }
                }
                catch (Exception exception)
                {
                    worker.ProcessMessageToVisual(exception.Message);
                }
            }
        }

        internal static bool ProcessMessageToVisual(this CommandLineWorker worker, string? data)
        {
            if (data == null)
                return false;

            worker.ProcessMessageToVisual(CLMessage.Build(data));

            return true;
        }

        private static void ProcessMessage(this StreamWriter standardInput, string message)
        {
            standardInput.WriteLine(message);
            standardInput.Flush();
        }

    }
}

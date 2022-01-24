using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GrimMerger.Constants;
using GrimMerger.Enums;
using GrimMerger.Models;

namespace GrimMerger.Helpers
{
    internal static class CommandLineWorkerHelper
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

        internal static async void ProcessMessageToCommandPrompt(this CommandLineWorker worker, CancellationToken token)
        {
            var awaitTime = TimeSpan.FromMilliseconds(100);
            var standardInput = worker.CommandPrompt.StandardInput;
            var messages = worker.Messages;
            var message = default(CLMessage);

            while (token.IsCancellationRequested == false)
            {
                try
                {
                    if (messages.TryDequeue(out message))
                    {
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

                                standardInput.ProcessMessage(CommandLineMessages.Logic.ExtractFiles.Forge(arguments[0], arguments[1]));

                                break;
                            case CLMessageType.ExtractDatabase:

                                //arguments[0] - From
                                //arguments[1] - To

                                standardInput.ProcessMessage(CommandLineMessages.Logic.ExtractDatabase.Forge(arguments[0], arguments[1]));

                                break;
                            case CLMessageType.PackFiles:

                                //arguments[0] - To
                                //arguments[1] - ParentDir

                                standardInput.ProcessMessage(CommandLineMessages.Logic.PackFiles.Forge(arguments[0], arguments[1]));

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

            worker.OnMessageObtained.Invoke(CLMessage.Build(data));

            return true;
        }

        private static void ProcessMessage(this StreamWriter standardInput, string message)
        {
            standardInput.WriteLine(message);
            standardInput.Flush();
        }

    }
}

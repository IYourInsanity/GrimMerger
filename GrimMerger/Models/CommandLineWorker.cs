using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using GrimMerger.Constants;
using GrimMerger.Extensions;

namespace GrimMerger.Models
{
    internal sealed class CommandLineWorker : IDisposable
    {
        internal const string DEFAULT_FILE_NAME = "cmd.exe";

        #region Properties

        internal ManualResetEvent ManualResetEvent { get; }

        internal ProcessStartInfo ProcessStartInfo { get; }
        internal Process CommandPrompt { get; }
        
        internal ConcurrentQueue<CLMessage> Messages { get; }

        #endregion

        internal static CommandLineWorker Build()
        {
            return new CommandLineWorker();
        }

        private CommandLineWorker()
        {
            ProcessStartInfo = new ProcessStartInfo(DEFAULT_FILE_NAME)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            CommandPrompt = new Process
            {
                StartInfo = ProcessStartInfo
            };

            Messages = new ConcurrentQueue<CLMessage>();
            ManualResetEvent = new ManualResetEvent(false);
        }

        internal void OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            this.ProcessMessageToVisual(eventArgs.Data);
        }

        internal void ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            this.ProcessMessageToVisual(eventArgs.Data);
        }

        public void Dispose()
        {
            //CommandPrompt.StandardInput.Close();
            //CommandPrompt.StandardInput.Dispose();
            CommandPrompt.WaitForExit();
            CommandPrompt.Dispose();
        }

        internal void ProcessMessageToVisual(CLMessage message)
        {
            OnMessageObtain?.Invoke(message);
        }

        internal event MessageObtain OnMessageObtain;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;

namespace PantsZipper
{
    public class AsyncWorker
    {
        public AsyncWorker(Action<object, AsyncWorker> runFunc)
        {
            Worker.DoWork += (sender, args) =>
            {
                Progress.Type = ProgressType.Running;
                runFunc(args.Argument, this);
            };
        }

        public AsyncWorker(Action<object, AsyncWorker> runFunc, Action<AsyncWorkerProgress> progressFunc)
        {
            Worker.DoWork += (sender, args) =>
            {
                Progress.Type = ProgressType.Running;
                runFunc(args.Argument, this);
            };
            Worker.ProgressChanged += (sender, args) =>
            {
                if (!StoppingWithError)
                    Progress.Type = ProgressType.Running;

                progressFunc(Progress);
            };
        }

        public AsyncWorker(System.Windows.Threading.Dispatcher dispatcher, Action<object, AsyncWorker> runFunc, Action<AsyncWorkerProgress> progressFunc, Action<AsyncWorkerProgress> onFinishFunc)
        {
            Worker.DoWork += (sender, args) =>
            {
                Progress.Type = ProgressType.Running;
                runFunc(args.Argument, this);
            };
            Worker.ProgressChanged += (sender, args) =>
            {
                if (!StoppingWithError)
                    Progress.Type = ProgressType.Running;
                progressFunc(Progress);
            };
            Worker.RunWorkerCompleted += (sender, args) =>
            {
                if (!StoppingWithError)
                    Progress.Type = ProgressType.Finished;

                dispatcher.Invoke(delegate ()
                {
                    onFinishFunc(Progress);
                });
            };
        }

        public void Run(object arg = null)
        {
            Worker.RunWorkerAsync(arg);
            Progress.Type = ProgressType.Running;
        }

        public void ReportProgress(AsyncWorkerProgress progress)
        {
            Progress = progress;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(string progressText)
        {
            Progress.ProgressText = progressText;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(double progressPercentage)
        {
            Progress.ProgressPercentage = progressPercentage;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(object argument)
        {
            Progress.Argument = argument;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(string progressText, double progressPercentage)
        {
            Progress.ProgressText = progressText;
            Progress.ProgressPercentage = progressPercentage;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(string progressText, object argument)
        {
            Progress.ProgressText = progressText;
            Progress.Argument = argument;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(double progressPercentage, object argument)
        {
            Progress.ProgressPercentage = progressPercentage;
            Progress.Argument = argument;
            Worker.ReportProgress(0);
        }

        public void ReportProgress(string progressText, double progressPercentage, object argument)
        {
            Progress.ProgressText = progressText;
            Progress.ProgressPercentage = progressPercentage;
            Progress.Argument = argument;
            Worker.ReportProgress(0);
        }

        public void StopWithError(string errorText)
        {
            StoppingWithError = true;
            Progress.Type = ProgressType.FinishedWithError;
            Progress.ProgressText = errorText;
        }

        public void StopWithError(string errorText, object argument)
        {
            StoppingWithError = true;
            Progress.Type = ProgressType.FinishedWithError;
            Progress.ProgressText = errorText;
            Progress.Argument = argument;
        }

        public AsyncWorkerProgress GetProgress()
        {
            return Progress;
        }

        private readonly BackgroundWorker Worker = new BackgroundWorker()
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true,
        };

        private AsyncWorkerProgress Progress = new AsyncWorkerProgress()
        {
            Type = ProgressType.NotStarted
        };

        public class AsyncWorkerProgress
        {
            public ProgressType Type;
            public string ProgressText;
            public double ProgressPercentage;
            public object Argument;
        }
        public enum ProgressType
        {
            Running,
            Finished,
            FinishedWithError,
            Cancelled,
            NotStarted,
            Error,
        }

        private bool StoppingWithError;

        public Action<AsyncWorkerProgress> OnFinishFunction = (progress) => { };
    }

    public class WorkerQueue
    {
        private static readonly Dictionary<AsyncWorker, object> Workers = new Dictionary<AsyncWorker, object>();
        public int MaxRunning = 1;
        private static readonly Timer ManagerTimer = new Timer()
        {
            Interval = 100,
        };

        public WorkerQueue(int maxRunning = 1)
        {
            MaxRunning = maxRunning;
            ManagerTimer.Elapsed += (sender, args) =>
            {
                if (Workers.Count == 0)
                {
                    ManagerTimer.Stop();
                    return;
                }

                for (int i = 0; i < Math.Min(maxRunning, Workers.Count); i++)
                {
                    var worker = Workers.ElementAt(i).Key;
                    var progress = worker.GetProgress();
                    if (progress.Type == AsyncWorker.ProgressType.NotStarted)
                    {
                        worker.Run(Workers.ElementAt(i).Value);
                    }
                    else if (progress.Type == AsyncWorker.ProgressType.Finished ||
                             progress.Type == AsyncWorker.ProgressType.Cancelled ||
                             progress.Type == AsyncWorker.ProgressType.FinishedWithError)
                    {
                        Workers.Remove(Workers.ElementAt(i).Key);
                        i--;
                    }
                }
            };
        }

        public void Add(AsyncWorker worker, object argumentToSend = null)
        {
            Workers.Add(worker, argumentToSend);
            ManagerTimer.Start();
        }

        public void Remove(AsyncWorker worker)
        {
            Workers.Remove(worker);
        }

        public void RemoveAt(int index)
        {
            Workers.Remove(Workers.ElementAt(index).Key);
        }
    }
}

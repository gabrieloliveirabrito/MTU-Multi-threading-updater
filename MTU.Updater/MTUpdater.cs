using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace MTU.Updater
{
    using Args;
    using Enums;
    using Models;

    public class MTUpdater : IDisposable
    {
        Dictionary<string, Worker> clients;
        bool result = false;

        public event EventHandler Started;
        public event EventHandler<ResultEventArgs> Finished;
        public event EventHandler<RetryEventArgs> Failed;
        public event EventHandler<ErrorEventArgs> ErrorThrowed;
        public event EventHandler<StateEventArgs> StateChanged;
        public event EventHandler<WorkerAppendEventArgs> WorkerAppend;
        public event EventHandler<ProgressEventArgs> WorkerProgress;

        UpdaterState _state;
        public UpdaterState State
        {
            get { return _state; }
            private set
            {
                _state = value;

                switch (value)
                {
                    case UpdaterState.Started:
                        if (Started != null)
                            Started(this, EventArgs.Empty);
                        break;
                    case UpdaterState.Finished:
                        if (Finished != null)
                            Finished(this, new ResultEventArgs(result));
                        break;
                }

                if (StateChanged != null)
                    StateChanged(this, new StateEventArgs(value));
            }
        }

        public string XMLLink
        {
            get; set;
        }

        public MTUpdater()
        {
            clients = new Dictionary<string, Worker>();
        }

        public MTUpdater(string xmlLink) : this()
        {
            XMLLink = xmlLink;
        }

        public void Dispose()
        {
            foreach (var client in clients.Values)
                client.Client.Dispose();
            clients.Clear();
        }

        int calculateNeededThreads(int count)
        {
            int nt = 1, i = 1;
            while (i < MTUConstants.MaximumThreads)
            {
                if (count % i == 0)
                    nt = i;
                i++;
            }

            return nt;
        }

        void InvokeError(Exception ex)
        {
            State = UpdaterState.Failed;
            if (ErrorThrowed != null)
                ErrorThrowed(this, new ErrorEventArgs(ex));
        }

        bool InvokeFailed()
        {
            State = UpdaterState.Failed;
            if (Failed != null)
            {
                var args = new RetryEventArgs();
                Failed(this, args);

                if (!args.Retry)
                {
                    result = false;
                    State = UpdaterState.Finished;
                }

                return args.Retry;
            }
            return false;
        }

        Worker CreateWorker(string name, WorkerType type)
        {
            lock (clients)
            {
                var client = new WebClient();
                client.DownloadProgressChanged += Client_DownloadProgressChanged;

                var worker = new Worker(name, type, client);
                clients.Add(name, worker);

                if (WorkerAppend != null)
                    WorkerAppend(this, new WorkerAppendEventArgs(name, type));
                return worker;
            }
        }

        void UpdateWorkerProgress(Worker worker, int progress)
        {
            if (WorkerProgress != null)
                WorkerProgress(this, new ProgressEventArgs(progress, worker.Name, worker.Type));
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var worker = e.UserState as Worker;
            UpdateWorkerProgress(worker, e.ProgressPercentage);
        }

        public void Update()
        {
            try
            {
                State = UpdaterState.Started;

                foreach (var client in clients.Values)
                    client.Client.Dispose();
                clients.Clear();

                var worker = CreateWorker("ListChecker", WorkerType.Verificate);
                worker.Client.DownloadDataCompleted += Client_DownloadDataCompleted;

                worker.Client.DownloadDataAsync(new Uri(XMLLink), worker);
                State = UpdaterState.DownloadingVerification;
            }
            catch (Exception ex)
            {
                InvokeError(ex);

                if (InvokeFailed())
                    Update();
            }
        }

        private void Client_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var worker = e.UserState as Worker;
            UpdateWorkerProgress(worker, 100);

            if (e.Cancelled && InvokeFailed())
                Update();
            else if (e.Error != null)
            {
                InvokeError(e.Error);
                if (InvokeFailed())
                    Update();
            }
            else
            {

                State = UpdaterState.CheckingFiles;
            }
        }
    }
}
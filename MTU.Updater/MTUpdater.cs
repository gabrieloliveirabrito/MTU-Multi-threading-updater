using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace MTU.Updater
{
    using Args;
    using Enums;
    using Models;
    using Comparers;

    public class MTUpdater : IDisposable
    {
        Dictionary<string, Worker> clients;
        List<Update> verificationList;
        HashSet<Update> downloadQueue;

        bool result = false;
        int current = 0, total = 0;

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

        public string XMLLink { get; set; }
        public string BasePath { get; set; }
        public HashAlgorithm Algorithm { get; set; }
        public Func<Stream, string> HashFunction { get; set; }

        public int CurrentUpdate
        {
            get { return current; }
        }

        public int TotalUpdates
        {
            get { return total; }
        }

        public MTUpdater()
        {
            clients = new Dictionary<string, Worker>();
            downloadQueue = new HashSet<Update>(new UpdateComparer());
            verificationList = new List<Update>();

            Algorithm = MD5.Create();
            HashFunction = BaseHashing;
        }

        public MTUpdater(string xmlLink) : this()
        {
            XMLLink = xmlLink;
        }

        public void Dispose()
        {
            foreach (var client in clients.Values)
                if (client.RealWorker is WebClient)
                    (client.RealWorker as WebClient).Dispose();
                else if (client.RealWorker is Thread)
                    (client.RealWorker as Thread).Interrupt();
            clients.Clear();
        }

        string BaseHashing(Stream s)
        {
            Algorithm.Clear();
            var buf = Algorithm.ComputeHash(s);

            return BitConverter.ToString(buf).Replace("-", string.Empty);
        }

        bool IsPrime(int n)
        {
            if (n < 5 || n % 2 == 0 || n % 3 == 0)
                return n == 2 || n == 3;
            else
            {
                var maxim = Math.Sqrt(n) + 2;
                for (int i = 5; i < maxim; i += 6)
                    if (n % i == 0 || n % (i + 2) == 0)
                        return false;
                return true;
            }
        }

        bool CalcVerificationThreads(int count, ref int threads)
        {
            int nt = -1, i = 1;

            while (i < MTUConstants.MaximumThreads)
            {
                if (count % i == 0)
                    nt = i;
                i++;
            }

            threads = nt;
            return nt != -1;
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

        Worker CreateWorker(string name, WorkerType type, object realWorker)
        {
            lock (clients)
            {
                var worker = new Worker(name, type, realWorker);
                clients.Add(name, worker);

                if (WorkerAppend != null)
                    WorkerAppend(this, new WorkerAppendEventArgs(name, type));
                return worker;
            }
        }

        Worker CreateClient(string name, WorkerType type)
        {
            var client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;

            return CreateWorker(name, type, client);
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
                verificationList.Clear();
                downloadQueue.Clear();

                State = UpdaterState.Started;
                Dispose();

                var worker = CreateClient("ListChecker", WorkerType.Verificate);
                (worker.RealWorker as WebClient).DownloadDataCompleted += Client_DownloadDataCompleted;
                (worker.RealWorker as WebClient).DownloadDataAsync(new Uri(XMLLink), worker);
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
            try
            {
                var cliWorker = e.UserState as Worker;
                UpdateWorkerProgress(cliWorker, 100);

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

                    using (var stream = new MemoryStream(e.Result))
                    using (var reader = new XmlTextReader(stream))
                    {
                        var doc = new XmlDocument();
                        doc.Load(reader);

                        foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("Update"))
                        {
                            var update = new Update();
                            update.Filename = node.Attributes["Filename"].Value;
                            update.Hash = node.Attributes["Hash"].Value;
                            update.Size = Convert.ToInt64(node.Attributes["Size"].Value);

                            verificationList.Add(update);
                        }

                        var count = 0;
                        var total = verificationList.Count;
                        var index = 0;

                        if (IsPrime(total))
                            total++;

                        var threads = new Thread[CalcVerificationThreads(total, ref count) ? count : count + 1];
                        var pack = total / threads.Length;
                        for(int i = 0; i < threads.Length; i++)
                        {
                            var packed = verificationList.Skip(index).Take(pack);
                            index += pack;

                            var thread = threads[i] = new Thread(new ParameterizedThreadStart(HandleVerification));
                            var worker = CreateWorker(string.Format("Verificator {0}", i + 1), WorkerType.Verificate, thread);
                            worker.State = packed;

                            threads[i].Start(worker);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InvokeError(ex);
                if (InvokeFailed())
                    Update();
            }
        }

        private void HandleVerification(object state)
        {
            var worker = state as Worker;
            var updates = worker.State as IEnumerable<Update>;

            var path = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(BasePath))
                path = Path.Combine(path, BasePath);
            var hpath = Path.Combine(path, "Hashes");

            var current = 0;
            var total = updates.Count();

            foreach(var update in updates)
            {
                var filename = Path.Combine(path, update.Filename);

                if (Path.GetExtension(filename) == ".hash")
                {
                    var from = verificationList.FirstOrDefault(u => u.Filename == update.Filename.Replace(".hash", string.Empty));
                    var fromF = Path.Combine(path, from.Filename);

                    if (!File.Exists(filename) || !File.Exists(fromF))
                        downloadQueue.Add(update);
                    else
                    {
                        using (var stream = File.OpenRead(filename))
                            if (update.Hash != HashFunction(stream))
                                downloadQueue.Add(update);
                    }
                }
                else
                {
                    var hashF = string.Concat(filename.Replace(path, hpath), ".hash");

                    if (!File.Exists(filename) || !File.Exists(hashF))
                        downloadQueue.Add(update);
                }

                current++;
                UpdateWorkerProgress(worker, (current * 100) / total);
            }
            UpdateWorkerProgress(worker, 100);
        }
    }
}
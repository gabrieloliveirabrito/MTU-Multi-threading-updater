using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using System.ComponentModel;
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
        Queue<Worker> downloadQueue;
        List<Update> verificationList, downloadCache;

        bool result = false;
        int current = 0, total = 0;
        readonly object syncLock = new object();

        public event EventHandler Started, CurrentChanged, TotalChanged;
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

        public string XmlPath { get; set; }
        public string BaseLink { get; set; }
        public string BasePath { get; set; }
        public Func<HashAlgorithm> Algorithm { get; set; }
        public Func<Stream, string> HashFunction { get; set; }

        public int CurrentUpdate
        {
            get
            {
                lock (syncLock)
                    return current;
            }
            private set
            {
                lock (syncLock)
                    current = value;

                if (CurrentChanged != null)
                    CurrentChanged(this, EventArgs.Empty);
            }
        }

        public int TotalUpdates
        {
            get
            {
                lock (syncLock)
                    return total;
            }
            set
            {
                lock (syncLock)
                    total = value;

                if (TotalChanged != null)
                    TotalChanged(this, EventArgs.Empty);
            }
        }

        public MTUpdater()
        {
            clients = new Dictionary<string, Worker>();
            downloadCache = new List<Update>();
            verificationList = new List<Update>();
            downloadQueue = new Queue<Worker>();

            Algorithm = new Func<HashAlgorithm>(() => MD5.Create());
            HashFunction = BaseHashing;
            XmlPath = "ver.xml";
        }

        public MTUpdater(string baseLink) : this()
        {
            BaseLink = baseLink;
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
            using (var algo = Algorithm())
            {
                var buf = algo.ComputeHash(s);
                return BitConverter.ToString(buf).Replace("-", string.Empty);
            }
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
                downloadCache.Clear();

                State = UpdaterState.Started;
                Dispose();

                var worker = CreateClient("ListChecker", WorkerType.Verificate);
                worker.Running = true;

                (worker.RealWorker as WebClient).DownloadDataCompleted += Client_DownloadDataCompleted;
                (worker.RealWorker as WebClient).DownloadDataAsync(new Uri(new Uri(BaseLink), XmlPath), worker);
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
                cliWorker.Running = false;
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
                    State = UpdaterState.ParsingList;

                    using (var stream = new MemoryStream(e.Result))
                    using (var reader = new XmlTextReader(stream))
                    {
                        var doc = new XmlDocument();
                        doc.Load(reader);

                        foreach (XmlNode node in doc.DocumentElement.GetElementsByTagName("Update"))
                        {
                            var update = new Update();
                            update.Filename = node.Attributes["Path"].Value;
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
                        }
                        State = UpdaterState.CheckingFiles;

                        var downloadQueuer = new Thread(DownloadQueuer);
                        var dWorker = CreateWorker("DownloadQueuer", WorkerType.Verificate, downloadQueuer);
                        dWorker.Running = true;
                        downloadQueuer.Start(dWorker);

                        for(int i = 0; i < threads.Length; i++)
                        {
                            var worker = clients.Values.Skip(1 + i).First();
                            worker.Running = true;

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

        void DownloadQueuer(object state)
        {
            var dWorker = state as Worker;
            while(State == UpdaterState.CheckingFiles || State == UpdaterState.Downloading)
            {
                int count = 0;
                lock (clients)
                    count = clients.Values.Count(w => w.Type == WorkerType.Update && w.Running && (w.RealWorker as WebClient).IsBusy);
                
                Worker worker = null;
                bool has = false;
                lock (downloadQueue)
                {
                    has = downloadQueue.Count > 0;

                    if (has && count <= MTUConstants.MaximumDownloads)
                        worker = downloadQueue.Dequeue();
                }

                if (count <= MTUConstants.MaximumDownloads && has)
                {
                    UpdateWorkerProgress(dWorker, (CurrentUpdate * 100) / TotalUpdates);

                    var client = worker.RealWorker as WebClient;
                    var update = worker.State as Update;

                    client.DownloadFileAsync(new Uri(new Uri(BaseLink), update.Filename), update.RealFilename, worker);
                }

                Thread.Sleep(MTUConstants.DownloadQueuerDelay);
            }

            UpdateWorkerProgress(dWorker, 100);
            dWorker.Running = false;
        }

        void EnqueueUpdate(Update update, string filename)
        {
            lock (clients)
            {
                if (!clients.ContainsKey(update.Filename))
                {
                    lock (downloadCache)
                    {
                        downloadCache.Add(update);

                        TotalUpdates++;
                        if (update.Filename.EndsWith("SD.WindowsForms.exe"))
                            Console.WriteLine("X");

                        var dp = Path.GetDirectoryName(update.RealFilename);
                        if (!Directory.Exists(dp))
                            Directory.CreateDirectory(dp);

                        var worker = CreateClient(update.Filename, WorkerType.Update);
                        worker.State = update;
                        worker.Running = true;

                        var client = worker.RealWorker as WebClient;
                        client.DownloadFileCompleted += Client_DownloadFileCompleted;

                        lock (downloadQueue)
                            downloadQueue.Enqueue(worker);
                    }
                }
            }
        }

        string GetBasePath()
        {
            var path = Environment.CurrentDirectory;
            if (!string.IsNullOrEmpty(BasePath))
                path = Path.Combine(path, BasePath);
            return path;
        }

        void TryToFinish()
        {
            lock (clients)
            {
                if (!clients.Where(k => k.Key.StartsWith("Verificator")).Any(w => w.Value.Running) && !clients.Values.Where(w => w.Type == WorkerType.Update).Any(w => w.Running))
                {
                    result = true;
                    State = UpdaterState.Finished;
                }
            }
        }

        private void HandleVerification(object state)
        {
            while (State != UpdaterState.CheckingFiles)
                Thread.Sleep(MTUConstants.ParsingWaitDelay);

            var worker = state as Worker;
            var updates = worker.State as IEnumerable<Update>;

            var path = GetBasePath();
            var hpath = Path.Combine(path, "Hashes");

            var current = 0;
            var total = updates.Count();

            foreach (var update in updates)
            {
                var filename = update.RealFilename = Path.Combine(path, update.Filename);

                if (Path.GetExtension(filename) == ".hash")
                {
                    var from = verificationList.FirstOrDefault(u => u.Filename == update.Filename.Substring(("Hashes" + Path.DirectorySeparatorChar).Length).Replace(".hash", string.Empty));

                    if (!downloadCache.Contains(from))
                    {
                        var fromF = Path.Combine(path, from.Filename);

                        from.RealFilename = fromF;
                        if (!File.Exists(filename) || !File.Exists(fromF))
                            EnqueueUpdate(from, filename);
                        else
                        {
                            using (var stream = File.OpenRead(filename))
                                if (update.Hash != HashFunction(stream))
                                    EnqueueUpdate(from, filename);
                        }
                    }
                }
                else if(!downloadCache.Contains(update))
                {
                    var hashF = string.Concat(filename.Replace(path, hpath), ".hash");

                    if (!File.Exists(filename) || !File.Exists(hashF))
                        EnqueueUpdate(update, filename);
                    else
                    {
                        var hash = File.ReadAllText(hashF);
                        if (hash != update.Hash)
                            EnqueueUpdate(update, filename);
                    }
                }
                Thread.Sleep(MTUConstants.VerificationDelay);

                current++;
                UpdateWorkerProgress(worker, (current * 100) / total);
            }

            worker.Running = false;

            lock (clients)
            {
                if (!clients.Values.Any(w => w.Type == WorkerType.Verificate && w.Running))
                {
                    if (!clients.Values.Any(w => w.Type == WorkerType.Update))
                    {
                        result = true;
                        State = UpdaterState.Finished;
                    }
                    else
                        State = UpdaterState.Downloading;
                }
            }
            UpdateWorkerProgress(worker, 100);

            TryToFinish();
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var worker = e.UserState as Worker;
            var update = worker.State as Update;
            var path = GetBasePath();
            var hpath = Path.Combine(path, "Hashes");
            worker.Running = false;

            UpdateWorkerProgress(worker, 100);

            if (e.Cancelled && InvokeFailed())
                EnqueueUpdate(update, update.RealFilename);
            else if (e.Error != null)
            {
                InvokeError(e.Error);
                if (InvokeFailed())
                    EnqueueUpdate(update, update.RealFilename);
            }
            else
            {
                CurrentUpdate++;

                var hp = string.Concat(Path.Combine(hpath, update.Filename), ".hash");
                var dp = Path.GetDirectoryName(hp);

                if (!Directory.Exists(dp))
                    Directory.CreateDirectory(dp);

                using (var fs = File.CreateText(hp))
                {
                    fs.Write(update.Hash);
                }

                TryToFinish();
            }
        }
    }
}
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.Concurrent;
class Program
{
    #region Fields & Properties

    static int _PingCount = 2;
    static int _PingInterval = 500;
    static Stopwatch _StopWatch;
    static List<string> _HostsNames = new List<string>()
        {
            "cnn.com",
            "sbs.com.au",
            "bbc.co.uk",
            "maariv.co.il",
            "brazilian.report"
        };
    static string _Menu = @"Choose async method invokation that you would like to compare to sync invokation:
                        t = Thread
                        tp = ThreadPool
                        ta = Task
                        pf = Parallel for
                        pfe = Parallel for each
                        pi = Parallel invoke
                        async = Async Await
                        OR ctrl+C to break...";

    #endregion
    public static void Main()
    {
        Console.WriteLine(_Menu);
        string userInput = Console.ReadLine().ToLower().Trim();
        Console.Clear();
        //
        PrintStars();
        PrintReport(GetHostsReplies);
        //        
        PrintStars();
        if (userInput == "t")
            PrintReport(GetHostsRepliesWithThreads);
        else if (userInput == "tp")
            PrintReport(GetHostsRepliesWithThreadPool);
        else if (userInput == "ta")
            PrintReport(GetHostsRepliesWithTasks);
        else if (userInput == "pf")
            PrintReport(GetHostsRepliesWithParallelFor);
        else if (userInput == "pfe")
            PrintReport(GetHostsRepliesWithParallelForEach);
        else if (userInput == "pi")
            PrintReport(GetHostsRepliesWithParallelInvoke);
        else if (userInput == "async")
            PrintReport(GetHostsRepliesWithAsync);
        else Console.WriteLine("invalid input...");
    }

    static Dictionary<string, List<PingReply>> GetHostsReplies()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        foreach (var hostName in _HostsNames)
        {
            Ping ping = new Ping();
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < _PingCount; i++)
            {
                pingReplies.Add(ping.Send(hostName));
                Thread.Sleep(_PingInterval);
            }
            hostsReplies.Add(hostName, pingReplies);
        }
        return hostsReplies;
    }
    static void AddPingRepliesToHostsReplies(Dictionary<string, List<PingReply>> hostsReplies, ConcurrentBag<PingReply> pingReplies)
    {
        foreach (var host in _HostsNames)
        {
            List<PingReply> pingRepliesForHost = new List<PingReply>();
            IPAddress[] AddressListForHost = Dns.GetHostAddresses(host);
            foreach (var replay in pingReplies)
            {
                if (AddressListForHost.Contains(replay.Address))
                    pingRepliesForHost.Add(replay);

            }
            hostsReplies.Add(host, pingRepliesForHost);
        }
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreads()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Thread> pingThreads = new List<Thread>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();
        foreach (var host in _HostsNames)
        {
            pingThreads.Add(new Thread(() =>
            {
                Ping Ping = new Ping();
                for (int i = 0; i < _PingCount; i++)
                {
                    PingReplies.Add(Ping.Send(host));
                    Thread.Sleep(_PingInterval);
                }
            }));
        }
        foreach (var pingThread in pingThreads)
            pingThread.Start();
        foreach (var pingThread in pingThreads)
            pingThread.Join();
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreadPool()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<EventWaitHandle> ThreadLocks = new List<EventWaitHandle>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();

        foreach (var host in _HostsNames)
        {
            EventWaitHandle LockForASingleThread = new EventWaitHandle(false, EventResetMode.ManualReset);
            ThreadLocks.Add(LockForASingleThread);
            ThreadPool.QueueUserWorkItem((X) =>
            {
                Ping Ping = new Ping();
                for (int i = 0; i < _PingCount; i++)
                {
                    PingReplies.Add(Ping.Send(host));
                    Thread.Sleep(_PingInterval);
                }
                LockForASingleThread.Set();
            });
        }
        foreach (var ewh in ThreadLocks)
            ewh.WaitOne();
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTasks()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Task> pingTasks = new List<Task>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();

        foreach (var host in _HostsNames)
            pingTasks.Add(Task.Run(() =>
            {
                Ping Ping = new Ping();
                for (int i = 0; i < _PingCount; i++)
                {
                    PingReplies.Add(Ping.Send(host));
                    Thread.Sleep(_PingInterval);
                }
            }));
        Task.WaitAll(pingTasks.ToArray());
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithAsync()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Task> tasks = new List<Task>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();

        foreach (string host in _HostsNames)
            tasks.Add(Task.Run(async () =>
            {
                for (int i = 0; i < _PingCount; i++)
                {
                    PingReplies.Add(new Ping().Send(host));
                    await Task.Delay(_PingInterval);
                }

            }));
        Task.WaitAll(tasks.ToArray());
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelInvoke()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        List<Action> actions = new List<Action>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();

        foreach (var host in _HostsNames)
            actions.Add(() =>
            {
                Ping Ping = new Ping();
                for (int i = 0; i < _PingCount; i++)
                {
                    PingReplies.Add(Ping.Send(host));
                    Thread.Sleep(_PingInterval);
                }
            });
        Parallel.Invoke(actions.ToArray());
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelForEach()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();
        Parallel.ForEach(_HostsNames, host =>
        {
            List<PingReply> pingReplies = new List<PingReply>();
            Ping Ping = new Ping();
            for (int i = 0; i < _PingCount; i++)
            {
                PingReplies.Add(Ping.Send(host));
                Thread.Sleep(_PingInterval);
            }
        });
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }

    static Dictionary<string, List<PingReply>> GetHostsRepliesWithParallelFor()
    {
        Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        ConcurrentBag<PingReply> PingReplies = new ConcurrentBag<PingReply>();
        Parallel.For(0, _HostsNames.Count, index =>
        {
            string host = _HostsNames[index];
            List<PingReply> pingReplies = new List<PingReply>();
            Ping Ping = new Ping();
            for (int i = 0; i < _PingCount; i++)
            {
                PingReplies.Add(Ping.Send(host));
                Thread.Sleep(_PingInterval);
            }
        });
        AddPingRepliesToHostsReplies(hostsReplies, PingReplies);
        return hostsReplies;
    }

    static Dictionary<string, PingReplyStatistics> GetHostsRepliesStatistics(Dictionary<string, List<PingReply>> hostsReplies)
    {
        Dictionary<string, PingReplyStatistics> hrs = new Dictionary<string, PingReplyStatistics>();
        foreach (var hr in hostsReplies)
            hrs.Add(hr.Key, new PingReplyStatistics(hr.Value));
        return hrs;
    }
    static void PrintLine() => Console.WriteLine("---------------------------");
    static void PrintStars() => Console.WriteLine("***************************");
    static void PrintReport(Func<Dictionary<string, List<PingReply>>> getHostsReplies)
    {
        Console.WriteLine($"Started {getHostsReplies.Method.Name}");
        _StopWatch = Stopwatch.StartNew();
        Dictionary<string, List<PingReply>> hostsReplies = getHostsReplies();
        _StopWatch.Stop();
        Console.WriteLine($"Finished {getHostsReplies.Method.Name}");
        PrintLine();
        Console.WriteLine($"Printing {getHostsReplies.Method.Name} report:");
        if (hostsReplies != null)
            PrintHostsRepliesReports(hostsReplies);
        PrintLine();
    }
    static void PrintHostsRepliesReports(Dictionary<string, List<PingReply>> hostsReplies)
    {
        long hostsTotalRoundtripTime = 0;
        Dictionary<string, PingReplyStatistics> hrs = GetHostsRepliesStatistics(hostsReplies);
        PrintTotalRoundtripTime(hrs);
        PrintLine();
        hostsTotalRoundtripTime = hrs.Sum(hr => hr.Value.TotalRoundtripTime);
        Console.WriteLine($"Report took {_StopWatch.ElapsedMilliseconds} ms to generate,{_PingCount * _HostsNames.Count} total pings took total {hostsTotalRoundtripTime} ms hosts roundtrip time");
    }
    static void PrintTotalRoundtripTime(Dictionary<string, PingReplyStatistics> hrs, bool ascendingOrder = true)
    {
        string orderDescription = ascendingOrder ? "ascending" : "descending";
        Console.WriteLine($"Hosts total roundtrip time in {orderDescription} order: (HostName:X,Replies statistics:Y)");
        var orderedHrs = ascendingOrder ? hrs.OrderBy(hr => hr.Value.TotalRoundtripTime) : hrs.OrderByDescending(hr => hr.Value.TotalRoundtripTime);
        foreach (var hr in orderedHrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }
    static void PrintHostsRepliesStatistics(Dictionary<string, PingReplyStatistics> hrs)
    {
        Console.WriteLine("Hosts replies statistics: (HostName:X,Replies statistics:Y)");
        foreach (var hr in hrs)
        {
            Console.WriteLine($"{hr.Key},{hr.Value}");
        }
    }


}
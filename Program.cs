using ICMP_Summery_SYNC;
using System.Diagnostics;
using System.Net.NetworkInformation;

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

    #endregion
    public static void Main()
    {

        PrintStars();
        PrintReport(GetHostsReplies);

        PrintStars();
        PrintReport(GetHostsRepliesWithThreads);

        PrintStars();
        PrintReport(GetHostsRepliesWithThreadPool);

        PrintStars();
        PrintReport(GetHostsRepliesWithTasks);

        PrintStars();
        PrintReport(GetHostsRepliesWithTPL);
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
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreads()
    {
        List<PingThread> pingThreads = new List<PingThread>();
        foreach (var hostName in _HostsNames)
        {
            pingThreads.Add(new PingThread(hostName, _PingCount, _PingInterval));
        }
        while (true)
        {
            int ended = 0;
            foreach (var pingThread in pingThreads)
                if (pingThread.PingSenderThread.IsAlive)
                    break;
                else ended++;
            if (ended == _HostsNames.Count)
                break;
        }

        return PingThread.hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithThreadPool()
    {
        new PingThreadPool(_HostsNames, _PingCount, _PingInterval);
        while (!OnlyMainThreadIsRunning()){
            Thread.Sleep(10);
        }
        return PingThreadPool.hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTasks()
    {
        foreach (var hostName in _HostsNames)
            new PingTask(hostName, _PingCount, _PingInterval);
        while (!OnlyMainThreadIsRunning()) {
            Thread.Sleep(10);
        }
        return PingTask.hostsReplies;
    }
    static Dictionary<string, List<PingReply>> GetHostsRepliesWithTPL()
    {
        new PingParallel(_HostsNames, _PingCount, _PingInterval);
        while (!PingParallel.IsTheProcessOver)
            Task.Delay(10);
        return PingParallel.hostsReplies;
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

    // This function will return true if theres only 1 (Main) thread in ThreadPool 
    static bool OnlyMainThreadIsRunning()
    {
        int workerThreads;
        int portThreads;
        int workerThreads2;
        int portThreads2;

        ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
        ThreadPool.GetAvailableThreads(out workerThreads2, out portThreads2);
        if (workerThreads - workerThreads2 > 1)
            return false;
        return true;

    }



}
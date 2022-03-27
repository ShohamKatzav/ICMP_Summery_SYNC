using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ICMP_Summery_SYNC
{

    internal class PingParallel
    {
        public static Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        public static bool IsTheProcessOver = false;

        public List<string> HostsNames;
        public int PingCount;
        public int PingInterval;
        public PingParallel(List<string> hostsNames, int pingCount, int pingInterval, string ParallelOption)
        {
            this.HostsNames = hostsNames;
            this.PingCount = pingCount;
            this.PingInterval = pingInterval;

            Task ParallelOptionTask;
            if (ParallelOption == "ForEach")
                ParallelOptionTask = ParallelForEach();
            else
            {
                if (ParallelOption == "For")
                    ParallelOptionTask = ParallelFor();
                else
                    ParallelOptionTask = ParallelInvoke();
            }
        }
        async Task ParallelForEach()
        {
            await Parallel.ForEachAsync(HostsNames, new ParallelOptions() { MaxDegreeOfParallelism = HostsNames.Count },
                 async (host, token) =>
                 {
                     List<PingReply> pingReplies = new List<PingReply>();
                     for (int i = 0; i < PingCount; i++)
                     {
                         pingReplies.Add(new Ping().Send(host));
                         await Task.Delay(PingInterval, token);
                     }
                     hostsReplies.Add(host, pingReplies);
                 });
            IsTheProcessOver = true;

        }
        Task ParallelFor()
        {
            Parallel.For(0, HostsNames.Count, index =>
            {
                string host = HostsNames[index];
                List<PingReply> pingReplies = new List<PingReply>();
                Parallel.For(0, PingCount, index =>
                { 
                    pingReplies.Add(new Ping().Send(host));
                    Thread.Sleep(PingInterval);
                });
                hostsReplies.Add(host, pingReplies);

            });
            IsTheProcessOver = true;
            return Task.CompletedTask;
        }

        Task ParallelInvoke()
        {
            Parallel.ForEach(HostsNames, host =>
            {
                List<PingReply> pingReplies = new List<PingReply>();
                Parallel.Invoke(() =>
                {
                    Parallel.For(0, PingCount, index =>
                    {
                        pingReplies.Add(new Ping().Send(host));
                        Thread.Sleep(PingInterval);
                    });
                });
                hostsReplies.Add(host, pingReplies);
            });
            IsTheProcessOver = true;
            return Task.CompletedTask;
        }

    }

}

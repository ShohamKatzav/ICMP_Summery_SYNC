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

        public static int index = 0;
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
                for (int i = 0; i < PingCount; i++)
                {
                    pingReplies.Add(new Ping().Send(host));
                    Thread.Sleep(PingInterval);
                }
                hostsReplies.Add(host, pingReplies);

            });
            IsTheProcessOver = true;
            return Task.CompletedTask;
        }

        Task ParallelInvoke()
        {
            Parallel.ForEach(HostsNames, new ParallelOptions() { MaxDegreeOfParallelism = HostsNames.Count },
                host => {
                    Parallel.Invoke(() => ProcForInvoke(host));
            });
            IsTheProcessOver = true;
            return Task.CompletedTask;
        }

        public void ProcForInvoke(string host)
        {
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < PingCount; i++)
            {
                Parallel.Invoke(() =>
                     {
                         pingReplies.Add(new Ping().Send(host));
                     }, () => { Thread.Sleep(PingInterval); });
            }
            hostsReplies.Add(host, pingReplies);
        }

    }

}

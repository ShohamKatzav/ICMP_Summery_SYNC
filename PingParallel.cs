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
        public EventWaitHandle ThreadLock = new EventWaitHandle(false, EventResetMode.ManualReset);
        public static int IndexForInvokeParallel = 0;

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
                    ParallelOptionTask = ParallelInvoke(HostsNames.Count);
            }
            ThreadLock.Set();
        }
        Task ParallelForEach()
        {
            Parallel.ForEach(HostsNames, new ParallelOptions() { MaxDegreeOfParallelism = HostsNames.Count }, host =>
                  {
                      List<PingReply> pingReplies = new List<PingReply>();
                      for (int i = 0; i < PingCount; i++)
                      {
                          pingReplies.Add(new Ping().Send(host));
                          Thread.Sleep(PingInterval);
                      }
                      hostsReplies.Add(host, pingReplies);
                  });
            return Task.CompletedTask;
        }
        Task ParallelFor()
        {
            Parallel.For(0, HostsNames.Count, new ParallelOptions() { MaxDegreeOfParallelism = HostsNames.Count }, index =>
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
            return Task.CompletedTask;
        }
        Task ParallelInvoke(int hostNumber)
        {
            bool flag = false;
            Parallel.Invoke(new ParallelOptions() { MaxDegreeOfParallelism = 5 },
                () =>
                {
                    if (IndexForInvokeParallel < HostsNames.Count)
                    {
                        ProcForInvoke(HostsNames[IndexForInvokeParallel++]);
                    }
                    else
                        flag = true;
                }, () =>
                {
                    if (!flag)
                        ParallelInvoke(IndexForInvokeParallel);
                });
            return Task.CompletedTask;
        }

        public void ProcForInvoke(string host)
        {
            List<PingReply> pingReplies = new List<PingReply>();
            for (int i = 0; i < PingCount; i++)
            {
                pingReplies.Add(new Ping().Send(host));
                Thread.Sleep(PingInterval);
            }
            hostsReplies.Add(host, pingReplies);
        }

    }

}

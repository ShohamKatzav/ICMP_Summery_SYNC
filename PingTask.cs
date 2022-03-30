using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ICMP_Summery_SYNC
{
    internal class PingTask
    {
        public static Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        public List<PingReply> PingReplies = new List<PingReply>();
        public Ping Ping = new Ping();
        public Task Task;

        public string HostName;
        public int PingCount;
        public int PingInterval;

        public PingTask(string hostName, int pingCount, int pingInterval)
        {
            this.HostName = hostName;
            this.PingCount = pingCount;
            this.PingInterval = pingInterval;

            Task = new Task(() =>
            {
                for (int i = 0; i < PingCount; i++)
                {
                    PingReplies.Add(Ping.Send(HostName));
                    Thread.Sleep(PingInterval);
                }
                hostsReplies.Add(HostName, PingReplies);
            });
            Task.Start();
            
        }
    }
}


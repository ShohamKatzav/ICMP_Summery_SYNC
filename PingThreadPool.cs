using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ICMP_Summery_SYNC
{
    internal class PingThreadPool
    {
        public static Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        public List<PingReply> pingReplies = new List<PingReply>();

        public List<string> HostName;
        public int PingCount;
        public int PingInterval;

        public PingThreadPool(List<string> hostName, int pingCount, int pingInterval)
        {
            this.HostName = hostName;
            this.PingCount = pingCount;
            this.PingInterval = pingInterval;
            foreach (string host in hostName)
                ThreadPool.QueueUserWorkItem(ThreadProc,host);
        }

        public void ThreadProc(object hostName)
        {
            for (int i = 0; i < PingCount; i++)
            {
                pingReplies.Add(new Ping().Send(hostName.ToString()));
                Thread.Sleep(PingInterval);
            }
            hostsReplies.Add(hostName.ToString(), pingReplies);
        }

    }
}

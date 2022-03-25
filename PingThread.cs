using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ICMP_Summery_SYNC
{
    internal class PingThread
    {
        public static Dictionary<string, List<PingReply>> hostsReplies = new Dictionary<string, List<PingReply>>();
        public List<PingReply> pingReplies = new List<PingReply>();
        public Ping Ping = new Ping();
        public Thread PingSenderThread;

        public string HostName;
        public int PingCount;
        public int PingInterval;
        public PingThread(string hostName, int pingCount, int pingInterval)
        {
            this.HostName = hostName;
            this.PingCount = pingCount;
            this.PingInterval = pingInterval;
            this.PingSenderThread = new Thread(new ThreadStart(ThreadProc));
            this.PingSenderThread.Start();
        }
        public void ThreadProc()
        {
            
            for (int i = 0; i < PingCount; i++)
            {
                pingReplies.Add(Ping.Send(HostName));
                Thread.Sleep(PingInterval);
            }
            hostsReplies.Add(HostName, pingReplies);
        }
    }
}

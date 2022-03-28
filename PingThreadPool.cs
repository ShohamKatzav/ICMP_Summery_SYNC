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
        public static Dictionary<string, List<PingReply>> HostsReplies = new Dictionary<string, List<PingReply>>();
        public static List<EventWaitHandle> ThreadLocks = new List<EventWaitHandle>();

        public List<string> HostName;
        public int PingCount;
        public int PingInterval;

        public PingThreadPool(List<string> hostName, int pingCount, int pingInterval)
        {
            this.HostName = hostName;
            this.PingCount = pingCount;
            this.PingInterval = pingInterval;
            foreach (string host in hostName)
            {
                EventWaitHandle LockForASingleThread = new EventWaitHandle(false, EventResetMode.ManualReset);
                ThreadLocks.Add(LockForASingleThread);
                ThreadPool.QueueUserWorkItem((X) => { ThreadProc(host); LockForASingleThread.Set(); });
            }


        }

        public void ThreadProc(object hostName)
        {
            List<PingReply> PingReplies = new List<PingReply>();
            for (int i = 0; i < PingCount; i++)
            {
                PingReplies.Add(new Ping().Send(hostName.ToString()));
                Thread.Sleep(PingInterval);
            }
            HostsReplies.Add(hostName.ToString(), PingReplies);
        }

    }
}

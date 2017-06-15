using System;
using System.Threading;


namespace NetworkNotifer
{
    class PeriodicCheck
    {
        Controller contobj;

        public int period { set; get; }

        public PeriodicCheck(Controller g, int period)
        {
            contobj = g;
            this.period = period;
        }

        private void NotifyIpUnreachable(string mac)
        {
            contobj.IpUnRechable(mac);
        }

        private void NotifyIpReachable(string mac)
        {
            contobj.IpRechable(mac);
        }

        public void PingIps()
        {
            while (true)
            {
                foreach (var device in contobj.devices)
                {
                    if (string.IsNullOrEmpty(device.Value.IP))
                        continue;

                    if (ARP.GetIpAddress_MAC(device.Value.IP) != null)
                    {
                        NotifyIpReachable(device.Key);
                    }
                    else
                    {
                        NotifyIpUnreachable(device.Key);
                    }
                }
                Thread.Sleep(TimeSpan.FromMinutes(period));
            }
        }

    }
}

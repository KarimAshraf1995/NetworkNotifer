using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using System.Net.NetworkInformation;

namespace NetworkNotifer
{
    class Controller
    {
        ConcurrentDictionary<string, IPState> ips;
        DHCPListener listener;
        PeriodicCheck Checker;
        Task ListeningTask;
        Task CheckingTask;
        UI GUI;

        public ConcurrentDictionary<string,IPState> devices
        {
            get
            {
                return ips;
            }
        }
        
        public Controller(UI g)
        {
            listener = new DHCPListener(this);
            Checker = new PeriodicCheck(this, 1);
            ips = new ConcurrentDictionary<string, IPState>();
            GUI = g;
            //new Task(IntialScan).Start();
            StartListening();
            StartChecking();
            //NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
            //NetworkChange.NetworkAvailabilityChanged += NetworkChange_NetworkAddressChanged;
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            GUI.Invoke(new Action(GUI.OnNetworkChanged));
            ips.Clear();
        }

        private void IntialScan()
        {
            SubnetScanner.ScanAll(this);
        }
        public void AddIntialdevice(IPMacPair ipmac)
        {
            GUI.Invoke(new Action<object>(GUI.NotifyAdded), ipmac);
            ips[ipmac.MAC] = new IPState()
            {
                IP = ipmac.IP,
                status = IPState.State.Connected
            };
        }
        private void StartListening()
        {
            ListeningTask = new Task(listener.Listen);
            ListeningTask.Start();
        }

        private void StartChecking()
        {
            CheckingTask = new Task(Checker.PingIps);
            CheckingTask.Start();
        }

        public void NewDhcpMessage(DHCPMessage d)
        {
            if (d.messagetype == DHCPMessage.DHCPMessageType.Discover)
            {
                GUI.Invoke(new Action<object>(GUI.NotifyConnecting), d);
                ips[d.MacAddr] = new IPState()
                {
                    IP = d.IpAddr,
                    status = IPState.State.Connecting
                };
            }
            else if (d.messagetype == DHCPMessage.DHCPMessageType.Request)
            {
                GUI.Invoke(new Action<object>(GUI.NotifyConnected), d);

                if (ips.ContainsKey(d.MacAddr) && ips[d.MacAddr].status == IPState.State.Connected && d.IpAddr.Equals(ips[d.MacAddr].IP))
                    return;
                
                ips[d.MacAddr] = new IPState()
                {
                    IP = d.IpAddr,
                    status = IPState.State.Connected
                };

            }

        }
        public void IpRechable(string mac)
        {
            if (ips[mac].status != IPState.State.Connected)
            {
                GUI.Invoke(new Action<object>(GUI.NotifyIPConnected), mac);
                var t = ips[mac];
                t.status = IPState.State.Connected;
                ips[mac] = t;
            }
        }
        public void IpUnRechable(string mac)
        {
            var t = ips[mac];
            t.CheckCount++;
            if (t.status != IPState.State.Disconnected)
            {
                GUI.Invoke(new Action<object>(GUI.NotifyIPDisonnected), mac);
                t.status = IPState.State.Disconnected;
                t.CheckCount = 0;
                ips[mac] = t;
            }
            if(t.CheckCount>2)
            {
                ips.TryRemove(mac,out t);
                GUI.Invoke(new Action<object>(GUI.RemoveDevice), mac);
            }
        }
    }

}

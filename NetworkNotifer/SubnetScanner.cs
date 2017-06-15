using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNotifer
{
    class SubnetScanner
    {
        private static List<Tuple<byte[], byte[]>> GetSubnets()
        {
            List<Tuple<byte[], byte[]>> startend = new List<Tuple<byte[], byte[]>>();

            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface Interface in Interfaces)
            {
                if (Interface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                if (Interface.OperationalStatus == OperationalStatus.Down) continue;


                UnicastIPAddressInformationCollection UnicastIPInfoCol = Interface.GetIPProperties().UnicastAddresses;
                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.IPv4Mask.ToString().Equals("0.0.0.0")) continue;

                    var ip_octs = UnicatIPInfo.Address.ToString().Split('.');
                    var subnet_octs = UnicatIPInfo.IPv4Mask.ToString().Split('.');

                    //1st host subnet & ip address + 1
                    //last host ^subnet | ip address - 1

                    byte[] start = new byte[4];
                    byte[] end = new byte[4];

                    for (int i = 0; i < 4; i++)
                    {
                        byte subnet_oct = Convert.ToByte(subnet_octs[i]);
                        byte ip_oct = Convert.ToByte(ip_octs[i]);
                        start[i] = unchecked((byte)(subnet_oct & ip_oct));
                        end[i]= unchecked((byte)((~subnet_oct) | ip_oct));
                        
                    }
                    start[3] += 1;
                    end[3] -= 1;
                    startend.Add(new Tuple<byte[], byte[]>(start, end));
                }
            }
            return startend;
        }
        public static void ScanAll(Controller contobj)
        {
            List<Tuple<byte[], byte[]>> startend = GetSubnets();
            
            foreach (var pair in startend)
            {
                
                byte[] curr_ip = pair.Item1;
                byte[] end_ip = pair.Item2;

                while (true)
                {
                    string ip = "";

                    for (int i = 0; i < 4; i++)
                    {
                        ip += curr_ip[i].ToString();
                        if (i != 3)
                            ip += '.';
                    }
                    
                    new Task(() =>
                    {
                        string mac = ARP.GetIpAddress_MAC(ip);

                        if (mac != null)
                        {
                            contobj.AddIntialdevice(new IPMacPair { IP = ip, MAC = mac });
                        }

                    }).Start();

                    Thread.Sleep(20);

                    curr_ip[3] += 1;
                    if (curr_ip[3] == 255)
                        curr_ip[2] += 1;
                    if (curr_ip[2] == 255)
                        curr_ip[1] += 1;
                    if (curr_ip[1] == 255)
                        curr_ip[0] += 1;
                    if (curr_ip[3] >= end_ip[3] && curr_ip[2] >= end_ip[2] && curr_ip[1] >= end_ip[1] && curr_ip[0] >= end_ip[0])
                        break;

                }
            }
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace NetworkNotifer
{
    class ARP
    {
        [DllImport("wsock32.dll", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int inet_addr(string s);

        [DllImport("iphlpapi.dll", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMACAddr, ref int PhyAddrLen);

        [DllImport("kernel32", EntryPoint = "RtlMoveMemory", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern void CopyMemory(ref byte dst, ref int src, int bcount);
        public static string GetIpAddress_MAC(string des_ip)
        {
            int inet = default(int);
            byte[] pMACAddr = new byte[8];
            short i = default(short);
            string result_mac = "";

            inet = Convert.ToInt32(inet_addr(des_ip));
            Int32 temp_PhyAddrLen = 6;

            if (SendARP(inet, 0, pMACAddr, ref temp_PhyAddrLen) == 0)
            {
                //CopyMemory(ref buffer[0], ref pMACAddr, 6);
                for (i = 0; i <= 5; i++)
                {
                    result_mac = result_mac + pMACAddr[i].ToString("X2");
                    if (i < 5)
                    {
                        result_mac += ":";
                    }
                }
                return result_mac;
            }
            return null;
        }

    }
}

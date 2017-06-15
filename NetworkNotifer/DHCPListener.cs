using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace NetworkNotifer
{
    class DHCPListener
    {
        UdpClient udpClient;
        Controller contobj;
        public DHCPListener(Controller g)
        {
            IPEndPoint AllInterfaces = new IPEndPoint(IPAddress.Any, 67);
            udpClient = new UdpClient();
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.ExclusiveAddressUse = false; // only if you want to send/receive on same machine.
            udpClient.Client.Bind(AllInterfaces);
            contobj = g;
        }

        public DHCPMessage ProcessMessage(byte[] bytes)
        {
            DHCPMessage m = new DHCPMessage();
            int i;

            m.TransactionID = "0x";
            for(i=4;i<8;i++)
            m.TransactionID += bytes[i].ToString("X2");

            /*
            m.IpAddr = array[12].ToString() + '.';
            m.IpAddr += array[13].ToString() + '.';
            m.IpAddr += array[14].ToString() + '.';
            m.IpAddr += array[15].ToString();
            */

            for(i=28;i<33;i++)
            m.MacAddr += bytes[i].ToString("X2") + ':';

            m.MacAddr += bytes[33].ToString("X2");


			//Note: DHCP magic cookie from byte #236 to #239 (4 bytes) and should be equal to 0x63 0x82 0x53 0x63
			
            int start = 240;
            int length;
            

            while(start<bytes.Length)
            {
                if (bytes[start] == 0x35)//DHCP Message Type
                {
                    m.messagetype = (DHCPMessage.DHCPMessageType)bytes[start + 2];
                }
                else if (bytes[start] == 0x32)//DCHP REQUEST IP
                {
                    length = bytes[start + 1];

                    for (i = 0; i < length - 1; i++)
                        m.IpAddr += bytes[start + i + 2].ToString() + ".";

                    m.IpAddr += bytes[start + i + 2].ToString();
                }

                else if (bytes[start] == 0x0c)//HOSTNAME
                {
                    length = bytes[start + 1];
                    for (i = 0; i < length; i++)
                        m.HostName += (char)bytes[start + i + 2];
                }
                else if (bytes[start] == 0xff)//end
                {
                    return m;
                }
                start = start + bytes[start + 1] + 2;
            }

            return m;
        }

        void NotifyController(object s)
        {
            byte[] bytes = (byte[])s;
            DHCPMessage m = ProcessMessage(bytes);
            contobj.NewDhcpMessage(m);
        }

        public void Listen()
        {
            try
            {
                IPEndPoint Sender=new IPEndPoint(IPAddress.Any,67);
                while (true)
                {
                    byte[] bytes = udpClient.Receive(ref Sender);
                    new Task(NotifyController, bytes).Start();
                }
            }
            finally
            {
                udpClient.Close();
            }

        }

    }
}

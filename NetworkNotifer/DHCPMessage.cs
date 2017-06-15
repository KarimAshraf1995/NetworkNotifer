namespace NetworkNotifer
{
    public struct DHCPMessage
    {
        public enum DHCPMessageType { Discover = 1, Offer = 2, Request = 3, Decline = 4, Ack = 5, Nak = 6, Release = 7, Inform = 8 };
        public DHCPMessageType messagetype;
        public string TransactionID;
        public string IpAddr;
        public string MacAddr;
        public string HostName;
    }
}

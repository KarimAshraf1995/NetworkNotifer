using System;

namespace NetworkNotifer
{
    class IPState
    {
        public enum State { Connecting, Connected, Disconnected };
        public String IP;
        public State status;
        public int CheckCount;
    }
}

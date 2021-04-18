using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartMusicServer.Proto
{
    class SysMsgHeartbeat
    {
        public class MsgPing : MsgBase
        {
            public MsgPing() : base("MsgPing")
            {
                //TODO 有可能出错
            }
        }

        public class MsgPong : MsgBase
        {
            public MsgPong() : base("MsgPong")
            {
            }
        }

    }
}

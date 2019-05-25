using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.EventArgs
{
    public class MeteorErrorEventArgs
    {
        internal MeteorErrorEventArgs(String reason, dynamic originalMessage)
        {
            this.OriginalMessage = originalMessage;
            this.Reason = reason;
        }

        public dynamic OriginalMessage { get; private set; }
        public String Reason { get; private set; }
    }
}

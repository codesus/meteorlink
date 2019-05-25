using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.EventArgs
{
    public class MeteorClientErrorEventArgs
    {
        public Exception Exception { get; private set; }
        internal MeteorClientErrorEventArgs(Exception x)
        {
            this.Exception = x;
        }
    }
}

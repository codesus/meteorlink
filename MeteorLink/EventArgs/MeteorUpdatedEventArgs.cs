using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.EventArgs
{
    public class MeteorUpdatedEventArgs
    {
        internal MeteorUpdatedEventArgs(string[] callIds)
        {
            this.CallIds = callIds;
        }

        public string[] CallIds { get; private set; }
    }
}

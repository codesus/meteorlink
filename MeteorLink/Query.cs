using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink
{
    public class Query
    {
        public string CallId { get; set; }

        public string Method { get; set; }

        public dynamic Arguments { get; set; }

        public MethodMessage Message { get; set; }

        public MethodResult Result { get; set; }

        public MethodError Error { get; set; }

        public Action<MethodError, MethodResult> Callback { get; set; }
    }
}

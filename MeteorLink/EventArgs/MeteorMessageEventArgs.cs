using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.EventArgs
{
    public class MeteorMessageEventArgs
    {
        public dynamic Data { get; private set; }
        public string Collection { get; private set; }
        public string Id { get; private set; }
        public string Method { get; private set; }

        internal MeteorMessageEventArgs(string id, string method, string collection, dynamic data)
        {
            this.Id = id;
            this.Collection = collection;
            this.Data = data;
            this.Method = method;
        }

        public override string ToString()
        {
            return string.Format("Collection: {0} Id: {1} Method: {2} Data: {3}", this.Collection, this.Id, this.Method, this.Data);
        }
    }
}

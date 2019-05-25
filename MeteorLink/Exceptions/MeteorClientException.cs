using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.Exceptions
{
    [Serializable]
    class MeteorClientException : ApplicationException
    {
        public MeteorClientException(String message) : base(message) { }
    }
}

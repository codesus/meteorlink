using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace MeteorLink.EventArgs
{
    public class MeteorSocketStateChangedEventArgs
    {
        private readonly WebSocketState current;
        private readonly WebSocketState previous;
        private string currentStateName;
        private string previousStateName;
        internal MeteorSocketStateChangedEventArgs(WebSocketState current, WebSocketState previous)
        {
            this.current = current;
            this.previous = previous;
            currentStateName = GetSocketStateName(this.current);
            previousStateName = GetSocketStateName(this.previous);
        }
        public static string GetSocketStateName(WebSocketState socketState)
        {
            string name = string.Empty;
            switch (socketState)
            {
                case WebSocketState.None:
                    name = "None";
                    break;
                case WebSocketState.Connecting:
                    name = "Connecting";
                    break;
                case WebSocketState.Open:
                    name = "Open";
                    break;
                case WebSocketState.CloseSent:
                    name = "CloseSent";
                    break;
                case WebSocketState.CloseReceived:
                    name = "CloseReceived";
                    break;
                case WebSocketState.Closed:
                    name = "Closed";
                    break;
                case WebSocketState.Aborted:
                    name = "Aborted";
                    break;
            }
            return name;
        }
        public override string ToString()
        {
            return string.Format("The status of the socket has changed: {0} => {1}", previousStateName, currentStateName);
        }
        public string GetSocketStatus()
        {
            return currentStateName;
        }
    }
}

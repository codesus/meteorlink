using System.Net.WebSockets;

namespace MeteorLink.EventArgs
{
    public class MeteorDisconnectedEventArgs
    {
        private readonly string reason;
        private readonly WebSocketState socketState;

        internal MeteorDisconnectedEventArgs(WebSocketState socketState)
        {
            //this.reason = reason;
            this.socketState = socketState;

            switch (socketState)
            {
                case WebSocketState.None:
                    reason = "None";
                    break;
                case WebSocketState.Connecting:
                    reason = "Connecting";
                    break;
                case WebSocketState.Open:
                    reason = "Open";
                    break;
                case WebSocketState.CloseSent:
                    reason = "A close message was sent to the remote endpoint.";
                    break;
                case WebSocketState.CloseReceived:
                    reason = "A close message was received from the remote endpoint.";
                    break;
                case WebSocketState.Closed:
                    reason = "Connection closed";
                    break;
                case WebSocketState.Aborted:
                    reason = "Aborted";
                    break;
            }
        }
        public WebSocketState GetSocketState()
        {
            return socketState;
        }
        public override string ToString()
        {
            return string.Format("Reason: {0}", reason);
        }
    }
}

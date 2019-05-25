using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MeteorLink.EventArgs;
using MeteorLink.Exceptions;
using Newtonsoft.Json;
namespace MeteorLink
{
    public class MeteorClient : IDisposable
    {
        public event EventHandler<MeteorMessageEventArgs> OnMessage;
        public event EventHandler<MeteorClientErrorEventArgs> OnClientError;
        public event EventHandler<MeteorUpdatedEventArgs> OnMethodUpdated;
        public event EventHandler<MeteorErrorEventArgs> OnMeteorError;
        public event EventHandler<MeteorDisconnectedEventArgs> OnDisconnected;
        public event EventHandler<MeteorSocketStateChangedEventArgs> OnSocketStateChanged;
        private ClientWebSocket socket;
        private readonly Uri uri;
        private List<Query> queries;
        private Nito.AsyncEx.AsyncLock asyncLock = new Nito.AsyncEx.AsyncLock();
        private string session;
        private Timer timerCheckConnection;
        private  WebSocketState currentSocketState;

        public MeteorClient(string url)
        {
            uri = new Uri(string.Format("ws://{0}/websocket", url));
            queries = new List<Query>();
        }
        public void Connect()
        {
            ConnectAsync().Wait();
        }
        public async Task ConnectAsync()
        {
            if (socket != null) socket.Dispose();
            socket = new ClientWebSocket();
            socket.Options.KeepAliveInterval = TimeSpan.FromMilliseconds(100);
            await socket.ConnectAsync(uri, CancellationToken.None)
                .ContinueWith(s =>
                {
                    return Send(new
                    {
                        msg = "connect",
                        version = "1",
                        support = new string[] { "1", "pre1" }
                    });
                })
                .ContinueWith(s =>
                {
                    timerCheckConnection = new Timer((e) =>
                    {
                        //Console.WriteLine("TIMER EJECUTADO");
                        //
                        if (currentSocketState != socket.State) {
                            //invocar evento
                            OnSocketStateChanged?.Invoke(this, new MeteorSocketStateChangedEventArgs(socket.State, currentSocketState));
                            currentSocketState = socket.State;
                        }
                        if (socket.State != WebSocketState.None && socket.State != WebSocketState.Open && socket.State != WebSocketState.Connecting)
                        {
                            OnDisconnected?.Invoke(this, new MeteorDisconnectedEventArgs(socket.State));
                            Console.WriteLine("TIMER OnDisconnected");
                        }
                    }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
                    return SubscriberLoop();
                });

        }


        public void Call(string callId, string method, dynamic args, Action<MethodError, MethodResult> callback)
        {
            CallAsync(callId, method, args, callback).Wait();
        }
        public async Task CallAsync(string callId, string method, dynamic args, Action<MethodError, MethodResult> callback)
        {
            queries.Add(new Query
            {
                CallId = callId,
                Method = method,
                Arguments = args,
                Callback = callback
            });
            await Send(new
            {
                msg = "method",
                method,
                @params = new[] { args },
                id = callId
            });
        }
        private async Task Send(dynamic message)
        {
            string serialize = JsonConvert.SerializeObject(message);
            Console.WriteLine(serialize);
            ArraySegment<byte> segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(serialize));

            using (await asyncLock.LockAsync())
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        private async Task SubscriberLoop()
        {
            int start = 0;
            int bufferSize = 1024 * 32;
            byte[] buffer = new byte[bufferSize];
            string msg = string.Empty;

            MemoryStream stream = new MemoryStream(bufferSize);
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
                    WebSocketReceiveResult result = await socket.ReceiveAsync(segment, CancellationToken.None);
                    if (!result.EndOfMessage)
                    {
                        stream.Write(buffer, start, result.Count);
                        start += result.Count;
                    } else
                    {
                        stream.Write(buffer, start, result.Count);
                        msg = Encoding.UTF8.GetString(stream.ToArray());
                        Console.WriteLine(msg);
                        dynamic message = JsonConvert.DeserializeObject<dynamic>(msg);

                        if (session == null) session = message.session; //OJO VER QUE LLEGA EN ESTA VARIABLE
                        else if (message == null || message.msg == null)
                        {
                            if (message.server_id == null)
                                throw new MeteorClientException(string.Format("Unknown message: {0}", message));
                        } else
                        {
                            string msgType = message.msg.ToString();

                            switch (msgType)
                            {
                                case "ping":
                                    await Send(new { msg = "pong" });
                                    break;
                                case "error":
                                    OnMeteorError?.Invoke(this, new MeteorErrorEventArgs(message.reason.ToString(), message.offendingMessage));
                                    break;
                                case "updated":
                                    if (OnMethodUpdated != null)
                                    {
                                        List<string> methods = new List<string>();
                                        foreach (var m in message.methods)
                                            methods.Add(m.ToString());
                                        OnMethodUpdated(this, new MeteorUpdatedEventArgs(methods.ToArray()));

                                    }
                                    break;
                                case "ready":
                                    break;
                                case "result":
                                    string callId = message.id.ToString();
                                    foreach (Query data in queries)
                                    {
                                        if (callId != data.CallId) continue;
                                        if (message.result != null)
                                        {
                                            data.Result = new MethodResult()
                                            {
                                                Response = CleanFormat(message.result.ToString())
                                            };
                                        }

                                        if (message.error != null)
                                        {
                                            data.Error = new MethodError
                                            {
                                                Code = (int)message.error.error,
                                                Reason = message.error.reason.ToString(),
                                                Message = message.error.message.ToString(),
                                                Type = message.error.errorType.ToString()
                                            };
                                            Console.WriteLine("ENCONTRE ERROR");
                                        }
                                        data.Callback(data.Error, data.Result);
                                        if (callId == data.CallId) break;
                                    }
                                    break;
                                default:
                                    if (message.id != null && message.msg != null && message.collection != null && message.fields != null)
                                    {
                                        OnMessage?.Invoke(this, new MeteorMessageEventArgs(message.id.ToString(), CleanFormat(message.msg.ToString()), CleanFormat(message.collection.ToString()), CleanFormat(message.fields)));
                                    }
                                    else
                                        throw new MeteorClientException("Unrecognized message: " + message.ToString());
                                    break;

                            }
                        }
                        stream.Dispose();
                        stream = new MemoryStream();
                    }

                }
                //OnDisconnected?.Invoke(this, new MeteorDisconnectedEventArgs(socket.State));
            }
            catch (Exception x)
            {
                OnClientError?.Invoke(this, new MeteorClientErrorEventArgs(x));
            }
            finally
            {
                session = null;
                stream.Dispose();
            }
        }
        public void Dispose()
        {
            queries.Clear();
            timerCheckConnection.Dispose();
            if (socket != null) socket.Dispose();
        }
        public bool Connected { get { return socket.State == WebSocketState.Open; } }
        private string CleanFormat(string data)
        {
            return data.Replace("$","");
        }
        public WebSocketState GetSocketState()
        {
            return currentSocketState;
        }
    }
}

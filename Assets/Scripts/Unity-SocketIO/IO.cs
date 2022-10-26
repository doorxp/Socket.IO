using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;

namespace IO
{
    public class Socket
    {
        public int Id { get; private set; } = -1;

        public string ConnectionId =>
#if UNITY_WEBGL
               Socket_Get_Conn_Id(Id);
#else
               client.Id;
#endif


        public bool connected =>
#if UNITY_WEBGL
                Socket_IsConnected(Id);
#else
                client.Connected;
#endif

        public bool Disconnected => !connected;

        public bool Disabled { get; private set; } = false;

        private event Action<JToken> Action_AnyEvents;
        private Dictionary<string, List<Action<JToken>>> ActionEvents = new();

#if UNITY_WEBGL
        protected internal Socket(int id) {
            this.Id = id;
        }
#else
        SynchronizationContext MainContext = null;

        private readonly SocketIO client;
        protected internal Socket(int id, SocketIO client)
        {
            MainContext = SynchronizationContext.Current;
            this.Id = id;
            this.client = client;
            client.OnAny((string eventname, SocketIOResponse res) =>
            {
                JToken ret = null;
                try
                {
                    ret = res.GetValue<JToken>();
                }
                catch (Exception e)
                {
                    Debug.Log($"{e}");
                }
                finally
                {
                    MainContext.Post(ctx => InvokeEvent(eventname, ret), null);
                }
            });

            client.OnConnected += (sender, args) =>
            {
                MainContext.Post(ctx => InvokeEvent("connect", null), null);
            };

            client.OnDisconnected += (sender, args) =>
            {
                MainContext.Post(ctx => InvokeEvent("disconnect", null), null);
            };

            client.OnError += (sender, args) =>
            {
                MainContext.Post(ctx => InvokeEvent("error", null), null);
                
            };

            client.OnReconnectAttempt += (sender, args) =>
            {
                MainContext.Post(ctx => InvokeEvent("reconnect_attempt", args.ToString()), null);
            };


            client.OnReconnected += (sender, arg) =>
            {
                MainContext.Post(ctx => InvokeEvent("reconnect", null), null);
 
            };

            client.OnReconnectError += (sender, arg) =>
            {
                MainContext.Post(ctx => InvokeEvent("reconnect_error", null), null);
            };

            client.OnReconnectFailed += (sender, arg) =>
            {
                MainContext.Post(ctx => InvokeEvent("reconnect_failed", null), null);
            };
            
            client.OnPing += (sender, arg) =>
            {
                InvokeEvent("ping", null);
            };

            client.OnPong += (sender, arg) =>
            {
                InvokeEvent("pong", null);
            };

        }
#endif

        public Socket Connect()
        {
#if UNITY_WEBGL
            Socket_Connect(Id);
#else
            client.ConnectAsync().ContinueWith((ret) => { });
#endif

            return this;
        }

        public Socket Open()
        {
            return Connect();
        }

        public Socket Disconnect()
        {
#if UNITY_WEBGL
            Socket_Disconnect(Id);
#else
            client.DisconnectAsync().ContinueWith(ret => { });
#endif
            return this;
        }

        public Socket Close()
        {
            return Disconnect();
        }

        public Socket Emit(string ev, object data)
        {
#if UNITY_WEBGL
            if (data == null)
            {
                Socket_Emit(Id, ev, null);
            }
            else
            {
                var str = JToken.FromObject(data).ToString(Newtonsoft.Json.Formatting.None);
                Socket_Emit(Id, ev, str);
            }
#else
            client.EmitAsync(ev, data).ContinueWith((ret) => { });
#endif
            return this;
        }

        public Socket On(string ev, Action<JToken> callback)
        {
            if (!ActionEvents.ContainsKey(ev))
            {
                ActionEvents.Add(ev, new List<Action<JToken>>());
            }
            ActionEvents[ev].Add(callback);

            return this;
        }

        public Socket Off(string ev, Action<JToken> callback = null)
        {
            if (callback != null)
            {
                if (ActionEvents.TryGetValue(ev, out List<Action<JToken>> value))
                {
                    value.Remove(callback);
                }
            }
            else
            {
                ActionEvents = new Dictionary<string, List<Action<JToken>>>();
            }
            return this;
        }

        public Socket OnAny(Action<JToken> callback)
        {
            Action_AnyEvents += callback;
            return this;
        }

        public Socket OffAny(Action<JToken> callback = null)
        {
            if (callback == null)
            {
                Action_AnyEvents = null;
            }
            else
            {
                Action_AnyEvents -= callback;
            }
            return this;
        }

        public void DisableSocket()
        {
            Disconnect();
            Disabled = true;
        }

        public void InvokeEvent(string ev, JToken data)
        {
            Action_AnyEvents?.Invoke(data);

            //invoke event specific events
            if (ActionEvents.TryGetValue(ev, out List<Action<JToken>> value))
            {
                foreach (Action<JToken> act in value)
                {
                    act.Invoke(data);
                }
            }
        }

#if UNITY_WEBGL
            //external methods
            [DllImport("__Internal")]
            private static extern bool Socket_IsConnected(int id);

            [DllImport("__Internal")]
            private static extern string Socket_Get_Conn_Id(int id);

            [DllImport("__Internal")]
            private static extern void Socket_Connect(int id);
            
            [DllImport("__Internal")]
            private static extern void Socket_Disconnect(int id);
            
            // [DllImport("__Internal")]
            // private static extern void Socket_Send(int id, string data);

            [DllImport("__Internal")]
            private static extern void Socket_Emit(int id, string ev, string data);
#endif

    }

    public class IO
    {

#if UNITY_WEBGL
        private static readonly string SOCKET_GAMEOBJECT_NAME = "SocketIo_Ref";
#endif

        private static byte _protocol = 0;
        public static byte Protocol
        {
            get
            {
                if (_protocol == 0)
                {
#if UNITY_WEBGL
                    _protocol = GetProtocol();
#else
                    _protocol = 5;
#endif
                }
                return _protocol;
            }
        }

        private static Dictionary<int, Socket> EnabledSockets = new();

        public static Socket Connection(string Url) => Connection(Url, "");

        public static Socket Connection(string Url, string options)
        {

#if UNITY_WEBGL
            //check for gameobject
            if (GameObject.Find(SOCKET_GAMEOBJECT_NAME) == null)
            {
                Debug.Log("Generating SocketIO Object");

                GameObject SocGObj = new GameObject(SOCKET_GAMEOBJECT_NAME);
                SocGObj.AddComponent<SocketIoInterface>();

                GameObject.DontDestroyOnLoad(SocGObj);

                SetupGameObjectName(SOCKET_GAMEOBJECT_NAME);
            }

            int newSocketId = EstablishSocket(Url, options);

            Socket soc = new Socket(newSocketId);
            EnabledSockets.Add(newSocketId, soc);

            return soc;
#else
            int id = -1;
            do
            {
                id = UnityEngine.Random.Range(1, 10000);
            } while (EnabledSockets.ContainsKey(id));

            var client = new SocketIO(Url, new SocketIOOptions
            {
                Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                },
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            })
            {
                JsonSerializer = new NewtonsoftJsonSerializer()
            };

            var soc = new Socket(id, client);

            EnabledSockets.Add(id, soc);

            return soc;
#endif
        }

        public static void RemoveSocket(int id)
        {
            if (EnabledSockets.TryGetValue(id, out Socket value))
            {
                value.DisableSocket();
                EnabledSockets.Remove(id);
            }
            else
            {
                Debug.LogWarning("Tried to remove a socket but it does not exist, Id: " + id);
            }
        }

        public static bool TryGetSocketById(int id, out Socket soc) => EnabledSockets.TryGetValue(id, out soc);

        public static Socket FindSocketWithConnId(string id) => EnabledSockets.Values.First(o => o.ConnectionId == id);

        //external methods

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern byte GetProtocol();

        [DllImport("__Internal")]
        private static extern int EstablishSocket(string url, string options);

        [DllImport("__Internal")]
        private static extern string SetupGameObjectName(string name);

        //gameobject for webgl
        public class SocketIoInterface : MonoBehaviour
        {
            public void callSocketEvent(string data)
            {
                try
                {
                    var token = JToken.Parse(data);
                    var SocketId = (int)token["SocketId"];
                    var EventName = (string)token["EventName"];
                    var JsonData = token["JsonData"];

                    if (EnabledSockets.TryGetValue(SocketId, out Socket soc))
                    {
                        soc.InvokeEvent(EventName, JsonData);
                    }
                    else
                    {
                        throw new NullReferenceException("socket does not exist");
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }
#endif

    }
}
var SocketPlugin = {

    $Data: {
        SocketGameObjectName: "",
        sockets: new Map(),
        CallUnityEvent: function(id, event, data) {
            var JsonData = null
            if(data != null) {
                JsonData = data
            }
            unityInstance.SendMessage(Data.SocketGameObjectName, 'callSocketEvent', JSON.stringify({
                EventName: event,
                SocketId: id,
                JsonData: JsonData
            }));
        },
    },

    SetupGameObjectName: function(str) {
        Data.SocketGameObjectName = UTF8ToString(str);
        Data.sockets = new Map();
    },

    GetProtocol: function() {
        if(io != undefined)
            return io.getProtocol;
        else {
            console.error("SocketIO io object not found! Did you forget to include Reference in header?");
            throw new Error("SocketIO object not found! Did you forget to include Reference in header?");
        }
    },

    EstablishSocket: function(url_raw, options_raw) {
        if(io != undefined) {
            const url = UTF8ToString(url_raw);
            const options = UTF8ToString(options_raw); //string of user options selected

            var soc;
            if(options.length > 0) 
                soc = io(url, JSON.parse(options));
            else 
                soc = io(url);
            
            var id = 0;
            do {
                //generate an id between 1 and 10000
                id = Math.floor(Math.random() * 10000) + 1;
            } while(Data.sockets.has(id));

            Data.sockets.set(id, soc);

            var cur = this;

            soc.onAny(function(event, args) {
                Data.CallUnityEvent(id, event, args);
            });

            soc.on("connect",function() {
                Data.CallUnityEvent(id, "connect", null);
            });

            soc.on("disconnect",function() {
                Data.CallUnityEvent(id, "connect", null);
            });

            soc.on("connect_error",function() {
                Data.CallUnityEvent(id, "connect_error", null);
            });

            
            soc.on("error",function() {
                Data.CallUnityEvent(id, "error", null);
            });

            soc.on("reconnect",function() {
                Data.CallUnityEvent(id, "reconnect", null);
            });

            soc.on("reconnect_attempt",function() {
                Data.CallUnityEvent(id, "reconnect_attempt", null);
            });

            soc.on("reconnect_error",function() {
                Data.CallUnityEvent(id, "reconnect_error", null);
            });

            soc.on("reconnect_failed",function() {
                Data.CallUnityEvent(id, "reconnect_failed", null);
            });

            soc.on("ping",function() {
                Data.CallUnityEvent(id, "ping", null);
            });

            soc.on("pong",function() {
                Data.CallUnityEvent(id, "pong", null);
            });

            return id;
        } else {
            console.error("SocketIO io object not found! Did you forget to include Reference in header?");
            throw new Error("SocketIO object not found! Did you forget to include Reference in header?");
        }
    },

    //Socket Object stuff

    Socket_IsConnected: function(id) {
        return Data.sockets.get(id).connected;
    },

    Socket_Connect: function(id) {
        Data.sockets.get(id).connect();
    },

    Socket_Disconnect: function(id) {
        Data.sockets.get(id).disconnect();
    },
    Socket_Emit: function(id, event_raw, data_raw) {
        var exec = UTF8ToString(event_raw);
        var s = Data.sockets.get(id);
        var data = null;
        if(UTF8ToString(data_raw).length > 0) {
            data = JSON.parse(UTF8ToString(data_raw));
        }
        s.emit(exec, data);
    },

    Socket_Get_Conn_Id: function(id) {
        var result = Data.sockets.get(id).id;
        if(result != undefined) {
            var buffersize = lengthBytesUTF8(result) + 1;
            var buffer = _malloc(buffersize);
            stringToUTF8(result, buffer, bufferSize);
            return buffer;
        } else {
            return null;
        }
    },
};
autoAddDeps(SocketPlugin, "$Data");
mergeInto(LibraryManager.library, SocketPlugin);
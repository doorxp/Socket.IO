# Socket.IO for Unity

## Description

A Wrapper for [socket.io-client-csharp](https://github.com/doghappy/socket.io-client-csharp) to work with Unity,
Supports socket.io server v2/v3/v4, and has implemented http polling and websocket.

## Give a Star! ⭐

Feel free to request an issue on github if you find bugs or request a new feature.
If you find this useful, please give it a star to show your support for this project.

## Supported Platforms

PC/Mac, iOS, Android, WebGL

Compile by il2cpp or Mono

Other platforms(including the Editor) have not been tested or/and may not work!

## Installation

Copy this url:
```git@github.com:doorxp/Socket.IO.git```
then in unity open Window -> Package Manager -> and click (+) add package from git URL... and past it there.


### Initiation

You may want to put the script on the Camera Object or using ```DontDestroyOnLoad``` to keep the socket alive between scenes!

```csharp
var io = IO.IO.Connection("ws://192.168.1.2:443");

io.On("connect", ret =>
{
    Debug.Log("connected");
});

io.On("disconnect", ret =>
{
    Debug.Log("disconnect");
});

io.On("error", ret =>
{
    Debug.Log("error");
});

io.On("reconnect", ret =>
{
    Debug.Log("reconnect");
});

io.Open();

io.Emit("test", "test data");
```

### JsonSerializer

use Newtonsoft

### Emiting

```csharp
io.Emit("eventName");
io.Emit("eventName", "Hello World");
io.Emit("eventName", someObject);
```

### Receiving

```csharp
io.On("eventName", (response) =>
{

    /* in Unity UI Thread, Do Something with data! @response as JToken */
    var obj = response.GetValue<SomeClass>();
    ...
});
```

### Connecting/Disconecting

```csharp
io.Open();
io.Connect();

io.Disconnect();
```

## Server Example

```javascript
const port = 11100;
const io = require('socket.io')();
io.use((socket, next) => {
    if (socket.handshake.query.token === "UNITY") {
        next();
    } else {
        next(new Error("Authentication error"));
    }
});

io.on('connection', socket => {
  socket.emit('connection', {date: new Date().getTime(), data: "Hello Unity"})

  socket.on('hello', (data) => {
    socket.emit('hello', {date: new Date().getTime(), data: data});
  });

  socket.on('spin', (data) => {
    socket.emit('spin', {date: new Date().getTime()});
  });

  socket.on('class', (data) => {
    socket.emit('class', {date: new Date().getTime(), data: data});
  });
});

io.listen(port);
console.log('listening on *:' + port);
```

## Acknowledgement

[socket.io-client-csharp](https://github.com/doghappy/socket.io-client-csharp)

[Socket.IO](https://github.com/socketio/socket.io)

[Newtonsoft Json.NET](https://www.newtonsoft.com/json/help/html/Introduction.htm)

[Unity Documentation](https://docs.unity.com)

## Author

doorxp, doorxp@msn.com

## License

Socket.IO for Unity is available under the MIT license. See the LICENSE file for more info.

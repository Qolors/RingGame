using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using Google.Protobuf;
using Rtech.Liveapi;

namespace RingGame.Services;

public class ServerConnect
{
    public ServerConnect(MainPageViewModel mpm)
    {
        var server = new WebSocketServer();
        Task.Run(() => server.Start());
    }
}

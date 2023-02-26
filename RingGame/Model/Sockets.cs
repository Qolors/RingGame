using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;
using Rtech.Liveapi;

namespace RingGame.Services;
public class WebSocketServer
{
    private const int Port = 7777;

    public async Task Start()
    {
        var listener = new TcpListener(IPAddress.Any, Port);
        listener.Start();
        Debug.WriteLine($"WebSocket server listening on port {Port}");

        while (true)
        {
            TcpClient tcpClient = await listener.AcceptTcpClientAsync();
            Debug.WriteLine($"WebSocket client connected from {tcpClient.Client.RemoteEndPoint}");

            var webSocketContext = await WebSocketStream(tcpClient.GetStream());
            var webSocket = webSocketContext.WebSocket;

            // Perform WebSocket handshake
            await webSocket.SendAsync(new ArraySegment<byte>(new byte[0]), WebSocketMessageType.Text, true, default);

            Debug.WriteLine("Getting to this?");

            // Receive and deserialize protobuf objects from the WebSocket
            while (webSocket.State == WebSocketState.Open)
            {
                var receiveBuffer = new ArraySegment<byte>(new byte[1024]);
                var receiveResult = await webSocket.ReceiveAsync(receiveBuffer, default);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", default);
                    Debug.WriteLine($"WebSocket client disconnected from {tcpClient.Client.RemoteEndPoint}");
                    break;
                }

                using (var stream = new MemoryStream(receiveBuffer.Array))
                {
                    Any live = null;

                    do
                    {

                        CodedInputStream input = new(receiveBuffer.Array);
                        var x = input.ReadString();

                        Debug.WriteLine(x);
                        


                    } while (live == null);
                    Debug.WriteLine("wow"); 
                }
            }
        }
    }

    private static async Task<WebSocketContext> WebSocketStream(Stream stream)
    {
        var buffer = new byte[1024];
        var stringBuilder = new System.Text.StringBuilder();
        WebSocket webSocket = null;
        var httpHeaderDelimiter = new byte[] { 13, 10, 13, 10 };

        while (stream.CanRead)
        {
            await stream.ReadAsync(buffer, 0, buffer.Length);

            string s = Encoding.UTF8.GetString(buffer);

            if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
            {
                Debug.WriteLine("=====Handshaking from client=====\n{0}", s);

                // 1. Obtain the value of the "Sec-WebSocket-Key" request header without any leading or trailing whitespace
                // 2. Concatenate it with "258EAFA5-E914-47DA-95CA-C5AB0DC85B11" (a special GUID specified by RFC 6455)
                // 3. Compute SHA-1 and Base64 hash of the new value
                // 4. Write the hash back as the value of "Sec-WebSocket-Accept" response header in an HTTP response
                string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                // HTTP/1.1 defines the sequence CR LF as the end-of-line marker
                byte[] response = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 101 Switching Protocols\r\n" +
                    "Connection: Upgrade\r\n" +
                    "Upgrade: websocket\r\n" +
                    "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                stream.Write(response, 0, response.Length);
                webSocket = WebSocket.CreateFromStream(stream, true, "websocket", TimeSpan.FromSeconds(120));
                break;
            }
        }
        
        return new WebSocketContext(webSocket);
    }

    private class WebSocketContext
    {
        public WebSocketContext(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public WebSocket WebSocket { get; }
    }
}

// Factory class to create a WebSocket from a stream
public class WebSocketFactory
{
    public static async Task<WebSocket> CreateWebSocket(Stream stream)
    {
        var ws = WebSocket.CreateFromStream(stream, true, null, TimeSpan.FromSeconds(120));
        return await Task.FromResult(ws);
    }
}

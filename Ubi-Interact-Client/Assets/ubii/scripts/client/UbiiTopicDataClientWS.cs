using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using Google.Protobuf;
using Ubii.TopicData;
using System.Net.WebSockets;

public class UbiiTopicDataClientWS : ITopicDataClient
{
    private static int RECEIVE_BUFFER_SIZE = 8192;

    private string clientId;
    private string address;

#if WINDOWS_UWP
    //TODO: alternative would be StreamWebSocket, see if necessary for larger messages (images, etc.)
    private Windows.Networking.Sockets.MessageWebSocket clientWebsocket = null;
#else
    private System.Net.WebSockets.ClientWebSocket clientWebsocket = null;
#endif

    private bool connected = false;
    private Task taskProcessIncomingMsgs = null;
    //TODO: merge cancel tokens?
    private CancellationTokenSource ctsReadSocket, ctsConnect;
    private MemoryStream msReadBuffer = new MemoryStream();
    private UbiiNetworkClient.CbHandleTopicData CbHandleMessage = null;
    private UbiiNetworkClient.CbTopicDataConnectionLost CbTopicDataConnectionLost = null;

    public UbiiTopicDataClientWS(
        string clientId,
        string address = "https://localhost:8104",
        UbiiNetworkClient.CbHandleTopicData CbHandleMessage = null,
        UbiiNetworkClient.CbTopicDataConnectionLost CbTopicDataConnectionLost = null)
    {
        this.clientId = clientId;
        this.address = address;
        this.CbHandleMessage = CbHandleMessage;
        this.CbTopicDataConnectionLost = CbTopicDataConnectionLost;

        Initialize();
    }

    private async Task<bool> Initialize()
    {
        Uri uri = new Uri(this.address + "?clientID=" + this.clientId);

        try
        {
#if WINDOWS_UWP
            clientWebsocket = new Windows.Networking.Sockets.MessageWebSocket();
            clientWebsocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Binary;
            clientWebsocket.MessageReceived += OnMessageReceivedUWP;
            clientWebsocket.Closed += OnWebsocketCloseUWP;
            await clientWebsocket.ConnectAsync(uri);
#else
            clientWebsocket = new System.Net.WebSockets.ClientWebSocket();
            ctsConnect = new CancellationTokenSource();
            await clientWebsocket.ConnectAsync(uri, ctsConnect.Token);

            ctsReadSocket = new CancellationTokenSource();
            taskProcessIncomingMsgs = Task.Run(ReadSocket, ctsReadSocket.Token);
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError("UBII UbiiTopicDataClientWS.Initialize(): " + ex.ToString());
            return false;
        }

        connected = true;
        return connected;
    }

    public async Task<bool> TearDown()
    {
        connected = false;
        if (clientWebsocket != null)
        {
#if WINDOWS_UWP
            clientWebsocket.Close(1000, "Client Node stopped");  // constants defined somewhere?
            clientWebsocket.Dispose();
#else
            ctsReadSocket.Cancel();
            await taskProcessIncomingMsgs;

            if (clientWebsocket.State == WebSocketState.Open)
            {
                CancellationTokenSource ctsCloseSocket = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                await clientWebsocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "disconnecting unity websocket client", ctsCloseSocket.Token);
            }

            clientWebsocket.Dispose();
#endif
        }

        return true;
    }

#if WINDOWS_UWP

    private async Task<CancellationToken> SendBytes(byte[] bytes)
    {
        using (var dataWriter = new Windows.Storage.Streams.DataWriter(this.clientWebsocket.OutputStream))
        {
            dataWriter.WriteBytes(bytes);
            await dataWriter.StoreAsync();
            dataWriter.DetachStream();
        }

        return new CancellationToken();
    }

    private async void OnMessageReceivedUWP(Windows.Networking.Sockets.MessageWebSocket sender, Windows.Networking.Sockets.MessageWebSocketMessageReceivedEventArgs args)
    {
        try
        {
            using (Windows.Storage.Streams.DataReader dataReader = args.GetDataReader())
            {
                dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                uint messageLength = dataReader.UnconsumedBufferLength;
                // PING message
                if (messageLength == 4)
                {
                    string msgString = dataReader.ReadString(messageLength);
                    if (msgString == "PING")
                    {
                        await this.SendBytes(Encoding.UTF8.GetBytes("PONG"));
                    }
                }
                // topic data
                else
                {
                    TopicData topicdata = null;
                    try
                    {
                        byte[] receiveBuffer = new byte[messageLength];
                        dataReader.ReadBytes(receiveBuffer);
                        topicdata = TopicData.Parser.ParseFrom(receiveBuffer, 0, (int)messageLength);

                        if (topicdata.TopicDataRecord != null)
                        {
                            this.InvokeTopicCallbacks(topicdata.TopicDataRecord);
                        }

                        if (topicdata.TopicDataRecordList != null)
                        {
                            foreach (TopicDataRecord record in topicdata.TopicDataRecordList.Elements)
                            {
                                this.InvokeTopicCallbacks(record);
                            }
                        }

                        if (topicdata.Error != null)
                        {
                            Debug.LogError(topicdata.Error.ToString());
                        }

                        msReadBuffer.Position = 0;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.LogError(ex.ToString());
        }
    }

    private void OnWebsocketCloseUWP(Windows.Networking.Sockets.IWebSocket sender, Windows.Networking.Sockets.WebSocketClosedEventArgs args)
    {
        Debug.LogError("OnWebsocketCloseUWP; Code: " + args.Code + ", Reason: \"" + args.Reason + "\"");
        this.connected = false;
    }
#else
    private async void ReadSocket()
    {
        byte[] receiveBuffer = new byte[RECEIVE_BUFFER_SIZE];
        int receiveBufferCount = 0;

        while (clientWebsocket.State == WebSocketState.Open && !ctsReadSocket.IsCancellationRequested)
        {
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(receiveBuffer, receiveBufferCount, receiveBuffer.Length - receiveBufferCount);
            WebSocketReceiveResult receiveResult = null;

            try
            {
                receiveResult = await clientWebsocket.ReceiveAsync(arraySegment, ctsReadSocket.Token);
                receiveBufferCount += receiveResult.Count;
            }
            catch (WebSocketException ex)
            {
                if (!ctsReadSocket.IsCancellationRequested)
                {
                    Debug.LogError("UBII - UbiiTopicDataClientWS.ReadSocket(): " + ex.ToString());
                    if (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        clientWebsocket.Dispose();
                        CbTopicDataConnectionLost();
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                if (!ctsReadSocket.IsCancellationRequested)
                {
                    Debug.LogError("UBII - UbiiTopicDataClientWS.ReadSocket(): " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("UBII - UbiiTopicDataClientWS.ReadSocket(): " + ex.ToString());
            }

            if (receiveResult.EndOfMessage)
            {
                // PING message
                if (receiveResult.Count == 4)
                {
                    string msgString = Encoding.UTF8.GetString(arraySegment.AsMemory().ToArray(), 0, receiveResult.Count);
                    if (msgString == "PING")
                    {
                        await this.SendBytes(Encoding.UTF8.GetBytes("PONG"), ctsReadSocket.Token);
                    }
                }
                // topic data
                else
                {
                    try
                    {
                        await msReadBuffer.WriteAsync(receiveBuffer, 0, receiveBufferCount, ctsReadSocket.Token);
                        TopicData topicData = TopicData.Parser.ParseFrom(msReadBuffer.GetBuffer(), 0, (int)msReadBuffer.Position);
                        CbHandleMessage(topicData);
                        
                        msReadBuffer.Position = 0;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                }

                receiveBufferCount = 0;
            }
            else
            {
                await msReadBuffer.WriteAsync(receiveBuffer, 0, receiveBufferCount, ctsReadSocket.Token);
                receiveBufferCount = 0;
            }
        }
    }

    private async Task<bool> SendBytes(byte[] bytes, CancellationToken ct)
    {
        var arraySegment = new ArraySegment<Byte>(bytes);
        await clientWebsocket.SendAsync(arraySegment, WebSocketMessageType.Binary, true, ct);
        return true;
    }
#endif

    public bool IsConnected()
    {
        return this.connected;
    }

    public async Task<bool> Send(TopicData topicData, CancellationToken ct)
    {
        MemoryStream memoryStream = new MemoryStream();
        CodedOutputStream codedOutputStream = new CodedOutputStream(memoryStream);
        topicData.WriteTo(codedOutputStream);
        codedOutputStream.Flush();
        var bytebuffer = memoryStream.ToArray();
        return await this.SendBytes(bytebuffer, ct);
    }
}

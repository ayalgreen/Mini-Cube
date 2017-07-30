using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;

namespace MiniCube
{
    class Server
    {
        private TcpListener tcpListn = null;
        private Thread listenThread = null;
        private bool isServerListening = false;
        private bool notTimedOut;
        private CubeForm cube;
        private ASCIIEncoding asciiEnco;
#if (DEBUG)
        Stopwatch serverWatch = new Stopwatch();
        long lastTime = 0;
#endif


        public Server(CubeForm passedCube)
        {
            cube = passedCube;
            asciiEnco = new ASCIIEncoding();
            notTimedOut = true;
            tcpListn = new TcpListener(IPAddress.Any, 8090);
            listenThread = new Thread(new ThreadStart(listeningToclients));
            this.isServerListening = true;
            listenThread.Start();
            
        }

        //listener
        private void listeningToclients()
        {
            tcpListn.Start();
            Console.WriteLine("Server started!");
            Console.WriteLine("Waiting for clients...");
            while (this.isServerListening)
            {
                try
                {
                    TcpClient tcpClient = tcpListn.AcceptTcpClient();
                    Thread clientThread = new Thread(new ParameterizedThreadStart(handleClient));
                    clientThread.Start(tcpClient);
                }
                catch (Exception ex)
                {

                }
            }

        }

        //client handler
        private void handleClient(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            Console.WriteLine("Client connected!");

            NetworkStream stream = client.GetStream();
            
            Handshaker(client, stream);
#if (DEBUG)
            serverWatch.Restart();
#endif
            Interact(client, stream);
                        
            stream.Flush();
            stream.Close();
            client.Close(); //close the client
        }

        //TODO: add timeout
        private void Handshaker(TcpClient client, NetworkStream stream)
        {
            StringBuilder clientMessage = new StringBuilder("");

            while (true && notTimedOut)
            {
                while (!stream.DataAvailable && notTimedOut) ;
                //read data from client
                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                clientMessage.Append(asciiEnco.GetString(bytes));
                String dataString = clientMessage.ToString();
                //TODO: does this need to be checked in following receptions?
                if (new Regex("^GET").IsMatch(dataString))
                {
                    clientMessage.Clear();
                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(dataString).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")))
                                    + Environment.NewLine
                        + Environment.NewLine);
                    stream.Write(response, 0, response.Length);
                    break;
                }
            }
        }
              
        private void Interact(TcpClient client, NetworkStream stream)
        {
            //TODO: support long messages?
            bool header = true;
            int messageLength = 0;
            Byte[] mask = new Byte[4];
            Byte[] data;
            Byte[] decoded;
            while (true && notTimedOut)
            {
                while (!stream.DataAvailable && notTimedOut)
                {
                    //don't waste all CPU in vain
                    System.Threading.Thread.Sleep(1);
                }
#if (DEBUG)
                Debug.WriteLine("got getQuat after: {0} milisecs from previous", serverWatch.ElapsedMilliseconds - lastTime);
                lastTime = serverWatch.ElapsedMilliseconds;
#endif

                //TODO: wait to complete the data
                //read data from client

                //first taking care of header
                if (header)
                {
                    if (client.Available >= 6)
                    {
                        Byte[] bytes = new Byte[2];
                        stream.Read(bytes, 0, bytes.Length);
                        if (bytes[0] != 129)
                        {
                            //Byte[] junk = new Byte[client.Available];
                            //stream.Read(junk, 0, junk.Length);
                            if (bytes[0] == 136)
                            {
                                Console.WriteLine("Client dissconnected!");
                                break;
                            }
                            Console.WriteLine("Unhandled opcode! first byte is {0}", bytes[0]);
                            break;
                        }
                        messageLength = bytes[1] & 127;
                        if (messageLength > 126)
                        {
                            Console.WriteLine("message too long!");
                            break;
                        }
                        stream.Read(mask, 0, mask.Length);
                        header = false;
                    }
                    else
                    {
                        continue;
                    }
                }
                //then data
                if (client.Available >= messageLength)
                {
                    data = new Byte[messageLength];
                    decoded = new Byte[messageLength];
                    stream.Read(data, 0, messageLength);
                    header = true;
                    for (int i = 0; i < data.Length; i++)
                    {
                        decoded[i] = (Byte)(data[i] ^ mask[i % 4]);
                    }
                    String dataString = asciiEnco.GetString(decoded);
                    //Console.WriteLine(dataString);
                    if (dataString == "getQuat")
                    {
                        SendQuat(stream);
                    }
                }
            }
        }

        public void stopServer()
        {
            this.isServerListening = false;
            tcpListn.Stop();
            notTimedOut = false;
            Console.WriteLine("Server stoped!");
        }

        public void SendQuat(NetworkStream stream)
        {
            float[] quat = cube.GetCorrectedQuatFloats();
            MemoryStream memStream = new MemoryStream();
            Byte[] headerBytes = new Byte[2] { (Byte)130, (Byte)16 };
            memStream.Write(headerBytes, 0, headerBytes.Length);
            Byte[] XBytes = BitConverter.GetBytes(quat[0]);
            Byte[] YBytes = BitConverter.GetBytes(quat[1]);
            Byte[] ZBytes = BitConverter.GetBytes(quat[2]);
            Byte[] WBytes = BitConverter.GetBytes(quat[3]);
            //taking care of Endianess js conventation
            Array.Reverse(XBytes);
            Array.Reverse(YBytes);
            Array.Reverse(ZBytes);
            Array.Reverse(WBytes);

            memStream.Write(XBytes, 0, 4);
            memStream.Write(YBytes, 0, 4);
            memStream.Write(ZBytes, 0, 4);
            memStream.Write(WBytes, 0, 4);
            Byte[] response = memStream.ToArray();
            stream.Write(response, 0, response.Length);
        }

    }
}

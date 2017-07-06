using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace MiniCube
{
    class Server
    {
        private TcpListener tcpListn = null;
        private Thread listenThread = null;
        private bool isServerListening = false;

        public Server()
        {
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

            StringBuilder clientMessage = new StringBuilder("");
            bool notTimedOut = true;
            string dataString = "";
            Byte[] bytes = new Byte[0];
            while (true && notTimedOut)
            {
                while (!stream.DataAvailable && notTimedOut) ;
                //read data from client
                bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);
                ASCIIEncoding asciiEnco = new ASCIIEncoding();
                
                clientMessage.Append(asciiEnco.GetString(bytes));
                dataString = clientMessage.ToString();
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
                }
                else
                {
                    Console.WriteLine(dataString);
                }                    
            }
            
            //write data to client
            //byte[] byteBuffOut = asciiEnco.GetBytes("Hello client! \n"+"You said : " + clientMessage.ToString() +"\n Your ID  : " + new Random().Next());
            //stream.Write(byteBuffOut, 0, byteBuffOut.Length);
            //writing data to the client is not required in this case

            stream.Flush();
            stream.Close();
            client.Close(); //close the client
        }

        public void stopServer()
        {
            this.isServerListening = false;
            tcpListn.Stop();
            Console.WriteLine("Server stoped!");
        }

    }
}

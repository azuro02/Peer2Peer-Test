using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Threading;


namespace ClientB
{
    internal class ClientB
    {
        static void Main(string[] args)
        {
            int port = 20000;
            IPAddress ServerIP = IPAddress.Parse("127.0.0.1");
            byte[] sendData = Encoding.Unicode.GetBytes("Hallo");
            int count = 0;
            IPEndPoint ep = new IPEndPoint(ServerIP, port);


            try
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Connect(ServerIP, 20000);
                udpClient.Send(sendData, sendData.Length);
                count++;
                Console.WriteLine(count + " packets Send.");

                Console.WriteLine("reciving...");
                Lauscher.Listen(2222);

                //With TCP
                //TcpListener server = new TcpListener(IPAddress.Any, 22000);
                //server.Start();
                //TcpClient client = server.AcceptTcpClient();

                //var buffer = new byte[1024];

                //NetworkStream stream = client.GetStream();
                //stream.Read(buffer, 0, buffer.Length);
                //string data = Encoding.Unicode.GetString(buffer);
                //Console.WriteLine(data);

                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static class Lauscher
        {
            public static void Listen(int port)
            {
                UdpClient listener = new UdpClient(port);

                try
                {
                    while (true)
                    {
                        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, port);
                        var data = listener.Receive(ref serverEP);
                        string data2 = Encoding.Unicode.GetString(data);
                        Console.WriteLine(data2);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
        }
    }
}
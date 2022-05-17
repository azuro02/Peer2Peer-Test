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


namespace Client
{
    internal class Client
    {
        public static string Locals { get; set; }
        static void Main(string[] args)
        {
            int serverPort = 13000;
            IPAddress ServerIP = IPAddress.Parse("192.168.178.22");
            
            int count = 0;
            IPEndPoint ep = new IPEndPoint(ServerIP, serverPort);

            
            try
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Connect(ep); //Verbindet sich mit Server
                Locals = udpClient.Client.LocalEndPoint.ToString(); //Evt müssen erst Daten gesendet werden
                byte[] sendData = Encoding.Unicode.GetBytes(Locals);

                udpClient.Send(sendData, sendData.Length); //Sendet daten an Server
                count++;
                Console.WriteLine(count + " packets Send.");

                Console.WriteLine("waiting for response...");
                Connector.Listen(2222); //Lauscht an port 2222 auf Antwort des Servers

                //udpClient.Close();
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static class Connector
        {
            public static void Listen(int port)
            {
                try
                {
                    var data = Lauscher.Lauschen(port);
                    string data2 = Encoding.Unicode.GetString(data);
                    Console.WriteLine(data2);
                    var data3 = data2.Split(';');
                    string[] data4 = data3[0].Split(':');
                    string peerIp = data4[0];
                    string peerPort = data4[1];

                    data4 = data3[1].Split(':');
                    string localIp = data4[0];
                    string localPort = data4[1];

                    //Auf eigenen Port lauschen, ob der andere Client etwas sendet...
                    Thread listenThread = new Thread(() =>
                    {
                        //Lauschen
                        string[] ep = Locals.Split(':');
                        var message = Encoding.Unicode.GetString(Lauscher.Lauschen(Int32.Parse(ep[1])));
                        Console.WriteLine(message);
                    });
                    listenThread.Start();

                    Thread thread1 = new Thread(() => Connect(peerIp, Int32.Parse(peerPort)));
                    Thread thread2 = new Thread(() => Connect(localIp, Int32.Parse(localPort)));
                    thread1.Start();
                    thread2.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            public static void Connect(string ip, int port)
            {
                while (true)
                {
                    UdpClient client = new UdpClient();
                    client.Connect(ip, port);
                    var message = Encoding.Unicode.GetBytes($"hallo freund!, ich bin {Locals}");
                    client.Send(message);
                    Thread.Sleep(1000);
                }
            }
        }

        static class Lauscher
        {
            public static byte[] Lauschen(int port)
            {
                UdpClient listener = new UdpClient(port);
                try
                {
                    while (true)
                    {
                        IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, port);
                        var data = listener.Receive(ref serverEP);
                        listener.Close();
                        listener.Dispose();
                        return data;
                    }
                }
                catch (Exception ex)
                {
                    listener.Close();
                    listener.Dispose();
                    Console.WriteLine(ex.ToString());
                }
                listener.Close();
                listener.Dispose();
                return null;
            }
        }
    }
}
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
        public static IPEndPoint ServerEp { get; set; }
   
        static void Main(string[] args)
        {
            //Init
            int serverPort = 13000;
            IPAddress ServerIP = IPAddress.Parse("192.168.178.22");
            
            int count = 0;
            ServerEp = new IPEndPoint(ServerIP, serverPort);

            
            try
            {
                //Server anfragen und lokalen EndPoint schicken
                UdpClient udpClient = new UdpClient();
                udpClient.Connect(ServerEp); 
                Locals = udpClient.Client.LocalEndPoint.ToString(); 
                byte[] sendData = Encoding.Unicode.GetBytes(Locals);

                udpClient.Send(sendData, sendData.Length); 
                udpClient.Close();
                udpClient.Dispose();
                count++;
                Console.WriteLine(count + " packets Send.");

                //Auf Antwort von Server mit Daten von Peer Partner warten
                Console.WriteLine("waiting for response...");
                Connector.Listen(2222); 

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static class Connector
        {
            static bool sending = true;
            public static void Listen(int port)
            {
                try
                {
                    //Auf Serverantwort warten
                    var data = Lauscher.Lauschen(port);

                    //Daten des Peer Partners verarbeiten
                    string data2 = Encoding.Unicode.GetString(data);
                    Console.WriteLine(data2);
                    var data3 = data2.Split(';');
                    string[] data4 = data3[0].Split(':');
                    string peerIp = data4[0];
                    string peerPort = data4[1];

                    data4 = data3[1].Split(':');
                    string localIp = data4[0];
                    string localPort = data4[1];

                    //Antwort an Server senden (paket erhalten)
                    
                    Thread antwortThread = new Thread(() =>
                    {
                        while (sending)
                        {
                            byte[] sendData = Encoding.Unicode.GetBytes("Daten erhalten!");
                            Sender.Senden(ServerEp, sendData);
                            Thread.Sleep(2000);
                        }
                    });
                    antwortThread.Start();

                    Lauscher.Lauschen(port);
                    //Senden Stoppen
                    sending = false; //Achtung illegal! hierfür lieber Async oder so statt Thread verwenden


                    //an Peer Partner Daten senden
                    Thread thread1 = new Thread(() => Connect(peerIp, Int32.Parse(peerPort)));
                    Thread thread2 = new Thread(() => Connect(localIp, Int32.Parse(localPort)));
                    thread1.Start();
                    thread2.Start();

                    //Auf eigenen Port lauschen, ob der andere Client etwas sendet
                    while (true)
                    {
                        string[] ep = Locals.Split(':');
                        var message = Encoding.Unicode.GetString(Lauscher.Lauschen(Int32.Parse(ep[1])));
                        Console.WriteLine(message);
                    }

                    //TCP Verbindung aufbauen
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            public static void Connect(string ip, int port)
            {
                //an Peer Partner Daten senden
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

        static class Sender
        {
            public static void Senden(IPEndPoint ep, byte[] data)
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Connect(ep.Address, ep.Port);
                udpClient.Send(data);
            }
        }
    }
}
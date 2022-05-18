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


namespace Server1
{
    public class Server
    {
        public static IPEndPoint listenerEp = new IPEndPoint(IPAddress.Any, 0); //der Client nimmt jede Ip und jeden Port an
        public static void Main(string[] args)
        {
            Queue<Client> clients = new Queue<Client>();

            while (true)
            {
                UdpClient udpClient = new UdpClient(13000); //Der client Lauscht auf Port 20000

                var buffer  = udpClient.Receive(ref listenerEp);
                udpClient.Close();
                udpClient.Dispose();
                string data = Encoding.Unicode.GetString(buffer);

                string[] data2 = data.Split(':');
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(data2[0]), Int32.Parse(data2[1]));
                clients.Enqueue(new Client(listenerEp, localEndPoint)); //neuen Client in die Warteschlange einreihen
                Console.WriteLine(clients.Count + " Client(s) Verbunden.");

                if(clients.Count % 2 == 0) 
                {
                    Client a = clients.Dequeue();
                    Client b = clients.Dequeue();
                    Shipper.Ship(a, b);
                }
            }
            
        }
    }

    public class Client
    {
        //Attribute
        IPAddress Ip { get; set; }
        string Port { get; set; }
        IPAddress LocalIp { get; set; }
        int LocalPort { get; set; }
        public IPEndPoint Ep { get; set; }
        public IPEndPoint LocalEp { get; set; }

        public Client(IPEndPoint ep, IPEndPoint localEndPoint)
        {
            Ep = ep;
            Ip = ep.Address;
            Port = ep.Port.ToString();
            Console.WriteLine("Data from : "+ Ip + ":" + Port);
            
            LocalEp = localEndPoint;
            LocalIp = localEndPoint.Address;
            LocalPort = localEndPoint.Port;
        }

        public void Exchange(IPEndPoint peerEp, IPEndPoint peerLocalEp)
        {

            //Response
            UdpClient udpClient = new UdpClient();
            udpClient.Connect(Ip, 2222);

            //var message = Encoding.Unicode.GetBytes("Server: verbinden mit " + ep.Address.ToString() + ":" + ep.Port);
            //udpClient.Send(message);

            var data = Encoding.Unicode.GetBytes(peerEp.Address.ToString() + ":" + peerEp.Port + ";" + peerLocalEp.Address.ToString()+ ":" + peerLocalEp.Port);
            udpClient.Send(data);
            udpClient.Close();
            udpClient.Dispose();

            UdpClient lauscher = new UdpClient(13000);
            var buffer = lauscher.Receive(ref Server.listenerEp);
            var data2 = Encoding.Unicode.GetString(buffer);
            Console.WriteLine(data2);
            lauscher.Close();
            lauscher.Dispose();
        }
    }

    public static class Shipper
    {
        public static void Ship(Client a, Client b)
        {
            a.Exchange(b.Ep, b.LocalEp);
            b.Exchange(a.Ep, a.LocalEp);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using list_server;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace list_server
{
    class Setup
    {
        static X509Certificate2 serverCertificate = null;

        static TcpListener serversocket;
        static int port = 801;

        static ClientComController ccc;
        static Logger lggr;

        static void Main(string[] args)
        {
            serverCertificate = new X509Certificate2();
            serverCertificate.Import(File.ReadAllBytes(@"C:\Users\Tobias\Desktop\cert.pfx"), "1", X509KeyStorageFlags.MachineKeySet);
            Initilize();
        }

        private static void Initilize()
        {
            lggr = Logger.GetInstance();

            SetupServer();
            Console.Title = "Server";
        }

        private static void SetupServer()
        {
            if (SocketSetup())
            {
                ccc = ClientComController.GetInstance();

                Log("Server started listening");

                ListenForClients();
            }
            else
            {

                //Platzhalter für Problem findung

            }
        }

        private static bool SocketSetup()
        {
            try
            {
                //Bindet Socket an Port und akzeptiert immer

                Log("Setting up server");
                serversocket = new TcpListener(IPAddress.Any,port);
                serversocket.Start();

                return true;
            }
            catch (Exception e)
            {
                Log("Setup failed");

                Log(e.ToString());
                return false;
            }
        }

        private static void ListenForClients()
        {
            while (true)
            {
                //Wartet auf Vebindung und Convertiert diese zu Clients

                string pregen = ccc.GenerateID();

                ccc.Add(new ClientCom(serversocket.AcceptTcpClient(), pregen , serverCertificate));
            }
        }

        private static void Log(string text)
        {
            lggr.Log("Setup",text);
        }
    }
}
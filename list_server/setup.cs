using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using list_server;

namespace list_server
{
    class Setup
    {
        private static Socket serversocket;
        static int port = 801;

        static ClientComController ccc;
        static Logger lggr;

        static bool server = true;
        static string host = "bspcliste";

        static void Main(string[] args)
        {
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

                Thread listen = new Thread(ListenForClients);
                listen.Start();
                Log("Server started listening");
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
                serversocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Log("Setting up server");
                serversocket.Bind(new IPEndPoint(IPAddress.Any, port));

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

                serversocket.Listen(0);
                ccc.Add(new ClientCom(serversocket.Accept(), pregen));
            }
        }

        private static void Log(string text)
        {
            lggr.Log("Setup",text);
        }
    }
}
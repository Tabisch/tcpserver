using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using list_server;

namespace PC_Liste_Server
{
    class Setup
    {
        private static readonly Socket serversocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public static readonly int port = 801;

        static ClientComController ccc;
        static Logger lggr;

        static void Main(string[] args)
        {
            Initilize();
            SocketSetup();
        }

        private static void Initilize()
        {
            ccc = ClientComController.getInstance();
            lggr = Logger.GetInstance();
        }

        private static void SocketSetup()
        {
            if (SetupServer())
            {
                Thread listen = new Thread(ListenForClients);
                listen.Start();
                Log("Server started listening");
            }
            else
            {

                //Platzhalter für Problem findung

            }
        }

        private static bool SetupServer()
        {
            try
            {
                //Bindet Socket an Port und akzeptiert immer

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
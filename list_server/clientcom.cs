using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace list_server
{
    class ClientCom
    {

        Socket Client;
        Thread Receiver;
        Thread Sender;
        Thread windup;

        DataController dc;
        Logger l;

        List<string> sendrequests;

        public string Name;
        public string ActiveUser;
        public bool[] flag;
        string ID;

        public ClientCom(Socket client, string ID)
        {
            dc = DataController.GetInstance();
            l = Logger.GetInstance();

            this.ID = ID;
            Client = client;

            Log("Connected");

            Initilze();
        }

        public ClientCom(string host,int port)
        {
            dc = DataController.GetInstance();
            l = Logger.GetInstance();

            this.ID = "Client";

            ConnectAsClient(host,port);

            Log("Connected as Client");

            Initilze();
        }

        private void Initilze()
        {
            Log("Entering Client initializing phase");

            flag = new bool[4];

            
            Client.ReceiveTimeout = 20000;

            sendrequests = new List<string>();

            Receiver = new Thread(Receive);
            Receiver.Start();
            Sender = new Thread(Send);
            Sender.Start();
        }

        public void ConnectAsClient(string host,int port)
        {
            IPHostEntry hostEntry;

            hostEntry = Dns.GetHostEntry(host);

            if (hostEntry.AddressList.Length > 0)
            {
                bool working = false;

                do
                {

                    try
                    {

                        var ip = hostEntry.AddressList[0];
                        Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        Client.Connect(ip, port);

                        working = true;

                    }
                    catch (Exception)
                    {

                    }

                } while (working == false);

            }
        }

        public void CloseConnection(string text)
        {
            if (flag[0] == false)
            {
                flag[0] = true;
                Log(text);

                windup = new Thread(Windup);
                windup.Start();
            }
        }

        private void Windup()
        {
            Log("Windup started");

            while (true)
            {
                if (!Receiver.IsAlive)
                {
                    flag[1] = true;
                }

                if (!Sender.IsAlive)
                {
                    flag[2] = true;
                }

                if (flag[0] && flag[1] && flag[2])
                {
                    flag[3] = true;
                    Log("Windup finished");
                    FinishWindup();
                    break;
                }

                Thread.Sleep(5000);
            }
        }

        private void FinishWindup()
        {
            sendrequests = null;
            Client.Close();
            Client = null;
            Receiver = null;
            Sender = null;

            dc = null;
            l = null;
        }

        public void Send()
        {
            bool empty = false;

            while (true)
            {
                if (sendrequests.Count == 0)
                {
                    if (flag[0] == false)
                    {
                        if (empty)
                        {
                            AddSendRequest("beat");
                            empty = false;
                        }
                        else
                        {
                            Thread.Sleep(5500);
                            empty = true;
                        }
                    }
                    else
                    {
                        if (SendRequestStackSize() == 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    empty = false;

                    if (Sending(sendrequests[0]))
                    {
                        sendrequests.Remove(sendrequests[0]);
                    }
                }
            }

            Log("Sender closed");
        }

        private bool Sending(string text)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(text + "\0");

            try
            {
                Client.Send(Buffer);
                Log("Sending : " + text);
                return true;
            }
            catch (SocketException)
            {
                CloseConnection("Send Error");
                sendrequests.Clear();
                return false;
            }
        }

        public void AddSendRequest(string text)
        {
            if (flag[0] == false)
            {
                sendrequests.Add(text);
            }
        }

        public int SendRequestStackSize()
        {
            return sendrequests.Count;
        }

        private void Receive()
        {
            Log("Receiver initalized");

            if(ID == "Client")
            {
                Thread.Sleep(5000);
            }

            do
            {
                Log("Waiting for data");

                try
                {
                    int BufferReadsize;
                    byte[] Buffer = new byte[Client.SendBufferSize]; ;

                    BufferReadsize = Client.Receive(Buffer);

                    if (BufferReadsize > 0)
                    {
                        string result = Encoding.ASCII.GetString(Buffer, 0, BufferReadsize);

                        List<string> clean = CleanupInput(result);

                        foreach (string element in clean)
                        {
                            Log("Received : " + element);

                            dc.AddToStack(ID, element, true);
                        }
                    }
                    else
                    {
                        CloseConnection("Disconnect");
                    }
                }
                catch (Exception)
                {
                    CloseConnection("Execption kill");
                }
            } while (flag[0] == false);

            Log("Receiver closed");
        }

        private void Log(string Text)
        {
            l.Log("Client : " + ID, Text);
        }

        private List<string> CleanupInput(string text)
        {

            List<string> vaild = new List<string>();

            string[] checksplitt = text.Split('\0');

            foreach (string element in checksplitt)
            {
                if (!string.IsNullOrWhiteSpace(element))
                {
                    /*
                    element.Replace("\n", "");
                    element.Replace("\r", "");
                    element.Replace("\t", "");
                    element.Replace("\v", "");
                    */
                    vaild.Add(element);
                }
            }

            return vaild;
        }

        public void SetName(string Name)
        {
            this.Name = Name;
        }

        public string GetName()
        {
            return Name;
        }

        public bool[] GetStatus()
        {
            return flag;
        }

        public string GetID()
        {
            return ID;
        }
    }

    class ClientComController
    {
        private static ClientComController ccc;
        private List<ClientCom> cc;
        private List<string> idlist;
        Thread DeadClientCleaner;

        private ClientComController()
        {
            cc = new List<ClientCom>();
            idlist = new List<string>();
            DeadClientCleaner = new Thread(CleanDeadClient);
        }

        public static ClientComController getInstance()
        {
            if (ccc == null)
            {
                ccc = new ClientComController();
            }

            return ccc;
        }

        public void Add(ClientCom clientcom)
        {
            cc.Add(clientcom);
        }

        private bool CheckIDCollision(string ID)
        {
            bool check = true;

            foreach (ClientCom client in cc)
            {
                if (client.GetID() == ID)
                {
                    check = false;
                }
            }

            return check;
        }

        public string GenerateID()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[32];
            string finalString;
            var random = new Random();

            do
            {
                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                finalString = new String(stringChars);

            } while (!CheckIDCollision(finalString));

            return finalString;
        }

        public void CleanDeadClient()
        {
            foreach (ClientCom client in cc)
            {
                if (client.GetStatus()[3])
                {
                    cc.Remove(client);
                }
            }
        }

        public int Size()
        {
            return cc.Count;
        }

        public void Clear()
        {
            cc.Clear();
        }

        public ClientCom GetClientByID(string ID)
        {
            ClientCom clienttoreturn = null;

            CleanDeadClient();

            foreach (ClientCom client in cc)
            {
                if (client.GetID() == ID)
                {
                    clienttoreturn = client;
                }
            }

            return clienttoreturn;
        }
    }
}
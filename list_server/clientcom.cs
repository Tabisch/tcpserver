using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.IO;

namespace list_server
{
    class ClientCom
    {
        TcpClient Client;
        X509Certificate cert;
        SslStream sslStream;
        NetworkStream nonsslStream;

        DateTime timestamp;

        DataController dc;
        Logger lggr;

        List<string> sendrequests;

        public string Name;
        public string ActiveUser;
        public bool[] flag;
        string ID;
        bool ssl;

        int loglevel;
        int logleveldefault = 1;

        public ClientCom(TcpClient client, string ID , X509Certificate cert)
        {
            dc = DataController.GetInstance();
            lggr = Logger.GetInstance();

            this.cert = cert;

            this.ID = ID;
            this.Client = client;

            Log("Connected",1);

            Initilze();
        }

        private void Initilze()
        {
            Log("Entering Client initializing phase",1);
            
            try
            {
                sslStream = new SslStream(Client.GetStream(), false);
                sslStream.AuthenticateAsServer(cert, false, SslProtocols.Tls, true);
            }
            catch
            {
                ssl = false;
                nonsslStream = Client.GetStream();
            }
            

            flag = new bool[3];

            flag[0] = false;
            flag[1] = false;
            flag[2] = false;


            sslStream.ReadTimeout = 20000;
            sslStream.WriteTimeout = 20000;

            sendrequests = new List<string>();
        }

        public void CloseConnection(string text)
        {
            if (flag[0] == false)
            {
                flag[0] = true;
                Log(text,1);
            }
        }

        public void Windup()
        {
            if (flag[0] && flag[1])
            {
                flag[2] = true;
                Log("Client " + GetID() + " dead",1);
            }
        }

        public void Send()
        {
            if (flag[1] == false)
            {
                if (SendRequestStackSize() == 0)
                {
                    if (flag[0] == false)
                    {
                        AddSendRequest("beat");
                    }
                    else
                    {
                        flag[1] = true;
                    }
                }
                else
                {
                    for (int i = 0; i < SendRequestStackSize(); i++)
                    {
                        if (Sending(sendrequests[0]))
                        {
                            sendrequests.Remove(sendrequests[0]);
                        }
                    }
                }
            }
        }

        private bool Sending(string text)
        {
            byte[] Buffer = Encoding.ASCII.GetBytes(text + "\0");

            if(ssl)
            {
                try
                {
                    sslStream.Write(Buffer);
                    Log("Sending : " + text, 1);
                    return true;
                }
                catch (SocketException)
                {
                    CloseConnection("Send Error");
                    sendrequests.Clear();
                    return false;
                }
            }
            else
            {
                try
                {
                    nonsslStream.Write(Buffer,0,Buffer.Length);
                    Log("Sending : " + text, 1);
                    return true;
                }
                catch (SocketException)
                {
                    CloseConnection("Send Error");
                    sendrequests.Clear();
                    return false;
                }
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

        public void Receive()
        {
            if (flag[0] == false)
            {
                Log("Waiting for data",1);

                try
                {
                    int BufferReadsize;

                    byte[] Buffer = new byte[Client.ReceiveBufferSize];

                    BufferReadsize = Receive(Buffer);

                    if (BufferReadsize > 0)
                    {
                        string result = Encoding.ASCII.GetString(Buffer, 0, BufferReadsize);

                        List<string> clean = CleanupInput(result);

                        foreach (string element in clean)
                        {
                            Log("Received : " + element,1);

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
            }
        }

        private int Receive(byte[] Buffer)
        {
            if (ssl)
            {
                return sslStream.Read(Buffer, 0, Buffer.Length);
            }
            else
            {
                return nonsslStream.Read(Buffer, 0, Buffer.Length);
            }
        }

        private void Log(string Text, int value)
        {
            lggr.Log("Client : " + ID, Text,value);
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

        public bool Dead()
        {
            if(flag[2])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    class ClientComController
    {
        private static ClientComController ccc;
        private List<ClientCom> cc;
        private List<ClientCom> ccaddstack;
        private List<string> idlist;

        int logleveldefault = 1;

        Thread Receiver;

        Logger lggr;

        private ClientComController()
        {
            lggr = Logger.GetInstance();
            cc = new List<ClientCom>();
            ccaddstack = new List<ClientCom>();
            idlist = new List<string>();
            Receiver = new Thread(RunClients);
            Receiver.Start();

            Log("ClientController initialized",1);
        }

        public static ClientComController GetInstance()
        {
            if (ccc == null)
            {
                ccc = new ClientComController();
            }

            return ccc;
        }

        public void Add(ClientCom clientcom)
        {
            Log("Client " + clientcom.Name + " added",1);
            cc.Add(clientcom);
        }

        private void AddFromStack()
        {
            for(int count = 0 ; count < ccaddstack.Count; count++)
            {
                cc.Add(ccaddstack[0]);
                ccaddstack.Remove(ccaddstack[0]);
            }
        }

        private bool CheckIDCollision(string ID)
        {
            bool check = true;

            foreach (string id in idlist)
            {
                if (id == ID)
                {
                    check = false;
                    break;
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

            idlist.Add(finalString);

            return finalString;
        }

        private void CleanDeadClient()
        {
            for (int i = 0; i < Size(); i++)
            {
                cc[i].Windup();

                if (cc[i].Dead())
                {
                    Log("Client " + cc[i].GetID() + " removed",1);
                    idlist.Remove(cc[i].GetID());
                    cc.Remove(cc[i]);
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

        private void Log(string text, int value)
        {
            lggr.Log("ClientController", text,value);
        }

        public int GetDefaultLoglevel()
        {
            return logleveldefault;
        }

        public ClientCom GetClientByID(string ID)
        {
            ClientCom clienttoreturn = null;

            foreach (ClientCom client in cc)
            {
                if (client.GetID() == ID)
                {
                    clienttoreturn = client;
                }
            }

            return clienttoreturn;
        }

        private void RunClients()
        {
            while (true)
            {
                CleanDeadClient();
                AddFromStack();

                foreach (ClientCom client in cc)
                {
                    client.Receive();
                    client.Send();
                }
            }
        }
    }
}
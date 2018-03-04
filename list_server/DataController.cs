using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace list_server
{
    class DataController
    {

        private static DataController dc;

        Requeststack rs;
        ClientComController ccc;
        Logger l;

        Thread worker;

        private DataController()
        {
            rs = new Requeststack();
            l = Logger.GetInstance();
            ccc = ClientComController.getInstance();

            worker = new Thread(DataControllerWorker);
            worker.Start();
        }

        public static DataController GetInstance()
        {
            if (dc == null)
            {
                dc = new DataController();
            }

            return dc;
        }

        public void AddToStack(string ID,string text,bool simple)
        {
            if (simple == false)
            {
                string[] checksplitt = text.Split('\r');

                foreach (string element in checksplitt)
                {
                    if (!string.IsNullOrWhiteSpace(element))
                    {
                        rs.Add(ID, element);
                    }
                }
            }
            else
            {
                rs.Add(ID,text);
            }
        }

        private void DataControllerWorker()
        {
            while(true)
            {
                if (rs.Size() > 0)
                {
                    try
                    {
                        Log("Stacksize = " + rs.Size());

                        string id = rs.GetRequestID();
                        string content = rs.GetRequestContent();
                        
                        ccc.GetClientByID(id).AddSendRequest("Echo : " + content);
                        Log("Added " + "Echo : " + content + " to " + id);
                    }
                    catch (Exception e)
                    {
                        Log("DataControllerWorker error");
                        Log(e.ToString());
                    }
                }
            }
        }

        public int StackSize()
        {
            return rs.Size();
        }

        public void Clear()
        {
            rs.Clear();
        }

        private void Log(string text)
        {
            l.Log("DataController",text);
        }

    }

    class Requeststack
    {
        List<string[]> req;
        string[] reqdemension;

        public Requeststack()
        {
            req = new List<string[]>();
        }

        public void Add(string clientID, string text)
        {
            reqdemension = new string[2];
            reqdemension[0] = clientID;
            reqdemension[1] = text;

            req.Add(reqdemension);
        }

        public string[] GetRequestAll()
        {
            string[] returnofstack = { GetRequestID() , GetRequestContent() };
            return returnofstack;
        }

        public string GetRequestID()
        {
            return req[0][0];
        }

        public string GetRequestContent()
        {
            string returnofstack = req[0][1];
            req.Remove(req[0]);
            return returnofstack;
        }

        public int Size()
        {
            return req.Count;
        }

        public void Clear()
        {
            req.Clear();
        }
    }
}

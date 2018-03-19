using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace list_server
{
    class Logger
    {

        static Logger lggr;

        Thread filelogger;

        char splitchar = '#';

        string path = @"C:\users\tobias\desktop\";
        string filename = "log.txt";
        string filepath;

        int logclock = 10;
        int clock;

        int loglevel;
        int logleveldefault = 1;

        List<string> logstack;

        private Logger()
        {
            logstack = new List<string>();

            CreateFilePath();
            DeleteOldLogFile();
            SetClock();

            filelogger = new Thread(LogToTextFile);
            filelogger.Start();        
        }

        public static Logger GetInstance()
        {
            if(lggr == null)
            {
                lggr = new Logger();
            }

            return lggr;
        }

        private void AddToLogStack(string Text)
        {
            logstack.Add(Text);
        }

        private void SetClock()
        {
            clock = logclock * 1000;
        }

        public void Log(string System, string Text,int value)
        {
            if (lggr.LogPriorty(value, loglevel))
            {

                string FormatedLogString = FormatString(System, Text);

                LogToConsole(FormatedLogString);

                AddToLogStack(FormatedLogString);

            }
        }

        private void Log(string text, int value)
        {
            Log("Logger", text, value);
        }

        public int GetDefaultLoglevel()
        {
            return logleveldefault;
        }

        private void LogToConsole(string Text)
        {
            Console.Write(Text);
        }

        private void LogToTextFile()
        {

            Log("Textfilelogger started",2);

            while(true)
            {

                if (logstack.Count > 0)
                {
                    try
                    {
                        File.AppendAllText(filepath, MakeOneWriteString());
                        Log("Logged to " + filename,1);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Logfile access error", "Issues accessing the logfile");
                    }
                }
                Thread.Sleep(clock);
            }

        }

        private string FormatString(string System,string Text)
        {
            return String.Format(" {0,-10} {1,-41} {2}", DateTime.Now.ToString() + " " + splitchar, System, splitchar + " " + Text + "\r\n");
        }

        private void DeleteOldLogFile()
        {
            if(File.Exists(filepath))
            {
               File.Delete(filepath);
            }
        }

        private string MakeOneWriteString()
        {
            string big = "";

            foreach(string text in logstack)
            {
                big = big + text;
            }

            ClearStack();

            return big;
        }

        private void CreateFilePath()
        {
            filepath = path + filename;
        }

        private void ClearStack()
        {
            logstack.Clear();
        }

        public bool LogPriorty(int value,int loglevel)
        {
            bool log = false;

            if (loglevel >= value)
            {
                log = true;
            }

            return log;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace list_server
{
    class Settings
    {
        static Settings setting;

        Logger lggr;
        List<SettingsSet> ss;

        int loglevel;
        int logleveldefault = 1;

        private Settings()
        {
            ss = new List<SettingsSet>();
            lggr = Logger.GetInstance();
        }

        public Settings GetInstance()
        {
            if (setting == null)
            {
                setting = new Settings();
            }

            return setting;
        }

        public string GetSetting(string Name)
        {
            string value = null;

            foreach (SettingsSet s in ss)
            {
                if (s.ReturnName() == Name)
                {
                    value = s.ReturnValue();
                    break;
                }
            }

            return value;
        }

        public int GetLoggerSetting(string input,int defaultlevel)
        {
            int value = 0;

            input = input.ToLower();

            switch (input)
            {
                case "none": value = 4; break;
                case "low": value = 1; break;
                case "high": value = 2; break;
                case "all": value = 3; break;
                default: break;
            }

            if(value == 0)
            {
                value = defaultlevel;
            }

            return value;
        }

        public int GetLogValue(string input)
        {
            int defaultlog;

            return 0;
        }

        private void RefreshFromFile()
        {

        }

        public void Save(string Name,string value)
        {

        }

        private void Log(string text, int value)
        {

            lggr.Log("Settings", text, value);
        }

        public int GetDefaultLoglevel()
        {
            return logleveldefault;
        }
    }

    class SettingsSet
    {

        string Name;
        string Value;

        public SettingsSet(string Name,string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }

        public string ReturnName()
        {
            return Name;
        }

        public string ReturnValue()
        {
            return Value;
        }
    }
}

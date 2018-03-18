using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace list_server
{
    class Settings
    {
        static Settings setting;

        List<SettingsSet> ss;

        private Settings()
        {
            ss = new List<SettingsSet>();
        }
        
        public Settings GetInstance()
        {
            if(setting == null)
            {
                setting = new Settings();
            }

            return setting;
        }

        public string GetSetting(string Name)
        {
            string value = null;

            foreach(SettingsSet s in ss)
            {
                if(s.ReturnName() == Name)
                {
                    value = s.ReturnValue();
                }
            }

            return value;
        }

        public int ConvertForLogger(string input)
        {
            int value = 0;

            input = input.ToLower();

            switch(input)
            {
                case "none"     : value = 0;    break;
                case "low"      : value = 1;    break;
                case "high"     : value = 2;    break;
                case "all"      : value = 3;    break;
                default         :               break;
            }

            return value;
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

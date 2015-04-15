using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raspberry.IO.GeneralPurpose;

namespace sharpberry.controller.configuration
{
    public static class Config
    {
        static Config()
        {
            Gpio = new Gpio();
        }

        public static void Load(NameValueCollection context)
        {
            Context = context;
            Load();
        }
        public static void Load()
        {
            if (Context == null)
                Context = ConfigurationManager.AppSettings;

            Gpio.vAcc = Get<ConnectorPin>("gpio.vAcc");
            Gpio.vBatt = Get<ConnectorPin>("gpio.vBatt");
        }

        public static Gpio Gpio { get; set; }

        public static NameValueCollection Context { get; private set; }

        static T Get<T>(string key, T @default = default(T))
        {
            var str = Context[key];
            if (string.IsNullOrWhiteSpace(str))
                return @default;
            var td = TypeDescriptor.GetConverter(typeof(T));
            try
            {
                return (T)td.ConvertFromInvariantString(str);
            }
            catch (Exception e)
            {
                return @default;
            }
        }
    }
}

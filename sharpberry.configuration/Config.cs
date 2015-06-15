using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Reflection;
using log4net;

namespace sharpberry.configuration
{
    public class Config
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Config()
        {
            Gpio = new Gpio();
            DataCollection = new DataCollection();
            Display = new Display();
        }

        public void Load(NameValueCollection context)
        {
            Context = context;
            Load();
        }

        public void Load()
        {
            if (Context == null)
                Context = ConfigurationManager.AppSettings;

            foreach (var key in Context.AllKeys)
                this.ImportKeyValuePair(key, Context[key]);
        }

        public Gpio Gpio { get; private set; }
        public DataCollection DataCollection { get; private set; }
        public Display Display { get; private set; }

        public NameValueCollection Context { get; private set; }
    }
}

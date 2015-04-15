using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace sharpberry.obd.Commands
{
    public static class ObdCommands
    {
        static List<ObdCommand> all = new List<ObdCommand>();
        public static List<ObdCommand> All { get { return all; } }

        private static bool defaultsLoaded = false;

        public static void LoadDefaults()
        {
            lock (typeof (ObdCommands))
            {
                if (defaultsLoaded)
                    return;

                All.AddRange(GetDefaults());
                defaultsLoaded = true;
            }
        }

        public static ObdCommand CoolantTemp { get { return GetCommand(0x01, 0x05); } }
        public static ObdCommand VehicleSpeed { get { return GetCommand(0x01, 0x0D); } }
        public static ObdCommand EngineRpm { get { return GetCommand(0x01, 0x0C); } }
        public static ObdCommand IntakeAirTemp { get { return GetCommand(0x01, 0x0F); } }
        public static ObdCommand ThrottlePosition { get { return GetCommand(0x01, 0x11); } }
        public static ObdCommand RelativeAcceleratorPedalPosition { get { return GetCommand(0x01, 0x5A); } }
        public static ObdCommand EngineOilTemp { get { return GetCommand(0x01, 0x5C); } }
        public static ObdCommand EngineFuelRate { get { return GetCommand(0x01, 0x5E); } }
        
        public static ObdCommand GetCommand(int mode, int pid)
        {
            lock (typeof (ObdCommands))
            {
                return All.FirstOrDefault(cmd => cmd.Mode == mode && cmd.Pid == pid);
            }
        }

        public static IEnumerable<ObdCommand> GetDefaults()
        {
            return GetFromResourceFile("sharpberry.obd.Pids.standard.csv");
        }

        public static IEnumerable<ObdCommand> GetFromResourceFile(string filename)
        {
            using (var stream = typeof (ObdCommand).Assembly.GetManifestResourceStream(filename))
            {
                if (stream == null)
                    throw new ObdException("The specified resource file could not be found");
                using (var reader = new StreamReader(stream))
                {
                    return GetFromString(reader.ReadToEnd());
                }
            }
        }

        public static IEnumerable<ObdCommand> GetFromFile(string filename)
        {
            if (filename.All(c => c != Path.DirectorySeparatorChar))
                filename = Path.Combine(typeof(ObdCommand).Assembly.Location, "Pids", filename);
            return GetFromString(File.ReadAllText(filename));
        }

        public static IEnumerable<ObdCommand> GetFromString(string content)
        {
            var reader = new StringReader(content);
            var csv = new CsvReader(reader);
            csv.Configuration.HasHeaderRecord = true;
            while (csv.Read())
            {
                //"Mode","Pid","ExpectedBytes",Description,"MinValue","MaxValue",Units,Formula
                yield return new ObdCommand(
                    csv.GetField<string>("Description"),
                    csv.GetField<int>("Mode", HexStringToIntConverter.Instance),
                    csv.GetField<int>("Pid", HexStringToIntConverter.Instance),
                    csv.GetField<int?>("ExpectedBytes", NullableIntConverter.Instance) ?? 0,
                    csv.GetField<int?>("MinValue", NullableIntConverter.Instance),
                    csv.GetField<int?>("MaxValue", NullableIntConverter.Instance),
                    csv.GetField<string>("Units"),
                    csv.GetField<string>("Formula")
                    );
            }
        }

        private class HexStringToIntConverter : ITypeConverter
        {
            internal static readonly ITypeConverter Instance = new HexStringToIntConverter();

            public string ConvertToString(TypeConverterOptions options, object value)
            {
                throw new NotImplementedException();
            }

            public object ConvertFromString(TypeConverterOptions options, string text)
            {
                return Convert.ToInt32(text, 16);
            }

            public bool CanConvertFrom(Type type)
            {
                return type == typeof (string);
            }

            public bool CanConvertTo(Type type)
            {
                return type == typeof (int);
            }
        }

        private class NullableIntConverter : ITypeConverter
        {
            internal static readonly ITypeConverter Instance = new NullableIntConverter();

            public string ConvertToString(TypeConverterOptions options, object value)
            {
                return value == null ? null : value.ToString();
            }

            public object ConvertFromString(TypeConverterOptions options, string text)
            {
                int i;
                return int.TryParse(text, out i) ? i : default(int?);
            }

            public bool CanConvertFrom(Type type)
            {
                return type == typeof (string);
            }

            public bool CanConvertTo(Type type)
            {
                return type == typeof (int) || type == typeof (int?);
            }
        }
    }
}

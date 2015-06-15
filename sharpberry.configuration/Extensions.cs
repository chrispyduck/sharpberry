using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace sharpberry.configuration
{
    public static class Extensions
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal static void ImportKeyValuePair(this object obj, string key, string value)
        {
            var parts = key.Split('.');

            // identify type and object of requested config property
            var activeObject = (object)obj;
            var activeType = obj.GetType();
            var destinationType = default(Type);
            var member = default(PropertyInfo);
            for (var i = 0; i < parts.Length; i++)
            {
                member = activeType.GetProperties().FirstOrDefault(p => p.Name.Equals(parts[i], StringComparison.InvariantCultureIgnoreCase));
                if (member == null)
                    break;

                if (i < parts.Length - 1)
                {
                    activeObject = member.GetValue(activeObject);
                    activeType = activeObject.GetType();
                }
            }

            if (member == null)
            {
                Logger.WarnFormat("Failed to parse configuration key '{0}'", key);
                return;
            }
            destinationType = member.PropertyType;

            // convert string value to desired type
            try
            {
                var convertedValue = value.ParseAs(destinationType);

                // assign the value
                Logger.DebugFormat("Setting Config.{0} = {1}", key, convertedValue);
                member.SetValue(activeObject, convertedValue);
            }
            catch (Exception e)
            {
                Logger.WarnFormat("Failed to parse configuration value '{0}' of key '{1}': {2}", value, key, e.Message);
                return;
            }
        }

        internal static object ParseAs(this string value, Type type)
        {
            Func<string, object> conversionFn = null;

            var elementType = type.IsArray ? type.GetElementType() : type;
            // use standard converter
            var converter = TypeDescriptor.GetConverter(elementType);
            if (converter.CanConvertFrom(typeof(string)))
                conversionFn = converter.ConvertFromString;
            else
            {
                // attempt to find public static parse method
                var parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                if (parseMethod != null)
                    conversionFn = s => parseMethod.Invoke(null, new object[] { s });
            }

            if (conversionFn == null)
                throw new NotSupportedException("Unable to find appropriate conversion method for type " + type.Name);

            if (type.IsArray)
            {
                var values = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                var parsed = Array.CreateInstance(elementType, values.Length);
                for (var i = 0; i < values.Length; i++)
                    parsed.SetValue(conversionFn(values[i]), i);
                return parsed;
            }
            else
                return conversionFn(value);
        }
    }
}

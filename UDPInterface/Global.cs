using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UDPInterface
{
    internal class Global
    {
        public static string ToCsvLine<T>(string separator, T data)
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();

            return ToCsvFields(separator, fields, data);
        }

        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return csvdata.ToString();
        }

        public static string ToCsvFields(string separator, FieldInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }
    }
}

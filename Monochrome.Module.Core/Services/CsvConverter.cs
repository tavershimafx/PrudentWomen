namespace Monochrome.Module.Core.Services
{
    public static class CsvConverter
    {
        /// <summary>
        /// Converts a file stream derived from a csv file into a <see cref="List{T}"/>.
        /// The first line of the csv file must contain <typeparamref name="T"/> property names which will be used
        /// to identify the corresponding properties to assign values to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">The stream derived from reading the csv file</param>
        /// <param name="csvDelimiter">A delimiter normally a character used to seperate values of the table cells</param>
        /// <returns>A list of <typeparamref name="T"/> derived from the <paramref name="stream"/>.</returns>
        public static IEnumerable<T> ReadCsvStream<T>(Stream stream, string csvDelimiter = ",") where T : new()
        {
            var records = new List<T>();
            var reader = new StreamReader(stream);
            bool isLine0 = true;
            List<string> propertiesFromFile = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(csvDelimiter.ToCharArray());
                if (isLine0)
                {
                    propertiesFromFile.AddRange(values);
                    isLine0 = false;
                }
                else
                {
                    var item = new T();
                    var properties = item.GetType().GetProperties();
                    for (int i = 0; i < propertiesFromFile.Count; i++)
                    {
                        var prop = properties.FirstOrDefault(x => x.Name == propertiesFromFile[i].Trim());
                        if (prop != null)
                        {
                            if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType.IsValueType)
                            {
                                try
                                {
                                    var value = Convert.ChangeType(values[i], prop.PropertyType);
                                    if (prop.SetMethod != null)
                                    {
                                        prop.SetValue(item, value, null);
                                    }
                                }
                                /// <summary>
                                /// TODO:
                                /// consider catching <see cref="InvalidCastException"/> and <seealso cref="FormatException"/> and <seealso cref="IndexOutOfRangeException"/>
                                /// </summary>
                                catch
                                {
                                    if (prop.SetMethod != null)
                                    {
                                        prop.SetValue(item, default, null);
                                    }
                                }
                            }
                        }
                    }

                    records.Add(item);
                }
            }

            return records;
        }

        /// <summary>
        /// Converts a <paramref name="data"/> into a string formatted csv file. This does not include any
        /// text formatting methodologies like string bolding or italics. The property names are used as the
        /// header or the first row of the csv file by default to identify the constituting columns. The pocess
        /// skips all <see cref="IEnumerable{}"/> except strings
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">The list of items to be exported.</param>
        /// <param name="includeHeader">Should the file include the property names as the header.</param>
        /// <param name="csvDelimiter">A string usually a character used to seperate values of columns from each other.</param>
        /// <returns>A csv file as string.</returns>
        public static string ExportCsv<T>(IEnumerable<T> data, bool includeHeader = true, string csvDelimiter = ",")
        {
            var type = data.GetType();
            Type itemType;

            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
            }
            else
            {
                itemType = type.GetElementType();
            }

            var stringWriter = new StringWriter();
            if (includeHeader)
            {
                stringWriter.WriteLine(
                    string.Join<string>(
                        csvDelimiter, itemType.GetProperties()
                        .Where(x => x.PropertyType == typeof(string) || !x.PropertyType.IsEnumerable())
                        .Select(x => x.Name)
                    )
                );
            }

            foreach (var obj in data)
            {
                var props = obj.GetType().GetProperties();

                string line = string.Empty;
                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(string) || !prop.PropertyType.IsEnumerable())
                    {
                        var val = prop.GetValue(obj, null);

                        if (val != null)
                        {
                            /// <summary>
                            /// Since we don't want to write the weird <object> string in place of an Object property,
                            /// we find a 'Name' property of that object and use its value instead.
                            /// </summary>
                            if (!prop.PropertyType.IsPrimitive && prop.PropertyType != typeof(string) && !prop.PropertyType.IsValueType)
                            {
                                var n = prop.PropertyType.GetProperties().FirstOrDefault(x => x.Name == "Name");
                                if (n != null)
                                {
                                    val = n.GetValue(val, null);
                                }
                            }

                            var escapeVal = val.ToString();
                            //Check if the value contans a comma and place it in quotes if so
                            if (escapeVal.Contains(","))
                            {
                                escapeVal = string.Concat("\"", escapeVal, "\"");
                            }

                            //Replace any \r or \n special characters from a new line with a space
                            if (escapeVal.Contains("\r"))
                            {
                                escapeVal = escapeVal.Replace("\r", " ");
                            }

                            if (escapeVal.Contains("\n"))
                            {
                                escapeVal = escapeVal.Replace("\n", " ");
                            }

                            line = string.Concat(line, escapeVal, csvDelimiter);
                        }
                        else
                        {
                            line = string.Concat(line, string.Empty, csvDelimiter);
                        }
                    }
                }

                stringWriter.WriteLine(line.TrimEnd(csvDelimiter.ToCharArray()));
            }

            return stringWriter.ToString();
        }

        public static bool IsEnumerable(this Type type)
        {
            return type.GetInterface(typeof(IEnumerable<>).Name) != null;
        }

        /// <summary>
        /// Determines if the provided <paramref name="buffer"/> has the signature of a csv file.
        /// This uses the standard csv header signatures and validates the file against spoofing 
        /// where an exe file can be renamed to a .csv extension.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>A flag determining if the <paramref name="buffer"/>is a byte array arising
        /// from an actual csv file.</returns>
        public static bool IsCsvFile(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}

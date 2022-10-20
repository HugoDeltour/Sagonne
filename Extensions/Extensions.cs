using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WebMarkupMin.Core;

namespace Extensions
{
    public static class Extensions
    {
        // Regex compilée pour gain de performance
        private static readonly Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);
        private static readonly Regex _emailRegex = new Regex(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$"
            , RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string RemoveHtmlTags(this string s)
        {
            return _htmlRegex.Replace(s, string.Empty);
        }

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static string AddZero(this int i)
        {
            return i < 10 ? $"0{i}" : i.ToString();
        }

        public static string ToPhoneNumber(this string text)
        {
            string phone = Regex.Replace(text ?? "", @"[^\d]", "");
            if (phone.Length > 10 && phone.StartsWith("33"))
            {
                phone = Regex.Replace(phone, "^33", "");
                return NumberFormat(phone);
            }
            else if (phone.Length == 9 || phone.Length == 10)
            {
                return NumberFormat(phone);
            }
            return text;
        }

        private static string NumberFormat(string phone)
        {
            return Convert.ToInt64(phone).ToString(@"0#\.##\.##\.##\.##");
        }

        public static string RemoveLineReturn(this string value, string replace = "")
        {
            return value?
                .Replace(Environment.NewLine, replace)
                .Replace("\r\n", replace)
                .Replace("\n", replace)
                .Replace("\r", replace)
                .Replace(((char)0x2028).ToString(), replace) // lineSeparator
                .Replace(((char)0x2029).ToString(), replace); // paragraphSeparator
        }

        public static string Nl2br(this string value)
        {
            return value?.RemoveLineReturn("<br />");
        }

        public static string Br2nl(this string value)
        {
            return value?
                .Replace("<br />", "\r\n")
                .Replace("<br/>", "\r\n")
                .Replace("<br>", "\r\n")
                .Replace("<BR />", "\r\n")
                .Replace("<BR/>", "\r\n")
                .Replace("<BR>", "\r\n");
        }

        public static string RemoveWhitespace(this string input)
        {
            return new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        public static bool IsValidEmail(this string input)
        {
            return !string.IsNullOrEmpty(input) && _emailRegex.IsMatch(input);
        }

        public static object CheckDbNull(this object value)
        {
            return value ?? DBNull.Value;
        }

        public static bool IsDbNull(this object value)
        {
            return value == DBNull.Value;
        }

        public static string ExtraitBody(this string input)
        {
            var matches = new Regex("<body>((.|\n)*)?</body>", RegexOptions.IgnoreCase).Matches(input);
            if (matches.Any())
            {
                return matches[0].Groups[1].Value;
            }
            else
            {
                matches = new Regex("<body>((.|\n)*)?$", RegexOptions.IgnoreCase).Matches(input);
                if (matches.Any())
                    return matches[0].Groups[1].Value;
            }
            return input;
        }

        public static string ExtraitHead(this string input)
        {
            var matches = new Regex("<head>((.|\n)*)?</head>", RegexOptions.IgnoreCase).Matches(input);
            if (matches.Any())
            {
                return matches[0].Groups[1].Value;
            }
            return null;
        }

        public static int WeekOfYear(this DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
                date = date.AddDays(3);

            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static DateTime NextWeekday(this DateTime date, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)date.DayOfWeek + 7) % 7;
            return date.AddDays(daysToAdd);
        }

        public static DateTime PreviousWeekDay(this DateTime date, DayOfWeek day)
        {
            // The (... - 7) % 7 ensures we end up with a value in the range [-0, -6]
            int daysToAdd = ((int)day - (int)date.DayOfWeek - 7) % 7;
            return date.AddDays(daysToAdd);
        }

        public static string TitleCase(this string text)
        {
            return Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(text);
        }

        public static T Clone<T>(this T value)
        {
            var inst = value.GetType().GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

            return (T)inst?.Invoke(value, null);
        }

        public static void AddRange<T>(this ICollection<T> source, IEnumerable<T> collectionToAdd)
        {
            foreach (var item in collectionToAdd)
            {
                source.Add(item);
            }
        }

        public static Collection<T> ToCollection<T>(this IEnumerable<T> enumerable)
        {
            var collection = new Collection<T>();
            foreach (T i in enumerable)
                collection.Add(i);
            return collection;
        }

        public static string Minify(this string htmlInput)
        {
            var htmlMinifier = new HtmlMinifier(new HtmlMinificationSettings 
            {
                RemoveRedundantAttributes = true,
                RemoveHttpProtocolFromAttributes = true,
                RemoveHtmlComments = true,
                RemoveHtmlCommentsFromScriptsAndStyles = true,
                MinifyEmbeddedCssCode = true,
                MinifyInlineCssCode = true,
                MinifyEmbeddedJsCode = true,
                MinifyInlineJsCode = true,
                MinifyEmbeddedJsonData = true,
                WhitespaceMinificationMode = WhitespaceMinificationMode.Safe,
                PreservableAttributeList = "button[type=\"submit\"]",
            });

            var result = htmlMinifier.Minify(htmlInput);

            return result.Errors.Count == 0 ? result.MinifiedContent : htmlInput;
        }

        public static string ToEml(this MailMessage message)
        {
            var assembly = typeof(SmtpClient).Assembly;
            var mailWriterType = assembly.GetType("System.Net.Mail.MailWriter");

            using var memoryStream = new MemoryStream();

            // Get reflection info for MailWriter contructor
            var mailWriterContructor = mailWriterType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Stream) }, null);

            // Construct MailWriter object with our FileStream
            var mailWriter = mailWriterContructor.Invoke(new object[] { memoryStream });

            // Get reflection info for Send() method on MailMessage
            var sendMethod = typeof(MailMessage).GetMethod("Send", BindingFlags.Instance | BindingFlags.NonPublic);

            // Call method passing in MailWriter
            sendMethod.Invoke(message, BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { mailWriter, true, true }, null);

            // Finally get reflection info for Close() method on our MailWriter
            var closeMethod = mailWriter.GetType().GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic);

            // Call close method
            closeMethod.Invoke(mailWriter, BindingFlags.Instance | BindingFlags.NonPublic, null, Array.Empty<object>(), null);

            return Encoding.ASCII.GetString(memoryStream.ToArray());
        }

        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null)
                return false;

            return items.Contains(item);
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Mélange le contenu d'une liste de façon aléatoire
        /// </summary>
        /// <typeparam name="T">Le type d'objet cotenu dans la liste.</typeparam>
        /// <param name="list">La liste à mélanger.</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            // Source: https://stackoverflow.com/questions/273313/randomize-a-listt

            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                int n = list?.Count ?? 0;
                while (n > 1)
                {
                    byte[] box = new byte[1];
                    do provider.GetBytes(box);
                    while (!(box[0] < n * (byte.MaxValue / n)));
                    int k = box[0] % n;
                    n--;
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }


    }
}

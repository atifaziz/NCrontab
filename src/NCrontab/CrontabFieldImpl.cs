#region License, Terms and Author(s)
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace NCrontab
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using Debug = System.Diagnostics.Debug;

    #endregion

    public delegate ExceptionProvider CrontabFieldAccumulator(int start, int end, int interval, ExceptionHandler onError);

    [ Serializable ]
    public sealed class CrontabFieldImpl : IObjectReference
    {
        public static readonly CrontabFieldImpl Minute    = new CrontabFieldImpl(CrontabFieldKind.Minute, 0, 59, null);
        public static readonly CrontabFieldImpl Hour      = new CrontabFieldImpl(CrontabFieldKind.Hour, 0, 23, null);
        public static readonly CrontabFieldImpl Day       = new CrontabFieldImpl(CrontabFieldKind.Day, 1, 31, null);
        public static readonly CrontabFieldImpl Month     = new CrontabFieldImpl(CrontabFieldKind.Month, 1, 12, new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
        public static readonly CrontabFieldImpl DayOfWeek = new CrontabFieldImpl(CrontabFieldKind.DayOfWeek, 0, 6, new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" });

        private static readonly CrontabFieldImpl[] _fieldByKind = new[] { Minute, Hour, Day, Month, DayOfWeek };

        private static readonly CompareInfo _comparer = CultureInfo.InvariantCulture.CompareInfo;
        private static readonly char[] _comma = new[] { ',' };

        private readonly CrontabFieldKind _kind;
        private readonly int _minValue;
        private readonly int _maxValue;
        private readonly string[] _names;

        public static CrontabFieldImpl FromKind(CrontabFieldKind kind)
        {
            if (!Enum.IsDefined(typeof(CrontabFieldKind), kind))
            {
                throw new ArgumentException(string.Format(
                    "Invalid crontab field kind. Valid values are {0}.",
                    string.Join(", ", Enum.GetNames(typeof(CrontabFieldKind)))), "kind");
            }

            return _fieldByKind[(int) kind];
        }

        private CrontabFieldImpl(CrontabFieldKind kind, int minValue, int maxValue, string[] names)
        {
            Debug.Assert(Enum.IsDefined(typeof(CrontabFieldKind), kind));
            Debug.Assert(minValue >= 0);
            Debug.Assert(maxValue >= minValue);
            Debug.Assert(names == null || names.Length == (maxValue - minValue + 1));

            _kind = kind;
            _minValue = minValue;
            _maxValue = maxValue;
            _names = names;
        }

        public CrontabFieldKind Kind
        {
            get { return _kind; }
        }

        public int MinValue
        {
            get { return _minValue; }
        }

        public int MaxValue
        {
            get { return _maxValue; }
        }

        public int ValueCount
        {
            get { return _maxValue - _minValue + 1; }
        }

        public void Format(ICrontabField field, TextWriter writer)
        {
            Format(field, writer, false);
        }

        public void Format(ICrontabField field, TextWriter writer, bool noNames)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            if (writer == null)
                throw new ArgumentNullException("writer");

            var next = field.GetFirst();
            var count = 0;

            while (next != -1)
            {
                var first = next;
                int last;

                do
                {
                    last = next;
                    next = field.Next(last + 1);
                }
                while (next - last == 1);

                if (count == 0 
                    && first == _minValue && last == _maxValue)
                {
                    writer.Write('*');
                    return;
                }
                
                if (count > 0)
                    writer.Write(',');

                if (first == last)
                {
                    FormatValue(first, writer, noNames);
                }
                else
                {
                    FormatValue(first, writer, noNames);
                    writer.Write('-');
                    FormatValue(last, writer, noNames);
                }

                count++;
            }
        }

        private void FormatValue(int value, TextWriter writer, bool noNames)
        {
            Debug.Assert(writer != null);

            if (noNames || _names == null)
            {
                if (value >= 0 && value < 100)
                {
                    FastFormatNumericValue(value, writer);
                }
                else
                {
                    writer.Write(value.ToString(CultureInfo.InvariantCulture));
                }
            }
            else
            {
                var index = value - _minValue;
                writer.Write(_names[index]);
            }
        }

        private static void FastFormatNumericValue(int value, TextWriter writer)
        {
            Debug.Assert(value >= 0 && value < 100);
            Debug.Assert(writer != null);

            if (value >= 10)
            {
                writer.Write((char) ('0' + (value / 10)));
                writer.Write((char) ('0' + (value % 10)));
            }
            else
            {
                writer.Write((char) ('0' + value));
            }
        }

        public void Parse(string str, CrontabFieldAccumulator acc)
        {
            TryParse(str, acc, ErrorHandling.Throw);
        }

        public ExceptionProvider TryParse(string str, CrontabFieldAccumulator acc, ExceptionHandler onError)
        {
            if (acc == null)
                throw new ArgumentNullException("acc");

            if (string.IsNullOrEmpty(str))
                return null;

            try
            {
                return InternalParse(str, acc, onError);
            }
            catch (FormatException e)
            {
                return OnParseException(e, str, onError);
            }
            catch (CrontabException e)
            {
                return OnParseException(e, str, onError);
            }
        }

        private ExceptionProvider OnParseException(Exception innerException, string str, ExceptionHandler onError)
        {
            Debug.Assert(str != null);
            Debug.Assert(innerException != null);

            return ErrorHandling.OnError(
                       () => new CrontabException(string.Format("'{0}' is not a valid [{1}] crontab field expression.", str, Kind), innerException), 
                       onError);
        }

        private ExceptionProvider InternalParse(string str, CrontabFieldAccumulator acc, ExceptionHandler onError)
        {
            Debug.Assert(str != null);
            Debug.Assert(acc != null);

            if (str.Length == 0)
                return ErrorHandling.OnError(() => new CrontabException("A crontab field value cannot be empty."), onError);

            //
            // Next, look for a list of values (e.g. 1,2,3).
            //
    
            var commaIndex = str.IndexOf(",");

            if (commaIndex > 0)
            {
                ExceptionProvider e = null;
                var token = ((IEnumerable<string>) str.Split(_comma)).GetEnumerator();
                while (token.MoveNext() && e == null)
                    e = InternalParse(token.Current, acc, onError);
                return e;
            }
            
            var every = 1;

            //
            // Look for stepping first (e.g. */2 = every 2nd).
            // 

            var slashIndex = str.IndexOf("/");

            if (slashIndex > 0)
            {
                every = int.Parse(str.Substring(slashIndex + 1), CultureInfo.InvariantCulture);
                str = str.Substring(0, slashIndex);
            }

            //
            // Next, look for wildcard (*).
            //
    
            if (str.Length == 1 && str[0]== '*')
            {
                return acc(-1, -1, every, onError);
            }

            //
            // Next, look for a range of values (e.g. 2-10).
            //

            var dashIndex = str.IndexOf("-");
        
            if (dashIndex > 0)
            {
                var first = ParseValue(str.Substring(0, dashIndex));
                var last = ParseValue(str.Substring(dashIndex + 1));

                return acc(first, last, every, onError);
            }

            //
            // Finally, handle the case where there is only one number.
            //

            var value = ParseValue(str);

            if (every == 1)
                return acc(value, value, 1, onError);

            Debug.Assert(every != 0);
            return acc(value, _maxValue, every, onError);
        }

        private int ParseValue(string str)
        {
            Debug.Assert(str != null);

            if (str.Length == 0)
                throw new CrontabException("A crontab field value cannot be empty.");

            var firstChar = str[0];
        
            if (firstChar >= '0' && firstChar <= '9')
                return int.Parse(str, CultureInfo.InvariantCulture);

            if (_names == null)
            {
                throw new CrontabException(string.Format(
                    "'{0}' is not a valid [{3}] crontab field value. It must be a numeric value between {1} and {2} (all inclusive).",
                    str, _minValue.ToString(), _maxValue.ToString(), _kind.ToString()));
            }

            for (var i = 0; i < _names.Length; i++)
            {
                if (_comparer.IsPrefix(_names[i], str, CompareOptions.IgnoreCase))
                    return i + _minValue;
            }

            throw new CrontabException(string.Format(
                "'{0}' is not a known value name. Use one of the following: {1}.", 
                str, string.Join(", ", _names)));
        }

        object IObjectReference.GetRealObject(StreamingContext context)
        {
            return FromKind(Kind);
        }
    }
}

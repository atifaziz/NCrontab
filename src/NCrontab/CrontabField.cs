#region License, Terms and Author(s)
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
//
//  Author(s):
//
//      Atif Aziz, http://www.raboof.com
//
// This library is free software; you can redistribute it and/or modify it 
// under the terms of the New BSD License, a copy of which should have 
// been delivered along with this distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS 
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
#endregion

namespace NCrontab
{
    #region Imports

    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;

    #endregion

    /// <summary>
    /// Represents a single crontab field.
    /// </summary>

    [ Serializable ]
    public sealed class CrontabField : ICrontabField
    {
        private readonly BitArray _bits;
        private /* readonly */ int _minValueSet;
        private /* readonly */ int _maxValueSet;
        private readonly CrontabFieldImpl _impl;

        /// <summary>
        /// Parses a crontab field expression given its kind.
        /// </summary>

        public static CrontabField Parse(CrontabFieldKind kind, string expression)
        {
            return TryParse(kind, expression, ErrorHandling.Throw).Value;
        }

        public static ValueOrError<CrontabField> TryParse(CrontabFieldKind kind, string expression)
        {
            return TryParse(kind, expression, null);
        }

        public static ValueOrError<CrontabField> TryParse(CrontabFieldKind kind, string expression, ExceptionHandler onError)
        {
            var field = new CrontabField(CrontabFieldImpl.FromKind(kind));
            return ValueOrError.Select(field, field._impl.TryParse(expression, field.Accumulate, onError));
        }

        /// <summary>
        /// Parses a crontab field expression representing minutes.
        /// </summary>

        public static CrontabField Minutes(string expression)
        {
            return Parse(CrontabFieldKind.Minute, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing hours.
        /// </summary>

        public static CrontabField Hours(string expression)
        {
            return Parse(CrontabFieldKind.Hour, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing days in any given month.
        /// </summary>
        
        public static CrontabField Days(string expression)
        {
            return Parse(CrontabFieldKind.Day, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing months.
        /// </summary>

        public static CrontabField Months(string expression)
        {
            return Parse(CrontabFieldKind.Month, expression);
        }

        /// <summary>
        /// Parses a crontab field expression representing days of a week.
        /// </summary>

        public static CrontabField DaysOfWeek(string expression)
        {
            return Parse(CrontabFieldKind.DayOfWeek, expression);
        }

        private CrontabField(CrontabFieldImpl impl)
        {
            if (impl == null)
                throw new ArgumentNullException("impl");

            _impl = impl;
            _bits = new BitArray(impl.ValueCount);

            _bits.SetAll(false);
            _minValueSet = int.MaxValue;
            _maxValueSet = -1;
        }

        /// <summary>
        /// Gets the first value of the field or -1.
        /// </summary>

        public int GetFirst()
        {
            return _minValueSet < int.MaxValue ? _minValueSet : -1;
        }

        /// <summary>
        /// Gets the next value of the field that occurs after the given 
        /// start value or -1 if there is no next value available.
        /// </summary>

        public int Next(int start)
        {
            if (start < _minValueSet)
                return _minValueSet;

            var startIndex = ValueToIndex(start);
            var lastIndex = ValueToIndex(_maxValueSet);

            for (var i = startIndex; i <= lastIndex; i++)
            {
                if (_bits[i]) 
                    return IndexToValue(i);
            }

            return -1;
        }

        private int IndexToValue(int index)
        {
            return index + _impl.MinValue;
        }

        private int ValueToIndex(int value)
        {
            return value - _impl.MinValue;
        }

        /// <summary>
        /// Determines if the given value occurs in the field.
        /// </summary>

        public bool Contains(int value)
        {
            return _bits[ValueToIndex(value)];
        }

        /// <summary>
        /// Accumulates the given range (start to end) and interval of values
        /// into the current set of the field.
        /// </summary>
        /// <remarks>
        /// To set the entire range of values representable by the field,
        /// set <param name="start" /> and <param name="end" /> to -1 and
        /// <param name="interval" /> to 1.
        /// </remarks>

        private ExceptionProvider Accumulate(int start, int end, int interval, ExceptionHandler onError)
        {
            var minValue = _impl.MinValue;
            var maxValue = _impl.MaxValue;

            if (start == end) 
            {
                if (start < 0) 
                {
                    //
                    // We're setting the entire range of values.
                    //

                    if (interval <= 1) 
                    {
                        _minValueSet = minValue;
                        _maxValueSet = maxValue;
                        _bits.SetAll(true);
                        return null;
                    }

                    start = minValue;
                    end = maxValue;
                } 
                else 
                {
                    //
                    // We're only setting a single value - check that it is in range.
                    //

                    if (start < minValue)
                        return OnValueBelowMinError(start, onError);

                    if (start > maxValue)
                        return OnValueAboveMaxError(start, onError);
                }
            } 
            else 
            {
                //
                // For ranges, if the start is bigger than the end value then
                // swap them over.
                //

                if (start > end) 
                {
                    end ^= start;
                    start ^= end;
                    end ^= start;
                }

                if (start < 0) 
                    start = minValue;
                else if (start < minValue) 
                    return OnValueBelowMinError(start, onError);                    

                if (end < 0) 
                    end = maxValue;
                else if (end > maxValue) 
                    return OnValueAboveMaxError(end, onError);
            }

            if (interval < 1) 
                interval = 1;

            int i;

            //
            // Populate the _bits table by setting all the bits corresponding to
            // the valid field values.
            //

            for (i = start - minValue; i <= (end - minValue); i += interval) 
                _bits[i] = true;

            //
            // Make sure we remember the minimum value set so far Keep track of
            // the highest and lowest values that have been added to this field
            // so far.
            //

            if (_minValueSet > start) 
                _minValueSet = start;

            i += (minValue - interval);

            if (_maxValueSet < i) 
                _maxValueSet = i;

            return null;
        }

        private ExceptionProvider OnValueAboveMaxError(int value, ExceptionHandler onError)
        {
            return ErrorHandling.OnError(
                () => new CrontabException(string.Format(
                    "{0} is higher than the maximum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                    value, _impl.MinValue, _impl.MaxValue)), 
                onError);
        }

        private ExceptionProvider OnValueBelowMinError(int value, ExceptionHandler onError)
        {
            return ErrorHandling.OnError(
                () => new CrontabException(string.Format(
                    "{0} is lower than the minimum allowable value for this field. Value must be between {1} and {2} (all inclusive).", 
                    value, _impl.MinValue, _impl.MaxValue)), 
                onError);
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(string format)
        {
            var writer = new StringWriter(CultureInfo.InvariantCulture);

            switch (format)
            {
                case "G":
                case null:
                    Format(writer, true);
                    break;
                case "N":
                    Format(writer);
                    break;
                default:
                    throw new FormatException();
            }

            return writer.ToString();
        }

        public void Format(TextWriter writer)
        {
            Format(writer, false);
        }

        public void Format(TextWriter writer, bool noNames)
        {
            _impl.Format(this, writer, noNames);
        }
    }
}

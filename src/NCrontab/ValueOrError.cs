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
    using System;

    public static class ValueOrError
    {
        public static ValueOrError<T> Value<T>(T value) { return new ValueOrError<T>(value); }
        public static ValueOrError<T> Error<T>(ExceptionProvider ep) { return new ValueOrError<T>(ep); }
        public static ValueOrError<T> Error<T>(Exception error) { return new ValueOrError<T>(() => error); }

        public static ValueOrError<T> Select<T>(T value, ExceptionProvider ep)
        {
            return ep != null ? Error<T>(ep) : Value(value);
        }

        public static ValueOrError<T> Select<T>(T value, Exception error)
        {
            return error != null ? Error<T>(error) : Value(value);
        }
    }

    [ Serializable ]
    public struct ValueOrError<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;
        private readonly ExceptionProvider _ep;

        private static readonly ExceptionProvider _dep = () => new Exception("Value is undefined.");

        public ValueOrError(T value) : this()
        {
            _hasValue = true;
            _value = value;
        }

        public ValueOrError(Exception error) : this(CheckError(error)) {}

        private static ExceptionProvider CheckError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");
            return () => error;
        }

        public ValueOrError(ExceptionProvider provider) : this()
        {
            if (provider == null) throw new ArgumentNullException("provider");
            _ep = provider;
        }

        public bool HasValue { get { return _hasValue; } }
        public T Value { get { if (!HasValue) throw ErrorProvider(); return _value; } }
        public bool IsError { get { return ErrorProvider != null; } }
        public Exception Error { get { return IsError ? ErrorProvider() : null; } }
        public ExceptionProvider ErrorProvider { get { return HasValue ? null : _ep ?? _dep; } }

        public override string ToString()
        {
            var error = Error;
            return IsError 
                 ? error.GetType().FullName + ": " + error.Message 
                 : _value != null 
                 ? _value.ToString() : string.Empty;
        }
    }
}
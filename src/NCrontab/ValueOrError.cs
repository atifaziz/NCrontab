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
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

namespace NCrontab.Tests
{
    #region Imports

    using System;
    using NUnit.Framework;
    using NUnit.Framework.SyntaxHelpers;

    #endregion

    [TestFixture]
    public sealed class ValueOrErrorTests
    {
        [Test]
        public void StringValue()
        {
            var str = ValueOrError.Value("hello");
            Assert.That(str.HasValue, Is.True);
            Assert.That(str.Value, Is.EqualTo("hello"));
            Assert.That(str.IsError, Is.False);
            Assert.That(str.Error, Is.Null);
            Assert.That(str.ToString(), Is.EqualTo("hello"));
        }

        [Test]
        public void ToStringForNullStringValue()
        {
            Assert.That(ValueOrError.Value((string)null).ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ErrorValue()
        {
            var str = ValueOrError.Error<string>(new Exception());
            Assert.That(str.HasValue, Is.False);
            Assert.That(str.ErrorProvider, Is.Not.Null);
            var error = str.Error;
            Assert.That(error, Is.InstanceOfType(typeof(Exception)));
            Assert.That(str.ToString().IndexOf(error.Message), Is.Not.EqualTo(-1));
        }

        [Test]
        public void DefaultConstructionHasNoValue()
        {
            Assert.That(new ValueOrError<object>().HasValue, Is.False);
        }

        [Test, ExpectedException(typeof(Exception))]
        public void DefaultConstructionValueThrows()
        {
            var unused = new ValueOrError<object>().Value;
        }

        [Test]
        public void DefaultConstructionIsInError()
        {
            Assert.That(new ValueOrError<object>().IsError, Is.True);
        }

        [Test]
        public void TypeOfDefaultConstructionError()
        {
            Assert.That(new ValueOrError<object>().Error, Is.InstanceOfType(typeof(Exception)));
        }
    }
}

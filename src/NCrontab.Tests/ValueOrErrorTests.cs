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
            var str = new ValueOrError<string>("hello");
            Assert.That(str.HasValue, Is.True);
            Assert.That(str.Value, Is.EqualTo("hello"));
            Assert.That(str.IsError, Is.False);
            Assert.That(str.Error, Is.Null);
            Assert.That(str.ToString(), Is.EqualTo("hello"));
        }

        [Test]
        public void ToStringForNullStringValue()
        {
            Assert.That(new ValueOrError<string>((string) null).ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ErrorValue()
        {
            var str = new ValueOrError<string>(new Exception());
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

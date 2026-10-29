#region License and Terms
//
// NCrontab - Crontab for .NET
// Copyright (c) 2008 Atif Aziz. All rights reserved.
// Portions Copyright (c) 2001 The OpenSymphony Group. All rights reserved.
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

using System;
using NUnit.Framework;

namespace NCrontab.Tests;

[TestFixture]
public sealed class CrontabFieldTests
{
    [Test]
    public void ParseRejectsNullExpression()
    {
        Assert.That(() => CrontabField.Parse(CrontabFieldKind.Minute, null!),
                    Throws.ArgumentNullException
                          .With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("expression"));
    }

    [Test]
    public void TryParseReturnsNullForNullExpression() =>
        Assert.That(CrontabField.TryParse(CrontabFieldKind.Minute, null!), Is.Null);

    [Test]
    public void ParseRejectsEmptyExpression() =>
        Assert.That(() => CrontabField.Parse(CrontabFieldKind.Minute, string.Empty),
                    Throws.TypeOf<CrontabException>());

    [Test]
    public void TryParseReturnsNullForEmptyExpression() =>
        Assert.That(CrontabField.TryParse(CrontabFieldKind.Minute, string.Empty), Is.Null);
}

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

namespace NCrontab
{
    public interface ICrontabField
    {
        int GetFirst();
#pragma warning disable CA1716 // Identifiers should not match keywords (by design)
        int Next(int start);
#pragma warning restore CA1716 // Identifiers should not match keywords
        int GetLast();
#pragma warning disable CA1716 // Identifiers should not match keywords (by design)
        int Prev(int start);
#pragma warning restore CA1716 // Identifiers should not match keywords
        bool Contains(int value);
    }
}

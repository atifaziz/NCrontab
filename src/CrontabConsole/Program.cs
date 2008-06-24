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

namespace NCrontabConsole
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using NCrontab;

    #endregion
    
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                    throw new ApplicationException("Missing required arguments. You must at least supply CRONTAB-EXPRESSION START-DATE END-DATE.");

                var expression = args[0];
                var start = ParseDateArgument(args[1], "start");
                var end = ParseDateArgument(args[2], "end");
                var format = args.Length == 4 ? args[3] : "f";

                WriteOccurrences(CrontabSchedule.Parse(expression), start, end, format, Console.Out);

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Trace.WriteLine(e.ToString());
                return 1;
            }  
        }

        private static DateTime ParseDateArgument(string arg, string hint)
        {
            try
            {
                return DateTime.Parse(arg, null, DateTimeStyles.AssumeLocal);
            }
            catch (FormatException e)
            {
                throw new ApplicationException("Invalid " + hint + " date or date format argument.", e);
            }
        }

        private static void WriteOccurrences(CrontabSchedule schedule, DateTime start, DateTime end, string format, TextWriter output) 
        {
            var occurrence = schedule.GetNextOccurrences(start, end).GetEnumerator();
            while (occurrence.MoveNext())
                output.WriteLine(occurrence.Current.ToString(format));
        }
    }
}

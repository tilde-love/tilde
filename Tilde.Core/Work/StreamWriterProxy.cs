// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Tilde.Core.Work
{
    /// <summary>
    /// Proxy class for StreamWriter. Find a better way to to do this. 
    /// </summary>
    public class WrapperStreamWriter
    {
        public StreamWriter Wrapped { get; set; } 

        public bool AutoFlush
        {
            get => Wrapped.AutoFlush;
            set => Wrapped.AutoFlush = value;
        }
        
        public Stream BaseStream => Wrapped.BaseStream;
        
        public Encoding Encoding => Wrapped.Encoding;
        
        public IFormatProvider FormatProvider => Wrapped.FormatProvider;
        
        public string NewLine
        {
            get => Wrapped.NewLine;
            set => Wrapped.NewLine = value;
        }

        public void Close()
        {
            Wrapped.Close();
        }
        
        public void Dispose()
        {
            Wrapped.Dispose();
        }

        public void Flush()
        {
            Wrapped.Flush();
        }
        
        public async Task FlushAsync()
        {
            await Wrapped.FlushAsync();
        }
        
        public object GetLifetimeService()
        {
            return Wrapped.GetLifetimeService();
        }
        
        public object InitializeLifetimeService()
        {
            return Wrapped.InitializeLifetimeService();
        }
        
        public void Write(char value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(char[] buffer)
        {
            Wrapped.Write(buffer);
        }

        public void Write(char[] buffer, int index, int count)
        {
            Wrapped.Write(buffer, index, count);
        }
        
        public void Write(string value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(bool value)
        {
            Wrapped.Write(value);
        }

        public void Write(decimal value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(double value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(int value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(long value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(object value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(float value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(string format, object arg0)
        {
            Wrapped.Write(format, arg0);
        }
        
        public void Write(string format, object arg0, object arg1)
        {
            Wrapped.Write(format, arg0, arg1);
        }
        
        public void Write(
            string format,
            object arg0,
            object arg1,
            object arg2)
        {
            Wrapped.Write(format, arg0, arg1, arg2);
        }
        
        public void Write(string format, params object[] arg)
        {
            Wrapped.Write(format, arg);
        }
        
        public void Write(uint value)
        {
            Wrapped.Write(value);
        }
        
        public void Write(ulong value)
        {
            Wrapped.Write(value);
        }
        
        public async Task WriteAsync(char value)
        {
            await Wrapped.WriteAsync(value);
        }
        
        public async Task WriteAsync(char[] buffer, int index, int count)
        {
            await Wrapped.WriteAsync(buffer, index, count);
        }
        
        public async Task WriteAsync(string value)
        {
            await Wrapped.WriteAsync(value);
        }
        
        public async Task WriteAsync(char[] buffer)
        {
            await Wrapped.WriteAsync(buffer);
        }
        
        public void WriteLine()
        {
            Wrapped.WriteLine();
        }

        public void WriteLine(bool value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(char value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(char[] buffer)
        {
            Wrapped.WriteLine(buffer);
        }
        
        public void WriteLine(char[] buffer, int index, int count)
        {
            Wrapped.WriteLine(buffer, index, count);
        }
        
        public void WriteLine(decimal value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(double value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(int value)
        {
            Wrapped.WriteLine(value);
        }

        public void WriteLine(long value)
        {
            Wrapped.WriteLine(value);
        }

        public void WriteLine(object value)
        {
            Wrapped.WriteLine(value);
        }

        public void WriteLine(float value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(string value)
        {
            Wrapped.WriteLine(value);
        }

        public void WriteLine(string format, object arg0)
        {
            Wrapped.WriteLine(format, arg0);
        }
        
        public void WriteLine(string format, object arg0, object arg1)
        {
            Wrapped.WriteLine(format, arg0, arg1);
        }
        
        public void WriteLine(
            string format,
            object arg0,
            object arg1,
            object arg2)
        {
            Wrapped.WriteLine(format, arg0, arg1, arg2);
        }

        public void WriteLine(string format, params object[] arg)
        {
            Wrapped.WriteLine(format, arg);
        }
        
        public void WriteLine(uint value)
        {
            Wrapped.WriteLine(value);
        }
        
        public void WriteLine(ulong value)
        {
            Wrapped.WriteLine(value);
        }

        public async Task WriteLineAsync()
        {
            await Wrapped.WriteLineAsync();
        }

        public async Task WriteLineAsync(char value)
        {
            await Wrapped.WriteLineAsync(value);
        }
        
        public async Task WriteLineAsync(char[] buffer, int index, int count)
        {
            await Wrapped.WriteLineAsync(buffer, index, count);
        }
        
        public async Task WriteLineAsync(string value)
        {
            await Wrapped.WriteLineAsync(value);
        }
        
        public async Task WriteLineAsync(char[] buffer)
        {
            await Wrapped.WriteLineAsync(buffer);
        }
    }
}
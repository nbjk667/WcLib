/*

	Copyright (c) 2015, Robert R. Khalikov

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.
 
*/

namespace WcLib
{
    public class LogManager
    {
        public virtual void WriteLine( string category, string format, params object[] args )
        {
            if ( !IsEnabled( category ) )
                return;

            string formattedLine = PreWriteLine( category, System.String.Format( format, args ) );

            System.IO.StreamWriter writer = GetWriter( category );

            if ( writer != null )
            {
                writer.WriteLine( formattedLine );
            }

            PostWriteLine( category, formattedLine );
        }

        protected virtual bool IsEnabled( string category ) { return true; }
        protected virtual string PreWriteLine( string category, string line ) { return line; }
        protected virtual void PostWriteLine( string category, string line ) { System.Console.WriteLine( line ); }
        protected virtual System.IO.StreamWriter GetWriter( string category ) { return null; }
    }

    public static class Log
    {
        public static string DefaultCategory = "Default";

        public static void SetManager( LogManager logManager )
        {
            m_logManager = logManager == null ? new LogManager() : logManager;
        }

        public static void WriteLine( string category, string format, params object[] args )
        {
            m_logManager.WriteLine( category, format, args );
        }

        public static void WriteLine( string format, params object[] args )
        {
            m_logManager.WriteLine( DefaultCategory, format, args );
        }

        static LogManager m_logManager = new LogManager();
    }
}


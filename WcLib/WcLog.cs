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
        public virtual string GetFileName( string category ) { return category + ".log"; }
        public virtual bool UsesConsoleOutput( string category ) { return true; }
        public virtual string ModifyLine( string categiory, string line ) { return line; }
    }

    public static class Log
    {
        public static void SetLogManager( LogManager logManager )
        {
            m_logManager = logManager == null ? new LogManager() : logManager;
        }

        public static void WriteLine( string category, string format, params object[] args )
        {
            string formattedLine = m_logManager.ModifyLine( category, string.Format( format, args ) );
            if ( formattedLine == null )
                return;

            System.IO.StreamWriter writer = GetWriter( m_logManager.GetFileName( category ) );
            
            if ( writer != null )
            {
                writer.WriteLine( formattedLine );
            }

            if ( m_logManager.UsesConsoleOutput( category ) )
            {
                System.Console.WriteLine( formattedLine );
            }
        }

        public static void CloseAll()
        {
            foreach ( var entry in m_logFiles )
            {
                entry.writer.Close();
            }

            m_logFiles.Clear();
        }

        static System.IO.StreamWriter GetWriter( string fileName )
        {
            if ( fileName == null )
            {
                return null;
            }

            System.IO.StreamWriter writer = null;

            foreach ( var entry in m_logFiles )
            {
                if ( entry.fileName == fileName )
                {
                    writer = entry.writer;
                    break;
                }
            }

            if ( writer == null )
            {
                var newEntry = new FileEntry();
                newEntry.fileName = fileName;
                newEntry.writer = new System.IO.StreamWriter( fileName );
                m_logFiles.Add( newEntry );
                writer = newEntry.writer;
            }

            return writer;
        }

        struct FileEntry { public string fileName; public System.IO.StreamWriter writer; }
        static System.Collections.Generic.List< FileEntry > m_logFiles = new System.Collections.Generic.List< FileEntry > ();
        static LogManager m_logManager = new LogManager();
    }
}


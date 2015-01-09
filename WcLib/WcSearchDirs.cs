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
    static class SearchDirs
    {
        public static string GetFullPath( string relativePath, string includePath = null )
        {
            if ( includePath != null )
            {
                string fullPath = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( includePath ), relativePath );
                if ( System.IO.File.Exists( fullPath ) )
                {
                    return fullPath;
                }
            }

            TryLoadSearchDirs();

            foreach ( string dir in m_searchDirs )
            {
                string fullPath = System.IO.Path.Combine( dir, relativePath );
                if ( System.IO.File.Exists( fullPath ) )
                {
                    return fullPath;
                }
            }

            return null;
        }

        static void TryLoadSearchDirs()
        {
            if ( !m_searchDirLoadAttempted )
            {
                m_searchDirLoadAttempted = true;

                string exeDirectory = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );
                string searchDirsFile = System.IO.Path.ChangeExtension( System.Reflection.Assembly.GetEntryAssembly().Location, "wcsearchdirs" );
                if ( System.IO.File.Exists( searchDirsFile ) )
                {
                    var reader = new System.IO.StreamReader( searchDirsFile );
                    string line = null;
                    while ( (line = reader.ReadLine()) != null )
                    {
                        string trimmedLine = line.Trim();
                        if ( trimmedLine.Length > 0 )
                        {
                            m_searchDirs.Add( System.IO.Path.Combine( exeDirectory, trimmedLine ) );
                        }
                    }
                }
            }
        }

        static System.Collections.Generic.List< string > m_searchDirs = new System.Collections.Generic.List< string > ();
        static bool m_searchDirLoadAttempted = false;
    }
}


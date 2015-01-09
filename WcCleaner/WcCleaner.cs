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

class MyFileFilter : WcLib.FileFilter
{
	public void Run( string directory, WcLib.PatternList patternList, bool logOnly, string logName = null )
	{
        string logFileName = "WcCleaner.log";
        string errorLogFileName = "WcCleaner-Errors.log";
        if ( logName != null )
        {
            logFileName = "WcCleaner-" + logName + ".log";
            errorLogFileName = "WcCleaner-" + logName + "-Errors.log";
        }

		m_logWriter = new System.IO.StreamWriter( logFileName );
		m_logWriterErrors = new System.IO.StreamWriter( errorLogFileName );
		m_logOnly = logOnly;

		Run( directory, patternList );	
	
		m_logWriter.Close();
		m_logWriterErrors.Close();
	}

	protected override void ProcessFile( string file, WcLib.FileState fileState )
	{
		if ( fileState == WcLib.FileState.Selected )
		{
			m_logWriter.WriteLine( file );
			System.Console.WriteLine( file );

			if ( !m_logOnly )
			{
				try
				{
					System.IO.File.Delete( file );
				}
				catch ( System.Exception e )
				{
					m_logWriterErrors.WriteLine( e );
					System.Console.WriteLine( e );
				}
			}
		}
	}

	protected override void LeaveDirectory( string directory )
	{
		bool isEmpty = true;

		foreach ( string s in System.IO.Directory.GetFileSystemEntries( directory ) )
		{
			isEmpty = false;
			break;
		}

		if ( isEmpty )
		{
			m_logWriter.WriteLine( directory );
			System.Console.WriteLine( directory );

			if ( !m_logOnly )
			{
				System.IO.Directory.Delete( directory );
			}
		}
	}

	System.IO.StreamWriter m_logWriter;
	System.IO.StreamWriter m_logWriterErrors;
	bool m_logOnly;
}

static class Program
{
	static void Main( string[] args )
	{
		string argInputDir = "";
		string argPatternFile = "";
		bool argLogOnly = false;
		bool argAuto = false;
        bool argNamedLog = false;
		System.Collections.Generic.List< string > argDefs = new System.Collections.Generic.List< string > ();

		foreach ( string arg in args )
		{
			if ( arg.StartsWith( "-dir=" ) )
			{
				argInputDir = System.IO.Path.GetFullPath( arg.Substring( 5 ) );
			}
			else if ( arg.StartsWith( "-pattern=" ) )
			{
				argPatternFile = System.IO.Path.GetFullPath( arg.Substring( 9 ) );
			}
			else if ( arg.StartsWith( "-logonly" ) )
			{
				argLogOnly = true;
			}
			else if ( arg.StartsWith( "-auto" ) )
			{
				argAuto = true;
			}
            else if ( arg.StartsWith( "-named" ) )
            {
                argNamedLog = true;
            }
			else if ( arg.StartsWith( "-def=" ) )
			{
				string[] defs = arg.Substring( 5 ).Split( new char[] { '+' }, System.StringSplitOptions.RemoveEmptyEntries );
				if ( defs != null )
				{
					argDefs.AddRange( defs );
				}
			}
		}

		if ( argInputDir.Length < 1 )
		{
			System.Console.WriteLine( "ERROR: Input directory is not specified!" );
			return;
		}

		if ( !System.IO.Directory.Exists( argInputDir ) )
		{
			System.Console.WriteLine( "ERROR: Input directory '{0}' doesn't exist!", argInputDir );
			return;
		}

		if ( !System.IO.File.Exists( argPatternFile ) )
		{
			System.Console.WriteLine( "ERROR: Pattern file '{0}' doesn't exist!", argPatternFile );
			return;
		}

		if ( !argLogOnly && !argAuto )
		{
			System.Console.WriteLine( "WARNING: Do you confirm DELETION of files in '{0}' based on patterns in '{1}'? (Press Y or N)", argInputDir, argPatternFile );
			while ( true )
			{
				System.ConsoleKey pressedKey = System.Console.ReadKey( true ).Key;

				if ( pressedKey == System.ConsoleKey.N )
				{
					System.Console.WriteLine( "Operation cancelled." );
					return;
				}
				
				if ( pressedKey == System.ConsoleKey.Y )
				{
					System.Console.WriteLine( "Running..." );
					break;
				}
			}
		}

        string logName = null;

        if ( argNamedLog )
        {
            logName = System.IO.Path.GetFileNameWithoutExtension( argPatternFile );
        }

		new MyFileFilter().Run( argInputDir, new WcLib.PatternList( argPatternFile, argDefs.ToArray() ), argLogOnly, logName );
	}
}


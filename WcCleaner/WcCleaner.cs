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
	public void Run( string directory, WcLib.PatternList patternList, bool logOnly, string logCategory = "" )
	{
		m_logOnly = logOnly;
        m_logCategory = "WcCleaner-" + logCategory;
		Run( directory, patternList );
	}

	protected override void ProcessFile( string file, WcLib.FileState fileState )
	{
		if ( fileState == WcLib.FileState.Selected )
		{
			WcLib.Log.WriteLine( m_logCategory, file );

			if ( !m_logOnly )
			{
				try
				{
					System.IO.File.Delete( file );
				}
				catch ( System.Exception e )
				{
					WcLib.Log.WriteLine( m_logCategory + "-Errors", e.ToString() );
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
			WcLib.Log.WriteLine( m_logCategory, directory );

			if ( !m_logOnly )
			{
				try
				{
					System.IO.Directory.Delete( directory );
				}
				catch ( System.Exception e )
				{
					WcLib.Log.WriteLine( m_logCategory + "-Errors", e.ToString() );
				}
			}
		}
	}

	bool m_logOnly;
    string m_logCategory;
}

class MyLogManager : WcLib.LogManager, System.IDisposable
{
    protected override System.IO.StreamWriter GetWriter( string category )
    {
        if ( !m_writers.ContainsKey( category ) )
        {
            m_writers.Add( category, new System.IO.StreamWriter( category + ".log" ) );
        }

        return m_writers[ category ];
    }

    public void Dispose()
    {
        foreach ( var writer in m_writers.Values )
        {
            writer.Close();
            writer.Dispose();
        }

        m_writers.Clear();
    }

    System.Collections.Generic.Dictionary< string, System.IO.StreamWriter > m_writers = new System.Collections.Generic.Dictionary< string, System.IO.StreamWriter > ();
}

static class Program
{
    static string errorLogCategory = "WcCleaner-Errors";

	static void Main( string[] args )
	{
        using ( var logManager = new MyLogManager() )
        {
            WcLib.Log.SetManager( logManager );

            try
            {
                RunCleaner( args );
            }
            catch ( System.Exception e )
            {
                WcLib.Log.WriteLine( errorLogCategory, e.ToString() );
            }
        }
	}

    static void RunCleaner( string[] args )
    {
		string argInputDir = "";
		string argPatternFile = "";
        string argCategory = "";
		bool argLogOnly = false;
		bool argAuto = false;
		System.Collections.Generic.List< string > argDefs = new System.Collections.Generic.List< string > ();

		foreach ( string arg in args )
		{
			if ( arg.StartsWith( "-dir=" ) ) argInputDir = System.IO.Path.GetFullPath( arg.Substring( 5 ) );
			else if ( arg.StartsWith( "-pattern=" ) ) argPatternFile = System.IO.Path.GetFullPath( arg.Substring( 9 ) );
			else if ( arg.StartsWith( "-logonly" ) ) argLogOnly = true;
			else if ( arg.StartsWith( "-auto" ) ) argAuto = true;
            else if ( arg.StartsWith( "-category=" ) ) argCategory = arg.Substring( 10 );
			else if ( arg.StartsWith( "-def=" ) )
			{
				string[] defs = arg.Substring( 5 ).Split( new char[] { '+' }, System.StringSplitOptions.RemoveEmptyEntries );

				if ( defs != null )
				{
					argDefs.AddRange( defs );
				}
			}
		}

        errorLogCategory = "WcCleaner-" + argCategory + "-Errors";

		if ( !ValidateInput( argInputDir, argPatternFile ) )
            return;

		if ( !argLogOnly && !argAuto )
		{
            if ( !ConfirmFileDeletion( argInputDir, argPatternFile ) )
                return;
		}

		new MyFileFilter().Run( argInputDir, new WcLib.PatternList( argPatternFile, argDefs.ToArray() ), argLogOnly, argCategory );
    }

    static bool ValidateInput( string inputDir, string patternFile )
    {
        if ( inputDir.Length < 1 )
        {
            System.Console.WriteLine( "Input directory is not specified!" );
            return false;
        }

		if ( !System.IO.Directory.Exists( inputDir ) )
		{
			System.Console.WriteLine( "ERROR: Input directory '{0}' doesn't exist!", inputDir );
			return false;
		}

		if ( !System.IO.File.Exists( patternFile ) )
		{
			System.Console.WriteLine( "ERROR: Pattern file '{0}' doesn't exist!", patternFile );
			return false;
		}

        return true;
    }

    static bool ConfirmFileDeletion( string inputDir, string patternFile )
    {
		System.Console.WriteLine( "WARNING: Do you confirm DELETION of files in '{0}' based on patterns in '{1}'? (Press Y or N)", inputDir, patternFile );

		while ( true )
		{
			System.ConsoleKey pressedKey = System.Console.ReadKey( true ).Key;

			if ( pressedKey == System.ConsoleKey.N )
			{
				System.Console.WriteLine( "Operation cancelled." );
				return false;
			}
				
			if ( pressedKey == System.ConsoleKey.Y )
			{
				System.Console.WriteLine( "Running..." );
				return true;
			}
		}
    }
}


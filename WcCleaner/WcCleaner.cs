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
    public enum Mode
    {
        Delete, Move, Copy, Skip
    }

	public void Run( string inputDir, string outputDir, WcLib.PatternList patternList, Mode mode, string logCategory = "" )
	{
		m_mode = mode;
        m_logCategory = logCategory.Length > 0 ? "WcCleaner-" + logCategory : "WcCleaner";
        m_inputDir = inputDir;
        m_outputDir = outputDir;
        Run( inputDir, patternList );
	}

	protected override void ProcessFile( string file, WcLib.FileState fileState )
	{
		if ( fileState == WcLib.FileState.Selected )
		{
			WcLib.Log.WriteLine( m_logCategory, file );

			if ( m_mode != Mode.Skip )
			{
                string outputFileName = null;

                if ( m_mode == Mode.Move || m_mode == Mode.Copy )
                {
                    string relativeFileName = file.Substring( m_inputDir.Length );
                    if ( relativeFileName.StartsWith( new string( System.IO.Path.DirectorySeparatorChar, 1 ) ) ||
                         relativeFileName.StartsWith( new string( System.IO.Path.AltDirectorySeparatorChar, 1 ) ) )
                    {
                        relativeFileName = relativeFileName.Substring( 1 );
                    }
                    
                    outputFileName = System.IO.Path.Combine( m_outputDir, relativeFileName );
                    string outputDirectory = System.IO.Path.GetDirectoryName( outputFileName );
                    if ( !System.IO.Directory.Exists( outputDirectory ) )
                    {
                        System.IO.Directory.CreateDirectory( outputDirectory );
                    }

                    WcLib.Log.WriteLine( m_logCategory + "-Output", outputFileName );
                }

				try
				{
                    switch ( m_mode )
                    {
                        case Mode.Delete:
                        {
                            System.IO.File.Delete( file );
                            break;
                        }
                        case Mode.Move:
                        {
                            System.IO.File.Move( file, outputFileName );
                            break;
                        }
                        case Mode.Copy:
                        {
                            System.IO.File.Copy( file, outputFileName );
                            break;
                        }
                    }
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

			if ( m_mode != Mode.Skip )
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

	Mode m_mode;
    string m_logCategory;
    string m_inputDir;
    string m_outputDir;
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
        string argOutputDir = "";
		string argPatternFile = "";
        string argCategory = "";
        bool argLogOnly = false;
		bool argAuto = false;
        MyFileFilter.Mode argMode = MyFileFilter.Mode.Delete;
		System.Collections.Generic.List< string > argDefs = new System.Collections.Generic.List< string > ();

		foreach ( string arg in args )
		{
			string lowercaseArg = arg.ToLower();
			if ( lowercaseArg.StartsWith( "-dir=" ) ) argInputDir = System.IO.Path.GetFullPath( arg.Substring( 5 ) );
			else if ( lowercaseArg.StartsWith( "-pattern=" ) ) argPatternFile = arg.Substring( 9 );
			else if ( lowercaseArg.StartsWith( "-logonly" ) ) argLogOnly = true;
			else if ( lowercaseArg.StartsWith( "-auto" ) ) argAuto = true;
            else if ( lowercaseArg.StartsWith( "-category=" ) ) argCategory = arg.Substring( 10 );
			else if ( lowercaseArg.StartsWith( "-def=" ) )
			{
				string[] defs = arg.Substring( 5 ).Split( new char[] { '+' }, System.StringSplitOptions.RemoveEmptyEntries );

				if ( defs != null )
				{
					argDefs.AddRange( defs );
				}
			}
            else if ( lowercaseArg.StartsWith( "-moveto=" ) )
            {
                argOutputDir = System.IO.Path.GetFullPath( arg.Substring( 8 ) );
                argMode = MyFileFilter.Mode.Move;
            }
            else if ( lowercaseArg.StartsWith( "-copyto=" ) )
            {
                argOutputDir = System.IO.Path.GetFullPath( arg.Substring( 8 ) );
                argMode = MyFileFilter.Mode.Copy;
            }
		}

        if ( argCategory.Length > 0 )
        {
            errorLogCategory = "WcCleaner-" + argCategory + "-Errors";
        }

        if ( argLogOnly )
        {
            argMode = MyFileFilter.Mode.Skip;
        }

		if ( !ValidateInput( argInputDir, argPatternFile ) )
            return;

        WcLib.PatternList patternList = new WcLib.PatternList( argPatternFile, argDefs.ToArray() );

		if ( argMode == MyFileFilter.Mode.Delete && !argAuto )
		{
            if ( !ConfirmFileDeletion( argInputDir, patternList.SourceFileName ) )
                return;
		}

		new MyFileFilter().Run( argInputDir, argOutputDir, patternList, argMode, argCategory );
    }

    static bool ValidateInput( string inputDir, string patternFile )
    {
        if ( inputDir.Length < 1 )
        {
            System.Console.WriteLine( "ERROR: Input directory is not specified!" );
            return false;
        }

		if ( !System.IO.Directory.Exists( inputDir ) )
		{
			System.Console.WriteLine( "ERROR: Input directory '{0}' doesn't exist!", inputDir );
			return false;
		}

		if ( patternFile.Length < 1 )
		{
			System.Console.WriteLine( "ERROR: Pattern file is not specified!" );
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


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
	public enum FileState
	{
		None,
		Selected,
		NotSelected,
		Locked
	}

	public class Pattern
	{
		public Pattern( string line )
		{
			m_line = line;
		}

		public FileState GetFileState( string filePath )
		{
			if ( Wildcard.MatchPattern( filePath, m_line ) )
			{
				if ( m_line.StartsWith( "!!" ) )
					return FileState.Locked;
				if ( m_line.StartsWith( "!" ) )
					return FileState.NotSelected;
				return FileState.Selected;
			}

			return FileState.None;
		}

		public override string ToString()
		{
			return m_line;
		}

		string m_line;
	}

	public class PatternList
	{
		public PatternList( string fileName, string[] definitions = null )
		{
			string sourceFileDirectory = System.IO.Path.GetDirectoryName( System.IO.Path.GetFullPath( fileName ) );
			string[] preprocessedLines;

			using ( var reader = new System.IO.StreamReader( fileName ) )
			{
				var lines = new System.Collections.Generic.List< string > ();

				string line = null;
				while ( (line = reader.ReadLine()) != null )
				{
					lines.Add( line );
				}

				preprocessedLines = Parser.Preprocess( lines.ToArray(), definitions );
			}

			foreach ( string line in preprocessedLines )
			{
				string trimmedLine = line.Trim();

				if ( trimmedLine.StartsWith( "#include" ) )
				{
					string includeFileName = trimmedLine.Substring( 8 ).Trim().Trim( '"' );
					string includeFilePath = System.IO.Path.Combine( sourceFileDirectory, includeFileName );
					m_patterns.AddRange( new PatternList( includeFilePath ).GetPatterns() );
				}
				else
				{
					if ( trimmedLine.Length > 0 && !trimmedLine.StartsWith( "#" ) && !trimmedLine.StartsWith( ";" ) )
					{
						m_patterns.Add( new Pattern( trimmedLine ) );
					}
				}
			}
		}

		public FileState GetFileState( string filePath )
		{
			FileState fileState = FileState.NotSelected;

			foreach ( var pattern in m_patterns )
			{
				FileState fs = pattern.GetFileState( filePath );				
				if ( fs == FileState.Locked )
					return FileState.NotSelected;
				if ( fs != FileState.None )
					fileState = fs;
			}

			return fileState;
		}

		public Pattern[] GetPatterns()
		{
			return m_patterns.ToArray();
		}

		System.Collections.Generic.List< Pattern > m_patterns = new System.Collections.Generic.List< Pattern > ();
	}
}


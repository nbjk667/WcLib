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
    public static class Wildcard
    {
		static bool CharEq( char sch, char pch )
		{
			if ( pch == '?' )
			{
				return true;
			}

			return System.Char.ToUpperInvariant( pch ) == System.Char.ToUpperInvariant( sch );
		}

		static bool MatchName( string name, string wildcard )
		{
			int ni = 0;
			int wi = 0;
			int lastStar = -1;

			while ( ni < name.Length && wi < wildcard.Length )
			{
				if ( wildcard[wi] == '*' )
				{
					if ( wi+1 == wildcard.Length )
						return true;

					lastStar = wi;
				}

				if ( !CharEq( name[ni], wildcard[wi] ) )
				{
					if ( lastStar < 0 )
						return false;

					wi = lastStar;
					if ( CharEq( name[ni], wildcard[wi+1] ) )
						wi++;
				}

				ni++;
				wi++;
			}

			return ni == name.Length && wi == wildcard.Length;
		}

		static bool MatchPath( string[] path, string[] wildcards )
		{
			int pi = 0;
			int wi = 0;
			int lastStars = -1;

			while ( pi < path.Length && wi < wildcards.Length )
			{
				if ( wildcards[wi].Contains( "**" ) )
				{
					while ( wi+1 < wildcards.Length && wildcards[wi+1].Contains( "**" ) )
						wi++;
					if ( wi+1 == wildcards.Length )
						return true;
				
					lastStars = wi;
				}

				if ( wi == lastStars )
				{
					if ( MatchName( path[pi], wildcards[wi+1] ) )
						wi++;
				}
				else
				{
					if ( !MatchName( path[pi], wildcards[wi] ) )
					{
						if ( lastStars < 0 )
							return false;

						wi = lastStars;
					}
				}

				pi++;
				wi++;
			}

			return pi == path.Length && wi == wildcards.Length;
		}

		public static bool MatchPattern( string filePath, string pattern )
		{
			pattern = pattern.TrimStart( '!' );

			char[] separators =
			{
				System.IO.Path.DirectorySeparatorChar,
				System.IO.Path.AltDirectorySeparatorChar
			};

			string[] splitPath = filePath.Split( separators, System.StringSplitOptions.RemoveEmptyEntries );
			string[] splitPattern = pattern.Split( separators, System.StringSplitOptions.RemoveEmptyEntries );

			if ( splitPattern.Length == 1 )
			{
				if ( pattern[0] != System.IO.Path.DirectorySeparatorChar &&
					 pattern[0] != System.IO.Path.AltDirectorySeparatorChar )
				{
					return MatchName( splitPath[ splitPath.Length - 1 ], splitPattern[0] );
				}
			}

			return MatchPath( splitPath, splitPattern );
		}
    }
}


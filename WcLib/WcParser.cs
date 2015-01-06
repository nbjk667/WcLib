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
	static class Parser
	{
		static bool IsTrue( string condition, string[] definitions )
		{
			if ( definitions == null )
			{
				return condition.StartsWith( "!" );
			}

			if ( condition.StartsWith( "!" ) )
			{
				string sub = condition.Substring( 1 );

				foreach ( string def in definitions )
				{
					if ( def == sub )
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				foreach ( string def in definitions )
				{
					if ( def == condition )
					{
						return true;
					}
				}

				return false;
			}
		}

		public static string[] Preprocess( string[] lines, string[] definitions )
		{
			var output = new System.Collections.Generic.List< string > ();
			var sections = new System.Collections.Generic.Stack< bool > ();

			foreach ( string line in lines )
			{
				string trimmedLine = line.TrimStart();

				if ( trimmedLine.StartsWith( "#if" ) )
				{
					if ( sections.Count > 0 && !sections.Peek() )
					{
						sections.Push( false );
					}
					else
					{
						sections.Push( IsTrue( trimmedLine.Substring( 3 ).Trim(), definitions ) );
					}
				}
				else if ( trimmedLine.StartsWith( "#else" ) )
				{
					sections.Push( !sections.Pop() );
				}
				else if ( trimmedLine.StartsWith( "#endif" ) )
				{
					sections.Pop();
				}
				else
				{
					if ( sections.Count < 1 || sections.Peek() )
					{
						output.Add( line );
					}
				}
			}

			return output.ToArray();
		}
	}
}


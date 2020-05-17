using System.Collections.Generic;
using System.IO;



namespace ValidateFloat
{
	using System.Globalization;



	public class Table
	{
		public (uint initialValue, (string operationName, uint i, float f)[] results)[] Data;
	}



	public static class ResultTable
	{
		const string TABLE_PATH = "CompTable.txt";
		
		public static Table GetTableFromFile()
		{
			var output = new List<(uint initialValue, (string operationName, uint i, float f)[] results)>();

			uint tempInitialValue = default;
			List<(string operationName, uint i, float f)> tempList = null;
			foreach( var line in File.ReadLines( TABLE_PATH ) )
			{
				if( string.IsNullOrWhiteSpace( line ) || line.Trim().StartsWith( "//" ) )
					continue;

				string[] values = line.Split( ' ' );
				if( values.Length == 1 )
				{
					if( tempList != null )
						output.Add( ( tempInitialValue, tempList.ToArray() ) );

					tempInitialValue = Utility.FromFloatBinaryFormatting<uint>( values[ 0 ] );
					tempList = new List<(string operationName, uint i, float f)>();
				}
				else
				{
					float fVal;
					if( values[ 2 ] == "NaN" )
					{
						fVal = Utility.To<uint, float>( Utility.FromFloatBinaryFormatting<uint>( values[ 1 ] ) );
						if( float.IsNaN( fVal ) == false )
							System.Console.WriteLine( $"Table float is NaN but conversion from uint representation isn't: {Utility.ToFloatBinaryFormatting( fVal )}" );
					}
					else
					{
						fVal = float.Parse( values[ 2 ], CultureInfo.InvariantCulture );
					}
					tempList.Add( ( values[ 0 ], Utility.FromFloatBinaryFormatting<uint>( values[ 1 ] ), fVal ) );
				}
			}

			if( tempList != null )
				output.Add( ( tempInitialValue, tempList.ToArray() ) );

			return new Table()
			{
				Data = output.ToArray(),
			};
		}



		public static void OverwriteCompTableWith( string content )
		{
			File.WriteAllText( TABLE_PATH, content );
		}
	}
}
namespace ValidateFloat
{
	using System;
	using System.Text.RegularExpressions;



	public static class Utility
	{
		public static T2 To<T, T2>( T v ) where T : unmanaged where T2 : unmanaged
		{
			unsafe
			{
				if( sizeof(T) != sizeof(T2) )
					throw new Exception();
				return *(T2*) & v;
			}
		}


		
		public static T FromFloatBinaryFormatting<T>( string text ) where T : unmanaged
		{
			unsafe
			{
				var regexMatch = Regex.Match( text, "^0b_*([01])_*([01]{8})_*([01]{23})$" );
				if( regexMatch.Success == false )
					throw new FormatException( text );
				
				var groups = regexMatch.Groups;

				T val = default;
				byte* valPtr = (byte*)& val;
				var mask = stackalloc byte[ 8 ]
				{
					0b_10000000,
					0b_01000000,
					0b_00100000,
					0b_00010000,
					0b_00001000,
					0b_00000100,
					0b_00000010,
					0b_00000001,
				};

				int i = 0;
				for( int j = 1; j < groups.Count; j++ )
				{
					foreach( var c in groups[j].Value )
					{
						if( c == '1' )
							valPtr[ 3 - i / 8 ] |= mask[ i % 8 ];
						i++;
					}
				}

				return val;
			}
		}
		
		

		public static string ToBinary<T>( T value ) where T : unmanaged
		{
			unsafe
			{
				Span<char> chars = stackalloc char[sizeof(T) * 8];
				for( int i = 0; i < sizeof(T); i++ )
				{
					byte currentByteVal = ((byte*)&value)[ sizeof(T) - 1 - i ];
					int bit = i*8;
					chars[ bit++ ] = (currentByteVal & 0b_10000000) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_01000000) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_00100000) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_00010000) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_00001000) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_00000100) != 0 ? '1' : '0';
					chars[ bit++ ] = (currentByteVal & 0b_00000010) != 0 ? '1' : '0';
					chars[ bit   ] = (currentByteVal & 0b_00000001) != 0 ? '1' : '0';
				}

				return chars.ToString();
			}
		}

		public static string ToFloatBinaryFormatting<T>( T value ) where T : unmanaged
		{
			string asBinary = ToBinary( value );
			return $"0b_{asBinary[ 0 ].ToString()}_{asBinary.Substring( 1, 8 )}_{asBinary.Substring( 9, 23 )}";
		}



		public static bool NaNAndBitsMatch( float left, float right )
		{
			return float.IsNaN( left ) && float.IsNaN( right ) && To<float, uint>( left ) == To<float, uint>( right );
		}
	}
}
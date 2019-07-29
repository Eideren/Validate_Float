namespace ValidateFloat
{
	using System;



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

		public static string FloatToSpecializedFormatting( float value )
		{
			string asBinary = ToBinary( value );
			return $"0b_{asBinary[ 0 ].ToString()}_{asBinary.Substring( 1, 8 )}_{asBinary.Substring( 9, 23 )}";
		}
	}
}
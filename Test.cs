namespace ValidateFloat
{
	using System;
	using System.IO;
	
	public class Test
	{
		readonly bool _verifyMode;
		readonly TextWriter _errorOut, _infoOut;
		bool _containsError;

		public Test( bool verifyModeParam, TextWriter errorOutParam, TextWriter infoOutParam, out bool error )
		{
			_verifyMode = verifyModeParam;
			_errorOut = errorOutParam;
			_infoOut = infoOutParam;
			
			{
				uint val = 1234567891;
				if( Convert.ToUInt32( ToBinary( Convert.ToUInt32( ToBinary( val ), 2 ) ), 2 ) != val )
					ReportError( $"{nameof(ToBinary)} and back failed" );
			}
			
			for( int testIndex = 0; testIndex < TESTS.Length; testIndex++ )
			{
				for( int testResult = 0; testResult < TESTS[testIndex].results.Length; testResult++ )
				{
					var result = TESTS[ testIndex ].results[ testResult ];
					var asFloat = To<uint, float>( result.i );
					if( asFloat != result.f )
						ReportError( $"FAILED PARSING {ToBinary(result.i)} to value {result.f:G9}, expected {ToBinary(result.f)}" );
				}
			}
			for( int testIndex = 0; testIndex < TESTS.Length; testIndex++ )
			{
				float value = To<uint, float>( TESTS[testIndex].initialValue );
				float dVal = value * 0.1379f;
				
				// Certain operations are funneling tests into specific ranges of values
				// so we aren't just using them as is, that is dVal's purpose in this code
				
				int checkIndex = 0;
				Validate( value, checkIndex++, testIndex, out value );
				Validate( value * -2.7f, checkIndex++, testIndex, out value );
				Validate( value + 1.4478f, checkIndex++, testIndex, out value );
				Validate( value / 1.5f, checkIndex++, testIndex, out value );
				Validate( value - 0.1f, checkIndex++, testIndex, out value );
				Validate( dVal + value % 7.0f, checkIndex++, testIndex, out value ); 
				Validate( dVal -MathF.Abs( value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Acos( value % 1f ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Acosh( 1f + MathF.Abs( value ) ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Asin( value % 1f ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Asinh( value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Atan( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Atan2( value, 0.17f ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Atanh( value % 1f ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Cbrt( value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Ceiling( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Cos( value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Cosh( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Exp( 1f / value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Floor( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.FusedMultiplyAdd( value, 1.33f, -1.5f ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.IEEERemainder( value, 25.78f ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Log( MathF.Abs( value ) ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Log( MathF.Abs( value ), 4f ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Log2( MathF.Abs( value ) ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Log10( MathF.Abs( value ) ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Pow( MathF.Abs( value ), 1.7f ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.ScaleB( value, 2 ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Sin( value ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Sinh( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Sqrt( MathF.Abs( value ) ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.Tan( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Tanh( value ), checkIndex++, testIndex, out value );
				Validate( dVal - MathF.Max( value, dVal ), checkIndex++, testIndex, out value );
				Validate( dVal + MathF.MaxMagnitude( value, dVal ), checkIndex++, testIndex, out value  );
				Validate( dVal - MathF.Min( value, dVal ), checkIndex++, testIndex, out value  );
				Validate( dVal + MathF.MinMagnitude( value, dVal ), checkIndex++, testIndex, out value  );
				Validate( dVal - MathF.Round( value ), checkIndex++, testIndex, out value  );
				Validate( dVal + MathF.Sign( value ), checkIndex++, testIndex, out value  );
				Validate( dVal - MathF.Truncate( value - dVal ), checkIndex++, testIndex, out value  );
			}

			error = _containsError;
		}

		void ReportError<T>( T data )
		{
			_containsError = true;
			_errorOut?.WriteLine( data ? .ToString() );
		}
		
		void Validate( float result, int checkIndex, int currentTest, out float expectedVal )
		{
			if( _verifyMode )
			{
				uint resultI = To<float, uint>( result );
				var expected = TESTS[ currentTest ].results[ checkIndex ];
				if( expected.i != resultI || expected.f != result )
				{
					string resultAsBin = FloatToSpecializedFormatting( To<uint, float>( resultI ) );
					string expectedAsBin = FloatToSpecializedFormatting( To<uint, float>( expected.i ) );
					string xorAsBin = FloatToSpecializedFormatting( To<uint, float>( resultI ^ expected.i ) );
					ReportError( $"FAILED {CheckNames[checkIndex]}\n\t{expectedAsBin}({expected.f:G9}) expected\n\t{resultAsBin}({result:G9}) got\n\t{xorAsBin} XOR" );
				}
				expectedVal = expected.f;
			}
			else
			{
				string resultAsBin = FloatToSpecializedFormatting( result );
				if(checkIndex == 0)
					_infoOut?.WriteLine( $"( {resultAsBin}, new (uint, float)[]\n{{" );
				_infoOut?.WriteLine( $"\t({FloatToSpecializedFormatting( result )}, {result:G9}f)," );
				if(checkIndex == CheckNames.Length-1)
					_infoOut?.WriteLine( $"}} )," );
				expectedVal = result;
			}
		}

		static T2 To<T, T2>( T v ) where T : unmanaged where T2 : unmanaged
		{
			unsafe
			{
				if( sizeof(T) != sizeof(T2) )
					throw new Exception();
				return *(T2*) & v;
			}
		}

		static string ToBinary<T>( T value ) where T : unmanaged
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

		static string FloatToSpecializedFormatting( float value )
		{
			string asBinary = ToBinary( value );
			return $"0b_{asBinary[ 0 ].ToString()}_{asBinary.Substring( 1, 8 )}_{asBinary.Substring( 9, 23 )}";
		}
		
		static readonly string[] CheckNames =
		{
			"BIN",
			"MULT_OP",
			"ADD_OP",
			"DIV_OP",
			"SUB_OP",
			"MOD_OP",
			"Abs",
			"Acos",
			"Acosh",
			"Asin",
			"Asinh",
			"Atan",
			"Atan2",
			"Atanh",
			"Cbrt",
			"Ceiling",
			"Cos",
			"Cosh",
			"Exp",
			"Floor",
			"FusedMultiplyAdd",
			"IEEERemainder",
			"Log",
			"Log+",
			"Log2",
			"Log10",
			"Pow",
			"ScaleB",
			"Sin",
			"Sinh",
			"Sqrt",
			"Tan",
			"Tanh",
			"Max",
			"MaxMagnitude",
			"Min",
			"MinMagnitude",
			"Round",
			"Sign",
			"Truncate",
		};

		static readonly (uint initialValue, (uint i, float f)[] results)[] TESTS =
		{
			( 0b_1_01111111_00000000000000000000000, new (uint, float)[]
			{
				( 0b_1_01111111_00000000000000000000000, - 1f ),
				( 0b_0_10000000_01011001100110011001101, 2.70000005f ),
				( 0b_0_10000001_00001001011101011000111, 4.14779997f ),
				( 0b_0_10000000_01100001111100100001001, 2.7651999f ),
				( 0b_0_10000000_01010101001001010100011, 2.6652f ),
				( 0b_0_10000000_01000011011111101001000, 2.52729988f ),
				( 0b_1_10000000_01010101001001010100010, - 2.66519976f ),
				( 0b_0_10000000_00010100100100000111000, 2.16065788f ),
				( 0b_1_01111111_11110100101011111011101, - 1.95580637f ),
				( 0b_1_01111111_01101001000010001100110, - 1.41029048f ),
				( 0b_0_01111111_00000001100011000110101, 1.0060488f ),
				( 0b_0_01111110_01001101000100000001100, 0.650513411f ),
				( 0b_1_01111111_01110011111111010001111, - 1.45308101f ),
				( 0b_1_01111110_01000000110000001011100, - 0.626470327f ),
				( 0b_0_01111110_01101111011111011111100, 0.71775794f ),
				( 0b_0_01111110_10111001011001010010110, 0.862100005f ),
				( 0b_1_01111110_10010011110101100101010, - 0.788744569f ),
				( 0b_0_01111111_00110000100010110010101, 1.18962348f ),
				( 0b_1_10000000_00111010010100110010010, - 2.45566225f ),
				( 0b_1_10000000_10010001101001101011010, - 3.13789988f ),
				( 0b_0_10000001_01100010010001011100000, 5.5355072f ),
				( 0b_0_10000001_01011001011100100110011, 5.39760733f ),
				( 0b_1_01111111_11010010111010000011011, - 1.82385576f ),
				( 0b_0_01111101_00101110101100001010110, 0.295595825f ),
				( 0b_0_01111111_10011110110100101010111, 1.62040222f ),
				( 0b_0_01111011_00100101110001101101100, 0.0717228353f ),
				( 0b_1_01111100_00110001101001001100110, - 0.149240106f ),
				( 0b_1_01111110_01111000001111111010000, - 0.73486042f ),
				( 0b_0_01111110_00010000101011101100110, 0.532583594f ),
				( 0b_0_01111101_10101110010011100100110, 0.420220554f ),
				( 0b_1_01111110_10010010100000010111111, - 0.786144197f ),
				( 0b_1_01111111_00100011101011110100011, - 1.13939321f ),
				( 0b_0_01111110_01011010010001010100010, 0.676309705f ),
				( 0b_1_01111110_10100000111000000001100, - 0.8142097f ),
				( 0b_1_01111110_11100111011110101110110, - 0.952109694f ),
				( 0b_0_01111110_10100000111000000001100, 0.8142097f ),
				( 0b_1_01111101_00011010011010110101000, - 0.27579999f ),
				( 0b_1_01111100_00011010011010110101000, - 0.137899995f ),
				( 0b_1_01111111_00100011010011010110101, - 1.13789999f ),
				( 0b_0_01111110_10111001011001010010110, 0.862100005f ),
			} ),
			( 0b_0_01111101_10010001011011110111110, new (uint, float)[]
			{
				( 0b_0_01111101_10010001011011110111110, 0.392026842f ),
				( 0b_1_01111111_00001110111110000000111, - 1.05847251f ),
				( 0b_0_01111101_10001110101010111110000, 0.389327526f ),
				( 0b_0_01111101_00001001110001111110101, 0.259551674f ),
				( 0b_0_01111100_01000110110000110000100, 0.15955168f ),
				( 0b_0_01111100_10110101011110100100111, 0.213612184f ),
				( 0b_1_01111100_01000110110000110000100, - 0.15955168f ),
				( 0b_0_01111111_11001000111110111110000, 1.78509331f ),
				( 0b_1_01111111_10100001001001000010100, - 1.62945795f ),
				( 0b_1_01111110_01000000111010110100001, - 0.626794875f ),
				( 0b_0_01111110_01001010100111001000111, 0.645725667f ),
				( 0b_0_01111110_01000001001111011101110, 0.627425075f ),
				( 0b_1_01111111_01000000100011000100000, - 1.25214005f ),
				( 0b_1_01111100_10100001000011000001011, - 0.203636333f ),
				( 0b_0_01111110_01001000111001101111010, 0.642387033f ),
				( 0b_0_01111111_00001101110101101110100, 1.05406046f ),
				( 0b_1_01111101_11000010100010110100000, - 0.439984322f ),
				( 0b_0_01111111_00100111000001010101011, 1.15242517f ),
				( 0b_1_10000000_00101001111010011110101, - 2.32745099f ),
				( 0b_1_10000000_01111001000101001000110, - 2.94593954f ),
				( 0b_0_10000001_01011110001101111110000, 5.47216034f ),
				( 0b_0_10000001_01100001101011011001101, 5.5262208f ),
				( 0b_1_01111111_10100111110010110010100, - 1.65544367f ),
				( 0b_0_01111101_10101011101100011000101, 0.417669445f ),
				( 0b_0_01111111_01010000010010011101110, 1.313627f ),
				( 0b_0_01111100_01100001010110001100001, 0.172532573f ),
				( 0b_0_01110110_11011011111110110010000, 0.00363144651f ),
				( 0b_0_01111011_00011000111011011111000, 0.0685862899f ),
				( 0b_1_01111000_11011010001110000010100, - 0.0144720264f ),
				( 0b_0_01111010_01000100010011011111110, 0.0395879671f ),
				( 0b_1_01111100_00101000110001001101111, - 0.144906744f ),
				( 0b_1_01111011_01111000010010111011100, - 0.091869086f ),
				( 0b_0_01111100_00101010010101100001010, 0.145671993f ),
				( 0b_1_01111011_01110111001111011001110, - 0.0916114897f ),
				( 0b_1_01111010_00110011100111100010001, - 0.0375509895f ),
				( 0b_0_01111011_01110111001111011001110, 0.0916114897f ),
				( 0b_0_01111011_10111010110111010001011, 0.108121f ),
				( 0b_0_01111010_10111010110111010001011, 0.0540605001f ),
				( 0b_0_01111111_00001101110101101110100, 1.05406046f ),
				( 0b_0_01111010_10111010110111010001011, 0.0540605001f ),
			} ),
			( 0b_1_00000111_00001001010000111110001, new (uint, float)[]
			{
				( 0b_1_00000111_00001001010000111110001, - 7.79544264E-37f ),
				( 0b_0_00001000_01100110000110111010010, 2.10476949E-36f ),
				( 0b_0_01111111_01110010101000110000011, 1.44780004f ),
				( 0b_0_01111110_11101110001011101011001, 0.965200007f ),
				( 0b_0_01111110_10111010111110110111111, 0.865199983f ),
				( 0b_0_01111110_10111010111110110111111, 0.865199983f ),
				( 0b_1_01111110_10111010111110110111111, - 0.865199983f ),
				( 0b_0_10000000_01001110111001000110100, 2.61634541f ),
				( 0b_1_01111111_11110101011110111111000, - 1.95892239f ),
				( 0b_1_01111111_01001000011111100110011, - 1.28317869f ),
				( 0b_0_01111111_00010001011100100111110, 1.06815314f ),
				( 0b_0_01111110_10100010111111010111001, 0.818339884f ),
				( 0b_1_01111111_01011101101100000101011, - 1.36597192f ),
				( 0b_1_01111101_10001000111110010111011, - 0.383764118f ),
				( 0b_0_01111110_01110100000100011111000, 0.726699352f ),
				( 0b_0_01111111_00000000000000000000000, 1f ),
				( 0b_1_01111110_00010100101000101000000, - 0.540302277f ),
				( 0b_0_01111111_00100110010010001101011, 1.14954889f ),
				( 0b_1_10000000_00110001011111101111110, - 2.38668776f ),
				( 0b_1_10000000_10000000000000000000000, - 3f ),
				( 0b_0_10000001_01011111010111000010101, 5.49000025f ),
				( 0b_0_10000001_01011111010111000010101, 5.49000025f ),
				( 0b_1_01111111_10110011111100110001110, - 1.7029283f ),
				( 0b_0_01111101_10001001001110011001110, 0.384008825f ),
				( 0b_0_01111111_01100001011110110101110, 1.38078856f ),
				( 0b_0_01111100_00011110111110110000000, 0.140127182f ),
				( 0b_1_01111010_00100010000011011010110, - 0.0354069099f ),
				( 0b_1_01111100_00100010000011011010110, - 0.14162764f ),
				( 0b_0_01111100_00100001000101011011000, 0.141154647f ),
				( 0b_0_01111100_00100010000010111011000, 0.141623855f ),
				( 0b_1_01111101_10000001010111001000001, - 0.376329452f ),
				( 0b_1_01111101_10010100101001011001000, - 0.395162821f ),
				( 0b_0_01111101_10000000110100100101111, 0.375802487f ),
				( 0b_1_01111101_10000000110100100101111, - 0.375802487f ),
				( 0b_1_01111101_10000000110100100101111, - 0.375802487f ),
				( 0b_0_01111101_10000000110100100101111, 0.375802487f ),
				( 0b_1_00000101_00100100101000111111101, - 2.14998309E-37f ),
				( 0b_1_00000100_00100100101000111111101, - 1.07499154E-37f ),
				( 0b_1_01111111_00000000000000000000000, - 1f ),
				( 0b_0_01111111_00000000000000000000000, 1f ),
			} ),
			( 0b_0_10001000_00000000000000000000000, new (uint, float)[]
			{
				( 0b_0_10001000_00000000000000000000000, 512f ),
				( 0b_1_10001001_01011001100110011001101, - 1382.40002f ),
				( 0b_1_10001001_01011001001111001111001, - 1380.95227f ),
				( 0b_1_10001000_11001100010100010100001, - 920.634827f ),
				( 0b_1_10001000_11001100010111100000111, - 920.734802f ),
				( 0b_0_10000101_00001011011110101110000, 66.8699951f ),
				( 0b_0_10000000_11011110000011100000000, 3.73480225f ),
				( 0b_0_10000101_00011101011001101010000, 71.3502197f ),
				( 0b_0_10000101_00000110100001010100111, 65.6301804f ),
				( 0b_0_10000101_00011101001001010111011, 71.2865829f ),
				( 0b_0_10000101_00000110100101000101111, 65.6448898f ),
				( 0b_0_10000101_00100000101001000011011, 72.1603622f ),
				( 0b_0_10000101_00010100001001010011101, 69.0363541f ),
				( 0b_0_10000101_00011010100100001000111, 70.6411667f ),
				( 0b_0_10000101_00001001111000100100011, 66.4709702f ),
				( 0b_0_10000110_00010011001101011010100, 137.604797f ),
				( 0b_0_10000101_00010111001011010000111, 69.7939987f ),
				( 0b_0_11100010_10011101011010101110010, 1.02357226E+30f ),
				( 0b_0_10000101_00010110011010110101000, 69.6047974f ),
				( 0b_0_10000110_00010111001101011010100, 139.604797f ),
				( 0b_1_10000101_11000110010001110100010, - 113.569595f ),
				( 0b_0_10000100_11100001001111011101110, 60.1552048f ),
				( 0b_0_10000101_00001010000010000000111, 66.5078659f ),
				( 0b_0_10000101_00100110100001111011010, 73.6325226f ),
				( 0b_0_10000101_00000001100111000011000, 64.4025269f ),
				( 0b_0_10000101_00100001101001111010001, 72.4137039f ),
				( 0b_1_10001001_01011001000111110100100, - 1380.48877f ),
				( 0b_1_10001011_01010100101101011001101, - 5451.3501f ),
				( 0b_0_10000101_00010111111000011111111, 69.9706955f ),
				( 0b_0_11100010_11101101010100011000011, 1.22139733E+30f ),
				( 0b_1_10110000_11110110100100101000101, - 1.10516844E+15f ),
				( 0b_0_10000101_00100110010001011101110, 73.568222f ),
				( 0b_0_10000101_00010110011010110101000, 69.6047974f ),
				( 0b_0_00000000_00000000000000000000000, 0f ),
				( 0b_0_10000110_00011010011010110101000, 141.209595f ),
				( 0b_0_00000000_00000000000000000000000, 0f ),
				( 0b_0_10000101_00011010011010110101000, 70.6047974f ),
				( 0b_1_01111101_10010100101100000000000, - 0.395202637f ),
				( 0b_0_10000101_00010110011010110101000, 69.6047974f ),
				( 0b_0_10000101_00011110011010110101000, 71.6047974f ),
			} ),
		};
	}
}
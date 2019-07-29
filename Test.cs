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
					var asFloat = To<uint, float>( result.bin );
					if( asFloat != result.f )
						ReportError( $"FAILED PARSING {ToBinary(result.bin)} to value {result.f:G9}, expected {ToBinary(result.f)}" );
				}
			}
			for( int testIndex = 0; testIndex < TESTS.Length; testIndex++ )
			{
				float value = To<uint, float>( TESTS[testIndex].initialValue );
				
				int checkIndex = 0;
				Validate( value, checkIndex++, testIndex );
				
				value *= value; Validate( value, checkIndex++, testIndex );
				value += value; Validate( value, checkIndex++, testIndex );
				value /= 5.5f; Validate( value, checkIndex++, testIndex );
				value -= 0.1f; Validate( value, checkIndex++, testIndex );
				value %= 7.0f; Validate( value, checkIndex++, testIndex );
				value = MathF.Abs(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Acos(value % 1f); Validate( value, checkIndex++, testIndex );
				value = MathF.Acosh(1f + MathF.Abs(value)); Validate( value, checkIndex++, testIndex );
				value = MathF.Asin(value % 1f); Validate( value, checkIndex++, testIndex );
				value = MathF.Asinh(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Atan(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Atan2(value, value); Validate( value, checkIndex++, testIndex );
				value = MathF.Atanh(value % 1f); Validate( value, checkIndex++, testIndex );
				value = MathF.Cbrt(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Ceiling(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Cos(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Cosh(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Exp(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Floor(value); Validate( value, checkIndex++, testIndex );
				value = MathF.FusedMultiplyAdd(value, value, value); Validate( value, checkIndex++, testIndex );
				value = MathF.IEEERemainder(value, 25.78f); Validate( value, checkIndex++, testIndex );
				value = MathF.Log(MathF.Abs(value)); Validate( value, checkIndex++, testIndex );
				value = MathF.Log(MathF.Abs(value), 4f); Validate( value, checkIndex++, testIndex );
				value = MathF.Log2(MathF.Abs(value)); Validate( value, checkIndex++, testIndex );
				value = MathF.Log10(MathF.Abs(value)); Validate( value, checkIndex++, testIndex );
				value = MathF.Pow(value, value); Validate( value, checkIndex++, testIndex );
				value = MathF.ScaleB(value, 2); Validate( value, checkIndex++, testIndex );
				value = MathF.Sin(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Sinh(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Sqrt(MathF.Abs(value)); Validate( value, checkIndex++, testIndex );
				value = MathF.Tan(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Tanh(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Log(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Max(value, TESTS[testIndex].initialValue); Validate( value, checkIndex++, testIndex );
				value = MathF.MaxMagnitude(value, TESTS[testIndex].initialValue); Validate( value, checkIndex++, testIndex );
				value = MathF.Min(value, TESTS[testIndex].initialValue); Validate( value, checkIndex++, testIndex );
				value = MathF.MinMagnitude(value, TESTS[testIndex].initialValue); Validate( value, checkIndex++, testIndex );
				value = MathF.Round(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Sign(value); Validate( value, checkIndex++, testIndex );
				value = MathF.Truncate(value + TESTS[testIndex].initialValue); Validate( value, checkIndex++, testIndex );
			}

			error = _containsError;
		}



		void ReportError<T>( T data )
		{
			_containsError = true;
			_errorOut?.WriteLine( data ? .ToString() );
		}
		
		
		
		void Validate( float result, int checkIndex, int currentTest )
		{
			if( _verifyMode )
			{
				uint asBin = To<float, uint>( result );
				var expectedResult = TESTS[ currentTest ].results[ checkIndex ];
				if( expectedResult.bin != asBin )
				{
					string currentAsBin = FloatToSpecializedFormatting( To<uint, float>( asBin ) );
					string expectedAsBin = FloatToSpecializedFormatting( To<uint, float>( expectedResult.bin ) );
					ReportError( $"FAILED BINARY {CheckNames[checkIndex]}, expected {expectedAsBin} got {currentAsBin}" );
				}

				if( expectedResult.f != result )
				{
					ReportError( $"FAILED FLOAT {CheckNames[checkIndex]}, expected {expectedResult.f:G9} got {result:G9}, delta {expectedResult.f - result}" );
				}
			}
			else
			{
				if(checkIndex == 0)
					_infoOut?.WriteLine( $"---TEST{currentTest}------" );
				_infoOut?.WriteLine( $"({FloatToSpecializedFormatting( result )}, {result:G9}f)," );
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
			"Log",
			"Max",
			"MaxMagnitude",
			"Min",
			"MinMagnitude",
			"Round",
			"Sign",
			"Truncate",
		};

		static readonly (uint initialValue, (uint bin, float f)[] results)[] TESTS =
		{
			(0b_1_01010101_01010101010101010101010, new (uint, float)[]
			{
				(0b_1_01010101_01010101010101010101010, -3.03164883E-13f),
				(0b_0_00101011_11000111000111000110111, 9.19089453E-26f),
				(0b_0_00101100_11000111000111000110111, 1.83817891E-25f),
				(0b_0_00101010_01001010111111010110100, 3.34214358E-26f),
				(0b_1_01111011_10011001100110011001101, -0.100000001f),
				(0b_1_01111011_10011001100110011001101, -0.100000001f),
				(0b_0_01111011_10011001100110011001101, 0.100000001f),
				(0b_0_01111111_01111000011110110010001, 1.47062886f),
				(0b_0_01111111_10001101110010111011110, 1.55388999f),
				(0b_0_01111110_00101100100011110001011, 0.587029159f),
				(0b_0_01111110_00011101100001110000010, 0.557670712f),
				(0b_0_01111110_00000100011101100001010, 0.508713365f),
				(0b_0_01111110_10010010000111111011011, 0.785398185f),
				(0b_0_01111111_00001111001011101011001, 1.05930626f),
				(0b_0_01111111_00000100111101101100010, 1.01939034f),
				(0b_0_10000000_00000000000000000000000, 2f),
				(0b_1_01111101_10101010001000100110011, -0.416146845f),
				(0b_0_01111111_00010110011111010001001, 1.08784592f),
				(0b_0_10000000_01111011111000110100110, 2.96787405f),
				(0b_0_10000000_00000000000000000000000, 2f),
				(0b_0_10000001_10000000000000000000000, 6f),
				(0b_0_10000001_10000000000000000000000, 6f),
				(0b_0_01111111_11001010101100001100000, 1.79175949f),
				(0b_0_01111101_10101110110010001111011, 0.42068848f),
				(0b_1_01111111_00111111110010011111110, -1.24917579f),
				(0b_0_01111011_10001011110001010010010, 0.0966235548f),
				(0b_0_01111110_10011000100000110010001, 0.797875464f),
				(0b_0_10000000_10011000100000110010001, 3.19150186f),
				(0b_1_01111010_10011000101011111011110, -0.0498884842f),
				(0b_1_01111010_10011000110110110010010, -0.0499091819f),
				(0b_0_01111100_11001001100001111101100, 0.223403633f),
				(0b_0_01111100_11010001010011000010001, 0.227195993f),
				(0b_0_01111100_11001001011101000001001, 0.223365918f),
				(0b_1_01111111_01111111101110101100101, -1.49894392f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
				(0b_0_01111111_00000000000000000000000, 1f),
				(0b_0_10011110_01010101010101010101011, 2.86331162E+09f),
			}),
			(0b_0_00100110_11100101011110000111100, new (uint, float)[]
			{
				(0b_0_00100110_11100101011110000111100, 3.0637501E-27f),
				(0b_0_00000000_00000000000000000000000, 0f),
				(0b_0_00000000_00000000000000000000000, 0f),
				(0b_0_00000000_00000000000000000000000, 0f),
				(0b_1_01111011_10011001100110011001101, -0.100000001f),
				(0b_1_01111011_10011001100110011001101, -0.100000001f),
				(0b_0_01111011_10011001100110011001101, 0.100000001f),
				(0b_0_01111111_01111000011110110010001, 1.47062886f),
				(0b_0_01111111_10001101110010111011110, 1.55388999f),
				(0b_0_01111110_00101100100011110001011, 0.587029159f),
				(0b_0_01111110_00011101100001110000010, 0.557670712f),
				(0b_0_01111110_00000100011101100001010, 0.508713365f),
				(0b_0_01111110_10010010000111111011011, 0.785398185f),
				(0b_0_01111111_00001111001011101011001, 1.05930626f),
				(0b_0_01111111_00000100111101101100010, 1.01939034f),
				(0b_0_10000000_00000000000000000000000, 2f),
				(0b_1_01111101_10101010001000100110011, -0.416146845f),
				(0b_0_01111111_00010110011111010001001, 1.08784592f),
				(0b_0_10000000_01111011111000110100110, 2.96787405f),
				(0b_0_10000000_00000000000000000000000, 2f),
				(0b_0_10000001_10000000000000000000000, 6f),
				(0b_0_10000001_10000000000000000000000, 6f),
				(0b_0_01111111_11001010101100001100000, 1.79175949f),
				(0b_0_01111101_10101110110010001111011, 0.42068848f),
				(0b_1_01111111_00111111110010011111110, -1.24917579f),
				(0b_0_01111011_10001011110001010010010, 0.0966235548f),
				(0b_0_01111110_10011000100000110010001, 0.797875464f),
				(0b_0_10000000_10011000100000110010001, 3.19150186f),
				(0b_1_01111010_10011000101011111011110, -0.0498884842f),
				(0b_1_01111010_10011000110110110010010, -0.0499091819f),
				(0b_0_01111100_11001001100001111101100, 0.223403633f),
				(0b_0_01111100_11010001010011000010001, 0.227195993f),
				(0b_0_01111100_11001001011101000001001, 0.223365918f),
				(0b_1_01111111_01111111101110101100101, -1.49894392f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
				(0b_0_01111111_00000000000000000000000, 1f),
				(0b_0_10011011_00110111001010111100010, 326286400f),
			}),
		};
	}
}
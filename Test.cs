namespace ValidateFloat
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static Utility;
	
	public class Test
	{
		readonly Mode _mode;
		readonly TextWriter _errorOut, _infoOut;
		readonly HashSet<string> _failedOperations;
		
		public bool RaisedErrors{ get; private set; }

		public Test( Mode modeParam, TextWriter errorOutParam, TextWriter infoOutParam )
		{
			_mode = modeParam;
			_errorOut = errorOutParam;
			_infoOut = infoOutParam;
			_failedOperations = new HashSet<string>();
			
			// Validate parsing, this whole program expects little endian
			{
				uint val = 1234567891;
				if( Convert.ToUInt32( ToBinary( Convert.ToUInt32( ToBinary( val ), 2 ) ), 2 ) != val )
					ReportError( $"{nameof(ToBinary)} and back failed" );
			}
			
			for( int testIndex = 0; testIndex < ResultTable.TABLE.Length; testIndex++ )
			{
				for( int testResult = 0; testResult < ResultTable.TABLE[testIndex].results.Length; testResult++ )
				{
					var result = ResultTable.TABLE[ testIndex ].results[ testResult ];
					var asFloat = To<uint, float>( result.i );
					if( asFloat != result.f && ! NaNAndBitsMatch( asFloat, result.f ) )
						ReportError( $"FAILED PARSING {ToBinary(result.i)} to value {result.f:G9}, expected {ToBinary(result.f)}" );
				}
			}
			for( int testIndex = 0; testIndex < ResultTable.TABLE.Length; testIndex++ )
			{
				float value = To<uint, float>( ResultTable.TABLE[testIndex].initialValue );
				
				// Certain operations are funneling tests into specific ranges of values
				// so we aren't just using them as is, that is dVal's purpose in this code
				
				int checkIndex = 0;
				StartValidation( value );
				Validate( "BIN", value, testIndex );
				Validate( "+", value + 0.1735f, testIndex );
				Validate( "-", value - 17f, testIndex );
				Validate( "*", value * 7.7757f, testIndex );
				Validate( "/", value / 0.3353f, testIndex );
				Validate( "%", value % 7.0f, testIndex ); 
				Validate( nameof(MathF.Abs), MathF.Abs( value ), testIndex );
				Validate( nameof(MathF.Acos), MathF.Acos( value % 1f ), testIndex );
				Validate( nameof(MathF.Acosh), MathF.Acosh( 1f + MathF.Abs( value ) ), testIndex );
				Validate( nameof(MathF.Asin), MathF.Asin( value % 1f ), testIndex );
				Validate( nameof(MathF.Asinh), MathF.Asinh( value ), testIndex );
				Validate( nameof(MathF.Atan), MathF.Atan( value ), testIndex );
				Validate( nameof(MathF.Atan2), MathF.Atan2( value, 0.17f ), testIndex );
				Validate( nameof(MathF.Atanh), MathF.Atanh( value % 1f ), testIndex );
				Validate( nameof(MathF.Cbrt), MathF.Cbrt( value ), testIndex );
				Validate( nameof(MathF.Ceiling), MathF.Ceiling( value ), testIndex );
				Validate( nameof(MathF.Cos), MathF.Cos( value ), testIndex );
				Validate( nameof(MathF.Cosh), MathF.Cosh( value % 2f ), testIndex );
				Validate( nameof(MathF.Exp), MathF.Exp( 1f / value ), testIndex );
				Validate( nameof(MathF.Floor), MathF.Floor( value ), testIndex );
				Validate( nameof(MathF.FusedMultiplyAdd), MathF.FusedMultiplyAdd( value, 1.33f, -1.5f ), testIndex );
				Validate( nameof(MathF.IEEERemainder), MathF.IEEERemainder( value, 25.78f ), testIndex );
				Validate( nameof(MathF.Log), MathF.Log( MathF.Abs( value ) ), testIndex );
				Validate( nameof(MathF.Log)+"(x,y)", MathF.Log( MathF.Abs( value ), 4f ), testIndex );
				Validate( nameof(MathF.Log2), MathF.Log2( MathF.Abs( value ) ), testIndex );
				Validate( nameof(MathF.Log10), MathF.Log10( MathF.Abs( value ) ), testIndex );
				Validate( nameof(MathF.Pow), MathF.Pow( MathF.Abs( value ) % 1E+23f, 1.7f ), testIndex );
				Validate( nameof(MathF.ScaleB), MathF.ScaleB( value % 1E+23f, 2 ), testIndex );
				Validate( nameof(MathF.Sin), MathF.Sin( value ), testIndex );
				Validate( nameof(MathF.Sinh), MathF.Sinh( value % 2f ), testIndex );
				Validate( nameof(MathF.Sqrt), MathF.Sqrt( MathF.Abs( value ) ), testIndex );
				Validate( nameof(MathF.Tan), MathF.Tan( value ), testIndex );
				Validate( nameof(MathF.Tanh), MathF.Tanh( value ), testIndex );
				Validate( nameof(MathF.Max), MathF.Max( value, 0.9f ), testIndex );
				Validate( nameof(MathF.MaxMagnitude), MathF.MaxMagnitude( value, -0.7f ), testIndex );
				Validate( nameof(MathF.Min), MathF.Min( value, 307f ), testIndex );
				Validate( nameof(MathF.MinMagnitude), MathF.MinMagnitude( value, -8.89f ), testIndex );
				Validate( nameof(MathF.Round), MathF.Round( value ), testIndex );
				Validate( nameof(MathF.Sign), MathF.Sign( value ), testIndex );
				Validate( nameof(MathF.Truncate), MathF.Truncate( value ), testIndex );
				EndValidation();
			}
		}



		void StartValidation( float result )
		{
			if( _mode != Mode.Validate )
			{
				string resultAsBin = FloatToSpecializedFormatting( result );
				_infoOut?.WriteLine( $"( {resultAsBin}, new (string, uint, float)[]\n{{" );
			}
		}

		void EndValidation()
		{
			if( _mode != Mode.Validate )
				_infoOut?.WriteLine( $"}} )," );
		}
		
		void Validate( string operationName, float result, int currentTest )
		{
			if( _mode == Mode.Validate )
			{
				uint resultI = To<float, uint>( result );
				
				(uint i, float f) expected;
				{
					(uint i, float f)? expectedOrNull = null;
					foreach( var expectedResult in ResultTable.TABLE[ currentTest ].results )
					{
						if( operationName.Equals( expectedResult.operationName ) )
							expectedOrNull = ( expectedResult.i, expectedResult.f );
					}

					if( expectedOrNull == null )
					{
						ReportError( $"Could not find expected data for operation '{operationName}'" );
						return;
					}

					expected = expectedOrNull.Value;
				}
				
				if( expected.i != resultI || expected.f != result && ! NaNAndBitsMatch( expected.f, result ) )
				{
					string resultAsBin = FloatToSpecializedFormatting( To<uint, float>( resultI ) );
					string expectedAsBin = FloatToSpecializedFormatting( To<uint, float>( expected.i ) );
					string xorAsBin = FloatToSpecializedFormatting( To<uint, float>( resultI ^ expected.i ) );
					ReportError( $"FAILED {operationName} on test {currentTest}\n\t{expectedAsBin}({expected.f:G9}) expected\n\t{resultAsBin}({result:G9}) got\n\t{xorAsBin} XOR" );
					if( _failedOperations.Contains( operationName ) == false )
						_failedOperations.Add( operationName );
				}
			}
			else
			{
				string resultAsBin = FloatToSpecializedFormatting( result );
				string resultString;
				if( float.IsPositiveInfinity( result ) )
					resultString = "float.PositiveInfinity";
				else if( float.IsNegativeInfinity( result ) )
					resultString = "float.NegativeInfinity";
				else if( float.IsNaN( result ) )
					resultString = $"To<uint, float>({resultAsBin})";
				else
					resultString = $"{result:G9}f";
				_infoOut?.WriteLine( $"\t(\"{operationName}\", {resultAsBin}, {resultString})," );
			}
		}
		
		void ReportError<T>( T data )
		{
			RaisedErrors = true;
			_errorOut?.WriteLine( data ? .ToString() );
		}

		public string[] GetFailedOperations()
		{
			return _failedOperations.OrderBy( s => s ).ToArray();
		}
		
		public enum Mode
		{
			Validate,
			PrintTables
		}
	}
}
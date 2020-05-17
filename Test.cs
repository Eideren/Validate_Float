namespace ValidateFloat
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using static Utility;
	
	public class Test
	{
		readonly Table _table;
		readonly Mode _mode;
		readonly TextWriter _errorOut, _infoOut;
		readonly HashSet<string> _failedOperations;
		
		public bool RaisedErrors{ get; private set; }

		public Test( Mode modeParam, TextWriter errorOutParam, TextWriter infoOutParam, Table table )
		{
			_table = table;
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
			
			for( int testIndex = 0; testIndex < _table.Data.Length; testIndex++ )
			{
				for( int testResult = 0; testResult < (_table.Data[testIndex].results?.Length ?? 0); testResult++ )
				{
					var result = _table.Data[ testIndex ].results[ testResult ];
					var asFloat = To<uint, float>( result.i );
					if( asFloat != result.f && ! NaNAndBitsMatch( asFloat, result.f ) )
						ReportError( $"FAILED PARSING {ToBinary(result.i)} to value {result.f:G9}, expected {ToBinary(result.f)}" );
				}
			}
			for( int testIndex = 0; testIndex < _table.Data.Length; testIndex++ )
			{
				float value = To<uint, float>( _table.Data[testIndex].initialValue );
				
				// Certain operations are funneling tests into specific ranges of values
				// so we aren't just using them as is, that is dVal's purpose in this code
				
				StartValidation( value );
				Validate( testIndex, "BIN", value );
				Validate( testIndex, "+", value + 0.1735f );
				Validate( testIndex, "-", value - 17f );
				Validate( testIndex, "*", value * 7.7757f );
				Validate( testIndex, "/", value / 0.3353f );
				Validate( testIndex, "%", value % 7.0f ); 
				// MATHF
				Validate( testIndex, nameof(MathF.Abs), MathF.Abs( value ) );
				Validate( testIndex, nameof(MathF.Acos), MathF.Acos( value % 1f ) );
				Validate( testIndex, nameof(MathF.Acosh), MathF.Acosh( 1f + MathF.Abs( value ) ) );
				Validate( testIndex, nameof(MathF.Asin), MathF.Asin( value % 1f ) );
				Validate( testIndex, nameof(MathF.Asinh), MathF.Asinh( value ) );
				Validate( testIndex, nameof(MathF.Atan), MathF.Atan( value ) );
				Validate( testIndex, nameof(MathF.Atan2), MathF.Atan2( value, 0.17f ) );
				Validate( testIndex, nameof(MathF.Atanh), MathF.Atanh( value % 1f ) );
				Validate( testIndex, nameof(MathF.Cbrt), MathF.Cbrt( value ) );
				Validate( testIndex, nameof(MathF.Ceiling), MathF.Ceiling( value ) );
				Validate( testIndex, nameof(MathF.Cos), MathF.Cos( value ) );
				Validate( testIndex, nameof(MathF.Cosh), MathF.Cosh( value % 2f ) );
				Validate( testIndex, nameof(MathF.Exp), MathF.Exp( 1f / value ) );
				Validate( testIndex, nameof(MathF.Floor), MathF.Floor( value ) );
				Validate( testIndex, nameof(MathF.FusedMultiplyAdd), MathF.FusedMultiplyAdd( value, 1.33f, -1.5f ) );
				Validate( testIndex, nameof(MathF.IEEERemainder), MathF.IEEERemainder( value, 25.78f ) );
				Validate( testIndex, nameof(MathF.Log), MathF.Log( MathF.Abs( value ) ) );
				Validate( testIndex, nameof(MathF.Log)+"(x,y)", MathF.Log( MathF.Abs( value ), 4f ) );
				Validate( testIndex, nameof(MathF.Log2), MathF.Log2( MathF.Abs( value ) ) );
				Validate( testIndex, nameof(MathF.Log10), MathF.Log10( MathF.Abs( value ) ) );
				Validate( testIndex, nameof(MathF.Pow), MathF.Pow( MathF.Abs( value ) % 1E+23f, 1.7f ) );
				Validate( testIndex, nameof(MathF.ScaleB), MathF.ScaleB( value % 1E+23f, 2 ) );
				Validate( testIndex, nameof(MathF.Sin), MathF.Sin( value ) );
				Validate( testIndex, nameof(MathF.Sinh), MathF.Sinh( value % 2f ) );
				Validate( testIndex, nameof(MathF.Sqrt), MathF.Sqrt( MathF.Abs( value ) ) );
				Validate( testIndex, nameof(MathF.Tan), MathF.Tan( value ) );
				Validate( testIndex, nameof(MathF.Tanh), MathF.Tanh( value ) );
				Validate( testIndex, nameof(MathF.Max), MathF.Max( value, 0.9f ) );
				Validate( testIndex, nameof(MathF.MaxMagnitude), MathF.MaxMagnitude( value, -0.7f ) );
				Validate( testIndex, nameof(MathF.Min), MathF.Min( value, 307f ) );
				Validate( testIndex, nameof(MathF.MinMagnitude), MathF.MinMagnitude( value, -8.89f ) );
				Validate( testIndex, nameof(MathF.Round), MathF.Round( value ) );
				if(float.IsNaN(value) == false) Validate( testIndex, nameof(MathF.Sign), MathF.Sign( value ) );
				Validate( testIndex, nameof(MathF.Truncate), MathF.Truncate( value ) );
				// MATH
				double valueAsDouble = value;
				Validate( testIndex, $"D.{nameof(Math.Abs)}", Math.Abs( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Acos)}", Math.Acos( valueAsDouble % 1d ) );
				Validate( testIndex, $"D.{nameof(Math.Acosh)}", Math.Acosh( 1d + Math.Abs( valueAsDouble ) ) );
				Validate( testIndex, $"D.{nameof(Math.Asin)}", Math.Asin( valueAsDouble % 1d ) );
				Validate( testIndex, $"D.{nameof(Math.Asinh)}", Math.Asinh( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Atan)}", Math.Atan( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Atan2)}", Math.Atan2( valueAsDouble, 0.17d ) );
				Validate( testIndex, $"D.{nameof(Math.Atanh)}", Math.Atanh( valueAsDouble % 1d ) );
				Validate( testIndex, $"D.{nameof(Math.Cbrt)}", Math.Cbrt( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Ceiling)}", Math.Ceiling( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Cos)}", Math.Cos( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Cosh)}", Math.Cosh( valueAsDouble % 2d ) );
				Validate( testIndex, $"D.{nameof(Math.Exp)}", Math.Exp( 1d / valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Floor)}", Math.Floor( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.FusedMultiplyAdd)}", Math.FusedMultiplyAdd( valueAsDouble, 1.33d, -1.5d ) );
				Validate( testIndex, $"D.{nameof(Math.IEEERemainder)}", Math.IEEERemainder( valueAsDouble, 25.78d ) );
				Validate( testIndex, $"D.{nameof(Math.Log)}", Math.Log( Math.Abs( valueAsDouble ) ) );
				Validate( testIndex, $"D.{nameof(Math.Log)}"+"(x,y)", Math.Log( Math.Abs( valueAsDouble ), 4d ) );
				Validate( testIndex, $"D.{nameof(Math.Log2)}", Math.Log2( Math.Abs( valueAsDouble ) ) );
				Validate( testIndex, $"D.{nameof(Math.Log10)}", Math.Log10( Math.Abs( valueAsDouble ) ) );
				Validate( testIndex, $"D.{nameof(Math.Pow)}", Math.Pow( Math.Abs( valueAsDouble ) % 1E+23d, 1.7d ) );
				Validate( testIndex, $"D.{nameof(Math.ScaleB)}", Math.ScaleB( valueAsDouble % 1E+23d, 2 ) );
				Validate( testIndex, $"D.{nameof(Math.Sin)}", Math.Sin( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Sinh)}", Math.Sinh( valueAsDouble % 2d ) );
				Validate( testIndex, $"D.{nameof(Math.Sqrt)}", Math.Sqrt( Math.Abs( valueAsDouble ) ) );
				Validate( testIndex, $"D.{nameof(Math.Tan)}", Math.Tan( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Tanh)}", Math.Tanh( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Max)}", Math.Max( valueAsDouble, 0.9d ) );
				Validate( testIndex, $"D.{nameof(Math.MaxMagnitude)}", Math.MaxMagnitude( valueAsDouble, -0.7d ) );
				Validate( testIndex, $"D.{nameof(Math.Min)}", Math.Min( valueAsDouble, 307d ) );
				Validate( testIndex, $"D.{nameof(Math.MinMagnitude)}", Math.MinMagnitude( valueAsDouble, -8.89d ) );
				Validate( testIndex, $"D.{nameof(Math.Round)}", Math.Round( valueAsDouble ) );
				if(float.IsNaN(value) == false) Validate( testIndex, $"D.{nameof(Math.Sign)}", Math.Sign( valueAsDouble ) );
				Validate( testIndex, $"D.{nameof(Math.Truncate)}", Math.Truncate( valueAsDouble ) );
			}
		}



		void StartValidation( float result )
		{
			if( _mode != Mode.Validate )
			{
				string resultAsBin = ToFloatBinaryFormatting( result );
				_infoOut?.WriteLine( resultAsBin );
			}
		}



		void Validate( int currentTest, string operationName, double result ) => Validate(currentTest, operationName, (float)result);
		
		void Validate( int currentTest, string operationName, float result )
		{
			if( _mode == Mode.Validate )
			{
				uint resultI = To<float, uint>( result );
				
				(uint i, float f) expected;
				{
					(uint i, float f)? expectedOrNull = null;
					foreach( var expectedResult in _table.Data[ currentTest ].results )
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
					string resultAsBin = ToFloatBinaryFormatting( To<uint, float>( resultI ) );
					string expectedAsBin = ToFloatBinaryFormatting( To<uint, float>( expected.i ) );
					string xorAsBin = ToFloatBinaryFormatting( To<uint, float>( resultI ^ expected.i ) );
					ReportError( $"FAILED {operationName} on test {currentTest}\n\t{expectedAsBin}({expected.f:G9}) expected\n\t{resultAsBin}({result:G9}) got\n\t{xorAsBin} XOR" );
					if( _failedOperations.Contains( operationName ) == false )
						_failedOperations.Add( operationName );
				}
			}
			else
			{
				string resultAsBin = ToFloatBinaryFormatting( result );
				_infoOut?.WriteLine( $"{operationName} {resultAsBin} {result:G9}" );
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
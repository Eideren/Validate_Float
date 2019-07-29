namespace ValidateFloat
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;

	class Program
	{
		const int AMOUNT_OF_RANDOM_FLOATS = 1024;
		
		static void Main( string[] args )
		{
			Console.WriteLine( "Beginning validation in ~3 seconds, press any key to access advanced features" );

			bool validate = true;
			for( int i = 3; i > 0; i-- )
			{
				if( Console.KeyAvailable )
				{
					validate = false;
					break;
				}
				Console.WriteLine( $"{i}..." );
				Thread.Sleep( 1000 );
			}
			
			Console.WriteLine( "Starting" );

			if( validate == false )
			{
				ASK_AGAIN:
				Console.WriteLine( "0: Print test tables" );
				Console.WriteLine( $"1: Print {AMOUNT_OF_RANDOM_FLOATS} random unique valid floats in binary" );
				switch( Console.ReadLine() )
				{
					case "0":
					{
						using( var writer = new StringWriter() )
						{
							new Test( Test.Mode.PrintTables, Console.Error, writer );

							string path = Path.Combine( Environment.CurrentDirectory, "Table.txt" );
							
							File.WriteAllText( path, writer.ToString() );
							Console.WriteLine( $"Written table to {path}" );
						}
						
						break;
					}

					case "1":
					{
						Random rng = new Random();
						byte[] bytes = new byte[ sizeof(float) ];
						HashSet<uint> generatedValues = new HashSet<uint>();
						for( int i = 0; i < AMOUNT_OF_RANDOM_FLOATS; i++ )
						{
							rng.NextBytes( bytes );

							float f = default;
							unsafe
							{
								for( int j = 0; j < sizeof(float); j++ )
									((byte*) & f )[ j ] = bytes[ j ];
							}

							if( float.IsNaN( f ) || float.IsInfinity( f ) )
							{
								// Kill one of the NaN/infinity bits
								uint mask;
								switch( rng.Next(0, 7+1) )
								{
									// We could bit shift here instead but w/e
									case 0: mask = 0b0_10000000_00000000000000000000000; break;
									case 1: mask = 0b0_01000000_00000000000000000000000; break;
									case 2: mask = 0b0_00100000_00000000000000000000000; break;
									case 3: mask = 0b0_00010000_00000000000000000000000; break;
									case 4: mask = 0b0_00001000_00000000000000000000000; break;
									case 5: mask = 0b0_00000100_00000000000000000000000; break;
									case 6: mask = 0b0_00000010_00000000000000000000000; break;
									case 7: mask = 0b0_00000001_00000000000000000000000; break;
									default: throw new Exception();
								}

								f = Utility.To<uint, float>( Utility.To<float, uint>( f ) & ~mask );
							}
							
							// Try again
							if( generatedValues.Contains( Utility.To<float, uint>( f ) ) )
								i--;
							else
							{
								generatedValues.Add( Utility.To<float, uint>( f ) );
								Console.WriteLine( $"( {Utility.FloatToSpecializedFormatting( f )}, new (string, uint, float)[0] )," );
							}
						}
						
						break;
					}
					default:
						Console.WriteLine("Invalid input.");
						goto ASK_AGAIN;
				}
			}
			else
			{
				var test = new Test( Test.Mode.Validate, Console.Error, Console.Out);
				if( test.RaisedErrors )
					Console.WriteLine( $"Failed operations:\n\t{string.Join( "\n\t", test.GetFailedOperations() )}" );
				else
					Console.WriteLine( "Tests passed" );
			}
			
			Console.WriteLine( "Press enter, return or close this window to exit" );
			Console.ReadLine();
		}
	}
}
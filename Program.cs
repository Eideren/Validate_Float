namespace ValidateFloat
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Threading;

	class Program
	{
		const int AMOUNT_OF_RANDOM_FLOATS = 8192;
		
		static void Main( string[] args )
		{
			Console.WriteLine( $"Starting up on {RuntimeInformation.OSDescription}/{RuntimeInformation.OSArchitecture} {RuntimeInformation.FrameworkDescription}/{RuntimeInformation.ProcessArchitecture}" );
			Console.WriteLine( "Loading table ..." );
			RETRY:
			Table table = null;
			try
			{
				table = ResultTable.GetTableFromFile();
			}
			catch( FileNotFoundException e )
			{
				Console.WriteLine( $"{nameof(FileNotFoundException)} while fetching table, trying to regenerate one ..." );
				GenerateTable();
				goto RETRY;
			}
			catch( Exception e )
			{
				Console.WriteLine( $"Exception while fetching table: {e}\nPress any key to continue" );
				Console.ReadKey();
			}

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
				Console.WriteLine( "0: Overwrite/create comparison table with this hardware's results" );
				Console.WriteLine( "1: Print test table" );
				Console.WriteLine( $"2: Print {AMOUNT_OF_RANDOM_FLOATS} pseudo random unique floats in binary format" );
				switch( Console.ReadLine() )
				{
					case "2":
					{
						using( var writer = new StringWriter() )
						{
							new Test( Test.Mode.PrintTables, Console.Error, writer, table );

							string path = Path.Combine( Environment.CurrentDirectory, "Table.txt" );
							
							File.WriteAllText( path, writer.ToString() );
							Console.WriteLine( $"Written table to {path}" );
						}
						
						break;
					}

					case "1":
					{
						foreach( uint u in PRandomTable() )
						{
							Console.WriteLine( Utility.ToFloatBinaryFormatting( u ) );
						}
						
						break;
					}

					case "0":
					{
						GenerateTable();
						break;
					}
					default:
						Console.WriteLine("Invalid input.");
						goto ASK_AGAIN;
				}
			}
			else
			{
				var test = new Test( Test.Mode.Validate, Console.Error, Console.Out, table );
				if( test.RaisedErrors )
					Console.WriteLine( $"Failed operations:\n\t{string.Join( "\n\t", test.GetFailedOperations() )}" );
				else
					Console.WriteLine( "Tests passed" );
			}
			
			Console.WriteLine( "Press enter, return or close this window to exit" );
			Console.ReadLine();
		}



		static void GenerateTable()
		{
			var pRandom = PRandomTable();
			var data = new (uint initialValue, (string operationName, uint i, float f)[] results)[ pRandom.Length ];
			for( int i = 0; i < pRandom.Length; i++ )
			{
				data[ i ].initialValue = pRandom[ i ];
			}
						
			using( var writer = new StringWriter() )
			{
				writer.WriteLine( $"// GENERATED FROM {RuntimeInformation.OSDescription}/{RuntimeInformation.OSArchitecture} {RuntimeInformation.FrameworkDescription}/{RuntimeInformation.ProcessArchitecture}" );
				new Test( Test.Mode.PrintTables, Console.Error, writer, new Table
				{
					Data = data
				} );
				ResultTable.OverwriteCompTableWith( writer.ToString() );
			}
		}



		static uint[] PRandomTable()
		{
			var output = new uint[ AMOUNT_OF_RANDOM_FLOATS ];
			Random rng = new Random(0);
			byte[] bytes = new byte[ sizeof(float) ];
			HashSet<uint> generatedValues = new HashSet<uint>();
			for( int i = 0; i < AMOUNT_OF_RANDOM_FLOATS; i++ )
			{
				rng.NextBytes( bytes );
				uint value = default;
				unsafe
				{
					for( int j = 0; j < sizeof(float); j++ )
						((byte*) & value )[ j ] = bytes[ j ];
				}
				
				if( generatedValues.Contains( value ) )
				{
					// Try again
					i--;
				}
				else
				{
					generatedValues.Add( value );
					output[ i ] = value;
				}
			}

			return output;
		}
	}
}

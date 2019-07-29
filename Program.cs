namespace ValidateFloat
{
	using System;
	using System.Threading;

	class Program
	{
		static void Main( string[] args )
		{
			Console.WriteLine( "Beginning validation in ~3 seconds, press any key to print the test tables instead" );

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
			
			new Test( validate, Console.Error, Console.Out, out bool error );

			Console.WriteLine( error ? "Error raised" : "Test passed" );
			Console.WriteLine( "Press enter, return or close this window to exit" );
			Console.ReadLine();
		}
	}
}
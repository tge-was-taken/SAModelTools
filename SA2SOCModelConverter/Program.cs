using System;
using System.IO;
using SAModelLibrary.SA2.SOC;

namespace SA2SOCModelConverter
{
    internal static class Program
    {
        private static void Main( string[] args )
        {
            if ( args.Length == 0 )
            {
                Console.WriteLine( "Missing path to input file.\n" );
                Console.WriteLine( "SA2SOCModelConverter 1.0 by TGE" );
                Console.WriteLine( "Usage:" );
                Console.WriteLine( "SA2SOCModelConverter <path to model file>                                   Export the model as Collada DAE." );
                Console.WriteLine( "SA2SOCModelConverter <path to OBJ, DAE, FBX> [-disable-conformance-mode]    Import the model and save it as a SOC model." );
                Console.WriteLine();
                return;
            }

            var filepath = args[ 0 ];
            var extension = Path.GetExtension( filepath );
            if ( string.IsNullOrEmpty( extension ) )
            {
                // Export
                TryCatch( () =>
                {
                    var model = new Model( filepath );
                    model.ExportCollada( Path.ChangeExtension( filepath, "dae" ) );
                }, e =>
                {
                    Console.WriteLine( "Failed to export model:" );
                    Console.WriteLine( e );
                });
            }
            else
            {
                // Import
                bool enableConformanceMode = !( args.Length > 1 && args[ 1 ] == "-disable-conformance-mode" );

                TryCatch( () =>
                {
                    var model = Model.Import( filepath, enableConformanceMode );
                    model.Save( Path.ChangeExtension( filepath, null ) );
                }, e =>
                {
                    Console.WriteLine( "Failed to import model:" );
                    Console.WriteLine( e );
                });
            }
        }

        private static bool TryCatch( Action action, Action<Exception> exceptionHandler )
        {
#if !DEBUG
            try
#endif
            {
                action();
            }
#if !DEBUG
            catch ( Exception e )
            {
                exceptionHandler( e );
                return false;
            }
#endif

            return true;
        }
    }
}

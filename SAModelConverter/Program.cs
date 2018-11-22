using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;

using SAModelLibrary;
using SAModelLibrary.GeometryFormats.Basic;
using SAModelLibrary.GeometryFormats.Chunk;
using SAModelLibrary.GeometryFormats.GC;
using SAModelLibrary.IO;
using SAModelLibrary.SA1;
using SAModelLibrary.SA2;

using FraGag.Compression;
using PeNet;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using IniParser;

namespace SAModelExporter
{
    internal static partial class Program
    {
        private const int SA2PC_BASE_OFFSET = 0x402600;

        public static string Usage = @"
SAModelExporter 0.1 by TGE.
Model exporter for SA1 and SA2.

Usage:
SAModelExporter <input filename> <args> <opts>

Supported inputs:                               Notes:
    - .sa1mdl
    - .sa2mdl
    - .nj
    - CHRMODELS.dll / CHRMODELS_orig.dll
    - Data_DLL.dll / Data_DLL_orig.dll
    - sonic.exe (stage models)
    - sonic2app.exe / sonic2app_decrypted.exe           death zones only
    - .ini (SAModel split list)                         requires path to file to be split as second argument
    - SA2 model list (...mdl.bin)
    - any binary (for scanning)

Supported options
    -base <int: base offset>
    -scan                                               Example: SAModelExporter sonic2app.exe -base 0x402600 -scan
";

        public static int BaseOffset;
        public static bool Scan;
        public static bool ScanRootNodeOnly;

        private static void Main( string[] args )
        {
            if ( args.Length == 0 )
            {
                Console.WriteLine( "Missing filename." );
                Console.WriteLine( Usage );
                return;
            }

            var inputFileName = args[0];
            var additionalArgs = ParseArgs( args );

            if ( !TryExportModelFile( inputFileName, additionalArgs ) )
            {
                Console.WriteLine( "Failed to export model file(s)." );
            }
            else
            {
                Console.WriteLine( "Model file(s) exported successfully" );
            }
        }

        private static List<string> ParseArgs( string[] args )
        {          
            var additionalArgs = new List<string>();

            for ( var i = 1; i < args.Length; i++ )
            {
                var arg = args[i];
                var param = i + 1 < args.Length ? args[i + 1] : null;

                if ( arg.StartsWith( "-" ) )
                {
                    // Option
                    switch ( arg )
                    {
                        case "-base":
                            {
                                if ( param == null )
                                {
                                    Console.WriteLine( "Missing base offset parameter" );
                                    continue;
                                }

                                if ( !TryParseInt( param, out var baseOffset ) )
                                {
                                    Console.WriteLine( "Invalid base offset parameter" );
                                    continue;
                                }

                                BaseOffset = baseOffset;
                            }
                            break;

                        case "-scan":
                            {
                                Scan = true;
                                if ( param == "true" )
                                    ScanRootNodeOnly = true;
                            }
                            break;
                    }
                }
                else
                {
                    additionalArgs.Add( arg );
                }
            }

            return additionalArgs;
        }

        private static bool TryParseInt( string str, out int value )
        {
            var isHex = false;

            if ( str.StartsWith( "0x" ) || str.Any( char.IsLetter ) )
            {
                isHex = true;
                str = str.Substring( 2 );
            }

            var style = NumberStyles.Number;
            if ( isHex )
                style = NumberStyles.HexNumber;

            return int.TryParse( str, style, CultureInfo.InvariantCulture, out value );
        }

        private static bool TryExportModelFile( string filepath, List<string> args )
        {
            var directory = Path.GetDirectoryName( filepath );
            var filename = Path.GetFileNameWithoutExtension( filepath );
            var extension = Path.GetExtension( filepath ).ToLowerInvariant();
            var outDirectory = Path.Combine( directory, filename );
            Directory.CreateDirectory( outDirectory );

            var stream = OpenMaybePRSFile( filepath );

            switch ( extension )
            {
                case ".sa1mdl":
                    ExportSAModelModel( stream, GeometryFormat.Basic, Path.Combine( outDirectory, $"{filename}.dae" ) );
                    return true;

                case ".sa2mdl":
                    ExportSAModelModel( stream, GeometryFormat.Chunk, Path.Combine( outDirectory, $"{filename}.dae" ) );
                    return true;

                case ".dll":
                    {
                        if ( filename.Equals( "CHRMODELS", StringComparison.InvariantCultureIgnoreCase ) ||
                             filename.Equals( "CHRMODELS_orig", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            ExtractSADXChrModelsDll( filepath, outDirectory );
                            return true;
                        }
                        else if ( filename.Equals( "Data_DLL", StringComparison.InvariantCultureIgnoreCase ) ||
                                  filename.Equals( "Data_DLL_orig", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            ExtractSA2PCDataDll( filepath, outDirectory );
                            return true;
                        }
                    }
                    break;
                case ".exe":
                    {
                        if ( filename.Equals( "sonic", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            ExtractSADXSonicExe( filepath, outDirectory );
                            return true;
                        }
                        else if ( filename.Equals( "sonic2app",           StringComparison.InvariantCultureIgnoreCase ) ||
                                  filename.Equals( "sonic2app_decrypted", StringComparison.InvariantCultureIgnoreCase ) )
                        {
                            ExtractSA2PCExe( filepath, outDirectory );
                            return true;
                        }
                    }
                    break;
                case ".ini":
                    {
                        if ( args.Count == 0 )
                        {
                            Console.WriteLine( "Missing path to file to split" );
                            return false;
                        }

                        ExtractSAModelSplitIni( filepath, args[ 0 ], outDirectory );
                        return true;
                    }
            }

            // Assume binary model
            var success = false;

            try
            {
                if ( TryLoadAs( stream, x => new ModelList( x, true ), out var modelList ) )
                {
                    success = ExportSA2ModelList( directory, filename, outDirectory, modelList );
                }
                else if ( TryLoadAs( stream, x => new Node( x, true, GeometryFormat.Unknown ), out var rootNode ) )
                {
                    success = TryExportModelRootNode( filename, outDirectory, rootNode );
                }
            }
            catch ( Exception )
            {
                Console.WriteLine( "Error occured while detecting file type." );
                success = false;
            }

            if ( Scan )
            {
                ScanAndExport( filepath, outDirectory, BaseOffset, ScanRootNodeOnly );
                success = true;
            }

            return success;
        }

        private static void ScanAndExport( string filePath, string outDirectory, int baseOffset, bool tryFindRootNodes )
        {
            var nodes = new HashSet<Node>();
            using ( var reader = new EndianBinaryReader( filePath, Endianness.Little ) )
            {
                reader.BaseOffset = -baseOffset;
                //reader.Position = 0x471C2C;
                //reader.Position = 0xAF0F2C - baseOffset;

                while ( reader.Position < reader.Length )
                {
                    if ( !Node.Validate( reader ) )
                    {
                        reader.SeekCurrent( 4 );
                        continue;
                    }

                    var nodePosition = reader.Position;

                    try
                    {
                        var node = reader.ReadObject<Node>();
                        Console.WriteLine( $"Found node at {nodePosition:X8} ({( node.SourceOffset + baseOffset ):X8})" );
                        nodes.Add( node );
                    }
                    catch
                    {
                        reader.SeekBegin( nodePosition + 4 );
                    }

                    reader.SeekBegin( nodePosition + 4 );
                }
            }

            if ( tryFindRootNodes )
            {
                var remainingNodes = new HashSet<Node>( nodes );
                foreach ( var node in nodes.Where( x => x.Child != null ) )
                {
                    foreach ( var otherNode in node.EnumerateAllNodes( false ) )
                        remainingNodes.Remove( otherNode );
                }

                nodes = remainingNodes;
            }

            var index = 0;
            foreach ( var node in nodes )
            {
                var geometryFormat = node.EnumerateAllNodes().FirstOrDefault( x => x.Geometry != null )?.Geometry.Format;
                if ( !geometryFormat.HasValue )
                {
                    // No geometry
                    continue;
                }

                Console.WriteLine( $"Exporting node {node.SourceOffset:X8} ({( node.SourceOffset + baseOffset ):X8})" );
                var fileName = $"{index}_{Path.GetFileNameWithoutExtension( filePath )}_node_{geometryFormat}_{( node.SourceOffset + baseOffset ):X8}";
                if ( TryExportModelRootNode( fileName,
                                              outDirectory, node ) )
                {
                    ++index;
                    Console.WriteLine( $"Exported {fileName}.dae" );
                }
                else
                {
                    Console.WriteLine( "Failed to export" );
                }
            }

            var nodeGeometryLookup = nodes.Where( x => x.Geometry != null ).Select( x => x.Geometry ).ToDictionary( x => x.SourceOffset );
            var geometries = new HashSet<IGeometry>();
            using ( var reader = new EndianBinaryReader( filePath, Endianness.Little ) )
            {
                reader.BaseOffset = -baseOffset;
                //reader.Position   = 0x471C2C;
                //reader.Position = 0xA00000;

                while ( reader.Position < reader.Length )
                {
                    var geometryPosition = reader.Position;
                    if ( nodeGeometryLookup.ContainsKey( geometryPosition ) )
                    {
                        // Already read as node
                        reader.SeekCurrent( 4 );
                        continue;
                    }

                    IGeometry geometry = null;

                    bool TryReadGeometry( GeometryFormat testFormat )
                    {
                        reader.Seek( geometryPosition, SeekOrigin.Begin );

                        switch ( testFormat )
                        {
                            case GeometryFormat.Basic:
                            case GeometryFormat.BasicDX:
                                if ( !SAModelLibrary.GeometryFormats.Basic.Geometry.Validate( reader ) )
                                    return false;

                                try
                                {
                                    geometry =
                                        reader.ReadObject<SAModelLibrary.GeometryFormats.Basic.Geometry>( testFormat == GeometryFormat.BasicDX );
                                }
                                catch
                                {
                                    return false;
                                }

                                return true;

                            case GeometryFormat.Chunk:
                                if ( !SAModelLibrary.GeometryFormats.Chunk.Geometry.Validate( reader ) )
                                    return false;

                                try
                                {
                                    geometry = reader.ReadObject<SAModelLibrary.GeometryFormats.Chunk.Geometry>();
                                }
                                catch
                                {
                                    return false;
                                }
                                    
                                return true;

                            case GeometryFormat.GC:
                                if ( !SAModelLibrary.GeometryFormats.GC.Geometry.Validate( reader ) )
                                    return false;

                                try
                                {
                                    geometry = reader.ReadObject<SAModelLibrary.GeometryFormats.GC.Geometry>();
                                }
                                catch
                                {
                                    return false;
                                }
                                    
                                return true;

                            default: return false;
                        }
                    }

                    bool success = TryReadGeometry( GeometryFormat.Basic );
                    if ( !success )
                        success = TryReadGeometry( GeometryFormat.BasicDX );
                    if ( !success )
                        success = TryReadGeometry( GeometryFormat.Chunk );
                    if ( !success )
                        success = TryReadGeometry( GeometryFormat.GC );

                    if ( !success )
                    {
                        reader.SeekBegin( geometryPosition + 4 );
                        continue;
                    }

                    if ( geometry != null )
                    {
                        Console.WriteLine( $"Found geometry at {geometryPosition:X8} ({( geometryPosition + baseOffset ):X8})" );
                        geometries.Add( geometry );
                    }

                    reader.SeekBegin( geometryPosition + 4 );
                }
            }

            index = 0;
            foreach ( var geometry in geometries )
            {
                Console.WriteLine( $"Exporting geometry {geometry.SourceOffset:X8} ({( geometry.SourceOffset + baseOffset ):X8})" );

                var fileName = $"{index}_{Path.GetFileNameWithoutExtension( filePath )}_geometry_{geometry.Format}_{( geometry.SourceOffset + baseOffset ):X8}";
                if ( TryExportModelRootNode( fileName,
                                             outDirectory, new Node() { Geometry = geometry } ))
                {
                    ++index;
                    Console.WriteLine( $"Exported {fileName}.dae" );
                }
                else
                {
                    Console.WriteLine( "Failed to export" );
                }
            }
        }

        private static void ExtractSA2PCExe( string filepath, string outDirectory )
        {
            BaseOffset = SA2PC_BASE_OFFSET;

            var deathZoneAddresses = new Dictionary<int, int>()
            {
                { 3, 0x01089260 },
                { 4, 0x00EAE574 },
                { 5, 0x0168B944 },
                { 6, 0x0117F6E8 },
                { 8, 0x00E8F9B0 },
                { 9, 0x0104C6E4 },
                { 10, 0x00B06570 },
                { 11, 0x00C040D0 },
                { 12, 0x009ED904 },
                { 13, 0x010DC678 },
                { 14, 0x015A28A0 },
                { 15, 0x00DCB6D0 },
                { 16, 0x00BED518 },
                { 17, 0x011BA410 },
                { 18, 0x00E5DD9C },
                { 19, 0x0100BD34 },
                { 20, 0x00EF46C0 },
                { 21, 0x00D022C0 },
                { 22, 0x009C8178 },
                { 23, 0x00A98528 },
                { 24, 0x009A153C },
                { 25, 0x00BAFDA0 },
                { 26, 0x00C4F040 },
                { 27, 0x00A06CB0 },
                { 28, 0x00AE0478 },
                { 29, 0x01AEE2C0 },
                { 30, 0x016C6C70 },
                { 31, 0x010F2F88 },
                { 32, 0x01165250 },
                { 34, 0x01650E54 },
                { 35, 0x0145CFC8 },
                { 36, 0x00E71200 },
                { 37, 0x01613B2C },
                { 38, 0x00CCE7D0 },
                { 39, 0x009709E8 },
                { 40, 0x01553AE0 },
                { 41, 0x01676570 },
                { 42, 0x01A5A61C },
                { 43, 0x00EDCCB0 },
                { 44, 0x00DBB114 },
                { 45, 0x014157FC },
                { 46, 0x0156C0C4 },
                { 48, 0x00D866C4 },
                { 50, 0x00C86D54 },
                { 51, 0x00972174 },
                { 52, 0x01A5AE60 },
                { 53, 0x00EF2F28 },
                { 54, 0x01445710 },
                { 55, 0x01AEFBF0 },
                { 57, 0x00DBDC04 },
                { 58, 0x01369710 },
                { 59, 0x00DC8DB8 },
            };

            using ( var reader = new EndianBinaryReader( filepath, Endianness.Little ) )
            {
                foreach ( var stageDeathZone in deathZoneAddresses )
                {
                    reader.SeekBegin( stageDeathZone.Value - BaseOffset );
                    reader.BaseOffset = -BaseOffset;

                    var deathZones = reader.ReadObject<DeathZoneList>();
                    DeathZoneListAssimpExporter.Default.Export( deathZones,
                                                                Path.Combine( outDirectory, $"stg{stageDeathZone.Key:D2}DeathZones.dae" ) );
                }
            }

            if ( Scan )
                ScanAndExport( filepath, outDirectory, BaseOffset, ScanRootNodeOnly );
        }

        private static void ExtractSAModelSplitIni( string filepath, string fileToSplitPath, string outDirectory )
        {
            var iniParser = new FileIniDataParser();
            var ini = iniParser.ReadFile( filepath );

            // Parse ini contents
            using ( var stream = File.OpenRead( fileToSplitPath ) )
            using ( var reader = new EndianBinaryReader( stream, true, Endianness.Little ) )
            {
                var game = ini.Global["game"];
                if ( game != "SA2B" )
                    throw new NotImplementedException( $"Only SA2B is implemented" );

                BaseOffset = SA2PC_BASE_OFFSET;
                reader.BaseOffset = -BaseOffset;

                var systemFolder = ini.Global["systemfolder"];
                var musicfolder = ini.Global["musicfolder"];

                foreach ( var section in ini.Sections )
                {
                    var type = section.Keys["type"];
                    var address = int.Parse( section.Keys["address"], System.Globalization.NumberStyles.HexNumber );
                    var filename = section.Keys["filename"];

                    if ( type != "chunkmodel" )
                        continue;

                    var outFilePath = Path.Combine( outDirectory, filename );
                    var outFileName = Path.GetFileNameWithoutExtension( outFilePath );
                    var outFileDirectory = Path.GetDirectoryName( outFilePath );
                    Directory.CreateDirectory( outFileDirectory );

                    // Try to read root node
                    reader.Position = ( 0x400000 + address ) - BaseOffset;
                    Node rootNode;
                    try
                    {
                        rootNode = reader.ReadObject<Node>( new NodeReadContext( GeometryFormat.Chunk ) );
                    }
                    catch ( Exception e )
                    {
                        Console.WriteLine( $"Failed to read {filename}" );
                        continue;
                    }

                    if ( TryExportModelRootNode( outFileName, outFileDirectory, rootNode ) )
                    {
                        Console.WriteLine( $"Exported {section.SectionName}" );
                    }
                    else
                    {
                        Console.WriteLine( $"Failed to export {filename}" );
                    }
                }
            }
        }

        private static void ExtractSADXSonicExe( string filepath, string outDirectory )
        {
            BaseOffset = 0x400000;

            var stages = new[]
            {
                new SADXStageInfo(0x23C7BCC, "HAMMER", "Hedgehog Hammer"),
                new SADXStageInfo(0xA99CB8, "BEACH01", "Emerald Coast 1"),
                new SADXStageInfo(0xC39E9C, "BEACH02", "Emerald Coast 2"),
                new SADXStageInfo(0xC386B4, "BEACH03", "Emerald Coast 3"),
                new SADXStageInfo(0x8051E0, "WINDY01", "Windy Valley 1"),
                new SADXStageInfo(0x8046C0, "WINDY02", "Windy Valley 2"),
                new SADXStageInfo(0x80433C, "WINDY03", "Windy Valley 3"),
                new SADXStageInfo(0x22B975C, "TWINKLE01", "Twinkle Park 1"),
                new SADXStageInfo(0x22B867C, "TWINKLE02", "Twinkle Park 2"),
                new SADXStageInfo(0x22B6B34, "TWINKLE03", "Twinkle Park 3"),
                new SADXStageInfo(0x22B1E98, "HIGHWAY01", "Speed Highway 1"),
                new SADXStageInfo(0x22ADBC8, "HIGHWAY02", "Speed Highway 2"),
                new SADXStageInfo(0x22ACF40, "HIGHWAY03", "Speed Highway 3"),
                new SADXStageInfo(0x1E405E0, "MOUNTAIN01", "Red Mountain 1"),
                new SADXStageInfo(0x20C8B58, "MOUNTAIN02", "Red Mountain 2"),
                new SADXStageInfo(0x20C6F14, "MOUNTAIN03", "Red Mountain 3"),
                new SADXStageInfo(0x1E39ADC, "SKYDECK01", "Sky Deck 1"),
                new SADXStageInfo(0x1E369A0, "SKYDECK02", "Sky Deck 2"),
                new SADXStageInfo(0x1E34800, "SKYDECK03", "Sky Deck 3"),
                new SADXStageInfo(0x1C38B60, null, "Lost World 1"),
                new SADXStageInfo(0x1C37C9C, null, "Lost World 2"),
                new SADXStageInfo(0x1C34D14, null, "Lost World 3"),
                new SADXStageInfo(0xA41D2C, "ICECAP01", "Icecap 1"),
                new SADXStageInfo(0xA414BC, "ICECAP02", "Icecap 2"),
                new SADXStageInfo(0xA409C4, "ICECAP03", "Icecap 3"),
                new SADXStageInfo(0xA3E024, "ICECAP04", "Icecap 4"),
                new SADXStageInfo(0x198A9D0, "CASINO01", "Casinopolis 1"),
                new SADXStageInfo(0x19887EC, "CASINO02", "Casinopolis 2"),
                new SADXStageInfo(0x1986A1C, "CASINO03", "Casinopolis 3"),
                new SADXStageInfo(0x1985C08, "CASINO04", "Casinopolis 4"),
                new SADXStageInfo(0x16600D0, "FINALEGG1", "Final Egg 1"),
                new SADXStageInfo(0x15C8ED0, "FINALEGG2", "Final Egg 2"),
                new SADXStageInfo(0x165CFA8, "FINALEGG3", "Final Egg 3"),
                new SADXStageInfo(0x13D09C0, "HOTSHELTER1", "Hot Shelter 1"),
                new SADXStageInfo(0x13CF288, "HOTSHELTER2", "Hot Shelter 2"),
                new SADXStageInfo(0x13C9B48, "HOTSHELTER3", "Hot Shelter 3"),
                new SADXStageInfo(0xD2136C, "CHAOS2", "Chaos 2"),
                new SADXStageInfo(0xD90930, null, "Chaos 4"),
                new SADXStageInfo(0xDEDE38, "CHAOS6", "Chaos 6 (Sonic)"),
                new SADXStageInfo(0xDED6F0, "CHAOS6", "Chaos 6 (Knuckles)"),
                new SADXStageInfo(0x102478C, null, "Perfect Chaos"),
                new SADXStageInfo(0x1170B1C, null, "Egg Hornet"),
                new SADXStageInfo(0x11EC454, null, "Egg Walker"),
                new SADXStageInfo(0x125E990, null, "Egg Viper"),
                new SADXStageInfo(0x12B4D38, null, "ZERO"),
                new SADXStageInfo(0x10FCEE8, null, "E-101 Beta"),
                new SADXStageInfo(0x1122578, null, "E-101mkII"),
                new SADXStageInfo(0x5C99A4, null, "Twinkle Circuit"),
                new SADXStageInfo(0x5C8170, null, "Twinkle Circuit 1"),
                new SADXStageInfo(0x5C6C30, null, "Twinkle Circuit 2"),
                new SADXStageInfo(0x5C585C, null, "Twinkle Circuit 3"),
                new SADXStageInfo(0x5C453C, null, "Twinkle Circuit 4"),
                new SADXStageInfo(0x5C3534, null, "Twinkle Circuit 5"),
                new SADXStageInfo(0x133EB64, "SANDBOARD", "Sand Hill"),
                new SADXStageInfo(0x300E738, null, "Station Square Chao Garden"),
                new SADXStageInfo(0x3005E54, null, "Egg Carrier Chao Garden"),
                new SADXStageInfo(0x3023700, null, "Chao Race Entry"),
                new SADXStageInfo(0x3024C58, null, "Chao Race"),
                new SADXStageInfo(0x2FCAC58, null, "Black Market"),
            };

            using ( var reader =
                new EndianBinaryReader( filepath, Endianness.Little ) )
            {
                reader.BaseOffset = BaseOffset;

                var systemDirectory = Path.Combine( Path.GetDirectoryName( filepath ), "system" );

                foreach ( var stage in stages )
                {
                    Console.WriteLine( $"Extracting {stage.Name}" );

                    reader.SeekBegin( stage.Offset );
                    var landTable = reader.ReadObject<LandTableSA1>();

                    var stageOutDirectory = Path.Combine( outDirectory, stage.Name );
                    Directory.CreateDirectory( stageOutDirectory );
                    LandTableSA1AssimpExporter.Default.Export( landTable, Path.Combine( stageOutDirectory, $"{stage.Name}.dae" ) );

                    if ( stage.TexturePakFileName != null )
                    {
                        var texturePakFileName = Path.Combine( systemDirectory, stage.TexturePakFileName + ".PVM" );
                        if ( File.Exists( texturePakFileName ) )
                            LoadAndExtractTexturePak( File.OpenRead( texturePakFileName ), stageOutDirectory );
                    }
                }
            }

            if ( Scan )
                ScanAndExport( filepath, outDirectory, BaseOffset, ScanRootNodeOnly );
        }

        private static void ExtractSA2PCDataDll( string filepath, string outDirectory )
        {
            var gcPcDirectory = Path.Combine( Path.GetDirectoryName( Path.GetDirectoryName( Path.GetDirectoryName( filepath ) ) ) );
            var library = new PeFile( filepath );
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = filepath;

                var rdataBaseOffset = ( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x1200 );
                var dataBaseOffset = ( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x2000 );
                BaseOffset = ( int )dataBaseOffset;

                var landTablesExports = library.ExportedFunctions.Where( x => x.Name.StartsWith( "objLandTable" ) );
                foreach ( var landTableExport in landTablesExports )
                {
                    Console.WriteLine( $"Extracting {landTableExport.Name}" );

                    var landTableOffset = landTableExport.Address;

                    reader.SeekBegin( landTableOffset - 0x2000 );
                    reader.BaseOffset = -BaseOffset;

                    var landTable = reader.ReadObject<LandTableSA2>();

                    var stageOutDirectory = Path.Combine( outDirectory, landTableExport.Name );
                    Directory.CreateDirectory( stageOutDirectory );
                    LandTableSA2AssimpExporter.Default.Export( landTable, Path.Combine( stageOutDirectory, $"{landTableExport.Name}.dae" ) );

                    if ( landTable.TexturePakFileName == null && landTableExport.Name.Length == 16 && int.TryParse( landTableExport.Name.Substring( 12 ), out var levelId ) )
                    {
                        // Try to guess it
                        landTable.TexturePakFileName = $"landtx{levelId}";
                    }

                    if ( landTable.TexturePakFileName != null )
                    {
                        var texturePakFileName = Path.Combine( gcPcDirectory, landTable.TexturePakFileName + ".prs" );
                        if ( File.Exists( texturePakFileName ) )
                        {
                            var texturePakStream = OpenMaybePRSFile( texturePakFileName );
                            LoadAndExtractTexturePak( texturePakStream, stageOutDirectory );
                        }
                    }
                    else
                    {
                        Console.WriteLine( $"{landTableExport.Name} has no texture pak file name associated with it" );
                    }
                }

            }

            if ( Scan )
                ScanAndExport( filepath, outDirectory, BaseOffset, ScanRootNodeOnly );
        }

        private static void ExtractSADXChrModelsDll( string filepath, string outDirectory )
        {
            var library = new PeFile( filepath );
            BaseOffset = ( int ) library.ImageNtHeaders.OptionalHeader.ImageBase;
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = filepath;

                foreach ( var modelListExport in library.ExportedFunctions.Where( x => x.Name.EndsWith( "_OBJECTS" ) ) )
                {
                    var modelListOffset = modelListExport.Address;
                    var characterPrefix = modelListExport.Name.Replace( "_OBJECTS", "" );
                    var characterName = characterPrefix.ToLowerInvariant().Replace( "_", "" );

                    reader.SeekBegin( modelListOffset );
                    reader.BaseOffset = -BaseOffset;
                    int modelIndex = 0;
                    while ( true )
                    {
                        var offset = reader.ReadInt32();
                        if ( !reader.IsValidOffset( offset ) )
                            break;

                        var valid = true;
                        reader.ReadAtOffset( offset, () => valid = Node.Validate( reader ) );
                        if ( !valid )
                            break;

                        var rootNode = reader.ReadObjectAtOffset<Node>( offset, new NodeReadContext( GeometryFormat.BasicDX ) );
                        if ( rootNode == null )
                            continue;

                        var characterOutDirectory = Path.Combine( outDirectory, characterName );
                        Directory.CreateDirectory( characterOutDirectory );

                        var texturePakFileName = Path.Combine( Path.GetDirectoryName( filepath ), characterName + ".PVM" );
                        List<string> textures = null;

                        if ( File.Exists( texturePakFileName ) )
                            textures = LoadAndExtractTexturePak( File.OpenRead( texturePakFileName ), characterOutDirectory );

                        BasicAssimpExporter.Animated.Export( rootNode, Path.Combine( characterOutDirectory, $"{characterName}_{modelIndex:D2}.dae" ), textures );

                        ++modelIndex;
                    }
                }
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private static bool TryExportModelRootNode( string filename, string outDirectory, Node rootNode )
        {
            var geometryFormat = rootNode.EnumerateAllNodes().FirstOrDefault( x => x.Geometry != null )?.Geometry.Format;
            if ( !geometryFormat.HasValue )
            {
                Console.WriteLine( "Has no geometry." );
                return false;
            }

            var outFilename = Path.Combine( outDirectory, $"{filename}.dae" );

            try
            {
                switch ( geometryFormat )
                {
                    case GeometryFormat.Basic:
                    case GeometryFormat.BasicDX:
                        BasicAssimpExporter.Animated.Export( rootNode, outFilename );
                        return true;
                    case GeometryFormat.Chunk:
                        ChunkAssimpExporter.Animated.Export( rootNode, outFilename );
                        return true;
                    case GeometryFormat.GC:
                        GCAssimpExporter.Default.Export( rootNode, outFilename );
                        return true;
                    default:
                        Console.WriteLine( $"Unsupported geometry format: {geometryFormat}" );
                        return false;
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( "Invalid model data" );
                return false;
            }
        }

        private static bool ExportSA2ModelList( string directory, string filename, string outDirectory, ModelList modelList )
        {
            bool hasTextureNames = TryLoadAndExtractTexturePak( directory, filename, outDirectory, out var textureNames );

            foreach ( var model in modelList )
            {
                var modelFileName = $"{filename}_{model.UID:D3}.dae";

                if ( hasTextureNames )
                    model.Export( Path.Combine( outDirectory, modelFileName ), textureNames );
                else
                    model.Export( Path.Combine( outDirectory, modelFileName ) );
            }

            return true;
        }

        private static bool TryLoadAndExtractTexturePak( string directory, string filename, string outDirectory, out List<string> textureNames )
        {
            var texturePakPath = Path.ChangeExtension( Path.Combine( directory, filename.Replace( "mdl", "tex" ) ), "prs" );
            if ( !File.Exists( texturePakPath ) )
            {
                texturePakPath = Path.ChangeExtension( texturePakPath, "bin" );
                if ( !File.Exists( texturePakPath ) )
                {
                    textureNames = null;
                    return false;
                }
            }

            var texturePakStream = OpenMaybePRSFile( texturePakPath );
            textureNames = LoadAndExtractTexturePak( texturePakStream, outDirectory );

            return true;
        }

        private static List<string> LoadAndExtractTexturePak( Stream texturePakStream, string outDirectory )
        {
            var textureNames = new List<string>();

            try
            {
                var archive = new GvmArchive();
                var archiveReader = ( GvmArchiveReader )archive.Open( texturePakStream );
                foreach ( var entry in archiveReader.Entries )
                {
                    var texture = new GvrTexture();
                    texture.Read( entry.Open(), out var bitmap );

                    var entryNameClean = Path.ChangeExtension( entry.Name, null );
                    bitmap.Save( Path.Combine( outDirectory, Path.ChangeExtension( entryNameClean, "png" ) ) );
                    textureNames.Add( entryNameClean );
                }
            }
            catch ( Exception )
            {
                texturePakStream.Position = 0;

                var archive = new PvmArchive();
                var archiveReader = ( PvmArchiveReader )archive.Open( texturePakStream );
                foreach ( var entry in archiveReader.Entries )
                {
                    var texture = new PvrTexture();
                    texture.Read( entry.Open(), out var bitmap );
                    var entryNameClean = Path.ChangeExtension( entry.Name, null );

                    try
                    {
                        bitmap.Save( Path.Combine( outDirectory, Path.ChangeExtension( entryNameClean, "png" ) ) );
                    }
                    catch ( Exception e )
                    {
                        Console.WriteLine( e );
                    }

                    textureNames.Add( entryNameClean );
                }
            }

            return textureNames;
        }

        private static Stream OpenMaybePRSFile( string filepath )
        {
            Stream stream = File.OpenRead( filepath );

            try
            {
                var decompressedStream = new MemoryStream();
                Prs.Decompress( stream, decompressedStream );
                stream.Dispose();
                stream = decompressedStream;
            }
            catch ( Exception )
            {
                // Not compressed
            }

            stream.Position = 0;
            return stream;
        }

        private static void ExportSAModelModel( Stream stream, GeometryFormat format, string outFilePath )
        {
            using ( var reader = new EndianBinaryReader( stream, Endianness.Little ) )
            {
                reader.SeekBegin( 8 );
                var rootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( format ) );
                if ( rootNode == null )
                {
                    Console.WriteLine( "Invalid model file" );
                    return;
                }

                if ( format == GeometryFormat.Basic )
                {
                    BasicAssimpExporter.Animated.Export( rootNode, outFilePath );
                }
                else if ( format == GeometryFormat.Chunk )
                {
                    ChunkAssimpExporter.Animated.Export( rootNode, outFilePath );
                }
                else
                {
                    Debug.Assert( false );
                }
            }
        }

        private static bool TryLoadAs<T>( Stream stream, Func<Stream, T> func, out T result ) where T : class
        {
            try
            {
                result = func( stream );
            }
            catch ( Exception )
            {
                stream.Position = 0;
                result = null;
                return false;
            }

            return true;
        }
    }
}

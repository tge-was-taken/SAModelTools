using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAModelLibrary;
using SAModelLibrary.GeometryFormats.Basic;
using SAModelLibrary.GeometryFormats.Chunk;
using SAModelLibrary.IO;
using FraGag;
using FraGag.Compression;
using PeNet;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using SAModelLibrary.SA1;
using SAModelLibrary.SA2;

namespace SAModelExporter
{
    internal static class Program
    {
        public static string Usage = @"
SAModelExporter 0.0.2 by TGE.
Model exporter for SA1 and SA2.

Usage:
SAModelExporter <filename>
";

        private static void Main( string[] args )
        {
            if ( args.Length == 0 )
            {
                Console.WriteLine( "Missing filename." );
                Console.WriteLine( Usage );
                return;
            }

            if ( !TryExportModelFile( args[ 0 ] ) )
            {
                Console.WriteLine( "Failed to export model file." );
            }
            else
            {
                Console.WriteLine( "Model file exported successfully" );
            }
        }

        public static void PVMToGVM( string path, string outPath )
        {
            var pvm       = new PvmArchive();
            var pvmReader = ( PvmArchiveReader )pvm.Open( path );

            var  gvm       = new GvmArchive();
            var  gvmStream = new MemoryStream();
            var  gvmWriter = ( GvmArchiveWriter )gvm.Create( gvmStream );
            uint index     = 500;
            foreach ( var entry in pvmReader.Entries )
            {
                var pvr = new PvrTexture();
                pvr.Read( entry.Open(), out var bitmap );

                var gvr = new GvrTexture();
                gvr.GlobalIndex    = index++;
                gvr.HasGlobalIndex = true;
                gvr.Write( bitmap, out var gvrBytes );

                gvmWriter.CreateEntry( new MemoryStream( gvrBytes ), entry.Name );
                gvmWriter.HasGlobalIndexes = true;
            }

            gvmWriter.Flush();

            gvmStream.Position = 0;
            var prsGvmStream = new MemoryStream();
            Prs.Compress( gvmStream, prsGvmStream );

            prsGvmStream.Position = 0;
            using ( var outFile = File.Create( outPath ) )
                prsGvmStream.CopyTo( outFile );
        }

        public static bool TryExportModelFile( string filepath )
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
                    }
                    break;
            }

            try
            {
                if ( TryLoadAs( stream, x => new ModelList( x, true ), out var modelList ) )
                {
                    return ExtractSA2ModelList( directory, filename, outDirectory, modelList );
                }
                else if ( TryLoadAs( stream, x => new Node( x, true, GeometryFormat.Unknown ), out var rootNode ) )
                {
                    return TryExtractModelRootNode( filename, outDirectory, rootNode );
                }
            }
            catch ( Exception )
            {
                Console.WriteLine( "Error occured while detecting file type." );
                return false;
            }

            return false;
        }

        struct SADXStageInfo
        {
            public int Offset { get; }

            public string TexturePakFileName { get; }

            public string Name { get; }

            public SADXStageInfo(int offset, string texturePakFileName, string name)
            {
                Offset = offset;
                TexturePakFileName = texturePakFileName;
                Name = name;
            }
        }

        private static void ExtractSADXSonicExe( string filepath, string outDirectory )
        {
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
                reader.BaseOffset = -0x400000;

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
        }

        private static void ExtractSA2PCDataDll( string filepath, string outDirectory )
        {
            var gcPcDirectory = Path.Combine( Path.GetDirectoryName( Path.GetDirectoryName( Path.GetDirectoryName( filepath ) ) ) );
            var library    = new PeFile( filepath );
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = filepath;

                var rdataBaseOffset = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x1200 );
                var dataBaseOffset  = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x2000 );

                var landTablesExports = library.ExportedFunctions.Where( x => x.Name.StartsWith( "objLandTable" ) );
                foreach ( var landTableExport in landTablesExports )
                {
                    Console.WriteLine( $"Extracting {landTableExport.Name}" );

                    var landTableOffset = landTableExport.Address;

                    reader.SeekBegin( landTableOffset - 0x2000 );
                    reader.BaseOffset = dataBaseOffset;

                    var landTable = reader.ReadObject<LandTableSA2>();

                    var stageOutDirectory = Path.Combine( outDirectory, landTableExport.Name );
                    Directory.CreateDirectory( stageOutDirectory );
                    LandTableSA2AssimpExporter.Default.Export( landTable, Path.Combine( stageOutDirectory, $"{landTableExport.Name}.dae" ) );

                    switch ( landTableExport.Name )
                    {
                        case "objLandTable0003":
                            landTable.TexturePakFileName = "landtx03";
                            break;

                        case "objLandTable0015":
                            landTable.TexturePakFileName = "landtx15";
                            break;

                        case "objLandTable0052":
                            landTable.TexturePakFileName = "landtx52";
                            break;

                        case "objLandTable0053":
                            landTable.TexturePakFileName = "landtx53";
                            break;

                        case "objLandTable0055":
                            landTable.TexturePakFileName = "landtx55";
                            break;

                        case "objLandTable0058":
                            landTable.TexturePakFileName = "landtx58";
                            break;
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
                        Console.WriteLine( landTableExport.Name );
                    }
                }

            }
        }

        private static void ExtractSADXChrModelsDll( string filepath, string outDirectory )
        {
            var library = new PeFile( filepath );
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = filepath;

                foreach ( var modelListExport in library.ExportedFunctions.Where( x => x.Name.EndsWith( "_OBJECTS" ) ) )
                {
                    var modelListOffset = modelListExport.Address;
                    var characterPrefix = modelListExport.Name.Replace( "_OBJECTS", "" );
                    var characterName = characterPrefix.ToLowerInvariant().Replace( "_", "" );

                    reader.SeekBegin( modelListOffset );
                    reader.BaseOffset = -( long )library.ImageNtHeaders.OptionalHeader.ImageBase;
                    int i = 0;
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

                        BasicAssimpExporter.Animated.Export( rootNode, Path.Combine( characterOutDirectory, $"{characterName}_{i:D2}.dae" ), textures );

                        ++i;
                    }
                }
            }
        }

        private static bool TryExtractModelRootNode( string filename, string outDirectory, Node rootNode )
        {
            var geometryFormat = rootNode.EnumerateAllNodes().FirstOrDefault( x => x.Geometry != null )?.Geometry.Format;
            if ( !geometryFormat.HasValue )
            {
                Console.WriteLine( "Unable to detect geometry format." );
                return false;
            }

            if ( geometryFormat == GeometryFormat.Basic ||
                 geometryFormat == GeometryFormat.BasicDX )
            {
                BasicAssimpExporter.Animated.Export( rootNode, Path.Combine( outDirectory, $"{filename}.dae" ) );
                return true;
            }
            else if ( geometryFormat == GeometryFormat.Chunk )
            {
                ChunkAssimpExporter.Animated.Export( rootNode, Path.Combine( outDirectory, $"{filename}.dae" ) );
                return true;
            }
            else
            {
                Console.WriteLine( $"Unsupported geometry format: {geometryFormat}" );
                return false;
            }
        }

        private static bool ExtractSA2ModelList( string directory, string filename, string outDirectory, ModelList modelList )
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

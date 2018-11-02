using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using SAModelLibrary;
using SAModelLibrary.SA2;
using SAModelLibrary.IO;
using FraGag.Compression;
using PeNet;
using SAModelLibrary.GeometryFormats.Basic;
using SAModelLibrary.GeometryFormats.Chunk;
using Geometry = SAModelLibrary.GeometryFormats.Chunk.Geometry;

using Basic = SAModelLibrary.GeometryFormats.Basic;
using GC = SAModelLibrary.GeometryFormats.GC;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using SAModelLibrary.GeometryFormats;
using SAModelLibrary.Maths;
using SAModelLibrary.SA1;
using SAModelLibrary.Utils;

namespace SA2ModelConverter
{
    internal static class Program
    {
        private static void Main( string[] args )
        {
            CustomLandTableTests();
            return;

            //            using ( var reader = new EndianBinaryReader( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\sonic2app_decrypted.exe",
            //                                                         Endianness.Little ) )
            //            {
            //                reader.SeekBegin( 0x10DE8AC );
            //                reader.BaseOffset = -0x402600;
            //                //var modelRootNode = reader.ReadObject<Node>();
            //                //ChunkAssimpExporter.Animated.Export( modelRootNode, "bigmdl.dae" );


            //                List<SetObjectDefinition> SetObjectDefinitions = new List<SetObjectDefinition>
            //{
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "RING_LINEAR", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "RING_CIRCLE", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "SPRA   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb533c0L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb533c0L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SPRB   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb533c0L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb533c0L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "3SPRING", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb1df18L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "BIGJUMP   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb357a0L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb35dc4L ) }, } },
            //new SetObjectDefinition { Name = "KASOKU ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb4364cL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb43e1cL ) },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb4364cL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb43e1cL ) }, } },
            //new SetObjectDefinition { Name = "SAVEPOINT", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb1adf4L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb1cbdcL ) }, } },
            //new SetObjectDefinition { Name = "SWITCH   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb16aa4L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb17aa4L ) }, } },
            //new SetObjectDefinition { Name = "ITEMBOX   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb493f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb493f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb493f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb493f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb493f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "ITEMBOXAIR   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb48784L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb492ecL ) }, } },
            //new SetObjectDefinition { Name = "ITEMBOXBALLOON", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "LEVUPDAI", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb1fdecL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb20c04L ) }, } },
            //new SetObjectDefinition { Name = "GOALRING", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb4db10L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb4eaccL ) }, } },
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "UDREEL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb129f8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "ORI", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb3eb98L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "CONTWOOD", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xadc30cL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "CONTIRON", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xadb5a8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "ROCKET   ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb37700L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "ROCKETMISSSILE", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb43f10L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb459e8L ) }, } },
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "MSGER", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb587a4L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb5a248L ) }, } },
            //new SetObjectDefinition { Name = "RING   ", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "SOLIDBOX", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xadb2d0L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "DMYOBJ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0x174b03cL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SOAP SW", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SKULL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb3bc20L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb3c53cL ) }, } },
            //new SetObjectDefinition { Name = "PSKULL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb3bc20L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb3c53cL ) }, } },
            //new SetObjectDefinition { Name = "CHAOPIPE", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb1d3e8L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb1de5cL ) }, } },
            //new SetObjectDefinition { Name = "MINIMAL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "WSMMLS", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "CONTCHAO", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb169a4L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb16000L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "STOPLSD", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "KNUDAI", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb0ec68L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "KDASIBA", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "KDWARPHOLE", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "KDDOOR", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb146b8L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb14adcL ) }, } },
            //new SetObjectDefinition { Name = "KDITEMBOX", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "KDDRNGL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb562e8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "KDDRNGC", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb562e8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "KDSPRING", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "KDSPRINGB", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "SPHERE", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "CCYL", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "CCUBE", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "CWALL", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "CCIRCLE", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "MODMOD ", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "EFFOBJ0", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "EFLENSF0", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "BUNCHIN", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb17b84L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb18094L ) }, } },
            //new SetObjectDefinition { Name = "IRONBALL2", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb3d120L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null },new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb3d120L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xb3ea4cL ) }, } },
            //new SetObjectDefinition { Name = "E GHOST", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "E SARU", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0x1462810L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "E 1000", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0x1468b14L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0x146e1c0L ) }, } },
            //new SetObjectDefinition { Name = "E PATH", Models = new List<SetObjectModel> {  } },
            //new SetObjectDefinition { Name = "E GOLD", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0x1492450L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "BIG THE CAT", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0x1521d38L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "LIGHT SW", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SWDRNGL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb562e8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SWDRNGC", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xb562e8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "IRONBAR", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaa2408L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xaa28a4L ) }, } },
            //new SetObjectDefinition { Name = "WALL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xac18dcL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "KEY", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaa21fcL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xab348cL ) }, } },
            //new SetObjectDefinition { Name = "KEYHOLE", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xabf934L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xac28bcL ) }, } },
            //new SetObjectDefinition { Name = "KEYDOOR", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xabab68L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xabbc30L ) }, } },
            //new SetObjectDefinition { Name = "SANDGLASS", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xabbcf4L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xabd544L ) }, } },
            //new SetObjectDefinition { Name = "DOOR", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaacac8L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "CHIMNEY", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xac2544L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "NEON", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xab8cd8L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xab8e34L ) }, } },
            //new SetObjectDefinition { Name = "FIREPOT", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xab3fe4L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "TORCHCUP", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xabf48cL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SNEAKHEAD", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xa9dc48L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xa9ed4cL ) }, } },
            //new SetObjectDefinition { Name = "SNEAKRAIL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xac8448L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xac1b74L ) }, } },
            //new SetObjectDefinition { Name = "BLOCK", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaad758L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "AWNING", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaacc3cL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xaad3ecL ) }, } },
            //new SetObjectDefinition { Name = "EYE", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaaabbcL ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xaab82cL ) }, } },
            //new SetObjectDefinition { Name = "LIGHT", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "WINDMILL", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaa3f20L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xaa88ecL ) }, } },
            //new SetObjectDefinition { Name = "WEED", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xab208cL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SNAKEDISH", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaa0ca8L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xa82f64L ) }, } },
            //new SetObjectDefinition { Name = "WARP", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "PYRAMID", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xac9250L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xac93ccL ) }, } },
            //new SetObjectDefinition { Name = "SNAKESTATUE", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xa81aa4L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xaa0c40L ) }, } },
            //new SetObjectDefinition { Name = "SHIP", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xa7bed4L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "HANGRING", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xaaffa0L ), ModelFormat = ModelFormat.Geometry, GeometryFormat = GeometryFormat.Chunk, Model = reader.ReadObjectAtOffset<SAModelLibrary.GeometryFormats.Chunk.Geometry>( 0xab15e4L ) }, } },
            //new SetObjectDefinition { Name = "SPIDERWEB", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xa95eecL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "BOARD", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xab10ecL ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "SNAKEDISH2", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = reader.ReadObjectAtOffset<TextureReferenceList>( 0xab9570L ), ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "G LIGHT SW", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },
            //new SetObjectDefinition { Name = "LINKLINK", Models = new List<SetObjectModel> { new SetObjectModel { TextureReferenceList = null, ModelFormat = ModelFormat.Unknown, GeometryFormat = GeometryFormat.Unknown, Model = null }, } },

            //};


            //                foreach ( var definition in SetObjectDefinitions )
            //                {
            //                    if ( definition.Models.Count == 0 )
            //                        continue;

            //                    var model = definition.Models.First();
            //                    switch ( model.ModelFormat )
            //                    {
            //                        case ModelFormat.Model:
            //                            switch ( model.GeometryFormat )
            //                            {
            //                                case GeometryFormat.Chunk:
            //                                    ChunkAssimpExporter.Animated.Export( ( Node ) model.Model, definition.Name + ".dae" );
            //                                    break;
            //                                case GeometryFormat.GC:
            //                                    GC.GCAssimpExporter.Default.Export( ( Node ) model.Model, definition.Name + ".dae" );
            //                                    break;
            //                            }
            //                            break;
            //                        case ModelFormat.Geometry:
            //                            break;
            //                    }
            //                }
            //            }

            //CustomLandTableTests();
            var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            var sonicTextures = new List<string>()
            {
                "stx_newspin",
                "so_lvup00",
                "mstx_ref0",
                "soitem00",
                "soitem01",
                "soitemx",
                "sonic_soapshoes",
                "stx_00",
                "stx_01",
                "stx_ref00",
                "s_wind"
            };
            models[16].Export( "sonicmdl/sonicmdl_16.dae", sonicTextures );
            ////models[ 16 ].RootNode = ChunkAssimpImporter.Animated.Import( @"sonicmdl/sonicmdl_16.dae" );
            ////models[ 16 ].RootNode = ChunkAssimpImporter.Animated.Import( @"D:\Users\smart\Downloads\HDSonicSA2_LV (2)\HDSonicSA2_LV\Sa2Ready_fix2.fbx" );
            //models.First( x => x.UID == 0 ).RootNode = models.First( x => x.UID == 31 ).RootNode =
            //    ChunkAssimpImporter.Animated.Import( @"D:\Users\smart\Desktop\SA2\SA1 Sonic\sonicmdl_000.FBX",
            //                                         @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonictex.prs" );

            //models[ 16 ].Export( "sonicmdl/sonicmdl_16_import.dae", sonicTextures );
            //SavePRS( models, @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonicmdl.prs" );
            //return;

            //ScanNodes( File.OpenRead( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\sonic2app_decrypted.exe" ), -0x402600 );
            //ScanNodes( File.OpenRead( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" ), 0 );
            //ExportLandTablesToObj();
            //var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            //models[ 16 ].Export( "sonicmdl/sonicmdl_16.dae",
            //                    new List<string>()
            //                    {
            //                        "stx_newspin",
            //                        "so_lvup00",
            //                        "mstx_ref0",
            //                        "soitem00",
            //                        "soitem01",
            //                        "soitemx",
            //                        "sonic_soapshoes",
            //                        "stx_00",
            //                        "stx_01",
            //                        "stx_ref00",
            //                        "s_wind"
            //                    });

            //var path = @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\CHRMODELS.DLL";
            //var library = new PeFile( path );
            //using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            //{
            //    reader.FileName = path;

            //    var modelListOffset = library.ExportedFunctions.Single( x => x.Name == "___SONIC_OBJECTS" ).Address;
            //    var modelCount = ( library.ExportedFunctions.Single( x => x.Name == "___SONIC_MODELS" ).Address - modelListOffset ) / 4;

            //    reader.SeekBegin( modelListOffset );
            //    reader.BaseOffset = -( long )library.ImageNtHeaders.OptionalHeader.ImageBase;
            //    for ( int i = 0; i < modelCount; i++ )
            //    {
            //        var rootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( GeometryFormat.BasicDX ) );
            //        if ( rootNode == null )
            //            continue;

            //        //ExportObjBasic( rootNode, $"sa1{i:D4}.obj" );

            //        if ( i == 0 )
            //        {
            //            //var converted = ConvertToChunk( rootNode );
            //            BasicAssimpExporter.Animated.Export( rootNode, "__SONIC_MODELS_0.dae" );
            //        }
            //    }
            //}

            //ExportDae( models[ 16 ], "sonicmdl_16.dae" );

            //LandTableExportTest();
            //CustomLandTableTests();
            //DumpUsedVertexConfigs();

            //var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            //var models2 = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\teriosmdl.prs" );
            //ExportObj( models2 );

            //var uidOffset = models2.OrderBy( x => x.UID )
            //                       .First( x => x.RootNode.EnumerateAllNodes().Count() == 62 ).UID;

            //for ( int i = 0; i < models2.Count; i++ )
            //{
            //    var model2 = models2[ i ];
            //    var modelUid = model2.UID - uidOffset;
            //    if ( modelUid < 0 )
            //        continue;

            //    var nodeCount = model2.RootNode.EnumerateAllNodes().Count();

            //    for ( int j = 0; j < models.Count; j++ )
            //    {
            //        if ( models[j].UID == modelUid && models[j].RootNode.EnumerateAllNodes().Count() == nodeCount )
            //            models[j].RootNode = model2.RootNode;
            //    }
            //}

            ////models[ 16 ].RootNode = models2.OrderBy( x => x.UID )
            ////                               .First( x => x.RootNode.EnumerateAllNodes().Count() ==
            ////                                            models[ 16 ].RootNode.EnumerateAllNodes().Count() ).RootNode;

            ////var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            //var path = @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\CHRMODELS.DLL";
            //var library = new PeFile( path );
            //using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            //{
            //    reader.FileName = path;

            //    var modelListOffset = library.ExportedFunctions.Single( x => x.Name == "___SONIC_OBJECTS" ).Address;
            //    var modelCount = ( library.ExportedFunctions.Single( x => x.Name == "___SONIC_MODELS" ).Address - modelListOffset ) / 4;

            //    reader.SeekBegin( modelListOffset );
            //    reader.BaseOffset = -( long )library.ImageNtHeaders.OptionalHeader.ImageBase;
            //    for ( int i = 0; i < modelCount; i++ )
            //    {
            //        var rootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( GeometryFormat.BasicDX ) );
            //        if ( rootNode == null )
            //            continue;

            //        ExportObjBasic( rootNode, $"sa1{i:D4}.obj" );

            //        if ( i == 0 )
            //        {
            //            var converted = ConvertToChunk( rootNode );

            //            //models[16].RootNode.Geometry = converted.EnumerateAllNodes().ToList()[22].Geometry;
            //            //models[16].RootNode.Flags &= ~NodeFlags.Hide;
            //            //ExportObj( models[16], "test2.obj" );

            //            models[16].RootNode = converted;
            //        }

            //        //using ( var writer = new EndianBinaryWriter( $"{i:D4}.nj", Endianness.Little ) )
            //        //{
            //        //    //writer.Write( new byte[] { 0x4E, 0x4A, 0x43, 0x4D } );
            //        //    //writer.Write( 0 );
            //        //    //writer.BaseOffset = 8;
            //        //    rootNode.Write( writer );
            //        //}


            //        //var test = new Node( $"{i:D4}.nj", GeometryFormat.BasicDX );
            //    }
            //}

            //models.Save( "test.mdl", Endianness.Big );

            //SavePRS( models, @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonicmdl.prs" );

            //var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            //ExportObj( models[ 16 ], "test.obj" );

            //var models = new ModelList( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\_\mdl files\sonicmdl.prs" );
            ////ExportObj( models.Single( x => x.UID == models.Min(y => y.UID)), "test.obj" );
            //SavePRS( models, @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonicmdl.prs" );

            //foreach ( var model in models )
            //{
            //    ExportObj( model, $"{model.UID:D4}.obj" );
            //}
            //models.Save( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonicmdl.prs" );

            //var model = models[16];

            //TestVertexIndexOverlap( model );

            //foreach ( var model in models )
            //{
            //    using ( var writer = new EndianBinaryWriter( $"{model.UID:D4}.nj", Endianness.Little ) )
            //    {
            //        writer.Write( new byte[] { 0x4E, 0x4A, 0x43, 0x4D } );
            //        writer.Write( 0 );
            //        writer.BaseOffset = 8;

            //        var weights = new List<List<VertexNNFChunk>>();
            //        var curWeights = new List<VertexNNFChunk>();
            //        var curState = ( WeightStatus ) 0xFF;

            //        ForEachNode(model.RootNode, node =>
            //        {
            //            if ( node.Geometry == null )
            //                return;

            //            var geometry = ( Geometry )node.Geometry;
            //            for ( var chunkIndex = 0; chunkIndex < geometry.VertexList.Count; chunkIndex++ )
            //            {
            //                var chunk = geometry.VertexList[ chunkIndex ];
            //                if ( chunk.Type != ChunkType.VertexNNF )
            //                    continue;

            //                var vertexChunk = ( VertexNNFChunk )chunk;

            //                Console.WriteLine( $"{geometry.SourceOffset:X8} {chunkIndex} {chunk.VertexCount} {chunk.WeightStatus}" );

            //                if ( chunk.WeightStatus == WeightStatus.End || curState == WeightStatus.Start && chunk.WeightStatus == WeightStatus.Start )
            //                {
            //                    weights.Add( curWeights );
            //                    curWeights = new List<VertexNNFChunk>();
            //                }

            //                curState = chunk.WeightStatus;
            //                curWeights.Add( vertexChunk );


            //                //var newVertexChunk = new VertexNChunk();
            //                //newVertexChunk.Vertices = vertexChunk.Vertices.Select( ( x, i ) =>
            //                //                                     {
            //                //                                         var weightVertexId = ( short )( x.NinjaFlags );
            //                //                                         var weight = x.NinjaFlags >> 16;
            //                //                                         var absWeightVertexId = ( uint )( weightVertexId + chunk.BaseIndex );
            //                //                                         var vertexId = i + chunk.BaseIndex;
            //                //                                         return new VertexN() { Position = x.Position, Normal = x.Normal };
            //                //                                     })
            //                //                                     .ToArray();

            //                //geometry.VertexList[chunkIndex] = newVertexChunk;

            //                //for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
            //                //{
            //                //    ref var vertex = ref vertexChunk.Vertices[i];
            //                //    var weightVertexId = ( short )( vertex.NinjaFlags );
            //                //    var weight = vertex.NinjaFlags >> 16;
            //                //    var absWeightVertexId = ( uint )( weightVertexId + chunk.BaseIndex );
            //                //    var vertexId = i + chunk.BaseIndex;
            //                //}
            //            }
            //        } );

            //        model.RootNode.Write( writer, null );
            //    }
            //}

            //SavePRS( models, @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\sonicmdl.prs" );


            //var vertexMap = new SortedDictionary<uint, List<(int, int, WeightStatus, uint)>>();
            //int nodeIndex = 0;
            //void Recurse( Node node )
            //{
            //    if ( node.Geometry != null )
            //    {
            //        var geometry = ( Geometry )node.Geometry;
            //        foreach ( var chunk in geometry.VertexList )
            //        {
            //            if ( chunk.Type != ChunkType.VertexNNF )
            //                continue;

            //            var vertexChunk = ( ( VertexNNFChunk )chunk );

            //            for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
            //            {
            //                var vertex = vertexChunk.Vertices[i];
            //                var weightVertexId = vertex.NinjaFlags & 0x0000FFFF;
            //                var weight = vertex.NinjaFlags >> 16;
            //                var absWeightVertexId = ( uint )( weightVertexId + chunk.BaseIndex );
            //                var vertexId = i + chunk.BaseIndex;

            //                if ( !vertexMap.ContainsKey( absWeightVertexId ) )
            //                    vertexMap[ absWeightVertexId ] = new List<(int, int, WeightStatus, uint)>();

            //                vertexMap[absWeightVertexId].Add( (nodeIndex, vertexId, chunk.WeightStatus, weight) );
            //            }
            //        }
            //    }

            //    ++nodeIndex;

            //    if ( node.Child != null )
            //        Recurse( node.Child );

            //    if ( node.Sibling != null )
            //        Recurse( node.Sibling );
            //}

            //    var vertexMap = new SortedDictionary<uint, List<(int, int, WeightStatus, uint)>>();
            //    int nodeIndex = 0;

            //    void Recurse( Node node )
            //    {
            //        while ( node != null )
            //        {
            //            if ( node.Geometry != null )
            //            {
            //                var geometry = ( Geometry ) node.Geometry;
            //                foreach ( var chunk in geometry.VertexList )
            //                {
            //                    if ( chunk.Type != ChunkType.VertexNNF )
            //                        continue;

            //                    var vertexChunk = ( VertexNNFChunk ) chunk;

            //                    for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
            //                    {
            //                        var vertex            = vertexChunk.Vertices[ i ];
            //                        var weightVertexId    = vertex.NinjaFlags & 0x0000FFFF;
            //                        var weight            = vertex.NinjaFlags >> 16;
            //                        var absWeightVertexId = ( uint ) ( weightVertexId + chunk.BaseIndex );
            //                        var vertexId          = i + chunk.BaseIndex;

            //                        if ( !vertexMap.ContainsKey( absWeightVertexId ) )
            //                            vertexMap[ absWeightVertexId ] = new List<(int, int, WeightStatus, uint)>();

            //                        vertexMap[ absWeightVertexId ].Add( ( nodeIndex, vertexId, chunk.WeightStatus, weight ) );
            //                    }
            //                }
            //            }

            //            ++nodeIndex;

            //            if ( node.Child != null )
            //                Recurse( node.Child );

            //            node = node.Sibling;
            //        }
            //    }

            //    Recurse( mainModel.RootNode );

            //    var perVertexWeights = new Dictionary<int, List<(int, float)>>();
            //    foreach ( var map in vertexMap.Values.SelectMany(x => x) )
            //    {
            //        var nodeId = map.Item1;
            //        var vertexId = map.Item2;
            //        var weightIndex = ( int ) map.Item3;
            //        var weight = map.Item4 / 255f;

            //        if ( !perVertexWeights.ContainsKey( vertexId ) )
            //            perVertexWeights[ vertexId ] = new List<(int, float)>();

            //        perVertexWeights[ vertexId ].Add( ( nodeId, weight ) );
            //    }
        }

        public static void PVMToGVM( string path, string outPath )
        {
            var pvm = new PvmArchive();
            var pvmReader = ( PvmArchiveReader )pvm.Open( path );

            var gvm = new GvmArchive();
            var gvmStream = new MemoryStream();
            var gvmWriter = ( GvmArchiveWriter )gvm.Create( gvmStream );
            uint index = 500;
            foreach ( var entry in pvmReader.Entries )
            {
                var pvr = new PvrTexture();
                pvr.Read( entry.Open(), out var bitmap );

                var gvr = new GvrTexture();
                gvr.GlobalIndex = index++;
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

        public static void ExportDeathZonesToObj()
        {
            var baseOffset = 0x402600;

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

            using ( var reader = new EndianBinaryReader( @"D:\Games\PC\Sonic Adventure 2\sonic2app_decrypt.exe", Endianness.Little ) )
            {
                foreach ( var stageDeathZone in deathZoneAddresses )
                {
                    using ( var writer = File.CreateText( $"stg{stageDeathZone.Key:D2}DeathZones.obj" ) )
                    {

                        reader.SeekBegin( stageDeathZone.Value - baseOffset );
                        reader.BaseOffset = -baseOffset;

                        int i = 0;
                        int positionBaseIndex = 0;
                        int normalBaseIndex = 0;
                        int uvBaseIndex = 0;
                        while ( true )
                        {
                            DeathZone deathZone;

                            try
                            {
                                deathZone = reader.ReadObject<DeathZone>();
                                if ( deathZone.Flags == 0 && deathZone.RootNode == null )
                                    break;
                            }
                            catch ( Exception e )
                            {
                                break;
                            }

                            ExportObjBasic( deathZone.RootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
                            //ExportObjBasic( deathZone.RootNode, $"stg{stageDeathZone.Key:D2}DeathZone{i}.obj" );
                            ++i;
                        }

                    }
                }
            }
        }

        public static void ExportLandTablesToObj()
        {
            var path       = @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\DLL\Win32\Data_DLL.dll";
            var library    = new PeFile( path );
            var landTables = new List<LandTableSA2>();
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = path;

                var rdataBaseOffset = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x1200 );
                var dataBaseOffset  = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x2000 );

                var landTablesExports = library.ExportedFunctions.Where( x => x.Name.StartsWith( "objLandTable0003" ) );
                foreach ( var landTableExport in landTablesExports )
                {
                    var landTableOffset = landTableExport.Address;

                    reader.SeekBegin( landTableOffset - 0x2000 );
                    reader.BaseOffset = dataBaseOffset;

                    var landTable = reader.ReadObject<LandTableSA2>();
                    landTables.Add( landTable );

                    ExportObj( landTable, landTableExport.Name + ".obj" );
                }

            }

        }

        public static void LandTableExportTest()
        {
            var path       = @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\DLL\Win32\Data_DLL_orig.dll";
            var library    = new PeFile( path );
            var landTables = new List<LandTableSA2>();
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                reader.FileName = path;

                var rdataBaseOffset = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x1200 );
                var dataBaseOffset  = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x2000 );

                var landTablesExports = library.ExportedFunctions.Where( x => x.Name.StartsWith( "objLandTable" ) );
                foreach ( var landTableExport in landTablesExports )
                {
                    var landTableOffset = landTableExport.Address;

                    reader.SeekBegin( landTableOffset - 0x2000 );
                    reader.BaseOffset = dataBaseOffset;

                    var landTable = reader.ReadObject<LandTableSA2>();
                    landTable.Save( landTableExport.Name + ".lt" );
                    var newLandTable = new LandTableSA2( landTableExport.Name + ".lt" );

                    ResourceFile.Save( landTable, landTableExport.Name + ".lt" );
                }

            }

        }

        public class LandModelSA1
        {
           
        }

        public static void CustomLandTableTests()
        {
            var landTable = ResourceFile.Load<LandTableSA2>( @"D:\Users\smart\Documents\visual studio 2017\Projects\SAModelLibrary\SA2ModelConverter\bin\Debug\objLandTable0013.lt" );
            //var newModels = new List<LandModelSA2>();

            //foreach ( var model in landTable.Models )
            //{
            //    //if ( model.RootNode?.Geometry?.Format != GeometryFormat.GC )
            //    //    continue;

            //    //var geometry = ( GC.Geometry )model.RootNode.Geometry;
            //    //foreach ( var mesh in geometry.OpaqueMeshes )
            //    //{
            //    //    // 0: no effect
            //    //    // 1: index attribute flags
            //    //    // 2: lighting
            //    //    // 3: no apparent effect
            //    //    // 4: no apparent effect
            //    //    // 5: no apparent effect
            //    //    // 6: no apparent effect
            //    //    // 7: no apparent effect
            //    //    // 8: material settings
            //    //    // 9: no apparent effect
            //    //    // 10: mipmap/uv related?
            //    //    // 11:
            //    //    //mesh.Parameters.RemoveAll( x => x.Type != (GC.MeshStateParamType)1 && x.Type != (GC.MeshStateParamType)2 && x.Type != (GC.MeshStateParamType)8 && x.Type != (GC.MeshStateParamType)10 );

            //    //    //mesh.Parameters.RemoveAll( x => x.Type == ( GC.MeshStateParamType ) 2 );
            //    //    //mesh.Parameters.Add( new GC.MeshStateParam() { Type = ( GC.MeshStateParamType ) 2, Value1 = 0x0b11, Value2 = 1 } );

            //    //    //mesh.Parameters.RemoveAll( x => x.Type == ( GC.MeshStateParamType )10 );
            //    //    //mesh.Parameters.Add( new GC.UnknownParam() { Type = ( GC.MeshStateParamType )10, Value1 = 0x104a, Value2 = 0 } );
            //    //    foreach ( var param in mesh.Parameters )
            //    //    {
            //    //        if ( param.Type == GC.MeshStateParamType.Texture )
            //    //        {
            //    //            var materialParam = ( GC.TextureParams ) param;
            //    //            Console.WriteLine( materialParam.TileMode );
            //    //            materialParam.TileMode = GC.TileMode.WrapU | GC.TileMode.WrapV;
            //    //        }
            //    //    }
            //    //}

            //    if ( model.RootNode?.Geometry?.Format != GeometryFormat.Basic )
            //        continue;

            //    var geometry = ( Basic.Geometry )model.RootNode.Geometry;
            //    var newModel = new LandModelSA2();
            //    newModel.Flags = SurfaceFlags.Visible;
            //    newModel.Bounds = model.Bounds;
            //    newModel.Field14 = model.Field14;
            //    newModel.Field18 = model.Field18;
            //    newModel.RootNode = new Node
            //    {
            //        Flags       = model.RootNode.Flags,
            //        Geometry    = BasicToGC( geometry ),
            //        Rotation    = model.RootNode.Rotation,
            //        Scale       = model.RootNode.Scale,
            //        Translation = model.RootNode.Translation,
            //        Child       = model.RootNode.Child,
            //        Sibling     = model.RootNode.Sibling
            //    };
            //    newModels.Add( newModel );
            //}

            //landTable.Models.InsertRange( 0, newModels );

            //ResourceFile.Save( landTable,
            //                   @"D:\Users\smart\Documents\visual studio 2017\Projects\SAModelLibrary\SA2ModelConverter\bin\Debug\test\stage.lt" );

            //PVMToGVM( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\BEACH01.PVM",
            //          @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\beach01.prs" );

            //PVMToGVM( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\HIGHWAY01.PVM",
            //          @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\highway01.prs" );

            //PVMToGVM( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\WINDY03.PVM",
            //          @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\windy03.prs" );

            //PVMToGVM( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\system\TWINKLE02.PVM",
            //          @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\twinkle02.prs" );

            //DumpUsedVertexConfigs();
            //LandTableSA2 sa2LandTable = new LandTableSA2();

            //using ( var reader =
            //    new EndianBinaryReader( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure DX\sonic.exe", Endianness.Little ) )
            //{
            //    reader.BaseOffset = -0x400000;
            //    reader.SeekBegin( 0xA99Cb8 ); // emerald coast 1
            //    //reader.SeekBegin( 0x22B1E98 ); // speed highway 1
            //    //reader.SeekBegin( 0x80433C ); // windy valley 3
            //    //reader.SeekBegin( 0x22B867C ); // twinkle park 2

            //    var sa1LandTable = reader.ReadObject<LandTableSA1>();
            //    new LandTableSA1AssimpExporter().Export( sa1LandTable, "beach01.dae" );
            //    sa2LandTable = sa1LandTable.ConvertToSA2Format( "beach01" );
            //    //sa2LandTable = sa1LandTable.ConvertToSA2Format( "highway01" );
            //    //sa2LandTable = sa1LandTable.ConvertToSA2Format( "windy03" );
            //    //sa2LandTable = sa1LandTable.ConvertToSA2Format( "twinkle02" );
            //}

            File.Delete( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\ghz2test.prs" );
            var sa2LandTableImporter = new LandTableSA2AssimpImporter();
            var sa2LandTable = sa2LandTableImporter
                .Import( @"D:\Users\smart\Documents\visual studio 2017\Projects\SAModelLibrary\SA2ModelConverter\bin\Debug\ghz2test.FBX",
                         @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\ghz2test.prs" );

            ResourceFile.Save( sa2LandTable,
                               @"D:\Users\smart\Documents\visual studio 2017\Projects\SAModelLibrary\SA2ModelConverter\bin\Debug\test\stage.lt" );
        }

        public static void DumpUsedVertexConfigs()
        {
            var uniqueFlags = new SortedSet<GC.IndexAttributeFlags>();
            var library    = new PeFile( @"D:\Games\PC\SteamLibrary\steamapps\common\Sonic Adventure 2\resource\gd_PC\DLL\Win32\Data_DLL_orig.dll" );
            using ( var reader = new EndianBinaryReader( library.Stream, Endianness.Little ) )
            {
                var dataBaseOffset  = -( long )( library.ImageNtHeaders.OptionalHeader.ImageBase + 0x2000 );
                var landTablesExports = library.ExportedFunctions.Where( x => x.Name.StartsWith( "objLandTable" ) );

                foreach ( var landTableExport in landTablesExports )
                {
                    var landTableOffset = landTableExport.Address;

                    reader.SeekBegin( landTableOffset - 0x2000 );
                    reader.BaseOffset = dataBaseOffset;

                    var landTable = reader.ReadObject<LandTableSA2>();

                    foreach ( var model in landTable.Models )
                    {
                        if ( model.RootNode?.Geometry?.Format != GeometryFormat.GC )
                            continue;

                        var geometry = ( GC.Geometry )model.RootNode.Geometry;
                        foreach ( var mesh in geometry.OpaqueMeshes )
                        {
                            foreach ( var meshParam in mesh.Parameters )
                            {
                                if ( meshParam.Type == GC.MeshStateParamType.IndexAttributeFlags )
                                {
                                    var flags = ( ( GC.IndexAttributeFlagsParam ) meshParam ).Flags;
                                    //if ( flags.HasFlag( GC.IndexAttributeFlags.HasNormal ) )  Debugger.Break();
                                    uniqueFlags.Add( flags );
                                }
                            }
                        }
                    }
                }

            }

            foreach ( var flags in uniqueFlags )
            {
                Console.WriteLine( flags );
            }
        }

        public static void ScanNodes( Stream stream, int baseOffset )
        {
            var nodes = new HashSet<Node>();

            stream.Position = 0x514a00; // start of .data
            //stream.Position = 0xC71F38;
            //stream.Position = 0xF71F38;
            //stream.Position = 0x11e24;
            using ( var reader = new EndianBinaryReader( stream, Endianness.Little ) )
            {
                reader.BaseOffset = baseOffset;

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
                        Console.WriteLine( $"Node @ {nodePosition:X8} ({( nodePosition - reader.BaseOffset ):X8})" );
                        nodes.Add( node );
                    }
                    catch ( Exception e )
                    {
                        reader.SeekBegin( nodePosition + 4 );
                        continue;
                    }

                }
            }

            var rootNodes = new HashSet<Node>();

            foreach ( var node in nodes.Where(x => x.Geometry != null ) )
            {
                var candidates = new List<(int, Node)>();

                foreach ( var otherNode in nodes )
                {
                    if ( otherNode == node )
                        continue;

                    var nodesInHierarchy = otherNode.EnumerateAllNodes().ToList();
                    if ( nodesInHierarchy.Contains( node ) )
                    {
                        candidates.Add( ( nodesInHierarchy.Count, otherNode ) );
                    }
                }

                if ( candidates.Count > 0 )
                {
                    var maxScore = candidates.Max( x => x.Item1 );

                    foreach ( var winner in candidates.Where( x => x.Item1 == maxScore ) )
                        rootNodes.Add( winner.Item2 );
                }
                else
                {
                    rootNodes.Add( node );
                }
            }

            var rootNodes2 = new HashSet<Node>();
            foreach ( var rootNode in rootNodes )
            {
                var node = rootNode;

                while ( true )
                {
                    if ( node.Parent != null )
                        node = node.Parent;
                    else
                        break;
                }

                rootNodes2.Add( rootNode );
            }


            //var candidates = new HashSet<Node>();

            ////nodes.Reverse();

            //foreach ( var geometry in nodes.Where( x => x.Geometry != null ).Select( x => x.Geometry ) )
            //{
            //    var curCandidates = new List<(int, Node)>();

            //    foreach ( var node in nodes )
            //    {
            //        if ( node.EnumerateAllNodes().Any( x => x.Geometry == geometry ) )
            //        {
            //            curCandidates.Add( ( node.EnumerateAllNodes().Count(), node ) );
            //        }
            //    }

            //    var maxScore = curCandidates.Max( x => x.Item1 );

            //    foreach ( var winner in curCandidates.Where(x => x.Item1 == maxScore ) )
            //    {
            //        candidates.Add( winner.Item2 );
            //    }
            //}

            //var rootNodes = new HashSet<Node>();
            //foreach ( var candidate in candidates )
            //{
            //    var node = candidate;
            //    while ( true )
            //    {
            //        if ( node.Parent != null )
            //        {
            //            node = node.Parent;
            //        }
            //        else
            //            break;
            //    }

            //    rootNodes.Add( node );
            //}

            foreach ( var node in rootNodes2 )
            {
                var geometryFormat = node.EnumerateAllNodes().FirstOrDefault( x => x.Geometry != null )?.Geometry.Format;
                if ( !geometryFormat.HasValue )
                    continue;

                switch ( geometryFormat.Value )
                {
                    case GeometryFormat.Basic:
                    case GeometryFormat.BasicDX:
                        ExportObjBasic( node,
                                        $"{Path.GetFileNameWithoutExtension( node.SourceFilePath )}_Basic_{( node.SourceOffset - baseOffset ):X8}.obj" );
                        break;
                    case GeometryFormat.Chunk:
                        ExportObj( node, $"{Path.GetFileNameWithoutExtension( node.SourceFilePath )}_Chunk_{( node.SourceOffset - baseOffset ):X8}.obj" );
                        break;
                    case GeometryFormat.GC:
                        ExportObjGC( node, $"{Path.GetFileNameWithoutExtension( node.SourceFilePath )}_GC_{( node.SourceOffset - baseOffset ):X8}.obj" );
                        break;
                    //default:
                        //throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static void SavePRS( ModelList models, string path )
        {
            using ( var fileStream = File.Create( path ) )
            {
                var memoryStream = new MemoryStream();
                Prs.Compress( models.Save(), memoryStream );
                memoryStream.Position = 0;
                memoryStream.CopyTo( fileStream );
            }
        }

        public static void ForEachNode( Node node, Action<Node> action )
        {
            foreach ( var curNode in node.EnumerateAllNodes() )
            {
                action( curNode );
            }
        }

        // Vertex indices overlap throughout the entire model
        // Vertex indices overlap throughout a single object, however
        // this only occurs when weights are used. Perhaps the vertices
        // need to be transformed?
        public static void TestVertexIndexOverlap( Model model )
        {
            var usedVertexIndexMap = new Dictionary<int, int>();

            ForEachNode( model.RootNode, node =>
            {
                if ( node.Geometry == null )
                    return;

                var geometry = ( Geometry )node.Geometry;

                foreach ( var chunk in geometry.VertexList )
                {
                    for ( int i = 0; i < chunk.VertexCount; i++ )
                    {
                        var vertexIndex = chunk.BaseIndex + i;
                        if ( !usedVertexIndexMap.ContainsKey( vertexIndex ) )
                            usedVertexIndexMap[vertexIndex] = 1;
                        else
                            usedVertexIndexMap[vertexIndex]++;
                    }
                }

                if ( usedVertexIndexMap.Any( x => x.Value > 1 ) )
                    Debugger.Break();

                usedVertexIndexMap.Clear();
            } );
        }

        public static void TestUndefinedVertexIndexReference( Model model )
        {
            var usedVertexIndexMap = new HashSet<int>();

            ForEachNode( model.RootNode, node =>
            {
                if ( node.Geometry == null )
                    return;

                var geometry = ( Geometry )node.Geometry;

                foreach ( var chunk in geometry.VertexList )
                {
                    for ( int i = 0; i < chunk.VertexCount; i++ )
                    {
                        var vertexIndex = chunk.BaseIndex + i;
                        usedVertexIndexMap.Add( vertexIndex );
                    }
                }

                foreach ( var chunk in geometry.PolygonList )
                {

                }

                usedVertexIndexMap.Clear();
            } );
        }

        public struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public List< Vector2 > UVs;
            public Color Color;
            public List<NodeWeight> Weights;

            public Vertex( Vector3 position, Vector3 normal, Color color, int nodeIndex, float weight )
            {
                Position = position;
                Normal = normal;
                UVs = new List<Vector2>();
                Color = color;
                Weights = new List<NodeWeight>() { new NodeWeight( nodeIndex, weight ) };
            }
        }

        public struct NodeWeight
        {
            public int NodeIndex;
            public float Weight;

            public NodeWeight(int nodeId, float weight)
            {
                NodeIndex = nodeId;
                Weight = weight;
            }
        }

        public static void ExportObj( Model model, string path )
        {
            ExportObj( model.RootNode, path );
        }

        public static void ExportObj( Node rootNode, string path )
        {
            var writer = File.CreateText( path );

            var positionBaseIndex = 0;
            var normalBaseIndex = 0;
            var uvBaseIndex = 0;

            ExportObjChunk( rootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );

            writer.Dispose();
        }

        public static void ExportObjChunk( Node rootNode, StreamWriter writer, ref int _positionBaseIndex, ref int _normalBaseIndex,
                                           ref int _uvBaseIndex )
        {
            var vertexCache = new SortedDictionary<int, Vertex>();
            var polygonCache = new SortedDictionary<int, (List<Chunk16> List, int Index)>();

            void AddToVertexCache( int index, Vertex vertex )
            {
                //if ( vertexCache.ContainsKey( index ) && vertexCache[index].Weights[0].Weight != 1f)
                //    throw new InvalidOperationException( $"Vertex cache already contains a vertex at index: {index}" );

                vertexCache[index] = vertex;
            }

            void AddToPolygonListCache( int index, (List<Chunk16> List, int Index) cached )
            {
                //if ( polygonCache.ContainsKey( index ) )
                //    throw new InvalidOperationException( $"Polygon cache already contains a entry at index: {index}" );

                polygonCache[index] = cached;
            }

            int nodeIndex = -1;
            int positionBaseIndex = _positionBaseIndex;
            int normalBaseIndex = _normalBaseIndex;
            int uvBaseIndex = _uvBaseIndex;

            void ProcessNode( Node node, Matrix4x4 parentTransform )
            {
                while ( node != null )
                {
                    ++nodeIndex;

                    var localTransform = node.Transform;
                    var worldTransform = localTransform * parentTransform;

                    var geometry = ( Geometry )node.Geometry;
                    if ( geometry != null )
                    {
                        var triangles = new List<ushort>();

                        foreach ( var chunk in geometry.VertexList )
                        {
                            switch ( chunk.Type )
                            {
                                case ChunkType.VertexXYZ:
                                    {
                                        var vertexChunk = ( VertexXYZChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex      = vertexChunk.Vertices[i];
                                            var position    = Vector3.Transform( vertex.Position, worldTransform );
                                            var cacheVertex = new Vertex( position, new Vector3(), Color.White, nodeIndex, 1f );
                                            AddToVertexCache( vertexChunk.BaseIndex + i, cacheVertex );
                                        }
                                    }
                                    break;

                                case ChunkType.VertexN:
                                    {
                                        var vertexChunk = ( VertexNChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, 1f );
                                            AddToVertexCache( vertexChunk.BaseIndex + i, cacheVertex );
                                        }
                                    }
                                    break;

                                case ChunkType.VertexNNF:
                                    {
                                        var vertexChunk = ( VertexNNFChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            var vertex = vertexChunk.Vertices[i];

                                            // Transform vertex
                                            var weightByte = vertex.NinjaFlags >> 16;
                                            var weight = weightByte * ( 1f / 255f );
                                            var position = Vector3.Transform( vertex.Position, worldTransform ) * weight;
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform ) * weight;

                                            // Store vertex in cache
                                            var vertexId = vertex.NinjaFlags & 0x0000FFFF;
                                            var vertexCacheId = ( int )( vertexChunk.BaseIndex + vertexId );

                                            if ( chunk.WeightStatus == WeightStatus.Start || !vertexCache.ContainsKey( vertexCacheId ) )
                                            {
                                                // Add new vertex to cache
                                                var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, weight );
                                                AddToVertexCache( vertexCacheId, cacheVertex );
                                            }
                                            else
                                            {
                                                // Update cached vertex
                                                var cacheVertex = vertexCache[vertexCacheId];
                                                cacheVertex.Position += position;
                                                cacheVertex.Normal += normal;
                                                cacheVertex.Weights.Add( new NodeWeight( nodeIndex, weight) );
                                                vertexCache[vertexCacheId] = cacheVertex;

                                            }
                                        }
                                    }
                                    break;

                                case ChunkType.VertexD8888:
                                    {
                                        var vertexChunk = ( VertexD8888Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var cacheVertex = new Vertex( position, Vector3.Zero, vertex.Diffuse, nodeIndex, 1f );
                                            AddToVertexCache( vertexChunk.BaseIndex + i, cacheVertex );
                                        }
                                    }
                                    break;

                                case ChunkType.VertexND8888:
                                    {
                                        var vertexChunk = ( VertexND8888Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal,  vertex.Diffuse, nodeIndex, 1f );
                                            AddToVertexCache( vertexChunk.BaseIndex + i, cacheVertex );
                                        }
                                    }
                                    break;

                                case ChunkType.VertexN32:
                                    {
                                        var vertexChunk = ( VertexN32Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var decodedNormal = NormalCodec.Decode( vertex.Normal );
                                            var normal = Vector3.TransformNormal( decodedNormal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, 1f );
                                            AddToVertexCache( vertexChunk.BaseIndex + i, cacheVertex );
                                        }
                                    }
                                    break;

                                default:
                                    Console.WriteLine( chunk.Type );
                                    break;
                            }
                        }

                        void ProcessPolygonList( int index, List<Chunk16> list, bool wasCached = false )
                        {
                            for ( var chunkIndex = index; chunkIndex < list.Count; chunkIndex++ )
                            {
                                var chunk = list[chunkIndex];

                                switch ( chunk.Type )
                                {
                                    case ChunkType.CachePolygonList:
                                        {
                                            if ( wasCached )
                                                throw new InvalidOperationException( "CachePolygonList in cached polygon list" );

                                            var cacheChunk = ( CachePolygonListChunk )chunk;
                                            AddToPolygonListCache( cacheChunk.CacheIndex, (list, chunkIndex + 1) );
                                        }
                                        break;

                                    case ChunkType.DrawPolygonList:
                                        {
                                            if ( wasCached )
                                                throw new InvalidOperationException( "DrawPolygonList in cached polygon list" );

                                            var drawChunk = ( DrawPolygonListChunk )chunk;
                                            if ( !polygonCache.ContainsKey( drawChunk.CacheIndex ) )
                                                continue;
                                            //throw new InvalidOperationException( "DrawPolygonList referenced unused polygon list cache entry" );

                                            var cachedList = polygonCache[drawChunk.CacheIndex];
                                            //polygonCache.Remove( drawChunk.CacheIndex );

                                            ProcessPolygonList( cachedList.Index, cachedList.List, true );
                                        }
                                        break;

                                    //case ChunkType.MaterialDiffuse:
                                    //    {
                                    //        var matChunk = ( MaterialDiffuseChunk )chunk;
                                    //    }
                                    //    break;

                                    case ChunkType.MaterialAmbient:
                                    case ChunkType.MaterialAmbient2:
                                    case ChunkType.MaterialAmbientSpecular:
                                    case ChunkType.MaterialAmbientSpecular2:
                                    case ChunkType.MaterialBump:
                                    case ChunkType.MaterialDiffuse:
                                    case ChunkType.MaterialDiffuse2:
                                    case ChunkType.MaterialDiffuseAmbient:
                                    case ChunkType.MaterialDiffuseAmbient2:
                                    case ChunkType.MaterialDiffuseAmbientSpecular:
                                    case ChunkType.MaterialDiffuseAmbientSpecular2:
                                    case ChunkType.MaterialDiffuseSpecular:
                                    case ChunkType.MaterialDiffuseSpecular2:
                                    case ChunkType.MaterialSpecular:
                                    case ChunkType.MaterialSpecular2:
                                    case ChunkType.MipmapDAdjust:
                                    case ChunkType.SpecularExponent:
                                    case ChunkType.TextureId:
                                    case ChunkType.TextureId2:
                                    case ChunkType.BlendAlpha:
                                        break;

                                    case ChunkType.StripUVN:
                                        {
                                            var stripChunk = ( StripUVNChunk )chunk;
                                            var triangleIndices = stripChunk.ToTriangles();
                                            foreach ( var stripIndex in triangleIndices )
                                            {
                                                if ( !vertexCache.ContainsKey( stripIndex.Index ) )
                                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );

                                                triangles.Add( stripIndex.Index );
                                            }
                                        }
                                        break;

                                    case ChunkType.Strip:
                                        {
                                            var stripChunk = ( StripChunk )chunk;
                                            var triangleIndices = stripChunk.ToTriangles();
                                            foreach ( var stripIndex in triangleIndices )
                                            {
                                                if ( !vertexCache.ContainsKey( stripIndex.Index ) )
                                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );

                                                triangles.Add( stripIndex.Index );
                                            }
                                        }
                                        break;

                                    default:
                                        Console.WriteLine( chunk.Type );
                                        break;
                                }

                                if ( chunk.Type == ChunkType.CachePolygonList )
                                {
                                    break;
                                }
                            }
                        }

                        ProcessPolygonList( 0, geometry.PolygonList );

                        for ( int i = 0; i < triangles.Count; i++ )
                        {
                            var vertex = vertexCache[triangles[i]];
                            writer.WriteLine( $"v {FormatFloat( vertex.Position.X )} {FormatFloat( vertex.Position.Y )} {FormatFloat( vertex.Position.Z )}" );
                        }

                        for ( int i = 0; i < triangles.Count; i++ )
                        {
                            var vertex = vertexCache[triangles[i]];
                            writer.WriteLine( $"vn {FormatFloat( vertex.Normal.X )} {FormatFloat( vertex.Normal.Y )} {FormatFloat( vertex.Normal.Z )}" );
                        }

                        if ( triangles.Count > 0 )
                        {
                            writer.WriteLine( $"g Geometry{nodeIndex}__{geometry.Format}_{geometry.SourceOffset:X8}" );
                            for ( int i = 0; i < triangles.Count; i += 3 )
                            {
                                writer.WriteLine( "f {0}//{1} {2}//{3} {4}//{5}",
                                    positionBaseIndex + i + 1,
                                    normalBaseIndex + i + 1,
                                    positionBaseIndex + i + 2,
                                                  normalBaseIndex + i + 2,
                                    positionBaseIndex + i + 3,
                                                  normalBaseIndex + i + 3 );
                            }

                            positionBaseIndex += triangles.Count;
                            normalBaseIndex += triangles.Count;
                        }

                        //vertexCache.Clear();
                        triangles.Clear();
                    }

                    if ( node.Child != null )
                    {
                        ProcessNode( node.Child, worldTransform );
                    }

                    node = node.Sibling;
                }
            }

            ProcessNode( rootNode, Matrix4x4.Identity );


            _positionBaseIndex = positionBaseIndex;
            _normalBaseIndex = normalBaseIndex;
            _uvBaseIndex = uvBaseIndex;
        }

        public static void ExportObj( ModelList models )
        {
            foreach ( var model in models )
            {
                ExportObj( model, $"{model.UID:D4}.obj" );
            }
        }

        public static void ExportObjBasic( Node rootNode, string path )
        {
            using ( var writer = File.CreateText( path ) )
            {
                var positionBaseIndex = 0;
                var normalBaseIndex = 0;
                var uvBaseIndex = 0;

                ExportObjBasic( rootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
            }
        }

        public static void ExportObjBasic( Node rootNode, StreamWriter writer, ref int _positionBaseIndex, ref int _normalBaseIndex, ref int _uvBaseIndex )
        {
            int nodeIndex = -1;
            int positionBaseIndex = _positionBaseIndex;
            int normalBaseIndex = _normalBaseIndex;
            int uvBaseIndex = _uvBaseIndex;

            void ProcessNode( Node node, Matrix4x4 parentTransform )
            {
                while ( node != null )
                {
                    ++nodeIndex;

                    var localTransform = node.Transform;
                    var worldTransform = localTransform * parentTransform;

                    var geometry = ( Basic.Geometry )node.Geometry;
                    if ( geometry?.VertexPositions != null )
                    {
                        foreach ( var localPos in geometry.VertexPositions )
                        {
                            var pos = Vector3.Transform( localPos, worldTransform );
                            writer.WriteLine( $"v {FormatFloat( pos.X )} {FormatFloat( pos.Y )} {FormatFloat( pos.Z )}" );
                        }

                        if ( geometry.VertexNormals != null )
                        {
                            foreach ( var localNrm in geometry.VertexNormals )
                            {
                                var nrm = Vector3.TransformNormal( localNrm, worldTransform );
                                writer.WriteLine( $"vn {FormatFloat( nrm.X )} {FormatFloat( nrm.Y )} {FormatFloat( nrm.Z )}" );
                            }
                        }

                        if ( geometry.Meshes != null )
                        {
                            for ( var meshIndex = 0; meshIndex < geometry.Meshes.Length; meshIndex++ )
                            {
                                var mesh      = geometry.Meshes[meshIndex];
                                var triangles = mesh.ToTriangles();

                                writer.WriteLine( $"g Geometry{nodeIndex}_{geometry.Format}_Mesh{meshIndex}_{geometry.SourceOffset:X8}" );
                                for ( int j = 0; j < triangles.Length; j += 3 )
                                {
                                    if ( geometry.VertexNormals != null )
                                    {
                                        writer.WriteLine( "f {0}//{1} {2}//{3} {4}//{5}", positionBaseIndex + triangles[j].VertexIndex + 1,
                                                          normalBaseIndex + triangles[j].VertexIndex + 1, positionBaseIndex + triangles[j + 1].VertexIndex + 1,
                                                          normalBaseIndex + triangles[j + 1].VertexIndex + 1,
                                                          positionBaseIndex + triangles[j + 2].VertexIndex + 1,
                                                          normalBaseIndex + triangles[j + 2].VertexIndex + 1 );
                                    }
                                    else
                                    {
                                        writer.WriteLine( "f {0}/// {1}/// {2}///",
                                                          positionBaseIndex + triangles[j].VertexIndex + 1,
                                                          positionBaseIndex + triangles[j + 1].VertexIndex + 1,
                                                          positionBaseIndex + triangles[j + 2].VertexIndex + 1 );

                                    }
                                }
                            }
                        }

                        positionBaseIndex += geometry.VertexPositions.Length;

                        if ( geometry.VertexNormals != null )
                            normalBaseIndex += geometry.VertexNormals.Length;
                    }

                    if ( node.Child != null )
                    {
                        ProcessNode( node.Child, worldTransform );
                    }

                    node = node.Sibling;
                }
            }

            ProcessNode( rootNode, Matrix4x4.Identity );

            _positionBaseIndex = positionBaseIndex;
            _normalBaseIndex = normalBaseIndex;
            _uvBaseIndex = uvBaseIndex;
        }

        public static void ExportObjGC( Node rootNode, string path )
        {
            using ( var writer = File.CreateText( path ) )
            {
                var positionBaseIndex = 0;
                var normalBaseIndex = 0;
                var uvBaseIndex = 0;

                ExportObjGC( rootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
            }
        }

        public static void ExportObj( LandTableSA2 landTable, string path )
        {
            using ( var writer = File.CreateText( path ) )
            {
                var positionBaseIndex = 0;
                var normalBaseIndex = 0;
                var uvBaseIndex = 0;

                foreach ( var model in landTable.Models )
                {
                    if ( model.RootNode != null )
                    {
                        var format = model.RootNode.EnumerateAllNodes().FirstOrDefault( x => x.Geometry != null )?.Geometry.Format;
                        if ( format == GeometryFormat.Basic || format == GeometryFormat.BasicDX )
                            ExportObjBasic( model.RootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
                        else if ( format == GeometryFormat.Chunk )
                            ExportObjChunk( model.RootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
                        else if ( format == GeometryFormat.GC )
                            ExportObjGC( model.RootNode, writer, ref positionBaseIndex, ref normalBaseIndex, ref uvBaseIndex );
                    }
                }
            }
        }

        public static void ExportObjGC( Node rootNode, StreamWriter writer, ref int _positionBaseIndex, ref int _normalBaseIndex, ref int _uvBaseIndex )
        {
            int nodeIndex = -1;
            int positionBaseIndex = _positionBaseIndex;
            int normalBaseIndex = _normalBaseIndex;
            int uvBaseIndex = _uvBaseIndex;

            void ProcessNode( Node node, Matrix4x4 parentTransform )
            {
                while ( node != null )
                {
                    ++nodeIndex;

                    var localTransform = node.Transform;
                    var worldTransform = localTransform * parentTransform;

                    var geometry = ( GC.Geometry )node.Geometry;
                    var positionCount = 0;
                    var normalCount = 0;
                    var uvCount = 0;
                    foreach ( var vertexBuffer in geometry.VertexBuffers )
                    {
                        switch ( vertexBuffer.Type )
                        {
                            case GC.VertexAttributeType.Position:
                                {
                                    var buffer = ( GC.VertexPositionBuffer )vertexBuffer;
                                    positionCount = buffer.ElementCount;
                                    foreach ( var element in buffer.Elements )
                                    {
                                        var pos = Vector3.Transform( element, worldTransform );
                                        writer.WriteLine( $"v {FormatFloat( pos.X )} {FormatFloat( pos.Y )} {FormatFloat( pos.Z )}" );
                                    }
                                }
                                break;
                            case GC.VertexAttributeType.Normal:
                                {
                                    var buffer = ( GC.VertexNormalBuffer )vertexBuffer;
                                    normalCount = buffer.ElementCount;
                                    foreach ( var element in buffer.Elements )
                                    {
                                        var nrm = Vector3.TransformNormal( element, worldTransform );
                                        writer
                                            .WriteLine( $"vn {FormatFloat( nrm.X )} {FormatFloat( nrm.Y )} {FormatFloat( nrm.Z )}" );
                                    }
                                }
                                break;
                            case GC.VertexAttributeType.Color:
                                break;
                            case GC.VertexAttributeType.UV:
                                {

                                    var buffer = ( GC.VertexUVBuffer )vertexBuffer;
                                    uvCount = buffer.ElementCount;
                                    foreach ( var element in buffer.Elements )
                                    {
                                        var decoded = UVCodec.Decode255( element );
                                        writer.WriteLine( $"vt {FormatFloat( decoded.X )} {FormatFloat( decoded.Y )}" );
                                    }
                                }
                                break;
                        }
                    }

                    GC.IndexAttributeFlags indexAttributeFlags = 0;

                    writer.WriteLine( $"g Geometry{nodeIndex}_{geometry.Format}_{geometry.SourceOffset:X8}" );

                    void ExportMesh( GC.Mesh mesh )
                    {
                        if ( mesh.Parameters != null )
                        {
                            foreach ( var param in mesh.Parameters )
                            {
                                if ( param.Type == GC.MeshStateParamType.IndexAttributeFlags )
                                    indexAttributeFlags = ( ( GC.IndexAttributeFlagsParam )param ).Flags;
                            }
                        }

                        for ( var displayListIndex = 0; displayListIndex < mesh.DisplayLists.Count; displayListIndex++ )
                        {
                            var displayList = mesh.DisplayLists[displayListIndex];
                            var triangles = displayList.ToTriangles();

                            for ( var i = 0; i < triangles.Length; i += 3 )
                            {

                                writer.Write( "f  " );

                                for ( int j = 0; j < 3; j++ )
                                {
                                    var index = triangles[i + j];

                                    writer.Write( $"{positionBaseIndex + index.PositionIndex + 1}" );

                                    if ( indexAttributeFlags.HasFlag( GC.IndexAttributeFlags.HasNormal ) )
                                        writer.Write( $"/{normalBaseIndex + index.NormalIndex + 1}" );
                                    else
                                        writer.Write( "//" );

                                    if ( indexAttributeFlags.HasFlag( GC.IndexAttributeFlags.HasUV ) )
                                        writer.Write( $"/{uvBaseIndex + index.UVIndex + 1}" );
                                    else
                                        writer.Write( "//" );

                                    writer.Write( " " );
                                }

                                writer.WriteLine();
                            }
                        }
                    }

                    foreach ( var mesh in geometry.OpaqueMeshes )
                    {
                        ExportMesh( mesh );
                    }

                    foreach ( var mesh in geometry.TranslucentMeshes )
                    {
                        ExportMesh( mesh );
                    }

                    positionBaseIndex += positionCount;
                    normalBaseIndex += normalCount;
                    uvBaseIndex += uvCount;

                    if ( node.Child != null )
                    {
                        ProcessNode( node.Child, worldTransform );
                    }

                    node = node.Sibling;
                }
            }

            ProcessNode( rootNode, Matrix4x4.Identity );

            _positionBaseIndex = positionBaseIndex;
            _normalBaseIndex = normalBaseIndex;
            _uvBaseIndex = uvBaseIndex;
        }

        private static string FormatFloat( float value )
        {
            return value.ToString( "F7", CultureInfo.InvariantCulture );
        }

        public static Node ConvertToChunk( Node node )
        {
            var objectCache = new Dictionary<object, object>();

            Node ConvertNodeToChunk( Node curNode )
            {
                var newNode = new Node();
                newNode.Flags = curNode.Flags;

                if ( curNode.Geometry != null )
                {
                    if ( !objectCache.TryGetValue( curNode.Geometry, out var geometry ) )
                    {
                        geometry = ConvertToChunk( ( ( Basic.Geometry )curNode.Geometry ) );
                        objectCache[curNode.Geometry] = geometry;
                    }

                    newNode.Geometry = ( IGeometry )geometry;
                }

                newNode.Translation = curNode.Translation;
                newNode.Rotation = curNode.Rotation;
                newNode.Scale = curNode.Scale;

                if ( curNode.Child != null )
                {
                    if ( !objectCache.TryGetValue( curNode.Child, out var child ) )
                    {
                        child = ConvertNodeToChunk( curNode.Child );
                        objectCache[curNode.Child] = child;
                    }

                    newNode.Child = ( Node )child;
                }

                if ( curNode.Sibling != null )
                {
                    if ( !objectCache.TryGetValue( curNode.Sibling, out var sibling ) )
                    {
                        sibling = ConvertNodeToChunk( curNode.Sibling );
                        objectCache[curNode.Sibling] = sibling;
                    }

                    newNode.Sibling = ( Node )sibling;
                }

                return newNode;
            }

            return ConvertNodeToChunk( node );
        }

        public static Geometry ConvertToChunk( Basic.Geometry basicGeometry )
        {
            var geometry = new Geometry();

            {
                // Build vertex chunk
                var vertices = new VertexN[basicGeometry.VertexCount];
                for ( int i = 0; i < vertices.Length; i++ )
                {
                    ref var vertex = ref vertices[i];
                    vertex.Position = basicGeometry.VertexPositions[i];
                    vertex.Normal = basicGeometry.VertexNormals[i];
                }

                geometry.VertexList.Add( new VertexNChunk( vertices ) );
            }

            {
                // Build polygon chunks
                foreach ( var mesh in basicGeometry.Meshes )
                {
                    var material = basicGeometry.Materials[mesh.MaterialId];

                    // Build material chunks
                    {
                        {
                            // Build material parameter chunk
                            var chunk = new MaterialDiffuseSpecularChunk
                            {
                                Diffuse = material.Diffuse,
                                Specular = new Color(material.Specular.R, material.Specular.G, material.Specular.B, (byte)material.Exponent ),
                                DestinationAlpha = material.DestinationAlpha,
                                SourceAlpha = material.SourceAlpha,
                            };
                            geometry.PolygonList.Add( chunk );
                        }

                        {
                            // Build texture id chunk
                            var chunk = new TextureIdChunk( ( short ) material.TextureId )
                            {
                                ClampU      = material.ClampU,
                                ClampV      = material.ClampV,
                                FilterMode  = material.FilterMode,
                                FlipU       = material.FlipU,
                                FlipV       = material.FlipV,
                                SuperSample = material.SuperSample,
                            };
                            geometry.PolygonList.Add( chunk );
                        }
                    }

                    // Build strip chunk
                    if ( mesh.PrimitiveType == PrimitiveType.Strips )
                    {
                        // Convert strip

                        if ( mesh.UVs == null )
                        {
                            var chunk = new StripChunk();
                            chunk.Strips = new Strip<StripIndex>[mesh.Primitives.Length];
                            for ( var i = 0; i < mesh.Primitives.Length; i++ )
                            {
                                var basicStrip = ( Strip )mesh.Primitives[i];
                                var strip = new Strip<StripIndex>( basicStrip.Reversed,
                                                                   basicStrip.Indices.Select( x => new StripIndex( x ) ).ToArray() );

                                chunk.Strips[i] = strip;
                            }

                            geometry.PolygonList.Add( chunk );
                        }
                        else
                        {
                            var chunk = new StripUVNChunk();
                            chunk.Strips = new Strip<StripIndexUVN>[mesh.Primitives.Length];
                            for ( var i = 0; i < mesh.Primitives.Length; i++ )
                            {
                                var basicStrip = ( Strip )mesh.Primitives[i];
                                var strip = new Strip<StripIndexUVN>( basicStrip.Reversed,
                                                                      basicStrip.Indices.Select( ( x, j ) => new StripIndexUVN( x, mesh.UVs[j] ) )
                                                                                .ToArray() );

                                chunk.Strips[i] = strip;
                            }

                            geometry.PolygonList.Add( chunk );
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }

            geometry.BoundingSphere = basicGeometry.Bounds;

            return geometry;
        }

        public static void ExportDae( Model model, string path )
        {
            var vertexCache = new Dictionary<int, Vertex>();
            var polygonCache = new Dictionary<int, (List<Chunk16> List, int Index)>();
            var currentTextureId = 0;
            int nodeIndex = -1;
            var textureToAssimpMaterialLookup = new Dictionary<int, int>();
            var nodes = model.RootNode.EnumerateAllNodes().ToList();

            var aiScene = new Assimp.Scene();

            IEnumerable<Assimp.Node> ConvertNodes( Node node, Assimp.Node aiParentNode, Matrix4x4 parentTransform )
            {
                while ( node != null )
                {
                    ++nodeIndex;

                    var aiNode = new Assimp.Node( nodeIndex.ToString(), aiParentNode ) { Transform = ToAssimp( node.Transform ) };

                    var worldTransform = node.Transform * parentTransform;
                    Matrix4x4.Invert( worldTransform, out var worldTransformInv );

                    var geometry = ( Geometry )node.Geometry;
                    if ( geometry != null )
                    {
                        var triangles = new List<(int CacheIndex, int UVIndex, int TextureId)>();

                        // Process vertices
                        foreach ( var chunk in geometry.VertexList )
                        {
                            switch ( chunk.Type )
                            {
                                case ChunkType.VertexXYZ:
                                    {
                                        var vertexChunk = ( VertexXYZChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var cacheVertex = new Vertex( position, new Vector3(), Color.White, nodeIndex, 1f );
                                            vertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                                        }
                                    }
                                    break;

                                case ChunkType.VertexN:
                                    {
                                        var vertexChunk = ( VertexNChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, 1f );
                                            vertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                                        }
                                    }
                                    break;

                                case ChunkType.VertexNNF:
                                    {
                                        var vertexChunk = ( VertexNNFChunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            var vertex = vertexChunk.Vertices[i];

                                            // Transform vertex
                                            var weightByte = vertex.NinjaFlags >> 16;
                                            var weight = weightByte * ( 1f / 255f );
                                            var position = Vector3.Transform( vertex.Position, worldTransform ) * weight;
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform ) * weight;

                                            // Store vertex in cache
                                            var vertexId = vertex.NinjaFlags & 0x0000FFFF;
                                            var vertexCacheId = ( int )( vertexChunk.BaseIndex + vertexId );

                                            if ( chunk.WeightStatus == WeightStatus.Start || !vertexCache.ContainsKey( vertexCacheId ) )
                                            {
                                                // Add new vertex to cache
                                                var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, weight );
                                                vertexCache[vertexCacheId] = cacheVertex;
                                            }
                                            else
                                            {
                                                // Update cached vertex
                                                var cacheVertex = vertexCache[vertexCacheId];
                                                cacheVertex.Position += position;
                                                cacheVertex.Normal += normal;
                                                cacheVertex.Weights.Add( new NodeWeight( nodeIndex, weight ) );
                                                vertexCache[vertexCacheId] = cacheVertex;

                                            }
                                        }
                                    }
                                    break;

                                case ChunkType.VertexD8888:
                                    {
                                        var vertexChunk = ( VertexD8888Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var cacheVertex = new Vertex( position, Vector3.Zero, vertex.Diffuse, nodeIndex, 1f );
                                            vertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                                        }
                                    }
                                    break;

                                case ChunkType.VertexND8888:
                                    {
                                        var vertexChunk = ( VertexND8888Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var normal = Vector3.TransformNormal( vertex.Normal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal, vertex.Diffuse, nodeIndex, 1f );
                                            vertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                                        }
                                    }
                                    break;

                                case ChunkType.VertexN32:
                                    {
                                        var vertexChunk = ( VertexN32Chunk )chunk;
                                        for ( var i = 0; i < vertexChunk.Vertices.Length; i++ )
                                        {
                                            // Transform and store vertex in cache
                                            var vertex = vertexChunk.Vertices[i];
                                            var position = Vector3.Transform( vertex.Position, worldTransform );
                                            var decodedNormal = NormalCodec.Decode( vertex.Normal );
                                            var normal = Vector3.TransformNormal( decodedNormal, worldTransform );
                                            var cacheVertex = new Vertex( position, normal, Color.White, nodeIndex, 1f );
                                            vertexCache[vertexChunk.BaseIndex + i] = cacheVertex;
                                        }
                                    }
                                    break;

                                default:
                                    Console.WriteLine( chunk.Type );
                                    break;
                            }
                        }

                        void ProcessPolygonList( int index, List<Chunk16> list, bool wasCached = false )
                        {
                            for ( var chunkIndex = index; chunkIndex < list.Count; chunkIndex++ )
                            {
                                var chunk = list[chunkIndex];

                                switch ( chunk.Type )
                                {
                                    case ChunkType.CachePolygonList:
                                        {
                                            if ( wasCached )
                                                throw new InvalidOperationException( "CachePolygonList in cached polygon list" );

                                            var cacheChunk = ( CachePolygonListChunk )chunk;
                                            polygonCache[ cacheChunk.CacheIndex ] = ( list, chunkIndex + 1 );
                                        }
                                        break;

                                    case ChunkType.DrawPolygonList:
                                        {
                                            if ( wasCached )
                                                throw new InvalidOperationException( "DrawPolygonList in cached polygon list" );

                                            var drawChunk = ( DrawPolygonListChunk )chunk;
                                            if ( !polygonCache.ContainsKey( drawChunk.CacheIndex ) )
                                                continue;
   
                                            var cachedList = polygonCache[drawChunk.CacheIndex];
                                            ProcessPolygonList( cachedList.Index, cachedList.List, true );
                                        }
                                        break;

                                    case ChunkType.MaterialAmbient:
                                    case ChunkType.MaterialAmbient2:
                                    case ChunkType.MaterialAmbientSpecular:
                                    case ChunkType.MaterialAmbientSpecular2:
                                    case ChunkType.MaterialBump:
                                    case ChunkType.MaterialDiffuse:
                                    case ChunkType.MaterialDiffuse2:
                                    case ChunkType.MaterialDiffuseAmbient:
                                    case ChunkType.MaterialDiffuseAmbient2:
                                    case ChunkType.MaterialDiffuseAmbientSpecular:
                                    case ChunkType.MaterialDiffuseAmbientSpecular2:
                                    case ChunkType.MaterialDiffuseSpecular:
                                    case ChunkType.MaterialDiffuseSpecular2:
                                    case ChunkType.MaterialSpecular:
                                    case ChunkType.MaterialSpecular2:
                                    case ChunkType.MipmapDAdjust:
                                    case ChunkType.SpecularExponent:
                                    case ChunkType.TextureId2:
                                    case ChunkType.BlendAlpha:
                                        break;

                                    case ChunkType.TextureId:
                                        {
                                            var textureIdChunk = ( TextureIdChunk )chunk;
                                            currentTextureId = textureIdChunk.Id;
                                        }
                                        break;

                                    case ChunkType.StripUVN:
                                        {
                                            var stripChunk = ( StripUVNChunk )chunk;
                                            var triangleIndices = stripChunk.ToTriangles();
                                            foreach ( var stripIndex in triangleIndices )
                                            {
                                                if ( !vertexCache.ContainsKey( stripIndex.Index ) )
                                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );

                                                var cachedVertex = vertexCache[stripIndex.Index];
                                                var uv = UVCodec.Decode255( stripIndex.UV );
                                                int uvIndex = cachedVertex.UVs.IndexOf( uv );
                                                if ( uvIndex == -1 )
                                                {
                                                    uvIndex = cachedVertex.UVs.Count;
                                                    cachedVertex.UVs.Add(uv);
                                                }

                                                triangles.Add( ( stripIndex.Index, uvIndex, currentTextureId ) );
                                                vertexCache[stripIndex.Index] = cachedVertex;
                                            }
                                        }
                                        break;

                                    case ChunkType.Strip:
                                        {
                                            var stripChunk = ( StripChunk )chunk;
                                            var triangleIndices = stripChunk.ToTriangles();
                                            foreach ( var stripIndex in triangleIndices )
                                            {
                                                if ( !vertexCache.ContainsKey( stripIndex.Index ) )
                                                    throw new InvalidOperationException( "Strip referenced vertex that is not in the vertex cache" );

                                                triangles.Add( (stripIndex.Index, 0, currentTextureId) );
                                            }
                                        }
                                        break;

                                    default:
                                        Console.WriteLine( chunk.Type );
                                        break;
                                }

                                if ( chunk.Type == ChunkType.CachePolygonList )
                                {
                                    break;
                                }
                            }
                        }

                        // Process polygon list, contains material & triangle data
                        ProcessPolygonList( 0, geometry.PolygonList );

                        // Build assimp mesh. Group the triangles by their material texture id.
                        var meshTriangleListsByTextureId = triangles.GroupBy( x => x.TextureId );
                        foreach ( var meshTriangleListGroup in meshTriangleListsByTextureId )
                        {
                            var aiMesh = new Assimp.Mesh();

                            // Take the triangles that belong to this material and extract the referenced vertices
                            // Obviously the indices won't make sense anymore because we're splitting the meshes up into parts with only 1 material
                            // Soo just take it all apart and let Assimp handle regenerating the vertex cache
                            var meshTriangles = meshTriangleListGroup.ToList();
                            var vertexWeights = new List<List<NodeWeight>>();
                            for ( int i = 0; i < meshTriangles.Count; i += 3 )
                            {
                                var aiFace = new Assimp.Face();

                                for ( int j = 0; j < 3; j++ )
                                {
                                    var index = i + j;
                                    var cachedVertex = vertexCache[meshTriangles[index].CacheIndex];
                                    var uvIndex = meshTriangles[index].UVIndex;
                                    Vector2 uv = new Vector2();
                                    if ( uvIndex < cachedVertex.UVs.Count )
                                        uv = cachedVertex.UVs[ uvIndex ];

                                    // Need to convert vertex positions and normals back to model space
                                    aiMesh.Vertices.Add( ToAssimp( Vector3.Transform( cachedVertex.Position, worldTransformInv ) ) );
                                    aiMesh.Normals.Add( ToAssimp( Vector3.TransformNormal( cachedVertex.Normal, worldTransformInv ) ) );

                                    aiMesh.TextureCoordinateChannels[0].Add( ToAssimp( uv ) );
                                    aiMesh.VertexColorChannels[0].Add( AssimpHelper.ToAssimp( cachedVertex.Color ) );

                                    // We need to do some processing on the weights so add them to a seperate list
                                    vertexWeights.Add( cachedVertex.Weights );

                                    // RIP cache efficiency
                                    aiFace.Indices.Add( index );
                                }

                                aiMesh.Faces.Add( aiFace );
                            }

                            // Convert vertex weights
                            var aiBoneMap = new Dictionary<int, Assimp.Bone>();
                            for ( int i = 0; i < vertexWeights.Count; i++ )
                            {
                                for ( int j = 0; j < vertexWeights[i].Count; j++ )
                                {
                                    var vertexWeight = vertexWeights[i][j];

                                    if ( !aiBoneMap.TryGetValue( vertexWeight.NodeIndex, out var aiBone ) )
                                    {
                                        aiBone      = aiBoneMap[vertexWeight.NodeIndex] = new Assimp.Bone();
                                        aiBone.Name = vertexWeight.NodeIndex.ToString();

                                        // Offset matrix: difference between world transform of weighted bone node and the world transform of the mesh's parent node
                                        Matrix4x4.Invert( nodes[vertexWeight.NodeIndex].WorldTransform * worldTransformInv, out var offsetMatrix );
                                        aiBone.OffsetMatrix = ToAssimp( offsetMatrix );
                                    }

                                    // Assimps way of storing weights is not very efficient
                                    aiBone.VertexWeights.Add( new Assimp.VertexWeight( i, vertexWeight.Weight ) );
                                }
                            }

                            aiMesh.Bones.AddRange( aiBoneMap.Values );

                            // Check if a material for the texture has already been created
                            if ( !textureToAssimpMaterialLookup.TryGetValue( meshTriangleListGroup.Key, out var materialIndex ) )
                            {
                                materialIndex = aiScene.Materials.Count;

                                // Lazy simple material
                                var material = new Assimp.Material();
                                material.Name = $"texture{meshTriangleListGroup.Key}";
                                material.TextureDiffuse = new Assimp.TextureSlot() { FilePath = $"{meshTriangleListGroup.Key}.png" };

                                aiScene.Materials.Add( material );
                                textureToAssimpMaterialLookup.Add( meshTriangleListGroup.Key, materialIndex );
                            }

                            aiMesh.MaterialIndex = materialIndex;
                            aiNode.MeshIndices.Add( aiScene.Meshes.Count );
                            aiScene.Meshes.Add( aiMesh );
                        }

                    }

                    if ( node.Child != null )
                    {
                        // Convert child nodes
                        foreach ( var aiChildNode in ConvertNodes( node.Child, aiNode, worldTransform ) )
                            aiNode.Children.Add( aiChildNode );
                    }

                    // Return currently converted node
                    yield return aiNode;

                    node = node.Sibling;
                }
            }

            aiScene.RootNode = new Assimp.Node( "RootNode" );
            foreach ( var node in ConvertNodes( model.RootNode, aiScene.RootNode, Matrix4x4.Identity ) )
                aiScene.RootNode.Children.Add( node );

            var aiContext = new Assimp.AssimpContext();
            aiContext.ExportFile( aiScene, path, "collada", Assimp.PostProcessSteps.JoinIdenticalVertices );
        }

        public static Assimp.Matrix4x4 ToAssimp( Matrix4x4 matrix )
        {
            return new Assimp.Matrix4x4( matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                                         matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                                         matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                                         matrix.M14, matrix.M24, matrix.M34, matrix.M44 );
        }

        public static Assimp.Vector3D ToAssimp( Vector3 value )
        {
            return new Assimp.Vector3D( value.X, value.Y, value.Z );
        }

        public static Assimp.Vector3D ToAssimp( Vector2 value )
        {
            return new Assimp.Vector3D( value.X, value.Y, 0 );
        }

    }

    internal class SetObjectDefinition
    {
        public string Name { get; internal set; }
        public List<SetObjectModel> Models { get; internal set; }
    }

    public class AssimpConverter
    {

    }
}

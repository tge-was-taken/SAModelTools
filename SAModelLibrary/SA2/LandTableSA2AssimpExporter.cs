using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SAModelLibrary.GeometryFormats;
using SAModelLibrary.GeometryFormats.Basic;
using SAModelLibrary.GeometryFormats.Chunk;
using SAModelLibrary.GeometryFormats.GC;

namespace SAModelLibrary.SA2
{
    public class LandTableSA2AssimpExporter : AssimpExporter
    {
        public static readonly LandTableSA2AssimpExporter Default = new LandTableSA2AssimpExporter();

        private static readonly Dictionary<SurfaceFlags, string> sTags = new Dictionary<SurfaceFlags, string>()
        {
            { SurfaceFlags.Collidable, "C" },
        };

        private BasicAssimpExporter mBasicExporter;
        private ChunkAssimpExporter mChunkExporter;
        private GCAssimpExporter mGCExporter;
        private LandModelSA2 mCurrentModel;

        public LandTableSA2AssimpExporter()
        {
            AttachMeshesToParentNode = false;
            RemoveNodes = true;
        }

        public void Export( LandTableSA2 landTable, string filePath )
        {
            TextureNames = landTable.Textures.Select( x => x.Name ).ToList();
            Scene        = CreateDefaultScene();
            Initialize( null );

            foreach ( var model in landTable.Models )
            {
                mCurrentModel = model;
                ConvertRootNode( model.RootNode );
            }

            ExportCollada( Scene, filePath );
        }

        protected override void Initialize( Node rootNode )
        {
            mBasicExporter = new BasicAssimpExporter { RemoveNodes = true, AttachMeshesToParentNode = false };
            mChunkExporter = new ChunkAssimpExporter{ RemoveNodes = true, AttachMeshesToParentNode = false };
            mGCExporter = new GCAssimpExporter { RemoveNodes = true, AttachMeshesToParentNode = false };
        }

        protected override void ConvertGeometry( IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform )
        {
            switch ( iGeometry.Format )
            {
                case GeometryFormat.Basic:
                case GeometryFormat.BasicDX:
                    // Collision
                    ConvertGeometry( Scene, TextureNames, mBasicExporter, iGeometry, ref nodeWorldTransform );
                    break;
                case GeometryFormat.Chunk:
                    ConvertGeometry( Scene, TextureNames, mChunkExporter, iGeometry, ref nodeWorldTransform );
                    break;
                case GeometryFormat.GC:
                    ConvertGeometry( Scene, TextureNames, mGCExporter, iGeometry, ref nodeWorldTransform );
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private static string BuildSurfaceFlagsTags( SurfaceFlags flags )
        {
            var tags = new StringBuilder();

            for ( int j = 0; j < 32; j++ )
            {
                var flag    = ( SurfaceFlags )( 1u << j );
                var hasFlag = flags.HasFlagFast( flag );

                if ( hasFlag )
                {
                    if ( !sTags.TryGetValue( flag, out var tag ) )
                        tag = $"B{j}";

                    tags.Append( $"@{tag}" );
                }
                else
                {
                    if ( flag == SurfaceFlags.Visible )
                        tags.Append( "@H" );
                }
            }

            return tags.ToString();
        }

        protected override string FormatMeshName( int meshIndex )
        {
            return base.FormatMeshName( meshIndex ) + BuildSurfaceFlagsTags( mCurrentModel.Flags );
        }
    }
}

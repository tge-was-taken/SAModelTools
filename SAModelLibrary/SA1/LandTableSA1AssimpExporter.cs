using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using SAModelLibrary.GeometryFormats.Basic;

namespace SAModelLibrary.SA1
{
    public class LandTableSA1AssimpExporter : BasicAssimpExporter
    {
        public new static readonly LandTableSA1AssimpExporter Default = new LandTableSA1AssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = true,
        };

        private static readonly Dictionary<SurfaceFlags, string> sTags = new Dictionary<SurfaceFlags, string>()
        {
            { SurfaceFlags.Collidable, "C" },
            { SurfaceFlags.Water, "W" },
            { SurfaceFlags.NoFriction, "NF" },
            { SurfaceFlags.NoAcceleration, "NA" },
            { SurfaceFlags.CannotLand, "NL" },
            { SurfaceFlags.IncreasedAcceleration, "A" },
            { SurfaceFlags.Diggable, "D" },
            { SurfaceFlags.Unclimbable, "NC" },
            { SurfaceFlags.Hurt, "H" },
            { SurfaceFlags.Footprints, "FP" },
        };

        private LandModelSA1 mCurrentModel;

        public void Export( LandTableSA1 landTable, string filePath )
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

using SAModelLibrary.GeometryFormats.Basic;

namespace SAModelLibrary
{
    public class DeathZoneListAssimpExporter : BasicAssimpExporter
    {
        public static readonly DeathZoneListAssimpExporter Default = new DeathZoneListAssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = true,
        };

        private int       mDeathZoneIndex;
        private DeathZone mDeathZone;
        private int       mDeathZoneMeshBaseIndex;

        public void Export( DeathZoneList deathZones, string filePath )
        {
            Scene = CreateDefaultScene();
            Initialize( null );

            for ( var i = 0; i < deathZones.Count; i++ )
            {
                var deathZone = deathZones[ i ];
                mDeathZoneIndex = i;
                mDeathZone      = deathZone;
                mDeathZoneMeshBaseIndex  = Scene.Meshes.Count;
                ConvertRootNode( deathZone.RootNode );
            }

            ExportCollada( Scene, filePath );
        }

        protected override string FormatMeshName( int meshIndex )
        {
            // death_zone_0_mesh_0, etc
            return $"death_zone_{mDeathZoneIndex}_mesh_{meshIndex - mDeathZoneMeshBaseIndex}@DZ";
        }
    }
}
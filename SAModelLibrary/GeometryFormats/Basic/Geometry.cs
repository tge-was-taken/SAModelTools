using System;
using System.Numerics;
using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Basic geometry format. 
    /// </summary>
    public class Geometry : IGeometry
    {
        private bool mUsesDXLayout;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public GeometryFormat Format { get; private set; } = GeometryFormat.Basic;

        /// <summary>
        /// Gets or sets whether the DX format layout is used.
        /// </summary>
        public bool UsesDXLayout
        {
            get => mUsesDXLayout;
            set
            {
                mUsesDXLayout = value;
                Format = value ? GeometryFormat.BasicDX : GeometryFormat.Basic;
            }
        }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the vertex positions.
        /// </summary>
        public Vector3[] VertexPositions { get; set; }

        /// <summary>
        /// Gets or sets the vertex normals.
        /// </summary>
        public Vector3[] VertexNormals { get; set; }

        /// <summary>
        /// Gets the number of vertices stored in the geometry.
        /// </summary>
        public int VertexCount => VertexPositions?.Length ?? 0;

        /// <summary>
        /// Gets or sets the mesh array.
        /// </summary>
        public Mesh[] Meshes { get; set; }

        /// <summary>
        /// Gets or sets the material array.
        /// </summary>
        public Material[] Materials { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere.
        /// </summary>
        public BoundingSphere Bounds { get; set; }

        /// <summary>
        /// Gets whether the mesh has vertex positions or not.
        /// </summary>
        public bool HasPositions => VertexPositions != null && VertexPositions.Length > 0;

        /// <summary>
        /// Gets whether the geometry has vertex normals or not.
        /// </summary>
        public bool HasNormals => VertexNormals != null && VertexNormals.Length > 0;

        /// <summary>
        /// Gets whether the geometry has meshes or not.
        /// </summary>
        public bool HasMeshes => Meshes != null && Meshes.Length > 0;

        /// <summary>
        /// Gets whether the geometry has material or not.
        /// </summary>
        public bool HasMaterials => Materials != null && Materials.Length > 0;

        public Geometry()
        {
        }

        public static bool Validate(EndianBinaryReader reader )
        {
            var start = reader.Position;

            try
            {
                var vertexListOffset   = reader.ReadInt32();
                var normalListOffset   = reader.ReadInt32();
                var vertexCount        = reader.ReadInt32();
                var meshListOffset     = reader.ReadInt32();
                var materialListOffset = reader.ReadInt32();
                var meshCount          = reader.ReadInt16();
                var materialCount      = reader.ReadInt16();

                // Check if vertex data is valid
                if ( !reader.IsValidOffset( vertexListOffset ) || vertexListOffset != 0 && vertexCount == 0 )
                    return false;

                // Check if vertex normal list is valid
                if ( !reader.IsValidOffset( normalListOffset ) || normalListOffset != 0 && vertexCount == 0 )
                    return false;

                // Sanity check vertex count
                if ( vertexCount < 0 || ( vertexListOffset == 0 && vertexCount > 0 ) || vertexCount >= 10_000 )
                    return false;

                // Validate mesh list offset
                if ( meshListOffset == 0 || !reader.IsValidOffset( meshListOffset ) || meshListOffset != 0 && meshCount == 0 )
                    return false;

                // Validate material list offset. Note: one particular model has a valid material list offset with 0 materials
                if ( !reader.IsValidOffset( materialListOffset ) )
                    return false;

                // Sanity check mesh count
                if ( meshCount < 0 || ( meshCount > 0 && meshListOffset == 0 ) || meshCount >= 1000 )
                    return false;

                // Sanity check material count
                if ( materialCount < 0 || ( materialCount > 0 && materialListOffset == 0 ) || materialCount >= 1000 )
                    return false;

                // Check if all of the data is 0
                if ( vertexListOffset == 0 && normalListOffset == 0 && vertexCount == 0 && meshListOffset == 0 && materialListOffset == 0 &&
                     meshCount == 0 && materialCount == 0 )
                {
                    return false;
                }

                return true;
            }
            finally
            {
                reader.Position = start;
            }
        }

        public void Read( EndianBinaryReader reader, bool usesDXLayout )
        {
            UsesDXLayout = usesDXLayout;
            var vertexListOffset   = reader.ReadInt32();
            var normalListOffset   = reader.ReadInt32();
            var vertexCount        = reader.ReadInt32();
            var meshListOffset     = reader.ReadInt32();
            var materialListOffset = reader.ReadInt32();
            var meshCount          = reader.ReadInt16();
            var materialCount      = reader.ReadInt16();
            Bounds = reader.ReadBoundingSphere();

            reader.ReadAtOffset( vertexListOffset, () => VertexPositions = reader.ReadVector3s( vertexCount ) );

            if ( reader.IsValidOffset(normalListOffset ))
                reader.ReadAtOffset( normalListOffset, () => VertexNormals   = reader.ReadVector3s( vertexCount ) );

            reader.ReadAtOffset( meshListOffset, () =>
            {
                Meshes = new Mesh[meshCount];
                for ( int i = 0; i < Meshes.Length; i++ )
                    Meshes[i] = reader.ReadObject<Mesh>( UsesDXLayout );
            } );
            reader.ReadAtOffset( materialListOffset, () =>
            {
                Materials = new Material[materialCount];
                for ( int i = 0; i < Materials.Length; i++ )
                    Materials[i] = reader.ReadObject<Material>();
            } );

            if ( UsesDXLayout )
            {
                var unused = reader.ReadInt32();
                if ( unused != 0 )
                    throw new NotImplementedException( $"Basic DX geometry unused field is not 0: {unused}" );
            }
        }

        public void Write( EndianBinaryWriter writer )
        {
            var positionCount = writer.ScheduleWriteArrayOffset( VertexPositions, 16, writer.Write );
            var normalCount = writer.ScheduleWriteArrayOffset( VertexNormals, 16, writer.Write );
            var vertexCount = Math.Max( positionCount, normalCount );

            writer.Write( vertexCount );

            var meshCount = writer.ScheduleWriteArrayOffset( Meshes, 16, x => writer.WriteObject( x, UsesDXLayout ) );
            var materialCount = writer.ScheduleWriteArrayOffset( Materials, 16, x => writer.WriteObject( x ) );

            writer.Write( ( short )meshCount );
            writer.Write( ( short )materialCount );
            writer.Write( Bounds );

            if ( UsesDXLayout )
                writer.Write( 0 ); // unused
        }

        /// <inheritdoc />
        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader, ( bool ) context );

        /// <inheritdoc />
        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}

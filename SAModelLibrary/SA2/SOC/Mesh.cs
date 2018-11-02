using System;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2.SOC
{
    public class Mesh : ISerializableObject
    {
        private Material mMaterial;
        private Vertex[] mVertices;
        private int[]    mIndices;

        public string     SourceFilePath   { get; set; }
        public long       SourceOffset     { get; set; }
        public Endianness SourceEndianness { get; set; }

        public Material Material
        {
            get => mMaterial;
            set => mMaterial = value ?? new Material();
        }

        public Vertex[] Vertices
        {
            get => mVertices;
            set => mVertices = value ?? throw new ArgumentNullException( nameof( value ) );
        }

        public int[] Indices
        {
            get => mIndices;
            set => mIndices = value ?? throw new ArgumentNullException( nameof( value ) );
        }

        public Mesh()
        {
            Material = new Material();
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Material = reader.ReadObject<Material>();

            var vertexCount = reader.ReadInt32();
            Vertices = new Vertex[vertexCount];
            for ( int i = 0; i < Vertices.Length; i++ )
            {
                ref var vertex = ref Vertices[ i ];
                vertex.Position = reader.ReadVector3();
                vertex.Normal   = reader.ReadVector3();
                vertex.Color    = reader.ReadColor();
                vertex.UV       = reader.ReadVector2();
            }

            var indexCount = reader.ReadInt32();
            Indices = reader.ReadInt32s( indexCount );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.WriteObject( Material );

            if ( mVertices == null )
            {
                writer.Write( 0 );
            }
            else
            {
                writer.Write( mVertices.Length );
                foreach ( var vertex in mVertices )
                {
                    writer.Write( vertex.Position );
                    writer.Write( vertex.Normal );
                    writer.Write( vertex.Color );
                    writer.Write( vertex.UV );
                }
            }

            if ( mIndices == null )
            {
                writer.Write( 0 );
            }
            else
            {
                writer.Write( Indices.Length );
                writer.Write( Indices );
            }
        }
    }
}
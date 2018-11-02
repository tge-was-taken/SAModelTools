using System.Collections.Generic;
using SAModelLibrary.GeometryFormats.Chunk;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2
{
    /// <summary>
    /// SA2 model structure containing the unique id and the root node of the model.
    /// </summary>
    public class Model : ISerializableObject
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the unique id for this model.
        /// </summary>
        public int UID { get; set; }

        /// <summary>
        /// Gets or sets the root node of the model's hierarchy.
        /// </summary>
        public Node RootNode { get; set; }

        public Model()
        {    
        }

        public Model(int uid, Node rootNode)
        {
            UID      = uid;
            RootNode = rootNode;
        }

        public void Export( string path, List<string> textureNames = null )
        {
            ChunkAssimpExporter.Animated.Export( RootNode, path, textureNames );
        }


        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            UID      = reader.ReadInt32();
            RootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( GeometryFormat.Chunk ) );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( UID );
            writer.ScheduleWriteObjectOffset( RootNode );
        }
    }
}
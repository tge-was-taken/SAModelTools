using System.Collections;
using System.Collections.Generic;
using System.IO;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2
{
    /// <summary>
    /// Represents an SA2 model list structure used to store model entries in a model file.
    /// </summary>
    public class ModelList : ISerializableObject, IList<Model>
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets the list of the models stored in the model list.
        /// </summary>
        public List<Model> Models { get; private set; }

        public ModelList()
        {
            Init();
        }

        public ModelList( string filepath )
        {
            Init();

            using ( var reader = new EndianBinaryReader( filepath, Endianness.Big ) )
                Read( reader );
        }

        public ModelList( Stream stream, bool leaveOpen )
        {
            Init();

            using ( var reader = new EndianBinaryReader( stream, leaveOpen, Endianness.Big ) )
                Read( reader );
        }

        public void Save( string filepath ) => Save( filepath, SourceEndianness );

        public void Save( string filepath, Endianness endianness )
        {
            using ( var writer = new EndianBinaryWriter( filepath, endianness ) )
                Write( writer );
        }

        public void Save( Stream stream, bool leaveOpen = true ) => Save( stream, leaveOpen, SourceEndianness );

        public void Save( Stream stream, bool leaveOpen, Endianness endianness )
        {
            using ( var writer = new EndianBinaryWriter( stream, leaveOpen, endianness ) )
                Write( writer );
        }

        public MemoryStream Save() => Save( SourceEndianness );

        public MemoryStream Save( Endianness endianness )
        {
            var stream = new MemoryStream();
            Save( stream );
            stream.Position = 0;
            return stream;
        }

        private void Init()
        {
            Models = new List<Model>();
        }

        private void Read( EndianBinaryReader reader )
        {
            SourceEndianness = DetectEndianness( reader );

            while ( true )
            {
                var model = reader.ReadObject<Model>();
                if ( model.UID == -1 )
                    break;

                Models.Add( model );
            }
        }

        private void Write( EndianBinaryWriter writer )
        {
            Models.ForEach( x => writer.WriteObject( x ) );

            // Write list terminator
            writer.Write( -1 );
            writer.Write( 0 );
        }

        private static Endianness DetectEndianness( EndianBinaryReader reader )
        {
            var id = reader.ReadInt32();
            var offset = reader.ReadInt32();
            if ( ( id & 0xFF000000 ) != 0 || ( offset & 0xFF000000 ) != 0 )
            {
                reader.Endianness = ( Endianness ) ( ( int ) reader.Endianness ^ 1 );
            }

            reader.Position = 0;
            return reader.Endianness;
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader );
        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );

        #region IList implementation
        public IEnumerator<Model> GetEnumerator()
        {
            return Models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) Models ).GetEnumerator();
        }

        public void Add( Model item )
        {
            Models.Add( item );
        }

        public void Clear()
        {
            Models.Clear();
        }

        public bool Contains( Model item )
        {
            return Models.Contains( item );
        }

        public void CopyTo( Model[] array, int arrayIndex )
        {
            Models.CopyTo( array, arrayIndex );
        }

        public bool Remove( Model item )
        {
            return Models.Remove( item );
        }

        public int Count => Models.Count;

        public bool IsReadOnly => false;

        public int IndexOf( Model item )
        {
            return Models.IndexOf( item );
        }

        public void Insert( int index, Model item )
        {
            Models.Insert( index, item );
        }

        public void RemoveAt( int index )
        {
            Models.RemoveAt( index );
        }

        public Model this[ int index ]
        {
            get => Models[ index ];
            set => Models[ index ] = value;
        }
        #endregion
    }
}

using System.Collections;
using System.Collections.Generic;
using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Represents a list of texture references.
    /// </summary>
    public class TextureReferenceList : IList<TextureReference>, ISerializableObject
    {
        private readonly List<TextureReference> mList;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Initializes a new empty instance of <see cref="TextureReferenceList"/>.
        /// </summary>
        public TextureReferenceList()
        {
            mList = new List<TextureReference>();
        }

        #region IList implementation
        public IEnumerator<TextureReference> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) mList ).GetEnumerator();
        }

        public void Add( TextureReference item )
        {
            mList.Add( item );
        }

        public void Clear()
        {
            mList.Clear();
        }

        public bool Contains( TextureReference item )
        {
            return mList.Contains( item );
        }

        public void CopyTo( TextureReference[] array, int arrayIndex )
        {
            mList.CopyTo( array, arrayIndex );
        }

        public bool Remove( TextureReference item )
        {
            return mList.Remove( item );
        }

        public int Count => mList.Count;

        public bool IsReadOnly => false;

        public int IndexOf( TextureReference item )
        {
            return mList.IndexOf( item );
        }

        public void Insert( int index, TextureReference item )
        {
            mList.Insert( index, item );
        }

        public void RemoveAt( int index )
        {
            mList.RemoveAt( index );
        }

        public TextureReference this[ int index ]
        {
            get => mList[ index ];
            set => mList[ index ] = value;
        }

        #endregion

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            var offset = reader.ReadInt32();
            var count  = reader.ReadInt32();

            reader.ReadAtOffset( offset, () =>
            {
                var baseOffset = reader.BaseOffset;

                if ( context != null )
                    reader.BaseOffset = ( long ) context;

                for ( int i = 0; i < count; i++ )
                    mList.Add( reader.ReadObject<TextureReference>() );

                reader.BaseOffset = baseOffset;
            } );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.ScheduleWriteOffset( () =>
            {
                foreach ( var reference in mList )
                    writer.WriteObject( reference );
            });
            writer.Write( mList.Count );
        }
    }
}
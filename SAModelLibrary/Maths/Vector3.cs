namespace SAModelLibrary.Maths
{
    /// <summary>
    /// Generic vector 3 structure for arbitrary data types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Vector3<T> where T : struct
    {
        /// <summary>
        /// X component of the <see cref="Vector3{T}"/>
        /// </summary>
        public T X;

        /// <summary>
        /// Y component of the <see cref="Vector3{T}"/>
        /// </summary>
        public T Y;

        /// <summary>
        /// Z component of the <see cref="Vector3{T}"/>
        /// </summary>
        public T Z;

        public Vector3( T x, T y, T z )
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
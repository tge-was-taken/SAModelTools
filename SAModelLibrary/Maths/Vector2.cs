namespace SAModelLibrary.Maths
{
    /// <summary>
    /// Generic vector 2 structure for arbitrary data types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct Vector2<T> where T : struct
    {
        /// <summary>
        /// X component of the <see cref="Vector2{T}"/>
        /// </summary>
        public T X;

        /// <summary>
        /// Y component of the <see cref="Vector2{T}"/>
        /// </summary>
        public T Y;

        public Vector2( T x, T y )
        {
            X = x;
            Y = y;
        }
    }
}
using SAModelLibrary;

namespace SA2ModelConverter
{
    internal class SetObjectModel
    {
        public object TextureReferenceList { get; set; }
        public ModelFormat ModelFormat { get; set; }
        public GeometryFormat GeometryFormat { get; set; }
        public object Model { get; set; }
    }

    public enum ModelFormat
    {
        Unknown,
        Model,
        Geometry
    }
}
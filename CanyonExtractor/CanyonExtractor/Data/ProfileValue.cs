namespace CanyonExtractor.Data
{
    class ProfileValue
    {
        private object altitude;
        private bool vertex;
        private Geometry geometry;

        public object Altitude { get => altitude; set => altitude = value; }
        public bool Vertex { get => vertex; set => vertex = value; }
        public Geometry Geometry { get => geometry; set => geometry = value; }
    }
}

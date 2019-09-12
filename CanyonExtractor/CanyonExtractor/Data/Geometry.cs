namespace CanyonExtractor.Data
{
    class Geometry
    {
        private string type;

        private object coordinates;

        public string Type { get => type; set => type = value; }
        public object Coordinates { get => coordinates; set => coordinates = value; }
    }
}

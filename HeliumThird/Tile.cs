namespace HeliumThird
{
    struct Tile
    {
        public enum ModelType { Pit, Ground, Wall, TransparentWall }

        public ModelType Type;
        public int LayerBottom;
        public int LayerTop;
        public int LayerDecoration;
    }
}

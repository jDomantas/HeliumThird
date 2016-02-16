using Lidgren.Network;

namespace HeliumThird.Events
{
    public class MapData : Event
    {
        public Tile[,] TileData;

        internal MapData(Map map, int x, int y)
        {
            TileData = new Tile[Map.MapChunkSize, Map.MapChunkSize];
            for (int xx = 0; xx < Map.MapChunkSize; xx++)
                for (int yy = 0; yy < Map.MapChunkSize; yy++)
                {
                    TileData[xx, yy] = new Tile();
                    TileData[xx, yy].Type = map.Tiles[x + xx, y + yy].Type;
                    TileData[xx, yy].LayerBottom = map.Tiles[x + xx, y + yy].LayerBottom;
                    TileData[xx, yy].LayerTop = map.Tiles[x + xx, y + yy].LayerTop;
                    TileData[xx, yy].LayerDecoration = map.Tiles[x + xx, y + yy].LayerDecoration;
                }
        }

        public MapData(NetIncomingMessage msg, Player sender) : base(sender)
        {
            TileData = new Tile[Map.MapChunkSize, Map.MapChunkSize];
            for (int x = 0; x < Map.MapChunkSize; x++)
                for (int y = 0; y < Map.MapChunkSize; y++)
                {
                    TileData[x, y].Type = (Tile.ModelType)msg.ReadInt32();
                    TileData[x, y].LayerBottom = msg.ReadInt32();
                    TileData[x, y].LayerTop = msg.ReadInt32();
                    TileData[x, y].LayerDecoration = msg.ReadInt32();
                }
        }

        public override void Serialize(NetOutgoingMessage msg)
        {
            for (int x = 0; x < Map.MapChunkSize; x++)
                for (int y = 0; y < Map.MapChunkSize; y++)
                {
                    msg.Write((int)TileData[x, y].Type);
                    msg.Write(TileData[x, y].LayerBottom);
                    msg.Write(TileData[x, y].LayerTop);
                    msg.Write(TileData[x, y].LayerDecoration);
                }
        }
    }
}

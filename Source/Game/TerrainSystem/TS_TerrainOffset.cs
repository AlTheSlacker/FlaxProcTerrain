using FlaxEngine;

namespace TerrainSystem
{
    public class TS_TerrainOffset(Terrain _terrain, float _terrainOffset = 0) : Script
    {
        private Terrain terrain = _terrain;
        private readonly float terrainOffset = _terrainOffset;

        public void TerrainOffset()
        {

            Int2 fhmDims = TS_Util.GetFHMDims(ref terrain);
            float[] fullHM = TS_Util.TerrainToFullHeightMap(ref terrain);
            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int x = 0; x < fhmDims.X; x++)
                {
                    fullHM[y * fhmDims.X + x] = fullHM[y * fhmDims.X + x] - terrainOffset;
                }
            }
            TS_Util.FullHeightMapToTerrain(ref fullHM, ref terrain);

            Actor seaBed = terrain.FindActor("SeaBed");
            if (seaBed != null)
            {
                seaBed.LocalPosition -= new Vector3(0, terrainOffset, 0);
            }
        }
    }

}
using FlaxEngine;

namespace TerrainSystem
{
    public class TS_TerrainOffset : Script
    {
        private Terrain terrain;
        private readonly float terrainOffset;

        public TS_TerrainOffset(Terrain _terrain, float _terrainOffset)
        {
            terrain = _terrain;
            terrainOffset = _terrainOffset;
        }

        public void TerrainOffset()
        {
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
            float[] fullHM = TS_Utility.TerrainToFullHeightMap(ref terrain);
            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int x = 0; x < fhmDims.X; x++)
                {
                    fullHM[y * fhmDims.X + x] = fullHM[y * fhmDims.X + x] - terrainOffset;
                }
            }
            TS_Utility.FullHeightMapToTerrain(ref fullHM, ref terrain);

            Actor seaBed = terrain.FindActor("SeaBed");
            if (seaBed != null)
            {
                seaBed.LocalPosition -= new Vector3(0, terrainOffset, 0);
            }
        }
    }

}
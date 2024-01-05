using FlaxEngine;
using FlaxEngine.Utilities;

namespace TerrainSystem
{
    public class TS_HeightNoise(Terrain _terrain, int _octaves = 8, float _noiseHeight = 1500, float _phaseLength = 149, float _persistence = 0.7f) : Script
    {
        private Terrain terrain = _terrain;
        private readonly int numOctaves = _octaves;
        private readonly float noiseHeight = _noiseHeight;
        private readonly float phaseLength = _phaseLength;
        private readonly float persistence = _persistence;  

        public void GenerateHeightNoise()
        {
            float[] fullHM = TS_Util.TerrainToFullHeightMap(ref terrain);
            Int2 fhmDims = TS_Util.GetFHMDims(ref terrain);
            PerlinNoise[] pNoise = new PerlinNoise[numOctaves + 1];

            for (int octave = 2; octave <= numOctaves; octave++)
            {
                pNoise[octave] = new PerlinNoise(0, phaseLength, noiseHeight * Mathf.Pow(persistence, octave), octave);
            }

            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int x = 0; x < fhmDims.X; x++)
                {
                    float noiseValue = fullHM[y * fhmDims.X + x];
                    for (int octave = 2; octave <= numOctaves; octave++)
                    {
                        noiseValue += pNoise[octave].Sample(x, y);
                    }
                    fullHM[y * fhmDims.X + x] = noiseValue;
                }
            }
            TS_Util.FullHeightMapToTerrain(ref fullHM, ref terrain);
        }

    }

}

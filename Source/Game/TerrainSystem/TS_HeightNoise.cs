using FlaxEngine;
using FlaxEngine.Utilities;

namespace TerrainSystem
{
    public class TS_HeightNoise : Script
    {
        private Terrain terrain;
        private readonly int numOctaves;
        private readonly float noiseHeight;
        private readonly float phaseLength;
        private readonly float persistence;

        public TS_HeightNoise(Terrain _terrain, int _octaves, float _noiseHeight, float _phaseLength, float _persistence)
        {
            terrain = _terrain;
            numOctaves = _octaves;
            noiseHeight = _noiseHeight;
            phaseLength = _phaseLength;
            persistence = _persistence;
        }


        public void GenerateHeightNoise()
        {
            float[] fullHM = TS_Utility.TerrainToFullHeightMap(ref terrain);
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
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
            TS_Utility.FullHeightMapToTerrain(ref fullHM, ref terrain);
        }

    }

}

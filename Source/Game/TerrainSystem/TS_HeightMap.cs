using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace TerrainSystem
{
    public class TS_HeightMap : Script
    {
        private Terrain terrain;
        private readonly float maxHeight;
        private readonly float boundaryHeight;
        private readonly float ruggedFactor;
        private readonly float fractionOfMaxHeights;
        private readonly int seed;
        private Int2 patchArrayDims;

        public TS_HeightMap(Terrain _terrain, float _maxHeight = 0, float _boundaryHeight = 0, float _ruggedFactor = 2, float _fractionOfMaxHeights = 0.12f, int _seed = 1)
        {
            terrain = _terrain;
            patchArrayDims = TS_Util.GetPatchArrayDims(ref terrain);
            maxHeight = _maxHeight;
            boundaryHeight = _boundaryHeight;
            ruggedFactor = _ruggedFactor;
            fractionOfMaxHeights = _fractionOfMaxHeights;
            seed = _seed;
        }

        public void GernerateHeightMap()
        {
            float[] heightSampleMap  = Array.ConvertAll(new float[(patchArrayDims.X + 2) * (patchArrayDims.Y + 2)], v => -1.0f);
            SetHeightSampleMap(ref heightSampleMap);

            float[] fullHM = TS_Util.TerrainToFullHeightMap(ref terrain);
            float basePhaseLength = CalculateBasePhaseLength();
            ApplyBaseNoise(ref fullHM, ref heightSampleMap, basePhaseLength);
            if (patchArrayDims.X + 1 == 1 || patchArrayDims.Y + 1 == 1)
            {
                ScaleSinglePatchMap(ref fullHM);
            }
            else
            {
                int blendWidth = 30;
                TS_Util.BlendPatchEdges(ref fullHM, ref terrain, blendWidth);
            }

            TS_Util.FullHeightMapToTerrain(ref fullHM, ref terrain);
        }

        private void ScaleSinglePatchMap(ref float[] fullHM)
        {
            Int2 fhmDims = TS_Util.GetFHMDims(ref terrain);
            PerlinNoise pNoise = new(0, 150, 0.707f, 1);
            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int x = 0; x < fhmDims.X; x++)
                {
                    fullHM[y * fhmDims.X + x] = (pNoise.Sample(x, y) + 0.5f) * maxHeight;
                }
            }
        }

        private void SetHeightSampleMap(ref float[] heightSampleMap)
        {
            int xLength = patchArrayDims.X + 2; 
            int yLength = patchArrayDims.Y + 2;
            
            for (int x = 0; x < xLength; x++)
            {
                heightSampleMap[TS_Util.Get1DIndexFrom2D(x, 0, xLength)] = boundaryHeight;
                heightSampleMap[TS_Util.Get1DIndexFrom2D(x, yLength - 1, xLength)] = boundaryHeight;
            }

            for (int y = 0; y < yLength; y++)
            {
                heightSampleMap[TS_Util.Get1DIndexFrom2D(0, y, xLength)] = boundaryHeight;
                heightSampleMap[TS_Util.Get1DIndexFrom2D(xLength - 1, y, xLength)] = boundaryHeight;
            }

            // if just one patch leave
            if (patchArrayDims.X + 1 == 1 || patchArrayDims.Y + 1 == 1) return;
 
            Queue<(int, int)> queue = new();
            int xPickableLength = (int)(xLength * 0.5f);
            int yPickableLength = (int)(yLength * 0.5f);
            int possibleMaxHeightPoints = xPickableLength * yPickableLength;
            int numberMaxHeightPoints = (int)(possibleMaxHeightPoints * fractionOfMaxHeights);
            Random rand = new(seed);
            int maxHeightSet = 0;
            if (xPickableLength < 1) { xPickableLength = 1; }
            if (yPickableLength < 1) { yPickableLength = 1; }
            if (numberMaxHeightPoints < 1) { numberMaxHeightPoints = 1; }

            for (int i = 0; i < numberMaxHeightPoints; i++)
            {
                queue.Enqueue((rand.Next(xPickableLength, xPickableLength + (xPickableLength / 2)), rand.Next(yPickableLength, yPickableLength + (yPickableLength / 2))));
                maxHeightSet++;
            }
            
            while (queue.Count > 0)
            {
                (int cx, int cy) = queue.Dequeue();
                if (cx < 1 || cx >= xLength - 1 || cy < 1 || cy >= yLength - 1 || heightSampleMap[TS_Util.Get1DIndexFrom2D(cx, cy, xLength)] != -1f) continue;

                if (maxHeightSet > 0)
                {
                    heightSampleMap[TS_Util.Get1DIndexFrom2D(cx, cy, xLength)] = rand.Next((int)maxHeight/2, (int)maxHeight);
                    maxHeightSet--;
                }
                else
                {
                    heightSampleMap[TS_Util.Get1DIndexFrom2D(cx, cy, xLength)] = 
                        SamplePointValue(
                            heightSampleMap[TS_Util.Get1DIndexFrom2D(cx - 1, cy, xLength)],
                            heightSampleMap[TS_Util.Get1DIndexFrom2D(cx + 1, cy, xLength)],
                            heightSampleMap[TS_Util.Get1DIndexFrom2D(cx, cy - 1, xLength)],
                            heightSampleMap[TS_Util.Get1DIndexFrom2D(cx, cy + 1, xLength)]
                            );
                }
                queue.Enqueue((cx - 1, cy));
                queue.Enqueue((cx + 1, cy));
                queue.Enqueue((cx, cy - 1));
                queue.Enqueue((cx, cy + 1));
            }
        }

        private static float SamplePointValue(float a, float b, float c, float d)
        {
            float maxHMScalar = 0.70f;
            float max = MathF.Max(MathF.Max(a, b), MathF.Max(c, d));
            float newHeight = (a + b + c + d) / 4;
            if (newHeight < max * maxHMScalar)
            {
                newHeight = max * maxHMScalar;
            }
            return newHeight;
        }

        private void ApplyBaseNoise(ref float[] fullHM, ref float[] heightSampleMap, float phaseLength)
        {
            PerlinNoise pNoise = new(0, phaseLength, 0.707f, 1);
            float scalar;
            int hsmLength_X = patchArrayDims.X + 2;
            int hsmLength_Y = patchArrayDims.Y + 2;
            Int2 fhmDims = TS_Util.GetFHMDims(ref terrain);

            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int x = 0; x < fhmDims.X; x++)
                {
                    scalar = BilinearInterp(x, y, hsmLength_X, hsmLength_Y, fhmDims, ref heightSampleMap);
                    fullHM[y * fhmDims.X + x] = (pNoise.Sample(x, y) + 0.5f) * scalar;
                }
            }
        }

        private static float BilinearInterp(int fullHM_X, int fullHM_Y, int hsmLength_X, int hsmLength_Y, Int2 fhmDims, ref float[] heightSampleMap)
        {
            float fullFrac_X = (float)fullHM_X / (fhmDims.X + 1);
            float fullFrac_Y = (float)fullHM_Y / (fhmDims.Y + 1);

            float hsmFloat_X = fullFrac_X * (hsmLength_X - 1);
            float hsmFloat_Y = fullFrac_Y * (hsmLength_Y - 1);

            int hsmMin_X = (int)MathF.Floor(hsmFloat_X);
            int hsmMax_X = (int)MathF.Ceiling(hsmFloat_X);
            int hsmMin_Y = (int)MathF.Floor(hsmFloat_Y);
            int hsmMax_Y = (int)MathF.Ceiling(hsmFloat_Y);

            float hsmSubFrac_X = hsmFloat_X - hsmMin_X;
            float hsmSubFrac_Y = hsmFloat_Y - hsmMin_Y;

            float hsmValue_X1Y1 = heightSampleMap[TS_Util.Get1DIndexFrom2D(hsmMin_X, hsmMin_Y, hsmLength_X)];
            float hsmValue_X2Y1 = heightSampleMap[TS_Util.Get1DIndexFrom2D(hsmMax_X, hsmMin_Y, hsmLength_X)];
            float hsmValue_X1Y2 = heightSampleMap[TS_Util.Get1DIndexFrom2D(hsmMin_X, hsmMax_Y, hsmLength_X)];
            float hsmValue_X2Y2 = heightSampleMap[TS_Util.Get1DIndexFrom2D(hsmMax_X, hsmMax_Y, hsmLength_X)];

            float hsmValue_Y1Interp = hsmValue_X1Y1 + (hsmValue_X2Y1 - hsmValue_X1Y1) * hsmSubFrac_X;
            float hsmValue_Y2Interp = hsmValue_X1Y2 + (hsmValue_X2Y2 - hsmValue_X1Y2) * hsmSubFrac_X;

            return hsmValue_Y1Interp + (hsmValue_Y2Interp - hsmValue_Y1Interp) * hsmSubFrac_Y;
        }

        private float CalculateBasePhaseLength()
        {
            Int2 fhmDims = TS_Util.GetFHMDims(ref terrain);
            return Math.Min(fhmDims.X, fhmDims.Y) / ruggedFactor;
        }
    }
}
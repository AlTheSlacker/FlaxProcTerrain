using FlaxEditor;
using FlaxEngine;
using System;

namespace TerrainSystem
{

    /// <summary>
    /// Terrain System Utilities.
    /// </summary>
    public static class TS_Utility
    {
        /// <summary>
        /// Returns an Int2 for the X Y indexes for an equivalent 2D array.
        /// </summary>
        public static Int2 Get2DIndexFrom1D(int index, int x_dimension)
        {
            int x = index % x_dimension;
            int y = index / x_dimension;
            Int2 xy = new (x, y);
            return xy;
        }


        /// <summary>
        /// Returns an Int of the index for an equivalent 1D array.
        /// </summary>
        public static int Get1DIndexFrom2D(int x, int y, int x_dimension)
        {
            return y * x_dimension + x;
        }

        /// <summary>
        /// Blends patch edges over a width of +/- blendOffset using a cubic polynomial
        /// </summary>
        public static void BlendPatchEdges(ref float[] fullHM, ref Terrain terrain, int blendOffset)
        {
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            int hsmLength_X = patchArrayDims.X + 2;
            int hsmLength_Y = patchArrayDims.Y + 2;
            Int2 fhmDims = GetFHMDims(ref terrain);
            float[] blendPoints = new float[blendOffset * 2 + 1];

            for (int y = 1; y < hsmLength_Y - 1; y++)
            {
                float hsmFraction_Y = y / ((float)hsmLength_Y - 1);
                int fullHM_Y = (int)Mathf.Round(hsmFraction_Y * fhmDims.Y);

                for (int x = 0; x < fhmDims.X; x++)
                {
                    for (int i = 0; i < blendOffset * 2 + 1; i++)
                    {
                        blendPoints[i] = fullHM[Get1DIndexFrom2D(x, fullHM_Y - blendOffset + i, fhmDims.X)];
                    }
                    BlendPoints(ref blendPoints);
                    for (int i = 0; i < blendOffset * 2 + 1; i++)
                    {
                        fullHM[Get1DIndexFrom2D(x, fullHM_Y - blendOffset + i, fhmDims.X)] = blendPoints[i];
                    }
                }
            }

            for (int x = 1; x < hsmLength_X - 1; x++)
            {
                float hsmFraction_X = x / ((float)hsmLength_X - 1);
                int fullHM_X = (int)Mathf.Round(hsmFraction_X * fhmDims.X);

                for (int y = 0; y < fhmDims.Y; y++)
                {
                    for (int i = 0; i < blendOffset * 2 + 1; i++)
                    {
                        blendPoints[i] = fullHM[Get1DIndexFrom2D(fullHM_X - blendOffset + i, y, fhmDims.X)];
                    }
                    BlendPoints(ref blendPoints);
                    for (int i = 0; i < blendOffset * 2 + 1; i++)
                    {
                        fullHM[Get1DIndexFrom2D(fullHM_X - blendOffset + i, y, fhmDims.X)] = blendPoints[i];
                    }
                }
            }
        }

        /// <summary>
        /// Modifies a 1D array of points to be a smooth blend between the first and last values.
        /// </summary>
        public static void BlendPoints(ref float[] blendPoints)
        {
            int blendLength = blendPoints.Length;
            int lastBlendIndex = blendLength - 1;
            float x0 = 0; float y0 = blendPoints[0];
            float x1 = lastBlendIndex; float y1 = blendPoints[lastBlendIndex];
            float m0 = blendPoints[1] - blendPoints[0];
            float m1 = blendPoints[lastBlendIndex] - blendPoints[lastBlendIndex - 1];
            float[,] augmentedMatrix = new float[4, 5];

            augmentedMatrix[0, 0] = MathF.Pow(x0, 3);
            augmentedMatrix[0, 1] = MathF.Pow(x0, 2);
            augmentedMatrix[0, 2] = x0;
            augmentedMatrix[0, 3] = 1;
            augmentedMatrix[0, 4] = y0;

            augmentedMatrix[1, 0] = MathF.Pow(x1, 3);
            augmentedMatrix[1, 1] = MathF.Pow(x1, 2);
            augmentedMatrix[1, 2] = x1;
            augmentedMatrix[1, 3] = 1;
            augmentedMatrix[1, 4] = y1;

            augmentedMatrix[2, 0] = 3 * MathF.Pow(x0, 2);
            augmentedMatrix[2, 1] = 2 * x0;
            augmentedMatrix[2, 2] = 1;
            augmentedMatrix[2, 3] = 0;
            augmentedMatrix[2, 4] = m0;

            augmentedMatrix[3, 0] = 3 * MathF.Pow(x1, 2);
            augmentedMatrix[3, 1] = 2 * x1;
            augmentedMatrix[3, 2] = 1;
            augmentedMatrix[3, 3] = 0;
            augmentedMatrix[3, 4] = m1;

            float[,] rref = ReducedRowEchelonForm(augmentedMatrix);

            for (int i = 1; i < blendLength - 1; i++)
            {
                blendPoints[i] = rref[0, 4] * i * i * i + rref[1, 4] * i * i + rref[2, 4] * i + rref[3, 4];
            }
        }


        /// <summary>
        /// Returns a float[,] matrix of the Reduced Row Echelon Form
        /// </summary>
        public static float[,] ReducedRowEchelonForm(float[,] matrix)
        {
            // Code from "https://rosettacode.org/wiki/Reduced_row_echelon_form#C#"
            // Converted from int to float
            // Minor improvement using tuple swap

            int lead = 0, rowCount = matrix.GetLength(0), columnCount = matrix.GetLength(1);
            for (int r = 0; r < rowCount; r++)
            {
                if (columnCount <= lead) break;
                int i = r;
                while (matrix[i, lead] == 0)
                {
                    i++;
                    if (i == rowCount)
                    {
                        i = r;
                        lead++;
                        if (columnCount == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }
                for (int j = 0; j < columnCount; j++)
                {
                    (matrix[i, j], matrix[r, j]) = (matrix[r, j], matrix[i, j]);
                }
                float div = matrix[r, lead];
                if (div != 0)
                    for (int j = 0; j < columnCount; j++) matrix[r, j] /= div;
                for (int j = 0; j < rowCount; j++)
                {
                    if (j != r)
                    {
                        float sub = matrix[j, lead];
                        for (int k = 0; k < columnCount; k++) matrix[j, k] -= (sub * matrix[r, k]);
                    }
                }
                lead++;
            }
            return matrix;
        }


        /// <summary>
        /// Returns a vector3 of the centre of a terrain.
        /// </summary>
        public static Vector3 GetCentreOfTerrain(ref Terrain terrain)
        {
            Vector3 basePosition = terrain.Position;
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * Terrain.PatchEdgeChunksCount + 1;
            Vector3 offset = new((patchArrayDims.X + 1) * heightMapEdgeLength, 0, (patchArrayDims.Y + 1) * heightMapEdgeLength);
            return basePosition + 50f * offset;
        }


        /// <summary>
        /// Returns an Int2 of the terrain patch layout (X by Y patches), zero based!
        /// </summary>
        public static Int2 GetPatchArrayDims(ref Terrain terrain)
        {
            Int2 maxPatchCoord = new(0, 0);
            for (int patchID = 0; patchID < terrain.PatchesCount; patchID++)
            {
                terrain.GetPatchCoord(patchID, out Int2 patchCoord);
                for (int i = 0; i <= 1; i++)
                {
                    maxPatchCoord[i] = patchCoord[i] > maxPatchCoord[i] ? patchCoord[i] : maxPatchCoord[i];
                }
            }
            return maxPatchCoord;
        }


        /// <summary>
        /// Returns a Int2[] giving the vertex sizes of the full height map to cover the terrain
        /// Note: common edge verts are overwritten not duplicated.
        /// </summary>
        public static Int2 GetFHMDims(ref Terrain terrain)
        {
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            Int2 fhmDims = new((patchArrayDims.X + 1) * heightMapEdgeLength - patchArrayDims.X,
                               (patchArrayDims.Y + 1) * heightMapEdgeLength - patchArrayDims.Y);
            return fhmDims;
        }


        /// <summary>
        /// Returns a float[] heightMap for entire terrain object.
        /// </summary>
        public static float[] TerrainToFullHeightMap(ref Terrain terrain)
        {
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            Int2 fhmDims = GetFHMDims(ref terrain);
            float[] fhm = new float[fhmDims.X * fhmDims.Y];

            for (int patchY = 0; patchY <= patchArrayDims.Y; patchY++)
            {
                for (int patchX = 0; patchX <= patchArrayDims.X; patchX++)
                {
                    Int2 patchCoord = new(patchX, patchY);
                    MapPatchToFullHeightMap(ref terrain, ref fhm, patchCoord);
                }
            }
            return fhm;
        }


        /// <summary>
        /// Updates entire terrain to the full height map.
        /// </summary>
        public static void FullHeightMapToTerrain(ref float[] fhm, ref Terrain terrain)
        {
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            for (int patchY = 0; patchY <= patchArrayDims.Y; patchY++)
            {
                for (int patchX = 0; patchX <= patchArrayDims.X; patchX++)
                {
                    Int2 patchCoord = new(patchX, patchY);
                    MapFullHeightMapToPatch(ref terrain, ref fhm, patchCoord);
                }
            }
        }


        /// <summary>
        /// Returns a float[] heightMap, extracted from the terrain patch.
        /// </summary>
        public static float[] GetHeightMapFromPatch(Int2 patchCoord, ref Terrain terrain)
        {
            int heightMapSize = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            float[] newHeightMap = new float[heightMapSize * heightMapSize];
            unsafe
            {
                float* heightMapData = TerrainTools.GetHeightmapData(terrain, ref patchCoord);
                for (int i = 0; i < heightMapSize * heightMapSize; i++)
                {
                    newHeightMap[i] = heightMapData[i];
                }
            }
            return newHeightMap;
        }


        /// <summary>
        /// Updates the full height map array from one terrain patch.
        /// </summary>
        private static void MapPatchToFullHeightMap(ref Terrain terrain, ref float[] fhm, Int2 patchCoord)
        {
            float[] patchHM = GetHeightMapFromPatch(patchCoord, ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            int fhmXEdgeLength = (patchArrayDims.X + 1) * heightMapEdgeLength - patchArrayDims.X;
            int xOffset = (heightMapEdgeLength - 1) * patchCoord.X;
            int yOffset = (heightMapEdgeLength - 1) * patchCoord.Y;
            for (int y = 0; y < heightMapEdgeLength; y++)
            {
                for (int x = 0; x < heightMapEdgeLength; x++)
                {
                    int fhmIndex = fhmXEdgeLength * (yOffset + y) + xOffset + x;
                    fhm[fhmIndex] = patchHM[Get1DIndexFrom2D(x, y, heightMapEdgeLength)];
                }
            }
        }


        /// <summary>
        /// Updates one terrain patch from the full height map data.
        /// </summary>
        private static void MapFullHeightMapToPatch(ref Terrain terrain, ref float[] fhm, Int2 patchCoord)
        {
            int heightMapEdgeLength = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            float[] patchHeightMap = new float[heightMapEdgeLength * heightMapEdgeLength];
            Int2 patchArrayDims = GetPatchArrayDims(ref terrain);
            int fhmXEdgeLength = (patchArrayDims.X + 1) * heightMapEdgeLength - patchArrayDims.X;
            int xOffset = (heightMapEdgeLength - 1) * patchCoord.X;
            int yOffset = (heightMapEdgeLength - 1) * patchCoord.Y;
            for (int y = 0; y < heightMapEdgeLength; y++)
            {
                for (int x = 0; x < heightMapEdgeLength; x++)
                {
                    int fhmIndex = fhmXEdgeLength * (yOffset + y) + xOffset + x;
                    patchHeightMap[Get1DIndexFrom2D(x, y, heightMapEdgeLength)] = fhm[fhmIndex];
                }
            }
            terrain.SetupPatchHeightMap(ref patchCoord, patchHeightMap);
        }

    }

}
using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.Utilities;

namespace TerrainSystem;

/// <summary>
/// TS_BlendOffset Script.
/// </summary>
public class TS_BlendOffset(Terrain _terrain, int _blendWidth) : Script
{
    private Terrain terrain = _terrain;
    private readonly int blendWidth = _blendWidth;

    public void BlendOffset()
    {
        float[] fullHM = TS_Util.TerrainToFullHeightMap(ref terrain);
        TS_Util.BlendPatchEdges(ref fullHM, ref terrain, blendWidth);
        TS_Util.FullHeightMapToTerrain(ref fullHM, ref terrain);
    }
}

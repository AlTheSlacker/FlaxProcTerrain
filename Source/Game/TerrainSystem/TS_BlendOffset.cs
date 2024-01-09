using FlaxEngine;

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
        float[] fullHM = TS_Utility.TerrainToFullHeightMap(ref terrain);
        TS_Utility.BlendPatchEdges(ref fullHM, ref terrain, blendWidth);
        TS_Utility.FullHeightMapToTerrain(ref fullHM, ref terrain);
    }
}

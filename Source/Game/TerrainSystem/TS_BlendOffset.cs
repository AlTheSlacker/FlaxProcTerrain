using FlaxEngine;

namespace TerrainSystem;

/// <summary>
/// TS_BlendOffset Script.
/// </summary>
public class TS_BlendOffset : Script
{
    private Terrain terrain;
    private readonly int blendWidth;

    public TS_BlendOffset(Terrain _terrain, int _blendWidth)
    {
        terrain = _terrain;
        blendWidth = _blendWidth;
    }

    public void BlendOffset()
    {
        float[] fullHM = TS_Utility.TerrainToFullHeightMap(ref terrain);
        TS_Utility.BlendPatchEdges(ref fullHM, ref terrain, blendWidth);
        TS_Utility.FullHeightMapToTerrain(ref fullHM, ref terrain);
    }
}

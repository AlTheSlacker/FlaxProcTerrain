using FlaxEngine;
using Newtonsoft.Json.Linq;
using FlaxEngine.GUI;



#if FLAX_EDITOR
using FlaxEditor.CustomEditors;
using FlaxEditor.CustomEditors.Editors;
using FlaxEditor.CustomEditors.Elements;

namespace TerrainSystem
{
    [CustomEditor(typeof(Terrain))]
    public class TS_Editor : GenericEditor
    {
        // these statics are here to remember settings that have changed AND been applied
        // they aren't saved between seesions, so if you find some you really like, write them down.
        private static float sRuggedFactor = 7;
        private static float sMaxHeight = 36000;
        private static float sBoundaryHeight = -2000;
        private static float sFractionOfMaxHeights = 0.12f;
        private static int sSeed = 7;

        private static int sOctaves = 8;
        private static float sNoiseHeight = 1500;
        private static float sPhaseLength = 149;
        private static float sPersistence = 0.7f;

        private static bool sDistantSeaFloor = true;
        private static bool sWaterVFX = true;
        private static bool sSeaPlane = true;

        private static float sOffset = 500;

        private static int sBlendWidth = 1;


        public override DisplayStyle Style => DisplayStyle.Inline;

        public override void Initialize(LayoutElementsContainer layout)
        {
            base.Initialize(layout);
            Terrain terrain = (Terrain)this.Values[0];
            SetTerrainMaterial(terrain);

            layout.Label("Procedural Terrain", TextAlignment.Near);
            layout.Space(10);

            Int2 patchArrayDims = TS_Util.GetPatchArrayDims(ref terrain);
            int patchCount = (patchArrayDims.X + 1) * (patchArrayDims.Y + 1);

            FloatValueElement maxHeight = layout.FloatValue("Max Height", "Float value (cm) for the highest peak in the terrain.");
            maxHeight.Value = sMaxHeight;

            FloatValueElement boundaryHeight = layout.FloatValue("Boundary Height", "Float value (cm) for the perimeter height (can be negative, e.g. if sea level is 0, set this to -2000.");
            boundaryHeight.Value = sBoundaryHeight;

            FloatValueElement ruggedFactor = layout.FloatValue("Rugged Factor", "Defines the frequency of undulations, a larger number means more ups and downs.");
            ruggedFactor.Value = sRuggedFactor;

            FloatValueElement fractionOfMaxHeights = layout.FloatValue("Fraction of max heights", "What fraction of the non-border patches will be targetting the Max Height.");
            fractionOfMaxHeights.Value = sFractionOfMaxHeights;

            IntegerValueElement seed = layout.IntegerValue("Seed", "Starting seed.");
            seed.Value = sSeed;

            string heightMapTooltip = "This is base map that additional noise will be added to, you can adjust this with normal terrain tools if you wish.";
            ButtonElement btn_HeightMap = layout.Button("Generate Base Height Map", Color.Gray, heightMapTooltip);
            btn_HeightMap.Button.Clicked += () => GenerateHeightMap(terrain, maxHeight.Value, boundaryHeight.Value, ruggedFactor.Value, fractionOfMaxHeights.Value, seed.Value);
            layout.Space(10);

            IntegerValueElement octaves = layout.IntegerValue("Octaves", "Higher octaves, give higher frequency noise.");
            octaves.Value = sOctaves;

            FloatValueElement noiseHeight = layout.FloatValue("Noise Height", "The maximum noise height for the first octave.");
            noiseHeight.Value = sNoiseHeight;

            FloatValueElement phaseLength = layout.FloatValue("Phase Length", "The phase length of the first noise octave.");
            phaseLength.Value = sPhaseLength;

            FloatValueElement persistence = layout.FloatValue("Persistence", "How much amplitude remains in the following octave.");
            persistence.Value = sPersistence;

            string noiseTooltip = "This may be used multiple times, with different settings to get the look you want.";
            ButtonElement btnNoise = layout.Button("Add Noise to Height Map", Color.Gray, noiseTooltip);
            btnNoise.Button.Clicked += () => GenerateHeightNoise(terrain, octaves.Value, noiseHeight.Value, phaseLength.Value, persistence.Value);
            layout.Space(10);

            layout.Label("Sea floor is created at the Boundary Height defined above.", TextAlignment.Near);

            CheckBoxElement distantSeaFloor = layout.Checkbox("Create distant sea floor", "Create a plane level with terrain boundary height.");
            distantSeaFloor.CheckBox.Checked = sDistantSeaFloor;

            CheckBoxElement waterVFX = layout.Checkbox("Create a water VFX volume", "Create a VFX volume for the camera below sea level.");
            waterVFX.CheckBox.Checked = sWaterVFX;

            CheckBoxElement seaPlane = layout.Checkbox("Create a sea plane", "Create a plane representing the sea level at y = 0.");
            seaPlane.CheckBox.Checked = sSeaPlane;

            string seafloorTooltip = "Creates a sea FX volume, a sea floor plane and blends the edge of your terrain to the sea floor plane.";
            ButtonElement btnSeaFloorLevel = layout.Button("Create Sea Floor", Color.Gray, seafloorTooltip);
            btnSeaFloorLevel.Button.Clicked += () => CreateSeaFloor(terrain, boundaryHeight.Value, distantSeaFloor.CheckBox.Checked, waterVFX.CheckBox.Checked, seaPlane.CheckBox.Checked);
            layout.Space(10);

            FloatValueElement terrainOffset = layout.FloatValue("Terrain Offset", "Float value to adjust terrain height by.");
            terrainOffset.Value = sOffset;

            string offsetTooltip = "This will cumulatively lower the terrain and sea floor by the amount you select, effectively flooding more land.";
            ButtonElement btnTerrainOffset = layout.Button("Offset Terrain", Color.Gray, offsetTooltip);
            btnTerrainOffset.Button.Clicked += () => TerrainOffset(terrain, terrainOffset.Value);
            layout.Space(10);

            IntegerValueElement blendWidth = layout.IntegerValue("Blend Width", "Integer value of the amount each patch edge is to be blended over.");
            blendWidth.Value = sBlendWidth;
            string blendTooltip = "Only use this if you have splits in your terrain.";
            ButtonElement btnBlendOffset = layout.Button("Blend Terrain Patch Edges", Color.Gray, blendTooltip);
            btnBlendOffset.Button.Clicked += () => BlendOffset(terrain, blendWidth.Value);
            layout.Space(10);
        }

        private static void GenerateHeightMap(Terrain terrain, float maxHeight, float boundaryHeight, float ruggedFactor, float fractionOfMaxHeights, int seed)
        {
            sMaxHeight = maxHeight;
            sBoundaryHeight = boundaryHeight;
            sRuggedFactor = ruggedFactor;
            sFractionOfMaxHeights = fractionOfMaxHeights;
            sSeed = seed;
            TS_HeightMap generateHeightMap = new(terrain, maxHeight, boundaryHeight, ruggedFactor, fractionOfMaxHeights, seed);
            generateHeightMap.GernerateHeightMap();
        }
        private static void GenerateHeightNoise(Terrain terrain, int octaves, float noiseHeight, float phaseLength, float persistence)
        {
            sOctaves = octaves;
            sNoiseHeight = noiseHeight;
            sPhaseLength = phaseLength;
            sPersistence = persistence;
            TS_HeightNoise generateHeightNoise = new(terrain, octaves, noiseHeight, phaseLength, persistence);
            generateHeightNoise.GenerateHeightNoise();
        }
        private static void CreateSeaFloor(Terrain terrain, float boundaryHeight, bool distantSeaFloor, bool waterVFX, bool seaPlane)
        {
            sBoundaryHeight = boundaryHeight;
            sDistantSeaFloor = distantSeaFloor;
            sWaterVFX = waterVFX;
            sSeaPlane = seaPlane;
            TS_SeaFloor createSeaFloor = new(terrain, boundaryHeight, distantSeaFloor, waterVFX, seaPlane);
            createSeaFloor.CreateSea();
        }
        private static void TerrainOffset(Terrain terrain, float offset)
        {
            sOffset = offset;
            TS_TerrainOffset terrainOffset = new(terrain, offset);
            terrainOffset.TerrainOffset();
        }

        private static void BlendOffset(Terrain terrain, int blendWidth)
        {
            sBlendWidth = blendWidth;
            TS_BlendOffset blendOffset = new(terrain, blendWidth);
            blendOffset.BlendOffset();
        }

        private static void SetTerrainMaterial(Terrain terrain)
        {
            Material material = Content.LoadAsync<Material>("Content/Materials/TerrainSystem/AutoTerrain.Flax");
            terrain.Material = material;
        }

    }
}
#endif
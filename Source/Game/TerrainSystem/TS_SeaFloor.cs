using FlaxEngine;

namespace TerrainSystem
{
    public class TS_SeaFloor : Script
    {
        private Terrain terrain;
        private readonly float boundaryHeight;
        private readonly bool distantSeaFloor;
        private readonly bool waterVFX;
        private readonly bool seaPlane;

        public TS_SeaFloor(Terrain _terrain, float _boundaryHeight, bool _distantSeaFloor, bool _waterVFX, bool _seaPlane)
        {
            terrain = _terrain;
            boundaryHeight = _boundaryHeight;
            distantSeaFloor = _distantSeaFloor;
            waterVFX = _waterVFX;
            seaPlane = _seaPlane;
        }


        public void CreateSea()
        {
            DestroyOldSeaObjects();

            float[] fullHM = TS_Utility.TerrainToFullHeightMap(ref terrain);
            CreateBoundary(ref fullHM);
            TS_Utility.FullHeightMapToTerrain(ref fullHM, ref terrain);

            if (distantSeaFloor) CreateSeaFloorFar();
            if (waterVFX) CreateWaterPostFX();
            if (seaPlane) CreateWater();
        }


        private void DestroyOldSeaObjects()
        {
            Actor child = terrain.FindActor("SeaBed");
            if (child != null && distantSeaFloor) { Destroy(child); }
            child = terrain.FindActor("SeaSurface");
            if (child != null && seaPlane) { Destroy(child); }
            child = terrain.FindActor("SeaVolumeFX");
            if (child != null && waterVFX) { Destroy(child); }
        }


        private void CreateSeaFloorFar()
        {
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * Terrain.PatchEdgeChunksCount + 1;
            float scaleX = (fhmDims.X + 1) * heightMapEdgeLength / 250f;
            float scaleY = (fhmDims.Y + 1) * heightMapEdgeLength / 250f;
            Vector3 position = TS_Utility.GetCentreOfTerrain(ref terrain);

            StaticModel planeActor = new()
            {
                Parent = terrain,
                Name = "SeaBed",
                Position = new Vector3(position.X, boundaryHeight, position.Z),
                Rotation = Matrix.RotationX(-3.14f/2),
                Scale = new Vector3(scaleX, scaleY, 1),
                Model = Content.LoadAsync<Model>("Content/Materials/TerrainSystem/PlaneCube/TS_Plane.Flax")
            };
            Material material = Content.LoadAsync<Material>("Content/Materials/TerrainSystem/Sand1_Wet.Flax");
            planeActor.SetMaterial(0, material);
        }

        private void CreateWater()
        {
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            float scaleX = (fhmDims.X + 1) * heightMapEdgeLength / 250f;
            float scaleY = (fhmDims.Y + 1) * heightMapEdgeLength / 250f;
            Vector3 position = TS_Utility.GetCentreOfTerrain(ref terrain);
            StaticModel cubeActor = new()
            {
                Parent = terrain,
                Name = "SeaSurface",
                Position = new Vector3(position.X, -2000, position.Z),
                Scale = new Vector3(scaleX, 40f, scaleY),
                Model = Content.LoadAsync<Model>("Content/Materials/TerrainSystem/PlaneCube/TS_Cube.Flax")
            };
            Material material = Content.LoadAsync<Material>("Content/Materials/TerrainSystem/Water.Flax");
            cubeActor.SetMaterial(0, material);
        }

        private void CreateWaterPostFX()
        {
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
            int heightMapEdgeLength = terrain.ChunkSize * FlaxEngine.Terrain.PatchEdgeChunksCount + 1;
            float scaleX = (fhmDims.X + 1) * heightMapEdgeLength / 2.5f;
            float scaleY = (fhmDims.Y + 1) * heightMapEdgeLength / 2.5f;
            Vector3 position = TS_Utility.GetCentreOfTerrain(ref terrain);
            PostFxVolume seaVolume = new()
            {
                Parent = terrain,
                Name = "SeaVolumeFX",
                Position = new Vector3(position.X, -2000, position.Z),
                Size = new Vector3(scaleX, 4000, scaleY)
            };
            AmbientOcclusionSettings ambientOcclusionSettings = new()
            {
                Enabled = true,
                Intensity = 0.7f,
                Power = 0.75f,
                Radius = 0.7f,
                FadeOutDistance = 5000,
                FadeDistance = 500,
                OverrideFlags = AmbientOcclusionSettingsOverride.Enabled | AmbientOcclusionSettingsOverride.Intensity |
                AmbientOcclusionSettingsOverride.Power | AmbientOcclusionSettingsOverride.Radius |
                AmbientOcclusionSettingsOverride.FadeOutDistance | AmbientOcclusionSettingsOverride.FadeDistance
            };
            seaVolume.AmbientOcclusion = ambientOcclusionSettings;

            ColorGradingSettings colorGradingSettings = new()
            {
                ColorContrast = new Float4(0, 0.46f, 0.54f, 1.4f),
                ColorGain = new Float4(0, 0.59f, 0.73f, 1),
                OverrideFlags = ColorGradingSettingsOverride.ColorContrast | ColorGradingSettingsOverride.ColorGain
            };
            seaVolume.ColorGrading = colorGradingSettings;

            CameraArtifactsSettings cameraArtifactsSettings = new()
            {
                ChromaticDistortion = 1,
                OverrideFlags = CameraArtifactsSettingsOverride.ChromaticDistortion
            };
            seaVolume.CameraArtifacts = cameraArtifactsSettings;
        }

        private void CreateBoundary(ref float[] fullHM)
        {
            Int2 fhmDims = TS_Utility.GetFHMDims(ref terrain);
            int blendOffset = 30;
            float[] blendPoints = new float[blendOffset];

            for (int x = 0; x < fhmDims.X; x++)
            {
                for (int i = 2; i < blendOffset; i++)
                {
                    blendPoints[i] = fullHM[TS_Utility.Get1DIndexFrom2D(x, i, fhmDims.X)];
                }
                blendPoints[0] = boundaryHeight;
                blendPoints[1] = boundaryHeight;
                TS_Utility.BlendPoints(ref blendPoints);
                for (int i = 0; i < blendOffset; i++)
                {
                    fullHM[TS_Utility.Get1DIndexFrom2D(x, i, fhmDims.X)] = blendPoints[i];
                }

                for (int i = 2; i < blendOffset; i++)
                {
                    blendPoints[i] = fullHM[TS_Utility.Get1DIndexFrom2D(x, fhmDims.Y - i - 1, fhmDims.X)];
                }
                blendPoints[0] = boundaryHeight;
                blendPoints[1] = boundaryHeight;
                TS_Utility.BlendPoints(ref blendPoints);
                for (int i = 0; i < blendOffset; i++)
                {
                    fullHM[TS_Utility.Get1DIndexFrom2D(x, fhmDims.Y - i - 1, fhmDims.X)] = blendPoints[i];
                }
            }

            for (int y = 0; y < fhmDims.Y; y++)
            {
                for (int i = 2; i < blendOffset; i++)
                {
                    blendPoints[i] = fullHM[TS_Utility.Get1DIndexFrom2D(i, y, fhmDims.X)];
                }
                blendPoints[0] = boundaryHeight;
                blendPoints[1] = boundaryHeight;
                TS_Utility.BlendPoints(ref blendPoints);
                for (int i = 0; i < blendOffset; i++)
                {
                    fullHM[TS_Utility.Get1DIndexFrom2D(i, y, fhmDims.X)] = blendPoints[i];
                }

                for (int i = 2; i < blendOffset; i++)
                {
                    blendPoints[i] = fullHM[TS_Utility.Get1DIndexFrom2D(fhmDims.X - i - 1, y, fhmDims.X)];
                }
                blendPoints[0] = boundaryHeight;
                blendPoints[1] = boundaryHeight;
                TS_Utility.BlendPoints(ref blendPoints);
                for (int i = 0; i < blendOffset; i++)
                {
                    fullHM[TS_Utility.Get1DIndexFrom2D(fhmDims.X - i - 1, y, fhmDims.X)] = blendPoints[i];
                }
            }
        }

    }

}
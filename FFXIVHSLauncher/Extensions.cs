using System.IO;
using FFXIVHSLib;
using SaintCoinach.Graphics;
using SharpDX;
using Quaternion = FFXIVHSLib.Quaternion;
using Vector3 = FFXIVHSLib.Vector3;

namespace FFXIVHSLauncher
{
    static class Extensions
    {
        public static MapModelEntry ToMapModelEntry(this TransformedModel t, int modelId)
        {
            //Id
            MapModelEntry m = new MapModelEntry();
            m.modelId = modelId;
            
            //Translation, rotation, scale
            Transform entryTransform = new Transform();

            entryTransform.translation = new Vector3(t.Translation.X, t.Translation.Y, t.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(t.Rotation.X) *
                                    Matrix.RotationY(t.Rotation.Y) *
                                    Matrix.RotationZ(t.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;

            entryTransform.scale = new Vector3(t.Scale.X, t.Scale.Y, t.Scale.Z);
            
            m.transform = entryTransform;
            return m;
        }

        public static MapModel ToMapModel(this ModelDefinition m)
        {
            MapModel mModel = new MapModel();

            mModel.modelPath = m.File.Path;
            mModel.modelName = Path.GetFileNameWithoutExtension(mModel.modelPath.Substring(mModel.modelPath.LastIndexOf('/') + 1));
            mModel.numMeshes = m.GetModel(ModelQuality.High).Meshes.Length;

            return mModel;
        }

        public static MapVfxEntry ToMapVfxEntry(this SaintCoinach.Graphics.Sgb.SgbVfxEntry v)
        {
            MapVfxEntry mVfx = new MapVfxEntry();
            var h = v.Header;

            mVfx.layerId = (int)h.UnknownId;
            mVfx.softParticleFadeRange = h.SoftParticleFadeRange;

            mVfx.color.red = h.Color.Red;
            mVfx.color.green = h.Color.Green;
            mVfx.color.blue = h.Color.Blue;
            mVfx.color.alpha = h.Color.Alpha;

            mVfx.autoPlay = h.AutoPlay;
            mVfx.noFarClip = h.NoFarClip;

            mVfx.nearFadeStart = h.NearFadeStart;
            mVfx.nearFadeEnd = h.NearFadeEnd;
            mVfx.farFadeStart = h.FarFadeStart;
            mVfx.farFadeEnd = h.FarFadeEnd;
            mVfx.zCorrect = h.ZCorrect;

            mVfx.modelPath = v.FilePath;//.Replace(".avfx", ".obj");

            mVfx.avfxPath = v.FilePath;


            // transform
            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(h.Translation.X, h.Translation.Y, h.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(h.Rotation.X) *
                                    Matrix.RotationY(h.Rotation.Y) *
                                    Matrix.RotationZ(h.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;
            entryTransform.scale = new Vector3(h.Scale.X, h.Scale.Y, h.Scale.Z);

            mVfx.transform = entryTransform;
            return mVfx;
        }

        public static MapVfxEntry ToMapVfxEntry(this SaintCoinach.Graphics.Lgb.LgbVfxEntry v)
        {
            MapVfxEntry mVfx = new MapVfxEntry();
            var h = v.Header;

            mVfx.layerId = (int)h.UnknownId;
            mVfx.softParticleFadeRange = h.SoftParticleFadeRange;

            mVfx.color.red = h.Color.Red;
            mVfx.color.green = h.Color.Green;
            mVfx.color.blue = h.Color.Blue;
            mVfx.color.alpha = h.Color.Alpha;

            mVfx.autoPlay = h.AutoPlay;
            mVfx.noFarClip = h.NoFarClip;

            mVfx.nearFadeStart = h.NearFadeStart;
            mVfx.nearFadeEnd = h.NearFadeEnd;
            mVfx.farFadeStart = h.FarFadeStart;
            mVfx.farFadeEnd = h.FarFadeEnd;
            mVfx.zCorrect = h.ZCorrect;

            mVfx.modelPath = v.FilePath;//.Replace(".avfx", ".obj");

            mVfx.avfxPath = v.FilePath;


            // transform
            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(h.Translation.X, h.Translation.Y, h.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(h.Rotation.X) *
                                    Matrix.RotationY(h.Rotation.Y) *
                                    Matrix.RotationZ(h.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;
            entryTransform.scale = new Vector3(h.Scale.X, h.Scale.Y, h.Scale.Z);

            mVfx.transform = entryTransform;
            return mVfx;
        }

        public static MapLightEntry ToMapLightEntry(this SaintCoinach.Graphics.Sgb.SgbLightEntry l)
        {
            MapLightEntry mLight = new MapLightEntry();
            var h = l.Header;

            mLight.layerId = (int)h.UnknownId;

            mLight.lightType = h.LightType.ToString().Replace("LightType", "");
            mLight.attenuation = h.Attenuation;
            mLight.rangeRate = h.RangeRate;
            mLight.pointLightType = h.PointLightType.ToString().Replace("PointLightType", "");
            mLight.attenuationConeCoefficient = h.AttenuationConeCoefficient;
            mLight.coneDegree = h.ConeDegree;

            mLight.texturePath = l.TexturePath;

            mLight.color.red = h.DiffuseColorHDRI.Red;
            mLight.color.green = h.DiffuseColorHDRI.Green;
            mLight.color.blue = h.DiffuseColorHDRI.Blue;
            mLight.color.alpha = h.DiffuseColorHDRI.Alpha;
            mLight.colorIntensity = h.DiffuseColorHDRI.Intensity;

            mLight.followsDirectionalLight = h.FollowsDirectionalLight;

            mLight.specularEnabled = h.SpecularEnabled;
            mLight.bgShadowEnabled = h.BGShadowEnabled;
            mLight.characterShadowEnabled = h.CharacterShadowEnabled;

            mLight.shadowClipRange = h.ShadowClipRange;
            mLight.planeLightRotationX = h.PlaneLightRotationX;
            mLight.planeLightRotationY = h.PlaneLightRotationY;
            mLight.mergeGroupID = h.MergeGroupID;

            // transform
            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(h.Translation.X, h.Translation.Y, h.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(h.Rotation.X) *
                                    Matrix.RotationY(h.Rotation.Y) *
                                    Matrix.RotationZ(h.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;
            entryTransform.scale = new Vector3(h.Scale.X, h.Scale.Y, h.Scale.Z);

            mLight.transform = entryTransform;
            return mLight;
        }

        public static MapLightEntry ToMapLightEntry(this SaintCoinach.Graphics.Lgb.LgbLightEntry l)
        {
            MapLightEntry mLight = new MapLightEntry();
            var h = l.Header;

            mLight.layerId = (int)h.UnknownId;

            mLight.lightType = h.LightType.ToString().Replace("LightType", "");
            mLight.attenuation = h.Attenuation;
            mLight.rangeRate = h.RangeRate;
            mLight.pointLightType = h.PointLightType.ToString().Replace("PointLightType", "");
            mLight.attenuationConeCoefficient = h.AttenuationConeCoefficient;
            mLight.coneDegree = h.ConeDegree;

            mLight.texturePath = l.TexturePath;

            mLight.color.red = h.DiffuseColorHDRI.Red;
            mLight.color.green = h.DiffuseColorHDRI.Green;
            mLight.color.blue = h.DiffuseColorHDRI.Blue;
            mLight.color.alpha = h.DiffuseColorHDRI.Alpha;
            mLight.colorIntensity = h.DiffuseColorHDRI.Intensity;

            mLight.followsDirectionalLight = h.FollowsDirectionalLight;

            mLight.specularEnabled = h.SpecularEnabled;
            mLight.bgShadowEnabled = h.BGShadowEnabled;
            mLight.characterShadowEnabled = h.CharacterShadowEnabled;

            mLight.shadowClipRange = h.ShadowClipRange;
            mLight.planeLightRotationX = h.PlaneLightRotationX;
            mLight.planeLightRotationY = h.PlaneLightRotationY;
            mLight.mergeGroupID = h.MergeGroupID;

            // transform
            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(h.Translation.X, h.Translation.Y, h.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(h.Rotation.X) *
                                    Matrix.RotationY(h.Rotation.Y) *
                                    Matrix.RotationZ(h.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;
            entryTransform.scale = new Vector3(h.Scale.X, h.Scale.Y, h.Scale.Z);

            mLight.transform = entryTransform;
            return mLight;
        }

        public static MapModel ToMapModel(this SaintCoinach.Graphics.Avfx.AvfxFile a)
        {
            MapModel m = new MapModel();

            m.modelName = Path.GetFileNameWithoutExtension(a.File.Path.Substring(a.File.Path.LastIndexOf('/') + 1));
            int validMeshes = 0;

            foreach (var mesh in a.Models)
                if (mesh.Indices.Length > 0)
                    validMeshes++;
            m.numMeshes = validMeshes;
            m.modelPath = a.File.Path;

            return m;
        }

        public static MapModel ToMapModel(this SaintCoinach.Graphics.Avfx.AvfxModelEntry mdl)
        {
            MapModel m = new MapModel();

            m.modelName = Path.GetFileNameWithoutExtension(mdl.Name);
            //int validMeshes = 0;
            m.numMeshes = 1;
            m.modelPath = "./" + mdl.Name + ".avfx";
            m.avfxFilePath = mdl.ModelFilePath;
            return m;
        }

        public static Quaternion ExtractRotationQuaternion(this Matrix m)
        {
            SharpDX.Quaternion dxRot = SharpDX.Quaternion.RotationMatrix(m);
            return new Quaternion(dxRot.X, dxRot.Y, dxRot.Z, dxRot.W);
        }

        public static Vector3 ToLibVector3(this SaintCoinach.Graphics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Quaternion ToQuaternion(this Vector3 v)
        {
            Matrix m = Matrix.Identity *
                       Matrix.RotationX(v.x) *
                       Matrix.RotationY(v.y) *
                       Matrix.RotationZ(v.z);

            SharpDX.Quaternion dxQuat = SharpDX.Quaternion.RotationMatrix(m);

            return new Quaternion(dxQuat.X, dxQuat.Y, dxQuat.Z, dxQuat.W);
        }
    }
}

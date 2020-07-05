﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FFXIVHSLib
{
    //Utility for serialization shared between WPF/SaintCoinach and Unity.
    //TODO: Extract classes to their own files

    public enum Territory
    {
        //Wards
        S1H1, F1H1, W1H1, E1H1,

        //Houses
        S1I1, S1I2, S1I3, S1I4,
        F1I1, F1I2, F1I3, F1I4,
        W1I1, W1I2, W1I3, W1I4,
        E1I1, E1I2, E1I3, E1I4
    }

    public enum Size
    {
        s, m, l, x = 254
    }

    public enum FixtureType
    {
        rof = 1,
        wal,
        wid,
        dor,
        orf,
        owl,
        osg,
        fnc
    }

    public enum DoorVariants
    {
        ca, cb, ci, co
    }

    public enum WindowVariants
    {
        ci, co
    }

    public enum FenceVariants
    {
        a, b, c, d
    }

    public class DefaultFences
    {
        public static readonly int[] fnc =
            {10222, 10223, 10224, 1022402};
    }

    public class Vector3
    {
        public float x, y, z;

        public Vector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator +(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator +(UnityEngine.Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public override bool Equals(object obj)
        {
            Vector3 v3 = obj as Vector3;

            if (v3 == null)
                return false;

            return (v3.x == x && v3.y == y && v3.z == z);
        }
    }

    public class Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion()
        {

        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator UnityEngine.Quaternion(Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        /// <summary>
        /// Cannot be called from outside of Unity code.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            UnityEngine.Vector3 euler = ((UnityEngine.Quaternion) this).eulerAngles;
            Vector3 vector = new Vector3(euler.x, euler.y, euler.z);
            return vector;
        }
    }

    public class Transform
    {
        public Vector3 translation { get; set; }
        public Quaternion rotation { get; set; }
        public Vector3 scale { get; set; }

        public static Transform Empty =
            new Transform(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), new Vector3(1, 1, 1));

        public Transform()
        {
            translation = new Vector3();
            rotation = new Quaternion();
            scale = new Vector3();
        }

        public Transform(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override bool Equals(object obj)
        {
            Transform tr = obj as Transform;

            if (tr == null)
                return false;

            return (tr.translation == translation &&
                    tr.rotation == rotation &&
                    tr.scale == scale);
        }
    }
    
    /// <summary>
    /// Settings relevant to parsing data to serialize into WardInfo.json.
    /// See FFXIVHousingSim.DataWriter for more info.
    /// </summary>
    public class WardSetting
    {
        public Territory Territory { get; set; }
        public string group { get; set; }
        public string subdivisionSuffix { get; set; }
        public string plotName { get; set; }

        public WardSetting() { }
    }

    /// <summary>
    /// Class representing a serialized bg.lgb.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Handles the groups of the map and their positions.
        /// </summary>
        public Dictionary<int, MapGroup> groups { get; set; }

        /// <summary>
        /// Maps a first-come, first-serve ID to each unique model.
        /// </summary>
        public Dictionary<int, MapModel> models { get; set; }

        public Dictionary<int, MapLightEntry> lights { get; set; }

        public Dictionary<int, MapVfxEntry> vfx { get; set; }

        public Dictionary<int, MapSoundEntry> sounds { get; set; }

        public Dictionary<int, MapAnimScriptEntry> animScripts { get; set; }

        public Dictionary<int, MapAnimTransformScriptEntry> animTransformScripts { get; set; }

        public Dictionary<int, MapAnimDoorScriptEntry> animDoorScripts { get; set; }
        public Dictionary<int, MapMovePathScriptEntry> movePathScripts { get; set; }

        public Map()
        {
            this.groups = new Dictionary<int, MapGroup>();
            this.models = new Dictionary<int, MapModel>();
            this.vfx = new Dictionary<int, MapVfxEntry>();
            this.sounds = new Dictionary<int, MapSoundEntry>();
            this.animScripts = new Dictionary<int, MapAnimScriptEntry>();
            this.animTransformScripts = new Dictionary<int, MapAnimTransformScriptEntry>();
            this.animDoorScripts = new Dictionary<int, MapAnimDoorScriptEntry>();
            this.movePathScripts = new Dictionary<int, MapMovePathScriptEntry>();
        }

        public void AddMapGroup(MapGroup group)
        {
            if (groups == null)
                groups = new Dictionary<int, MapGroup>();
            
            int id = groups.Keys.Count;
            
            group.id = id;
            groups.Add(id, group);
        }

        /// <summary>
        /// Attempts to add a new model to the models dictionary.<br />
        /// Always returns the ID of the model that was attempted to add.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int TryAddUniqueModel(MapModel model)
        {
            if (models == null)
                models = new Dictionary<int, MapModel>();

            //Attempt to get model
            var res = models.Where(_ => _.Value.Equals(model)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;
            
            int id = models.Count;
            model.id = id;
            models.Add(id, model);
            return id;
        }

        public int TryAddUniqueLight(MapLightEntry light)
        {
            if (lights == null)
                lights = new Dictionary<int, MapLightEntry>();

            //Attempt to get light
            var res = lights.Where(_ => _.Value.Equals(light)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = lights.Count;
            light.id = id;
            lights.Add(id, light);

            return id;
        }

        public int TryAddUniqueVfx(MapVfxEntry vfxe)
        {
            if (this.vfx == null)
                this.vfx = new Dictionary<int, MapVfxEntry>();

            //Attempt to get vfx
            var res = vfx.Where(_ => _.Value.Equals(vfxe)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.vfx.Count;
            vfxe.id = id;
            this.vfx.Add(id, vfxe);

            return id;
        }

        public int TryAddUniqueSound(MapSoundEntry se)
        {
            if (this.sounds == null)
                this.sounds  = new Dictionary<int, MapSoundEntry>();

            //Attempt to get vfx
            var res = sounds.Where(_ => _.Value.Equals(se)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.sounds.Count;
            se.id = id;
            this.sounds.Add(id, se);

            return id;
        }

        public int TryAddUniqueAnimScript(MapAnimScriptEntry se)
        {
            if (this.animScripts == null)
                this.animScripts = new Dictionary<int, MapAnimScriptEntry>();

            var res = animScripts.Where(_ => _.Value.Equals(se)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.animScripts.Count;
            se.id = id;
            this.animScripts.Add(id, se);
            return id;
        }

        public int TryAddUniqueAnimTransformScript(MapAnimTransformScriptEntry se)
        {
            if (this.animTransformScripts == null)
                this.animTransformScripts = new Dictionary<int, MapAnimTransformScriptEntry>();

            var res = animTransformScripts.Where(_ => _.Value.Equals(se)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.animTransformScripts.Count;
            se.id = id;
            this.animTransformScripts.Add(id, se);
            return id;
        }

        public int TryAddUniqueAnimDoorScript(MapAnimDoorScriptEntry se)
        {
            if (this.animDoorScripts == null)
                this.animDoorScripts = new Dictionary<int, MapAnimDoorScriptEntry>();

            var res = animDoorScripts.Where(_ => _.Value.Equals(se)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.animDoorScripts.Count;
            se.id = id;
            this.animDoorScripts.Add(id, se);
            return id;
        }

        public int TryAddUniqueMovePathScript(MapMovePathScriptEntry se)
        {
            if (this.movePathScripts == null)
                this.movePathScripts = new Dictionary<int, MapMovePathScriptEntry>();

            var res = movePathScripts.Where(_ => _.Value.Equals(se)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;

            int id = this.movePathScripts.Count;
            se.id = id;
            this.movePathScripts.Add(id, se);
            return id;
        }
    }

    public class MapGroup
    {
        public enum GroupType
        {
            LGB, SGB, TERRAIN
        }

        public int id;
        public GroupType type;
        public string groupName;
        public Transform groupTransform;

        public List<MapGroup> groups;
        public List<MapModelEntry> entries;
        public List<MapLightEntry> lights;
        public List<MapVfxEntry> vfx;
        public List<MapSoundEntry> sounds;
        public List<int> animScriptRefs;
        public List<int> animTransformScriptRefs;
        public List<int> animDoorScriptRefs;
        public List<int> movePathScriptRefs;

        public MapGroup()
        {

        }

        public MapGroup(GroupType t, string name)
        {
            type = t;
            groupName = name;
        }

        public void AddGroup(MapGroup mg)
        {
            if (groups == null)
                groups = new List<MapGroup>();

            groups.Add(mg);
        }

        public void AddEntry(MapModelEntry mme)
        {
            if (entries == null)
                entries = new List<MapModelEntry>();

            entries.Add(mme);
        }

        public void AddEntry(MapLightEntry mle)
        {
            if (lights == null)
                lights = new List<MapLightEntry>();

            lights.Add(mle);
        }

        public void AddEntry(MapVfxEntry mve)
        {
            if (vfx == null)
                vfx = new List<MapVfxEntry>();

            vfx.Add(mve);
        }

        public void AddEntry(MapSoundEntry mse)
        {
            if (sounds == null)
                sounds = new List<MapSoundEntry>();
            sounds.Add(mse);
        }

        public void AddEntry(MapAnimScriptEntry mse)
        {
            if (animScriptRefs == null)
                animScriptRefs = new List<int>();
            animScriptRefs.Add(mse.id);
        }

        public void AddEntry(MapAnimTransformScriptEntry mse)
        {
            if (animTransformScriptRefs == null)
                animTransformScriptRefs = new List<int>();
            animTransformScriptRefs.Add(mse.id);
        }

        public void AddEntry(MapAnimDoorScriptEntry mse)
        {
            if (animDoorScriptRefs == null)
                animDoorScriptRefs = new List<int>();
            animDoorScriptRefs.Add(mse.id);
        }

        public void AddEntry(MapMovePathScriptEntry mse)
        {
            if (movePathScriptRefs == null)
                movePathScriptRefs = new List<int>();
            movePathScriptRefs.Add(mse.id);
        }
    }

    /// <summary>
    /// Class representing a TransformedModel located within a bg.lgb.
    /// </summary>
    public class MapModelEntry
    {
        //Determine if id necessary
        public int id { get; set; }
        public int modelId { get; set; }
        public List<int> animScriptIds { get; set; }
        public List<int> animTransformScriptIds { get; set; }
        public List<int> animDoorScriptIds { get; set; }
        public Transform transform { get; set; }

        public MapModelEntry()
        {
            this.animScriptIds = new List<int>();
            this.animTransformScriptIds = new List<int>();
            this.animDoorScriptIds = new List<int>();
        }
    }

    public class MapColor
    {
        public byte red;
        public byte green;
        public byte blue;
        public byte alpha;

        public override bool Equals(object obj)
        {
            if (obj is MapColor)
            {
                MapColor l = (MapColor)obj;
                return l.red == red && l.green == green && l.blue == blue && l.alpha == alpha;
            }
            return false;
        }
    }

    public class MapVfxEntry
    {
        public int id { get; set; }
        public int layerId { get; set; }
        public List<int> modelIds { get; set; }
        public List<int> animScriptIds { get; set; }
        public List<int> animTransformScriptIds { get; set; }
        public List<int> animDoorScriptIds { get; set; }
        public Transform transform { get; set; }
        public string avfxPath { get; set; }
        public string modelPath { get; set; }

        public float softParticleFadeRange { get; set; }
        public MapColor color { get; set; }
        public byte autoPlay { get; set; }
        public byte noFarClip{ get; set; }

        public float nearFadeStart { get; set; }
        public float nearFadeEnd { get; set; }
        public float farFadeStart { get; set; }
        public float farFadeEnd { get; set; }
        public float zCorrect { get; set; }

        public MapVfxEntry()
        {
            transform = new Transform();
            color = new MapColor();
            modelIds = new List<int>();
            animScriptIds = new List<int>();
            animTransformScriptIds = new List<int>();
            animDoorScriptIds = new List<int>();
        }
    }

    public class MapLightEntry
    {
        public int id { get; set; }
        public int layerId { get; set; }
        public Transform transform { get; set; }

        public List<int> animScriptIds { get; set; }
        public List<int> animTransformScriptIds { get; set; }
        public List<int> animDoorScriptIds { get; set; }

        public string lightType{ get; set; }
        public float attenuation{ get; set; }
        public float rangeRate{ get; set; }
        public string pointLightType{ get; set; }
        public float attenuationConeCoefficient{ get; set; }
        public float coneDegree{ get; set; }

        public string texturePath{ get; set; }

        public MapColor color{ get; set; }
        public float colorIntensity{ get; set; }

        public byte followsDirectionalLight{ get; set; }
        public byte specularEnabled{ get; set; }
        public byte bgShadowEnabled{ get; set; }
        public byte characterShadowEnabled{ get; set; }

        public float shadowClipRange{ get; set; }
        public float planeLightRotationX{ get; set; }
        public float planeLightRotationY{ get; set; }
        public ushort mergeGroupID{ get; set; }

        public MapLightEntry()
        {
            transform = new Transform();
            color = new MapColor();
            animScriptIds = new List<int>();
            animTransformScriptIds = new List<int>();
            animDoorScriptIds = new List<int>();
        }

        public override bool Equals(object l)
        {
            if (l is MapLightEntry)
            {
                MapLightEntry m = (MapLightEntry)l;
                return transform == m.transform && lightType == m.lightType && color == m.color && colorIntensity == m.colorIntensity;
            }
            return false;
        }
    }

    public class MapSoundEntry
    {
        public int id { get; set; }
        public string filePath { get; set; }
        public string fileName { get; set; }

        public List<int> animScriptIds { get; set; }
        public List<int> animTransformScriptIds { get; set; }
        public List<int> animDoorScriptIds { get; set; }

        public Transform transform { get; set; }

        public MapSoundEntry()
        {
            this.animScriptIds = new List<int>();
            this.animTransformScriptIds = new List<int>();
            this.animDoorScriptIds = new List<int>();
        }

        public override bool Equals(object l)
        {
            if (l is MapSoundEntry)
            {
                MapSoundEntry m = (MapSoundEntry)l;
                return m.filePath == filePath && m.transform == transform;
            }
            return false;
        }
    }

    public enum MapAnimRotationAxis
    {
        X,
        Y,
        Z
    }
    public enum MapMovePathModeLayer
    {
        None_4 = 0x0,
        SGAction = 0x1,
        Timeline_1 = 0x2,
    };
    public enum MapRotationTypeLayer
    {
        NoRotate = 0x0,
        AllAxis = 0x1,
        YAxisOnly = 0x2,
    };

    public enum MapAnimTransformCurveType
    {
        CurveLinear = 0x0,
        CurveSpline = 0x1,
        CurveAcceleration = 0x2,
        CurveDeceleration = 0x3,
    };

    public enum MapAnimDoorCurveType
    {
        Spline = 0x1,
        Linear = 0x2,
        Acceleration = 0x3,
        Deceleration = 0x4,
    };

    public enum MapAnimDoorOpenStyle
    {
        Rotation_0 = 0x0,
        HorizontalSlide = 0x1,
        VerticalSlide = 0x2,
    };

    public enum MapAnimTransformMovementType
    {
        MovementOneWay = 0x0,
        MovementRoundTrip = 0x1,
        MovementRepetition = 0x2,
    };

    public class MapMovePathScriptEntry
    {
        // name = "MOVE_" + id + ".cs";
        public int id { get; set; }
        public string name { get; set; }
        public string scriptFileName { get; set; }
        public MapMovePathModeLayer mode { get; set; }
        public byte autoPlay { get; set; }
        public ushort time { get; set; }
        public byte loop { get; set; }
        public byte reverse { get; set; }
        public MapRotationTypeLayer rotation { get; set; }
        public ushort accelerateTime { get; set; }
        public ushort decelerateTime { get; set; }

        public float horizontalSwingRange0 { get; set; }
        public float horizontalSwingRange1 { get; set; }

        public float swingMoveSpeedRange0 { get; set; }
        public float swingMoveSpeedRange1 { get; set; }

        public float swingRotation0 { get; set; }
        public float swingRotation1 { get; set; }

        public float swingRotationSpeedRange0 { get; set; }
        public float swingRotationSpeedRange1 { get; set; }

        public override bool Equals(object l)
        {
            if (l is MapMovePathScriptEntry)
            {
                MapMovePathScriptEntry m = (MapMovePathScriptEntry)l;
                return m.mode == mode && m.autoPlay == autoPlay && m.time == time && m.loop == loop && m.reverse == reverse &&
                    m.rotation == rotation && m.accelerateTime == accelerateTime && m.decelerateTime == decelerateTime &&
                    m.horizontalSwingRange0 == horizontalSwingRange0 && m.horizontalSwingRange1 == horizontalSwingRange1 &&
                    m.swingMoveSpeedRange0 == swingMoveSpeedRange0 && m.swingMoveSpeedRange1 == swingMoveSpeedRange1 &&
                    m.swingRotation0 == swingRotation0 && m.swingRotation1 == swingRotation1 &&
                    m.swingRotationSpeedRange0 == swingRotationSpeedRange0 && m.swingRotationSpeedRange1 == swingRotationSpeedRange1;
            }
            return false;
        }
    };

    public class MapAnimScriptEntry
    {
        public int id { get; set; }
        public int animIndex { get; set; }
        public string parentSgbPath { get; set; }
        public string name { get; set; }
        public string scriptFileName { get; set; }
        public uint targetSgbEntryIndex { get; set; }
        public MapAnimRotationAxis axis { get; set; }
        public float fullRotationTime { get; set; }
        public float delay { get; set; }
        public uint targetSgbVfxId { get; set; }
        public uint targetSgbVfx2Id { get; set; }
        public uint targetSgbSoundStartId { get; set; }
        public uint targetSgbSoundMidId { get; set; }
        public uint targetSgbSoundEndId { get; set; }

        public override bool Equals(object l)
        {
            // only need to check it has the same entry id in the sgb?
            if (l is MapAnimScriptEntry)
            {
                MapAnimScriptEntry m = (MapAnimScriptEntry)l;
                return m.animIndex == animIndex && m.parentSgbPath == parentSgbPath;
            }
            return false;
        }
    }

    /// <summary>
    /// Class representing SGActionTransform2
    /// </summary>
    public class MapAnimTransformScriptEntry
    {
        public int id { get; set; }
        public int animIndex { get; set; }
        public List<uint> targetSgMemberIndexes { get; set; }
        public string parentSgbPath { get; set; }
        public string name { get; set; }
        public string scriptFileName { get; set; }
        public byte loop { get; set; }
        public Vector3 translation { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }

        public byte enabled { get; set; }
        public Vector3 offset { get; set; }
        public float randomRate { get; set; }
        public uint time { get; set; }
        public uint startEndTime { get; set; }
        public MapAnimTransformCurveType curveType { get; set; }
        public MapAnimTransformMovementType movementType { get; set; }
        public MapAnimTransformScriptEntry()
        {
            this.targetSgMemberIndexes = new List<uint>();
        }
        public override bool Equals(object l)
        {
            if (l is MapAnimTransformScriptEntry)
            {
                MapAnimTransformScriptEntry m = (MapAnimTransformScriptEntry)l;
                return m.animIndex == animIndex && m.parentSgbPath == parentSgbPath;
            }
            return false;
        }
    }

    public class MapAnimDoorScriptEntry
    {
        public int id { get; set; }
        public int animIndex { get; set; }
        public byte targetDoor1Idx { get; set; }
        public int targetDoor2Idx { get; set; }
        public int targetDoor3Idx { get; set; }
        public int targetDoor4Idx { get; set; }
        public MapAnimDoorOpenStyle openStyle { get; set; }
        public float timeLength { get; set; }
        public float openAngle { get; set; }
        public float openDistance { get; set; }
        public byte targetSoundOpeningIdx { get; set; }
        public byte targetSoundClosingIdx { get; set; }
        public MapAnimDoorCurveType curveType { get; set; }
        public MapAnimRotationAxis rotationAxis { get; set; }

        public string name { get; set; }
        public string scriptFileName { get; set; }
        public string parentSgbPath { get; set; }

        public override bool Equals(object l)
        {
            if (l is  MapAnimDoorScriptEntry)
            {
                MapAnimDoorScriptEntry m = (MapAnimDoorScriptEntry)l;
                return m.animIndex == animIndex && m.parentSgbPath == parentSgbPath;
            }
            return false;
        }
    }

    /// <summary>
    /// Class representing a Model.
    /// </summary>
    public class MapModel
    {
        public int id { get; set; }
        public string modelPath { get; set; }
        public string modelName { get; set; }
        public string avfxFilePath { get; set; }
        public int numMeshes { get; set; }

        public bool isEmissive { get; set; }

        public MapModel()
        {
            isEmissive = false;
        }

        public override bool Equals(object l)
        {
            if (l is MapModel)
            {
                MapModel m = (MapModel) l;
                return modelPath == m.modelPath &&
                       modelName == m.modelName &&
                       numMeshes == m.numMeshes && isEmissive == m.isEmissive;
            }
            return false;
        }
    }

    /// <summary>
    /// Plot data relevant to serializing plot data into WardInfo.json.
    /// See FFXIVHousingSim.DataWriter for more info.
    /// </summary>
    public class Plot
    {
        public Territory ward { get; set; }
        public bool subdiv { get; set; }
        public byte index { get; set; }
        public int defaultFenceId { get; set; }
        public Size size { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }
        
        public Plot() { }

        public Plot(Territory ward, bool sub, byte ind, Size size)
        {
            this.ward = ward;
            defaultFenceId = DefaultFences.fnc[(int) ward];
            this.index = ind;
            this.subdiv = sub;
            this.size = size;
        }

        public static Territory StringToWard(String ward)
        {
            return (Territory) Enum.Parse(typeof(Territory), ward.ToUpperInvariant());
        }
    }

    /// <summary>
    /// Class representing the exterior of a house via IDs. Structured to be compatible with FFXIV WardInfo landSet data.
    /// </summary>
    public class HousingExteriorStructure
    {
        public Size size { get; set; }
        public int[] fixtures;
    }

    public class HousingExteriorBlueprintSet
    {
        public static string[] SgbPaths = {"bg/ffxiv/sea_s1/hou/dyna/house/s1h0_03_s_house.sgb",
                                            "bg/ffxiv/sea_s1/hou/dyna/house/s1h0_01_m_house.sgb",
                                            "bg/ffxiv/sea_s1/hou/dyna/house/s1h0_02_l_house.sgb"};

        public HousingExteriorBlueprint[] set { get; set; }
    }

    /// <summary>
    /// A class representation of the locations in which to place HousingExteriorFixtures.<br />
    /// </summary>
    public class HousingExteriorBlueprint
    {
        public Size size { get; set; }
        public Dictionary<FixtureType, List<Transform>[]> fixtureTransforms;

        /// <summary>
        /// Constructor that populates the Blueprint dictionary.
        /// </summary>
        public HousingExteriorBlueprint()
        {
            fixtureTransforms = new Dictionary<FixtureType, List<Transform>[]>();
            fixtureTransforms.Add(FixtureType.rof, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.wal, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.wid, new List<Transform>[2]);
            fixtureTransforms.Add(FixtureType.dor, new List<Transform>[4]);
            fixtureTransforms.Add(FixtureType.orf, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.owl, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.osg, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.fnc, new List<Transform>[4]);
        }
    }

    /// <summary>
    /// A class representation of an exterior housing fixture.<br />
    /// For fixtures that have only one variant, the length of variant must be 1.
    /// </summary>
    public class HousingExteriorFixture
    {
        public int itemId { get; set; }

        public int fixtureId { get; set; }
        public byte fixtureModelKey { get; set; }
        public FixtureType fixtureType { get; set; }
        public int fixtureIntendedUse { get; set; }
        public Size size { get; set; }

        public string name { get; set; }
        public HousingExteriorFixtureVariant[] variants { get; set; }

        /// <summary>
        /// Returns a string array of paths to .sgbs for this Fixture.
        /// TODO: I'm not even done writing this method, but please clean it up at some point.
        /// </summary>
        /// <returns></returns>
        public string[] GetPaths()
        {
            int variants = GetVariants(fixtureType);

            string[] paths = new string[variants];

            /*  Opt paths are constructed differently
                Their intended use is always 20   */
            if (fixtureType == FixtureType.orf ||
                fixtureType == FixtureType.osg ||
                fixtureType == FixtureType.owl)
            {
                string fixtureTypePath = fixtureType.ToString().Substring(1);
                string sgbFormat = "bgcommon/hou/dyna/opt/{0}/{1:D4}/asset/opt_{0}_m{1:D4}.sgb";

                //Only one variant here
                paths[0] = string.Format(sgbFormat, fixtureTypePath, fixtureModelKey);
            }
            /*
             * Doors and windows must take fixtureIntendedUse into account.
             */
            else if (fixtureType == FixtureType.dor)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"ca", "cb", "ci", "co"};

                string sgbFormat = "bgcommon/hou/dyna/{0}/{1}/{2:D4}/asset/{0}_{3}_{1}{2:D4}.sgb";
                string placeString = "";

                //Can get info from TerritoryType... but this is fine
                switch (fixtureIntendedUse)
                {
                    case 20:
                        placeString = "com";
                        break;
                    case 22:
                        placeString = "s1h0";
                        break;
                    case 23:
                        placeString = "f1h0";
                        break;
                    case 24:
                        placeString = "w1h0";
                        break;
                    case 2402:
                        placeString = "e1h0";
                        break;
                }

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, placeString, fixtureTypePath, fixtureModelKey,
                        ((DoorVariants) i).ToString());
            }
            else if (fixtureType == FixtureType.wid)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"ci", "co"};

                string sgbFormat = "bgcommon/hou/dyna/{0}/{1}/{2:D4}/asset/{0}_{3}_{1}{2:D4}.sgb";
                string placeString = "";

                //Can get info from TerritoryType... but this is fine
                switch (fixtureIntendedUse)
                {
                    case 20:
                        placeString = "com";
                        break;
                    case 22:
                        placeString = "s1h0";
                        break;
                    case 23:
                        placeString = "f1h0";
                        break;
                    case 24:
                        placeString = "w1h0";
                        break;
                    case 2402:
                        placeString = "e1h0";
                        break;
                }

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, placeString, fixtureTypePath, fixtureModelKey,
                        ((WindowVariants) i).ToString());
            }
            else if (fixtureType == FixtureType.fnc)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"a", "b", "c", "d"};

                string sgbFormat = "bgcommon/hou/dyna/com/c_{0}/{1:D4}/asset/com_f_{0}{1:D4}{2}.sgb";

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, fixtureTypePath, fixtureModelKey,
                        ((FenceVariants) i).ToString());
            }
            else
            {
                string fixtureSizeStr = size.ToString();
                string fixtureTypePath = fixtureType.ToString();

                string sgbFormat = "bgcommon/hou/dyna/com/{0}_{1}/{2:D4}/asset/com_{0}_{1}{2:D4}.sgb";

                paths[0] = string.Format(sgbFormat, fixtureSizeStr, fixtureTypePath, fixtureModelKey);
            }

            return paths;
        }

        /// <summary>
        /// For use with fences in which the .sgb files are in HousingExterior.
        /// JANK AF
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string[] GetPaths(string s)
        {
            string[] paths = new string[4];

            string cutoff = s.Substring(0, s.Length - 5);

            paths[0] = s;
            paths[1] = cutoff + "b.sgb";
            paths[2] = cutoff + "c.sgb";
            paths[3] = cutoff + "d.sgb";

            return paths;
        }

        /// <summary>
        /// Returns the number of variants available for a given FixtureType.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int GetVariants(FixtureType f)
        {
            switch (f)
            {
                case FixtureType.wid:
                    return 2;
                case FixtureType.dor:
                    return 4;
                case FixtureType.fnc:
                    return 4;
                default:
                    return 1;
            }
        }
    }

    /// <summary>
    /// A class representation of an .sgb file for an exterior housing fixture.
    /// </summary>
    public class HousingExteriorFixtureVariant
    {
        public string sgbPath { get; set; }
        public HousingExteriorFixtureModel[] models { get; set; }
    }

    /// <summary>
    /// A class representation of an .mdl file belonging to a HousingExteriorFixtureVariant.
    /// </summary>
    public class HousingExteriorFixtureModel
    {
        public string modelPath { get; set; }
        public string modelName { get; set; }
        public int numMeshes { get; set; }
        public Transform transform { get; set; }
    }
}

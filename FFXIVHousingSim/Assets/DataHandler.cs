using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using FFXIVHSLib;
using Newtonsoft.Json;
using Object = System.Object;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

/// <summary>
/// Handles all extracted FFXIV data.
/// </summary>
public static class DataHandler
{
	private static bool DebugLoadMap = true;
	private static bool DebugCustomLoad = false;
	private static bool DebugLoadExteriors = false;
	
//	private static int[] debugCustomLoadList =
//	{
//		0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30
//	};
	
	private static int[] debugCustomLoadList =
	{
		126
	};
	
    //Serialized FFXIV data
    private static List<Plot> _wardInfo;
	private static HousingExteriorStructure[] _landSet;
    private static Dictionary<int, HousingExteriorFixture> _exteriorFixtures;
    private static HousingExteriorBlueprintSet _blueprints;
    private static Map _map;

    //Extracted model handling

    private struct CustomMesh
    {
        public Mesh mesh;
        public bool emissive;
    }
    private static Dictionary<int, CustomMesh[]> _modelMeshes;
	private static Dictionary<int, Mesh[][][]> _exteriorFixtureMeshes;
	private static Dictionary<int, FFXIVHSLib.Transform[][]> _exteriorFixtureMeshTransforms;
    
    // script shits
    private static Dictionary<int, string> _mapAnimScriptNames;
    private static Dictionary<int, string> _mapMovePathScriptNames;
    private static Dictionary<int, string> _mapAnimTransformScriptNames;
    private static Dictionary<int, string> _mapAnimDoorScriptNames;

    private static Dictionary<int, MonoBehaviour> _mapAnimScriptObjs;
    private static Dictionary<int, MonoBehaviour> _mapMovePathScriptObjs;
    private static Dictionary<int, MonoBehaviour> _mapAnimTransformScriptObjs;
    private static Dictionary<int, MonoBehaviour> _mapAnimDoorScriptObjs;

    private static Territory _territory = (Territory) 999;
    private static string _teriStr;

    public static GameObject rootGameObj;

    public static string teriStr
    {
        get { return _teriStr; }
        set
        {
            if (_teriStr == value)
                return;

            _teriStr = value;

            Debug.LogFormat("Territory changed to {0}", value);

            //TODO: When events implemented make this an event
            //CameraHandler[] c = Resources.FindObjectsOfTypeAll<CameraHandler>();
            //c[0]._territory = null;

            GameObject[] currentGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            //Destroy old objects
            if (currentGameObjects.Length > defaultGameObjects.Length)
                foreach (GameObject obj in currentGameObjects)
                    if (!defaultGameObjects.Contains(obj))
                        UnityEngine.Object.Destroy(obj);

            LoadTerritory();
        }
    }
    public static Territory territory
    {
        get { return _territory; }
        set
        {
	        if (_territory == value)
		        return;
	        
			_territory = value;
	        Debug.LogFormat("Territory changed to {0}", value.ToString());
	        
	        //TODO: When events implemented make this an event
			
			GameObject[] currentGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
	
			//Destroy old objects
			if (currentGameObjects.Length > defaultGameObjects.Length)
				foreach (GameObject obj in currentGameObjects)
					if (!defaultGameObjects.Contains(obj))
						UnityEngine.Object.Destroy(obj);

			LoadTerritory();
        }
    }
    
    public static GameObject[] defaultGameObjects { get; set; }
    
    private static void LoadWardInfo()
    {
        string jsonText = File.ReadAllText(FFXIVHSPaths.GetWardInfoJson());

        _wardInfo = JsonConvert.DeserializeObject<List<Plot>>(jsonText);
    }

    private static void LoadExteriorFixtures()
    {
        string jsonText = File.ReadAllText(FFXIVHSPaths.GetHousingExteriorJson());

        _exteriorFixtures = JsonConvert.DeserializeObject<Dictionary<int, HousingExteriorFixture>>(jsonText);
    }

    private static void LoadExteriorBlueprints()
    {
        string jsonText = File.ReadAllText(FFXIVHSPaths.GetHousingExteriorBlueprintSetJson());

        _blueprints = JsonConvert.DeserializeObject<HousingExteriorBlueprintSet>(jsonText);
    }

    private static void LoadMapTerrainInfo()
    {
        UnityEngine.Debug.Log(teriStr);//
        string jsonText = File.ReadAllText(FFXIVHSPaths.GetTerritoryJson(teriStr));
        //
        _map = JsonConvert.DeserializeObject<Map>(jsonText);
	    Debug.Log("_map loaded.");
    }

    private static void LoadMapMeshes()
    {
        LoadMapTerrainInfo();

        _modelMeshes = new Dictionary<int, CustomMesh[]>();

	    string objectsFolder = FFXIVHSPaths.GetTerritoryObjectsDirectory(_teriStr);
        
        foreach (MapModel model in _map.models.Values)
        {
            CustomMesh[] modelMeshes = new CustomMesh[model.numMeshes];

            for (int i = 0; i < model.numMeshes; i++)
            {
                string meshFileName = string.Format("{0}{1}_{2}.obj", objectsFolder, model.modelName, i);
	            modelMeshes[i].mesh = FastObjImporter.Instance.ImportFile(meshFileName);
                modelMeshes[i].emissive = model.isEmissive;

                //AssetDatabase.CreateAsset(modelMeshes[i].mesh, meshFileName + ".asset");
            }
            _modelMeshes.Add(model.id, modelMeshes);
        }
	    Debug.Log("_modelMeshes loaded.");
    }
    
    private static void LoadAnimationScripts()
    {
        LoadMapTerrainInfo();
        _mapAnimScriptNames = new Dictionary<int, string>();
        _mapAnimScriptObjs = new Dictionary<int, MonoBehaviour>();
        _mapMovePathScriptNames = new Dictionary<int, string>();
        _mapMovePathScriptObjs = new Dictionary<int, MonoBehaviour>();
        _mapAnimTransformScriptNames = new Dictionary<int, string>();
        _mapAnimTransformScriptObjs = new Dictionary<int, MonoBehaviour>();
        _mapAnimDoorScriptNames = new Dictionary<int, string>();
        _mapAnimDoorScriptObjs = new Dictionary<int, MonoBehaviour>();

        {
            var scriptFolder = Path.Combine(Path.Combine(FFXIVHSPaths.GetRootDirectory(), teriStr + "\\"), "scripts\\");
            foreach (MapAnimScriptEntry animScript in _map.animScripts.Values)
            {
                if (!_mapAnimScriptNames.ContainsKey(animScript.id))
                    _mapAnimScriptNames.Add(animScript.id, animScript.name);

                var fname = "./Assets/" + animScript.scriptFileName;
                File.Copy(Path.Combine(scriptFolder, animScript.scriptFileName), "./Assets/" + animScript.scriptFileName, true);
                UnityEditor.AssetDatabase.ImportAsset(fname);
                MonoBehaviour scriptObj = UnityEditor.AssetDatabase.LoadAssetAtPath<MonoBehaviour>(fname);

                if (!_mapAnimScriptObjs.ContainsKey(animScript.id))
                    _mapAnimScriptObjs.Add(animScript.id, scriptObj);
            }

            foreach (MapAnimTransformScriptEntry animScript in _map.animTransformScripts.Values)
            {
                if (!_mapAnimTransformScriptNames.ContainsKey(animScript.id))
                    _mapAnimTransformScriptNames.Add(animScript.id, animScript.name);

                var fname = "./Assets/" + animScript.scriptFileName;
                File.Copy(Path.Combine(scriptFolder, animScript.scriptFileName), "./Assets/" + animScript.scriptFileName, true);
                UnityEditor.AssetDatabase.ImportAsset(fname);
                MonoBehaviour scriptObj = UnityEditor.AssetDatabase.LoadAssetAtPath<MonoBehaviour>(fname);

                if (!_mapAnimTransformScriptObjs.ContainsKey(animScript.id))
                    _mapAnimTransformScriptObjs.Add(animScript.id, scriptObj);
            }

            foreach (MapAnimDoorScriptEntry animScript in _map.animDoorScripts.Values)
            {
                if (!_mapAnimDoorScriptNames.ContainsKey(animScript.id))
                    _mapAnimDoorScriptNames.Add(animScript.id, animScript.name);

                var fname = "./Assets/" + animScript.scriptFileName;
                File.Copy(Path.Combine(scriptFolder, animScript.scriptFileName), "./Assets/" + animScript.scriptFileName, true);
                UnityEditor.AssetDatabase.ImportAsset(fname);
                MonoBehaviour scriptObj = UnityEditor.AssetDatabase.LoadAssetAtPath<MonoBehaviour>(fname);

                if (!_mapAnimDoorScriptObjs.ContainsKey(animScript.id))
                    _mapAnimDoorScriptObjs.Add(animScript.id, scriptObj);
            }

            foreach (MapMovePathScriptEntry movePathScript in _map.movePathScripts.Values)
            {
                if (!_mapMovePathScriptNames.ContainsKey(movePathScript.id))
                    _mapMovePathScriptNames.Add(movePathScript.id, movePathScript.name);

                var fname = "./Assets/" + movePathScript.scriptFileName;
                File.Copy(Path.Combine(scriptFolder, movePathScript.scriptFileName), "./Assets/" + movePathScript.scriptFileName, true);
                UnityEditor.AssetDatabase.ImportAsset(fname);
                MonoBehaviour scriptObj = UnityEditor.AssetDatabase.LoadAssetAtPath<MonoBehaviour>(fname);

                if (!_mapMovePathScriptObjs.ContainsKey(movePathScript.id))
                    _mapMovePathScriptObjs.Add(movePathScript.id, scriptObj);
            }
        }
        //UnityEditor.AssetDatabase.Build();
        UnityEditor.AssetDatabase.Refresh();

    }

    private static void LoadTerritory()
    {
        rootGameObj = new GameObject();
        rootGameObj.name = teriStr;

        LoadAnimationScripts();
	    if (DebugLoadMap)
	    {
            LoadMapMeshes();

            foreach (MapGroup group in _map.groups.Values)
			    LoadMapGroup(group);
	    }
        //LoadLights();
	    //LoadLandset();
    }

    private static void LoadLight(MapLightEntry light)
    {
        
    }

    private static void LoadMapGroup(MapGroup group, GameObject parent = null)
	{
		if (group.groupName.Contains("fnc0000"))
			return;
		
		GameObject groupRootObject = new GameObject(group.groupName);

        // anim scripts
        if (group.animScriptRefs != null)
            foreach (var mapScriptId in group.animScriptRefs)
                groupRootObject.AddComponent(Type.GetType(_mapAnimScriptNames[mapScriptId]));
        if (group.movePathScriptRefs != null)
            foreach (var mapScriptId in group.movePathScriptRefs)
            {
                groupRootObject.AddComponent(Type.GetType(_mapMovePathScriptNames[mapScriptId]));
                Debug.LogFormat("========MOVEPATH: " + _mapMovePathScriptNames[mapScriptId] + " =========\n");
            }
        if (group.animTransformScriptRefs != null)
            foreach (var mapScriptId in group.animTransformScriptRefs)
                groupRootObject.AddComponent(Type.GetType(_mapAnimTransformScriptNames[mapScriptId]));

        if (parent == null)
		{
            groupRootObject.GetComponent<Transform>().SetParent(rootGameObj.GetComponent<Transform>());
			groupRootObject.GetComponent<Transform>().position = Vector3.Reflect(group.groupTransform.translation, Vector3.left);
			groupRootObject.GetComponent<Transform>().rotation = Quaternion.Euler(Vector3.Reflect(group.groupTransform.rotation.ToVector3(), Vector3.left));
			groupRootObject.GetComponent<Transform>().localScale = Vector3.Reflect(group.groupTransform.scale, Vector3.left);
		}
		else
		{
			groupRootObject.GetComponent<Transform>().SetParent(parent.GetComponent<Transform>());
			groupRootObject.GetComponent<Transform>().localPosition = group.groupTransform.translation;
			groupRootObject.GetComponent<Transform>().localRotation = group.groupTransform.rotation;
			groupRootObject.GetComponent<Transform>().localScale = group.groupTransform.scale;
		}
		
		groupRootObject.SetActive(true);
		
		if (group.entries != null && group.entries.Count > 0)
		{
			foreach (MapModelEntry entry in group.entries)
			{
				CustomMesh[] meshes = _modelMeshes[entry.modelId];
				GameObject obj = AddMeshToNewGameObject(meshes, true);

                // anim scripts
                foreach (var mapScriptId in entry.animScriptIds)
                {
                    var comp = Type.GetType(_mapAnimScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimScriptNames[mapScriptId] + "\n");
                }

                foreach (var mapScriptId in entry.animTransformScriptIds)
                {
                    var comp = Type.GetType(_mapAnimTransformScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimTransformScriptNames[mapScriptId] + "\n");
                }

                foreach (var mapScriptId in entry.animDoorScriptIds)
                {
                    var comp = Type.GetType(_mapAnimDoorScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimDoorScriptNames[mapScriptId] + "\n");
                }
                obj.GetComponent<Transform>().SetParent(groupRootObject.GetComponent<Transform>());
				obj.GetComponent<Transform>().localPosition = entry.transform.translation;
				obj.GetComponent<Transform>().localRotation = entry.transform.rotation;
				obj.GetComponent<Transform>().localScale = entry.transform.scale;
				obj.SetActive(true);
			}	
		}

        if (group.lights != null && group.lights.Count > 0)
        {
            foreach (MapLightEntry entry in group.lights)
            {
                UnityEngine.GameObject obj = new UnityEngine.GameObject("LIGHT_" + entry.id + "_" + entry.layerId);

                // anim scripts
                foreach (var mapScriptId in entry.animScriptIds)
                {
                    var comp = Type.GetType(_mapAnimScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimScriptNames[mapScriptId] + "\n");
                }

                foreach (var mapScriptId in entry.animTransformScriptIds)
                {
                    var comp = Type.GetType(_mapAnimTransformScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimTransformScriptNames[mapScriptId] + "\n");
                }

                foreach (var mapScriptId in entry.animDoorScriptIds)
                {
                    var comp = Type.GetType(_mapAnimDoorScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimDoorScriptNames[mapScriptId] + "\n");
                }

                obj.GetComponent<Transform>().SetParent(groupRootObject.GetComponent<Transform>());
                obj.GetComponent<Transform>().localPosition = entry.transform.translation;
                obj.GetComponent<Transform>().localRotation = entry.transform.rotation;
                obj.GetComponent<Transform>().localScale = entry.transform.scale;

                UnityEngine.Light light = obj.AddComponent<UnityEngine.Light>();
                float x = entry.transform.scale.x;

                if (x != 1.0f && x != -1.0f)
                    light.areaSize = new UnityEngine.Vector2(entry.transform.scale.x, entry.rangeRate);

                // LightType
                {
                    if (entry.lightType == "Point")
                        light.type = UnityEngine.LightType.Point;
                    else if (entry.lightType == "Spot")
                        light.type = UnityEngine.LightType.Spot;
                    else if (entry.lightType == "Directional")
                        light.type = UnityEngine.LightType.Directional;
                    else if (entry.lightType == "Line" || entry.lightType == "Plane")
#if UNITY_2018_1_OR_NEWER
                        light.type = UnityEngine.LightType.Rectangle;
#else
                        light.type = UnityEngine.LightType.Area;
#endif
                    else
                        light.type = UnityEngine.LightType.Point;

                    /*  Directional, Point, Spot, Plane, Line, FakeSpecular, */
                }

                // Attenuation
                // RangeRate
                // PointLightType
                // AttenuationConeCoefficient
                // ConeDegree
                light.spotAngle = entry.coneDegree;

                // ColorHDRI
                float intensity = entry.colorIntensity;
                float a = entry.color.alpha / 255.0f;
                float b = entry.color.blue / 255.0f;
                float g = entry.color.green / 255.0f;
                float r = entry.color.red / 255.0f;

                light.color = new UnityEngine.Color(r, g, b, a);
                if (entry.color.alpha == 255 && entry.color.blue == 255 && entry.color.green == 255 && entry.color.red == 255)
                    light.intensity = 0.0f;
                else
                    light.intensity = intensity * 1.25f;


                // FollowsDirectionalLight
                // SpecularEnabled
                // BGShadowEnabled

                // ShadowClipRange
                light.shadowNearPlane = entry.shadowClipRange;
                light.shadows = UnityEngine.LightShadows.Hard;
                light.range = 15.0f;

                obj.SetActive(true);
            }
        }

        if (group.vfx != null && group.vfx.Count > 0)
        {
            foreach (MapVfxEntry entry in group.vfx)
            {
                foreach (var modelId in entry.modelIds)
                {
                    CustomMesh[] meshes = _modelMeshes[modelId];
                    // uncomment to try load avfx meshes
                    GameObject obj = AddMeshToNewGameObject(meshes, false);

                    // anim scripts
                    foreach (var mapScriptId in entry.animScriptIds)
                    {
                        var comp = Type.GetType(_mapAnimScriptNames[mapScriptId]);
                        obj.AddComponent(comp);
                        Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimScriptNames[mapScriptId] + "\n");
                    }

                    foreach (var mapScriptId in entry.animTransformScriptIds)
                    {
                        var comp = Type.GetType(_mapAnimTransformScriptNames[mapScriptId]);
                        obj.AddComponent(comp);
                        Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimTransformScriptNames[mapScriptId] + "\n");
                    }

                    foreach (var mapScriptId in entry.animDoorScriptIds)
                    {
                        var comp = Type.GetType(_mapAnimDoorScriptNames[mapScriptId]);
                        obj.AddComponent(comp);
                        Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimDoorScriptNames[mapScriptId] + "\n");
                    }
                    // anim scripts
                    foreach (var mapScriptId in entry.animScriptIds)
                        obj.AddComponent(Type.GetType(_mapAnimScriptNames[mapScriptId]));

                    //GameObject obj = new GameObject();

                    obj.name = ("VFX_" + entry.id + "_" + entry.layerId + "_" + System.IO.Path.GetFileNameWithoutExtension(entry.avfxPath) + "_" + modelId);

                    obj.GetComponent<Transform>().SetParent(groupRootObject.GetComponent<Transform>());
                    obj.GetComponent<Transform>().localPosition = entry.transform.translation;
                    obj.GetComponent<Transform>().localRotation = entry.transform.rotation;
                    obj.GetComponent<Transform>().localScale = entry.transform.scale;

                    UnityEngine.Light light = obj.AddComponent<UnityEngine.Light>();
                    float x = entry.transform.scale.x;

                    if (x != 1.0f && x != -1.0f)
                        light.areaSize = new UnityEngine.Vector2(entry.transform.scale.x, entry.transform.scale.y);

                    //float intensity = entry.colorIntensity;
                    float a = entry.color.alpha / 255.0f;
                    float b = entry.color.blue / 255.0f;
                    float g = entry.color.green / 255.0f;
                    float r = entry.color.red / 255.0f;

                    light.color = new UnityEngine.Color(r, g, b, a);
                    if (entry.color.alpha == 255 && entry.color.blue == 255 && entry.color.green == 255 && entry.color.red == 255)
                        light.intensity = 0.0f;
                    else
                        light.intensity = 1.25f;

                    light.type = UnityEngine.LightType.Point;
                    light.shadows = UnityEngine.LightShadows.Hard;
                    light.range = 15.0f;

                    obj.SetActive(true);
                }
            }
        }

        if (group.sounds != null && group.sounds.Count > 0)
        {
            foreach (MapSoundEntry entry in group.sounds)
            {
                GameObject obj = new GameObject();
                obj.name = "SOUND_" + entry.fileName;

                // anim scripts
                foreach (var mapScriptId in entry.animScriptIds)
                    obj.AddComponent(Type.GetType(_mapAnimScriptNames[mapScriptId]));

                foreach (var mapScriptId in entry.animTransformScriptIds)
                {
                    var comp = Type.GetType(_mapAnimTransformScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimTransformScriptNames[mapScriptId] + "\n");
                }

                foreach (var mapScriptId in entry.animDoorScriptIds)
                {
                    var comp = Type.GetType(_mapAnimDoorScriptNames[mapScriptId]);
                    obj.AddComponent(comp);
                    Debug.LogFormat("========Entry: " + entry.id + "Script: " + _mapAnimDoorScriptNames[mapScriptId] + "\n");
                }
                obj.GetComponent<Transform>().SetParent(groupRootObject.GetComponent<Transform>());
                obj.GetComponent<Transform>().localPosition = entry.transform.translation;
                obj.GetComponent<Transform>().localRotation = entry.transform.rotation;
                obj.GetComponent<Transform>().localScale = entry.transform.scale;

                obj.SetActive(true);
            }
        }

		if (group.groups != null && group.groups.Count > 0)
		{
			foreach (MapGroup subGroup in group.groups)
			{
				if (subGroup != null)
					LoadMapGroup(subGroup, groupRootObject);
			}	
		}
        //AssetDatabase.CreateAsset(groupRootObject)
	}

    private static void WriteScriptFile(String path, FFXIVHSLib.MapAnimScriptEntry entry)
    {
        // todo: delay memes too
        string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
        outStr += "public class " + entry.name + " : MonoBehaviour {\n";
        outStr += "\n\n";
        outStr += "\tfloat delayTime = " + entry.delay + "f;\n";
        outStr += "\tvoid Start(){}\n\n";
        outStr += "\tvoid Update() {\n\t";
        outStr += "\ttransform.Rotate";
        if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.X)
            outStr += $"(Time.deltaTime * {entry.fullRotationTime / 10.0f}f, 0.0f, 0.0f);";
        else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Y)
            outStr += $"(0.0f, Time.deltaTime * {entry.fullRotationTime / 10.0f}f, 0.0f);";
        else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Z)
            outStr += $"(0.0f, 0.0f, Time.deltaTime * {entry.fullRotationTime / 10.0f});";

        outStr += "\n\t}";
        outStr += "\n}\n";

        File.WriteAllText(path, outStr);
    }

    private static void AddMeshToGameObject(Mesh[] meshes, GameObject obj)
	{
		Renderer objRenderer = obj.GetComponent<Renderer>();
		Material[] mats = new Material[meshes.Length];
		MaterialHandler mtlHandler = MaterialHandler.GetInstance();
	
		for (int i = 0; i < meshes.Length; i++)
		{
			Material mat = mtlHandler.GetMaterialForMesh(meshes[i].name);
				
			if (mat == null)
			{
				Debug.LogFormat("Could not find material for mesh {0}", meshes[i].name);
				mats[i] = mtlHandler.GetMaterialForMesh("default");
			}
			else
				mats[i] = mat;
		}
		objRenderer.materials = mats;
			
		Mesh main = new Mesh();
		main.subMeshCount = meshes.Length;
			
		if (meshes.Length == 0)
		{
			obj.GetComponent<MeshFilter>().mesh = main;
		}
		else
		{
			CombineInstance[] combine = new CombineInstance[meshes.Length];
						
			for (int i = 0; i < meshes.Length; i++)
			{
				combine[i].mesh = meshes[i];
				combine[i].transform = Matrix4x4.identity;
			}
			main.CombineMeshes(combine);
		
			for (int i = 0; i < main.subMeshCount; i++)
			{
				int[] tri = new int[0];
		
				tri = meshes[i].triangles;
		
				int offset = 0;
				if (i > 0)
					for (int j = 0; j < i; j++)
						offset += meshes[j].vertexCount;
						
				main.SetTriangles(tri, i, false, offset);
					
				//Don't ask?
				if (main.subMeshCount != meshes.Length)
					main.subMeshCount = meshes.Length;
			}
		
			obj.GetComponent<MeshFilter>().mesh = main;
		}
	}
	
	private static GameObject AddMeshToNewGameObject(CustomMesh[] cmeshes, bool addMeshCollider = false, string name = null)
	{
        Mesh[] meshes = new Mesh[cmeshes.Length];
        for (int i = 0; i < meshes.Length; ++i)
            meshes[i] = cmeshes[i].mesh;
		//Set up our gameobject and add a renderer and filter
		GameObject obj = new GameObject();
		obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshFilter>();
		
		Renderer objRenderer = obj.GetComponent<Renderer>();
		Material[] mats = new Material[meshes.Length];
		MaterialHandler mtlHandler = MaterialHandler.GetInstance();

		for (int i = 0; i < meshes.Length; i++)
		{
			Material mat = mtlHandler.GetMaterialForMesh(meshes[i].name);
            bool emissive = false;//cmeshes[i].emissive;

			if (mat == null)
			{
				Debug.LogFormat("Could not find material for mesh {0}", meshes[i].name);
				mats[i] = mtlHandler.GetMaterialForMesh("default");
			}
			else
				mats[i] = mat;

            if (emissive)
            {
                mats[i].EnableKeyword("_EmissionPow");
                mats[i].SetFloat("_EmissionPow", 1.25f);
            }
        }
		objRenderer.materials = mats;
			
		Mesh main = new Mesh();
		main.subMeshCount = meshes.Length;
			
		if (meshes.Length == 0)
		{
			obj.GetComponent<MeshFilter>().mesh = main;
		}
		else
		{
			CombineInstance[] combine = new CombineInstance[meshes.Length];
						
			for (int i = 0; i < meshes.Length; i++)
			{
				combine[i].mesh = meshes[i];
				combine[i].transform = Matrix4x4.identity;
			}
			main.CombineMeshes(combine);
		
			for (int i = 0; i < main.subMeshCount; i++)
			{
				int[] tri = meshes[i].triangles;
		
				int offset = 0;
				if (i > 0)
					for (int j = 0; j < i; j++)
						offset += meshes[j].vertexCount;
						
				main.SetTriangles(tri, i, false, offset);
					
				//Don't ask?
				if (main.subMeshCount != meshes.Length)
					main.subMeshCount = meshes.Length;
			}
		
			obj.GetComponent<MeshFilter>().mesh = main;
		}
		if (addMeshCollider)
			obj.AddComponent<MeshCollider>();

		string newName = "";
		//Redo this
		if (name != null)
			newName = name;
		else
			newName = meshes.Length > 0 ? meshes[0].name.Substring(0, meshes[0].name.Length - 2) : "Unknown";

		obj.name = newName;
		obj.SetActive(false);

		return obj;
	}
	
	private static Mesh[][][] GetMeshesForExteriorFixture(int fixtureId, ref FFXIVHSLib.Transform[][] transformsPerModel)
	{
		//A little different this time!
		if (_exteriorFixtureMeshes == null)
			_exteriorFixtureMeshes = new Dictionary<int, Mesh[][][]>();

		if (_exteriorFixtureMeshTransforms == null)
			_exteriorFixtureMeshTransforms = new Dictionary<int, FFXIVHSLib.Transform[][]>();

		Mesh[][][] modelMeshes;
		if (!_exteriorFixtureMeshes.TryGetValue(fixtureId, out modelMeshes))
		{
			string exteriorHousingObjectsFolder = FFXIVHSPaths.GetHousingExteriorObjectsDirectory();
			
			//Load the meshes if not found
			HousingExteriorFixture fixture = _exteriorFixtures[fixtureId];

			//Initialize variants dimensions
			int numVariants = HousingExteriorFixture.GetVariants(fixture.fixtureType);
			modelMeshes = new Mesh[numVariants][][];
			transformsPerModel = new FFXIVHSLib.Transform[numVariants][];

			int i = 0;
			foreach (HousingExteriorFixtureVariant variant in fixture.variants)
			{
				//Initialize model dimensions for this variant
				int numModels = variant.models.Length;
				modelMeshes[i] = new Mesh[numModels][];
				transformsPerModel[i] = new FFXIVHSLib.Transform[numModels];
				
				int j = 0;
				foreach (HousingExteriorFixtureModel model in variant.models)
				{
					modelMeshes[i][j] = new Mesh[model.numMeshes];
					transformsPerModel[i][j] = model.transform;
					
					for (int k = 0; k < model.numMeshes; k++)
					{
						string meshFileName = string.Format("{0}{1}_{2}.obj", exteriorHousingObjectsFolder, model.modelName, k);
						modelMeshes[i][j][k] = FastObjImporter.Instance.ImportFile(meshFileName);
					}
					j++;
				}
				i++;
			}
			_exteriorFixtureMeshes.Add(fixtureId, modelMeshes);
			_exteriorFixtureMeshTransforms.Add(fixtureId, transformsPerModel);
		}
		else
		{
			//If the meshes are in the dictionary, so are the transforms :p
			transformsPerModel = _exteriorFixtureMeshTransforms[fixtureId];
		}
		return modelMeshes;
	}
    
    public static Plot GetPlot(Territory territory, int plotNum, bool subdiv)
    {
        if (_wardInfo == null)
            LoadWardInfo();

	    Debug.LogFormat("GetPlot {0} {1} {2}", territory, plotNum, subdiv);
        Plot p = _wardInfo.Where(_ => _.ward == territory &&
                                      _.index == plotNum &&
                                      _.subdiv == subdiv)
                                        .Select(_ => _).Single();

        return p;
    }
}
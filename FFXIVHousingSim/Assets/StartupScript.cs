﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FFXIVHSLib;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

using UnityEditor;
using System.Collections;
using UnityEditor.SceneManagement;

public class FFXIVHSDialog : EditorWindow
{
    public float saveTime = 300;
    public float nextSave = 0;
    [MenuItem("FFXIVHS/Load HS Export")]
    static void Init()
    {
        FFXIVHSDialog window = (FFXIVHSDialog)EditorWindow.GetWindowWithRect(typeof(FFXIVHSDialog), new Rect(0, 0, 640, 480));
        window.ShowUtility();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Import HS Export or Save current scene as unity prefab.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(10);

        string teriStr = "r2t1";
        teriStr = EditorGUILayout.TextField("Teri: ", teriStr);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import"))
        {
            DataHandler.defaultGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            DebugTimer timer = new DebugTimer();
            timer.registerEvent("Begin");

            DataHandler.teriStr = "r2t1";
            timer.registerEvent("TerritoryLoad");

            Debug.Log("Startupscript finished.");
        }
        else if (GUILayout.Button("Save prefab"))
        {
            string[] path = EditorSceneManager.GetActiveScene().path.Split(char.Parse("/"));
            path[path.Length - 1] = "Assets/Scenes/" + teriStr + ".unity";
            bool saveOK = EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), string.Join("/", path));
            Debug.Log("Saved Scene " + (saveOK ? "OK" : "Error!"));
        }
    }
}

public class StartupScript : MonoBehaviour
{
	private static bool debug = false;
    private static bool askedSave = false;
	void Start()
	{

	}

	// Update is called once per frame
	void Update () {
		if (!askedSave)
        {

        }
	}
}

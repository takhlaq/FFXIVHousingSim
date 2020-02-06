using System;
using System.Collections.Generic;
using System.IO;
using SaintCoinach.Graphics;

namespace FFXIVHSLauncher
{
    public static class ScriptFileWriter
    {
        private static bool debug = true;

        public static void WriteScriptFile(String path, FFXIVHSLib.MapAnimScriptEntry entry)
        {
            /*
             * using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rot : MonoBehaviour {

	int _framesElapsed = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//if (++_framesElapsed % 8 == 0)
			transform.Rotate (0.0f, 1 / 18.0f, 0.0f);
	}
}

             */
            // todo: delay memes too
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";
            outStr += "\tfloat delayTime = " + entry.delay + "f;\n";
            outStr += "\tvoid Start(){}\n\n";
            outStr += "\tvoid Update() {\n\t";
            outStr += "\ttransform.Rotate";
            if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.X)
                outStr += $"(Time.deltaTime * {entry.fullRotationTime / 100.0f}f, 0.0f, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Y)
                outStr += $"(0.0f, Time.deltaTime * {entry.fullRotationTime / 100.0f}f, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Z)
                outStr += $"(0.0f, 0.0f, Time.deltaTime * {entry.fullRotationTime / 100.0f}f);";

            outStr += "\n\t}";
            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }
    }
}
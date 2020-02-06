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
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";

            float newDelay = (float)Math.Sqrt(entry.delay * entry.delay);

            outStr += "\tfloat delay = " + newDelay + "f;\n";
            
            outStr += "\tvoid Start(){}\n\n";
            outStr += "\tvoid Update() {\n";

            outStr += "\t\tif (delay >= 0.0f) { delay -= 100 / Time.deltaTime; return; }\n";
            outStr += "\t\ttransform.Rotate";
            if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.X)
                outStr += $"({100 / entry.fullRotationTime}f, 0.0f, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Y)
                outStr += $"(0.0f, {100 / entry.fullRotationTime}f, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Z)
                outStr += $"(0.0f, 0.0f, {100 / entry.fullRotationTime}f);";

            outStr += "\n\t}";
            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }
    }
}
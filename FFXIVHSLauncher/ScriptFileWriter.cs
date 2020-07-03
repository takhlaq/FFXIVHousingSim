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
            outStr += "\tfloat fullRotationTime = " + entry.fullRotationTime + "f;\n";
            outStr += "\tvoid Start(){}\n\n";
            outStr += "\tvoid Update() {\n";

            outStr += "\t\tif (delay >= 0.0f) { delay -= Time.deltaTime * 10.0f; return; }\n";
            outStr += "\t\ttransform.Rotate";

            string rotateStr = "(float)(100.0f / fullRotationTime)";

            if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.X)
                outStr += $"({rotateStr}, 0.0f, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Y)
                outStr += $"(0.0f, {rotateStr}, 0.0f);";
            else if (entry.axis == FFXIVHSLib.MapAnimRotationAxis.Z)
                outStr += $"(0.0f, 0.0f, {rotateStr});";

            outStr += "\n\t}";
            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }


        public static void WriteScriptFile(String path, FFXIVHSLib.MapMovePathScriptEntry entry)
        {
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";

            float newDelay = (float)Math.Sqrt(entry.accelerateTime * entry.accelerateTime);

            outStr += "\tbyte autoPlay = " + entry.autoPlay + ";\n";
            outStr += "\tfloat delay = " + newDelay + "f;\n";
            outStr += "\tfloat fullRotationTime = " + entry.time + "f;\n";
            outStr += "\tvoid Start(){}\n\n";
            outStr += "\tvoid Update() {\n";

            outStr += "\t\tif (autoPlay == 0) return;\n";
            outStr += "\t\tif (delay >= 0.0f) { delay -= Time.deltaTime * 10.0f; return; }\n";
            outStr += "\t\ttransform.Rotate";

            string rotateStr = "(float)(100.0f / fullRotationTime)";

            if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.YAxisOnly)
                outStr += $"(0.0f, {rotateStr}, 0.0f);";
            else if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.AllAxis)
                outStr += $"({rotateStr}, {rotateStr}, {rotateStr});";
            else
                outStr += "(0.0f, 0.0f, 0.0f);";

            outStr += "\n\t}";
            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }

        public static void WriteScriptFile(String path, FFXIVHSLib.MapAnimTransformScriptEntry entry)
        {
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";

            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }

        public static void WriteScriptFile(String path, FFXIVHSLib.MapAnimDoorScriptEntry entry)
        {
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";

            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }
    }
}
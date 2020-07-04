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
            outStr += "\tvoid FixedUpdate() {\n";

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
            outStr += "\n\n\tfloat offset = 1.0f;\n";
            outStr += "\n\n\tfloat timeElapsed = 0.0f;\n";

            outStr += "\tvoid Start(){ }\n\n";
            outStr += "\tvoid FixedUpdate() {\n";

            outStr += "\t\ttimeElapsed += Time.deltaTime * 10.0f;\n";
            outStr += "\t\tif (autoPlay == 0) return;\n";
            outStr += "\t\tif (delay >= 0.0f) { delay -= Time.deltaTime * 10.0f; return; }\n";
            outStr += "\t\tif (timeElapsed >= fullRotationTime) { timeElapsed = 0.0f; offset *= -1.0f;  }\n";
            
            // todo: fix this, it's wrong
            // todo: also actually do the horizontal range thing

            /*
            string rotateStr = "\t\ttransform.Rotate(float)(100.0f / fullRotationTime * offset)";

            if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.YAxisOnly)
                outStr += $"(0.0f, {rotateStr}, 0.0f);";
            else if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.AllAxis)
                outStr += $"({rotateStr}, {rotateStr}, {rotateStr});";
            else
                outStr += "(0.0f, 0.0f, 0.0f);";

            if (entry.swingMoveSpeedRange0 != 0.0f)
            {
                if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.YAxisOnly || entry.rotation == FFXIVHSLib.MapRotationTypeLayer.NoRotate)
                {
                    outStr += "\n\t\ttransform.Translate(0.0f, offset * Time.deltaTime, 0.0f);\n";
                }
                else if (entry.rotation == FFXIVHSLib.MapRotationTypeLayer.AllAxis)
                {
                    outStr += "\n\t\ttransform.Translate(offset * Time.deltaTime, offset * Time.deltaTime, offset * Time.deltaTime);\n";
                }
                else
                {

                }

            }
            //*/

            outStr += "\n\t}";
            outStr += "\n}\n";

            File.WriteAllText(path, outStr);
        }

        public static void WriteScriptFile(String path, FFXIVHSLib.MapAnimTransformScriptEntry entry)
        {
            string outStr = "using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\n";
            outStr += "public class " + entry.name + " : MonoBehaviour {\n";
            outStr += "\n\n";
            outStr += "\tfloat animTime = " + entry.time + ";\n";
            outStr += "\tfloat animStartEndTime = " + entry.startEndTime + ";\n";
            outStr += "\tbool animLoop = " + (entry.loop == 1 ? "true;\n" : "false;\n");
            outStr += "\tUnityEngine.Vector3 animTranslation = new UnityEngine.Vector3((float)" + entry.translation.x + ", (float)" + entry.translation.y + ", (float)" + entry.translation.z + ");\n";
            outStr += "\tUnityEngine.Vector3 animRotation = new UnityEngine.Vector3((float)" + entry.rotation.x + ", (float)" + entry.rotation.y + ", (float)" + entry.rotation.z + ");\n";
            outStr += "\tUnityEngine.Vector3 animScale = new UnityEngine.Vector3((float)" + entry.scale.x + ", (float)" + entry.scale.y + ", (float)" + entry.scale.z + ");\n";
            outStr += "\tUnityEngine.Vector3 animOffset = new UnityEngine.Vector3((float)" + entry.offset.x + ", (float)" + entry.offset.y + ", (float)" + entry.offset.z + ");\n";
            outStr += "\t// CurveLinear = 0, CurveSpline = 1;\n";
            outStr += "\tbyte animCurveType = " + (byte)entry.curveType + ";\n";
            outStr += "\t// OneWay = 0, RoundTrip = 1, Repetition = 2\n";
            outStr += "\tbyte animMovementType = " + (byte)entry.movementType + ";\n\n";

            outStr += "\tfloat timeElapsed = 0.0f;\n";

            outStr += "\tvoid Start(){\n";
            //outStr += "\tinitialPos = Instantiate(transform);\n";
            outStr +=  "\t}\n";
            outStr += "\tvoid FixedUpdate() {\n";

            outStr += "timeElapsed += Time.deltaTime * 10.0f;\n";
            outStr += "if (timeElapsed >= animTime){ timeElapsed = 0.0f; animTranslation *= -1.0f; animRotation *= -1.0f; animScale *= -1.0f; }\n";

            string animStr = "\t\ttransform.Translate(";
            if (entry.translation.x != 0.0f)
                animStr += "animTranslation.x * Time.deltaTime,";
            else
                animStr += "0.0f,";
            if (entry.translation.y != 0.0f)
                animStr += "animTranslation.y * Time.deltaTime,";
            else
                animStr += "0.0f,";
            if (entry.translation.z != 0.0f)
                animStr += "animTranslation.z);\n";
            else
                animStr += "0.0f);\n";

            animStr += "\n\t\ttransform.Rotate(";
            if (entry.rotation.x != 0.0f)
                animStr += "animRotation.x * Time.deltaTime,";
            else
                animStr += "0.0f,";
            if (entry.rotation.y != 0.0f)
                animStr += "animRotation.y * Time.deltaTime,";
            else
                animStr += "0.0f,";
            if (entry.rotation.z != 0.0f)
                animStr += "animRotation.z * Time.deltaTime);\n";
            else
                animStr += "0.0f);\n";

            //*
            animStr += "\n\t\ttransform.localScale = new UnityEngine.Vector3(transform.localScale.x + ";
            if (entry.scale.x != 1.0f)
                animStr += "animScale.x * Time.deltaTime, ";
            else
                animStr += "0.0f, ";

            animStr += "transform.localScale.y + ";
            if (entry.scale.y != 1.0f)
                animStr += "animScale.y * Time.deltaTime, ";
            else
                animStr += "0.0f, ";

            animStr += "transform.localScale.z + ";
            if (entry.scale.z != 1.0f)
                animStr += "animScale.z * Time.deltaTime);";
            else
                animStr += "0.0f);\n";

            //*/
            outStr += animStr;

            outStr += "\n\t}";
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using SaintCoinach.Graphics;

namespace FFXIVHSLauncher
{
    public static class ObjectFileWriter
    {
        private static bool debug = true;
        public static bool ExportPng = false;
        public static void WriteObjectFile(String path, ModelFile mdlFile)
        {
            try
            {
                Model mdl = mdlFile.GetModelDefinition().GetModel(ModelQuality.High);

                Mesh[] meshes = mdl.Meshes;

                List<Vector3> vsList = new List<Vector3>();
                List<Vector4> vcList = new List<Vector4>();
                List<Vector4> vtList = new List<Vector4>();
                List<Vector3> nmList = new List<Vector3>();
                List<Vector3> inList = new List<Vector3>();

                for (int i = 0; i < meshes.Length; i++)
                {
                    EnumerateFromVertices(meshes[i].Vertices, ref vsList, ref vcList, ref vtList, ref nmList);
                    EnumerateIndices(meshes[i].Indices, ref inList);

                    WriteObjectFileForMesh(path, mdl.Meshes[i].Material.Get(), mdl.Definition.File.Path, i, vsList, vcList, vtList, nmList, inList);

                    vsList.Clear();
                    vcList.Clear();
                    vtList.Clear();
                    nmList.Clear();
                    inList.Clear();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("FUCKING SHIT MODEL " + mdlFile.Path);
            }
        }

        private static void WriteAvfxModel(string path, SaintCoinach.Graphics.Avfx.AvfxFile file)
        {
            int invalidCount = 0;
            for (var i = 0; i < file.Models.Count; ++i) 
            {
                List<Vector3> vsList = new List<Vector3>();
                List<Vector4> vcList = new List<Vector4>();
                List<Vector4> vtList = new List<Vector4>();
                List<Vector3> nmList = new List<Vector3>();
                List<Vector3> inList = new List<Vector3>();

                var mesh = file.Models[i];
                var mdlPath = "./" + mesh.Name + ".mtrl";
                if (mesh.Indices.Length == 0)
                {
                    ++invalidCount;
                    continue;
                }
                List<ushort> indices = new List<ushort>();
                foreach (var f in mesh.Indices)
                {
                    indices.Add(f.I1);
                    indices.Add(f.I2);
                    indices.Add(f.I3);
                }
                EnumerateFromVertices(mesh.ConvertedVertexes, ref vsList, ref vcList, ref vtList, ref nmList);
                EnumerateIndices(indices.ToArray(), ref inList);

                WriteObjectFileForMesh(path, null, mdlPath, 0, vsList, vcList, vtList, nmList, inList, file.Textures.ToArray(), file.TexturePaths);
            }
        }

        public static void WriteObjectFile(string path, SaintCoinach.Graphics.Avfx.AvfxFile file)
        {
            WriteAvfxModel(path, file);
        }

        private static void EnumerateFromVertices(Vertex[] verts, ref List<Vector3> vsList, ref List<Vector4> vcList, 
                                                    ref List<Vector4> vtList, ref List<Vector3> nmList)
        {
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 vs = new Vector3();
                Vector4 vc = new Vector4();
                Vector4 vt = new Vector4();
                Vector3 nm = new Vector3();

                Vertex tv = verts[i];

                vs.X = tv.Position.Value.X;
                vs.Y = tv.Position.Value.Y;
                vs.Z = tv.Position.Value.Z;

                if (tv.Color == null)
                {
                    vc.X = 0;
                    vc.Y = 0;
                    vc.W = 0;
                    vc.Z = 0;
                }
                else
                {
                    vc.X = tv.Color.Value.X;
                    vc.Y = tv.Color.Value.Y;
                    vc.W = tv.Color.Value.W;
                    vc.Z = tv.Color.Value.Z;
                }

                vt.X = tv.UV.Value.X;
                vt.Y = -1.0f * tv.UV.Value.Y;
                vt.W = (tv.UV.Value.W == 0) ? vt.X : tv.UV.Value.W;
                vt.Z = (tv.UV.Value.Z == 0) ? vt.Y : (tv.UV.Value.Z * -1.0f);
           
                nm.X = tv.Normal.Value.X;
                nm.Y = tv.Normal.Value.Y;
                nm.Z = tv.Normal.Value.Z;

                vsList.Add(vs);
                vcList.Add(vc);
                vtList.Add(vt);
                nmList.Add(nm);
            }
        }

        private static void EnumerateIndices(ushort[] indices, ref List<Vector3> inList)
        {
            for (int i = 0; i < indices.Length; i+=3)
            {
                Vector3 ind = new Vector3();

                ind.X = indices[i + 0] + 1;
                ind.Y = indices[i + 1] + 1;
                ind.Z = indices[i + 2] + 1;

                inList.Add(ind);
            }
        }

        private static void WriteObjectFileForMesh(String path, Material mat, string modelName, int meshNumber, List<Vector3> vsList, 
                                            List<Vector4> vcList, List<Vector4> vtList, List<Vector3> nmList, List<Vector3> inList, SaintCoinach.Imaging.ImageFile[] textures = null, List<string> texturePaths = null)
        {
            int finalSep = modelName.LastIndexOf('/');
            modelName = modelName.Substring(finalSep);
            modelName = Path.GetFileNameWithoutExtension(modelName);
            modelName = modelName + "_" + meshNumber + ".obj";
            
            StreamWriter sw = new StreamWriter(new FileStream(path + "//" + modelName, FileMode.Create));
            sw.WriteLine("#for housing sim :-)");

            //mtl
            string mtlPath = @"..\textures\";
            string mtlName = mat == null ? modelName.Replace(".obj", ".mtrl") : mat.Definition.Name;

            int mtlFinalSep = mtlName.LastIndexOf('/');
            mtlName = mtlName.Substring(mtlFinalSep + 1);
            mtlName = mtlName.Replace(".mtrl", ".mtl");
            mtlPath += mtlName;

            sw.WriteLine("mtllib {0}", mtlPath);
            sw.WriteLine("usemtl {0}", mtlName.Replace(".mtl", ""));

            WriteMaterial(Path.GetFullPath(Path.Combine(path, mtlPath)), mtlName, mat, textures, texturePaths);

            //verts
            sw.WriteLine("#vert");
            foreach (Vector3 vert in vsList)
                sw.WriteLine("v {0} {1} {2}", (decimal)vert.X, (decimal)vert.Y, (decimal)vert.Z);
            sw.WriteLine();

            //colors
            sw.WriteLine("#vertex colors");
            foreach (Vector4 color in vcList)
            {
                sw.WriteLine("vc {0} {1} {2} {3}", (decimal)color.W, (decimal)color.X, (decimal)color.Y, (decimal)color.Z);
            }
            sw.WriteLine();

            //texcoords
            sw.WriteLine("#texcoords");
            if (!ExportPng)
                foreach (Vector4 texCoord in vtList)
                    sw.WriteLine("vt {0} {1} {2} {3}", (decimal)texCoord.X, -1 * (decimal)texCoord.Y, (decimal)texCoord.W, -1 * (decimal)texCoord.Z);
            else
                foreach (Vector4 texCoord in vtList)
                    sw.WriteLine("vt {0} {1} {2} {3}", (decimal)texCoord.X, (decimal)texCoord.Y, (decimal)texCoord.W, (decimal)texCoord.Z);
            
            sw.WriteLine();

            //normals
            sw.WriteLine("#normals");
            foreach (Vector3 norm in nmList)
                sw.WriteLine("vn {0} {1} {2}", (decimal)norm.X, (decimal)norm.Y, (decimal)norm.Z);
            sw.WriteLine();

            //indices
            sw.WriteLine("#indices");
            foreach (Vector3 ind in inList)
                sw.WriteLine("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", (decimal)ind.X, (decimal)ind.Y, (decimal)ind.Z);

            sw.Flush();
            sw.Close();
        }

        private static void WriteMaterial(String mtlPath, String mtlFileName, Material mat, SaintCoinach.Imaging.ImageFile[] textures = null, List<string> texturePaths = null)
        {
            if (File.Exists(mtlPath))
                return;

            String mtlFolder = mtlPath.Substring(0, mtlPath.LastIndexOf(@"\") + 1);

            if (!Directory.Exists(mtlFolder))
                Directory.CreateDirectory(mtlFolder);

            List<String> matLines = new List<String>();

            matLines.Add("newmtl " + mtlFileName.Substring(0, mtlFileName.Length - 4));
            if (mat != null)
            {
                textures = mat.TexturesFiles;
                texturePaths = new List<string>();
                foreach (var t in textures)
                    texturePaths.Add(t.Path);

                matLines.Add("# shader: " + mat.Shader);
            }
            string ext = ExportPng ? ".png" : ".dds";

            for (var i = 0; i < textures.Length; ++i)
            {
                var img = textures[i];
                String imgName = texturePaths[i];
                int imgLastSep = imgName.LastIndexOf('/');
                imgName = imgName.Substring(imgLastSep + 1);
                imgName = imgName.Replace(".atex", "_d.dds").Replace(".tex", ext);

                try
                {
                    //Write the image out
                    if (!File.Exists(mtlFolder + imgName))
                    {
                        byte[] ddsBytes = null;
                        if (!ExportPng && (ddsBytes = SaintCoinach.Imaging.ImageConverter.GetDDS(img)) != null)
                        {
                            System.IO.File.WriteAllBytes(mtlFolder + imgName, ddsBytes);
                        }
                        else
                        {
                            imgName = imgName.Replace(".dds", ".png");
                            //img.GetImage().Save(mtlFolder + imgName, System.Drawing.Imaging.ImageFormat.Png);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                
                                img.GetImage().Save(ms, ImageFormat.Png);
                                ms.Seek(0, SeekOrigin.Begin);

                                PngBitmapEncoder enc = new PngBitmapEncoder();
                                enc.Interlace = PngInterlaceOption.Off;
                                enc.Frames.Add(BitmapFrame.Create(ms));

                                using (FileStream ostream = new FileStream(mtlFolder + imgName, FileMode.Create))
                                {
                                    enc.Save(ostream);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("FUCKING SHIT TEXTURE " + imgName);
                    continue;
                }
                String imgSuffix = imgName.Substring(imgName.LastIndexOf(".") - 2, 2);

                switch (imgSuffix)
                {
                    case "_n":
                        matLines.Add("bump " + imgName);
                        break;
                    case "_s":
                        matLines.Add("map_Ks " + imgName);
                        break;
                    case "_h":
                    case "_d":
                        matLines.Add("map_Kd " + imgName);
                        break;
                    case "_a":
                        matLines.Add("map_Ka " + imgName);
                        break;
                    default:
                        matLines.Add("map_Ka " + imgName);
                        break;
                }
            }
            File.WriteAllLines(mtlPath, matLines);
        }

    }
}
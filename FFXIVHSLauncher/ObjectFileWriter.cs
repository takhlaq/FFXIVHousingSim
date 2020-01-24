using System;
using System.Collections.Generic;
using System.IO;
using SaintCoinach.Graphics;

namespace FFXIVHSLauncher
{
    public static class ObjectFileWriter
    {
        private static bool debug = true;

        public static void WriteObjectFile(String path, ModelFile mdlFile)
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

        private static void WriteAvfxModel(string path, Vector3 t, Vector3 r, Vector3 s, SaintCoinach.Graphics.Avfx.AvfxFile file)
        {

            for (var i = 0; i < file.Models.Count; ++i) 
            {
                List<Vector3> vsList = new List<Vector3>();
                List<Vector4> vcList = new List<Vector4>();
                List<Vector4> vtList = new List<Vector4>();
                List<Vector3> nmList = new List<Vector3>();
                List<Vector3> inList = new List<Vector3>();

                var mesh = file.Models[i];
                List<ushort> indices = new List<ushort>();
                foreach (var f in mesh.Indices)
                {
                    indices.Add(f.I1);
                    indices.Add(f.I2);
                    indices.Add(f.I3);
                }
                EnumerateFromVertices(mesh.ConvertedVertexes, ref vsList, ref vcList, ref vtList, ref nmList);
                EnumerateIndices(indices.ToArray(), ref inList);

                WriteObjectFileForMesh(path, null, file.File.Path, i, vsList, vcList, vtList, nmList, inList, file.Textures.ToArray());
            }
        }

        public static void WriteObjectFile(string path, SaintCoinach.Graphics.Lgb.LgbVfxEntry vfx)
        {
            WriteAvfxModel(path, vfx.Header.Translation, vfx.Header.Rotation, vfx.Header.Scale, vfx.AvfxFile);
        }

        public static void WriteObjectFile(string path, SaintCoinach.Graphics.Sgb.SgbVfxEntry vfx)
        {
            WriteAvfxModel(path, vfx.Header.Translation, vfx.Header.Rotation, vfx.Header.Scale, vfx.AvfxFile);
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
                vt.Y = -1 * tv.UV.Value.Y;
                vt.W = (tv.UV.Value.W == 0) ? vt.X : tv.UV.Value.W;
                vt.Z = (tv.UV.Value.Z == 0) ? vt.Y : tv.UV.Value.Z;
           
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
                                            List<Vector4> vcList, List<Vector4> vtList, List<Vector3> nmList, List<Vector3> inList, SaintCoinach.Imaging.ImageFile[] textures = null)
        {
            int finalSep = modelName.LastIndexOf('/');
            modelName = modelName.Substring(finalSep);
            modelName = Path.GetFileNameWithoutExtension(modelName);
            modelName = modelName + "_" + meshNumber + ".obj";
            
            StreamWriter sw = new StreamWriter(new FileStream(path + "//" + modelName, FileMode.Create));
            sw.WriteLine("#for housing sim :-)");

            //mtl
            string mtlPath = @"..\textures\";
            string mtlName = mat == null ? modelName : mat.Definition.Name;

            int mtlFinalSep = mtlName.LastIndexOf('/');
            mtlName = mtlName.Substring(mtlFinalSep + 1);
            mtlName = mtlName.Replace(".mtrl", ".mtl");
            mtlPath += mtlName;

            sw.WriteLine("mtllib {0}", mtlPath);
            sw.WriteLine("usemtl {0}", mtlName.Replace(".mtl", ""));

            WriteMaterial(Path.GetFullPath(Path.Combine(path, mtlPath)), mtlName, mat, textures);

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
            foreach (Vector4 texCoord in vtList)
            {
                sw.WriteLine("vt {0} {1} {2} {3}", (decimal)texCoord.X, -1 * (decimal)texCoord.Y, (decimal)texCoord.W, -1 * (decimal)texCoord.Z);
            }
            
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

        private static void WriteMaterial(String mtlPath, String mtlFileName, Material mat, SaintCoinach.Imaging.ImageFile[] textures = null)
        {
            if (File.Exists(mtlPath))
                return;
            
            String mtlFolder = mtlPath.Substring(0, mtlPath.LastIndexOf(@"\") + 1);

            if (!Directory.Exists(mtlFolder))
                Directory.CreateDirectory(mtlFolder);

            List<String> matLines = new List<String>();

            matLines.Add("newmtl " + mtlFileName.Substring(0, mtlFileName.Length - 4));

            if (mat != null)
                textures = mat.TexturesFiles;

            foreach (var img in textures)
            {
                String imgName = img.Path;
                int imgLastSep = imgName.LastIndexOf('/');
                imgName = imgName.Substring(imgLastSep + 1);
                imgName = imgName.Replace(".tex", ".dds");

                //Write the image out
                if (!File.Exists(mtlFolder + imgName))
                {
                    var ddsBytes = SaintCoinach.Imaging.ImageConverter.GetDDS(img);
                    if (ddsBytes != null)
                    {
                        System.IO.File.WriteAllBytes(mtlFolder + imgName, ddsBytes);
                    }
                    else
                    {
                        imgName = imgName.Replace(".dds", ".png");
                        img.GetImage().Save(mtlFolder + imgName);
                    }
                }
                String imgSuffix = imgName.Substring(imgName.Length - 6);

                switch (imgSuffix)
                {
                    case "_n.dds":
                    case "_n.png":
                        matLines.Add("bump " + imgName);
                        break;
                    case "_s.dds":
                    case "_s.png":
                        matLines.Add("map_Ks " + imgName);
                        break;
                    case "_d.dds":
                    case "_d.png":
                        matLines.Add("map_Kd " + imgName);
                        break;
                    case "_a.dds":
                    case "_a.png":
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
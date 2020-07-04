﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
 using UnityEngine.UI;

//TODO write this like fastobjimporter eventually to see if it will increase speed
public class MaterialHandler
{
	struct DDS_PIXELFORMAT
	{
		public uint dwSize;
		public uint dwFlags;
		public uint dwFourCC;
		public uint dwRGBBitCount;
		public uint dwRBitMask;
		public uint dwGBitMask;
		public uint dwBBitMask;
		public uint dwABitMask;
	};

	struct DDS_HEADER
	{
		public uint dwMagic;
		public uint dwSize;
		public uint dwFlags;
		public uint dwHeight;
		public uint dwWidth;
		public uint dwPitchOrLinearSize;
		public uint dwDepth;
		public uint dwMipMapCount;
		public uint dwReserved0;
		public uint dwReserved1;
		public uint dwReserved2;
		public uint dwReserved3;
		public uint dwReserved4;
		public uint dwReserved5;
		public uint dwReserved6;
		public uint dwReserved7;
		public uint dwReserved8;
		public uint dwReserved19;
		public uint dwReserved10;

		public DDS_PIXELFORMAT ddspf;
		public uint dwCaps;
		public uint dwCaps2;
		public uint dwCaps3;
		public uint dwCaps4;
		public uint dwReserved11;
	};

	enum DDSD_ENUM : uint
	{
		DDSD_CAPS = 0x1,//Required in every.dds file. 	
		DDSD_HEIGHT = 0x2,//Required in every.dds file.
		DDSD_WIDTH = 0x4,//Required in every.dds file.
		DDSD_PITCH = 0x8,//Required when pitch is provided for an uncompressed texture.
		DDSD_PIXELFORMAT = 0x1000,//Required in every.dds file.
		DDSD_MIPMAPCOUNT = 0x20000,//Required in a mipmapped texture.
		DDSD_LINEARSIZE = 0x80000,//Required when pitch is provided for a compressed texture.
		DDSD_DEPTH = 0x800000,//Required in a depth texture
	}

	enum DDPF_ENUM : uint
	{
		DDPF_ALPHAPIXELS = 0x1, // Texture contains alpha data; dwRGBAlphaBitMask contains valid data.
		DDPF_ALPHA = 0x2, // Used in some older DDS files for alpha channel only uncompressed data (dwRGBBitCount contains the alpha channel bitcount; dwABitMask contains valid data)
		DDPF_FOURCC = 0x4, // Texture contains compressed RGB data; dwFourCC contains valid data.
		DDPF_RGB = 0x40, // Texture contains uncompressed RGB data; dwRGBBitCountand the RGB masks(dwRBitMask, dwGBitMask, dwBBitMask) contain valid data.
	}

	//Whether or not to actually load textures, or just use default material
	private bool DebugLoadFiles = true;
	
	//This turns every shader into the shader specified by sh if true.
	private static bool shOverride = true;
	private static Shader sh = Shader.Find("Silent/FF14 World NonBlend");
	
	//TODO: Refactor to use model ids at some point if needed
	//Materials are registered in the object importer which doesn't see IDs
	private Dictionary<String, String> _meshMaterialDictionary = new Dictionary<String, String>();
	private Dictionary<String, Material> _materialDictionary = new Dictionary<String, Material>();
	private static readonly MaterialHandler Instance = new MaterialHandler();

	private Shader standard;
	private Shader nonBlend;
	private Shader texBlend;
	private Shader cutout;
	private Shader cutoutWind;

	private MaterialHandler()
	{
		LoadShaders();
		
		//Always have a backup plan
		LoadDefaultMaterial();
	}

	private void LoadShaders()
	{
		if (shOverride)
		{
			standard = sh;
			nonBlend = sh;
			texBlend = sh;
			cutout = sh;
			cutoutWind = sh;
		}
		else
		{
			standard = Shader.Find("Standard");
			nonBlend = Shader.Find("Silent/FF14 World NonBlend");
			texBlend = Shader.Find("Silent/FF14 World TexBlend");
			cutout = Shader.Find("Silent/FF14 World Cutout");
			cutoutWind = Shader.Find("Silent/FF14 World Cutout (Multisampling + Wind)");	
		}
	}

	public static MaterialHandler GetInstance()
	{
		return Instance;
	}

	private void LoadDefaultMaterial()
	{
		Material thisMaterial = new Material(standard);
		thisMaterial.name = "DefaultMaterial";
		Texture2D tex = new Texture2D(1, 1);
		thisMaterial.SetTexture("_MainTex", tex);

		_materialDictionary.Add("default", thisMaterial);
		RegisterMaterialForMesh("default", "default");
	}
	
	public Boolean LoadMaterial(String materialPath)
	{
		if (!File.Exists(materialPath))
			return false;

		StreamReader stream = File.OpenText(materialPath);
		String mtlText = stream.ReadToEnd();
		stream.Close();

		Material thisMaterial = new Material(nonBlend);
        bool emissive = false;

		using (StringReader str = new StringReader(mtlText))
		{
			String thisLine = str.ReadLine();

			while (thisLine != null)
			{
				thisLine = thisLine.Trim();
				String[] splitLine = thisLine.Split(' ');

				switch (splitLine[0].Trim())
				{
					case "newmtl":
						if (!String.IsNullOrEmpty(splitLine[1]))
							thisMaterial.name = splitLine[1];
							Material mat;
							if (_materialDictionary.TryGetValue(thisMaterial.name, out mat))
								return true;
						break;
					case "map_Kd":
						{
							//TODO: fix alpha/cutout
								
							Texture existingTexture = thisMaterial.GetTexture("_Albedo0");
							Texture2D tex;
							
							bool hasAlphaInDiffuse;
							bool isDummy = splitLine[1].Contains("dummy");

							if (isDummy)
							{
								if (existingTexture != null)
									tex = existingTexture as Texture2D;
								else
									tex = Texture2D.whiteTexture;
							}
							else
								tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(), splitLine[1].Trim()), out emissive);

							if (tex.alphaIsTransparency)
								thisMaterial.shader = cutout;
								
							if (tex != null && existingTexture == null)
								thisMaterial.SetTexture("_Albedo0", tex);
							else if (tex != null)
								thisMaterial.SetTexture("_Albedo1", tex);
						}
						break;
					case "bump":
						{
							Texture existingTexture = thisMaterial.GetTexture("_NormalMap0");
							Texture2D tex;
							bool isDummy = splitLine[1].Contains("dummy");

							if (isDummy)
							{
								if (existingTexture != null)
									tex = existingTexture as Texture2D;
								else
									tex = null;
                            }
							else
								tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(), splitLine[1].Trim()), out emissive);

                            if (tex != null && tex.alphaIsTransparency)
                            {
                                thisMaterial.EnableKeyword("_EmissionPow");
                                thisMaterial.SetFloat("_EmissionPow", 2.25f);
                            }

                            //tex.alphaIsTransparency = true;
                            //SetNormalMap(ref tex);

                            if (tex != null && existingTexture == null)
								thisMaterial.SetTexture("_NormalMap0", tex);
							else if (tex != null)
								thisMaterial.SetTexture("_NormalMap1", tex);
						}
						break;
					case "map_Ks":
						{
							Texture existingTexture = thisMaterial.GetTexture("_Metallic0");
							Texture2D tex;
							bool isDummy = splitLine[1].Contains("dummy");

							if (isDummy)
							{
								if (existingTexture != null)
									tex = existingTexture as Texture2D;
								else
									tex = Texture2D.blackTexture;
							}
							else
								tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(), splitLine[1].Trim()), out emissive);

                            if (emissive)
                            {
                                thisMaterial.EnableKeyword("_EmissionPow");
                                thisMaterial.SetFloat("_EmissionPow", 2.25f);
                            }
							//tex.alphaIsTransparency = true;
							if (tex != null && existingTexture == null)
								thisMaterial.SetTexture("_Metallic0", tex);
							else if (tex != null)
								thisMaterial.SetTexture("_Metallic1", tex);
						}
						break;
//					case "map_Ka":
//					{
//						Texture2D tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(),
//							splitLine[1].Trim()));
//	
//						Texture existingTexture = thisMaterial.GetTexture("_EmissionMap");
//						if (tex != null && existingTexture == null)
//							thisMaterial.SetTexture("_EmissionMap", tex);
//						break;
//					}
				}
				thisLine = str.ReadLine();
			}
		}

		//		if (!DebugLoadFiles)
		//			Debug.LogFormat("Added material named {0} with shader {1}", thisMaterial.name, thisMaterial.shader.name);
		//if (!_materialDictionary.ContainsKey(thisMaterial.name))
		thisMaterial.enableInstancing = true;
			_materialDictionary.Add(thisMaterial.name, thisMaterial);
        //AssetDatabase.CreateAsset(thisMaterial, thisMaterial.name + ".asset");
        return true;
	}
	
	private Texture2D SetNormalMap2(Texture2D tex) {
		Color[] colors = tex.GetPixels();
		for(int i=0; i<colors.Length;i++) {
			Color c = colors[i];
			c.r = c.a*2-1;  //red<-alpha (x<-w)
			c.g = c.g*2-1; //green is always the same (y)
			Vector2 xy = new Vector2(c.r, c.g); //this is the xy vector
			c.b = Mathf.Sqrt(1-Mathf.Clamp01(Vector2.Dot(xy, xy))); //recalculate the blue channel (z)
			colors[i] = new Color(c.r*0.5f+0.5f, c.g*0.5f+0.5f, c.b*0.5f+0.5f); //back to 0-1 range
		}
		tex.SetPixels(colors); //apply pixels to the texture
		tex.Apply();
		return tex;
	}
	
	private static void SetNormalMap1(ref Texture2D tex)
	{
		Color[] pixels = tex.GetPixels();
		for(int i=0; i < pixels.Length; i++)
		{
			Color temp = pixels[i];
			temp.a = pixels[i].r;
			temp.g = pixels[i].g;
			temp.r = 0;
			temp.b = 0;
			pixels[i] = temp;
		}
		tex.SetPixels(pixels);
		tex.Apply();
	}
	
	public static void SetNormalMap(ref Texture2D tex)
	{
		Color[] pixels = tex.GetPixels();
		for(int i=0; i < pixels.Length; i++)
		{
			Color temp = pixels[i];
			temp.r = pixels[i].g;
			temp.a = pixels[i].r;
			pixels[i] = temp;
		}
		tex.SetPixels(pixels);
		tex.Apply();
	}

	// https://stackoverflow.com/a/2887
	public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
	{
		T stuff;
		GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		try
		{
			stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
		}
		finally
		{
			handle.Free();
		}
		return stuff;
	}
	
	public static Texture2D LoadTextureDXT(byte[] ddsBytes)
    {
        TextureFormat textureFormat;
        byte ddsSizeCheck = ddsBytes[4];
        if (ddsSizeCheck != 124)
            throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files


        if (ddsBytes[0x57] == '1')
            textureFormat = TextureFormat.DXT1;
        else
            textureFormat = TextureFormat.DXT5;

        int DDS_HEADER_SIZE = 128;

		DDS_HEADER header = ByteArrayToStructure<DDS_HEADER>(ddsBytes);

		int height = (int)header.dwHeight;
		int width = (int)header.dwWidth;
		Texture2D texture = new Texture2D(width, height, textureFormat, true);

		byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
        Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

		texture.LoadRawTextureData(dxtBytes);
		texture.Apply(true);
        return (texture);
    }

    private Texture2D LoadTexture(String texPath)
	{
		Texture2D texture = null;
 
		if (!File.Exists(texPath))
			return texture;
 
		texture = new Texture2D(1, 1);
		
        if (texPath.Contains("dds"))
        {
            texture = LoadTextureDXT(File.ReadAllBytes(texPath));
            texture.alphaIsTransparency = true;
            return texture;
        }
		if (DebugLoadFiles && !texPath.Contains("dummy"))
			texture.LoadImage(File.ReadAllBytes(texPath));

		//texture.alphaIsTransparency = true;
 
		return texture;
	}
	
	private Texture2D LoadTexture(String texPath, out Boolean isEmissive)
	{
		Texture2D texture = null;
		isEmissive = false;
     
		if (!File.Exists(texPath))
			return texture;
     
		texture = new Texture2D(1, 1);

        if (texPath.Contains("dds"))
        {
            texture = LoadTextureDXT(File.ReadAllBytes(texPath));
            texture.alphaIsTransparency = true;
        }
        else if (!texPath.Contains("dummy"))
			texture.LoadImage(File.ReadAllBytes(texPath));

		Color32[] alphaCheck = texture.GetPixels32();

        bool hasAlpha = false;
        //if (texPath.Contains("_s."))
        {
			if (texPath.Contains("_a.") || texPath.Contains("_s."))
			{
				foreach (Color32 c in alphaCheck)
				{
					if (c.a != 0xFF)
					{
						hasAlpha = true;
						break;
					}
				}
			}
			//if (hasAlpha)
			if (texPath.Contains("_s."))
			{
				foreach (Color32 c in alphaCheck)
                {
                    if ((c.r == 255 && c.g == 95 && c.b == 65) || (c.r > 100 && c.a != 0xFF))
                    {
                        isEmissive = true;
                        break;
                    }
                }
            }
        }
		texture.alphaIsTransparency = hasAlpha;
     
		return texture;
	}
	
	//May not be needed?
//	public Material GetMaterial(String materialName)
//	{
//		Material material;
//		_materialDictionary.TryGetValue(materialName, out material))
//		
//		return material;
//	}

	public Boolean SetMaterialShader(String meshName, String shader)
	{
		String strMaterial;
		Material material = null;
		
		if (_meshMaterialDictionary.TryGetValue(meshName, out strMaterial))
			_materialDictionary.TryGetValue(strMaterial, out material);

		if (material == null)
			return false;

		//If we changed it from default, don't change it afterwards!
		if (material.shader != nonBlend)
			return false;
		
		switch (shader.ToLower())
		{
			case "standard":
				material.shader = standard;
				break;
			case "nonblend":
				material.shader = nonBlend;
				break;			
			case "texblend":
				material.shader = texBlend;
				break;			
			case "cutout":
				material.shader = cutout;
				break;			
			case "cutoutwind":
				material.shader = cutoutWind;
				break;
		}
		
		return material;
	}

	public void RegisterMaterialForMesh(String meshName, String materialName)
	{
		Material material;
		string dictMaterialName;
		
		if (_meshMaterialDictionary.TryGetValue(meshName, out dictMaterialName))
			return;
		
		if (!_materialDictionary.TryGetValue(materialName, out material))
			return;

		_meshMaterialDictionary.Add(meshName, materialName);
	}

	public Material GetMaterialForMesh(String meshName)
	{
		String strMaterial;
		Material material = null;
		
		if (_meshMaterialDictionary.TryGetValue(meshName, out strMaterial))
			_materialDictionary.TryGetValue(strMaterial, out material);

		if (material == null)
		{
			Debug.LogFormat("No material found for mesh {0}.", meshName);
		}
		
		return material;
	}
}

//Standard shader code, for old reference
	/*
	 * TODO: Place a material, or multiple materials using these shaders into Resources in Unity?
	 * https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
	 * The Shaders won't be compiled into the executable if they are not encountered somewhere
	 * in the scene and only with code
	 *
	 * This texture setup is awkward.
	 * It looks best with Standard shader if you apply the first diffuse map
	 * on _MainTex, apply the first and second normal maps on _BumpMap and _DetailNormalMap,
	 * and first specular on _SpecGlossMap
	 */
//	public Boolean LoadMaterial(String materialPath)
//	{
//		if (!File.Exists(materialPath))
//			return false;
//
//		StreamReader stream = File.OpenText(materialPath);
//		String mtlText = stream.ReadToEnd();
//		stream.Close();
//
//		//Standard shader (For now)
//		Shader sh = Shader.Find("Standard (Specular setup)");
//		Material thisMaterial = new Material(sh);
//
//		//Enable shader features
//		thisMaterial.EnableKeyword("_ALPHATEST_ON");
//
//		using (StringReader str = new StringReader(mtlText))
//		{
//			String thisLine = str.ReadLine();
//
//			while (thisLine != null)
//			{
//				thisLine = thisLine.Trim();
//				String[] splitLine = thisLine.Split(' ');
//
//				switch (splitLine[0].Trim())
//				{
//					case "newmtl":
//						if (!String.IsNullOrEmpty(splitLine[1]))
//							thisMaterial.name = splitLine[1];
//							Material mat;
//							if (_materialDictionary.TryGetValue(thisMaterial.name, out mat))
//								return true;
//						break;
//					case "map_Kd":
//						{
//							//braces because what really is C#
//							Texture2D tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(),
//								splitLine[1].Trim()));
//	
//							Texture existingTexture = thisMaterial.GetTexture("_MainTex");
//							if (tex != null && existingTexture == null)
//							{
//								thisMaterial.SetTexture("_MainTex", tex);
//							}
//						}
//						break;
//					case "bump":
//						{
//							Texture2D tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(),
//								splitLine[1].Trim()));
//
//							//SetNormalMap(ref tex);
//							
//							Texture existingTexture = thisMaterial.GetTexture("_BumpMap");
//							if (tex != null && existingTexture == null)
//							{
//								thisMaterial.EnableKeyword("_NORMALMAP");
//								thisMaterial.SetTexture("_BumpMap", tex);
//								
//							}
//							else if (tex != null)
//							{
//								thisMaterial.EnableKeyword("_DETAIL_MULX2");
//								thisMaterial.SetTexture("_DetailNormalMap", tex);
//							}
//						}
//						break;
//					case "map_Ks":
//						{
//							Texture2D tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(),
//								splitLine[1].Trim()));
//	
//							Texture existingTexture = thisMaterial.GetTexture("_SpecGlossMap");
//							if (tex != null && existingTexture == null)
//							{
//								thisMaterial.SetTexture("_SpecGlossMap", tex);
//								thisMaterial.SetFloat("_GlossMapScale", 0);
//							}
//						}
//						break;
//					case "map_Ka":
//					{
//						Texture2D tex = LoadTexture(Path.Combine(Directory.GetParent(materialPath).ToString(),
//							splitLine[1].Trim()));
//	
//						Texture existingTexture = thisMaterial.GetTexture("_EmissionMap");
//						if (tex != null && existingTexture == null)
//							thisMaterial.SetTexture("_EmissionMap", tex);
//						break;
//					}
//				}
//				thisLine = str.ReadLine();
//			}
//		}
//
//		if (debug)
//			Debug.LogFormat("Added material named {0}", thisMaterial.name);
//		_materialDictionary.Add(thisMaterial.name, thisMaterial);
//		return true;
//	}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// Token: 0x02000343 RID: 835
[ExecuteInEditMode]
public class LightVolume : MonoBehaviour
{
	// Token: 0x06001B70 RID: 7024 RVA: 0x00075530 File Offset: 0x00073730
	private void SetShaderVars()
	{
		Shader.SetGlobalFloat("brightness", this.brightness);
		Shader.SetGlobalFloat("ambienceStrength", this.ambienceStrength);
		Shader.SetGlobalFloat("ambienceMin", this.ambienceMin);
		Shader.SetGlobalVector("gridRes", this.gridRes);
		Shader.SetGlobalFloat("raySpacing", this.raySpacing);
		Shader.SetGlobalVector("gridOffset", this.gridOffset);
	}

	// Token: 0x06001B71 RID: 7025 RVA: 0x000755AC File Offset: 0x000737AC
	public void SetSize()
	{
		Shader.SetGlobalTexture("_LightMap", null);
		GameObject gameObject = (this.sceneParent == null) ? base.gameObject : this.sceneParent;
		Bounds totalBounds = this.GetTotalBounds(gameObject);
		this.gridOffset = totalBounds.center;
		this.gridRes = new Vector3Int(Mathf.CeilToInt((totalBounds.size.x + 3f) / this.raySpacing), Mathf.CeilToInt((totalBounds.size.y + 3f) / this.raySpacing), Mathf.CeilToInt((totalBounds.size.z + 3f) / this.raySpacing));
	}

	// Token: 0x06001B72 RID: 7026 RVA: 0x0007565C File Offset: 0x0007385C
	private Bounds GetTotalBounds(GameObject gameObject)
	{
		Bounds result = default(Bounds);
		bool flag = true;
		foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			if (flag)
			{
				result = meshRenderer.bounds;
			}
			else
			{
				result.Encapsulate(meshRenderer.bounds);
			}
			flag = false;
		}
		Vector3 vector = new Vector3(result.center.x, result.min.y, result.center.z);
		Vector3 vector2 = Vector3.Scale(result.size, this.boundsSizeMultiplier);
		vector2 = Vector3.Max(vector2, Vector3.one * 0.01f);
		Vector3 center = new Vector3(vector.x, vector.y + vector2.y * 0.5f, vector.z) + this.boundsCenterOffset;
		result.size = vector2;
		result.center = center;
		return result;
	}

	// Token: 0x06001B73 RID: 7027 RVA: 0x0007574C File Offset: 0x0007394C
	private void OnDrawGizmosSelected()
	{
		if (!this.showVolumeGizmos)
		{
			return;
		}
		Gizmos.color = Color.black;
		Gizmos.DrawWireCube(this.gridOffset - Vector3.one * 0.25f, this.gridRes * this.raySpacing);
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(this.gridOffset + Vector3.one * 0.25f, this.gridRes * this.raySpacing);
	}

	// Token: 0x06001B74 RID: 7028 RVA: 0x000757E0 File Offset: 0x000739E0
	private void Awake()
	{
		LightVolume.instance = this;
	}

	// Token: 0x06001B75 RID: 7029 RVA: 0x000757E8 File Offset: 0x000739E8
	private void Start()
	{
		this.SetShaderVars();
		Shader.SetGlobalTexture("_LightMap", this.lightMap);
	}

	// Token: 0x17000283 RID: 643
	// (get) Token: 0x06001B76 RID: 7030 RVA: 0x00075800 File Offset: 0x00073A00
	private bool RaytracingShaderNotSupported
	{
		get
		{
			return !SystemInfo.supportsRayTracingShaders;
		}
	}

	// Token: 0x06001B77 RID: 7031 RVA: 0x0007580C File Offset: 0x00073A0C
	public void Bake()
	{
		if (!this.computeShader)
		{
			Debug.LogError("Cannot bake at runtime (serialize the ComputeShader if you want to do this)");
			return;
		}
		if (!this.rayTracingShader)
		{
			Debug.LogError("Cannot bake at runtime (serialize the RayTracingShader if you want to do this)");
			return;
		}
		this.SetSize();
		RenderTexture inputTex = this.RunBake();
		RenderTexture renderTexture = this.RunBlur(inputTex);
		renderTexture.name = "LightVolumeRenderTexture";
		this.SetShaderVars();
		Shader.SetGlobalTexture("_LightMap", renderTexture);
		base.StartCoroutine(this.SaveTex(renderTexture));
	}

	// Token: 0x06001B78 RID: 7032 RVA: 0x00075888 File Offset: 0x00073A88
	private RenderTexture RunBake()
	{
		this.rayTracingShader.SetVector("gridRadius", new Vector3((float)this.gridRes.x, (float)this.gridRes.y, (float)this.gridRes.z) * (this.raySpacing / 2f));
		this.rayTracingShader.SetVector("gridOffset", this.gridOffset);
		this.rayTracingShader.SetVector("skyColor", this.skyColor);
		this.rayTracingShader.SetInt("rayCount", this.rayCount);
		ComputeBuffer computeBuffer;
		int num = this.BuildLights(out computeBuffer);
		IDisposable disposable;
		this.BuildMeshes(out disposable, num);
		RenderTexture renderTexture = LightVolume.Create3DTexture(FilterMode.Bilinear, RenderTextureFormat.ARGBHalf, this.gridRes);
		this.rayTracingShader.SetTexture("lightMap", renderTexture);
		for (int i = 0; i < num + 1; i++)
		{
			this.rayTracingShader.SetInt("doLightIndex", i);
			this.rayTracingShader.Dispatch("RaygenShader", this.gridRes.x, this.gridRes.y, this.gridRes.z, null);
		}
		computeBuffer.Dispose();
		disposable.Dispose();
		return renderTexture;
	}

	// Token: 0x06001B79 RID: 7033 RVA: 0x000759C4 File Offset: 0x00073BC4
	private static RenderTexture Create3DTexture(FilterMode filterMode, RenderTextureFormat format, Vector3Int resolution)
	{
		RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, 0);
		renderTexture.enableRandomWrite = true;
		renderTexture.format = format;
		renderTexture.dimension = TextureDimension.Tex3D;
		renderTexture.volumeDepth = resolution.z;
		renderTexture.wrapMode = TextureWrapMode.Clamp;
		renderTexture.filterMode = filterMode;
		renderTexture.hideFlags = HideFlags.DontSave;
		if (!renderTexture.Create())
		{
			throw new Exception("Failed to create texture");
		}
		return renderTexture;
	}

	// Token: 0x06001B7A RID: 7034 RVA: 0x00075A30 File Offset: 0x00073C30
	private int BuildLights(out ComputeBuffer toDispose)
	{
		List<LightVolume.GpuLight> list = new List<LightVolume.GpuLight>();
		GameObject gameObject = (this.sceneParent == null) ? base.gameObject : this.sceneParent;
		if (this.allLightsFound == null)
		{
			this.allLightsFound = new List<BakedVolumeLight>();
		}
		this.allLightsFound.Clear();
		foreach (BakedVolumeLight bakedVolumeLight in gameObject.GetComponentsInChildren<BakedVolumeLight>())
		{
			this.allLightsFound.Add(bakedVolumeLight);
			Vector3 a = new Vector3(bakedVolumeLight.color.r, bakedVolumeLight.color.g, bakedVolumeLight.color.b);
			BakedVolumeLight.LightModes mode = bakedVolumeLight.mode;
			float num;
			if (mode != BakedVolumeLight.LightModes.Point)
			{
				if (mode != BakedVolumeLight.LightModes.Spot)
				{
					throw new Exception();
				}
				num = bakedVolumeLight.coneSize * 0.017453292f;
			}
			else
			{
				num = 0f;
			}
			float coneSize = num;
			list.Add(new LightVolume.GpuLight
			{
				Position = bakedVolumeLight.transform.position,
				ConeSize = coneSize,
				Direction = bakedVolumeLight.transform.forward,
				Radius = bakedVolumeLight.radius,
				Color = a * bakedVolumeLight.intensity,
				Falloff = bakedVolumeLight.falloff,
				ConeFalloff = bakedVolumeLight.coneFalloff
			});
		}
		int count = list.Count;
		if (count == 0)
		{
			list.Add(default(LightVolume.GpuLight));
		}
		ComputeBuffer computeBuffer = new ComputeBuffer(list.Count, 52);
		computeBuffer.SetData<LightVolume.GpuLight>(list);
		this.rayTracingShader.SetBuffer("lightBuffer", computeBuffer);
		this.rayTracingShader.SetInt("lightBufferLength", count);
		toDispose = computeBuffer;
		return count;
	}

	// Token: 0x06001B7B RID: 7035 RVA: 0x00075BE4 File Offset: 0x00073DE4
	private void BuildMeshes(out IDisposable toDispose, int lightCountForDebug)
	{
		int value = this.occluderMask.value;
		GameObject gameObject = (this.sceneParent == null) ? base.gameObject : this.sceneParent;
		if (this.allMeshRenderersFound == null)
		{
			this.allMeshRenderersFound = new List<MeshRenderer>();
		}
		this.allMeshRenderersFound.Clear();
		RayTracingAccelerationStructure rayTracingAccelerationStructure = new RayTracingAccelerationStructure();
		uint num = 0U;
		int num2 = 0;
		foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>())
		{
			if ((1 << meshRenderer.gameObject.layer & value) != 0)
			{
				this.allMeshRenderersFound.Add(meshRenderer);
				Mesh sharedMesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;
				if (!(sharedMesh == null))
				{
					int subMeshCount = sharedMesh.subMeshCount;
					for (int j = 0; j < subMeshCount; j++)
					{
						num += sharedMesh.GetIndexCount(j);
					}
					num2 += sharedMesh.vertexCount;
					RayTracingSubMeshFlags[] array = new RayTracingSubMeshFlags[subMeshCount];
					for (int k = 0; k < array.Length; k++)
					{
						array[k] = (RayTracingSubMeshFlags.Enabled | RayTracingSubMeshFlags.ClosestHitOnly);
					}
					rayTracingAccelerationStructure.AddInstance(meshRenderer, array, true, false, 255U, uint.MaxValue);
				}
			}
		}
		rayTracingAccelerationStructure.Build();
		Debug.Log(string.Format("Light Volume Baker found: {0} lights, {1} meshes, {2} indices, {3} vertices", new object[]
		{
			lightCountForDebug,
			this.allMeshRenderersFound.Count,
			num,
			num2
		}));
		this.rayTracingShader.SetAccelerationStructure("g_SceneAccelStruct", rayTracingAccelerationStructure);
		toDispose = rayTracingAccelerationStructure;
	}

	// Token: 0x06001B7C RID: 7036 RVA: 0x00075D68 File Offset: 0x00073F68
	private RenderTexture RunBlur(RenderTexture inputTex)
	{
		if (this.blurRadius <= 0)
		{
			return inputTex;
		}
		this.computeShader.SetInt("blurRadius", this.blurRadius);
		Vector3Int resolution = new Vector3Int(inputTex.width, inputTex.height, inputTex.volumeDepth);
		RenderTexture renderTexture = LightVolume.Create3DTexture(inputTex.filterMode, inputTex.format, resolution);
		for (int i = 0; i < 3; i++)
		{
			this.computeShader.SetTexture(1, "blurInputLightMap", inputTex);
			this.computeShader.SetTexture(1, "lightMap", renderTexture);
			this.computeShader.SetInt("blurAxis", i);
			uint num = 4U;
			uint num2 = 4U;
			uint num3 = 4U;
			long num4 = ((long)resolution.x + (long)((ulong)num) - 1L) / (long)((ulong)num);
			long num5 = ((long)resolution.y + (long)((ulong)num2) - 1L) / (long)((ulong)num2);
			long num6 = ((long)resolution.z + (long)((ulong)num3) - 1L) / (long)((ulong)num3);
			this.computeShader.Dispatch(1, (int)num4, (int)num5, (int)num6);
			RenderTexture renderTexture2 = inputTex;
			RenderTexture renderTexture3 = renderTexture;
			renderTexture = renderTexture2;
			inputTex = renderTexture3;
		}
		Object.DestroyImmediate(renderTexture);
		return inputTex;
	}

	// Token: 0x06001B7D RID: 7037 RVA: 0x00075E73 File Offset: 0x00074073
	private IEnumerator SaveTex(RenderTexture renderTexture)
	{
		AsyncGPUReadbackRequest downloadedData = AsyncGPUReadback.Request(renderTexture, 0, null);
		while (!downloadedData.done)
		{
			yield return null;
		}
		if (downloadedData.hasError)
		{
			Debug.LogError("AsyncGPUReadback error while baking light volume texture.");
			Object.DestroyImmediate(renderTexture);
			yield break;
		}
		byte[] array = new byte[downloadedData.layerDataSize * downloadedData.layerCount];
		for (int i = 0; i < downloadedData.layerCount; i++)
		{
			NativeArray<byte>.Copy(downloadedData.GetData<byte>(i), 0, array, i * downloadedData.layerDataSize, downloadedData.layerDataSize);
		}
		if (!this.lightMap || this.lightMap.width != renderTexture.width || this.lightMap.height != renderTexture.height || this.lightMap.depth != renderTexture.volumeDepth || this.lightMap.graphicsFormat != renderTexture.graphicsFormat)
		{
			if (this.lightMap)
			{
				Object.DestroyImmediate(this.lightMap);
			}
			this.lightMap = new Texture3D(renderTexture.width, renderTexture.height, renderTexture.volumeDepth, renderTexture.graphicsFormat, TextureCreationFlags.None);
		}
		this.lightMap.name = this.fileName;
		this.lightMap.wrapMode = renderTexture.wrapMode;
		this.lightMap.filterMode = renderTexture.filterMode;
		this.lightMap.SetPixelData<byte>(array, 0, 0);
		this.lightMap.Apply();
		Shader.SetGlobalTexture("_LightMap", this.lightMap);
		Object.DestroyImmediate(renderTexture);
		yield break;
	}

	// Token: 0x06001B7E RID: 7038 RVA: 0x00075E89 File Offset: 0x00074089
	public static LightVolume Instance()
	{
		if (LightVolume.instance == null)
		{
			LightVolume.instance = Object.FindAnyObjectByType<LightVolume>();
		}
		return LightVolume.instance;
	}

	// Token: 0x06001B7F RID: 7039 RVA: 0x00075EA8 File Offset: 0x000740A8
	internal Color SamplePosition(Vector3 worldPos)
	{
		worldPos -= this.gridOffset;
		worldPos += this.raySpacing * this.gridRes * 0.5f;
		worldPos.x /= this.raySpacing;
		worldPos.y /= this.raySpacing;
		worldPos.z /= this.raySpacing;
		return this.lightMap.GetPixel((int)worldPos.x, (int)worldPos.y, (int)worldPos.z);
	}

	// Token: 0x06001B80 RID: 7040 RVA: 0x00075F40 File Offset: 0x00074140
	public float SamplePositionAlpha(Vector3 worldPos)
	{
		worldPos -= this.gridOffset;
		worldPos += this.raySpacing * this.gridRes * 0.5f;
		worldPos.x /= this.raySpacing;
		worldPos.y /= this.raySpacing;
		worldPos.z /= this.raySpacing;
		return this.lightMap.GetPixel((int)worldPos.x, (int)worldPos.y, (int)worldPos.z).a;
	}

	// Token: 0x04001251 RID: 4689
	public bool showVolumeGizmos = true;

	// Token: 0x04001252 RID: 4690
	public float brightness = 1f;

	// Token: 0x04001253 RID: 4691
	public float ambienceStrength = 1f;

	// Token: 0x04001254 RID: 4692
	public float ambienceMin = 0.05f;

	// Token: 0x04001255 RID: 4693
	public Color skyColor = Color.white;

	// Token: 0x04001256 RID: 4694
	public FilterMode filterModeRT = FilterMode.Bilinear;

	// Token: 0x04001257 RID: 4695
	public RenderTextureFormat formatRT = RenderTextureFormat.ARGBFloat;

	// Token: 0x04001258 RID: 4696
	public Vector3Int gridRes;

	// Token: 0x04001259 RID: 4697
	public Vector3 gridOffset;

	// Token: 0x0400125A RID: 4698
	public int rayCount = 128;

	// Token: 0x0400125B RID: 4699
	public float raySpacing = 1.5f;

	// Token: 0x0400125C RID: 4700
	public Vector3 boundsSizeMultiplier = Vector3.one;

	// Token: 0x0400125D RID: 4701
	public Vector3 boundsCenterOffset = Vector3.zero;

	// Token: 0x0400125E RID: 4702
	[Tooltip("Colliders matching this mask will be used for light tracing, colliders not matching will be ignored")]
	public LayerMask occluderMask = -1;

	// Token: 0x0400125F RID: 4703
	[Tooltip("Radius (in texels) for how much to box blur the output texture")]
	public int blurRadius;

	// Token: 0x04001260 RID: 4704
	public GameObject sceneParent;

	// Token: 0x04001261 RID: 4705
	public ComputeShader computeShader;

	// Token: 0x04001262 RID: 4706
	public RayTracingShader rayTracingShader;

	// Token: 0x04001263 RID: 4707
	public Texture3D lightMap;

	// Token: 0x04001264 RID: 4708
	public string fileName = "LightVolumeBakeTexture";

	// Token: 0x04001265 RID: 4709
	public List<BakedVolumeLight> allLightsFound;

	// Token: 0x04001266 RID: 4710
	public List<MeshRenderer> allMeshRenderersFound;

	// Token: 0x04001267 RID: 4711
	internal static LightVolume instance;

	// Token: 0x02000344 RID: 836
	private struct GpuLight
	{
		// Token: 0x04001268 RID: 4712
		public Vector3 Position;

		// Token: 0x04001269 RID: 4713
		public float ConeSize;

		// Token: 0x0400126A RID: 4714
		public Vector3 Direction;

		// Token: 0x0400126B RID: 4715
		public float Radius;

		// Token: 0x0400126C RID: 4716
		public Vector3 Color;

		// Token: 0x0400126D RID: 4717
		public float Falloff;

		// Token: 0x0400126E RID: 4718
		public float ConeFalloff;
	}
}

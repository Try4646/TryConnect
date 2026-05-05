using System;
using UnityEngine;

// Token: 0x02000023 RID: 35
public class CameraDoRender : MonoBehaviour
{
	// Token: 0x0600008C RID: 140 RVA: 0x00004ED0 File Offset: 0x000030D0
	private void Awake()
	{
		Camera camera;
		base.TryGetComponent<Camera>(out camera);
		camera.Render();
	}
}

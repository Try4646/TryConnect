using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

// Token: 0x02000305 RID: 773
[RequireComponent(typeof(VideoPlayer))]
public class StreamingAssetVideoPlayer : MonoBehaviour
{
	// Token: 0x06001A77 RID: 6775 RVA: 0x0006F968 File Offset: 0x0006DB68
	private void Awake()
	{
		VideoPlayer component = base.GetComponent<VideoPlayer>();
		string text = Path.Combine(Application.streamingAssetsPath, this.relativeFolder, this.fileNameWithoutExtension + ".webm");
		string text2 = Path.Combine(Application.streamingAssetsPath, this.relativeFolder, this.fileNameWithoutExtension + ".mp4");
		component.source = VideoSource.Url;
		component.clip = null;
		if (Application.platform == RuntimePlatform.LinuxPlayer && File.Exists(text))
		{
			component.url = text;
			return;
		}
		if (File.Exists(text))
		{
			component.url = text;
			return;
		}
		if (File.Exists(text2))
		{
			component.url = text2;
		}
	}

	// Token: 0x0400111D RID: 4381
	[SerializeField]
	private string fileNameWithoutExtension;

	// Token: 0x0400111E RID: 4382
	[SerializeField]
	private string relativeFolder = "ScreensVideos";
}

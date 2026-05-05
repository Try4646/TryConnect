using System;
using System.Collections;
using Dissonance;
using Dissonance.Integrations.MirrorIgnorance;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200020B RID: 523
[RequireComponent(typeof(Image))]
public class PlayerVoiceSpriteIndicator : MonoBehaviour
{
	// Token: 0x0600135C RID: 4956 RVA: 0x000539A1 File Offset: 0x00051BA1
	private void Awake()
	{
		this._image = base.GetComponent<Image>();
		if (!this.targetPlayer)
		{
			this.targetPlayer = base.GetComponentInParent<MirrorIgnorancePlayer>();
		}
	}

	// Token: 0x0600135D RID: 4957 RVA: 0x000539C8 File Offset: 0x00051BC8
	private void Start()
	{
		base.StartCoroutine(this.Initialize());
	}

	// Token: 0x0600135E RID: 4958 RVA: 0x000539D7 File Offset: 0x00051BD7
	private IEnumerator Initialize()
	{
		while (!this._dissonanceComms)
		{
			this._dissonanceComms = DissonanceComms.GetSingleton();
			yield return null;
		}
		while (!this.targetPlayer || string.IsNullOrEmpty(this.targetPlayer.PlayerId))
		{
			yield return null;
		}
		while (this._voicePlayerState == null)
		{
			this._voicePlayerState = this._dissonanceComms.FindPlayer(this.targetPlayer.PlayerId);
			yield return null;
		}
		yield break;
	}

	// Token: 0x0600135F RID: 4959 RVA: 0x000539E8 File Offset: 0x00051BE8
	private void Update()
	{
		Sprite sprite = (this._voicePlayerState != null && this._voicePlayerState.IsSpeaking) ? this.speakingSprite : this.silentSprite;
		if (sprite && sprite != this._currentSprite)
		{
			this._currentSprite = sprite;
			this._image.sprite = sprite;
		}
	}

	// Token: 0x04000C56 RID: 3158
	[SerializeField]
	private MirrorIgnorancePlayer targetPlayer;

	// Token: 0x04000C57 RID: 3159
	[SerializeField]
	private Sprite speakingSprite;

	// Token: 0x04000C58 RID: 3160
	[SerializeField]
	private Sprite silentSprite;

	// Token: 0x04000C59 RID: 3161
	private Image _image;

	// Token: 0x04000C5A RID: 3162
	private DissonanceComms _dissonanceComms;

	// Token: 0x04000C5B RID: 3163
	private VoicePlayerState _voicePlayerState;

	// Token: 0x04000C5C RID: 3164
	private Sprite _currentSprite;
}

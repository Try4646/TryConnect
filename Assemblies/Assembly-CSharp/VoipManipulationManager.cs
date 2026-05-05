using System;
using System.Collections.Generic;
using Dissonance;
using UnityEngine;

// Token: 0x02000289 RID: 649
public class VoipManipulationManager : MonoBehaviour
{
	// Token: 0x1400001C RID: 28
	// (add) Token: 0x06001721 RID: 5921 RVA: 0x000622B4 File Offset: 0x000604B4
	// (remove) Token: 0x06001722 RID: 5922 RVA: 0x000622EC File Offset: 0x000604EC
	public event Action OnDesiredVoipFXChanged;

	// Token: 0x1400001D RID: 29
	// (add) Token: 0x06001723 RID: 5923 RVA: 0x00062324 File Offset: 0x00060524
	// (remove) Token: 0x06001724 RID: 5924 RVA: 0x0006235C File Offset: 0x0006055C
	public event Action OnDesiredVoipBusChanged;

	// Token: 0x1400001E RID: 30
	// (add) Token: 0x06001725 RID: 5925 RVA: 0x00062394 File Offset: 0x00060594
	// (remove) Token: 0x06001726 RID: 5926 RVA: 0x000623CC File Offset: 0x000605CC
	public event Action OnMouthFXChanged;

	// Token: 0x06001727 RID: 5927 RVA: 0x00062401 File Offset: 0x00060601
	private void OnValidate()
	{
		Action onDesiredVoipFXChanged = this.OnDesiredVoipFXChanged;
		if (onDesiredVoipFXChanged != null)
		{
			onDesiredVoipFXChanged();
		}
		Action onDesiredVoipBusChanged = this.OnDesiredVoipBusChanged;
		if (onDesiredVoipBusChanged == null)
		{
			return;
		}
		onDesiredVoipBusChanged();
	}

	// Token: 0x06001728 RID: 5928 RVA: 0x00062424 File Offset: 0x00060624
	private void OnEnable()
	{
		this.dissonanceComms.OnPlayerJoinedSession += this.AddPlayer;
		this.dissonanceComms.OnPlayerLeftSession += this.RemovePlayer;
	}

	// Token: 0x06001729 RID: 5929 RVA: 0x00062454 File Offset: 0x00060654
	private void OnDisable()
	{
		this.dissonanceComms.OnPlayerJoinedSession -= this.AddPlayer;
		this.dissonanceComms.OnPlayerLeftSession -= this.RemovePlayer;
	}

	// Token: 0x0600172A RID: 5930 RVA: 0x00062484 File Offset: 0x00060684
	private void AddPlayer(VoicePlayerState playerVoice)
	{
		this._desiredVoipFX.TryAdd(playerVoice.Name, VoipManipulationManager.VoipFX.Default);
		this._mouthFX.TryAdd(playerVoice.Name, false);
		int i = 0;
		while (i < 10)
		{
			if (!this._desiredVoipBus.ContainsValue(i))
			{
				this._desiredVoipBus.TryAdd(playerVoice.Name, i);
				Action onDesiredVoipBusChanged = this.OnDesiredVoipBusChanged;
				if (onDesiredVoipBusChanged == null)
				{
					return;
				}
				onDesiredVoipBusChanged();
				return;
			}
			else
			{
				i++;
			}
		}
	}

	// Token: 0x0600172B RID: 5931 RVA: 0x000624F8 File Offset: 0x000606F8
	private void RemovePlayer(VoicePlayerState playerVoice)
	{
		if (this._desiredVoipFX.ContainsKey(playerVoice.Name))
		{
			this._desiredVoipFX.Remove(playerVoice.Name);
		}
		if (this._desiredVoipBus.ContainsKey(playerVoice.Name))
		{
			this._desiredVoipBus.Remove(playerVoice.Name);
		}
		if (this._mouthFX.ContainsKey(playerVoice.Name))
		{
			this._mouthFX.Remove(playerVoice.Name);
		}
	}

	// Token: 0x0600172C RID: 5932 RVA: 0x00062574 File Offset: 0x00060774
	public void AssignPlayerVoipFX(string playerName, VoipManipulationManager.VoipFX voipFX)
	{
		if (playerName == this.dissonanceComms.LocalPlayerName)
		{
			Debug.Log("VoiceFX on Local Player: " + voipFX.ToString());
			return;
		}
		if (!this._desiredVoipFX.ContainsKey(playerName))
		{
			Debug.LogError("Can't find player in the dict");
			return;
		}
		VoipManipulationManager.VoipFX voipFX2 = this._desiredVoipFX[playerName];
		this._desiredVoipFX[playerName] = voipFX;
		if (voipFX2 != voipFX)
		{
			Action onDesiredVoipFXChanged = this.OnDesiredVoipFXChanged;
			if (onDesiredVoipFXChanged != null)
			{
				onDesiredVoipFXChanged();
			}
			Debug.Log("VoiceFX on " + playerName + ": " + voipFX.ToString());
		}
	}

	// Token: 0x0600172D RID: 5933 RVA: 0x0006261C File Offset: 0x0006081C
	public bool SetPlayerNoMouthFX(string playerName, bool active)
	{
		if (playerName == null)
		{
			return false;
		}
		if (playerName == this.dissonanceComms.LocalPlayerName)
		{
			Debug.Log("Local player mouth FX: " + active.ToString());
			return true;
		}
		if (!this._desiredVoipBus.ContainsKey(playerName))
		{
			return false;
		}
		if (this._mouthFX[playerName] == active)
		{
			return true;
		}
		this._mouthFX[playerName] = active;
		Action onMouthFXChanged = this.OnMouthFXChanged;
		if (onMouthFXChanged != null)
		{
			onMouthFXChanged();
		}
		Debug.Log("Player mouth FX on " + playerName + ": " + active.ToString());
		return true;
	}

	// Token: 0x0600172E RID: 5934 RVA: 0x000626B5 File Offset: 0x000608B5
	public VoipManipulationManager.VoipFX GetDesiredVoipFX(string playerName)
	{
		if (this._desiredVoipFX.ContainsKey(playerName))
		{
			return this._desiredVoipFX[playerName];
		}
		Debug.Log("Player not added to voip fx dict, assigning default bus");
		return VoipManipulationManager.VoipFX.Default;
	}

	// Token: 0x0600172F RID: 5935 RVA: 0x000626E0 File Offset: 0x000608E0
	public int GetDesiredVoipBus(string playerName)
	{
		if (this._desiredVoipBus.ContainsKey(playerName))
		{
			Debug.Log("Player voice bus : " + this._desiredVoipBus[playerName].ToString());
			return this._desiredVoipBus[playerName];
		}
		Debug.Log("Player not added to voip fx dict yet, assigning to 0");
		return 0;
	}

	// Token: 0x06001730 RID: 5936 RVA: 0x00062736 File Offset: 0x00060936
	public int GetDesiredMouthFX(string playerName)
	{
		if (!this._mouthFX.ContainsKey(playerName))
		{
			Debug.Log("Player not added to voip fx dict, assigning default bus");
			return 0;
		}
		if (!this._mouthFX[playerName])
		{
			return 0;
		}
		return 1;
	}

	// Token: 0x04000F06 RID: 3846
	public DissonanceComms dissonanceComms;

	// Token: 0x04000F07 RID: 3847
	private Dictionary<string, VoipManipulationManager.VoipFX> _desiredVoipFX = new Dictionary<string, VoipManipulationManager.VoipFX>();

	// Token: 0x04000F08 RID: 3848
	private Dictionary<string, bool> _mouthFX = new Dictionary<string, bool>();

	// Token: 0x04000F09 RID: 3849
	private Dictionary<string, int> _desiredVoipBus = new Dictionary<string, int>();

	// Token: 0x0200028A RID: 650
	public enum VoipFX
	{
		// Token: 0x04000F0E RID: 3854
		Default,
		// Token: 0x04000F0F RID: 3855
		Wobble,
		// Token: 0x04000F10 RID: 3856
		Low,
		// Token: 0x04000F11 RID: 3857
		High,
		// Token: 0x04000F12 RID: 3858
		Radio
	}
}

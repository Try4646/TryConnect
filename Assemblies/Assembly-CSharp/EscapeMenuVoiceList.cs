using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dissonance;
using Extensions;
using UnityEngine;

// Token: 0x020002A2 RID: 674
public class EscapeMenuVoiceList : MonoBehaviour
{
	// Token: 0x060017DD RID: 6109 RVA: 0x00065103 File Offset: 0x00063303
	private void Awake()
	{
		if (this.dissonanceComms == null)
		{
			this.dissonanceComms = Object.FindFirstObjectByType<DissonanceComms>();
		}
		this._savedVolumes = PlayerVoiceVolumePersistence.Load();
	}

	// Token: 0x060017DE RID: 6110 RVA: 0x0006512C File Offset: 0x0006332C
	private void OnEnable()
	{
		if (this.dissonanceComms != null)
		{
			this.dissonanceComms.OnPlayerJoinedSession += this.OnPlayerJoined;
			this.dissonanceComms.OnPlayerLeftSession += this.OnPlayerLeft;
		}
		this.RefreshList();
		if (this._refreshRetry == null)
		{
			this._refreshRetry = base.StartCoroutine(this.RefreshListRetryRoutine());
		}
	}

	// Token: 0x060017DF RID: 6111 RVA: 0x00065198 File Offset: 0x00063398
	private void OnDisable()
	{
		if (this._refreshRetry != null)
		{
			base.StopCoroutine(this._refreshRetry);
			this._refreshRetry = null;
		}
		if (this.dissonanceComms != null)
		{
			this.dissonanceComms.OnPlayerJoinedSession -= this.OnPlayerJoined;
			this.dissonanceComms.OnPlayerLeftSession -= this.OnPlayerLeft;
		}
	}

	// Token: 0x060017E0 RID: 6112 RVA: 0x000651FC File Offset: 0x000633FC
	private void OnPlayerJoined(VoicePlayerState state)
	{
		this.RefreshList();
	}

	// Token: 0x060017E1 RID: 6113 RVA: 0x000651FC File Offset: 0x000633FC
	private void OnPlayerLeft(VoicePlayerState state)
	{
		this.RefreshList();
	}

	// Token: 0x060017E2 RID: 6114 RVA: 0x00065204 File Offset: 0x00063404
	public void RefreshList()
	{
		if (this.container == null || this.entryPrefab == null || this.dissonanceComms == null)
		{
			return;
		}
		string localPlayerName = this.dissonanceComms.LocalPlayerName;
		if (string.IsNullOrEmpty(localPlayerName))
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, VoiceVolumeEntry> keyValuePair in this._entriesByPlayerId)
		{
			if (this.dissonanceComms.FindPlayer(keyValuePair.Key) == null)
			{
				list.Add(keyValuePair.Key);
			}
		}
		foreach (string key in list)
		{
			VoiceVolumeEntry voiceVolumeEntry;
			if (this._entriesByPlayerId.TryGetValue(key, out voiceVolumeEntry) && voiceVolumeEntry != null)
			{
				Object.Destroy(voiceVolumeEntry.gameObject);
			}
			this._entriesByPlayerId.Remove(key);
		}
		foreach (VoicePlayerState voicePlayerState in this.dissonanceComms.Players)
		{
			if (!(voicePlayerState.Name == localPlayerName))
			{
				string name = voicePlayerState.Name;
				VoiceVolumeEntry voiceVolumeEntry2;
				if (this._entriesByPlayerId.TryGetValue(name, out voiceVolumeEntry2) && voiceVolumeEntry2 != null)
				{
					ValueTuple<string, string, bool> valueTuple = this.ResolveDisplayNameAndSteamId(name);
					if (valueTuple.Item3)
					{
						voiceVolumeEntry2.UpdateIdentity(valueTuple.Item1, valueTuple.Item2);
					}
				}
				else
				{
					ValueTuple<string, string, bool> valueTuple2 = this.ResolveDisplayNameAndSteamId(name);
					string displayName = valueTuple2.Item3 ? valueTuple2.Item1 : "";
					string text = valueTuple2.Item3 ? valueTuple2.Item2 : name;
					float initialVolume = 1f;
					float num;
					if (!string.IsNullOrEmpty(text) && this._savedVolumes.TryGetValue(text, out num))
					{
						initialVolume = num;
					}
					VoiceVolumeEntry component = Object.Instantiate<GameObject>(this.entryPrefab.gameObject, this.container).GetComponent<VoiceVolumeEntry>();
					if (component != null)
					{
						component.Setup(voicePlayerState, displayName, text, initialVolume, new Action<string, float>(this.OnVolumeChanged));
						this._entriesByPlayerId[name] = component;
					}
				}
			}
		}
	}

	// Token: 0x060017E3 RID: 6115 RVA: 0x000654A4 File Offset: 0x000636A4
	private IEnumerator RefreshListRetryRoutine()
	{
		yield return new WaitForSecondsRealtime(0.75f);
		this.RefreshList();
		yield return new WaitForSecondsRealtime(0.75f);
		this.RefreshList();
		this._refreshRetry = null;
		yield break;
	}

	// Token: 0x060017E4 RID: 6116 RVA: 0x000654B4 File Offset: 0x000636B4
	[return: TupleElementNames(new string[]
	{
		"displayName",
		"steamId",
		"resolved"
	})]
	private ValueTuple<string, string, bool> ResolveDisplayNameAndSteamId(string dissonancePlayerId)
	{
		LocalManager instance = MonoSingleton<LocalManager>.Instance;
		if (((instance != null) ? instance.players : null) == null)
		{
			return new ValueTuple<string, string, bool>(dissonancePlayerId, dissonancePlayerId, false);
		}
		foreach (PlayerReferences playerReferences in MonoSingleton<LocalManager>.Instance.players)
		{
			if (!(playerReferences.mirrorIgnorance == null) && !(playerReferences.profile == null) && !(playerReferences.mirrorIgnorance.PlayerId != dissonancePlayerId) && playerReferences.profile.hasSynced)
			{
				string playerName = playerReferences.profile.playerName;
				string item = (playerReferences.profile.steamId != 0UL) ? playerReferences.profile.steamId.ToString() : "";
				return new ValueTuple<string, string, bool>(playerName, item, true);
			}
		}
		return new ValueTuple<string, string, bool>(dissonancePlayerId, dissonancePlayerId, false);
	}

	// Token: 0x060017E5 RID: 6117 RVA: 0x000655A4 File Offset: 0x000637A4
	private void OnVolumeChanged(string steamId, float volume)
	{
		if (string.IsNullOrEmpty(steamId))
		{
			return;
		}
		this._savedVolumes[steamId] = volume;
		PlayerVoiceVolumePersistence.Save(this._savedVolumes);
	}

	// Token: 0x04000F6C RID: 3948
	[SerializeField]
	private DissonanceComms dissonanceComms;

	// Token: 0x04000F6D RID: 3949
	[SerializeField]
	private Transform container;

	// Token: 0x04000F6E RID: 3950
	[SerializeField]
	private VoiceVolumeEntry entryPrefab;

	// Token: 0x04000F6F RID: 3951
	private readonly Dictionary<string, VoiceVolumeEntry> _entriesByPlayerId = new Dictionary<string, VoiceVolumeEntry>();

	// Token: 0x04000F70 RID: 3952
	private Dictionary<string, float> _savedVolumes = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);

	// Token: 0x04000F71 RID: 3953
	private Coroutine _refreshRetry;
}

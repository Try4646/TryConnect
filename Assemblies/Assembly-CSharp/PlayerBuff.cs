using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using Mirror;
using UnityEngine;

// Token: 0x020001F1 RID: 497
public class PlayerBuff : NetworkBehaviour
{
	// Token: 0x060011CE RID: 4558 RVA: 0x0004CE2B File Offset: 0x0004B02B
	private void Awake()
	{
		this._pb = MonoSingleton<LocalManager>.Instance.playerBuffUI;
	}

	// Token: 0x060011CF RID: 4559 RVA: 0x0004CE3D File Offset: 0x0004B03D
	public override void OnStartServer()
	{
		this._buffs[PlayerBuffType.TipsyFortune] = 1f;
		this._buffs[PlayerBuffType.InspiringMelody] = 0f;
		this._buffs[PlayerBuffType.Immunity] = 0f;
	}

	// Token: 0x060011D0 RID: 4560 RVA: 0x0004CE72 File Offset: 0x0004B072
	public override void OnStartClient()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		SyncDictionary<PlayerBuffType, float> buffs = this._buffs;
		buffs.OnChange = (Action<SyncIDictionary<PlayerBuffType, float>.Operation, PlayerBuffType, float>)Delegate.Combine(buffs.OnChange, new Action<SyncIDictionary<PlayerBuffType, float>.Operation, PlayerBuffType, float>(this.OnBuffsChanged));
	}

	// Token: 0x060011D1 RID: 4561 RVA: 0x0004CEA4 File Offset: 0x0004B0A4
	private void OnBuffsChanged(SyncIDictionary<PlayerBuffType, float>.Operation op, PlayerBuffType key, float oldValue)
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		this._pb.OnChanged(key, this._buffs[key]);
	}

	// Token: 0x060011D2 RID: 4562 RVA: 0x0004CEC8 File Offset: 0x0004B0C8
	[Server]
	public void SetBuffArea(PlayerBuffType type, Item item, BuffArea area)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerBuff::SetBuffArea(PlayerBuffType,Item,BuffArea)' called when server was not active");
			return;
		}
		Dictionary<Item, BuffArea> dictionary;
		if (!this._buffAreas.TryGetValue(type, out dictionary))
		{
			dictionary = new Dictionary<Item, BuffArea>();
			this._buffAreas[type] = dictionary;
		}
		dictionary[item] = area;
	}

	// Token: 0x060011D3 RID: 4563 RVA: 0x0004CF18 File Offset: 0x0004B118
	[Server]
	public void ResetBuffArea(PlayerBuffType type, Item item)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerBuff::ResetBuffArea(PlayerBuffType,Item)' called when server was not active");
			return;
		}
		Dictionary<Item, BuffArea> dictionary;
		if (this._buffAreas.TryGetValue(type, out dictionary))
		{
			dictionary.Remove(item);
		}
	}

	// Token: 0x060011D4 RID: 4564 RVA: 0x0004CF54 File Offset: 0x0004B154
	private void Update()
	{
		if (!base.isServer)
		{
			return;
		}
		Vector3 position = base.transform.position;
		foreach (KeyValuePair<PlayerBuffType, Dictionary<Item, BuffArea>> keyValuePair in this._buffAreas)
		{
			PlayerBuffType playerBuffType;
			Dictionary<Item, BuffArea> dictionary;
			keyValuePair.Deconstruct(out playerBuffType, out dictionary);
			PlayerBuffType playerBuffType2 = playerBuffType;
			Dictionary<Item, BuffArea> dictionary2 = dictionary;
			float num = 0f;
			this._removeCache.Clear();
			if (playerBuffType2 == PlayerBuffType.InspiringMelody)
			{
				goto IL_102;
			}
			if (playerBuffType2 == PlayerBuffType.Immunity)
			{
				using (Dictionary<Item, BuffArea>.Enumerator enumerator2 = dictionary2.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						KeyValuePair<Item, BuffArea> keyValuePair2 = enumerator2.Current;
						Item key = keyValuePair2.Key;
						BuffArea value = keyValuePair2.Value;
						if (!value.Source)
						{
							this._removeCache.Add(key);
						}
						else if (value.IsActive && (value.Source.position - position).sqrMagnitude <= value.Range * value.Range)
						{
							num = 1f;
							break;
						}
					}
					goto IL_1B8;
				}
				goto IL_102;
			}
			IL_1B8:
			foreach (Item key2 in this._removeCache)
			{
				dictionary2.Remove(key2);
			}
			this._buffs[playerBuffType2] = num;
			continue;
			IL_102:
			foreach (KeyValuePair<Item, BuffArea> keyValuePair3 in dictionary2)
			{
				Item key3 = keyValuePair3.Key;
				BuffArea value2 = keyValuePair3.Value;
				if (!value2.Source)
				{
					this._removeCache.Add(key3);
				}
				else if (value2.IsActive && (value2.Source.position - position).sqrMagnitude <= value2.Range * value2.Range)
				{
					num += value2.Amount;
					if (num >= 1f)
					{
						break;
					}
				}
			}
			num = Mathf.Clamp01(num);
			goto IL_1B8;
		}
	}

	// Token: 0x060011D5 RID: 4565 RVA: 0x0004D1E4 File Offset: 0x0004B3E4
	[Server]
	public void ApplyBuff(PlayerBuffType type, float amount, float duration = 0f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Void PlayerBuff::ApplyBuff(PlayerBuffType,System.Single,System.Single)' called when server was not active");
			return;
		}
		if (this._buffs.ContainsKey(type))
		{
			SyncDictionary<PlayerBuffType, float> buffs = this._buffs;
			buffs[type] += amount;
		}
		else
		{
			this._buffs[type] = 1f + amount;
		}
		if (duration > 0f)
		{
			base.StartCoroutine(this.RemoveBuffAfter(type, amount, duration));
		}
	}

	// Token: 0x060011D6 RID: 4566 RVA: 0x0004D25C File Offset: 0x0004B45C
	[Server]
	private IEnumerator RemoveBuffAfter(PlayerBuffType type, float amount, float delay)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning("[Server] function 'System.Collections.IEnumerator PlayerBuff::RemoveBuffAfter(PlayerBuffType,System.Single,System.Single)' called when server was not active");
			return null;
		}
		PlayerBuff.<RemoveBuffAfter>d__12 <RemoveBuffAfter>d__ = new PlayerBuff.<RemoveBuffAfter>d__12(0);
		<RemoveBuffAfter>d__.<>4__this = this;
		<RemoveBuffAfter>d__.type = type;
		<RemoveBuffAfter>d__.amount = amount;
		<RemoveBuffAfter>d__.delay = delay;
		return <RemoveBuffAfter>d__;
	}

	// Token: 0x060011D7 RID: 4567 RVA: 0x0004D2AC File Offset: 0x0004B4AC
	public float GetValue(PlayerBuffType type)
	{
		return this._buffs.GetValueOrDefault(type, 0f);
	}

	// Token: 0x060011D8 RID: 4568 RVA: 0x0004D2BF File Offset: 0x0004B4BF
	public PlayerBuff()
	{
		base.InitSyncObject(this._buffs);
	}

	// Token: 0x060011D9 RID: 4569 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x04000B80 RID: 2944
	private readonly SyncDictionary<PlayerBuffType, float> _buffs = new SyncDictionary<PlayerBuffType, float>();

	// Token: 0x04000B81 RID: 2945
	private readonly Dictionary<PlayerBuffType, Dictionary<Item, BuffArea>> _buffAreas = new Dictionary<PlayerBuffType, Dictionary<Item, BuffArea>>();

	// Token: 0x04000B82 RID: 2946
	private readonly List<Item> _removeCache = new List<Item>();

	// Token: 0x04000B83 RID: 2947
	private PlayerBuffUI _pb;
}

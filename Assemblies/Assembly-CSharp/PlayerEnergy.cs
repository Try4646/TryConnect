using System;
using System.Runtime.InteropServices;
using Mirror;
using MoreMountains.Tools;
using UnityEngine;

// Token: 0x020002D4 RID: 724
public class PlayerEnergy : NetworkBehaviour
{
	// Token: 0x06001975 RID: 6517 RVA: 0x0006AD99 File Offset: 0x00068F99
	public void DecreaseEnergy(float amount)
	{
		this.Networkenergy = this.energy - amount;
		this.Networkenergy = Mathf.Clamp(this.energy, 0f, 100f);
	}

	// Token: 0x06001976 RID: 6518 RVA: 0x0006ADC4 File Offset: 0x00068FC4
	public void AddEnergy(float amount)
	{
		this.Networkenergy = this.energy + amount;
		this.Networkenergy = Mathf.Clamp(this.energy, 0f, 100f);
	}

	// Token: 0x06001977 RID: 6519 RVA: 0x0006ADF0 File Offset: 0x00068FF0
	private void OnEnergyChanged(float oldValue, float newValue)
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		float alpha = MMMaths.Remap(newValue, 100f, 0f, 0f, 1f);
		this.energyUI.alpha = alpha;
	}

	// Token: 0x06001978 RID: 6520 RVA: 0x0006AE2D File Offset: 0x0006902D
	public PlayerEnergy()
	{
		this._Mirror_SyncVarHookDelegate_energy = new Action<float, float>(this.OnEnergyChanged);
	}

	// Token: 0x06001979 RID: 6521 RVA: 0x00002321 File Offset: 0x00000521
	public override bool Weaved()
	{
		return true;
	}

	// Token: 0x17000263 RID: 611
	// (get) Token: 0x0600197A RID: 6522 RVA: 0x0006AE48 File Offset: 0x00069048
	// (set) Token: 0x0600197B RID: 6523 RVA: 0x0006AE5B File Offset: 0x0006905B
	public float Networkenergy
	{
		get
		{
			return this.energy;
		}
		[param: In]
		set
		{
			base.GeneratedSyncVarSetter<float>(value, ref this.energy, 1UL, this._Mirror_SyncVarHookDelegate_energy);
		}
	}

	// Token: 0x0600197C RID: 6524 RVA: 0x0006AE7C File Offset: 0x0006907C
	public override void SerializeSyncVars(NetworkWriter writer, bool forceAll)
	{
		base.SerializeSyncVars(writer, forceAll);
		if (forceAll)
		{
			writer.WriteFloat(this.energy);
			return;
		}
		writer.WriteVarULong(this.syncVarDirtyBits);
		if ((this.syncVarDirtyBits & 1UL) != 0UL)
		{
			writer.WriteFloat(this.energy);
		}
	}

	// Token: 0x0600197D RID: 6525 RVA: 0x0006AED4 File Offset: 0x000690D4
	public override void DeserializeSyncVars(NetworkReader reader, bool initialState)
	{
		base.DeserializeSyncVars(reader, initialState);
		if (initialState)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.energy, this._Mirror_SyncVarHookDelegate_energy, reader.ReadFloat());
			return;
		}
		long num = (long)reader.ReadVarULong();
		if ((num & 1L) != 0L)
		{
			base.GeneratedSyncVarDeserialize<float>(ref this.energy, this._Mirror_SyncVarHookDelegate_energy, reader.ReadFloat());
		}
	}

	// Token: 0x0400105E RID: 4190
	[SerializeField]
	[SyncVar(hook = "OnEnergyChanged")]
	private float energy;

	// Token: 0x0400105F RID: 4191
	public CanvasGroup energyUI;

	// Token: 0x04001060 RID: 4192
	public Action<float, float> _Mirror_SyncVarHookDelegate_energy;
}

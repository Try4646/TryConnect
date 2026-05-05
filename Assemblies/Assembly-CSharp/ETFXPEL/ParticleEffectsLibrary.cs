using System;
using System.Collections.Generic;
using UnityEngine;

namespace ETFXPEL
{
	// Token: 0x0200036A RID: 874
	public class ParticleEffectsLibrary : MonoBehaviour
	{
		// Token: 0x06001CD9 RID: 7385 RVA: 0x0007CCD4 File Offset: 0x0007AED4
		private void Awake()
		{
			ParticleEffectsLibrary.GlobalAccess = this;
			this.currentActivePEList = new List<Transform>();
			this.TotalEffects = this.ParticleEffectPrefabs.Length;
			this.CurrentParticleEffectNum = 1;
			if (this.ParticleEffectSpawnOffsets.Length != this.TotalEffects)
			{
				Debug.LogError("ParticleEffectsLibrary-ParticleEffectSpawnOffset: Not all arrays match length, double check counts.");
			}
			if (this.ParticleEffectPrefabs.Length != this.TotalEffects)
			{
				Debug.LogError("ParticleEffectsLibrary-ParticleEffectPrefabs: Not all arrays match length, double check counts.");
			}
			this.effectNameString = string.Concat(new string[]
			{
				this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].name,
				" (",
				this.CurrentParticleEffectNum.ToString(),
				" of ",
				this.TotalEffects.ToString(),
				")"
			});
		}

		// Token: 0x06001CDA RID: 7386 RVA: 0x000048A7 File Offset: 0x00002AA7
		private void Start()
		{
		}

		// Token: 0x06001CDB RID: 7387 RVA: 0x0007CD98 File Offset: 0x0007AF98
		public string GetCurrentPENameString()
		{
			return string.Concat(new string[]
			{
				this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].name,
				" (",
				this.CurrentParticleEffectNum.ToString(),
				" of ",
				this.TotalEffects.ToString(),
				")"
			});
		}

		// Token: 0x06001CDC RID: 7388 RVA: 0x0007CDFC File Offset: 0x0007AFFC
		public void PreviousParticleEffect()
		{
			if (this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex] == 0f && this.currentActivePEList.Count > 0)
			{
				for (int i = 0; i < this.currentActivePEList.Count; i++)
				{
					if (this.currentActivePEList[i] != null)
					{
						Object.Destroy(this.currentActivePEList[i].gameObject);
					}
				}
				this.currentActivePEList.Clear();
			}
			if (this.CurrentParticleEffectIndex > 0)
			{
				this.CurrentParticleEffectIndex--;
			}
			else
			{
				this.CurrentParticleEffectIndex = this.TotalEffects - 1;
			}
			this.CurrentParticleEffectNum = this.CurrentParticleEffectIndex + 1;
			this.effectNameString = string.Concat(new string[]
			{
				this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].name,
				" (",
				this.CurrentParticleEffectNum.ToString(),
				" of ",
				this.TotalEffects.ToString(),
				")"
			});
		}

		// Token: 0x06001CDD RID: 7389 RVA: 0x0007CF08 File Offset: 0x0007B108
		public void NextParticleEffect()
		{
			if (this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex] == 0f && this.currentActivePEList.Count > 0)
			{
				for (int i = 0; i < this.currentActivePEList.Count; i++)
				{
					if (this.currentActivePEList[i] != null)
					{
						Object.Destroy(this.currentActivePEList[i].gameObject);
					}
				}
				this.currentActivePEList.Clear();
			}
			if (this.CurrentParticleEffectIndex < this.TotalEffects - 1)
			{
				this.CurrentParticleEffectIndex++;
			}
			else
			{
				this.CurrentParticleEffectIndex = 0;
			}
			this.CurrentParticleEffectNum = this.CurrentParticleEffectIndex + 1;
			this.effectNameString = string.Concat(new string[]
			{
				this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].name,
				" (",
				this.CurrentParticleEffectNum.ToString(),
				" of ",
				this.TotalEffects.ToString(),
				")"
			});
		}

		// Token: 0x06001CDE RID: 7390 RVA: 0x0007D014 File Offset: 0x0007B214
		public void SpawnParticleEffect(Vector3 positionInWorldToSpawn)
		{
			this.spawnPosition = positionInWorldToSpawn + this.ParticleEffectSpawnOffsets[this.CurrentParticleEffectIndex];
			GameObject gameObject = Object.Instantiate<GameObject>(this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex], this.spawnPosition, this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex].transform.rotation);
			Object @object = gameObject;
			string str = "PE_";
			GameObject gameObject2 = this.ParticleEffectPrefabs[this.CurrentParticleEffectIndex];
			@object.name = str + ((gameObject2 != null) ? gameObject2.ToString() : null);
			if (this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex] == 0f)
			{
				this.currentActivePEList.Add(gameObject.transform);
			}
			this.currentActivePEList.Add(gameObject.transform);
			if (this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex] != 0f)
			{
				Object.Destroy(gameObject, this.ParticleEffectLifetimes[this.CurrentParticleEffectIndex]);
			}
		}

		// Token: 0x04001366 RID: 4966
		public static ParticleEffectsLibrary GlobalAccess;

		// Token: 0x04001367 RID: 4967
		public int TotalEffects;

		// Token: 0x04001368 RID: 4968
		public int CurrentParticleEffectIndex;

		// Token: 0x04001369 RID: 4969
		public int CurrentParticleEffectNum;

		// Token: 0x0400136A RID: 4970
		public Vector3[] ParticleEffectSpawnOffsets;

		// Token: 0x0400136B RID: 4971
		public float[] ParticleEffectLifetimes;

		// Token: 0x0400136C RID: 4972
		public GameObject[] ParticleEffectPrefabs;

		// Token: 0x0400136D RID: 4973
		private string effectNameString = "";

		// Token: 0x0400136E RID: 4974
		private List<Transform> currentActivePEList;

		// Token: 0x0400136F RID: 4975
		private Vector3 spawnPosition = Vector3.zero;
	}
}

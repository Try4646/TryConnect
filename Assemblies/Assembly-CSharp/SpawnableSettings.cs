using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000339 RID: 825
[CreateAssetMenu(menuName = "Game Settings/Spawnable Settings", fileName = "SpawnableSettings")]
public class SpawnableSettings : ScriptableObject
{
	// Token: 0x06001B50 RID: 6992 RVA: 0x000746C4 File Offset: 0x000728C4
	public static SpawnableSO GetSpawnableSoById(int id)
	{
		return Resources.Load<SpawnableSettings>("SpawnableSettings").spawnables.FirstOrDefault((SpawnableSO s) => s.spawnableID == id);
	}

	// Token: 0x14000029 RID: 41
	// (add) Token: 0x06001B51 RID: 6993 RVA: 0x00074700 File Offset: 0x00072900
	// (remove) Token: 0x06001B52 RID: 6994 RVA: 0x00074734 File Offset: 0x00072934
	public static event Action<SpawnableSettings> SettingsChanged;

	// Token: 0x06001B53 RID: 6995 RVA: 0x00074767 File Offset: 0x00072967
	public void NotifyChanged()
	{
		Action<SpawnableSettings> settingsChanged = SpawnableSettings.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged(this);
	}

	// Token: 0x0400121B RID: 4635
	[Header("Settings")]
	public bool isEnabled = true;

	// Token: 0x0400121C RID: 4636
	[Header("Spawnable Prefabs")]
	public List<SpawnableSO> spawnables = new List<SpawnableSO>();
}

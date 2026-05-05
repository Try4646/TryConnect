using System;
using UnityEngine;

// Token: 0x02000338 RID: 824
[CreateAssetMenu(fileName = "SpawnableSO", menuName = "Spawnable/SpawnableSO")]
public class SpawnableSO : ScriptableObject
{
	// Token: 0x04001217 RID: 4631
	[UniqueID("spawnables")]
	public int spawnableID;

	// Token: 0x04001218 RID: 4632
	public string spawnableName;

	// Token: 0x04001219 RID: 4633
	[TextArea(3, 10)]
	public string spawnableDescription;

	// Token: 0x0400121A RID: 4634
	public GameObject prefab;
}

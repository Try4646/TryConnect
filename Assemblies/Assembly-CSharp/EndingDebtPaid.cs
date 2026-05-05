using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200029D RID: 669
public class EndingDebtPaid : MonoBehaviour
{
	// Token: 0x060017CD RID: 6093 RVA: 0x00064CE4 File Offset: 0x00062EE4
	private void Start()
	{
		if (this.livingRoomStuff == null)
		{
			Debug.LogError("Living Room Stuff reference is missing!");
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (object obj in this.livingRoomStuff.transform)
		{
			Transform transform = (Transform)obj;
			list.Add(transform.gameObject);
		}
		foreach (GameObject gameObject in list)
		{
			gameObject.SetActive(false);
		}
	}

	// Token: 0x060017CE RID: 6094 RVA: 0x00064DA4 File Offset: 0x00062FA4
	public void EnableLivingRoomStuff()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (object obj in this.livingRoomStuff.transform)
		{
			Transform transform = (Transform)obj;
			list.Add(transform.gameObject);
		}
		foreach (GameObject gameObject in list)
		{
			gameObject.SetActive(true);
		}
	}

	// Token: 0x04000F67 RID: 3943
	[SerializeField]
	private GameObject livingRoomStuff;
}

using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Token: 0x02000281 RID: 641
public class SFXManager : MonoBehaviour
{
	// Token: 0x17000208 RID: 520
	// (get) Token: 0x060016C8 RID: 5832 RVA: 0x0006125E File Offset: 0x0005F45E
	// (set) Token: 0x060016C9 RID: 5833 RVA: 0x00061265 File Offset: 0x0005F465
	public static SFXManager Instance { get; private set; }

	// Token: 0x060016CA RID: 5834 RVA: 0x0006126D File Offset: 0x0005F46D
	private void Awake()
	{
		if (SFXManager.Instance != null && SFXManager.Instance != this)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		SFXManager.Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x060016CB RID: 5835 RVA: 0x000612A6 File Offset: 0x0005F4A6
	public static void SFXOneShot(EventReference sfxEvent, Vector3 pos = default(Vector3))
	{
		if (sfxEvent.IsNull)
		{
			return;
		}
		RuntimeManager.PlayOneShot(sfxEvent, pos);
	}

	// Token: 0x060016CC RID: 5836 RVA: 0x000612BC File Offset: 0x0005F4BC
	public static void SFXOneShotWithParameters(EventReference sfxEvent, SFXParams[] sFXParams, Vector3 pos = default(Vector3), float pitch = 1f)
	{
		if (sfxEvent.IsNull)
		{
			return;
		}
		EventInstance eventInstance = RuntimeManager.CreateInstance(sfxEvent);
		if (sFXParams != null)
		{
			foreach (SFXParams sfxparams in sFXParams)
			{
				eventInstance.setParameterByName(sfxparams.name, sfxparams.value, false);
			}
		}
		eventInstance.set3DAttributes(pos.To3DAttributes());
		eventInstance.setPitch(pitch);
		eventInstance.start();
		eventInstance.release();
	}

	// Token: 0x060016CD RID: 5837 RVA: 0x00061334 File Offset: 0x0005F534
	public static void SFXOneShot3DAttachedWithParameters(EventReference sfxEvent, SFXParams[] sFXParams, GameObject attachObject, bool non_rigidbody_velocity = false)
	{
		if (sfxEvent.IsNull)
		{
			return;
		}
		EventInstance instance = RuntimeManager.CreateInstance(sfxEvent);
		RuntimeManager.AttachInstanceToGameObject(instance, attachObject, non_rigidbody_velocity);
		if (sFXParams != null)
		{
			foreach (SFXParams sfxparams in sFXParams)
			{
				instance.setParameterByName(sfxparams.name, sfxparams.value, false);
			}
		}
		instance.start();
		instance.release();
	}

	// Token: 0x060016CE RID: 5838 RVA: 0x0006139C File Offset: 0x0005F59C
	public static void SFXOneShot3DAttached(EventReference sfxEvent, GameObject attachObject, bool non_rigidbody_velocity = false)
	{
		if (sfxEvent.IsNull)
		{
			return;
		}
		EventInstance instance = RuntimeManager.CreateInstance(sfxEvent);
		RuntimeManager.AttachInstanceToGameObject(instance, attachObject, non_rigidbody_velocity);
		instance.start();
		instance.release();
	}
}

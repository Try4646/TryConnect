using System;
using Mirror;
using UnityEngine;

namespace Dissonance.Integrations.MirrorIgnorance.Demo
{
	// Token: 0x0200038E RID: 910
	public class MirrorIgnorancePlayerController : NetworkBehaviour
	{
		// Token: 0x06001DE8 RID: 7656 RVA: 0x0008085C File Offset: 0x0007EA5C
		private void Update()
		{
			if (!base.isLocalPlayer)
			{
				return;
			}
			CharacterController component = base.GetComponent<CharacterController>();
			float yAngle = Input.GetAxis("Horizontal") * Time.deltaTime * 150f;
			float d = Input.GetAxis("Vertical") * 3f;
			base.transform.Rotate(0f, yAngle, 0f);
			Vector3 a = base.transform.TransformDirection(Vector3.forward);
			component.SimpleMove(a * d);
			if (base.transform.position.y < -3f)
			{
				base.transform.position = Vector3.zero;
				base.transform.rotation = Quaternion.identity;
			}
		}

		// Token: 0x06001DEA RID: 7658 RVA: 0x00002321 File Offset: 0x00000521
		public override bool Weaved()
		{
			return true;
		}
	}
}

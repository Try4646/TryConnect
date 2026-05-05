using System;
using UnityEngine;

namespace Smooth
{
	// Token: 0x02000365 RID: 869
	public class StateMirror
	{
		// Token: 0x06001CC5 RID: 7365 RVA: 0x0007BB10 File Offset: 0x00079D10
		public StateMirror copyFromState(StateMirror state)
		{
			this.ownerTimestamp = state.ownerTimestamp;
			this.position = state.position;
			this.rotation = state.rotation;
			this.scale = state.scale;
			this.velocity = state.velocity;
			this.angularVelocity = state.angularVelocity;
			this.receivedTimestamp = state.receivedTimestamp;
			this.localTimeResetIndicator = state.localTimeResetIndicator;
			return this;
		}

		// Token: 0x06001CC6 RID: 7366 RVA: 0x0007BB80 File Offset: 0x00079D80
		public static StateMirror Lerp(StateMirror targetTempStateMirror, StateMirror start, StateMirror end, float t)
		{
			targetTempStateMirror.position = Vector3.Lerp(start.position, end.position, t);
			targetTempStateMirror.rotation = Quaternion.Lerp(start.rotation, end.rotation, t);
			targetTempStateMirror.scale = Vector3.Lerp(start.scale, end.scale, t);
			targetTempStateMirror.velocity = Vector3.Lerp(start.velocity, end.velocity, t);
			targetTempStateMirror.angularVelocity = Vector3.Lerp(start.angularVelocity, end.angularVelocity, t);
			targetTempStateMirror.ownerTimestamp = Mathf.Lerp(start.ownerTimestamp, end.ownerTimestamp, t);
			return targetTempStateMirror;
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x0007BC20 File Offset: 0x00079E20
		public void resetTheVariables()
		{
			this.ownerTimestamp = 0f;
			this.position = Vector3.zero;
			this.rotation = Quaternion.identity;
			this.scale = Vector3.zero;
			this.velocity = Vector3.zero;
			this.angularVelocity = Vector3.zero;
			this.atPositionalRest = false;
			this.atRotationalRest = false;
			this.teleport = false;
			this.receivedTimestamp = 0f;
			this.localTimeResetIndicator = 0;
		}

		// Token: 0x06001CC8 RID: 7368 RVA: 0x0007BC98 File Offset: 0x00079E98
		public void copyFromSmoothSync(SmoothSyncMirror smoothSyncScript)
		{
			this.ownerTimestamp = smoothSyncScript.localTime;
			this.position = smoothSyncScript.getPosition();
			this.rotation = smoothSyncScript.getRotation();
			this.scale = smoothSyncScript.getScale();
			if (smoothSyncScript.hasRigidbody)
			{
				this.velocity = smoothSyncScript.rb.linearVelocity;
				this.angularVelocity = smoothSyncScript.rb.angularVelocity * 57.29578f;
			}
			else if (smoothSyncScript.hasRigidbody2D)
			{
				this.velocity = smoothSyncScript.rb2D.linearVelocity;
				this.angularVelocity.x = 0f;
				this.angularVelocity.y = 0f;
				this.angularVelocity.z = smoothSyncScript.rb2D.angularVelocity;
			}
			else
			{
				this.velocity = Vector3.zero;
				this.angularVelocity = Vector3.zero;
			}
			this.localTimeResetIndicator = smoothSyncScript.localTimeResetIndicator;
		}

		// Token: 0x04001345 RID: 4933
		public float ownerTimestamp;

		// Token: 0x04001346 RID: 4934
		public Vector3 position;

		// Token: 0x04001347 RID: 4935
		public Quaternion rotation;

		// Token: 0x04001348 RID: 4936
		public Vector3 scale;

		// Token: 0x04001349 RID: 4937
		public Vector3 velocity;

		// Token: 0x0400134A RID: 4938
		public Vector3 angularVelocity;

		// Token: 0x0400134B RID: 4939
		public bool teleport;

		// Token: 0x0400134C RID: 4940
		public bool atPositionalRest;

		// Token: 0x0400134D RID: 4941
		public bool atRotationalRest;

		// Token: 0x0400134E RID: 4942
		public float receivedOnServerTimestamp;

		// Token: 0x0400134F RID: 4943
		public float receivedTimestamp;

		// Token: 0x04001350 RID: 4944
		public int localTimeResetIndicator;

		// Token: 0x04001351 RID: 4945
		public Vector3 reusableRotationVector;

		// Token: 0x04001352 RID: 4946
		public bool serverShouldRelayPosition;

		// Token: 0x04001353 RID: 4947
		public bool serverShouldRelayRotation;

		// Token: 0x04001354 RID: 4948
		public bool serverShouldRelayScale;

		// Token: 0x04001355 RID: 4949
		public bool serverShouldRelayVelocity;

		// Token: 0x04001356 RID: 4950
		public bool serverShouldRelayAngularVelocity;
	}
}

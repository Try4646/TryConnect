using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

// Token: 0x02000112 RID: 274
public class VoiceAnnouncementPlayer : MonoBehaviour
{
	// Token: 0x06000B65 RID: 2917 RVA: 0x0002E148 File Offset: 0x0002C348
	public void OnVoiceStart()
	{
		UnityEngine.Debug.Log("Voice announcement starting");
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(true);
		}
		this.OnVoiceStop();
		this._resamplePosition = 0f;
		this._resampleBuffer.Clear();
		Queue<float[]> obj = this.audioBufferQueue;
		lock (obj)
		{
			this.audioBufferQueue.Clear();
		}
		this.isPlaying = true;
		SPEAKERMODE speakermode;
		int num;
		RuntimeManager.CoreSystem.getSoftwareFormat(out this._fmodSampleRate, out speakermode, out num);
		this._needsResampling = (this._fmodSampleRate != 16000);
		if (!this._needsResampling)
		{
			this._resampleRatio = 1f;
			UnityEngine.Debug.Log(string.Format("✅ Sample rates match ({0} Hz), no resampling needed", this._fmodSampleRate));
		}
		else
		{
			this._resampleRatio = 16000f / (float)this._fmodSampleRate;
			UnityEngine.Debug.Log(string.Format("⚠️ Sample rate mismatch - FMOD: {0} Hz, Input: {1} Hz, Resample ratio: {2:F6}", this._fmodSampleRate, 16000, this._resampleRatio));
		}
		this._resamplePosition = 0f;
		this._generator = new VoiceAnnouncementPlayer.AudioDataGenerator(this);
		this._handle = GCHandle.Alloc(this._generator);
		DSP_DESCRIPTION dsp_DESCRIPTION = new DSP_DESCRIPTION
		{
			numinputbuffers = 0,
			numoutputbuffers = 1,
			read = new DSP_READ_CALLBACK(VoiceAnnouncementPlayer.ReadDSP),
			shouldiprocess = new DSP_SHOULDIPROCESS_CALLBACK(VoiceAnnouncementPlayer.ShouldProcessDSP),
			userdata = (IntPtr)this._handle
		};
		RESULT result = RuntimeManager.CoreSystem.createDSP(ref dsp_DESCRIPTION, out this._dsp);
		if (result != RESULT.OK)
		{
			UnityEngine.Debug.LogError(string.Format("Failed to create FMOD DSP: {0}", result));
			return;
		}
		GUID id = GUID.Parse("{56eae11b-1ae9-48d6-9574-3178c27509a6}");
		FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
		if (studioSystem.getBusByID(id, out this._voiceBus) == RESULT.OK)
		{
			this._voiceBus.lockChannelGroup();
			studioSystem = RuntimeManager.StudioSystem;
			studioSystem.flushCommands();
			ChannelGroup channelgroup;
			if (this._voiceBus.getChannelGroup(out channelgroup) == RESULT.OK && RuntimeManager.CoreSystem.playDSP(this._dsp, channelgroup, false, out this._channel) == RESULT.OK)
			{
				UnityEngine.Debug.Log("✅ Voice playback started on bus");
			}
		}
		if (!this._channel.hasHandle())
		{
			result = RuntimeManager.CoreSystem.playDSP(this._dsp, default(ChannelGroup), false, out this._channel);
			if (result != RESULT.OK)
			{
				UnityEngine.Debug.LogError(string.Format("Failed to play FMOD DSP: {0}", result));
				return;
			}
			UnityEngine.Debug.LogWarning("⚠️ Could not set bus, playing on default channel");
		}
		this._channel.setPriority(0);
		studioSystem = RuntimeManager.StudioSystem;
		studioSystem.setParameterByName("AnnouncerFX", (float)(this.applyRadioEffect ? 1 : 0), false);
		this.PlayMicOnSFX();
		UnityEngine.Debug.Log("✅ Voice playback started via FMOD DSP");
	}

	// Token: 0x06000B66 RID: 2918 RVA: 0x0002E430 File Offset: 0x0002C630
	public void OnVoiceAudio(string base64AudioData)
	{
		if (!this.isPlaying)
		{
			UnityEngine.Debug.LogWarning("Received audio data but not playing. Call OnVoiceStart first.");
			return;
		}
		if (string.IsNullOrEmpty(base64AudioData))
		{
			UnityEngine.Debug.LogWarning("Received empty audio data");
			return;
		}
		try
		{
			byte[] array = Convert.FromBase64String(base64AudioData);
			if (array.Length < 2)
			{
				UnityEngine.Debug.LogWarning(string.Format("Audio data too short: {0} bytes", array.Length));
			}
			else
			{
				int num = array.Length / 2;
				float[] array2 = new float[num];
				for (int i = 0; i < num; i++)
				{
					short num2 = (short)((int)array[i * 2] | (int)array[i * 2 + 1] << 8);
					array2[i] = (float)num2 / 32768f;
				}
				Queue<float[]> obj = this.audioBufferQueue;
				lock (obj)
				{
					this.audioBufferQueue.Enqueue(array2);
				}
				if (Random.Range(0, 20) == 0)
				{
					UnityEngine.Debug.Log(string.Format("Audio chunk received: {0} samples, Queue size: {1}", num, this.audioBufferQueue.Count));
				}
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Error processing audio data: " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	// Token: 0x06000B67 RID: 2919 RVA: 0x0002E568 File Offset: 0x0002C768
	public void OnVoiceStop()
	{
		this.isPlaying = false;
		if (this._channel.hasHandle())
		{
			this._channel.stop();
			this._channel.clearHandle();
		}
		if (this._dsp.hasHandle())
		{
			this._dsp.release();
			this._dsp.clearHandle();
		}
		if (this._voiceBus.isValid())
		{
			this._voiceBus.unlockChannelGroup();
			this._voiceBus.clearHandle();
		}
		if (this._handle.IsAllocated)
		{
			this._handle.Free();
		}
		this._generator = null;
		this._resampleBuffer.Clear();
		this._resamplePosition = 0f;
		Queue<float[]> obj = this.audioBufferQueue;
		lock (obj)
		{
			this.audioBufferQueue.Clear();
		}
	}

	// Token: 0x06000B68 RID: 2920 RVA: 0x0002E658 File Offset: 0x0002C858
	internal bool GetAudioData(float[] buffer, int samplesRequested)
	{
		if (!this.isPlaying)
		{
			for (int i = 0; i < samplesRequested; i++)
			{
				buffer[i] = 0f;
			}
			return true;
		}
		if (this._needsResampling)
		{
			this.ResampleAudio(buffer, samplesRequested);
		}
		else
		{
			this.CopySamplesDirectly(buffer, samplesRequested);
		}
		Queue<float[]> obj = this.audioBufferQueue;
		bool result;
		lock (obj)
		{
			result = (this.audioBufferQueue.Count == 0 && !this.isPlaying);
		}
		return result;
	}

	// Token: 0x06000B69 RID: 2921 RVA: 0x0002E6E8 File Offset: 0x0002C8E8
	private void CopySamplesDirectly(float[] outputBuffer, int outputSamples)
	{
		int num = 0;
		Queue<float[]> obj = this.audioBufferQueue;
		lock (obj)
		{
			while (num < outputSamples && this.audioBufferQueue.Count > 0)
			{
				float[] array = this.audioBufferQueue.Dequeue();
				int num2 = Mathf.Min(array.Length, outputSamples - num);
				Array.Copy(array, 0, outputBuffer, num, num2);
				num += num2;
				if (num2 < array.Length)
				{
					float[] array2 = new float[array.Length - num2];
					Array.Copy(array, num2, array2, 0, array2.Length);
					this.audioBufferQueue.Enqueue(array2);
				}
			}
		}
		for (int i = num; i < outputSamples; i++)
		{
			outputBuffer[i] = 0f;
		}
	}

	// Token: 0x06000B6A RID: 2922 RVA: 0x0002E7AC File Offset: 0x0002C9AC
	private void ResampleAudio(float[] outputBuffer, int outputSamples)
	{
		int num = Mathf.CeilToInt((float)outputSamples * this._resampleRatio) + 2;
		Queue<float[]> obj = this.audioBufferQueue;
		lock (obj)
		{
			while (this._resampleBuffer.Count < num && this.audioBufferQueue.Count > 0)
			{
				float[] collection = this.audioBufferQueue.Dequeue();
				this._resampleBuffer.AddRange(collection);
			}
		}
		if (this._resampleBuffer.Count < 2)
		{
			for (int i = 0; i < outputSamples; i++)
			{
				outputBuffer[i] = 0f;
			}
			return;
		}
		for (int j = 0; j < outputSamples; j++)
		{
			float resamplePosition = this._resamplePosition;
			int num2 = Mathf.FloorToInt(resamplePosition);
			float t = resamplePosition - (float)num2;
			if (num2 + 1 < this._resampleBuffer.Count)
			{
				outputBuffer[j] = Mathf.Lerp(this._resampleBuffer[num2], this._resampleBuffer[num2 + 1], t);
			}
			else if (num2 < this._resampleBuffer.Count)
			{
				outputBuffer[j] = this._resampleBuffer[num2];
			}
			else
			{
				outputBuffer[j] = 0f;
			}
			this._resamplePosition += this._resampleRatio;
		}
		int num3 = Mathf.FloorToInt(this._resamplePosition);
		if (num3 > 0)
		{
			if (num3 <= this._resampleBuffer.Count)
			{
				this._resampleBuffer.RemoveRange(0, num3);
				this._resamplePosition -= (float)num3;
				return;
			}
			this._resamplePosition -= (float)this._resampleBuffer.Count;
			this._resampleBuffer.Clear();
		}
	}

	// Token: 0x06000B6B RID: 2923 RVA: 0x0002E958 File Offset: 0x0002CB58
	private void OnDisable()
	{
		this.OnVoiceStop();
	}

	// Token: 0x06000B6C RID: 2924 RVA: 0x0002E960 File Offset: 0x0002CB60
	private void PlayMicOnSFX()
	{
		if (!string.IsNullOrWhiteSpace(this.sfxMicOnEvent))
		{
			RuntimeManager.PlayOneShot(this.sfxMicOnEvent, default(Vector3));
			return;
		}
	}

	// Token: 0x06000B6D RID: 2925 RVA: 0x0002E990 File Offset: 0x0002CB90
	[MonoPInvokeCallback(typeof(DSP_READ_CALLBACK))]
	private static RESULT ReadDSP(ref DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
	{
		IntPtr intPtr;
		if (dsp_state.functions.getuserdata(ref dsp_state, out intPtr) != RESULT.OK || intPtr == IntPtr.Zero)
		{
			VoiceAnnouncementPlayer.ClearBuffer(outbuffer, length);
			return RESULT.OK;
		}
		VoiceAnnouncementPlayer.AudioDataGenerator audioDataGenerator = (VoiceAnnouncementPlayer.AudioDataGenerator)GCHandle.FromIntPtr(intPtr).Target;
		if (audioDataGenerator == null)
		{
			VoiceAnnouncementPlayer.ClearBuffer(outbuffer, length);
			return RESULT.OK;
		}
		return audioDataGenerator.GetAudio(outbuffer, length, (uint)outchannels);
	}

	// Token: 0x06000B6E RID: 2926 RVA: 0x0002E9F4 File Offset: 0x0002CBF4
	[MonoPInvokeCallback(typeof(DSP_SHOULDIPROCESS_CALLBACK))]
	private static RESULT ShouldProcessDSP(ref DSP_STATE dsp_state, bool inputsidle, uint length, CHANNELMASK inmask, int inchannels, SPEAKERMODE speakermode)
	{
		IntPtr intPtr;
		if (dsp_state.functions.getuserdata(ref dsp_state, out intPtr) != RESULT.OK || intPtr == IntPtr.Zero)
		{
			return RESULT.ERR_DSP_SILENCE;
		}
		VoiceAnnouncementPlayer.AudioDataGenerator audioDataGenerator = (VoiceAnnouncementPlayer.AudioDataGenerator)GCHandle.FromIntPtr(intPtr).Target;
		if (audioDataGenerator == null)
		{
			return RESULT.ERR_DSP_SILENCE;
		}
		return audioDataGenerator.ShouldProcess();
	}

	// Token: 0x06000B6F RID: 2927 RVA: 0x0002EA46 File Offset: 0x0002CC46
	private static void ClearBuffer(IntPtr buffer, uint length)
	{
		Marshal.Copy(new float[length], 0, buffer, (int)length);
	}

	// Token: 0x0400070C RID: 1804
	private Queue<float[]> audioBufferQueue = new Queue<float[]>();

	// Token: 0x0400070D RID: 1805
	private bool isPlaying;

	// Token: 0x0400070E RID: 1806
	private const int INPUT_SAMPLE_RATE = 16000;

	// Token: 0x0400070F RID: 1807
	private DSP _dsp;

	// Token: 0x04000710 RID: 1808
	private Channel _channel;

	// Token: 0x04000711 RID: 1809
	private GCHandle _handle;

	// Token: 0x04000712 RID: 1810
	private VoiceAnnouncementPlayer.AudioDataGenerator _generator;

	// Token: 0x04000713 RID: 1811
	private int _fmodSampleRate;

	// Token: 0x04000714 RID: 1812
	[Header("Voice Effect Settings")]
	[SerializeField]
	private bool applyRadioEffect = true;

	// Token: 0x04000715 RID: 1813
	private Bus _voiceBus;

	// Token: 0x04000716 RID: 1814
	[Header("SFX")]
	[SerializeField]
	private string sfxMicOnEvent = "event:/Items/Microphone/MicOn";

	// Token: 0x04000717 RID: 1815
	private float _resamplePosition;

	// Token: 0x04000718 RID: 1816
	private float _resampleRatio;

	// Token: 0x04000719 RID: 1817
	private bool _needsResampling;

	// Token: 0x0400071A RID: 1818
	private List<float> _resampleBuffer = new List<float>();

	// Token: 0x02000113 RID: 275
	private class AudioDataGenerator
	{
		// Token: 0x06000B71 RID: 2929 RVA: 0x0002EA86 File Offset: 0x0002CC86
		public AudioDataGenerator(VoiceAnnouncementPlayer parent)
		{
			this._parent = parent;
		}

		// Token: 0x06000B72 RID: 2930 RVA: 0x0002EAA0 File Offset: 0x0002CCA0
		public RESULT GetAudio(IntPtr outbuffer, uint length, uint outchannels)
		{
			object @lock = this._lock;
			RESULT result;
			lock (@lock)
			{
				int num = (int)(length / outchannels);
				if (this._tempBuffer == null || num > this._tempBuffer.Length)
				{
					this._tempBuffer = new float[num * 2];
				}
				this._parent.GetAudioData(this._tempBuffer, num);
				if (outbuffer != IntPtr.Zero)
				{
					float[] array = new float[length];
					int num2 = 0;
					for (uint num3 = 0U; num3 < length; num3 += outchannels)
					{
						float num4 = this._tempBuffer[num2++];
						int num5 = 0;
						while ((long)num5 < (long)((ulong)outchannels))
						{
							array[(int)(checked((IntPtr)(unchecked((ulong)num3 + (ulong)((long)num5)))))] = num4;
							num5++;
						}
					}
					Marshal.Copy(array, 0, outbuffer, (int)length);
				}
				result = RESULT.OK;
			}
			return result;
		}

		// Token: 0x06000B73 RID: 2931 RVA: 0x0002EB78 File Offset: 0x0002CD78
		public RESULT ShouldProcess()
		{
			Queue<float[]> audioBufferQueue = this._parent.audioBufferQueue;
			RESULT result;
			lock (audioBufferQueue)
			{
				result = ((this._parent.isPlaying && this._parent.audioBufferQueue.Count > 0) ? RESULT.OK : RESULT.ERR_DSP_SILENCE);
			}
			return result;
		}

		// Token: 0x0400071B RID: 1819
		private readonly VoiceAnnouncementPlayer _parent;

		// Token: 0x0400071C RID: 1820
		private float[] _tempBuffer;

		// Token: 0x0400071D RID: 1821
		private readonly object _lock = new object();
	}
}

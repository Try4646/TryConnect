using System;
using System.ComponentModel;
using SRDebugger;
using SRDebugger.Internal;
using SRF.Service;
using UnityEngine;
using UnityEngine.Scripting;

// Token: 0x02000351 RID: 849
[Preserve]
public class SROptions : INotifyPropertyChanged
{
	// Token: 0x17000292 RID: 658
	// (get) Token: 0x06001BE0 RID: 7136 RVA: 0x00078223 File Offset: 0x00076423
	public static SROptions Current
	{
		get
		{
			return SROptions._current;
		}
	}

	// Token: 0x06001BE1 RID: 7137 RVA: 0x0007822A File Offset: 0x0007642A
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	public static void OnStartup()
	{
		SROptions._current = new SROptions();
		SRServiceManager.GetService<InternalOptionsRegistry>().AddOptionContainer(SROptions.Current);
	}

	// Token: 0x1400002B RID: 43
	// (add) Token: 0x06001BE2 RID: 7138 RVA: 0x00078248 File Offset: 0x00076448
	// (remove) Token: 0x06001BE3 RID: 7139 RVA: 0x00078280 File Offset: 0x00076480
	public event SROptionsPropertyChanged PropertyChanged;

	// Token: 0x06001BE4 RID: 7140 RVA: 0x000782B5 File Offset: 0x000764B5
	public void OnPropertyChanged(string propertyName)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, propertyName);
		}
		if (this.InterfacePropertyChangedEventHandler != null)
		{
			this.InterfacePropertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	// Token: 0x1400002C RID: 44
	// (add) Token: 0x06001BE5 RID: 7141 RVA: 0x000782E8 File Offset: 0x000764E8
	// (remove) Token: 0x06001BE6 RID: 7142 RVA: 0x00078320 File Offset: 0x00076520
	private event PropertyChangedEventHandler InterfacePropertyChangedEventHandler;

	// Token: 0x1400002D RID: 45
	// (add) Token: 0x06001BE7 RID: 7143 RVA: 0x00078355 File Offset: 0x00076555
	// (remove) Token: 0x06001BE8 RID: 7144 RVA: 0x0007835E File Offset: 0x0007655E
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			this.InterfacePropertyChangedEventHandler += value;
		}
		remove
		{
			this.InterfacePropertyChangedEventHandler -= value;
		}
	}

	// Token: 0x040012AB RID: 4779
	private static SROptions _current;

	// Token: 0x02000352 RID: 850
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class DisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
	{
		// Token: 0x06001BEA RID: 7146 RVA: 0x00078367 File Offset: 0x00076567
		public DisplayNameAttribute(string displayName) : base(displayName)
		{
		}
	}

	// Token: 0x02000353 RID: 851
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class IncrementAttribute : SRDebugger.IncrementAttribute
	{
		// Token: 0x06001BEB RID: 7147 RVA: 0x00078370 File Offset: 0x00076570
		public IncrementAttribute(double increment) : base(increment)
		{
		}
	}

	// Token: 0x02000354 RID: 852
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class NumberRangeAttribute : SRDebugger.NumberRangeAttribute
	{
		// Token: 0x06001BEC RID: 7148 RVA: 0x00078379 File Offset: 0x00076579
		public NumberRangeAttribute(double min, double max) : base(min, max)
		{
		}
	}

	// Token: 0x02000355 RID: 853
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
	public sealed class SortAttribute : SRDebugger.SortAttribute
	{
		// Token: 0x06001BED RID: 7149 RVA: 0x00078383 File Offset: 0x00076583
		public SortAttribute(int priority) : base(priority)
		{
		}
	}
}

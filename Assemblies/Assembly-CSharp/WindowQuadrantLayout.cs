using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

// Token: 0x020002CA RID: 714
public class WindowQuadrantLayout : MonoBehaviour
{
	// Token: 0x06001943 RID: 6467
	[DllImport("user32.dll")]
	private static extern bool EnumWindows(WindowQuadrantLayout.EnumWindowsProc lpEnumFunc, IntPtr lParam);

	// Token: 0x06001944 RID: 6468
	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	// Token: 0x06001945 RID: 6469
	[DllImport("user32.dll")]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	// Token: 0x06001946 RID: 6470
	[DllImport("user32.dll")]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

	// Token: 0x06001947 RID: 6471
	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

	// Token: 0x06001948 RID: 6472
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

	// Token: 0x06001949 RID: 6473
	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);

	// Token: 0x0600194A RID: 6474
	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

	// Token: 0x0600194B RID: 6475
	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out WindowQuadrantLayout.RECT lpRect);

	// Token: 0x0600194C RID: 6476 RVA: 0x0006A5E4 File Offset: 0x000687E4
	private void Start()
	{
		int argInt = this.GetArgInt("--quadIndex", -1);
		if (argInt < 0 || argInt > 3)
		{
			return;
		}
		Screen.fullScreenMode = FullScreenMode.Windowed;
		base.StartCoroutine(this.PositionRoutine(argInt));
	}

	// Token: 0x0600194D RID: 6477 RVA: 0x0006A61B File Offset: 0x0006881B
	private IEnumerator PositionRoutine(int quadIndex)
	{
		IntPtr hwnd = IntPtr.Zero;
		int i = 0;
		int num;
		while (i < 40 && hwnd == IntPtr.Zero)
		{
			hwnd = this.FindOwnTopLevelWindow();
			if (hwnd == IntPtr.Zero)
			{
				yield return new WaitForSeconds(0.05f);
			}
			num = i;
			i = num + 1;
		}
		if (hwnd == IntPtr.Zero)
		{
			yield break;
		}
		int systemWidth = Display.main.systemWidth;
		int systemHeight = Display.main.systemHeight;
		int halfW = Mathf.Max(200, systemWidth / 2);
		int halfH = Mathf.Max(200, systemHeight / 2);
		int num2 = quadIndex % 2;
		int num3 = quadIndex / 2;
		int x = num2 * halfW;
		int y = num3 * halfH;
		Screen.SetResolution(halfW, halfH, FullScreenMode.Windowed);
		WindowQuadrantLayout.ShowWindow(hwnd, 1);
		WindowQuadrantLayout.SetForegroundWindow(hwnd);
		for (i = 0; i < 12; i = num + 1)
		{
			WindowQuadrantLayout.MoveWindow(hwnd, x, y, halfW, halfH, true);
			yield return new WaitForSeconds(0.1f);
			WindowQuadrantLayout.RECT rect;
			if (WindowQuadrantLayout.GetWindowRect(hwnd, out rect))
			{
				int num4 = Math.Abs(rect.Left - x);
				int num5 = Math.Abs(rect.Top - y);
				if (num4 <= 8 && num5 <= 8)
				{
					break;
				}
			}
			num = i;
		}
		yield break;
	}

	// Token: 0x0600194E RID: 6478 RVA: 0x0006A631 File Offset: 0x00068831
	private IntPtr FindOwnTopLevelWindow()
	{
		uint myPid = (uint)Process.GetCurrentProcess().Id;
		IntPtr found = IntPtr.Zero;
		WindowQuadrantLayout.EnumWindows(delegate(IntPtr hWnd, IntPtr lParam)
		{
			if (!WindowQuadrantLayout.IsWindowVisible(hWnd))
			{
				return true;
			}
			uint num;
			WindowQuadrantLayout.GetWindowThreadProcessId(hWnd, out num);
			if (num != myPid)
			{
				return true;
			}
			int windowTextLength = WindowQuadrantLayout.GetWindowTextLength(hWnd);
			if (windowTextLength <= 0)
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(windowTextLength + 1);
			WindowQuadrantLayout.GetWindowText(hWnd, stringBuilder, stringBuilder.Capacity);
			found = hWnd;
			return false;
		}, IntPtr.Zero);
		return found;
	}

	// Token: 0x0600194F RID: 6479 RVA: 0x0006A670 File Offset: 0x00068870
	private int GetArgInt(string key, int fallback)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].StartsWith(key))
			{
				string[] array = commandLineArgs[i].Split('=', StringSplitOptions.None);
				int result;
				if (array.Length == 2 && int.TryParse(array[1], out result))
				{
					return result;
				}
			}
		}
		return fallback;
	}

	// Token: 0x0400103E RID: 4158
	private const int SW_SHOWNORMAL = 1;

	// Token: 0x020002CB RID: 715
	// (Invoke) Token: 0x06001952 RID: 6482
	internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	// Token: 0x020002CC RID: 716
	private struct RECT
	{
		// Token: 0x0400103F RID: 4159
		public int Left;

		// Token: 0x04001040 RID: 4160
		public int Top;

		// Token: 0x04001041 RID: 4161
		public int Right;

		// Token: 0x04001042 RID: 4162
		public int Bottom;
	}
}

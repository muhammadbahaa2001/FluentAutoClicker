﻿using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FluentAutoClicker.Helpers;

public static class AutoClicker
{
    private static Thread _autoClickerThread;
    public static int ClickInterval = 1000; // Milliseconds
    public static int RepeatAmount = 0; // How many clicks
    public static int MouseButton = 0; // 0 = Left, 1 = Middle, 2 = Right
    public static bool IsAutoClickerRunning { get; private set; }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    public static event Action AutoClickerStopped;

    public static void StartAutoClicker()
    {
        IsAutoClickerRunning = true;
        _autoClickerThread = new Thread(AutoClickerThread);
        _autoClickerThread.Start();
    }

    public static void StopAutoClicker()
    {
        IsAutoClickerRunning = false;
        _autoClickerThread?.Join();
    }

    private static async void AutoClickerThread()
    {
        var clickCount = 0;
        while (IsAutoClickerRunning)
        {
            if (clickCount >= RepeatAmount && RepeatAmount != 0)
            {
                StopAutoClicker();
                OnAutoClickerStopped();
                break;
            }

            switch (MouseButton)
            {
                case 0:
                    MouseEvent(0, 0, (uint)MouseEventF.LeftDown, 0, 0, IntPtr.Zero);
                    MouseEvent(0, 0, (uint)MouseEventF.LeftUp, 0, 0, IntPtr.Zero);
                    break;
                case 1:
                    MouseEvent(0, 0, (uint)MouseEventF.MiddleDown, 0, 0, IntPtr.Zero);
                    MouseEvent(0, 0, (uint)MouseEventF.MiddleUp, 0, 0, IntPtr.Zero);
                    break;
                case 2:
                    MouseEvent(0, 0, (uint)MouseEventF.RightDown, 0, 0, IntPtr.Zero);
                    MouseEvent(0, 0, (uint)MouseEventF.RightUp, 0, 0, IntPtr.Zero);
                    break;
            }

            if (RepeatAmount > 0) clickCount++;
            await Task.Delay(ClickInterval);
        }
    }

    private static void MouseEvent(int dx, int dy, uint dwFlags, uint dwData, uint time, nint dwExtraInfo)
    {
        var inputs = new Input[2];
        inputs[0] = MouseInput(dx, dy, dwData, dwFlags, time, dwExtraInfo);
        inputs[1] = MouseInput(dx, dy, dwData, dwFlags, time, dwExtraInfo);
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
    }

    private static Input MouseInput(int dx, int dy, uint mouseData, uint dwFlags, uint time, nint dwExtraInfo)
    {
        return new Input
        {
            type = 0,
            mi = new InputMouse
            {
                dx = dx,
                dy = dy,
                mouseData = mouseData,
                dwFlags = dwFlags,
                time = time,
                dwExtraInfo = dwExtraInfo
            }
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int type;
        public InputMouse mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct InputMouse
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [Flags]
    private enum MouseEventF : uint
    {
        LeftDown = 0x0002,
        LeftUp = 0x0004,
        RightDown = 0x0008,
        RightUp = 0x0010,
        MiddleDown = 0x0020,
        MiddleUp = 0x0040
    }

    private static void OnAutoClickerStopped()
    {
        AutoClickerStopped?.Invoke();
    }
}
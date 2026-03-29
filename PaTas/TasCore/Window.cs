using Avalonia.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace PalTas.TasCore;

public static class TasWindow
{
    /// <summary>
    /// 窗口句柄
    /// </summary>
    static HWND WindowHandle { get; set; }
    public static ConcurrentDictionary<User32.VK, bool> PressedKeys { get; set; } = [];

    /// <summary>
    /// 初始化 Tas 游戏窗口模块
    /// </summary>
    public static void Init()
    {
        // 打开游戏
        OpenGame();

        // 等待 PAL.EXE 运行
        WaitForPalProcess();

        // 延迟 1.2 秒，待游戏加载完成
        //Sleep(1200);
        Sleep(1);

#if DEBUG
        // 设置游戏无边框全屏
        //SetFullscreenWithoutBorder(WindowHandle);
#endif // DEBUG
    }

    /// <summary>
    /// 根据进程名终止进程
    /// </summary>
    /// <param name="processName">进程名（不区分大小写）</param>
    private static void KillProcessByName(string processName)
    {
        var processes = (List<Process>)[.. Process.GetProcesses().Where(p => p.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))];

        if (processes.Count == 0)
        {
            Log($"未找到 {processName} 进程");
            return;
        }

        Log($"找到 {processes.Count} 个 {processName} 进程，正在关闭...");

        foreach (var process in processes)
        {
            if (!process.HasExited)
            {
                process.Kill();
                process.WaitForExit(1000); // 等待进程退出
                Log($"已关闭进程: {process.ProcessName} (ID: {process.Id})");
            }

            process.Dispose();
        }
    }

    /// <summary>
    /// 自动重新打开游戏
    /// </summary>
    public static void OpenGame()
    {
        Log("关闭相关进程...");
#if DEBUG
        KillProcessByName("MSIAfterburner");
        KillProcessByName("RTSS");
#endif // DEBUG
        KillProcessByName("Pal");

        Sleep(300);

        // 游戏目录
#if DEBUG
        var gameDir = @"E:\Game\PAL98_v1.2";
        //var gameDir = @"E:\Game\Pal3.0";
#else
        var gameDir = $@"{Environment.CurrentDirectory}\..\";
#endif // DEBUG
        var palExePath = $@"{gameDir}\PAL.EXE";

        // 启动游戏
        Log("启动游戏...");
        var gameProcess = Process.Start(new ProcessStartInfo
        {
            FileName = palExePath,
            WorkingDirectory = gameDir,
            UseShellExecute = true
        });

#if DEBUG
        Process.Start(new ProcessStartInfo
        {
            FileName = "MSI Afterburner",
            WorkingDirectory = gameDir,
            UseShellExecute = true
        });
#endif // DEBUG
    }

    /// <summary>
    /// 查找 PAL 进程
    /// </summary>
    /// <returns>窗口句柄</returns>
    public static HWND FindPalProcess() => User32.FindWindow("ThunderRTMain");

    /// <summary>
    /// 循环等待游戏运行
    /// </summary>
    /// <returns>窗口句柄</returns>
    public static HWND WaitForPalProcess()
    {
        while (WindowHandle == 0)
        {
            Sleep();
            WindowHandle = FindPalProcess();
        }

        return WindowHandle;
    }

    /// <summary>
    /// 获取窗口句柄
    /// </summary>
    /// <returns>窗口句柄</returns>
    public static HWND GetWindowHandle() => WaitForPalProcess();

    /// <summary>
    /// 将指定窗口设置为无边框全屏
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    public static bool SetFullscreenWithoutBorder(HWND windowHandle)
    {
        // 获取当前窗口样式（返回 int，但需要显式转换为 WindowStyles）
        var style = (User32.WindowStyles)User32.GetWindowLong(windowHandle, User32.WindowLongFlags.GWL_STYLE);

        // 移除标题栏和可调整边框，添加弹出式样式
        style &= ~(User32.WindowStyles.WS_CAPTION | User32.WindowStyles.WS_THICKFRAME);
        style |= User32.WindowStyles.WS_POPUP;

        // 设置新样式（直接传递枚举值）
        User32.SetWindowLong(windowHandle, User32.WindowLongFlags.GWL_STYLE, (int)style);

        // 强制刷新窗口样式
        User32.SetWindowPos(windowHandle, HWND.NULL, 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOZORDER | User32.SetWindowPosFlags.SWP_FRAMECHANGED);

        var bounds = new List<PRECT>();
        User32.EnumDisplayMonitors(nint.Zero, null, (_, _, pRect, _) =>
        {
            bounds.Add(pRect!);
            return true;
        }, nint.Zero);

        // 第二个显示器（索引 1）的坐标和尺寸
        var secondary = bounds[0];
        int width = secondary.right - secondary.left;
        int height = secondary.bottom - secondary.top;

        // 移动窗口并调整大小（不改变 Z 顺序）
        return User32.SetWindowPos(windowHandle, HWND.HWND_TOPMOST, secondary.left, secondary.top, width, height, User32.SetWindowPosFlags.SWP_NOZORDER);
    }

    /// <summary>
    /// 将指定窗口设置为活动的前台窗口
    /// </summary>
    /// <param name="windowHandle">窗口句柄</param>
    /// <returns>是否设置成功</returns>
    public static bool ActivateWindow(HWND windowHandle)
    {
        // 如果窗口最小化，先还原
        User32.ShowWindow(windowHandle, ShowWindowCommand.SW_RESTORE);

        // 临时置顶再取消，强制窗口出现在前台
        User32.SetWindowPos(windowHandle, HWND.HWND_TOPMOST, 0, 0, 0, 0,
            User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE);
        User32.SetWindowPos(windowHandle, HWND.HWND_NOTOPMOST, 0, 0, 0, 0,
            User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOSIZE);

        // 最后尝试设为前台窗口
        return User32.SetForegroundWindow(windowHandle);
    }

    /// <summary>
    /// 在此检查游戏窗口激活状态，避免卡键
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    public static async Task ProcessPalWindowStatus(CancellationToken token)
    {
        var needReleaseAllKeys = false;

        while (!token.IsCancellationRequested)
        {
            await Delay(1, token);

            if ((User32.GetForegroundWindow() != WindowHandle) && !needReleaseAllKeys)
                // 需要释放所有键
                needReleaseAllKeys = true;
            else if ((User32.GetForegroundWindow() == WindowHandle) && needReleaseAllKeys)
            {
                needReleaseAllKeys = false;

                // 释放所有按键状态避免卡键
                await ReleaseAllKeys(token);
            }
        }
    }

    /// <summary>
    /// 按下并保持一个键
    /// </summary>
    /// <param name="key">要按下的虚拟键码</param>
    /// <returns>是否成功按下</returns>
    public static unsafe bool PressKey(User32.VK key)
    {
        var result = false;

        // 如果窗口为前台活动窗口，才发送按键事件
        if (User32.GetForegroundWindow() != WindowHandle) return false;

        // 无效的方向键
        if (key == User32.VK.VK_OEM_CLEAR) return false;

        // 如果键已经按下，先释放它
        if (PressedKeys.ContainsKey(key)) ReleaseKey(key);

        // 方法1: 使用 SendInput 发送 KEYDOWN 事件
        var scanCode = (ushort)User32.MapVirtualKey((uint)key, 0);
        User32.INPUT[] inputs = [
                new()
                {
                    type = User32.INPUTTYPE.INPUT_KEYBOARD,
                    ki = new()
                    {
                        wVk = 0,
                        wScan = scanCode,
                        time = 0,
                        dwFlags = User32.KEYEVENTF.KEYEVENTF_SCANCODE,
                    }
                }
            ];

        Dispatcher.UIThread.Post(() =>
        {
            if (User32.SendInput(inputs, sizeof(User32.INPUT)) > 0)
            {
                PressedKeys[key] = true;
                Log($"按下键: {key}，扫描码: 0x{scanCode:X2}");
                result = true;
            }
        });

        // 发送失败
        return result;
    }

    /// <summary>
    /// 释放一个键
    /// </summary>
    /// <param name="key">要释放的虚拟键码</param>
    /// <returns>是否成功释放</returns>
    public static unsafe bool ReleaseKey(User32.VK key)
    {
        var result = false;

        // 如果窗口为前台活动窗口，才发送按键事件
        if ((User32.GetForegroundWindow() != WindowHandle)) return false;

        // 无效的方向键
        if (key == User32.VK.VK_OEM_CLEAR) return false;

        // 方法1: 使用 SendInput 发送 KEYUP 事件
        var scanCode = (ushort)User32.MapVirtualKey((uint)key, 0);
        User32.INPUT[] inputs = [
                new()
                {
                    type = User32.INPUTTYPE.INPUT_KEYBOARD,
                    ki = new()
                    {
                        wVk = 0,
                        wScan = scanCode,
                        time = 0,
                        dwFlags = User32.KEYEVENTF.KEYEVENTF_KEYUP | User32.KEYEVENTF.KEYEVENTF_SCANCODE,
                    }
                }
            ];


        Dispatcher.UIThread.Post(() =>
        {
            if (User32.SendInput(inputs, sizeof(User32.INPUT)) > 0)
            {
                PressedKeys.TryRemove(key, out _);
                Log($"释放键: {key}，扫描码: 0x{scanCode:X2}");
                result = true;
            }
        });

        // 释放失败
        return result;
    }

    /// <summary>
    /// 检查键是否被按下
    /// </summary>
    /// <param name="key">要检查的虚拟键码</param>
    /// <returns>键是否被按下</returns>
    public static bool IsKeyPressed(User32.VK key) => PressedKeys.ContainsKey(key);

    /// <summary>
    /// 释放所有按下的键
    /// </summary>
    public static async Task ReleaseAllKeys(CancellationToken token)
    {
        CurrentDirection = TasDirection.Current;
        //foreach (var key in PressedKeys.Keys.ToList()) ReleaseKey(key);

        ReleaseKey(User32.VK.VK_RETURN);
        ReleaseKey(User32.VK.VK_SPACE);
        await Delay(1, token);
        ReleaseKey(User32.VK.VK_LCONTROL);
        ReleaseKey(User32.VK.VK_DOWN);
        await Delay(1, token);
        ReleaseKey(User32.VK.VK_LEFT);
        ReleaseKey(User32.VK.VK_UP);
        await Delay(1, token);
        ReleaseKey(User32.VK.VK_RIGHT);
        PressedKeys = [];
    }
}

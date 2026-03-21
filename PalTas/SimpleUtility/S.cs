using System;
using System.Threading;

namespace PalTas.SimpleUtility;

/// <summary>
/// S 类名称的全写为 Safely，
/// 其旨在为程序提供"安全的工具"。
/// </summary>
public static class S
{
    /// <summary>
    /// 进程暂停，接受用户输入文本
    /// </summary>
    public static void Pause() => Console.ReadLine();

    /// <summary>
    /// 输出调试信息到控制台
    /// </summary>
    /// <param name="text">欲输出的内容</param>
    public static void Log(string? text = null) => Console.WriteLine(text ?? string.Empty);

    /// <summary>
    /// 延迟一段时间，避免 CPU 使用率过高
    /// </summary>
    /// <param name="millisecondsTimeout">欲延迟的毫秒数</param>
    public static void Sleep(int millisecondsTimeout = 1)
    {
        Thread.Sleep(millisecondsTimeout);
    }
}

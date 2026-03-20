using Vanara.PInvoke;

[assembly: System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations | System.Diagnostics.DebuggableAttribute.DebuggingModes.Default)]
namespace PalTas;

public static class Program
{
    /// <summary>
    /// 初始化全局数据
    /// </summary>
    public static void Init()
    {
        TasGlobal.Init();
        TasScript.InitScriptWalkPlans();
    }

    /// <summary>
    /// 销毁全局数据
    /// </summary>  
    public static void Free()
    {
        TasMemory.FreeProcessHandle();
    }

    /// <summary>
    /// 异步自动跳过对话
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessSkipDialogue(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 检查是否有对话显示到了屏幕
            if (GetCurrentDialogueLineId() > 0)
            {
                //ReleaseAllKeys();       // 避免卡键
                {
                    // 回车
                    PressKey(User32.VK.VK_RETURN);
                    await Delay(1, token);
                    ReleaseKey(User32.VK.VK_RETURN);

                    // 空格
                    PressKey(User32.VK.VK_SPACE);
                    await Delay(1, token);
                    ReleaseKey(User32.VK.VK_SPACE);
                }
                //ReleaseAllKeys();       // 避免卡键

                await Delay(1, token);
            }
            else await Delay(1, token);
        }
    }

    /// <summary>
    /// 异步主脚本循环
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessTasMainLoop(CancellationToken token)
    {
        //return;
        while (!token.IsCancellationRequested)
        {
            await Delay(1, token);

            if (IsInBattle)
                //执行自动战斗脚本
                TasScript.RunBattle();
            else if (!TeamWalkPlanEnd)
            {
                // 自动行走
                var walkPath = CurrentWalkPlan[CurrentWalkStep];

                if ((walkPath.SceneId == -1) || (GetCurrentSceneId() == walkPath.SceneId))
                {
                    if (TeamWalkTo(walkPath.Pos))
                    {
                        // 设置下一个坐标
                        CurrentWalkStep++;
                    }

                    // 检查行进路径是否结束
                    TeamWalkPlanEnd = (CurrentWalkStep >= CurrentWalkPlan.Length);
                }
            }
            else
                // 执行一帧场景脚本
                TasScript.Run();
        }
    }

    /// <summary>
    /// 程序入口
    /// </summary>
    /// <param name="args">暂时用不到</param>
    public static async Task Main(string[] args)
    {
        WinMm.timeBeginPeriod(2);
        Init();

        // 等待游戏正式开始
        WaitGameOfficialStart();

        // 关闭战斗随机数
        DisableRandom();

        // 设置总是会触发暴击
        SetAlwaysCriticalHit();

        // 布置后台任务
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        Task processTasMainLoop = Task.Run(() => ProcessTasMainLoop(token));
        Task processSkipDialogue = Task.Run(() => ProcessSkipDialogue(token));
        Task initGlobalSceneEvent = Task.Run(() => TasScript.InitGlobalSceneEventAsync(token));

        // 等待所有任务完成清理（超时可选）
        await Task.WhenAll(processTasMainLoop, processSkipDialogue, initGlobalSceneEvent);

        // 通知所有后台任务停止
        cts.Cancel();

        Free();
        WinMm.timeEndPeriod(2);
    }
}

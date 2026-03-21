using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;

[assembly: System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations | System.Diagnostics.DebuggableAttribute.DebuggingModes.Default)]
namespace PalTas;

public static class TasMain
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
            await Delay(1, token);

            // 检查是否有对话显示到了屏幕
            if (GetCurrentDialogueLineId() > 0)
            {
                // 设置对话逐字输出延迟为最小
                SetDialogueOutputDelayToMin();
                CurrentDirection = TasDirection.Current;
                PressedKeys = [];

                //ReleaseAllKeys();       // 避免卡键
                {
                    PressKey(User32.VK.VK_RETURN);
                    await Delay(1, token);
                    ReleaseKey(User32.VK.VK_RETURN);

                    PressKey(User32.VK.VK_SPACE);
                    await Delay(1, token);
                    ReleaseKey(User32.VK.VK_LCONTROL);
                }
                //ReleaseAllKeys();       // 避免卡键
            }
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
                await TasScript.RunBattle(token);
            else if (!TeamWalkPlanEnd)
            {
                // 自动行走
                var walkPath = CurrentWalkPlan[CurrentWalkStep];

                if ((walkPath.SceneId == -1) || (GetCurrentSceneId() == walkPath.SceneId))
                {
                    if (TeamWalkTo(walkPath.Pos))
                    {
                        if (walkPath.Direction != TasDirection.Current)
                        {
                            // 先停下
                            TeamStopWalk();

                            // 领队应该改变面朝方向
                            SetLeaderDirection(walkPath.Direction);
                        }

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

    // 从 ntdll.dll 导入 NtSetTimerResolution
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int NtSetTimerResolution(
        uint DesiredResolution,
        bool SetResolution,
        out uint CurrentResolution
    );

    /// <summary>
    /// 程序入口
    /// </summary>
    /// <param name="args">暂时用不到</param>
    public static async Task Main(string[] args)
    {
        WinMm.timeBeginPeriod(1);
        NtSetTimerResolution(1, true, out _);
        {
            Init();

            // 等待游戏正式开始
            WaitGameOfficialStart();

            // 关闭战斗随机数
            DisableRandom();

            // 设置总是会触发暴击
            SetAlwaysCriticalHit();

#if DEBUG
            //TasScript.Progress = TasScript.TasProgress.见石碑篇_初登岛_过迷阵;
            //TasScript.SubStageId = 3;
#endif // DEBUG

            // 布置后台任务
            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            Task processPalWindowStatus = Task.Run(() => ProcessPalWindowStatus(token));
            Task processTasMainLoop = Task.Run(() => ProcessTasMainLoop(token));
            Task processSkipDialogue = Task.Run(() => ProcessSkipDialogue(token));
            Task initGlobalSceneEvent = Task.Run(() => TasScript.InitGlobalSceneEventAsync(token));

            // 等待所有任务完成清理（超时可选）
            await Task.WhenAll(processPalWindowStatus, processTasMainLoop, processSkipDialogue, initGlobalSceneEvent);

            // 通知所有后台任务停止
            cts.Cancel();

            Free();
        }
        NtSetTimerResolution(1, false, out _);
        WinMm.timeEndPeriod(1);
    }
}

using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

//[assembly: System.Diagnostics.Debuggable(System.Diagnostics.DebuggableAttribute.DebuggingModes.DisableOptimizations | System.Diagnostics.DebuggableAttribute.DebuggingModes.Default)]
namespace PalTas.TasCore;

public static class TasMain
{
    /// <summary>
    /// Tas 核心当前运行状态
    /// </summary>
    public static bool IsRunning { get; set; }
    public static bool NeedExit { get; set; }

    /// <summary>
    /// 是否需要滑步取物
    /// </summary>
    static volatile bool NeedPreInput;
    //public static bool NeedPreInput
    //{
    //    get => _needPreInput;
    //    set => _needPreInput = value;
    //}

    /// <summary>
    /// Tas 异步任务管理器
    /// </summary>
    static CancellationTokenSource TasToken { get; set; } = null!;

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
            // 延迟一秒，避免 CPU 使用率过高
            await Delay(1, token);

            // 检查是否有对话显示到了屏幕
            var haveDialogue = GetCurrentDialogueLineId() > 0;
            if (haveDialogue || NeedPreInput)
            {
                if (haveDialogue)
                    // 设置对话逐字输出延迟为最小
                    SetDialogueOutputDelayToMin();

                // 按下触发键
                PressKey(VK.VK_RETURN);
                await Delay(1, token);
                ReleaseKey(VK.VK_RETURN);

                PressKey(VK.VK_SPACE);
                await Delay(1, token);
                ReleaseKey(VK.VK_SPACE);

                //PressKey(VK.VK_LCONTROL);
                //await Delay(1, token);
                //ReleaseKey(VK.VK_LCONTROL);

                if (haveDialogue)
                    // 避免卡键
                    await ReleaseAllKeys(token);
            }
        }
    }

    /// <summary>
    /// 异步滑步取物（不成熟的实现方式）
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessCheckSearch(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
            // 检查是否滑步取物是否完成
            if (GetCurrentDialogueLineId() > 0)
                // 滑步取物完成
                NeedPreInput = false;
    }

    /// <summary>
    /// 自动使用道具
    /// </summary>
    /// <param name="itemId">道具编号</param>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    public static async Task UseInventoryItem(TasItems itemId, CancellationToken token)
    {
        // 检查是否需要使用道具
        if (itemId != TasItems.NULL)
        {
            // 获取道具在道具列表哪个位置
            var currsorId = GetItemIdInInventory(itemId);

            if (currsorId == -1)
                // 未找到道具
                return;

            // 将光标设置到此处
            SetInventoryItemCursorId(currsorId);

            // 使用道具
            while (TasMemory.ReadUInt16(0x0019FC57) != 0x000F)
            {
                PressKey(VK.VK_E);
                await Delay(1, token);
                ReleaseKey(VK.VK_E);
                await Delay(1, token);
            }

            while (TasMemory.ReadUInt16(0x0019FC57) == 0x000F)
            {
                PressKey(VK.VK_RETURN);
                await Delay(1, token);
                ReleaseKey(VK.VK_RETURN);
                await Delay(1, token);
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
            // 延迟一秒，避免 CPU 使用率过高
            await Delay(1, token);

            if (IsInBattle)
                // 执行自动战斗
                await TasScript.RunBattle(token);
            else if (TasScript.BattleIsRunning)
            {
                // 标记战斗彻底结束
                TasScript.SetBattleStatus(false);

                // 战斗结束，跳过结算界面
                while (TasMemory.ReadUInt16(0x0019FBCC) != 0xFFFF)
                {
                    PressKey(VK.VK_RETURN);
                    await Delay(10, token);
                    ReleaseKey(VK.VK_RETURN);

                    PressKey(VK.VK_SPACE);
                    await Delay(10, token);
                    ReleaseKey(VK.VK_SPACE);
                }
            }
            //else if (!TeamWalkPlanEnd)
            //{
            //    // 自动行走
            //    var walkPath = CurrentWalkPlan[CurrentWalkStep];

            //    if ((walkPath.SceneId == -1) || (GetCurrentSceneId() == walkPath.SceneId))
            //    {
            //        NeedPreInput = CurrentWalkPlan[CurrentWalkStep].NeedPreInput;
            //        if (TeamWalkTo(walkPath.Pos))
            //        {
            //            if (walkPath.Direction != TasDirection.Current)
            //                // 领队应该改变面朝方向
            //                TeamWalkOneStep(walkPath.Direction);

            //            // 检查是否需要滑步取物
            //            while (NeedPreInput) await Task.Yield();

            //            // 检查是否需要使用道具
            //            await UseInventoryItem(CurrentWalkPlan[CurrentWalkStep].NeedUseItemId, token);

            //            // 先停下
            //            TeamStopWalk();

            //            // 设置下一个坐标
            //            CurrentWalkStep++;
            //        }

            //        // 检查行进路径是否结束
            //        TeamWalkPlanEnd = (CurrentWalkStep >= CurrentWalkPlan.Length);
            //    }
            //}
            //else
            //    // 执行一帧场景脚本
            //    await TasScript.Run(token);
        }
    }

    /// <summary>
    /// [ntdll.dll] 设置线程当前线程计时器最小精度，最小极限为 0.499ms
    /// </summary>
    /// <param name="DesiredResolution">期望的精度</param>
    /// <param name="SetResolution">ture为设置，false为还原</param>
    /// <param name="CurrentResolution">设置/还原后的当前精度</param>
    /// <returns>操作是否成功</returns>
    [DllImport("ntdll.dll", SetLastError = true)]
    private static extern int NtSetTimerResolution(uint DesiredResolution, bool SetResolution, out uint CurrentResolution);

    /// <summary>
    /// Tas 程序核心入口
    /// </summary>
    public static async Task TasCoreMain()
    {
        IsRunning = true;       // 将 Tas 核心的状态标记为已正在运行
        {
            WinMm.timeBeginPeriod(1);
            NtSetTimerResolution(1, true, out _);
            {
                Init();

                // 等待游戏正式开始
                if (!await WaitGameOfficialStart()) goto Exit;

                // 初始化自动化战斗模块
                TasScript.InitBattle();

                TasScript.Progress = TasScript.TasProgress.赤鬼王测试;
#if DEBUG
                //TasScript.Progress = TasScript.TasProgress.见石碑篇_初登岛_过草妖;
                //TasScript.SubStageId = 1;
#endif // DEBUG

                // 布置后台任务
                TasToken ??= new();
                var token = TasToken.Token;

                Task processPalWindowStatus = Task.Run(() => ProcessPalWindowStatus(token));
                Task processTasMainLoop = Task.Run(() => ProcessTasMainLoop(token));
                Task processSkipDialogue = Task.Run(() => ProcessSkipDialogue(token));
                Task processCheckSearch = Task.Run(() => ProcessCheckSearch(token));
                //Task initGlobalSceneEvent = Task.Run(() => TasScript.InitGlobalSceneEventAsync(token));

                // 等待所有任务完成清理（超时可选）
                await Task.WhenAll(processPalWindowStatus, processTasMainLoop, processSkipDialogue, processCheckSearch);

            Exit:
                // 释放全局资源
                Free();
            }
            NtSetTimerResolution(1, false, out _);
            WinMm.timeEndPeriod(1);
        }
        IsRunning = false;      // 将 Tas 核心的状态标记为已被关闭
    }

    /// <summary>
    /// 关闭 Tas 核心
    /// </summary>
    public static void Exit()
    {
        // 标记为需要退出
        NeedExit = true;

        // 通知所有后台任务停止
        TasToken?.Cancel();
    }
}

using Vanara.PInvoke;

namespace PalTas;

public static class Program
{
    /// <summary>
    /// 初始化全局数据
    /// </summary>
    public static void Init()
    {
        TasGlobal.Init();
    }

    /// <summary>
    /// 销毁全局数据
    /// </summary>
    public static void Free()
    {
        TasMemory.FreeProcessHandle();
    }

    /// <summary>
    /// 异步自动使用道具
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessUseItem(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 检查是否需要使用道具
            if (InventoryItemId != TasItems.NULL)
            {
                // 检索指定道具在道具列表中的索引
                var cursorId = GetItemIdInInventory(InventoryItemId);

                if (cursorId > 0)
                {
                    // 开始使用道具
                    SetInventoryItemCursorId(cursorId);

                    PressKey(User32.VK.VK_E);

                    await Task.Delay(1, token);

                    // 按下确认键
                    PressKey(User32.VK.VK_RETURN);

                    await Task.Delay(10, token);

                    // 释放按键，完成操作
                    ReleaseKey(User32.VK.VK_E);
                    ReleaseKey(User32.VK.VK_RETURN);

                    await Task.Delay(1, token);
                }
            }
            else await Task.Delay(1, token);
        }
    }

    /// <summary>
    /// 异步主动与 NPC 互动
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessSearchEvent(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            // 检查是否需要互动
            if (NeedSearchEvent)
            {
                // 按下回车
                PressKey(User32.VK.VK_RETURN);

                await Task.Delay(1, token);

                // 按下空格
                PressKey(User32.VK.VK_SPACE);

                await Task.Delay(1, token);

                // 按下左 CTRL
                PressKey(User32.VK.VK_LCONTROL);

                await Task.Delay(4, token);

                // 释放按键，完成操作
                ReleaseKey(User32.VK.VK_RETURN);
                ReleaseKey(User32.VK.VK_SPACE);
                ReleaseKey(User32.VK.VK_LCONTROL);

                await Task.Delay(5, token);
            }
            else await Task.Delay(1, token);
        }
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
                // 事件触发成功了
                NeedSearchEvent = false;
                InventoryItemId = TasItems.NULL;

                // 按下回车
                PressKey(User32.VK.VK_RETURN);
                await Task.Delay(1, token);

                // 按下空格
                PressKey(User32.VK.VK_SPACE);
                await Task.Delay(1, token);

                // 按下左 CTRL
                PressKey(User32.VK.VK_LCONTROL);
                //await Task.Delay(4, token);
                await Task.Delay(1, token);

                // 释放按键，完成操作
                ReleaseKey(User32.VK.VK_RETURN);
                ReleaseKey(User32.VK.VK_SPACE);
                ReleaseKey(User32.VK.VK_LCONTROL);
                await Task.Delay(3, token);
            }
            else await Task.Delay(1, token);
        }
    }

    /// <summary>
    /// 异步主脚本循环
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    static async Task ProcessTasMainLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token);

            if (!TeamWalkPlanEnd)
            {
                var walkPath = CurrentWalkPlan[CurrentWalkStep];
                NeedSearchEvent = walkPath.NeedPreInput;

                if ((walkPath.SceneId == -1) || (GetCurrentSceneId() == walkPath.SceneId))
                {
                    // 自动行走
                    if (TeamWalkTo(walkPath.Pos))
                    {
                        // 设置下一个坐标
                        CurrentWalkStep++;

                        NeedSearchEvent = walkPath.NeedPreInput;
                    }

                    // 检查行进路径是否结束
                    TeamWalkPlanEnd = (CurrentWalkStep >= CurrentWalkPlan.Length);
                }
            }
            else
                // 执行一帧的脚本
                TasScript.Run();
        }
    }

    /// <summary>
    /// 程序入口
    /// </summary>
    /// <param name="args">暂时用不到</param>
    public static async Task Main(string[] args)
    {
        Init();

        // 等待游戏正式开始
        WaitGameOfficialStart();

        // 布置后台任务
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        Task processTasMainLoop = Task.Run(() => ProcessTasMainLoop(token));
        Task processSkipDialogue = Task.Run(() => ProcessSkipDialogue(token));
        Task initGlobalSceneEvent = Task.Run(() => TasScript.InitGlobalSceneEventAsync(token));
        Task processUseItem = Task.Run(() => ProcessUseItem(token));
        //Task processSearchEvent = Task.Run(() => ProcessSearchEvent(token));

        // 等待所有任务完成清理（超时可选）
        await Task.WhenAll(processTasMainLoop, processSkipDialogue, initGlobalSceneEvent, processUseItem);

        // 通知所有后台任务停止
        cts.Cancel();

        Free();
    }
}

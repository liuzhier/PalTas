using PalTas.Records;
using Vanara.PInvoke;

namespace PalTas;

public static class TasGlobal
{
    /// <summary>
    /// 当前场景编号
    /// </summary>
    static int CurrentSceneId { get; set; }

    /// <summary>
    /// 队伍当前正在向哪个方向移动
    /// </summary>
    static TasDirection CurrentDirection { get; set; } = TasDirection.Current;

    /// <summary>
    /// 全局行进步数
    /// </summary>
    public static int CurrentWalkStep { get; set; } = 0;

    /// <summary>
    /// 当前行进路径规划
    /// </summary>
    public static TasWalkPath[] CurrentWalkPlan { get; set; } = null!;

    /// <summary>
    /// 队伍是否已经完成了行进路径
    /// </summary>
    public static bool TeamWalkPlanEnd { get; set; } = true;

    /// <summary>
    /// 是否需要触发事件
    /// </summary>
    public static bool NeedSearchEvent { get; set; }

    /// <summary>
    /// 需要使用的道具在库存位置的Id
    /// </summary>
    public static TasItems InventoryItemId { get; set; } = TasItems.NULL;

    /// <summary>
    /// 初始化全局数据
    /// </summary>
    public static void Init()
    {
        TasWindow.Init();
    }

    /// <summary>
    /// 检查指定场景编号是否为当前场景
    /// </summary>
    /// <param name="sceneId">场景编号</param>
    /// <returns>是否为当前场景</returns>
    public static bool IsCurrentScene(int sceneId) => (sceneId == -1) || (sceneId == GetCurrentSceneId());

    /// <summary>
    /// 检查游戏是否正式开始
    /// </summary>
    /// <returns>游戏是否正式开始</returns>
    public static void WaitGameOfficialStart()
    {
        while ((CurrentSceneId == 0) || (CurrentSceneId == 0xFFFF)) CheckSceneSwitched();
    }

    /// <summary>
    /// 检查场景是否切换了
    /// </summary>
    /// <returns>场景是否切换了</returns>
    public static bool CheckSceneSwitched()
    {
        var newScene = GetCurrentSceneId();
        var switched = (CurrentSceneId != newScene);

        if (switched) CurrentSceneId = newScene;
        return switched;
    }

    /// <summary>
    /// 根据要行走的方向计算应该按哪个键
    /// </summary>
    /// <param name="direction">方向</param>
    /// <returns>应该按哪个键</returns>
    public static User32.VK GetDirectionKey(TasDirection direction) => direction switch
    {
        TasDirection.Down => User32.VK.VK_DOWN,
        TasDirection.Left => User32.VK.VK_LEFT,
        TasDirection.Up => User32.VK.VK_UP,
        TasDirection.Right => User32.VK.VK_RIGHT,
        _ => User32.VK.VK_OEM_CLEAR,
    };

    public static void TeamStopWalk()
    {
        // 释放当前方向键
        ReleaseKey(GetDirectionKey(CurrentDirection));
        CurrentDirection = TasDirection.Current;
    }

    /// <summary>
    /// 队伍走一步
    /// </summary>
    public static void TeamWalkOneStep(TasDirection direction)
    {
        var key = GetDirectionKey(direction);

        if (!PressedKeys.ContainsKey(key) || (CurrentDirection != direction))
        {
            // 释放当前方向键
            TeamStopWalk();
            CurrentDirection = direction;

            // 按下对应方向键
            PressKey(key);
        }

        // 按下对应方向键
        //PressKey(GetDirectionKey(CurrentDirection));
    }

    public static bool CheckTeamArrivedDestination(RPos destination)
    {
        var current = GetLeaderActualPosition();

        //return ((current.X == destination.X) && (current.Y == destination.Y)) || CheckSceneSwitched();
        return ((current.X == destination.X) && (current.Y == destination.Y));
    }

    /// <summary>
    /// 队伍走到指定坐标
    /// </summary>
    /// <param name="targetX">目的地的 X</param>
    /// <param name="targetY">目的地的 Y</param>
    /// <returns>是否达到了目的地</returns>
    public static bool TeamWalkTo(RPos destination)
    {
        var current = GetLeaderActualPosition();
        var arrived = CheckTeamArrivedDestination(destination);
        if (arrived) goto End;

        var direction = TasDirection.Current;
        if (destination.Y < current.Y) direction = ((destination.X < current.X) ? TasDirection.Left : TasDirection.Up);
        else direction = ((destination.X < current.X) ? TasDirection.Down : TasDirection.Right);
        TeamWalkOneStep(direction);

        arrived = CheckTeamArrivedDestination(destination);
    End:
        if (arrived) TeamStopWalk();        // 队伍停止前进
        return arrived;
    }

    /// <summary>
    /// 设置要行进规划
    /// </summary>
    public static void SetWalkPlanning(TasWalkPath[] walkPlan)
    {
        CheckSceneSwitched();
        TeamWalkPlanEnd = false;
        CurrentWalkStep = 0;
        CurrentWalkPlan = walkPlan;
    }

    /// <summary>
    /// 检查道具列表是否有某道具
    /// </summary>
    /// <param name="itemId">道具编号</param>
    /// <param name="num">数量</param>
    /// <returns>是否有某道具</returns>
    public static bool CheckInventoryItem(TasItems itemId, int num = 1)
    {
        var count = 0;
        var item = (ushort)itemId;

        for (var i = 0; i < MAX_INVENTORY; i++)
        {
            var inventoryItem = GetInventoryItem(i);
            if (inventoryItem.ItemId == item)
            {
                count += inventoryItem.Count;
                if (count >= num) return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 检索道具在道具列表中的索引（位置）
    /// </summary>
    /// <param name="itemId">道具编号</param>
    /// <returns>在道具列表中的索引（位置），未找到则返回 -1</returns>
    public static short GetItemIdInInventory(TasItems itemId)
    {
        var item = (ushort)itemId;

        for (var i = (short)0; i < MAX_INVENTORY; i++)
        {
            var inventoryItem = GetInventoryItem(i);
            if ((inventoryItem.ItemId == item) && (inventoryItem.Count > 0))
                return i;
        }

        return -1;
    }
}

using PalTas.Records;
using static PalTas.Records.Core;

namespace PalTas;

public static unsafe class TasData
{
    /// <summary>
    /// 获取当前对话行数
    /// </summary>
    /// <param name="needReadMemory">是否需要读取内存</param>
    /// <returns>当前对话行数</returns>
    public static byte GetCurrentDialogueLineId() => TasMemory.ReadByte(TasMemory.CurrentDialogueLineIdAddr);

    /// <summary>
    /// 获取当前场景编号
    /// </summary>
    /// <param name="needReadMemory">是否需要读取内存</param>
    /// <returns>当前场景编号</returns>
    public static ushort GetCurrentSceneId() => TasMemory.ReadUInt16(TasMemory.CurrentSceneIdAddr);

    /// <summary>
    /// 获取指定场景
    /// </summary>
    /// <param name="sceneId">场景编号，缺省则为当前场景</param>
    /// <returns>返回指定场景</returns>
    public static RScene GetScene(int sceneId = -1)
    {
        var sceneAddr = TasMemory.ReadUInt32(TasMemory.SceneAddr);
        if (sceneId == -1) sceneId = GetCurrentSceneId();
        sceneId--;
        var baseOffset = sceneAddr + (uint)(sceneId * sizeof(RScene));

        return new()
        {
            MapId = TasMemory.ReadUInt16(baseOffset),
            ScriptOnEnter = TasMemory.ReadUInt16(baseOffset, 2),
            ScriptOnTeleport = TasMemory.ReadUInt16(baseOffset, 4),
            EventObjectIndex = TasMemory.ReadUInt16(baseOffset, 6),
        };
    }

    /// <summary>
    /// 获取指定事件数据
    /// </summary>
    /// <param name="sceneId">场景编号</param>
    /// <param name="eventId">事件编号</param>
    /// <returns>指定事件数据</returns>
    public static REvent GetEvent(int sceneId, TasScript.SceneEvent eventId)
    {
        var isCurrentScene = ((sceneId == -1) || (sceneId == GetCurrentSceneId()));
        //var isCurrentScene = ((sceneId == -1));
        var eventAddr = TasMemory.ReadUInt32(isCurrentScene ? TasMemory.CurrentSceneEventAddr : TasMemory.EventAddr);
        if (!isCurrentScene)
        {
            eventId += GetScene(sceneId).EventObjectIndex;
            eventId--;
        }
        var baseOffset = eventAddr + (uint)((int)eventId * sizeof(REvent));

        return new()
        {
            VanishTime = TasMemory.ReadInt16(baseOffset),
            X = TasMemory.ReadInt16(baseOffset, 2),
            Y = TasMemory.ReadInt16(baseOffset, 4),
            Layer = TasMemory.ReadInt16(baseOffset, 6),
            TriggerScript = TasMemory.ReadUInt16(baseOffset, 8),
            AutoScript = TasMemory.ReadUInt16(baseOffset, 10),
            State = (EventState)TasMemory.ReadUInt16(baseOffset, 12),
            TriggerMode = (EventTriggerMode)TasMemory.ReadUInt16(baseOffset, 14),
            SpriteId = TasMemory.ReadUInt16(baseOffset, 16),
            FramesPerDirection = TasMemory.ReadUInt16(baseOffset, 18),
            Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, 20),
            CurrentFrameId = TasMemory.ReadUInt16(baseOffset, 22),
            TriggerIdleFrame = TasMemory.ReadUInt16(baseOffset, 24),
            AutoIdleFrame = TasMemory.ReadUInt16(baseOffset, 26),
        };
    }

    /// <summary>
    /// 获取当前场景的事件数据
    /// </summary>
    /// <param name="eventId">事件编号</param>
    /// <returns>指定事件数据</returns>
    public static REvent GetCurrentSceneEvent(TasScript.SceneEvent eventId) => GetEvent(-1, eventId);

    /// <summary>
    /// 获取队员相对视口的步伐数据
    /// </summary>
    /// <param name="memberId">队员编号</param>
    /// <returns>步伐数据</returns>
    public static RMemberTrailRelativeToViewport GetMemberTrailRelativeToViewport(int memberId)
    {
        if (memberId < 0 || memberId > (MAX_MEMBER_COUNT - 1)) return default;

        var baseOffset = TasMemory.MemberTrailRelativeToViewportAddr + (uint)(memberId * sizeof(RMemberTrailRelativeToViewport));
        return new()
        {
            HeroId = TasMemory.ReadUInt16(baseOffset),
            RelativeTrail = new()
            {
                Pos = new()
                {
                    X = TasMemory.ReadInt16(baseOffset, 2),
                    Y = TasMemory.ReadInt16(baseOffset, 4),
                },
                Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, 6),
            },
            FrameOffset = TasMemory.ReadUInt16(baseOffset, 8)
        };
    }

    /// <summary>
    /// 获取队员相对视口的步伐数据
    /// </summary>
    /// <param name="memberId">队员编号</param>
    /// <returns>步伐数据</returns>
    public static RMemberTrail GetMemberTrail(int memberId)
    {
        if (memberId < 0 || memberId > (MAX_MEMBER_COUNT - 1)) return default;

        var memberTrailAddr = TasMemory.ReadUInt32(TasMemory.MemberTrailAddr);
        var baseOffset = memberTrailAddr + (uint)(memberId * sizeof(RMemberTrail));

        return new()
        {
            Pos = new()
            {
                X = TasMemory.ReadInt16(baseOffset),
                Y = TasMemory.ReadInt16(baseOffset, 2),
            },
            Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, 4),
        };
    }

    /// <summary>
    /// 获取领队者坐标
    /// </summary>
    /// <returns>(X, Y) 坐标</returns>
    public static RPos GetLeaderPosition()
    {
        var pos = GetMemberTrail(0).Pos;
        return new()
        {
            X = pos.X,
            Y = pos.Y,
        };
    }

    /// <summary>
    /// 获取视口的坐标
    /// </summary>
    /// <returns>(X, Y) 坐标</returns>
    public static RPos GetViewportPosition() => new ()
    {
        X = TasMemory.ReadInt16(TasMemory.ViewportPosAddr),
        Y = TasMemory.ReadInt16(TasMemory.ViewportPosAddr, 2),
    };

    /// <summary>
    /// 获取队员相对视口的坐标
    /// </summary>
    /// <returns>(X, Y) 坐标</returns>
    public static RPos GetLeaderPositionRelativeToViewport()
    {
        var pos = GetMemberTrailRelativeToViewport(0).RelativeTrail.Pos;
        return new()
        {
            X = pos.X,
            Y = pos.Y,
        };
    }

    /// <summary>
    /// 获取领队者坐标
    /// </summary>
    /// <returns>(X, Y) 坐标</returns>
    public static RPos GetLeaderActualPosition()
    {
        var pos = GetViewportPosition();
        pos.X += 160;
        pos.Y += 112;
        return new()
        {
            X = pos.X,
            Y = pos.Y,
        };
    }

    /// <summary>
    /// 获取道具列表种指定道具
    /// </summary>
    /// <param name="inventoryItemId">道具列表的子项编号</param>
    /// <returns>指定道具</returns>
    public static RInventory GetInventoryItem(int inventoryItemId)
    {
        if (inventoryItemId < 0 || inventoryItemId > (MAX_INVENTORY - 1)) return default;

        var inventoryAddr = TasMemory.ReadUInt32(TasMemory.InventoryAddr);
        var baseOffset = inventoryAddr + (uint)(inventoryItemId * sizeof(RInventory));
        return new()
        {
            ItemId = TasMemory.ReadUInt16(baseOffset),
            Count = TasMemory.ReadUInt16(baseOffset, 2),
            DeductionCount = TasMemory.ReadUInt16(baseOffset, 4)
        };
    }

    /// <summary>
    /// 获取道具列表光标位置
    /// </summary>
    /// <returns>光标位置</returns>
    public static int GetInventoryItemCursorId()
    {
        var inventoryItemIdAddr = TasMemory.ReadUInt16(TasMemory.InventoryItemIdAddr);
        return TasMemory.ReadUInt16(inventoryItemIdAddr);
    }

    /// <summary>
    /// 设置道具列表光标位置
    /// </summary>
    public static void SetInventoryItemCursorId(short inventoryItemCursorId)
    {
        var inventoryItemIdAddr = TasMemory.ReadInt16(TasMemory.InventoryItemIdAddr);
        TasMemory.WriteInt16(TasMemory.InventoryItemIdAddr, inventoryItemCursorId);
    }

    /// <summary>
    /// 设置事件触发方式
    /// </summary>
    /// <param name="sceneId">场景编号，-1 则为当前场景</param>
    /// <param name="eventId">事件编号</param>
    /// <param name="triggerMode">触发方式</param>
    public static void SetEventTriggerMode(int sceneId, TasScript.SceneEvent eventId, EventTriggerMode triggerMode)
    {
        var isCurrentScene = ((sceneId == -1) || (sceneId == GetCurrentSceneId()));
        //var isCurrentScene = ((sceneId == -1));
        var eventAddr = TasMemory.ReadUInt32(isCurrentScene ? TasMemory.CurrentSceneEventAddr : TasMemory.EventAddr);
        if (!isCurrentScene)
        {
            eventId += GetScene(sceneId).EventObjectIndex;
            eventId--;
        }
        var baseOffset = eventAddr + (uint)((int)eventId * sizeof(REvent));

        TasMemory.WriteUInt16(baseOffset, (ushort)triggerMode, 14);
    }

    /// <summary>
    /// 设置当前场景的事件触发方式
    /// </summary>
    /// <param name="eventId">事件编号</param>
    /// <param name="triggerMode">触发方式</param>
    public static void SetCurrentSceneEventTriggerMode(TasScript.SceneEvent eventId, EventTriggerMode triggerMode) =>
        SetEventTriggerMode(-1, eventId, triggerMode);

    /// <summary>
    /// 设置事件触发方式
    /// </summary>
    /// <param name="sceneId">场景编号，-1 则为当前场景</param>
    /// <param name="eventId">事件编号</param>
    /// <param name="triggerMode">触发方式</param>
    public static void SetEventTriggerScript(int sceneId, TasScript.SceneEvent eventId, ushort address)
    {
        var isCurrentScene = ((sceneId == -1) || (sceneId == GetCurrentSceneId()));
        //var isCurrentScene = ((sceneId == -1));
        var eventAddr = TasMemory.ReadUInt32(isCurrentScene ? TasMemory.CurrentSceneEventAddr : TasMemory.EventAddr);
        if (!isCurrentScene)
        {
            eventId += GetScene(sceneId).EventObjectIndex;
            eventId--;
        }
        var baseOffset = eventAddr + (uint)((int)eventId * sizeof(REvent));

        TasMemory.WriteUInt16(baseOffset, address, 8);
    }

    /// <summary>
    /// 设置当前场景的事件触发方式
    /// </summary>
    /// <param name="eventId">事件编号</param>
    /// <param name="triggerMode">触发方式</param>
    public static void SetCurrentSceneEventTriggerScript(TasScript.SceneEvent eventId, ushort address) =>
        SetEventTriggerScript(-1, eventId, address);

    /// <summary>
    /// 从库存中删除指定道具，每次删一个
    /// </summary>
    public static void RemoveInventoryItem(TasItems itemId)
    {
        var item = (ushort)itemId;

        for (var i = 0; i < MAX_INVENTORY; i++)
        {
            var inventoryItem = GetInventoryItem(i);
            if (inventoryItem.ItemId == item)
            {
                var count = inventoryItem.Count;
                if (inventoryItem.Count >= 1)
                {
                    var inventoryAddr = TasMemory.ReadUInt32(TasMemory.InventoryAddr);
                    var baseOffset = inventoryAddr + (uint)(i * sizeof(RInventory));
                    TasMemory.WriteUInt16(baseOffset, --count, 2);
                }
            }
        }
    }
}

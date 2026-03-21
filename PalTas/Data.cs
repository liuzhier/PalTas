using PalTas.Records;
using System;
using System.Diagnostics;
using Vanara.PInvoke;
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
    /// 设置事件信息
    /// </summary>
    /// <param name="sceneId">场景编号，-1 则为当前场景</param>
    /// <param name="eventId">事件编号</param>
    /// <param name="event">事件信息</param>
    public static void SetEventInfo(int sceneId, TasScript.SceneEvent eventId, REvent @event)
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

        TasMemory.WriteInt16(baseOffset, @event.VanishTime);
        TasMemory.WriteInt16(baseOffset, @event.X, 2);
        TasMemory.WriteInt16(baseOffset, @event.Y, 4);
        TasMemory.WriteInt16(baseOffset, @event.Layer, 6);
        TasMemory.WriteUInt16(baseOffset, @event.TriggerScript, 8);
        TasMemory.WriteUInt16(baseOffset, @event.AutoScript, 10);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.State, 12);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.TriggerMode, 14);
        TasMemory.WriteUInt16(baseOffset, @event.SpriteId, 16);
        TasMemory.WriteUInt16(baseOffset, @event.FramesPerDirection, 18);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.Direction, 20);
        TasMemory.WriteUInt16(baseOffset, @event.CurrentFrameId, 22);
        TasMemory.WriteUInt16(baseOffset, @event.TriggerIdleFrame, 24);
        TasMemory.WriteUInt16(baseOffset, @event.AutoIdleFrame, 26);
    }

    /// <summary>
    /// 设置当前场景中指定事件的信息
    /// </summary>
    /// <param name="eventId">事件编号</param>
    /// <param name="event">事件信息</param>
    public static void SetCurrentSceneEventInfo(TasScript.SceneEvent eventId, REvent @event) => SetEventInfo(-1, eventId, @event);

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

    /// <summary>
    /// 获取当前敌方队列编号（内存更新的很慢）
    /// </summary>
    public static ushort GetCurrentEnemyTeamId() => TasMemory.ReadUInt16(TasMemory.CurrentEnemyTeamIdAddr);

    /// <summary>
    /// 获取敌方战斗时的临时数据
    /// </summary>
    /// <returns>敌方战斗时的临时数据</returns>
    public static ushort GetEnemyBattleTempData() => TasMemory.ReadUInt16(TasMemory.CurrentEnemyTeamIdAddr);

    /// <summary>
    /// 获取当前是否在战斗中（游戏中共 0x017C 组敌方队伍）
    /// </summary>
    public static bool IsInBattle => TasMemory.ReadByte(TasMemory.IsInBattleAddr) != 0;

    /// <summary>
    /// 将指定地址的指令替换为指定指令字节
    /// </summary>
    /// <param name="handle">进程句柄</param>
    /// <param name="address">进程地址</param>
    /// <param name="commandBytes">机器码字节</param>
    /// <returns>是否成功</returns>
    private static bool PatchInstruction(HPROCESS handle, nint address, byte[] commandBytes)
    {
        // 修改内存保护为可读写执行
        if (!Kernel32.VirtualProtectEx(handle, address, (SizeT)commandBytes.Length, Kernel32.MEM_PROTECTION.PAGE_EXECUTE_READWRITE, out var oldProtect))
        {
            var error = Kernel32.GetLastError();
            Log($"VirtualProtectEx 失败，错误码: {error} (0x{error:X})");
            return false;
        }

        // 写入 NOP 字节
        bool success = Kernel32.WriteProcessMemory(handle, address, commandBytes, (uint)commandBytes.Length, out var _);

        // 恢复原保护属性（可选，但推荐）
        Kernel32.VirtualProtectEx(handle, address, (SizeT)commandBytes.Length, oldProtect, out _);

        return success;
    }

    /// <summary>
    /// 获取 PAL.DLL 模块基地址
    /// </summary>
    /// <returns>PAL.DLL 模块基地址</returns>
    /// <exception cref="Exception">获取游戏进程 Id 失败</exception>
    public static nint GetPalModuleBase()
    {
        // 通过窗口句柄获取进程ID（复用你的逻辑）
        User32.GetWindowThreadProcessId(GetWindowHandle(), out var processId);
        if (processId == 0) throw new Exception("无法获取进程ID");

        // 通过进程ID获取 Process 对象
        using (var process = Process.GetProcessById((int)processId))
        {
            foreach (ProcessModule module in process.Modules)
            {
                if (module.ModuleName.Equals("Pal.dll", StringComparison.OrdinalIgnoreCase))
                    return module.BaseAddress;
            }
        }
        throw new Exception("未找到 Pal.dll 模块");
    }

    /// <summary>
    /// 设置游戏是正常随机，还是是由 Tas 掌控
    /// </summary>
    /// <param name="needRandom">是否启用正常随机</param>
    public static void DisableRandom(bool disable = true)
    {
        var handle = TasMemory.GetProcessHandle();
        if (handle != 0)
        {
            var palDllBase = TasMemory.PalDllAddr;
            Console.WriteLine($"Pal.dll 基址: 0x{palDllBase:X8}");

            // 计算指令实际地址（使用你测得的偏移量）
            var instr1Offset = 0x0002_DF99;     // 第一条指令偏移
            var instr2Offset = 0x0002_E00B;     // 第二条指令偏移
            var instr1Addr = nint.Add((nint)palDllBase, instr1Offset);
            var instr2Addr = nint.Add((nint)palDllBase, instr2Offset);

            var patch1 = (byte[])null!;
            var patch2 = (byte[])null!;

            if (disable)
            {
                // 禁用：NOP 填充（长度分别为5和6）
                patch1 = [0x90, 0x90, 0x90, 0x90, 0x90];
                patch2 = [0x90, 0x90, 0x90, 0x90, 0x90, 0x90];
            }
            else
            {
                var seedBytes = BitConverter.GetBytes(TasMemory.RandomSeedAddr);

                // 恢复第一条指令：mov [seedAddr], eax (A3 + 地址)
                patch1 = new byte[5];
                patch1[0] = 0xA3;
                Buffer.BlockCopy(seedBytes, 0, patch1, 1, 4);

                // 恢复第二条指令：mov [seedAddr], ebx (89 1D + 地址)
                patch2 = new byte[6];
                patch2[0] = 0x89;
                patch2[1] = 0x1D;
                Buffer.BlockCopy(seedBytes, 0, patch2, 2, 4);
            }

            // 补丁第一条指令（长度 5 字节：A3 xx xx xx xx）
            Log(PatchInstruction(handle, instr1Addr, patch1) ? "第一条指令补丁成功" : "第一条指令补丁失败");

            // 补丁第二条指令（长度 6 字节：89 1D xx xx xx xx）
            Log(PatchInstruction(handle, instr2Addr, patch2) ? "第二条指令补丁成功" : "第二条指令补丁失败");
        }
    }

    /// <summary>
    /// 设置总是暴击（李逍遥 6 倍暴击）
    /// </summary>
    public static void SetAlwaysCriticalHit() => TasMemory.WriteUInt32(TasMemory.RandomSeedAddr, 0x0055_5555);

    /// <summary>
    /// 设置总是最低伤害
    /// </summary>
    public static void SetNeverCriticalHit() => TasMemory.WriteUInt32(TasMemory.RandomSeedAddr, 0x008A_AAA7);

    /// <summary>
    /// 设置我方夺魂必中
    /// </summary>
    public static void SetDuoHunAlwaysKill() => TasMemory.WriteUInt32(TasMemory.RandomSeedAddr, 0x0054_5555);

    /// <summary>
    /// 设置对话逐字输出延迟
    /// </summary>
    /// <param name="delay">逐字输出延迟</param>
    public static void SetDialogueOutputDelay(ushort delay) => TasMemory.WriteUInt16(TasMemory.DialogueOutputDelayAddr, delay);

    /// <summary>
    /// 设置最大对话逐字输出延迟
    /// </summary>
    public static void SetDialogueOutputDelayToMax() => SetDialogueOutputDelay(ushort.MaxValue);

    /// <summary>
    /// 设置最小对话逐字输出延迟
    /// </summary>
    public static void SetDialogueOutputDelayToMin() => SetDialogueOutputDelay(ushort.MinValue);

    /// <summary>
    /// 设置队员方向
    /// </summary>
    /// <param name="memberId">队员编号</param>
    /// <param name="direction">方向</param>
    public static void SetLeaderDirection(TasDirection direction) => TasMemory.WriteUInt16(TasMemory.LeaderDirectionAddr, (ushort)direction);

    /// <summary>
    /// 设置队员相对于视口的步伐信息
    /// </summary>
    /// <param name="memberId"></param>
    /// <param name="info"></param>
    //public static void SetMemberTrailRelativeToViewportInfo(int memberId, RMemberTrailRelativeToViewport info)
    //{
    //    if (memberId < 0 || memberId > (MAX_MEMBER_COUNT - 1)) return;

    //    var baseOffset = TasMemory.MemberTrailRelativeToViewportAddr + (uint)(memberId * sizeof(RMemberTrailRelativeToViewport));

    //    ref var relativeTrail = ref info.RelativeTrail;
    //    TasMemory.WriteUInt16(baseOffset, info.HeroId);
    //    TasMemory.WriteInt16(baseOffset, relativeTrail.Pos.X, 2);
    //    TasMemory.WriteInt16(baseOffset, relativeTrail.Pos.Y, 4);
    //    SetMemberDirection(memberId, relativeTrail.Direction);
    //    TasMemory.WriteUInt16(baseOffset, (ushort)relativeTrail.Direction, 6);
    //    TasMemory.WriteUInt16(baseOffset, info.FrameOffset, 8);
    //}

    /// <summary>
    /// 检查所有敌人是否均已阵亡，若全部阵亡则说明战斗结束
    /// </summary>
    public static void CheckBattleEnd()
    {

    }
}

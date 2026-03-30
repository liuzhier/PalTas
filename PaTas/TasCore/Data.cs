using PalTas.TasCore.Records;
using System;
using System.Diagnostics;
using Vanara.PInvoke;
using static PalTas.TasCore.Records.Base;
using static PalTas.TasCore.Records.Core;
using static PalTas.TasCore.Records.Entity;
using static PalTas.TasCore.Records.Game;

namespace PalTas.TasCore;

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
    public static short GetCurrentSceneId() => TasMemory.ReadInt16(TasMemory.CurrentSceneIdAddr);

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
            MapId = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 0),
            ScriptOnEnter = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 1),
            ScriptOnTeleport = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 2),
            EventObjectIndex = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 3),
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
            VanishTime = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 0),
            X = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 1),
            Y = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 2),
            Layer = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 3),
            TriggerScript = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 4),
            AutoScript = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 5),
            State = (EventState)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 6),
            TriggerMode = (EventTriggerMode)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 7),
            SpriteId = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 8),
            FramesPerDirection = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 9),
            Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 10),
            CurrentFrameId = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 11),
            TriggerIdleFrame = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 12),
            AutoIdleFrame = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 13),
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

        var baseAddr = TasMemory.ReadUInt32(TasMemory.MemberTrailRelativeToViewportAddr);
        var baseOffset = baseAddr + (uint)(memberId * sizeof(RMemberTrailRelativeToViewport));
        return new()
        {
            HeroId = (TasHero)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 0),
            RelativeTrail = new()
            {
                Pos = new()
                {
                    X = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 1),
                    Y = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 2),
                },
                Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 3),
            },
            FrameOffset = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 4)
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
                X = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 0),
                Y = TasMemory.ReadInt16(baseOffset, sizeof(ushort) * 1),
            },
            Direction = (TasDirection)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 2),
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
            ItemId = (TasItems)TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 0),
            Count = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 1),
            DeductionCount = TasMemory.ReadUInt16(baseOffset, sizeof(ushort) * 2)
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

        TasMemory.WriteInt16(baseOffset, @event.VanishTime, sizeof(ushort) * 0);
        TasMemory.WriteInt16(baseOffset, @event.X, sizeof(ushort) * 1);
        TasMemory.WriteInt16(baseOffset, @event.Y, sizeof(ushort) * 2);
        TasMemory.WriteInt16(baseOffset, @event.Layer, sizeof(ushort) * 3);
        TasMemory.WriteUInt16(baseOffset, @event.TriggerScript, sizeof(ushort) * 4);
        TasMemory.WriteUInt16(baseOffset, @event.AutoScript, sizeof(ushort) * 5);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.State, sizeof(ushort) * 6);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.TriggerMode, sizeof(ushort) * 7);
        TasMemory.WriteUInt16(baseOffset, @event.SpriteId, sizeof(ushort) * 8);
        TasMemory.WriteUInt16(baseOffset, @event.FramesPerDirection, sizeof(ushort) * 9);
        TasMemory.WriteUInt16(baseOffset, (ushort)@event.Direction, sizeof(ushort) * 10);
        TasMemory.WriteUInt16(baseOffset, @event.CurrentFrameId, sizeof(ushort) * 11);
        TasMemory.WriteUInt16(baseOffset, @event.TriggerIdleFrame, sizeof(ushort) * 12);
        TasMemory.WriteUInt16(baseOffset, @event.AutoIdleFrame, sizeof(ushort) * 13);
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
        for (var i = 0; i < MAX_INVENTORY; i++)
        {
            var inventoryItem = GetInventoryItem(i);
            if (inventoryItem.ItemId == itemId)
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
    /// 获取当前是否在战斗中（游戏中共 0x017C 组敌方队伍）
    /// </summary>
    //public static bool IsInBattle => TasMemory.ReadByte(TasMemory.IsInBattleAddr) != 0;
    public static bool IsInBattle => !CheckBattleEnd();

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
    /// <param name="disable">是否禁用正常随机</param>
    public static void DisableRandom(bool disable = true)
    {
        var handle = TasMemory.GetProcessHandle();
        if (handle != 0)
        {
            var palDllBase = TasMemory.PalDllAddr;
            Console.WriteLine($"Pal.dll 基址: 0x{palDllBase:X8}");

            // 计算指令实际地址
            var instr1Offset = 0x0002_DC90 + 0x0000_0293;     // 指令偏移
            var instr1Addr = nint.Add((nint)palDllBase, instr1Offset);
            var patch = (byte[])null!;

            if (disable)
            {
                patch = [
                    0x83, 0xEC, 0x08,                                   // sub esp,08
                    0xF3, 0x0F, 0x10, 0x05, 0x00, 0x00, 0x00, 0x00,     // movss xmm0,[Pal.DLL+71FCC]  (地址占位 7)
                    0xF3, 0x0F, 0x11, 0x44, 0x24, 0x04,                 // movss [esp+04],xmm0
                    0xC7, 0x04, 0x24, 0xFE, 0xFF, 0x7F, 0x3F,           // mov [esp],3F7FFFFE
                    0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,                 // call dword ptr [Pal.DLL+58544]  (地址占位 26)
                    0xD9, 0x5C, 0x24, 0x04,                             // fstp dword ptr [esp+04]
                    0xC7, 0x04, 0x24, 0x00, 0x00, 0x00, 0x00,           // mov [esp],00000000
                    0xFF, 0x15, 0x00, 0x00, 0x00, 0x00,                 // call dword ptr [Pal.DLL+5853C]  (地址占位 43)
                    0x83, 0xC4,                                         // add esp,08
                    0x08, 0x5D,                                         // pop ebp
                    0xC2, 0x04, 0x00                                    // ret 0004
                ];

                // 填充地址
                Buffer.BlockCopy(BitConverter.GetBytes(TasMemory.RandomResultAddr), 0, patch, 7, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(TasMemory.PalDllAddr + 0x0005_8544), 0, patch, 26, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(TasMemory.PalDllAddr + 0x0005_853C), 0, patch, 43, 4);
            }
            else
            {
                patch = [
                    0x51,                                   // push ecx
                    0x56,                                   // push esi
                    0x8B, 0x35, 0x00, 0x00, 0x00, 0x00,     // mov esi,[Pal.DLL+71E44]  (地址占位 4)
                    0x85, 0xF6,                             // test esi,esi
                    0x75, 0x24,                             // jne +0x24
                    0xE8, 0x1C, 0xFF, 0xFF, 0xFF,           // call Pal.GetAPICallLog+1C0
                    0x84, 0xC0,                             // test al,al
                    0x75, 0x15,                             // jne +0x15
                    0xA1, 0x00, 0x00, 0x00, 0x00,           // mov eax,[Pal.DLL+70B7C]  (地址占位 22)
                    0xFF, 0x75, 0x08,                       // push [ebp+08]
                    0x89, 0x45, 0xFC,                       // mov [ebp-04],eax
                    0xFF, 0x55, 0xFC,                       // call dword ptr [ebp-04]
                    0x5E,                                   // pop esi
                    0x8B, 0xE5,                             // mov esp,ebp
                    0x5D,                                   // pop ebp
                    0xC2, 0x04, 0x00,                       // ret 4
                    0x8B, 0x35, 0x00, 0x00, 0x00, 0x00,     // mov esi,[Pal.DLL+71E44]  (地址占位 44)
                    0x64, 0xA1, 0x2C, 0x00, 0x00, 0x00,     // mov eax,fs:[0x2C]
                ];

                // 填充地址
                var addrPalDll_71E44 =BitConverter.GetBytes(TasMemory.PalDllAddr + 0x0007_1E44);
                Buffer.BlockCopy(addrPalDll_71E44, 0, patch, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(TasMemory.PalDllAddr + 0x0007_0B7C), 0, patch, 22, 4);
                Buffer.BlockCopy(addrPalDll_71E44, 0, patch, 44, 4);
            }

            // 补丁指令
            Log(PatchInstruction(handle, instr1Addr, patch) ? "指令补丁成功" : "指令补丁失败");
        }
    }

    /// <summary>
    /// 设置总是暴击（李逍遥 6 倍暴击）
    /// </summary>
    public static float RandomResult => TasMemory.ReadSingle(TasMemory.RandomResultAddr);

    /// <summary>
    /// 设置总是暴击（李逍遥 6 倍暴击）
    /// </summary>
    public static void SetRandomResult(float randomResult) => TasMemory.WriteSingle(TasMemory.RandomResultAddr, randomResult);

    /// <summary>
    /// 自动设置下一次随机结果
    /// </summary>
    public static void AutoRandomNext() => SetRandomResult(Random.Shared.NextSingle());

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
    /// 检查所有敌人是否均已阵亡，若全部阵亡则说明战斗结束
    /// </summary>
    public static REnemyBattleTempData GetEnemyBattleTempData(TasBattleFighter targetId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.EnemyBattleTempDataAddr) + (uint)(sizeof(REnemyBattleTempData) * (short)targetId);

        return new()
        {
            EnemyBaseDataId = TasMemory.ReadInt16(baseAddr, sizeof(short) * 0),
            Pos = new()
            {
                X = TasMemory.ReadInt16(baseAddr, sizeof(short) * 1),
                Y = TasMemory.ReadInt16(baseAddr, sizeof(short) * 2),
            },
            OriginPos = new()
            {
                X = TasMemory.ReadInt16(baseAddr, sizeof(short) * 3),
                Y = TasMemory.ReadInt16(baseAddr, sizeof(short) * 4),
            },
            CurrentFrameId = TasMemory.ReadInt16(baseAddr, sizeof(short) * 5),
            HP = TasMemory.ReadInt16(baseAddr, sizeof(short) * 9),
            EnemyId = (TasEnemys)TasMemory.ReadUInt16(baseAddr, sizeof(short) * 10),
            Script = new()
            {
                TurnStart = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 11),
                BattleWon = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 12),
                Action = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 13),
            },
        };
    }

    public static RMemberBattleTempData GetMemberBattleTempData(TasBattleFighter fightId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.MemberBattleTempDataAddr) + (uint)(sizeof(REnemyBattleTempData) * (short)fightId);

        return new()
        {
            //SpriteId = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 0),
            //Pos = new()
            //{
            //    X = TasMemory.ReadInt16(baseAddr, sizeof(short) * 1),
            //    Y = TasMemory.ReadInt16(baseAddr, sizeof(short) * 2),
            //},
            //OriginPos = new()
            //{
            //    X = TasMemory.ReadInt16(baseAddr, sizeof(short) * 4),
            //    Y = TasMemory.ReadInt16(baseAddr, sizeof(short) * 5),
            //},
            CurrentFrameId = TasMemory.ReadInt16(baseAddr, sizeof(short) * 6),
            //BakupFrameId = TasMemory.ReadInt16(baseAddr, sizeof(short) * 7),
            //CooperativeMagicId = (TasMagics)TasMemory.ReadUInt16(baseAddr, sizeof(short) * 11),
        };
    }

    /// <summary>
    /// 检查所有敌人是否均已阵亡，若全部阵亡则说明战斗结束
    /// </summary>
    public static bool CheckBattleEnd()
    {
        var BattleEnemyMaxId = TasMemory.ReadUInt16(TasMemory.BattleEnemyMaxIdAddr);
        var baseAddr =  TasMemory.ReadUInt32(TasMemory.EnemyBattleTempDataAddr);
        for (var i = 0; i <= BattleEnemyMaxId; i++)
        {
            // 获取当前敌人血量
            if (TasMemory.ReadInt16((uint)(baseAddr + (sizeof(REnemyBattleTempData) * i)), sizeof(short) * 9) > 0)
                // 战斗未结束
                return false;
        }

        // 战斗胜利
        return true;
    }

    /// <summary>
    /// 获取队员本回合的行动动作
    /// </summary>
    /// <param name="memberId">队员编号</param>
    /// <param name="action">动作参数</param>
    public static RMemberRoundAction GetMemberRoundAction(TasBattleFighter fightId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.MemberRoundActionAddr);
        var baseOffset = baseAddr + (uint)((int)fightId * sizeof(RMemberRoundAction));

        return new()
        {
            TargetOfAttack = (TasTargetOfAttack)TasMemory.ReadInt16(baseOffset, sizeof(short) * 0),
            MemberAction = (TasMemberActions)TasMemory.ReadInt16(baseOffset, sizeof(short) * 1),
            EntityId = new()
            {
                RawValue = TasMemory.ReadInt16(baseOffset, sizeof(short) * 2),
            },
            MenuCursorId = TasMemory.ReadUInt16(baseOffset, sizeof(short) * 4),
        };
    }

    /// <summary>
    /// 设置队员本回合的行动动作
    /// </summary>
    /// <param name="memberId">队员编号</param>
    /// <param name="action">动作参数</param>
    public static void SetMemberRoundAction(int memberId, RMemberRoundAction action)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.MemberRoundActionAddr);
        var baseOffset = baseAddr + (uint)(memberId * sizeof(RMemberRoundAction));

        TasMemory.WriteInt16(baseOffset, (short)action.TargetOfAttack, sizeof(short) * 0);
        TasMemory.WriteUInt16(baseOffset, (ushort)action.MemberAction, sizeof(ushort) * 1);
        TasMemory.WriteInt16(baseOffset, action.EntityId.RawValue, sizeof(short) * 2);
        TasMemory.WriteUInt16(baseOffset, action.MenuCursorId, sizeof(ushort) * 3);
    }

    /// <summary>
    /// 获取敌人实体巫抗
    /// </summary>
    /// <param name="enemyId">敌人实体编号</param>
    /// <param name="resilience">巫抗</param>
    public static ushort GetEnemyResilience(TasEnemys enemyId) =>
        TasMemory.ReadUInt16(TasMemory.ReadUInt32(TasMemory.EntityDataAddr), (uint)enemyId * 0x000E + sizeof(ushort) * 1);

    /// <summary>
    /// 设置敌人实体巫抗
    /// </summary>
    /// <param name="enemyId">敌人实体编号</param>
    /// <param name="resilience">巫抗</param>
    public static void SetEnemyResilience(TasEnemys enemyId, ushort resilience) =>
        TasMemory.WriteUInt16(TasMemory.ReadUInt32(TasMemory.EntityDataAddr), resilience, (uint)enemyId * 0x000E + sizeof(ushort) * 1);

    /// <summary>
    /// 获取敌人实体数据
    /// </summary>
    /// <param name="enemyId">敌人实体编号</param>
    /// <returns>敌人实体数据</returns>
    public static REnemy GetEnemyEntity(TasEnemys enemyId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.EntityDataAddr) + (uint)enemyId * 0x000E;

        return new()
        {
            EnemyDataId = TasMemory.ReadUInt16(baseAddr, sizeof(ushort) * 0),
            ResistanceToSorcery = TasMemory.ReadInt16(baseAddr, sizeof(ushort) * 1),
            Script = new()
            {
                TurnStart = TasMemory.ReadUInt16(baseAddr, sizeof(ushort) * 2),
                BattleWon = TasMemory.ReadUInt16(baseAddr, sizeof(ushort) * 3),
                Action = TasMemory.ReadUInt16(baseAddr, sizeof(ushort) * 4),
            },
        };
    }

    /// <summary>
    /// 获取敌人实体数据
    /// </summary>
    /// <param name="enemyId">敌人实体编号</param>
    /// <returns>敌人实体数据</returns>
    public static void SetEnemyEntity(TasEnemys enemyId, REnemy enemy)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.EntityDataAddr) + (uint)enemyId * 0x000E;

        TasMemory.WriteUInt16(baseAddr, enemy.EnemyDataId, sizeof(short) * 0);
        TasMemory.WriteInt16(baseAddr, enemy.ResistanceToSorcery, sizeof(short) * 1);
        TasMemory.WriteUInt16(baseAddr, enemy.Script.TurnStart, sizeof(short) * 2);
        TasMemory.WriteUInt16(baseAddr, enemy.Script.BattleWon, sizeof(short) * 3);
        TasMemory.WriteUInt16(baseAddr, enemy.Script.Action, sizeof(short) * 4);
    }

    /// <summary>
    /// 修改脚本内容
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="codes">实际内容</param>
    public static void SetScriptEntry(ushort address, ushort[] codes)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.ScriptEntryAddr);
        var scr = TasMemory.ReadUInt16(baseAddr, (uint)(address * 0x0008), sizeof(ushort) * 0);
        var arg1 = TasMemory.ReadUInt16(baseAddr, (uint)(address * 0x0008), sizeof(ushort) * 1);
        var arg2 = TasMemory.ReadUInt16(baseAddr, (uint)(address * 0x0008), sizeof(ushort) * 2);
        var arg3 = TasMemory.ReadUInt16(baseAddr, (uint)(address * 0x0008), sizeof(ushort) * 3);
        var str = $"@{address:X4}: {scr:X4} {arg1:X4} {arg2:X4} {arg3:X4}";

        for (var i = 0; i < codes.Length; i++)
            TasMemory.WriteUInt16(baseAddr, codes[i], (uint)(address * 0x0008 + sizeof(ushort) * i));
    }

    /// <summary>
    /// 获取我方轮到谁选动作
    /// </summary>
    public static TasBattleFighter CurrentActorSelectorId => (TasBattleFighter)TasMemory.ReadUInt16(TasMemory.CurrentActorSelectorIdAddr);

    /// <summary>
    /// 设置我方轮到谁选动作
    /// </summary>
    /// <param name="fighterId">阵营人员编号</param>
    public static void SetCurrentActorSelectorId(TasBattleFighter fighterId) => TasMemory.WriteInt16(TasMemory.CurrentActorSelectorIdAddr, (short)fighterId);

    /// <summary>
    /// 获取轮到谁执行动作(不区分阵营)
    /// </summary>
    public static TasBattleFighter CurrentActorId => (TasBattleFighter)TasMemory.ReadUInt16(TasMemory.CurrentActorIdAddr);

    /// <summary>
    /// 设置轮到谁执行动作(不区分阵营，该修改将影响回合正常运行)
    /// </summary>
    /// <param name="fighterId">阵营人员编号</param>
    public static void SetCurrentActorId(TasBattleFighter fighterId) => TasMemory.WriteInt16(TasMemory.CurrentActorIdAddr, (short)fighterId);

    /// <summary>
    /// 获取当前是那一阵营在执行动作<-1敌 0我>
    /// </summary>
    public static TasBattleTeam CurrentActorTeam => (TasBattleTeam)TasMemory.ReadInt16(TasMemory.CurrentActorTeamAddr);

    /// <summary>
    /// 设置当前是那一阵营在执行动作(该修改不影响回合正常运行)
    /// </summary>
    public static void SetCurrentActorTeam(TasBattleTeam battleTeam) => TasMemory.WriteInt16(TasMemory.CurrentActorTeamAddr, (short)battleTeam);

    /// <summary>
    /// 设置我方队员特殊状态
    /// </summary>
    public static void SetMemberSpecialStatus(int member, TasSpecialStatus specialStatus, ushort roundCount)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.MemberSpecialStatusAddr);
        var offsetAddr = (uint)((ushort)specialStatus * MAX_MEMBER_COUNT + member) * sizeof(ushort);
        TasMemory.WriteUInt16(baseAddr + offsetAddr, roundCount);
    }

    /// <summary>
    /// 获取触发当前战斗的脚本地址
    /// </summary>
    public static ushort TriggerBattleScriptAddress => TasMemory.ReadUInt16(TasMemory.TriggerBattleScriptAddressAddr);

    /// <summary>
    /// 获取 Hero 指定属性的装备总加成
    /// </summary>
    /// <param name="heroId">Hero 编号</param>
    /// <param name="extraAttributeId">额外属性编号</param>
    /// <returns>Hero 指定属性的装备总加成</returns>
    public static short GetHeroExtraAttribute(TasHero heroId, TasHeroExtraAttribute extraAttributeId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.HeroExtraAttributeAddr);
        var offsetAddr = MAX_HERO_BATTLE_EQUIPMENT_COUNT * MAX_HERO_COUNT * (ushort)extraAttributeId;

        var attribute = (short)0;
        for (var i = 0; i < MAX_HERO_BATTLE_EQUIPMENT_COUNT; i++)
        {
            var actualOffsetAddr = (offsetAddr + MAX_HERO_COUNT * i + (ushort)heroId) * sizeof(ushort);

            attribute += TasMemory.ReadInt16(baseAddr, (uint)actualOffsetAddr);
        }

        return attribute;
    }

    /// <summary>
    /// 设置 Hero 临时属性
    /// </summary>
    /// <param name="heroId">hero 编号</param>
    /// <param name="extraAttributeId">额外属性编号</param>
    /// <param name="value"></param>
    public static void SetHeroTempAttribute(TasHero heroId, TasHeroExtraAttribute extraAttributeId, short value)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.HeroExtraAttributeAddr);
        var offsetAddr = MAX_HERO_BATTLE_EQUIPMENT_COUNT * MAX_HERO_COUNT * (ushort)extraAttributeId;
        var actualOffsetAddr = (offsetAddr + MAX_HERO_COUNT * (ushort)TasEquipType.临时 + (ushort)heroId) * sizeof(ushort);

        TasMemory.WriteInt16(baseAddr, value, (uint)actualOffsetAddr);
    }

    /// <summary>
    /// 获取 hero 指定属性的实际数值
    /// </summary>
    /// <param name="heroId">hero 编号</param>
    /// <param name="extraAttributeId">属性编号</param>
    /// <returns>hero 指定属性的实际数值</returns>
    public static short GetHeroAttribute(TasHero heroId, TasHeroAttribute heroAttributeId)
    {
        var baseAddr = TasMemory.ReadUInt32(TasMemory.HeroBaseDataAddr);
        var offsetAddr = ((ushort)heroAttributeId * MAX_HERO_COUNT + (ushort)heroId) * sizeof(ushort);

        return TasMemory.ReadInt16(baseAddr, (uint)offsetAddr);
    }

    /// <summary>
    /// 获取 Hero 指定属性的实际数值
    /// </summary>
    /// <param name="heroId">Hero 编号</param>
    /// <param name="extraAttributeId">属性编号</param>
    /// <returns>Hero 指定属性的实际数值</returns>
    public static short GetHeroActualAttribute(TasHero heroId, TasHeroAttribute heroAttributeId)
    {
        var value = GetHeroAttribute(heroId, heroAttributeId);

        if ((heroAttributeId >= TasHeroAttribute.武术) && (heroAttributeId <= TasHeroAttribute.避土率))
            // 加上装备加成和道具临时加成
            value += GetHeroExtraAttribute(heroId, (TasHeroExtraAttribute)(heroAttributeId - TasHeroAttribute.武术));

        return value;
    }

    /// <summary>
    /// 获取敌方基础数据
    /// </summary>
    /// <param name="baseDataId">基础数据编号</param>
    /// <returns>敌方基础数据</returns>
    public static REnemyBaseData GetEnemyBaseData(int baseDataId)
    {
        var baseAddr = TasMemory.EnemyBaseDataAddr + (uint)(sizeof(REnemyBaseData) * baseDataId);
        var data = new REnemyBaseData()
        {
            IdleFrames = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 0),
            MagicFrames = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 1),
            AttackFrames = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 2),
            IdleAnimSpeed = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 3),
            ActWaitFrames = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 4),
            YPosOffset = TasMemory.ReadInt16(baseAddr, sizeof(short) * 5),
            AttackSound = TasMemory.ReadInt16(baseAddr, sizeof(short) * 6),
            ActionSound = TasMemory.ReadInt16(baseAddr, sizeof(short) * 7),
            MagicSound = TasMemory.ReadInt16(baseAddr, sizeof(short) * 8),
            DeathSound = TasMemory.ReadInt16(baseAddr, sizeof(short) * 9),
            CallSound = TasMemory.ReadInt16(baseAddr, sizeof(short) * 10),
            MaxHP = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 11),
            Exp = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 12),
            Cash = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 13),
            Level = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 14),
            MagicId = TasMemory.ReadInt16(baseAddr, sizeof(short) * 15),
            MagicRate = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 16),
            AttackEquivItemId = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 17),
            AttackEquivItemRate = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 18),
            StealItemId = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 19),
            StealItemCount = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 20),
            Attribute = new()
            {
                AttackStrength = TasMemory.ReadInt16(baseAddr, sizeof(short) * 21),
                MagicStrength = TasMemory.ReadInt16(baseAddr, sizeof(short) * 22),
                Defense = TasMemory.ReadInt16(baseAddr, sizeof(short) * 23),
                Dexterity = TasMemory.ReadInt16(baseAddr, sizeof(short) * 24),
                FleeRate = TasMemory.ReadInt16(baseAddr, sizeof(short) * 25),
            },
            PoisonResistance = TasMemory.ReadInt16(baseAddr, sizeof(short) * 26),
            PhysicalResistance = TasMemory.ReadInt16(baseAddr, sizeof(short) * 32),
            DualMove = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 33),
            CollectValue = TasMemory.ReadUInt16(baseAddr, sizeof(short) * 34),
        };
        data.ElementalResistance[0] = TasMemory.ReadInt16(baseAddr, sizeof(short) * 27);
        data.ElementalResistance[1] = TasMemory.ReadInt16(baseAddr, sizeof(short) * 28);
        data.ElementalResistance[2] = TasMemory.ReadInt16(baseAddr, sizeof(short) * 29);
        data.ElementalResistance[3] = TasMemory.ReadInt16(baseAddr, sizeof(short) * 30);
        data.ElementalResistance[4] = TasMemory.ReadInt16(baseAddr, sizeof(short) * 31);
        return data;
    }

    /// <summary>
    /// 获取敌方基础数据
    /// </summary>
    /// <param name="baseDataId">基础数据编号</param>
    /// <returns>敌方基础数据</returns>
    public static void SetEnemyBaseData(int baseDataId, REnemyBaseData data)
    {
        var baseAddr = TasMemory.EnemyBaseDataAddr + (uint)(sizeof(REnemyBaseData) * baseDataId);
        TasMemory.WriteUInt16(baseAddr, data.IdleFrames, sizeof(short) * 0);
        TasMemory.WriteUInt16(baseAddr, data.MagicFrames, sizeof(short) * 1);
        TasMemory.WriteUInt16(baseAddr, data.AttackFrames, sizeof(short) * 2);
        TasMemory.WriteUInt16(baseAddr, data.IdleAnimSpeed, sizeof(short) * 3);
        TasMemory.WriteUInt16(baseAddr, data.ActWaitFrames, sizeof(short) * 4);
        TasMemory.WriteInt16(baseAddr, data.YPosOffset, sizeof(short) * 5);
        TasMemory.WriteInt16(baseAddr, data.AttackSound, sizeof(short) * 6);
        TasMemory.WriteInt16(baseAddr, data.ActionSound, sizeof(short) * 7);
        TasMemory.WriteInt16(baseAddr, data.MagicSound, sizeof(short) * 8);
        TasMemory.WriteInt16(baseAddr, data.DeathSound, sizeof(short) * 9);
        TasMemory.WriteInt16(baseAddr, data.CallSound, sizeof(short) * 10);
        TasMemory.WriteUInt16(baseAddr, data.MaxHP, sizeof(short) * 11);
        TasMemory.WriteUInt16(baseAddr, data.Exp, sizeof(short) * 12);
        TasMemory.WriteUInt16(baseAddr, data.Cash, sizeof(short) * 13);
        TasMemory.WriteUInt16(baseAddr, data.Level, sizeof(short) * 14);
        TasMemory.WriteInt16(baseAddr, data.MagicId, sizeof(short) * 15);
        TasMemory.WriteUInt16(baseAddr, data.MagicRate, sizeof(short) * 16);
        TasMemory.WriteUInt16(baseAddr, data.AttackEquivItemId, sizeof(short) * 17);
        TasMemory.WriteUInt16(baseAddr, data.AttackEquivItemRate, sizeof(short) * 18);
        TasMemory.WriteUInt16(baseAddr, data.StealItemId, sizeof(short) * 19);
        TasMemory.WriteUInt16(baseAddr, data.StealItemCount, sizeof(short) * 20);
        var attr = data.Attribute;
        TasMemory.WriteInt16(baseAddr, attr.AttackStrength, sizeof(short) * 21);
        TasMemory.WriteInt16(baseAddr, attr.MagicStrength, sizeof(short) * 22);
        TasMemory.WriteInt16(baseAddr, attr.Defense, sizeof(short) * 23);
        TasMemory.WriteInt16(baseAddr, attr.Dexterity, sizeof(short) * 24);
        TasMemory.WriteInt16(baseAddr, attr.FleeRate, sizeof(short) * 25);
        TasMemory.WriteInt16(baseAddr, data.PoisonResistance, sizeof(short) * 26);
        TasMemory.WriteInt16(baseAddr, data.ElementalResistance[0], sizeof(short) * 27);
        TasMemory.WriteInt16(baseAddr, data.ElementalResistance[1], sizeof(short) * 28);
        TasMemory.WriteInt16(baseAddr, data.ElementalResistance[2], sizeof(short) * 29);
        TasMemory.WriteInt16(baseAddr, data.ElementalResistance[3], sizeof(short) * 30);
        TasMemory.WriteInt16(baseAddr, data.ElementalResistance[4], sizeof(short) * 31);
        TasMemory.WriteInt16(baseAddr, data.PhysicalResistance, sizeof(short) * 32);
        TasMemory.WriteUInt16(baseAddr, data.DualMove, sizeof(short) * 33);
        TasMemory.WriteUInt16(baseAddr, data.CollectValue, sizeof(short) * 34);
    }

    /// <summary>
    /// 获取我方队伍最大索引
    /// </summary>
    /// <returns>我方队伍最大索引</returns>
    public static ushort BattleMemberMaxId => TasMemory.ReadUInt16(TasMemory.BattleMemberMaxIdAddr);
}

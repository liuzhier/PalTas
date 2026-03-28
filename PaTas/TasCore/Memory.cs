using System;
using System.Runtime.InteropServices;
using System.Text;
using Vanara.PInvoke;

namespace PalTas.TasCore;

public static unsafe class TasMemory
{
    /// <summary>
    /// 基址
    /// </summary>
    public const uint
        PAL_EXE                                 = 0x0002_8000,                  // PAL.EXE
        PAL_EXE_BASE                            = PAL_EXE + 0x0040_0000,        // 数据基地址
        GLOBAL_DATA                             = 0x0000_003C,                  // 全局数据基地址
        EVENT                                   = 0x0000_0144,                  // 连续结构：事件
        DIALOGUE_OUTPUT_DELAY                   = 0x0000_0232,                  // 对话逐字输出延迟
        CURRENT_DIALOGUE_LINE_ID                = 0x0000_0234,                  // 当前对话行数
        VIEWPORT_POSITION                       = 0x0000_0262,                  // 视口坐标
        CURRENT_SCENE_ID                        = 0x0000_026A,                  // 当前场景编号
        LEADER_DIRECTION                        = 0x0000_026E,                  // 领队当前面朝方向
        CURRENT_SCENE_MAX_EVENT_ID              = 0x0000_0296,                  // 当前场景事件最大编号
        CURRENT_ACTOR_SELECTOR_ID               = 0x0000_02C2,                  // 【战斗】我方轮到谁选动作
        BATTLE_ENEMY_MAX_ID                     = 0x0000_02DE,                  // 【战斗】敌方最大索引
        INVENTORY_ITEM_ID                       = 0x0000_02F0,                  // 道具列表光标
        CURRENT_ACTOR_ID                        = 0x0000_0312,                  // 【战斗】轮到谁执行动作（不区分阵营）
        CURRENT_ACTION_TEAM                     = 0x0000_031C,                  // 【战斗】当前是那一阵营在执行动作<-1敌 0我>
        SCENE                                   = 0x0000_0398,                  // 连续结构：场景
        MEMBER_TRAIL_RELATIVE_TO_VIEWPORT       = 0x0000_04B8,                  // 连续结构：队伍相对于视口的步伐
        //MEMBER_TRAIL_RELATIVE_TO_VIEWPORT       = 0x0000_0274,                  // 连续结构：队伍相对于视口的步伐
        MEMBER_TRAIL                            = 0x0000_04D0,                  // 连续结构：队伍步伐
        MEMBER_ROUND_ACTION                     = 0x0000_05A8,                  // 【战斗】连续结构：队员本回合的行动动作
        ENEMY_BATTLE_TEMP_DATA                  = 0x0000_05C0,                  // 【战斗】连续结构：战斗中敌方的战斗临时数据
        MEMBER_SPECIAL_STATUS                   = 0x0000_06D0,                  // 【战斗】连续结构：队员的增益/恶性状态
        MEMBER_POISON_STATUS                    = 0x0000_0710,                  // 【战斗】连续结构：队员的增益/恶性状态
        ENTITY                                  = 0x0000_0750,                  // 连续结构：实体对象（需解引用）
        INVENTORY                               = 0x0000_0768,                  // 连续结构：道具列表
        HERO_BASE_DATA                          = 0x0000_07A8,                  // 【战斗】我方基础数据
        CURRENT_SCENE_EVENT                     = 0x0000_07E8,                  // 连续结构：当前场景事件
        TRIGGER_BATTLE_SCRIPT_ADDRESS           = 0x0019_FBCC,                  // 触发当前战斗的脚本地址
        CURRENT_ENEMY_TEAM_ID                   = PAL_EXE + 0x0017_7BC8,        // 【战斗】当前敌方队伍编号
        RANDOM_RESULT                           = 0x0007_1FCC;                  // PAL.DLL v1.02 随机数结果（原意义为种子）

    /// <summary>
    /// 进程句柄
    /// </summary>
    static SafeHandle ProcessSafeHandle { get; set; } = null!;

    /// <summary>
    /// 各种数据的基地址
    /// </summary>
    public static uint PalBaseAddr
    {
        get
        {
            var addr = 0u;

            while (addr == 0) addr = ReadUInt32(PAL_EXE_BASE);

            return addr;
        }
    }
    public static uint GlobalDataAddr => PalBaseAddr + GLOBAL_DATA;
    public static uint HeroExtraAttributeAddr => ReadUInt32(GlobalDataAddr) - 0x0000_051C;
    public static uint ScriptEntryAddr => ReadUInt32(GlobalDataAddr) - 0x0000_0B88;
    public static uint EntityDataAddr => PalBaseAddr + ENTITY;
    public static uint EventAddr => PalBaseAddr + EVENT;
    public static uint CurrentSceneEventAddr => PalBaseAddr + CURRENT_SCENE_EVENT;
    public static uint EventMaxIdAddr => PalBaseAddr + CURRENT_SCENE_MAX_EVENT_ID;
    public static uint DialogueOutputDelayAddr => PalBaseAddr + DIALOGUE_OUTPUT_DELAY;
    public static uint CurrentDialogueLineIdAddr => PalBaseAddr + CURRENT_DIALOGUE_LINE_ID;
    public static uint ViewportPosAddr => PalBaseAddr + VIEWPORT_POSITION;
    public static uint CurrentSceneIdAddr => PalBaseAddr + CURRENT_SCENE_ID;
    public static uint LeaderDirectionAddr => PalBaseAddr + LEADER_DIRECTION;
    public static uint CurrentActorSelectorIdAddr => PalBaseAddr + CURRENT_ACTOR_SELECTOR_ID;
    public static uint BattleEnemyMaxIdAddr => PalBaseAddr + BATTLE_ENEMY_MAX_ID;
    public static uint InventoryItemIdAddr => PalBaseAddr + INVENTORY_ITEM_ID;
    public static uint CurrentActorIdAddr => PalBaseAddr + CURRENT_ACTOR_ID;
    public static uint CurrentActorTeamAddr => PalBaseAddr + CURRENT_ACTION_TEAM;
    public static uint SceneAddr => PalBaseAddr + SCENE;
    public static uint MemberTrailRelativeToViewportAddr => PalBaseAddr + MEMBER_TRAIL_RELATIVE_TO_VIEWPORT;
    public static uint MemberRoundActionAddr => PalBaseAddr + MEMBER_ROUND_ACTION;
    public static uint EnemyBattleTempDataAddr => PalBaseAddr + ENEMY_BATTLE_TEMP_DATA;
    public static uint MemberSpecialStatusAddr => PalBaseAddr + MEMBER_SPECIAL_STATUS;
    public static uint MemberPoisonStatusAddr => PalBaseAddr + MEMBER_POISON_STATUS;
    public static uint InventoryAddr => PalBaseAddr + INVENTORY;
    public static uint MemberTrailAddr => PalBaseAddr + MEMBER_TRAIL;
    public static uint HeroBaseDataAddr => PalBaseAddr + HERO_BASE_DATA;
    public static uint TriggerBattleScriptAddressAddr => TRIGGER_BATTLE_SCRIPT_ADDRESS;
    public static uint CurrentEnemyTeamIdAddr => CURRENT_ENEMY_TEAM_ID;
    public static uint PalDllAddr => (uint)GetPalModuleBase();
    public static uint RandomResultAddr => PalDllAddr + RANDOM_RESULT;

    /// <summary>
    /// 初始化内存模块
    /// </summary>
    /// <returns>初始化结果</returns>
    public static nint GetProcessHandle()
    {
        // 已有有效句柄，直接返回
        if (ProcessSafeHandle != null) goto End;

        FreeProcessHandle();

        do
        {
            User32.GetWindowThreadProcessId(GetWindowHandle(), out var processId);

            if (processId != 0)
            {

                // 使用正确的访问权限枚举
                var access = Kernel32.ProcessAccess.PROCESS_VM_READ |
                        Kernel32.ProcessAccess.PROCESS_VM_WRITE |
                        Kernel32.ProcessAccess.PROCESS_VM_OPERATION |
                        Kernel32.ProcessAccess.PROCESS_QUERY_INFORMATION;
                ProcessSafeHandle = Kernel32.OpenProcess((uint)access, false, processId);
            }

            Sleep();
        } while ((ProcessSafeHandle?.DangerousGetHandle() ?? 0) == 0);

    End:
        return ProcessSafeHandle!.DangerousGetHandle();
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    public static void FreeProcessHandle()
    {
        if (ProcessSafeHandle != null)
        {
            // 关闭进程句柄
            Kernel32.CloseHandle(ProcessSafeHandle.DangerousGetHandle());
            ProcessSafeHandle = null!;
        }
    }

    /// <summary>
    /// 参考实现的内存读取方法
    /// </summary>
    public static T Readm<T>(nint handle, int addr)
    {
        var res = (T)default!;
        var t = typeof(T);
        int size = (t.Name == "String") ? 1024 : sizeof(T);

        var buffer = new byte[size];

        // 使用GCHandle固定缓冲区
        var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            if (Kernel32.ReadProcessMemory(handle, new nint(addr), gch.AddrOfPinnedObject(), (SizeT)size, out _))
            {
                if (t == typeof(short))
                {
                    short tmp = BitConverter.ToInt16(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(ushort))
                {
                    ushort tmp = BitConverter.ToUInt16(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(byte))
                {
                    byte tmp = buffer[0];
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(int))
                {
                    int tmp = BitConverter.ToInt32(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(uint))
                {
                    uint tmp = BitConverter.ToUInt32(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(long))
                {
                    long tmp = BitConverter.ToInt64(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(bool))
                {
                    bool tmp = BitConverter.ToBoolean(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(double))
                {
                    double tmp = BitConverter.ToDouble(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(float))
                {
                    float tmp = BitConverter.ToSingle(buffer, 0);
                    res = (T)Convert.ChangeType(tmp, t);
                }
                else if (t == typeof(string))
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < buffer.Length; ++i)
                    {
                        char c = (char)buffer[i];
                        if (c == '\0')
                        {
                            res = (T)Convert.ChangeType(sb.ToString(), t);
                            break;
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
            }
        }
        finally
        {
            gch.Free();
        }

        return res!;
    }

    /// <summary>
    /// 读取内存数据（使用参考实现的方法）
    /// </summary>
    public static T ReadMemory<T>(uint baseAddress, params uint[] offsets)
    {
        // 转换为int（32位地址）
        var addr = (int)baseAddress;

        // 应用偏移
        foreach (var offset in offsets) addr += (int)offset;

        return Readm<T>(GetProcessHandle(), addr);
    }

    /// <summary>
    /// 写入内存数据
    /// </summary>
    public static bool WriteMemory<T>(uint baseAddress, T value, params uint[] offsets)
    {
        try
        {
            if (GetProcessHandle() == 0) return false;

            var currentAddress = baseAddress;

            // 应用偏移
            foreach (var offset in offsets) currentAddress += offset;

            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];

            // 使用GCHandle固定缓冲区
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value!, handle.AddrOfPinnedObject(), false);

                // 正确使用Vanara.PInvoke.Kernel32.WriteProcessMemory
                return Kernel32.WriteProcessMemory(GetProcessHandle(), (nint)currentAddress, handle.AddrOfPinnedObject(), (SizeT)size, out _);
            }
            finally
            {
                handle.Free();
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 读取1字节数据
    /// </summary>
    public static byte ReadByte(uint baseAddress, params uint[] offsets) => ReadMemory<byte>(baseAddress, offsets);

    /// <summary>
    /// 读取2字节数据
    /// </summary>
    public static short ReadInt16(uint baseAddress, params uint[] offsets) => ReadMemory<short>(baseAddress, offsets);
    public static ushort ReadUInt16(uint baseAddress, params uint[] offsets) => ReadMemory<ushort>(baseAddress, offsets);

    /// <summary>
    /// 读取4字节数据
    /// </summary>
    public static uint ReadUInt32(uint baseAddress, params uint[] offsets) => ReadMemory<uint>(baseAddress, offsets);

    /// <summary>
    /// 写入1字节数据
    /// </summary>
    public static bool WriteByte(uint baseAddress, byte value, params uint[] offsets) => WriteMemory(baseAddress, value, offsets);

    /// <summary>
    /// 写入2字节数据
    /// </summary>
    public static bool WriteUInt16(uint baseAddress, ushort value, params uint[] offsets) => WriteMemory(baseAddress, value, offsets);
    public static bool WriteInt16(uint baseAddress, short value, params uint[] offsets) => WriteMemory(baseAddress, value, offsets);

    /// <summary>
    /// 写入4字节数据
    /// </summary>
    public static bool WriteUInt32(uint baseAddress, uint value, params uint[] offsets) => WriteMemory(baseAddress, value, offsets);
    public static bool WriteSingle(uint baseAddress, float value, params uint[] offsets) => WriteMemory(baseAddress, value, offsets);
}

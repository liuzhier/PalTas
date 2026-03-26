using System.Runtime.InteropServices;

namespace PalTas.TasCore.Records;

public static class Core
{
    /// <summary>
    /// Event状态枚举
    /// </summary>
    public enum EventState : short
    {
        Hidden          = 0,        // 隐藏
        NonObstacle     = 1,        // 漂浮
        Obstacle        = 2,        // 障碍（阻碍领队通过）
    }

    /// <summary>
    /// Event触发模式枚举
    /// </summary>
    public enum EventTriggerMode : ushort
    {
        None                = 0,        // 无法触发
        SearchNear          = 1,        // 手动触发，范围 1（须脸贴或重合，大部分道具的获取方式）
        SearchNormal        = 2,        // 手动触发，范围 3
        SearchFar           = 3,        // 手动触发，范围 5
        TouchNear           = 4,        // 自动触发，范围 0（须重合，如将军冢石板机关）
        TouchNormal         = 5,        // 自动触发，范围 1
        TouchFar            = 6,        // 自动触发，范围 2
        TouchFarther        = 7,        // 自动触发，范围 3
        TouchFarthest       = 8,        // 自动触发，范围 4
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct REvent
    {
        public  short                   VanishTime;             // 正数为剩余隐匿帧数，负数为逃跑后僵直帧数（一般为战斗事件）
        public  short                   X;                      // X 坐标
        public  short                   Y;                      // Y 坐标
        public  short                   Layer;                  // 图层
        public  ushort                  TriggerScript;          // 触发脚本
        public  ushort                  AutoScript;             // 自动脚本
        public  EventState              State;                  // 触发状态
        public  EventTriggerMode        TriggerMode;            // 触发模式
        public  ushort                  SpriteId;               // 形象
        public  ushort                  FramesPerDirection;     // 形象每个方向的帧数
        public  TasDirection            Direction;              // 当前面朝方向
        public  ushort                  CurrentFrameId;         // 当前帧数（当前方向上的）
        public  ushort                  TriggerIdleFrame;       // 触发脚本累计被触发次数
        readonly    ushort              _unknown;               // 未知数据
        readonly    ushort              _spriteFramesAuto;      // 形象总帧数（自动计算，只在内存中有意义）
        public  ushort                  AutoIdleFrame;          // 自动脚本累计被触发次数
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RScene
    {
        public  ushort      MapId;                  // 实际地图
        public  ushort      ScriptOnEnter;          // 脚本：进入场景
        public  ushort      ScriptOnTeleport;       // 脚本：脱离场景（引路蜂、土灵珠）
        public  ushort      EventObjectIndex;       // 事件起始索引，实际索引为（EventObjectIndex + 1）
    }
}

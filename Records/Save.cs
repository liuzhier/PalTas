using System.Runtime.InteropServices;

namespace PalTas.Records;

/// <summary>
/// 坐标
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RPos(short X = 0, short Y = 0)
{
    public  short       X = X, Y = Y;       // X, Y（相对于视角）

    public override readonly string ToString() => $"({X}, {Y})";
}

/// <summary>
/// 队员步伐信息
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RMemberTrail
{
    public  RPos             Pos;                // 坐标 X, Y
    public  TasDirection     Direction;          // 面朝方向（下左上右 0123）
}

/// <summary>
/// 队员相对于视口的步伐信息
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RMemberTrailRelativeToViewport
{
    public  ushort              HeroId;             // 形象编号
    public  RMemberTrail        RelativeTrail;      // 坐标 X, Y（相对于视角）
    public  ushort              FrameOffset;        // 形象帧偏移（原地行走 012）
}

/// <summary>
/// 道具列表
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RInventory
{
    public  ushort      ItemId;             // 道具编号
    public  ushort      Count;              // 数量
    public  ushort      DeductionCount;     // 本回合预计消耗数（只在内存中有效哦！）
}

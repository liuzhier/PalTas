namespace PalTas.TasCore.Records;

/// <summary>
/// 队伍行进路径
/// </summary>
/// <param name="X">坐标 X</param>
/// <param name="Y">坐标 Y</param>
/// <param name="NeedPreInput">需要预输入（滑步取物）</param>
/// <param name="SceneId">指定等待进入哪个场景后才开始行进</param>
public class TasWalkPath(short X = 0, short Y = 0, int SceneId = -1, int PreInputTimes = 0, TasDirection Direction = TasDirection.Current, TasItems NeedUseItemId = TasItems.NULL)
{
    public RPos Pos { get; set; } = new(X, Y);
    public int SceneId { get; set; } = SceneId;
    public int PreInputTimes { get; set; } = PreInputTimes;
    public TasDirection Direction { get; set; } = Direction;
    public TasItems NeedUseItemId { get; set; } = NeedUseItemId;
}

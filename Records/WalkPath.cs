namespace PalTas.Records;

/// <summary>
/// 队伍行进路径
/// </summary>
/// <param name="X">坐标 X</param>
/// <param name="Y">坐标 Y</param>
/// <param name="NeedPreInput">需要预输入（滑步取物）</param>
/// <param name="SceneId">指定等待进入哪个场景后才开始行进</param>
public class TasWalkPath(short X = 0, short Y = 0, bool NeedPreInput = false, int SceneId = -1)
{
    public RPos Pos { get; set; } = new(X, Y);
    public bool NeedPreInput { get; set; } = NeedPreInput;
    public int SceneId { get; set; } = SceneId;
}

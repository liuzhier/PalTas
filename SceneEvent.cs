using PalTas.Records;

using static PalTas.TasScript.SceneEvent;

namespace PalTas;

public static partial class TasScript
{
    public enum SceneEvent : ushort
    {
        // 客栈大厅
        _4_去厨房的李大娘      = 13,
        _4_醉倒酒剑仙          = 19,
        _4_还魂香              = 23,

        // 各分立房间
        _2_逍遥房间到客栈大厅场景切换点       = 4,
        _2_出房间李大娘                       = 11,
        _2_十里香                             = 15,
        _2_执灶李大娘                         = 20,
        _2_酒菜                               = 21,
    }

    public static async Task InitGlobalSceneEventAsync(CancellationToken token)
    {
        await Task.Delay(2000, token);
        SetEventTriggerMode(4, _4_还魂香, Core.EventTriggerMode.TouchNormal);
        SetEventTriggerMode(2, _2_十里香, Core.EventTriggerMode.TouchNormal);
        SetEventTriggerMode(2, _2_酒菜, Core.EventTriggerMode.TouchNormal);
        SetEventTriggerMode(2, _2_执灶李大娘, Core.EventTriggerMode.TouchFarthest);
    }
}

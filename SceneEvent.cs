using PalTas.Records;

using static PalTas.TasScript.SceneEvent;

namespace PalTas;

public static partial class TasScript
{
    public enum SceneEvent : ushort
    {
        // 客栈大厅
        _4_去厨房的李大娘            = 13,
        _4_胖喵_树欲静而风不止       = 16,
        _4_醉倒酒剑仙                = 19,
        _4_还魂香                    = 23,

        // 各分立房间
        _2_逍遥房间到客栈大厅场景切换点       = 4,
        _2_皮帽                               = 6,
        _2_出房间李大娘                       = 11,
        _2_十里香                             = 15,
        _2_执灶李大娘                         = 20,
        _2_酒菜                               = 21,

        // 各分立房间 2
        _3_驱魔香                             = 2,
        _3_王小虎                             = 5,
        _3_还神丹                             = 11,
        _3_忘魂花                             = 12,

        // 盛渔村集市
        _6_张四哥                             = 9,
    }

    public static async Task InitGlobalSceneEventAsync(CancellationToken token)
    {
        await Delay(2000, token);

        // 见石碑篇_出房间
        var 皮帽 = GetEvent(2, _2_皮帽);
        皮帽.TriggerMode = Core.EventTriggerMode.TouchNormal;
        SetEventInfo(2, _2_皮帽, 皮帽);

        // 见石碑篇_下楼直走还魂香
        var 还魂香 = GetEvent(4, _4_还魂香);
        还魂香.TriggerMode = Core.EventTriggerMode.TouchNormal;
        SetEventInfo(4, _4_还魂香, 还魂香);

        // 见石碑篇_赶乞丐
        var 酒菜 = GetEvent(2, _2_酒菜);
        酒菜.TriggerMode = Core.EventTriggerMode.TouchNormal;
        SetEventInfo(2, _2_酒菜, 酒菜);

        // 见石碑篇_去厨房帮大娘打下手
        var 执灶李大娘 = GetEvent(2, _2_执灶李大娘);
        执灶李大娘.TriggerMode = Core.EventTriggerMode.TouchFarthest;
        SetEventInfo(2, _2_执灶李大娘, 执灶李大娘);

        // 见石碑篇_拿十里香
        var 十里香 = GetEvent(2, _2_十里香);
        十里香.TriggerMode = Core.EventTriggerMode.TouchNormal;
        SetEventInfo(2, _2_十里香, 十里香);

        // 见石碑篇_大娘病倒了回客栈探望王小虎
        var 还神丹 = GetEvent(3, _3_还神丹);
        var 驱魔香 = GetEvent(3, _3_驱魔香);
        var 忘魂花 = GetEvent(3, _3_忘魂花);
        var 王小虎 = GetEvent(3, _3_王小虎);
        还神丹.TriggerMode = Core.EventTriggerMode.TouchNormal;
        驱魔香.TriggerMode = Core.EventTriggerMode.TouchNear;
        驱魔香.X = 1344;
        驱魔香.Y = 640;
        忘魂花.TriggerMode = Core.EventTriggerMode.TouchNormal;
        忘魂花.SpriteId = 482;
        王小虎.TriggerMode = Core.EventTriggerMode.TouchFarther;

        王小虎.SpriteId = 474;
        王小虎.Direction = TasDirection.Down;
        王小虎.CurrentFrameId = 0;
        王小虎.AutoScript = 0xA091;
        王小虎.FramesPerDirection = 4;
        //王小虎.State = Core.EventState.Obstacle;

        SetEventInfo(3, _3_还神丹, 还神丹);
        SetEventInfo(3, _3_驱魔香, 驱魔香);
        SetEventInfo(3, _3_忘魂花, 忘魂花);
        SetEventInfo(3, _3_王小虎, 王小虎);
    }
}

using PalTas.Records;

using static PalTas.TasScript.SceneEvent;

namespace PalTas;

public static partial class TasScript
{
    /// <summary>
    /// 通关进度
    /// </summary>
    public static TasProgress Progress { get; set; }
#if DEBUG
    = TasProgress.见石碑篇_初登岛_过草妖;
#endif // DEBUG

    /// <summary>
    /// 当前进度的子阶段编号
    /// </summary>
    public static double SubStageId { get; set; }

    /// <summary>
    /// 执行一帧的脚本
    /// </summary>
    public static void Run()
    {
        switch (Progress)
        {
            case TasProgress.见石碑篇_出房间:
                {
                    if (GetCurrentSceneId() == 2)
                    {
                        var 出房间李大娘 = GetCurrentSceneEvent(_2_出房间李大娘);
                        var 客栈大厅场景切换点 = GetCurrentSceneEvent(_2_逍遥房间到客栈大厅场景切换点);

                        if ((出房间李大娘.State == Core.EventState.Hidden) &&
                            (客栈大厅场景切换点.State == Core.EventState.NonObstacle))
                        {
                            SetWalkPlanning(TasWalkPlans[Progress][0]);
                            Progress = TasProgress.见石碑篇_接客;
                        }
                    }
                }
                break;

            case TasProgress.见石碑篇_接客:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = TasProgress.见石碑篇_下楼直走还魂香;
                }
                break;

            case TasProgress.见石碑篇_下楼直走还魂香:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = TasProgress.见石碑篇_大娘吩咐赶乞丐;
                }
                break;

            case TasProgress.见石碑篇_大娘吩咐赶乞丐:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 设置醉倒酒剑仙自动互动
                        var 醉倒酒剑仙 = GetEvent(4, _4_醉倒酒剑仙);
                        醉倒酒剑仙.TriggerMode = Core.EventTriggerMode.TouchNormal;
                        SetEventInfo(4, _4_醉倒酒剑仙, 醉倒酒剑仙);

                        Progress = TasProgress.见石碑篇_赶乞丐;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_赶乞丐:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 和乞丐互动结束后酒菜才会出现
                        var 酒菜 = GetEvent(2, _2_酒菜);
                        if (酒菜.State == Core.EventState.NonObstacle)
                        {
                            // 关闭醉倒酒剑仙自动互动
                            var 醉倒酒剑仙 = GetEvent(4, _4_醉倒酒剑仙);
                            醉倒酒剑仙.TriggerMode = Core.EventTriggerMode.SearchNear;
                            SetEventInfo(4, _4_醉倒酒剑仙, 醉倒酒剑仙);

                            Progress = TasProgress.见石碑篇_去厨房帮大娘打下手;
                            SubStageId = 0;
                        }
                    }
                }
                break;

            case TasProgress.见石碑篇_去厨房帮大娘打下手:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 和大娘同时进门，但比她更快一些，让她在门外罚站
                        var 去厨房的李大娘 = GetCurrentSceneEvent(_4_去厨房的李大娘);
                        if (去厨房的李大娘.X <= 1008 && 去厨房的李大娘.Y <= 1480 && 去厨房的李大娘.CurrentFrameId == 0)
                        {
                            SetWalkPlanning(TasWalkPlans[Progress][1]);

                            SubStageId = 2;
                        }
                    }
                    else if (SubStageId == 2)
                    {
                        // 对话完毕后才能拿酒菜
                        var 酒菜 = GetCurrentSceneEvent(_2_酒菜);
                        if (酒菜.TriggerScript == 0x0247)
                        {
                            // 开启执灶李大娘自动互动
                            var 执灶李大娘 = GetEvent(2, _2_执灶李大娘);
                            执灶李大娘.TriggerMode = Core.EventTriggerMode.SearchFar;
                            SetEventInfo(2, _2_执灶李大娘, 执灶李大娘);

                            Progress = TasProgress.见石碑篇_端酒菜;
                            SubStageId = 0;
                        }
                    }
                }
                break;

            case TasProgress.见石碑篇_端酒菜:
                {
                    if (SubStageId == 0)
                    {
                        // 开启互动拿起酒菜
                        var 酒菜 = GetEvent(2, _2_酒菜);
                        酒菜.TriggerMode = Core.EventTriggerMode.TouchNormal;
                        SetEventInfo(2, _2_酒菜, 酒菜);

                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);

                        Progress = TasProgress.见石碑篇_送餐;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_送餐:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = TasProgress.见石碑篇_拿十里香;
                }
                break;

            case TasProgress.见石碑篇_拿十里香:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);

                    Progress = TasProgress.见石碑篇_将桂花酒交于酒剑仙;
                }
                break;

            case TasProgress.见石碑篇_将桂花酒交于酒剑仙:
                {
                    if (SubStageId == 0)
                    {
                        // 开启醉倒酒剑仙自动互动
                        var 醉倒酒剑仙 = GetEvent(4, _4_醉倒酒剑仙);
                        醉倒酒剑仙.TriggerScript = 0x028A;
                        醉倒酒剑仙.TriggerMode = Core.EventTriggerMode.TouchFar;
                        SetEventInfo(4, _4_醉倒酒剑仙, 醉倒酒剑仙);

                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 删除道具：桂花酒
                        RemoveInventoryItem(TasItems.桂花酒);

                        Progress = TasProgress.见石碑篇_不予理会直接出客栈;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_不予理会直接出客栈:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);

                        Progress = TasProgress.见石碑篇_大娘病倒了回客栈探望王小虎;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_大娘病倒了回客栈探望王小虎:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 检查胖喵是否已经出现在客栈门口
                        var 胖喵_树欲静而风不止 = GetEvent(4, _4_胖喵_树欲静而风不止);
                        if (胖喵_树欲静而风不止.TriggerScript == 0x042E)
                        {
                            var 王小虎 = GetEvent(3, _3_王小虎);
                            王小虎.TriggerMode = Core.EventTriggerMode.SearchNormal;
                            SetEventInfo(3, _3_王小虎, 王小虎);
                            王小虎.AutoScript = 0x0C03;

                            SetWalkPlanning(TasWalkPlans[Progress][2]);

                            Progress = TasProgress.见石碑篇_张四哥救人如救驾;
                            SubStageId = 0;
                        }
                    }
                }
                break;

            case TasProgress.见石碑篇_张四哥救人如救驾:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 设置张四哥自动互动
                        var 张四哥 = GetEvent(6, _6_张四哥);
                        张四哥.TriggerMode = Core.EventTriggerMode.TouchFarther;
                        SetEventInfo(6, _6_张四哥, 张四哥);

                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        SubStageId = 2;
                    }
                    else if (SubStageId == 2)
                    {
                        // 检查张四哥是否消失（被“撑蒿张四哥”换班站岗）
                        var 张四哥 = GetEvent(6, _6_张四哥);
                        if (张四哥.State == Core.EventState.Hidden)
                        {
                            张四哥.TriggerMode = Core.EventTriggerMode.SearchNormal;
                            SetEventInfo(6, _6_张四哥, 张四哥);

                            SetWalkPlanning(TasWalkPlans[Progress][2]);

                            Progress = TasProgress.见石碑篇_张四哥救人如救驾;
                            SubStageId = 0;
                        }
                    }
                }
                break;

            case TasProgress.见石碑篇_初登岛_过草妖:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);

                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        Progress = TasProgress.见石碑篇_初登岛_过迷阵;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_初登岛_过迷阵:
                {

                }
                break;
        }
    }
}

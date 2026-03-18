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
        = TasProgress.见石碑篇_下楼直走还魂香;
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
                        SetCurrentSceneEventTriggerMode(_4_醉倒酒剑仙, Core.EventTriggerMode.TouchNormal);

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
                        // 对话结束后酒菜才会出现
                        var 酒菜 = GetEvent(2, _2_酒菜);
                        if (酒菜.State == Core.EventState.NonObstacle)
                        {
                            SetCurrentSceneEventTriggerMode(_4_醉倒酒剑仙, Core.EventTriggerMode.SearchNear);

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
                            SetCurrentSceneEventTriggerMode(_2_执灶李大娘, Core.EventTriggerMode.SearchFar);

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
                        SetCurrentSceneEventTriggerMode(_2_酒菜, Core.EventTriggerMode.TouchNormal);

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
                    if (SubStageId == 0)
                    {
                        SetCurrentSceneEventTriggerMode(_2_十里香, Core.EventTriggerMode.TouchNormal);

                        SetWalkPlanning(TasWalkPlans[Progress][0]);

                        Progress = TasProgress.见石碑篇_将桂花酒交于酒剑仙;
                        SubStageId = 0;
                        //SubStageId = 1;
                    }
                    //else if (SubStageId == 1)
                    //{
                    //    var 床头十里香 = GetCurrentSceneEvent(15);
                    //    if (床头十里香.State == Core.EventState.Hidden)
                    //    {
                    //        Progress = TasProgress.见石碑篇_将桂花酒交于酒剑仙;
                    //        SubStageId = 0;
                    //    }
                    //}
                }
                break;

            case TasProgress.见石碑篇_将桂花酒交于酒剑仙:
                {
                    if (SubStageId == 0)
                    {
                        SetEventTriggerScript(4, _4_醉倒酒剑仙, 0x028A);
                        SetEventTriggerMode(4, _4_醉倒酒剑仙, Core.EventTriggerMode.TouchFar);

                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        // 删除道具：桂花酒
                        RemoveInventoryItem(TasItems.桂花酒);

                        Progress = TasProgress.见石碑篇_不予理会直接出门;
                        SubStageId = 0;
                    }
                }
                break;

            case TasProgress.见石碑篇_不予理会直接出门:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);

                        Progress = TasProgress.NULL;
                        SubStageId = 0;
                    }
                }
                break;
        }
    }
}

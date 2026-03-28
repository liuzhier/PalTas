using System.Threading;
using System.Threading.Tasks;
using static PalTas.TasCore.TasScript.SceneEvent;
using static PalTas.TasCore.TasScript.TasProgress;

namespace PalTas.TasCore;

public static partial class TasScript
{
    /// <summary>
    /// 通关进度
    /// </summary>
    public static TasProgress Progress { get; set; }

    /// <summary>
    /// 当前进度的子阶段编号
    /// </summary>
    public static double SubStageId { get; set; }

    /// <summary>
    /// 执行一帧的脚本
    /// </summary>
    public static async Task Run(CancellationToken token)
    {
        switch (Progress)
        {
            case 见石碑篇_出房间:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_接客;
                }
                break;

            case 见石碑篇_接客:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_下楼直走还魂香;
                }
                break;

            case 见石碑篇_下楼直走还魂香:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_大娘吩咐赶乞丐;
                }
                break;

            case 见石碑篇_大娘吩咐赶乞丐:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_赶乞丐;
                }
                break;

            case 见石碑篇_赶乞丐:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_去厨房帮大娘打下手;
                }
                break;

            case 见石碑篇_去厨房帮大娘打下手:
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
                        Progress = 见石碑篇_端酒菜;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_端酒菜:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        Progress = 见石碑篇_送餐;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_送餐:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_拿十里香;
                }
                break;

            case 见石碑篇_拿十里香:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);

                    Progress = 见石碑篇_将桂花酒交于酒剑仙;
                }
                break;

            case 见石碑篇_将桂花酒交于酒剑仙:
                {
                    SetWalkPlanning(TasWalkPlans[Progress][0]);
                    Progress = 见石碑篇_不予理会直接出客栈;
                    SubStageId = 0;
                }
                break;

            case 见石碑篇_不予理会直接出客栈:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);

                        Progress = 见石碑篇_大娘病倒了回客栈探望王小虎;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_大娘病倒了回客栈探望王小虎:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        Progress = 见石碑篇_张四哥救人如救驾;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_张四哥救人如救驾:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        Progress = 见石碑篇_初登岛_过草妖;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_初登岛_过草妖:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        Progress = 见石碑篇_初登岛_过迷阵;
                        SubStageId = 0;
                    }
                }
                break;

            case 见石碑篇_初登岛_过迷阵:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if(SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        SubStageId = 2;
                    }
                    else if (SubStageId == 2)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][2]);
                        SubStageId = 3;
                    }
                    else if (SubStageId == 3)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][3]);
                        SubStageId = 4;
                    }
                    else if (SubStageId == 4)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][4]);
                        SubStageId = 5;
                    }
                    else if (SubStageId == 5)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][5]);
                        SubStageId = 6;
                    }
                    else if (SubStageId == 6)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][6]);
                        Progress = 学功夫篇_初登岛_结婚;
                        SubStageId = 0;
                    }
                }
                break;

            case 学功夫篇_初登岛_结婚:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        SubStageId = 2;
                    }
                    else if (SubStageId == 2)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][2]);
                        Progress = 学功夫篇_离岛;
                        SubStageId = 0;
                    }
                }
                break;

            case 学功夫篇_离岛:
                {
                    if (SubStageId == 0)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][0]);
                        SubStageId = 1;
                    }
                    else if (SubStageId == 1)
                    {
                        SetWalkPlanning(TasWalkPlans[Progress][1]);
                        SubStageId = 2;
                    }
                }
                break;
        }
    }
}

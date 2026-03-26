using System.Threading;
using System.Threading.Tasks;
using static PalTas.TasCore.Records.Game;
using static Vanara.PInvoke.User32;

namespace PalTas.TasCore;

public static partial class TasScript
{
    /// <summary>
    /// 是否正在运行自动化战斗
    /// </summary>
    public static bool BattleIsRunning { get; set; }

    /// <summary>
    ///  是否刚进入战斗
    /// </summary>
    public static bool BattleBeginning { get; set; } = true;

    /// <summary>
    /// 当前回合数
    /// </summary>
    public static int BattleScriptRound { get; set; }

    /// <summary>
    /// 当前回合数自定义快捷键动作
    /// </summary>
    public static RShortcutKeyRoundAction ShortcutKeyRoundAction { get; set; } = null!;

    /// <summary>
    /// 初始化自动化战斗模块
    /// </summary>
    public static void InitBattle()
    {
        // 初始化战斗回合数，以供判定回合数
        SetBattleStatus(false);

        // 禁用随机功能，随机结果由 TAS 程序托管
        DisableRandom(true);
        //DisableRandom(false);               // 恢复随机功能

        // 将所有敌人的巫抗设置为 0
        for (var i = TasEnemys.史莱姆; i <= TasEnemys.拜月教主; i++)
            SetEnemyResilience(i, 0);

        // 取消回梦脚本中的失误判定
        SetScriptEntry(0xA84D, [
            0x0068, 0xA840, 0x0000, 0x0000,     // 目标若为我方则跳转   A840
            0x002E, 0x0002, 0x0004, 0x977F,     // 设置敌方状态   昏睡  4  977F
            0x0000, 0x0000, 0x0000, 0x0000,     // ===========================================
        ]);

        // 取消夺魂脚本中的失误判定
        SetScriptEntry(0xA86E, [
            0x0068, 0xA840, 0x0000, 0x0000,     // 目标若为我方则跳转   A840
            0x002E, 0x0000, 0x0000, 0xA840,     // 设置敌方状态   疯魔  0  A840
            0x0060, 0x0000, 0x0000, 0x0000,     // 敌方阵亡
            0x0000, 0x0000, 0x0000, 0x0000,     // ===========================================
        ]);

        // 赤鬼王测试
        var addr = (uint)TasEnemys.赤鬼王 * 0x000E;
        TasMemory.WriteUInt16(TasMemory.EntityDataAddr, 0xA60D, addr + sizeof(ushort) * 2);
        // 只会普攻
        SetScriptEntry(0xA60E, [
            0x0000, 0x0000, 0x0000, 0x0000,     // ===========================================
        ]);
    }

    /// <summary>
    /// 设置战斗状态为开始/结束
    /// </summary>
    /// <param name="isBeginning">true 开始；false 结束</param>
    public static void SetBattleStatus(bool isBeginning)
    {
        BattleIsRunning = isBeginning;      // 战斗正在运行的开关
        BattleBeginning = !isBeginning;     // 战斗刚刚启动的开关
        NextRound();                        // 更新回合状态

        if (isBeginning)
            // 给所有人加上 32767 回合天罡、醉仙
            for (var i = 0; i < MAX_MEMBER_IN_TEAM_COUNT; i++)
            {
                SetMemberSpecialStatus(i, TasSpecialStatus.力拔山河, 0x7FFF);
                SetMemberSpecialStatus(i, TasSpecialStatus.动若脱兔, 0x7FFF);
            }
    }

    /// <summary>
    /// 回合结束，更新战斗状态到下一回合
    /// </summary>
    public static void NextRound()
    {
        ShortcutKeyRoundAction = null!;                                 // 重制快捷键状态
        BattleScriptRound = (IsInBattle) ? ++BattleScriptRound : 0;     // 更新回合数统计
        SetCurrentActorSelectorId(TasBattleFighter.NULL);               // 设置当前不该选择动作
        SetCurrentActorId(TasBattleFighter.NULL);                       // 设置当前没有人正在行动
    }

    /// <summary>
    /// 战斗回合控制
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    public static async Task RunBattle(CancellationToken token)
    {
        if (BattleBeginning)
        {
            // 战斗刚刚启动
            // 初始化回合状态
            SetBattleStatus(true);
            BattleScriptRound = 0;

            // 等待允许选择动作
            while ((CurrentActorSelectorId == TasBattleFighter.NULL) && IsInBattle) await Delay(1, token);
        }

        // 自动选择角色本回合需要执行的动作
        BattleAutoSelectAction();

        // 将我方标记为动作选择完毕
        var currentActorSelectorId = ShortcutKeyRoundAction?.FighterId ?? TasBattleFighter.Last;
        SetCurrentActorSelectorId(currentActorSelectorId);

        // 让我方行动起来
        var key = (VK)(ShortcutKeyRoundAction?.Key ?? TasActionShortcutKeys.NULL);
        while ((CurrentActorId == TasBattleFighter.NULL) && IsInBattle)
        {
            // 若我方没有开始行动则不停按快捷键
            if (key == (VK)TasActionShortcutKeys.NULL)
            {
                // 随便按一个动作回退键触发回合开始
                PressKey(VK.VK_ESCAPE);
                await Delay(30, token);
                ReleaseKey(VK.VK_ESCAPE);

                PressKey(VK.VK_LMENU);
                await Delay(30, token);
                ReleaseKey(VK.VK_LMENU);
            }
            else
            {
                // 为该队员指定了快捷键
                PressKey(key);
                await Delay(30, token);
                ReleaseKey(key);
            }

            await Delay(1, token);
        }

        // 检查回合中哪一方在行动
        while ((CurrentActorSelectorId != TasBattleFighter.First) && IsInBattle)
        {
            // 检查该哪一方行动，我方永远最幸运，敌方永远最倒霉
            if (CurrentActorTeam == TasBattleTeam.我方)
            {
                var memberRoundAction = GetMemberRoundAction((int)CurrentActorId);

                switch (memberRoundAction.MemberAction)
                {
                    case TasMemberActions.普攻:
                        // 这样设置仍然是错误的，应该检查 Hero 编号，而不是 Member 编号
                        // 今天太累惹．．．．．．明天在改叭．．．．．．
                        SetRandomResult((CurrentActorId == TasBattleFighter.First) ? .3333333135f : 1);
                        break;

                    case TasMemberActions.防御:
                        SetRandomResult(.6500000358f);
                        break;

                    case TasMemberActions.进攻仙术:
                        if (memberRoundAction.EntityId.MagicId == TasMagics.飞龙探云手)
                            SetRandomResult(0);
                        else
                            SetRandomResult(1);
                        break;

                    default:
                        SetRandomResult(1);
                        break;
                }
            }
            else AutoRandomNext();      // 托管敌人的随机

            //await Delay(1, token);
        }

        // 回合结束，更新回合状态
        NextRound();
    }

    /// <summary>
    /// 运行一回合的自动战斗脚本
    /// </summary>
    public static void BattleAutoSelectAction()
    {
        // 直接把所有人动作全选择完
        switch (Progress)
        {
            case TasProgress.见石碑篇_初登岛_过草妖:
                {
                    BattleScriptRound = 0;
                    if (BattleScriptRound == 0)
                    {
                        // 直接砍
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._0,
                            MemberAction = TasMemberActions.普攻,
                        });
                    }
                    else if (BattleScriptRound == 1)
                    {
                        // 梦蛇
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._0,
                            MemberAction = TasMemberActions.防守仙术,
                            EntityId = new()
                            {
                                MagicId = TasMagics.梦蛇,
                            },
                        });
                    }
                    else
                    {
                        // 御剑伏魔1
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._0,
                            MemberAction = TasMemberActions.进攻仙术,
                            EntityId = new()
                            {
                                MagicId = TasMagics.御剑伏魔,
                            },
                        });
                    }
                }
                break;

            case TasProgress.赤鬼王测试:
                {
                    BattleScriptRound = 1;
                    if (BattleScriptRound == 0)
                    {
                        // A
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.普攻,
                        });
                        // 木
                        SetMemberRoundAction(1, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.投掷道具,
                            EntityId = new()
                            {
                                ItemId = TasItems.木剑,
                            },
                        });
                        // 木
                        SetMemberRoundAction(2, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.投掷道具,
                            EntityId = new()
                            {
                                ItemId = TasItems.木剑,
                            },
                        });
                    }
                    else if (BattleScriptRound == 1)
                    {
                        // 飞龙探云手
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.进攻仙术,
                            EntityId = new()
                            {
                                MagicId = TasMagics.飞龙探云手,
                            },
                        });
                        // 回梦
                        SetMemberRoundAction(1, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.进攻仙术,
                            EntityId = new()
                            {
                                MagicId = TasMagics.回梦,
                            },
                        });
                        // R
                        ShortcutKeyRoundAction = new()
                        {
                            FighterId = TasBattleFighter._2,
                            Key = TasActionShortcutKeys.重复,
                        };
                    }
                    else if (BattleScriptRound <= 3)
                    {
                        // R
                        ShortcutKeyRoundAction = new()
                        {
                            FighterId = TasBattleFighter._0,
                            Key = TasActionShortcutKeys.重复,
                        };
                    }
                    else if (BattleScriptRound == 4)
                    {
                        // A
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.普攻,
                        });
                        // 柳月刀
                        SetMemberRoundAction(1, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.投掷道具,
                            EntityId = new()
                            {
                                ItemId = TasItems.柳月刀,
                            },
                        });
                        // R
                        ShortcutKeyRoundAction = new()
                        {
                            FighterId = TasBattleFighter._2,
                            Key = TasActionShortcutKeys.重复,
                        };
                    }
                    else if (BattleScriptRound == 5)
                    {
                        // A
                        SetMemberRoundAction(0, new()
                        {
                            TargetOfAttack = TasTargetOfAttack._1,
                            MemberAction = TasMemberActions.普攻,
                        });
                        // Q
                        ShortcutKeyRoundAction = new()
                        {
                            FighterId = TasBattleFighter._2,
                            Key = TasActionShortcutKeys.逃跑,
                        };
                    }
                }
                break;
        }
    }
}

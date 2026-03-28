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
    /// 进入战斗的脚本地址
    /// </summary>
    public static int TriggerBattleScriptAddressBackup { get; set; }

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

        //// 赤鬼王测试
        //var addr = (uint)TasEnemys.赤鬼王 * 0x000E;
        //TasMemory.WriteUInt16(TasMemory.ReadUInt32(TasMemory.EntityDataAddr), 0xA60D, addr + sizeof(ushort) * 2);
        //// 只会普攻
        //SetScriptEntry(0xA60E, [
        //    0x0000, 0x0000, 0x0000, 0x0000,     // ===========================================
        //]);

        // 让灵儿不再挡道
        SetScriptEntry(0x15A9, [
            0x0049, 0x00DE, 0x0001, 0x0000,     // 设置对象状态   赵灵儿  漂浮
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
        {
            // 给所有人加上 32767 回合天罡、醉仙
            for (var i = 0; i < MAX_MEMBER_IN_TEAM_COUNT; i++)
            {
                SetMemberSpecialStatus(i, TasSpecialStatus.力拔山河, 0x7FFF);
                //SetMemberSpecialStatus(i, TasSpecialStatus.坚如磐石, 0x7FFF);
                //SetMemberSpecialStatus(i, TasSpecialStatus.身轻如燕, 0x7FFF);
                //SetMemberSpecialStatus(i, TasSpecialStatus.动若脱兔, 0x7FFF);
            }

            // 等待完全进入战斗
            do
            {
                // 备份进入战斗的脚本地址
                TriggerBattleScriptAddressBackup = TriggerBattleScriptAddress;
            } while (TriggerBattleScriptAddress == 0xFFFF);

            // 将我方先手概率提到最高，战前预算超常发挥时的实际身法，并加上真实的身法值以达到必定超常发挥的效果
            var 李逍遥额外身法 = (short)float.Ceiling(GetHeroActualAttribute(TasHero.李逍遥, TasHeroAttribute.身法) * 2f / 9);
            var 赵灵儿额外身法 = (short)float.Ceiling(GetHeroActualAttribute(TasHero.赵灵儿, TasHeroAttribute.身法) * 2f / 9);
            SetHeroTempAttribute(TasHero.李逍遥, TasHeroExtraAttribute.身法, 李逍遥额外身法);
            SetHeroTempAttribute(TasHero.赵灵儿, TasHeroExtraAttribute.身法, 赵灵儿额外身法);
        }
    }

    /// <summary>
    /// 回合结束，更新战斗状态到下一回合
    /// </summary>
    public static void NextRound()
    {
        ShortcutKeyRoundAction = null!;                                     // 重制快捷键状态
        BattleScriptRound = (IsInBattle) ? ++BattleScriptRound : -1;        // 更新回合数统计
        SetCurrentActorSelectorId(TasBattleFighter.Last);                   // 设置当前不该选择动作
        SetCurrentActorId(TasBattleFighter.NULL);                           // 设置当前没有人正在行动
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

            // 等待允许选择动作
            goto AwaitActionSelection;
        }

        return;

        // 自动选择队员本回合需要执行的动作
        BattleAutoSelectAction();

        // 将我方标记为动作选择完毕
        SetCurrentActorSelectorId(TasBattleFighter.Last);

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

                // 解决回合行动阶段，队员头上出现光标的问题
                SetCurrentActorSelectorId(TasBattleFighter.Last);
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

    AwaitActionSelection:
        // 保证我方总是先手
        SetRandomResult(0);

        // 等待允许选择动作
        //while ((CurrentActorSelectorId != TasBattleFighter.First) && IsInBattle)
        //{
        //    // 跳过随时可能出现的条幅框
        //    PressKey(VK.VK_RETURN);
        //    await Delay(1, token);
        //    ReleaseKey(VK.VK_RETURN);

        //    PressKey(VK.VK_SPACE);
        //    await Delay(1, token);
        //    ReleaseKey(VK.VK_SPACE);

        //    await Delay(1, token);
        //}

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
                    // A
                    SetMemberRoundAction(0, new()
                    {
                        MemberAction = TasMemberActions.逃跑,
                    });
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

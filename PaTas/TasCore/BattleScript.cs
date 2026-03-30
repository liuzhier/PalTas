using System;
using System.Collections.Generic;
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
    /// 队员们对应的角色编号
    /// </summary>
    public static Dictionary<TasHero, TasBattleFighter> MembersHeroId { get; set; } = [];

    /// <summary>
    /// 初始化自动化战斗模块
    /// </summary>
    public static void InitBattle()
    {
        // 初始化战斗回合数，以供判定回合数
        SetBattleStatus(false);

        // 禁用随机功能，随机结果由 TAS 程序托管
        DisableRandom(true);

        // 将所有巫抗不为 10 的敌人巫抗设置为 0
        for (var i = TasEnemys.史莱姆; i <= TasEnemys.拜月教主; i++)
            if (GetEnemyResilience(i) != 10)
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

        // 蜜蜂必定掉落
        var 蜜蜂 = GetEnemyEntity(TasEnemys.蜜蜂);
        蜜蜂.Script.BattleWon++;
        SetEnemyEntity(TasEnemys.蜜蜂, 蜜蜂);

        // 蛹（蜂蛹）必定掉落
        var 蛹 = GetEnemyEntity(TasEnemys.蛹);
        蛹.Script.BattleWon++;
        SetEnemyEntity(TasEnemys.蛹, 蛹);

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

        // 判断刚刚进入战斗，有些数据需要初始化
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

            // 备份队友对应的角色编号
            var membersHeroId = new TasHero[BattleMemberMaxId];
            for (var i = 0; i <= BattleMemberMaxId; i++)
                membersHeroId[i] = GetMemberTrailRelativeToViewport(i).HeroId;
            MembersHeroId = [];
            for (var i = TasHero.李逍遥; i <= TasHero.盖罗娇; i++)
                for (var j = TasBattleFighter.First; j <= TasBattleFighter.Last; j++)
                    if (membersHeroId[(int)j] == i)
                        MembersHeroId[i] = j;
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
    /// 计算所需额外武术值
    /// </summary>
    /// <param name="heroAttackStrength">角色武术</param>
    /// <param name="enemyDefense">敌方防御</param>
    /// <param name="enemyLevel">敌方等级</param>
    /// <param name="enemyPhysicalResistance">敌方物抗</param>
    /// <returns>所需额外武术值</returns>
    public static short CalcRequiredAttackStrength(short heroAttackStrength, short enemyDefense, ushort enemyLevel, short enemyPhysicalResistance)
    {
        // --- 步骤 1: 计算敌方实际防御 D ---
        // 对应文档 C38: D = 敌方防御 + (敌方等级 + 6) * 4
        var enemyActualDefense = enemyDefense + (enemyLevel + 6) * 4;

        // --- 步骤 2: 计算当前基础伤害 B0 ---
        // 对应文档 C40 以及 C11-C12 的分段公式
        var B0 = 0f;

        if (heroAttackStrength > enemyActualDefense)
            // 公式: A - 0.8D (最常见的情况)
            B0 = heroAttackStrength - 0.8f * enemyActualDefense;
        else if (heroAttackStrength > 0.6f * enemyActualDefense)
            // 公式: 0.5A - 0.3D
            B0 = 0.5f * heroAttackStrength - 0.3f * enemyActualDefense;
        else
            // 此时基础伤害为 0，破不了防
            B0 = 0;

        // --- 步骤 3: 计算目标基础伤害 B_target ---
        // 对应文档 C42: B_target ≈ 1.08000000257 * B0 + 0.41333334582 * R
        // 这里保留了文档中的高精度系数，确保计算精准
        var B_target = 1.08000000257f * B0 + 0.41333334582f * enemyPhysicalResistance;

        // --- 步骤 4: 反推目标武术 A_target ---
        // 对应文档 C44-C46 的分段反推
        var A_target = 0f;

        // 判定条件：B_target 是否大于 0.2D
        if (B_target > 0.2f * enemyPhysicalResistance)
            // 对应文档 C44: 说明目标在第三段 (A' > D)
            // 公式: A_target = B_target + 0.8D
            A_target = B_target + 0.8f * enemyActualDefense;
        else
            // 对应文档 C46: 说明目标在第二段 (0.6D < A' <= D)
            // 公式: A_target = 2 * B_target + 0.6D
            A_target = 2.0f * B_target + 0.6f * enemyActualDefense;

        // --- 步骤 5: 计算差值并向上取整 ---
        // 对应文档 C48: ΔA = ceil(A_target) - A
        // 使用 Math.Ceiling 向上取整，确保伤害至少持平
        var delta = Math.Ceiling(A_target) - heroAttackStrength;

        // 如果算出负数（比如属性已经溢出了），则不需要加
        return short.Max(0, (short)delta);
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

#if !ONLY_AUXILIARY_MODE
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
#endif  // !ONLY_AUXILIARY_MODE

    AwaitActionSelection:
        // 保证我方总是先手
        SetRandomResult(0);

        // 等待允许选择动作
        while ((CurrentActorSelectorId != TasBattleFighter.First) && IsInBattle)
        {
            // 跳过随时可能出现的条幅框
            PressKey(VK.VK_RETURN);
            await Delay(1, token);
            ReleaseKey(VK.VK_RETURN);

            PressKey(VK.VK_SPACE);
            await Delay(1, token);
            ReleaseKey(VK.VK_SPACE);

            await Delay(1, token);
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

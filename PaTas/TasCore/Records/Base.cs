using System.Runtime.InteropServices;

namespace PalTas.TasCore.Records;

public static unsafe class Base
{
    public const   int
        MaxShopItem         = 9,
        MaxEnemysInTeam     = 5,
        MaxHero             = 6,
        MaxHeroesInTeam     = 5,
        MagicElementalNum   = 5,
        MaxHeroMagic        = 32,
        MaxScenes           = 300,
        MaxEffectiveScenes  = MaxScenes - 1,
        MaxLevel            = 99;


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct REnemy
    {
        public  ushort              IdleFrames;                                 // 原地蠕动帧数
        public  ushort              MagicFrames;                                // 施法帧数
        public  ushort              AttackFrames;                               // 攻击帧数
        public  ushort              IdleAnimSpeed;                              // 原地蠕动动画每帧间隔节拍
        public  ushort              ActWaitFrames;                              // 行动动画每帧间隔节拍
        public  short               YPosOffset;                                 // Y 轴偏移
        public  short               AttackSound;                                // 音效：普攻
        public  short               ActionSound;                                // 音效：行动
        public  short               MagicSound;                                 // 音效：施法
        public  short               DeathSound;                                 // 音效：阵亡
        public  short               CallSound;                                  // 音效：进入战场时呼喊
        public  ushort              MaxHP;                                      // 最大体力
        public  ushort              Exp;                                        // 战利品：经验值
        public  ushort              Cash;                                       // 战利品：金钱
        public  ushort              Level;                                      // 修行
        public  short               MagicId;                                    // 法术
        public  ushort              MagicRate;                                  // 施法概率
        public  ushort              AttackEquivItemId;                          // 普攻附带道具
        public  ushort              AttackEquivItemRate;                        // 普攻附带道具概率
        public  ushort              StealItemId;                                // 可偷道具
        public  ushort              StealItemCount;                             // 可偷道具数量
        public  CEnemyAttribute     Attribute;                                  // 五维（武灵防速逃）
        public  short               PoisonResistance;                           // 毒抗
        public  fixed   short       ElementalResistance[MagicElementalNum];     // 灵抗
        public  short               PhysicalResistance;                         // 物抗
        public  ushort              DualMove;                                   // 每回合是否能连续行动两次
        public  ushort              CollectValue;                               // 灵葫能量
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CEnemyAttribute
    {
        public  short       AttackStrength;     // 武术
        public  short       MagicStrength;      // 灵力
        public  short       Defense;            // 防御
        public  short       Dexterity;          // 身法
        public  short       FleeRate;           // 吉运
    }
}

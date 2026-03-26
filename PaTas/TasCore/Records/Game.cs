using System.Runtime.InteropServices;

namespace PalTas.TasCore.Records;

public static class Game
{
    /// <summary>
    /// 敌方战斗时的临时数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct REnemyBattleTempData
    {
        public  short                   EnemyBaseDataId;        // 基础数据编号（在 DATA.MKF#1）
        public  RPos                    Pos;                    // 当前在战场上的坐标 XY
        public  RPos                    OriginPos;              // 初始坐标 XY（备份，行动后归位用）
        public  short                   FrameId;                // 当前帧编号（实时渲染时用）
        readonly    short               _unkown6;               // ？？？
        readonly    short               _unkown7;               // ？？？（和帧编号相关）
        readonly    short               _unkown8;               // ？？？
        public  short                   HP;                     // 剩余 HP
        public  short                   EnemyId;                // 敌人实体编号
        public  Entity.REnemyScript     Script;                 // 各种脚本
    }

    /// <summary>
    /// 我方当前回合的动作
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct RHeroRoundActionEntity
    {
        [FieldOffset(0)]
        public  short           RawValue;       // 原生数值
        [FieldOffset(0)]
        public  TasItems        ItemId;         // 道具编号
        [FieldOffset(0)]
        public  TasMagics       MagicId;        // 仙术编号
    }

    /// <summary>
    /// 我方队员当前回合的动作
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RMemberRoundAction
    {
        public  TasTargetOfAttack           TargetOfAttack;     // 作用目标编号
        public  TasMemberActions            MemberAction;       // 实际动作
        public  RHeroRoundActionEntity      EntityId;           // 使用的仙术/道具实体编号
        public  ushort                      MenuCursorId;       // 使用的实体是仙术/道具菜单中的第几个
        readonly    short                   _unkown4;           // ？？？
    }

    /// <summary>
    /// 回合动作快捷键（自定义的，内存里没有这个东西）
    /// </summary>
    public class RShortcutKeyRoundAction
    {
        public  TasBattleFighter            FighterId;      // 从哪个队员开始按下
        public  TasActionShortcutKeys       Key;            // 按下行动快捷键
    }
}

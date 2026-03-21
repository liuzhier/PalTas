using System.Runtime.InteropServices;

namespace PalTas.Records;

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
        readonly  short                 _unkown4;               // ？？？
        readonly  short                 _unkown5;               // ？？？（和帧编号相关）
        readonly  short                 _unkown6;               // ？？？
        public  short                   HP;                     // 剩余 HP
        public  short                   EnemyId;                // 敌人实体编号
        public  Entity.REnemyScript     Script;                 // 各种脚本
    }
}

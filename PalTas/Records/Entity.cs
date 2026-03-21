using System.Runtime.InteropServices;

namespace PalTas.Records;

public class Entity
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct REnemy
    {
        public  ushort              EnemyDataId;                // 敌方基础数据（在 DATA.MKF #1）
                                                                // 同时也代表敌方图像（在 ABC.MKF）
        public  short               ResistanceToSorcery;        // 巫抗（0～10）
        public  REnemyScript        Script;                     // 各种脚本
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct REnemyScript
    {
        public  ushort      ScriptOnTurnStart;          // 回合开始脚本
        public  ushort      ScriptOnBattleWon;          // 战斗结算脚本
        public  ushort      ScriptOnAction;             // 回合行动脚本（出招脚本）
        readonly short      _unkown3;                   // 未知脚本
    }
}

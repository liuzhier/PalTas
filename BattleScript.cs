using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace PalTas;

public static partial class TasScript
{
    public static void RunBattle()
    {
        switch (Progress)
        {
            case TasProgress.见石碑篇_初登岛_过草妖:
                {
                    // 直接砍
                    PressKey(VK.VK_SPACE);
                    //if (BattleScriptRound == 1)
                    //{
                    //    // 直接砍
                    //    PressKey(VK.VK_Enter);

                    //    // 执行下一回合
                    //    BattleScriptRound = 1;
                    //}
                }
                break;
        }
    }
}

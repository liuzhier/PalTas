using System.Threading;
using System.Threading.Tasks;
using static Vanara.PInvoke.User32;

namespace PalTas;

public static partial class TasScript
{
    public static async Task RunBattle(CancellationToken token)
    {
        switch (Progress)
        {
            case TasProgress.见石碑篇_初登岛_过草妖:
                {
                    // 直接逃跑
                    PressKey(VK.VK_Q);
                    //await Delay(1, token);
                    //ReleaseKey(VK.VK_Q);

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

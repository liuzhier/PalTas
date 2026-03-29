using Avalonia.Controls;
using Avalonia.Interactivity;
using PalTas.TasCore;
using PalTas.TasCore.Records;
using System.Threading;
using System.Threading.Tasks;

namespace PalTas;

public partial class MainWindow : Window
{
    bool IsClosing { get; set; } = false;

    /// <summary>
    /// 初始化窗口组件
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 开始按钮被点击
    /// </summary>
    /// <param name="sender">控件自身</param>
    /// <param name="e">事件状态信息</param>
    async void BeginButton_Click(object? sender, RoutedEventArgs e)
    {
        // 禁用当前按钮
        (sender as Button)?.IsEnabled = false;

        // 运行 Tas 核心
        var token = TasMain.TasToken.Token;
        var tasCoreMain =  Task.Run(async() => await TasMain.TasCoreMain());
        var readAndUpdateCurrentGameStatus = Task.Run(async() => await ReadAndUpdateCurrentGameStatus(token));

        // 等待所有任务完成清理（超时可选）
        await Task.WhenAll(tasCoreMain, readAndUpdateCurrentGameStatus);
    }

    /// <summary>
    /// 窗口被关闭
    /// </summary>
    /// <param name="sender">控件自身</param>
    /// <param name="e">事件状态信息</param>
    void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        if (IsClosing)
            // 避免多次被关闭
            return;

        // 设置正在关闭状态
        IsClosing = true;

        // 关闭 Tas 核心
        TasMain.Exit();

        // 检查所有任务是否结束
        var i = 0;
        while (TasMain.IsRunning)
        {
            if (i++ > 1000) return;
            Sleep();
        }
    }

    /// <summary>
    /// 读取并更新当前游戏状态
    /// </summary>
    /// <param name="token">任务令牌</param>
    /// <returns>当前任务</returns>
    async Task ReadAndUpdateCurrentGameStatus(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (WindowHandle != 0)
            {
                UiUpdate(async () =>
                {
                    CheckSceneSwitched();
                    GameStatus_Label.Content = $"游戏{((CurrentSceneId == 0) || (CurrentSceneId == -1) ? "未" : "已")}开始．．．";
                    CurrentScene_Label.Content = Description.Scenes.TryGetValue(CurrentSceneId, out var sceneName) ? sceneName : "未知";

                    var current = GetLeaderActualPosition();
                    PosX_Label.Content = $"{current.X}";
                    PosY_Label.Content = $"{current.Y}";

#if ONLY_AUXILIARY_MODE
                    CurrentProgress_Label.Content = "仅辅助模式不进行判断";
                    CurrentWalkDirection_Label.Content = "仅辅助模式不进行判断";
#else
                    CurrentProgress_Label.Content = TasScript.Progress;
                    CurrentWalkDirection_Label.Content = CurrentDirection switch
                    {
                        TasDirection.Down => "↙",
                        TasDirection.Left => "↖",
                        TasDirection.Up => "↗",
                        TasDirection.Right => "↘",
                        TasDirection.Current or _ => "无",
                    };
#endif  // ONLY_AUXILIARY_MODE
                });

                await Delay(50, token);
            }
        }
    }
}
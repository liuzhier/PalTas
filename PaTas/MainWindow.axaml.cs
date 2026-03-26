using Avalonia.Controls;
using Avalonia.Interactivity;
using PalTas.TasCore;
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
    private async void BeginButton_Click(object? sender, RoutedEventArgs e)
    {
        // 禁用当前按钮
        (sender as Button)?.IsEnabled = false;

        // 运行 Tas 核心
        await Task.Run(async () => await TasMain.TasCoreMain());
    }

    /// <summary>
    /// 窗口被关闭
    /// </summary>
    /// <param name="sender">控件自身</param>
    /// <param name="e">事件状态信息</param>
    private void Window_Closing(object? sender, WindowClosingEventArgs e)
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
}
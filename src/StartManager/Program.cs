namespace StartManager;

class Program
{
    static void Main(string[] args)
    {
        bool exit = false;
        while (!exit)
        {
            ShowMenu();
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Application.StartServer();
                    Console.WriteLine("3秒后启动客户端");
                    Thread.Sleep(3000);
                    Application.StartClient();
                    break;
                case "R":
                    Application.Reset();
                    break;
                case "E":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("无效的选择，请重新输入。");
                    break;
            }
        }
    }

    static void ShowMenu()
    {
        Console.WriteLine("请选择一个选项：");
        Console.WriteLine("1. 开始升级");
        Console.WriteLine("R. 初始化run目录");
        Console.WriteLine("E. 退出");
        Console.Write("输入你的选择: ");
    }
}
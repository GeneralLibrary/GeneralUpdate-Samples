namespace StartManager;

class Program
{
    static void Main(string[] args)
    {
        bool exit = false;
        while (!exit)
        {
            ShowMenu();
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Application.Reset();
                    break;
                case "2":
                    Application.StartFileServer();
                    break;
                case "3":
                    Application.StartServer();
                    break;
                case "4":
                    Application.StartClient();
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
        Console.WriteLine("1. 还原run目录");
        Console.WriteLine("2. 启动文件服务端");
        Console.WriteLine("3. 启动服务端");
        Console.WriteLine("4. 启动客户端");
        Console.WriteLine("E. 退出");
        Console.Write("输入你的选择: ");
    }
}
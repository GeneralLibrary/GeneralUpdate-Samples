namespace StartManager;

class Program
{
    static void Main(string[] args)
    {
        bool exit = false;
        ShowMenu();
        while (!exit)
        {
            string input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Option1();
                    break;
                case "2":
                    Option2();
                    break;
                case "3":
                    Option3();
                    break;
                case "4":
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
        Console.WriteLine("1. 启动服务端");
        Console.WriteLine("2. 启动客户端");
        Console.WriteLine("3. 还原");
        Console.WriteLine("4. 退出");
        Console.Write("输入你的选择: ");
    }

    static void Option1()
    {
        Console.WriteLine("你选择了选项1");
        // 在这里添加选项1的逻辑
    }

    static void Option2()
    {
        Console.WriteLine("你选择了选项2");
        // 在这里添加选项2的逻辑
    }

    static void Option3()
    {
        Console.WriteLine("你选择了选项3");
        // 在这里添加选项3的逻辑
    }
}
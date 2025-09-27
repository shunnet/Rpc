using Snet.Log;
using Snet.Rpc.client;
using Snet.Utility;
using System.Text;



RpcClient rpcClient = RpcClient.Instance(new Snet.Rpc.data.Client.Basics
{
    IpAddress = "127.0.0.1",
    Port = 6688,
    TimeOut = 1000,
    Password = "ysai",
    UserName = "ysai",
    ISn = "RPC1",
    INs = new List<Snet.Rpc.data.Details> { new Snet.Rpc.data.Details() { INames = "IHello" } },
});

LogHelper.Info(rpcClient.Open().ToJson(true));
//注册
rpcClient.Register<IHello, hello>();


while (true)
{
    Console.ReadLine();
    //创建
    IHello hello = rpcClient.Create<IHello>();
    hello.Kitty(new Test { aaaa = "客户端 => 服务端", bbbb = DateTime.Now.ToDateTimeString() });
    LogHelper.Info(hello.Get());
}




public class Test
{
    public string aaaa { get; set; }
    public string bbbb { get; set; }
}
public interface IHello
{
    void Kitty(Test test);
    string Get();
}

public class hello : IHello
{
    public string Get()
    {
        // 假设每个字符占用2字节（对于UTF-16编码的字符串），  
        // 则需要大约 10 * 1024 * 1024 / 2 = 5,242,880 个字符来构成10MB的字符串。  
        const int targetSizeBytes = 20 * 1024 * 1024; // 10MB  
        const int charSizeBytes = 2; // UTF-16字符通常占用2字节  
        int targetSizeChars = targetSizeBytes / charSizeBytes;

        // 创建一个足够大的字符串构建器  
        StringBuilder sb = new StringBuilder(targetSizeChars);

        // 使用一个较小的字符串重复填充构建器，直到达到目标大小  
        const string repeatString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // 62个字符  
        for (int i = 0; i < targetSizeChars; i++)
        {
            sb.Append(repeatString);
        }

        // 截取到确切的目标大小（如果需要的话）  
        string result = sb.ToString(0, targetSizeChars);



        return result + $"\r\n{result.ToBytes().Length}\r\n我是客户端的GET方法：" + DateTime.Now.ToDateTimeString();
    }

    public void Kitty(Test test)
    {
        LogHelper.Info(test.ToJson(true));
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoRPC
{
    public class Rpc
    {
        public static void Main()
        {
            var rpcClient = new RpcClient();
            Console.WriteLine(" [x] Requesting fib number:");
            var number = Console.ReadLine();
            var response = rpcClient.Call(number);
            Console.WriteLine($" [.] Got {response}");

            rpcClient.Close();
        }
    }

}

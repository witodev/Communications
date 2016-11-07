using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AsyncSystem;
using System.Threading;

namespace AsyncSystemTest
{
    [TestClass]
    public class ServerTest
    {
        [TestMethod]
        public void TestServerBehavior()
        {
            var server = new Server();
            server.Port = 9876;
            server.Response += Server_Response;
            server.Start();
            Console.WriteLine("Server started");
            Thread.Sleep(5000);
            server.Stop();

            Assert.AreEqual(false, server.Working);
        }

        private void Server_Response(object sender, User e)
        {
            e.sb.Clear();
            e.sb.Append("Siema<EOF>");
        }
    }
}

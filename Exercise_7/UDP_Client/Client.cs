using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDP
{
	class udp_client
	{
		private const int PORT = 9000;
		private const int Buffer = 1000;

		private udp_client(string[] args)
		{

			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			var broadcast = IPAddress.Parse(args[0]);

			byte[] sendbuf = Encoding.ASCII.GetBytes(args[1]);
			var ep = new IPEndPoint(broadcast, PORT);

			s.SendTo(sendbuf, ep);

			Console.WriteLine("Sent " + args[1]);

			receiveData(s); 


		}

		private void receiveData(Socket s)
		{
			Byte[] receiveBytes = new byte[Buffer];
			s.Receive(receiveBytes);
			string returnData = Encoding.ASCII.GetString(receiveBytes).ToLower();
			Console.WriteLine("Received " + returnData);
		}


		public static void Main(string[] args)
		{
			new udp_client(args);
		}
	}
}

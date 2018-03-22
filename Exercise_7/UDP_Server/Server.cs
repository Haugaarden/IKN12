using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace UDP
{
	class udp_server
	{
		private const int PORT = 9000;
		private const int Buffer = 1000;

		private udp_server()
		{

			var listener = new UdpClient(PORT);
			var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, PORT);

			//Byte[] receiveBytes = listener.Receive(ref RemoteIpEndPoint);

			string returnData;
			do
			{
				Byte[] receiveBytes = listener.Receive(ref RemoteIpEndPoint);
				returnData = Encoding.ASCII.GetString(receiveBytes).ToLower();
				Console.WriteLine(returnData);

				Console.WriteLine(!returnData.Equals("u"));

			} while(!returnData.Equals("u") && !returnData.Equals("l"));


			string fileName;

			if(returnData.Equals("u"))
			{
				fileName = "/proc/uptime";
			} else
			{
				fileName = "/proc/loadavg";
			}

			Console.WriteLine(fileName);


			string stringToSend = readFile(fileName);
			sendString(stringToSend, listener, RemoteIpEndPoint);

		}

		private string readFile(string fileName)
		{
			FileStream fs = File.OpenRead(fileName);	//Opens filestream

			string fileText = System.IO.File.ReadAllText(fileName);

			int fileSize = fileText.Length;

			//var fileBuffer = new byte[Buffer];	//Buffer to contain parts of the file

			//int fileSize = fs.Read(fileBuffer, 0, fileBuffer.Length);

			Console.WriteLine(fileSize);

			return fileText;
		}

		private void sendString(string stringToSend, UdpClient client, IPEndPoint remoteEP)
		{
			var bytesToSend = Encoding.ASCII.GetBytes(stringToSend);
			client.Send(bytesToSend, bytesToSend.Length, remoteEP);
			Console.WriteLine("Sent string");
		}

		public static void Main(string[] args)
		{
			new udp_server();
		}
	}
}

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

			while(true)
			{
				var listener = new UdpClient(PORT);
				var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, PORT);

				Byte[] receiveBytes = listener.Receive(ref RemoteIpEndPoint);

				string returnData;
				do
				{
					returnData = Encoding.ASCII.GetString(receiveBytes).ToLower;
					Console.WriteLine(returnData);

				} while(!returnData.Equals("u") || !returnData.Equals("l"));

				string fileName;

				if(returnData.Equals("u"))
				{
					fileName = "/proc/uptime";
				} else
				{
					fileName = "/proc/loadavg";
				}

				Stream fileStream = File.OpenRead(fileName);	//Opens filestream

				var fileBuffer = new byte[Buffer];	//Buffer to contain parts of the file

				fileStream.Read(fileBuffer, 0, fileBuffer.Length);


			}

		}

		private void sendData(String fileName, long fileSize, NetworkStream io)
		{
			
		}

		public static void Main(string[] args)
		{
			new udp_server();
		}
	}
}

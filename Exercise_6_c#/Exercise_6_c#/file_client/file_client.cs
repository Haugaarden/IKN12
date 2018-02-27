using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcp
{
	class file_client
	{
		/// <summary>
		/// The PORT.
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		const int BUFSIZE = 1000;

		System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient (); 
		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments. First ip-adress of the server. Second the filename
		/// </param>
		private file_client (string[] args)
		{
			string ip = args[0];
			string filename = args[1];

			System.Net.Sockets.TcpClient clientSocket = new System.Net.Sockets.TcpClient (); 
			Console.WriteLine("Client Started");
			clientSocket.Connect(ip, PORT);// TO DO Your own code
			Console.WriteLine("Client connected");
			var networkStream = clientSocket.GetStream();
			receiveFile (filename, networkStream);
			clientSocket.Close(); 
		}

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='io'>
		/// Network stream for reading from the server
		/// </param>
		private void receiveFile (String fileName, NetworkStream io)
		{
			string fileSize;
			string dataDir = "/root/ExFiles/";
			do 
			{
				if(fileSize == "0")
				{
					Console.WriteLine("file does not exist, write filename:");
					fileName = Console.ReadLine(); 
				}
				LIB.writeTextTCP(io, fileName);
				fileSize = LIB.readTextTCP(io);
				
			} while(fileSize == "0");  

			FileStream filestream = File.Create(dataDir, (int)fileSize); 

			var fileBuffer = new byte[BUFSIZE]; 

			filestream.CopyTo(io);

		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			Console.WriteLine ("Client starts...");
			new file_client(args);
		}
	}
}

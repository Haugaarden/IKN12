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
			//If file does not exist on the server, the user can input another filename. Won't continue untill a correct name is input
			long fileSize = 0;
			do 
			{
				LIB.writeTextTCP(io, fileName);	//Sends filename to server
				fileSize = LIB.getFileSizeTCP(io);	//Reads filesize from server

				//If file does not exist, get new filename
				if(fileSize == 0)
				{
					Console.WriteLine("File does not exist, write filename:");
					fileName = Console.ReadLine(); 
				}

			} while(fileSize == 0);


			Console.WriteLine("Filesize: " + fileSize);

			string dataDir = "/root/ExFiles/";	//filepath to save file to
			FileStream fileStream = new FileStream (dataDir + fileName, FileMode.Create, FileAccess.Write);	//Creates file and return filestream
			var fileBuffer = new byte [BUFSIZE];	//Buffer for holding filedata packets

			Console.WriteLine("Reading...");

			int bytesRead = 0;
			int totalBytesRead = 0;
			while(totalBytesRead < fileSize)
			{
				bytesRead = io.Read(fileBuffer, 0, BUFSIZE);
				fileStream.Write(fileBuffer, 0, bytesRead);
				totalBytesRead += bytesRead;
			}
				
			Console.WriteLine("Done reading");
			fileStream.Close();

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

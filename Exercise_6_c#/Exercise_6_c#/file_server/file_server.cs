using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace tcp
{
	class file_server
	{
		/// <summary>
		/// The PORT
		/// </summary>
		const int PORT = 9000;
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		const int BUFSIZE = 1000;


		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// Opretter en socket.
		/// Venter på en connect fra en klient.
		/// Modtager filnavn
		/// Finder filstørrelsen
		/// Kalder metoden sendFile
		/// Lukker socketen og programmet... IKKE
		/// </summary>
		private file_server()
		{
			String dataDir = "/root/ServerData/";

			var serverSocket = new TcpListener(IPAddress.Any, PORT);   //Listens on PORT
			var clientSocket = default(TcpClient);
			serverSocket.Start();
			Console.WriteLine("Server started");
			clientSocket = serverSocket.AcceptTcpClient();
			Console.WriteLine("Accept connection from client");

			var networkStream = clientSocket.GetStream();   //Open a stream

			String fileName;
			long fileSize;
			do
			{
				Console.WriteLine("Reading filename");
				fileName = LIB.readTextTCP(networkStream);
				Console.WriteLine("Filename: " + fileName);
				fileSize = LIB.check_File_Exists(dataDir + fileName);
				Console.WriteLine("Filepath: " + dataDir + fileName);
				Console.WriteLine("Filesize: " + fileSize);

				LIB.writeTextTCP(networkStream, fileSize.ToString("0"));
			} while (fileSize == 0);

			sendFile(fileName, fileSize, networkStream);

			//Cleaning
			clientSocket.Close();
			serverSocket.Stop();
			Console.WriteLine("Exit");
			Console.ReadKey();
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// The filename.
		/// </param>
		/// <param name='fileSize'>
		/// The filesize.
		/// </param>
		/// <param name='io'>
		/// Network stream for writing to the client.
		/// </param>
		private void sendFile(String fileName, long fileSize, NetworkStream io)
		{
			Stream fileStream = File.OpenRead(fileName);	//Opens filestream

			int count = 0;	//count for the offset in filestream.Read

			while(true)
			{
				var fileBuffer = new byte[BUFSIZE];	//Buffer to contain parts of the file
				int status = fileStream.Read(fileBuffer, BUFSIZE * count, BUFSIZE);	//reads BUFSIZE amount of bytes into fileBuffer. Starts at byte number BUFSIZE * count. Status is number f bytes read

				//If any bytes are read
				if(status > 0)
				{
					//If the number of bytes read are less than the expected amount. Means that there's no more bytes left to read
					if(status < BUFSIZE)
					{
						Array.Resize(ref fileBuffer, status);	//Resize fileBuffer to be smaller (the size of status)
					}

					Console.WriteLine("Sending " + fileBuffer.Length + " bytes");
						
					var fileString = System.Text.Encoding.ASCII.GetString(fileBuffer);	//Convert byteArray to string
					LIB.writeTextTCP(io, fileString);	//Send string
					count++;	//Increment count for larger offset
				} else
				{
					break;	//No bytes were read
				}
			}
			Console.WriteLine("Done sending");
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main(string[] args)
		{
			Console.WriteLine("Server starts...");
			new file_server();
		}
	}
}

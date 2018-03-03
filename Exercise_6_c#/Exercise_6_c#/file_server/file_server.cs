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

			sendFile(dataDir + fileName, fileSize, networkStream);

			//Cleaning
			clientSocket.Close();
			serverSocket.Stop();
			Console.WriteLine("Exit");
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

			var fileBuffer = new byte[BUFSIZE];	//Buffer to contain parts of the file

			int bytesToSend = 0;

			while((bytesToSend = fileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0) //I exist to keep sending bytes until I only got 0 bytes to send left 
			{
				io.Write(fileBuffer, 0, bytesToSend); //I must send that byte

				Console.WriteLine($"Sent {bytesToSend} bytes");

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
			while(true)
			{
				new file_server();
			}
		}
	}
}

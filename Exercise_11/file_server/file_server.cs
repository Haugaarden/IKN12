using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_server
	{
		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_SERVER";

		Transport transport;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_server"/> class.
		/// </summary>
		private file_server ()
		{
			transport = new Transport(BUFSIZE, APP);

			String dataDir = "/root/ServerData/";
			var receivedData = new byte[BUFSIZE];

			Console.WriteLine("Server started");

			String fileName;
			long fileSize;
			int receivedSize;
			do
			{
				Console.WriteLine("Reading filename");
				receivedSize = transport.receive(ref receivedData);
				Console.WriteLine("ReceivedSize: " + receivedSize);
				fileName = Encoding.ASCII.GetString(receivedData, 0, receivedSize);
				Console.WriteLine("Filename: " + fileName);
				fileSize = LIB.check_File_Exists(dataDir + fileName);
				Console.WriteLine("Filepath: " + dataDir + fileName);
				Console.WriteLine("Filesize: " + fileSize);

				transport.send(Encoding.ASCII.GetBytes(fileSize.ToString()), fileSize.ToString().Length);
			} while (fileSize == 0);

			sendFile(dataDir + fileName, fileSize, transport);

			//Cleaning
			Console.WriteLine("Exit");
		}

		/// <summary>
		/// Sends the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='fileSize'>
		/// File size.
		/// </param>
		/// <param name='tl'>
		/// Tl.
		/// </param>
		private void sendFile(String fileName, long fileSize, Transport transport)
		{
			Stream fileStream = File.OpenRead(fileName);	//Opens filestream

			var fileBuffer = new byte[BUFSIZE];	//Buffer to contain parts of the file

			int bytesToSend = 0;

			while((bytesToSend = fileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0) //keep reading from fileStream until there are no bytes left to read
			{
				transport.send(fileBuffer, bytesToSend); //I must send that byte

				Console.WriteLine("Sent " + bytesToSend + " bytes");

			}

			Console.WriteLine("Done sending");
		}

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>
		/// The command-line arguments.
		/// </param>
		public static void Main (string[] args)
		{
			Console.WriteLine("Server starts...");
			while(true)
			{
				new file_server();
			}
		}
	}
}
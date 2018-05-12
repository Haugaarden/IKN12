using System;
using System.IO;
using System.Text;
using Transportlaget;
using Library;

namespace Application
{
	class file_client
	{
		/// <summary>
		/// The BUFSIZE.
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_CLIENT";

		Transport transport;

		/// <summary>
		/// Initializes a new instance of the <see cref="file_client"/> class.
		/// 
		/// file_client metoden opretter en peer-to-peer forbindelse
		/// Sender en forspÃ¸rgsel for en bestemt fil om denne findes pÃ¥ serveren
		/// Modtager filen hvis denne findes eller en besked om at den ikke findes (jvf. protokol beskrivelse)
		/// Lukker alle streams og den modtagede fil
		/// Udskriver en fejl-meddelelse hvis ikke antal argumenter er rigtige
		/// </summary>
		/// <param name='args'>
		/// Filnavn med evtuelle sti.
		/// </param>
	    private file_client(String[] args)
	    {
			transport = new Transport(BUFSIZE, APP);

			string filename = args[0];

			Console.WriteLine("Client Started");
			receiveFile (filename, transport);
	    }

		/// <summary>
		/// Receives the file.
		/// </summary>
		/// <param name='fileName'>
		/// File name.
		/// </param>
		/// <param name='transport'>
		/// Transportlaget
		/// </param>
		private void receiveFile (String fileName, Transport transport)
		{
			var receivedData = new byte[BUFSIZE];

			//If file does not exist on the server, the user can input another filename. Won't continue untill a correct name is input
			long fileSize = 0;
			do 
			{
				transport.send(Encoding.ASCII.GetBytes(fileName), fileName.Length);	//Sends filename to server

				Console.WriteLine("Receiving filesize");
				transport.receive(ref receivedData);	//Reads filesize from server
				fileSize = Convert.ToInt64(Encoding.ASCII.GetString(receivedData));
				Console.WriteLine("Filesize: " + fileSize);

				//If file does not exist, get new filename
				if(fileSize == 0)
				{
					Console.WriteLine("File does not exist, write filename:");
					fileName = Console.ReadLine(); 
				}

			} while(fileSize == 0);


			string dataDir = "/root/ExFiles/SerialTransmission/";	//filepath to save file to
			Directory.CreateDirectory(dataDir);	//Create directory if it does not exist
			FileStream fileStream = new FileStream (dataDir + fileName, FileMode.Create, FileAccess.Write);	//Creates file and return filestream
			var fileBuffer = new byte [BUFSIZE];	//Buffer for holding filedata packets

			Console.WriteLine("Reading...");

			int bytesRead = 0;
			int totalBytesRead = 0;
			while(totalBytesRead < fileSize)
			{
				bytesRead = transport.receive(ref fileBuffer);
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
		/// First argument: Filname
		/// </param>
		public static void Main (string[] args)
		{
			//Checks the number of arguments. Will terminate if there's less than 2 arguments
			if(args.Length >= 1)
			{
				Console.WriteLine("Client starts...");
				new file_client(args);
			} else
			{
				Console.WriteLine("This program needs 1 argument. filename");
				Console.WriteLine("Terminating");
			}
		}
	}
}
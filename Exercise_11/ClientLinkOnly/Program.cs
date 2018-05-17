using System;
using System.IO;
using System.Text;
using Library;

using Linklaget;


namespace ClientLinkOnly
{
	class MainClass
	{

		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_CLIENT";
		
		public static void Main(string[] args)
		{
			Link link;
			link = new Link(BUFSIZE, APP);

			Console.WriteLine("Hello World!");

			var receivedData = new byte[BUFSIZE];

			link.receive(ref receivedData);

			foreach(var Byte in receivedData)
			{
				Console.WriteLine(Byte);
			}
		}
	}
}

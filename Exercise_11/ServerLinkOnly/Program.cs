using System;
using System.IO;
using System.Text;
using Library;

using Linklaget;

namespace ServerLinkOnly
{
	class MainClass
	{
		/// <summary>
		/// The link.
		/// </summary>

		/// <summary>
		/// The BUFSIZE
		/// </summary>
		private const int BUFSIZE = 1000;
		private const string APP = "FILE_SERVER";

		public static void Main(string[] args)
		{
			Link link;
			link = new Link(BUFSIZE, APP);

			Console.WriteLine("Hello World!");

			string output = "AXBY Sut mig";

			link.send(Encoding.ASCII.GetBytes(output), output.Length);

			Console.WriteLine("Sent");
		}
	}
}

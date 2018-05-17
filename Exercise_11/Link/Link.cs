using System;
using System.Text;
using System.IO.Ports;
using System.Collections.Generic;

/// <summary>
/// Link.
/// </summary>
namespace Linklaget
{
	/// <summary>
	/// Link.
	/// </summary>
	public class Link
	{
		/// <summary>
		/// The DELIMITER for slip protocol.
		/// </summary>
		const byte DELIMITER = (byte)'A';
		/// <summary>
		/// The buffer for link.
		/// </summary>
		private byte[] buffer;
		/// <summary>
		/// The serial port.
		/// </summary>
		SerialPort serialPort;

		/// <summary>
		/// Initializes a new instance of the <see cref="link"/> class.
		/// </summary>
		public Link(int BUFSIZE, string APP)
		{
			// Create a new SerialPort object with default settings.
			#if DEBUG
			if(APP.Equals("FILE_SERVER"))
			{
				serialPort = new SerialPort("/dev/tnt0", 115200, Parity.None, 8, StopBits.One);
			} else
			{
				serialPort = new SerialPort("/dev/tnt1", 115200, Parity.None, 8, StopBits.One);
			}
			#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
			#endif
			if(!serialPort.IsOpen)
				serialPort.Open();

			buffer = new byte[(BUFSIZE * 2)];

			// Uncomment the next line to use timeout
			//serialPort.ReadTimeout = 500;

			serialPort.DiscardInBuffer();
			serialPort.DiscardOutBuffer();
		}

		/// <summary>
		/// Send the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send(byte[] buf, int size)
		{
//			serialPort.DiscardInBuffer();
//			serialPort.DiscardOutBuffer();

			var SLIP = new List<byte>();
			Console.WriteLine("Slip size = " + size);
			// implement SLIP
			//var SLIP = new StringBuilder(); 

			SLIP.Add(DELIMITER);

			for(int i = 0; i < size; i++)
			{
				if(size < 50)
				{
					//Console.WriteLine(buf[i]);
				}
				if(buf[i] == 'A')
				{
					//SLIP.Append(Encoding.ASCII.GetBytes("BC"));
					SLIP.Add((byte)'B');
					SLIP.Add((byte)'C');
				} else if(buf[i] == 'B')
				{
					SLIP.Add((byte)'B');
					SLIP.Add((byte)'D');
				} else
				{
					SLIP.Add(buf[i]);
				}
			}
			SLIP.Add(DELIMITER);

			//Send over serial
			try 
			{
				serialPort.Write(SLIP.ToArray(), 0, SLIP.Count);
			}
			catch(Exception)
			{
				Console.WriteLine("Write timeout exception");
			}
		}

		/// <summary>
		/// Receive the specified buf and size.
		/// </summary>
		/// <param name='buf'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public int receive(ref byte[] buf)
		{
			int size = serialPort.Read(buffer, 0, buffer.Length);

			int bufIndex = 0;
			// Make sure first index is DELIMITER
			if (buffer [0] == DELIMITER) 
			{

				// Loop through from next index
				for (int i = 1; i < size; i++) 
				{

					if (buffer [i] == 'B') 
					{
						// Must check on next index to insert A or B
						switch (buffer [i + 1]) 
						{
						case (byte)'C':
							buf [bufIndex++] = (byte)'A';
							i++;
							break;
						case (byte)'D':
							buf [bufIndex++] = (byte)'B';	
							i++;
							break;
						}
					} 
					else if (buffer [i] == DELIMITER)
						break;
					else 
						buf [bufIndex++] = buffer [i];
				}
			} 
			else 
			{
				return -1;
			}

			return bufIndex;
		}
	}
}

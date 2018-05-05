using System;
using System.Text;
using System.IO.Ports;

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
		public Link (int BUFSIZE, string APP)
		{
//			string[] ports = SerialPort.GetPortNames();
//			foreach(string port in ports)
//			{
//				Console.WriteLine(port);
//			}

			// Create a new SerialPort object with default settings.
			#if DEBUG
				if(APP.Equals("FILE_SERVER"))
				{
					serialPort = new SerialPort("/dev/tnt0",115200,Parity.None,8,StopBits.One);
				}
				else
				{
				serialPort = new SerialPort("/dev/tnt1",115200,Parity.None,8,StopBits.One);
				}
			#else
				serialPort = new SerialPort("/dev/ttyS1",115200,Parity.None,8,StopBits.One);
			#endif
			if(!serialPort.IsOpen)
 				serialPort.Open();

			buffer = new byte[(BUFSIZE*2)];

			// Uncomment the next line to use timeout
			serialPort.ReadTimeout = 500;

			serialPort.DiscardInBuffer ();
			serialPort.DiscardOutBuffer ();
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
		public void send (byte[] buf, int size)
		{
	    	// implement SLIP
			var SLIP = new StringBuilder(); 

			SLIP.Append((char)DELIMITER);

			for(int i = 0; i<size; i++)
			{
				if(buf[i] == 'A')
				{
					SLIP.Append("BC");
				} else if(buf[i] == 'B')
				{
					SLIP.Append("BD");
				} else
				{
					SLIP.Append((char)buf[i]);
				}
			}
			SLIP.Append((char)DELIMITER);

			//Send over serial
			serialPort.Write(SLIP.ToString());
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
		public int receive (ref byte[] buf)
		{
			//Read from serial
			serialPort.Read(buffer, 0, buffer.Length);

			//Convert from SLIP to regular string
			var DeSLIP = new StringBuilder(); 
			
			for(int i = 0; i<buffer.Length; i++)
			{
				if(buffer[i] == DELIMITER)
				{
					//Do nothing. Delimeters are ignored

				} else if(buffer[i] == 'B')
				{
					if(buffer[i + 1] == 'C')
					{
						DeSLIP.Append('A');
						i++;
					} else if(buffer[i + 1] == 'D')
					{
						DeSLIP.Append('B');
						i++;
					}
				} else
				{
					DeSLIP.Append((char)buffer[i]);
				}
			}

			buf = Encoding.ASCII.GetBytes(DeSLIP.ToString());

			return buf.Length;
		}
	}
}

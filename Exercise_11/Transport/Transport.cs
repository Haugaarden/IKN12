using System;
using Linklaget;

/// <summary>
/// Transport.
/// </summary>
namespace Transportlaget
{
	/// <summary>
	/// Transport.
	/// </summary>
	public class Transport
	{
		/// <summary>
		/// The link.
		/// </summary>
		private Link link;
		/// <summary>
		/// The 1' complements checksum.
		/// </summary>
		private Checksum checksum;
		/// <summary>
		/// The buffer.
		/// </summary>
		private byte[] buffer;
		/// <summary>
		/// The seq no.
		/// </summary>
		private byte seqNo;
		/// <summary>
		/// The ack_seq no.
		/// </summary>
		private byte ack_seqNo;
		/// <summary>
		/// The old_seq no.
		/// </summary>
		private byte old_seqNo;
		/// <summary>
		/// The error count.
		/// </summary>
		private int errorCount;
		/// <summary>
		/// The maximum error count.
		/// </summary>
		private int maxErrors = 10;
		/// <summary>
		/// The transmit count.
		/// </summary>
		private int transmitCount;
		/// <summary>
		/// The DEFAULT_SEQNO.
		/// </summary>
		private const int DEFAULT_SEQNO = 2;
		/// <summary>
		/// The data received. True = received data in receiveAck, False = not received data in receiveAck
		/// </summary>
		private bool dataReceived;
		/// <summary>
		/// The number of data the recveived.
		/// </summary>
		private int recvSize = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="Transport"/> class.
		/// </summary>
		public Transport(int BUFSIZE, string APP)
		{
			link = new Link(BUFSIZE + (int)TransSize.ACKSIZE, APP);
			checksum = new Checksum();
			buffer = new byte[BUFSIZE + (int)TransSize.ACKSIZE];
			seqNo = 0;
			ack_seqNo = DEFAULT_SEQNO;
			old_seqNo = DEFAULT_SEQNO;
			errorCount = 0;
			transmitCount = 0;
			dataReceived = false;
		}

		/// <summary>
		/// Receives the ack.
		/// </summary>
		/// <returns>
		/// The ack.
		/// </returns>
		private byte receiveAck()
		{
			Console.WriteLine("receiveAck()");
			recvSize = link.receive(ref buffer);
			dataReceived = true;

			if(recvSize == (int)TransSize.ACKSIZE)
			{
				Console.WriteLine("Acksize correct");
				dataReceived = false;
				if(!checksum.checkChecksum(buffer, (int)TransSize.ACKSIZE) ||
				   buffer[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				{
					Console.WriteLine("Error in received ack");
					ack_seqNo = (byte)((ack_seqNo + 1) % 2);
				} else
				{
					ack_seqNo = (byte)buffer[(int)TransCHKSUM.SEQNO]; // no increment if no error
				}
			}

			Console.WriteLine("ack_seqNo" + ack_seqNo.ToString());
 
			return ack_seqNo;
		}

		/// <summary>
		/// Sends the ack.
		/// </summary>
		/// <param name='ackType'>
		/// Ack type.
		/// </param>
		private void sendAck(bool ackType)
		{
			Console.WriteLine("Sending Ack: " + ackType);
			byte[] ackBuf = new byte[(int)TransSize.ACKSIZE];
			ackBuf[(int)TransCHKSUM.SEQNO] = 
				(byte)(ackType ? (byte)buffer[(int)TransCHKSUM.SEQNO] : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
			ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);

			if(++transmitCount == 50) // Simulate noise
			{
				ackBuf[1]++; // Important: Only spoil a checksum-field (ackBuf[0] or ackBuf[1])
				Console.WriteLine("Noise! byte #1 is spoiled in the second transmitted ACK-package");
				transmitCount = 0;
			}

			link.send(ackBuf, (int)TransSize.ACKSIZE);
		}

		/// <summary>
		/// Send the specified buffer and size.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		/// <param name='size'>
		/// Size.
		/// </param>
		public void send(byte[] buf, int size)
		{
			do
			{
				// Reset buffer
				for (int i = 0; i < buffer.Length; i++) 
				{
					buffer [i] = 0;
				}

				//Seq
				buffer [(int)TransCHKSUM.SEQNO] = seqNo;
				//Type
				buffer [(int)TransCHKSUM.TYPE] = (int)TransType.DATA;

				Array.Copy(buf, 0, buffer, 4, size);

				//Tilføjer de to første "bytes" på buf
				checksum.calcChecksum (ref buffer, buffer.Length);

				Console.WriteLine($"TRANSMIT #{++transmitCount}");

				if(transmitCount == 3) // Simulate noise
				{
					buffer[1]++; // Important: Only spoil a checksum-field (buffer[0] or buffer[1])
					Console.WriteLine($"Noise! - pack #{transmitCount} is spoiled");
				}

				if (transmitCount == 5)
					transmitCount = 0;

				ack_seqNo = seqNo;
				try
				{
					Console.WriteLine($"Sending pack with seqNo #{seqNo}");

					link.send(buffer, size+4);

					receiveAck ();


					Console.WriteLine($"Receiving ack with seqNo #{ack_seqNo}");

					if (ack_seqNo != seqNo) 
					{
						Console.WriteLine ("\tError: Did not receive correctly");
						Console.WriteLine("\t\tResending same package");
					} 
					else
					{
						Console.WriteLine ("\tReceived correctly\n");
						break;
					}
				}
				catch(TimeoutException) 
				{
					Console.WriteLine ("\tTimed out, resending");
				}

				errorCount++;
				Console.WriteLine ("\tErrorcount: " + errorCount + "\n");

			}while ((errorCount < maxErrors));

			if (errorCount >= maxErrors) 
			{
				Console.WriteLine ("With errorcount " + errorCount + ", I am out.");
				Environment.Exit (1);
			}

			seqNo = (byte)((seqNo + 1) % 2);

			errorCount = 0;
			ack_seqNo = DEFAULT_SEQNO;
		}

		/// <summary>
		/// Receive the specified buffer.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		public int receive(ref byte[] buf)
		{
			// Reset buffer
			for(int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = 0;
			}

			while(true)
			{
				recvSize = 0;
				// Will time out while waiting, so must catch 
				while(recvSize == 0 || recvSize == -1)
				{
					try
					{
						recvSize = link.receive(ref buffer);	//returns length of received byte array
						if(recvSize == -1)
						{
							sendAck(false); 
							System.Threading.Thread.Sleep(250);

						}
					} catch(Exception)
					{
					}
				}

				Console.WriteLine($"TRANSMIT #{++transmitCount}");

				if(checksum.checkChecksum(buffer, recvSize))
				{
					Console.WriteLine("Data pack OK.");

					seqNo = buffer[(int)TransCHKSUM.SEQNO];

					Array.Copy(buffer, (int)TransSize.ACKSIZE, buf, 0, recvSize - (int)TransSize.ACKSIZE);
					if(seqNo == old_seqNo)
						Console.WriteLine("\tReceived identical package. Ignore");

					old_seqNo = seqNo;
					sendAck(true);
					return recvSize - 4;
				}

				Console.WriteLine("Error in data pack. Sending NACK.");
				sendAck(false);
			}
		}
	}
}
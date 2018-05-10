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
		/// The old_seq no.
		/// </summary>
		private byte ack_seqNo;
		/// <summary>
		/// The error count.
		/// </summary>
		private int errorCount;
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
			//byte seqNoL = new byte();
			Console.WriteLine("receiveAck()");
			recvSize = link.receive(ref buffer);
			dataReceived = true;

			if(recvSize == (int)TransSize.ACKSIZE)
			{
				Console.WriteLine("Acksize correct");
				dataReceived = false;
				if(!checksum.checkChecksum(buffer, (int)TransSize.ACKSIZE) ||
				   buffer[(int)TransCHKSUM.SEQNO] != seqNo ||
				   buffer[(int)TransCHKSUM.TYPE] != (int)TransType.ACK)
				{
					ack_seqNo = (byte)buffer[(int)TransCHKSUM.SEQNO];	//No increment if error
					//ack_seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2); // increment if error
				} else
				{
					//ack_seqNo = (byte)((buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);	//increments seqNo if no checkSum error
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
			ackBuf[(int)TransCHKSUM.SEQNO] = (byte)
				(ackType ? (byte)buffer[(int)TransCHKSUM.SEQNO] : (byte)(buffer[(int)TransCHKSUM.SEQNO] + 1) % 2);
			ackBuf[(int)TransCHKSUM.TYPE] = (byte)(int)TransType.ACK;
			checksum.calcChecksum(ref ackBuf, (int)TransSize.ACKSIZE);

			if(++transmitCount == 2) // Simulate noise
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
				//reset buffer
				for(int i = 0; i < buffer.Length; i++)
				{
					buffer[i] = 0;
				}

				//If 5 errors in a row. Receiver application does not want to receive data. Abort
				if(errorCount++ >= 5)	//increment errorCount after check
				{
					return;
					//throw 
				}

				buffer[2] = seqNo;	//The current sequence number
				buffer[3] = (byte)(int)TransType.DATA;	//Data is being sent
				Array.Copy(buf, 0, buffer, (int)TransSize.ACKSIZE, size);	//Copy from buf starting at [0] to buffer starting at [4]
				checksum.calcChecksum(ref buffer, buffer.Length);

				if(++transmitCount == 3) // Simulate noise
				{
					buffer[1]++; // Important: Only spoil a checksum-field (buffer[0] or buffer[1])
					Console.WriteLine("Noise! - byte #1 is spoiled in the third transmission");
					transmitCount = 0;
				}				

				link.send(buffer, size + (int)TransSize.ACKSIZE);	//Send the data

				ack_seqNo = seqNo;
				try
				{
					receiveAck();	//Get acknowledged seqNo
				} catch(TimeoutException)
				{
					//maybe do nothing
					Console.WriteLine("Read TimeoutException. ErrorCount: " + errorCount);
				}

				Console.WriteLine("ack_seqNo: " + ack_seqNo + " seqNo: " + seqNo);

				if(seqNo != ack_seqNo)
				{
					Console.WriteLine("Ack error");
				}

			} while(seqNo != ack_seqNo);	//keep trying as long as dataSeqNo does not match ackSeqNo

			seqNo = (byte)((seqNo + 1) % 2);	//Change seqNo
			ack_seqNo = DEFAULT_SEQNO;	//Make sure seqNo does not match a useable seqNo, in case the direction is changed
			errorCount = 0;	//Reset errorCount

			Console.WriteLine("Done Sending");
		}

		/// <summary>
		/// Receive the specified buffer.
		/// </summary>
		/// <param name='buffer'>
		/// Buffer.
		/// </param>
		public int receive(ref byte[] buf)
		{
			while(true)
			{
				//reset buffer
				for(int i = 0; i < buffer.Length; i++)
				{
					buffer[i] = 0;
				}

				// receive data
				while(recvSize == 0)
				{
					try
					{
						recvSize = link.receive(ref buffer);	//returns length of received byte array
					} catch(Exception)
					{
						//Maybe do nothing
					}
				}

				//check for bit errors
				if(checksum.checkChecksum(buffer, recvSize))
				{
					Console.WriteLine("No bit errors");
					sendAck(true);	//Data had no errors
					Array.Copy(buffer, (int)TransSize.ACKSIZE, buf, 0, recvSize - (int)TransSize.ACKSIZE);	//Copy from buffer starting at [4] to buf starting at [0]
					Console.WriteLine(buffer.Length + " " + buf.Length);
					recvSize = 0;
					return buf.Length;
				}

				recvSize = 0;
				sendAck(false);	//Data had errors. Resend pls

				//return 0;
			}
		}
	}
}
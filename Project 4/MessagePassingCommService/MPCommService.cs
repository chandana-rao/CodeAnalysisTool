/////////////////////////////////////////////////////////////////////
// MPCommService.cs - service for MessagePassingComm               //
// Language:    C#, 2017, .Net Framework 4.6.1                    //
// Application: Project #4, CSE681 Fall 2018                     //
// Author:      Chandana Rao                                    //
//  Source:     Jim Fawcett                                    //        
////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines three classes:
 * - Sender which implements the public methods:
 *   -------------------------------------------
 *   - Sender           : constructs sender using address string and port
 *   - connect          : opens channel and attempts to connect to an endpoint, 
 *                        trying multiple times to send a connect message
 *   - close            : closes channel
 *   - postMessage      : posts to an internal thread-safe blocking queue, which
 *                        a sendThread then dequeues msg, inspects for destination,
 *                        and calls connect(address, port)
 *   - postFile         : attempts to upload a file in blocks
 *   - close            : closes current connection
 *   - getLastError     : returns exception messages on method failure
 *
 * - Receiver which implements the public methods:
 *   ---------------------------------------------
 *   - Receiver         : constructs Receiver instance
 *   - start            : creates instance of ServiceHost which services incoming messages
 *                        using address string and port of listener
 *   - postMessage      : Sender proxies call this message to enqueue for processing
 *   - getMessage       : called by Receiver application to retrieve incoming messages
 *   - close            : closes ServiceHost
 *   - openFileForWrite : opens a file for storing incoming file blocks
 *   - writeFileBlock   : writes an incoming file block to storage
 *   - closeFile        : closes newly uploaded file
 *   - size             : returns number of messages waiting in receive queue
 *   
 * - Comm which implements, using Sender and Receiver instances, the public methods:
 *   -------------------------------------------------------------------------------
 *   - Comm             : create Comm instance with address and port
 *   - postMessage      : send CommMessage instance to a Receiver instance
 *   - getMessage       : retrieves a CommMessage from a Sender instance
 *   - postFile         : called by a Sender instance to transfer a file
 *   - close()          : stops sender and receiver threads
 *   - restart          : attempts to restart with port - that must be different from
 *                        any port previously used while the embedding process states alive
 *   - closeConnection  : closes current connection, can reopen that or another connection
 *   - size             : returns number of messages in receive queue
 *   
 * Public Interface Documentation:
 * public class Receiver: receives CommMessages and Files from Senders
 * public bool start(string baseAddress, int port) : create ServiceHost listening on specified endpoint
 * public void createCommHost(string address) : create ServiceHost listening on specified endpoint
 * public void postMessage(CommMessage msg) : enqueue a message for transmission to a Receiver
 * public CommMessage getMessage() : retrieve a message sent by a Sender instance
 * public void close() : close ServiceHost 
 * public class Sender : sends messages and files to Receiver
 * public void createSendChannel(string address) : creates proxy with interface of remote instance
 * public bool connect(string baseAddress, int port) : attempts to connect to Receiver instance
 * public class Comm : Comm class combines Receiver and Sender
 * public bool restart(int newport) : restart comm
 *
 * Required Files:
 * ---------------
 * IMPCommService.cs, MPCommService.cs
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 14 Jun 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Threading;
using System.IO;
using Navigator;

namespace MessagePassingComm
{
    ///////////////////////////////////////////////////////////////////
    // Receiver class - receives CommMessages and Files from Senders

    public class Receiver : IMessagePassingComm
    {
        public static SWTools.BlockingQueue<CommMessage> rcvQ { get; set; } = null;
        public bool restartFailed { get; set; } = false;
        ServiceHost commHost = null;
        FileStream fs = null;
        string lastError = "";

        /*----< constructor >------------------------------------------*/

        public Receiver()
        {
            if (rcvQ == null)
                rcvQ = new SWTools.BlockingQueue<CommMessage>();
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        public bool start(string baseAddress, int port)
        {
            try
            {
                string address = baseAddress + ":" + port.ToString() + "/IMessagePassingComm";
                TestUtilities.putLine(string.Format("starting Receiver on thread {0}", Thread.CurrentThread.ManagedThreadId));
                createCommHost(address);
                restartFailed = false;
                return true;
            }
            catch (Exception ex)
            {
                restartFailed = true;
                Console.Write("\n{0}\n", ex.Message);
                Console.Write("\n  You can't restart a listener on a previously used port");
                Console.Write(" - Windows won't release it until the process shuts down");
                return false;
            }
        }
        /*----< create ServiceHost listening on specified endpoint >---*/
        public void createCommHost(string address)
        {
            WSHttpBinding binding = new WSHttpBinding();
            Uri baseAddress = new Uri(address);
            commHost = new ServiceHost(typeof(Receiver), baseAddress);
            commHost.AddServiceEndpoint(typeof(IMessagePassingComm), binding, baseAddress);
            commHost.Open();
        }
        /*----< enqueue a message for transmission to a Receiver >-----*/

        public void postMessage(CommMessage msg)
        {
            msg.threadId = Thread.CurrentThread.ManagedThreadId;
            TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}", Thread.CurrentThread.ManagedThreadId));
            rcvQ.enQ(msg);
        }
        /*----< retrieve a message sent by a Sender instance >---------*/

        public CommMessage getMessage()
        {
            CommMessage msg = rcvQ.deQ();
            if (msg.type == CommMessage.MessageType.closeReceiver)
            {
                close();
            }
            if (msg.type == CommMessage.MessageType.connect)
            {
                msg = rcvQ.deQ();  // discarding the connect message
            }
            return msg;
        }
        /*----< how many messages in receive queue? >-----------------*/

        public int size()
        {
            return rcvQ.size();
        }
        /*----< close ServiceHost >----------------------------------*/

        public void close()
        {
            Console.Write("\n  closing receiver - please wait");
            commHost.Close();
            (commHost as IDisposable).Dispose();

            Console.Write("\n  commHost.Close() returned");
        }
        /*---< called by Sender's proxy to open file on Receiver >-----*/

        public bool openFileForWrite(string name)
        {
            try
            {
                string writePath = Path.Combine(ServerEnvironment.root, name);
                fs = File.OpenWrite(writePath);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< write a block received from Sender instance >----------*/

        public bool writeFileBlock(byte[] block)
        {
            try
            {
                fs.Write(block, 0, block.Length);
                return true;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
        }
        /*----< close Receiver's uploaded file >-----------------------*/

        public void closeFile()
        {
            fs.Close();
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Sender class - sends messages and files to Receiver

    public class Sender
    {
        private IMessagePassingComm channel;
        private ChannelFactory<IMessagePassingComm> factory = null;
        private SWTools.BlockingQueue<CommMessage> sndQ = null;
        private int port = 0;
        private string fromAddress = "";
        private string toAddress = "";
        Thread sndThread = null;
        int tryCount = 0, maxCount = 10;
        string lastError = "";
        string lastUrl = "";

        /*----< constructor >------------------------------------------*/

        public Sender(string baseAddress, int listenPort)
        {
            port = listenPort;
            fromAddress = baseAddress + listenPort.ToString() + "/IMessagePassingComm";
            sndQ = new SWTools.BlockingQueue<CommMessage>();
            TestUtilities.putLine(string.Format("starting Sender on thread {0}", Thread.CurrentThread.ManagedThreadId));
            sndThread = new Thread(threadProc);
            sndThread.Start();
        }
        /*----< creates proxy with interface of remote instance >------*/

        public void createSendChannel(string address)
        {
            EndpointAddress baseAddress = new EndpointAddress(address);
            WSHttpBinding binding = new WSHttpBinding();
            factory = new ChannelFactory<IMessagePassingComm>(binding, address);
            channel = factory.CreateChannel();
        }
        /*----< attempts to connect to Receiver instance >-------------*/

        public bool connect(string baseAddress, int port)
        {
            toAddress = baseAddress + ":" + port.ToString() + "/IMessagePassingComm";
            return connect(toAddress);
        }
        /*----< attempts to connect to Receiver instance >-------------*/
        public bool connect(string toAddress)
        {
            int timeToSleep = 500;
            TestUtilities.putLine("attempting to connect to \"" + toAddress + "\"");
            createSendChannel(toAddress);
            CommMessage connectMsg = new CommMessage(CommMessage.MessageType.connect);
            while (true)
            {
                try
                {
                    channel.postMessage(connectMsg);
                    tryCount = 0;
                    return true;
                }
                catch (Exception ex)
                {
                    if (++tryCount < maxCount)
                    {
                        TestUtilities.putLine("failed to connect - waiting to try again");
                        Thread.Sleep(timeToSleep);
                    }
                    else
                    {
                        TestUtilities.putLine("failed to connect - quitting");
                        lastError = ex.Message;
                        return false;
                    }
                }
            }
        }
        /*----< closes Sender's proxy >--------------------------------*/

        public void close()
        {
            while (sndQ.size() > 0)
            {
                CommMessage msg = sndQ.deQ();
                try
                {
                    channel.postMessage(msg);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }

            try
            {
                if (factory != null)
                    factory.Close();
            }
            catch (Exception ex)
            {
                Console.Write("\n  already closed");
            }
        }
        /*----< processing for send thread >--------------------------*/
        /*
         * - send thread dequeues send message and posts to channel proxy
         * - thread inspects message and routes to appropriate specified endpoint
         */
        void threadProc()
        {
            while (true)
            {
                TestUtilities.putLine(string.Format("sender enqueuing message on thread {0}", Thread.CurrentThread.ManagedThreadId));

                CommMessage msg = sndQ.deQ();
                if (msg.type == CommMessage.MessageType.closeSender)
                {
                    TestUtilities.putLine("Sender send thread quitting");
                    break;
                }
                if (msg.to == lastUrl)
                {
                    channel.postMessage(msg);
                }
                else
                {
                    close();
                    if (!connect(msg.to))
                        continue;
                    lastUrl = msg.to;
                    channel.postMessage(msg);
                }
            }
        }
        /*----< main thread enqueues message for sending >-------------*/

        public void postMessage(CommMessage msg)
        {
            sndQ.enQ(msg);
        }
        /*----< uploads file to Receiver instance >--------------------*/

        public bool postFile(string fileName)
        {
            FileStream fs = null;
            long bytesRemaining;

            try
            {
                string path = Path.Combine(ClientEnvironment.root, fileName);
                fs = File.OpenRead(path);
                bytesRemaining = fs.Length;
                channel.openFileForWrite(fileName);
                while (true)
                {
                    long bytesToRead = Math.Min(ClientEnvironment.blockSize, bytesRemaining);
                    byte[] blk = new byte[bytesToRead];
                    long numBytesRead = fs.Read(blk, 0, (int)bytesToRead);
                    bytesRemaining -= numBytesRead;

                    channel.writeFileBlock(blk);
                    if (bytesRemaining <= 0)
                        break;
                }
                channel.closeFile();
                fs.Close();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                return false;
            }
            return true;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Comm class combines Receiver and Sender

    public class Comm
    {
        private Receiver rcvr = null;
        private Sender sndr = null;
        private string address = null;
        private int portNum = 0;

        /*----< constructor >------------------------------------------*/
        /*
         * - starts listener listening on specified endpoint
         */
        public Comm(string baseAddress, int port)
        {
            address = baseAddress;
            portNum = port;
            rcvr = new Receiver();
            rcvr.start(baseAddress, port);
            sndr = new Sender(baseAddress, port);
        }
        /*----< shutdown comm >----------------------------------------*/

        public void close()
        {
            Console.Write("\n  Comm closing");
            rcvr.close();
            sndr.close();
        }
        /*----< restart comm >-----------------------------------------*/

        public bool restart(int newport)
        {
            rcvr = new Receiver();
            rcvr.start(address, newport);
            if (rcvr.restartFailed)
            {
                return false;
            }
            sndr = new Sender(address, portNum);
            return true;
        }
        /*----< closes connection but keeps comm alive >---------------*/

        public void closeConnection()
        {
            sndr.close();
        }
        /*----< post message to remote Comm >--------------------------*/

        public void postMessage(CommMessage msg)
        {
            sndr.postMessage(msg);
        }
        /*----< retrieve message from remote Comm >--------------------*/

        public CommMessage getMessage()
        {
            return rcvr.getMessage();
        }
        /*----< called by remote Comm to upload file >-----------------*/

        public bool postFile(string filename)
        {
            return sndr.postFile(filename);
        }
        /*----< how many messages in receive queue? >-----------------*/

        public int size()
        {
            return rcvr.size();
        }
    }

    class Test
    {
        static void Main(string[] args)
        {

        }
    }
       
    
}

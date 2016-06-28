﻿//******************************************************************************************************
//  FileClient.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/24/2006 - Pinal C. Patel
//       Original version of source code generated.
//  09/06/2006 - J. Ritchie Carroll
//       Added bypass optimizations for high-speed file data access.
//  09/29/2008 - J. Ritchie Carroll
//       Converted to C#.
//  07/08/2009 - J. Ritchie Carroll
//       Added WaitHandle return value from asynchronous connection.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  06/23/2010 - Stephen C. Wills
//       Modified to use the absolute file path.
//  11/29/2010 - Pinal C. Patel
//       Corrected the implementation of ConnectAsync() method.
//  12/04/2010 - Pinal C. Patel
//       Removed locking around m_fileClient.Provider since it was unnecessary.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Timers;
using GSF.Configuration;
using GSF.IO;
using Timer = System.Timers.Timer;

namespace GSF.Communication
{
    /// <summary>
    /// Represents a communication client based on <see cref="FileStream"/>.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="FileClient"/> for writing data to a file:
    /// <code>
    /// using System;
    /// using GSF.Communication;
    /// 
    /// class Program
    /// {
    ///     static FileClient s_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         s_client = new FileClient(@"File=c:\File.txt");
    ///         s_client.Initialize();
    ///         // Register event handlers.
    ///         s_client.ConnectionAttempt += s_client_ConnectionAttempt;
    ///         s_client.ConnectionEstablished += s_client_ConnectionEstablished;
    ///         s_client.ConnectionTerminated += s_client_ConnectionTerminated;
    ///         s_client.SendDataComplete += s_client_SendDataComplete;
    ///         // Connect the client.
    ///         s_client.Connect();
    /// 
    ///         // Write user input to the file.
    ///         string input;
    ///         while (string.Compare(input = Console.ReadLine(), "Exit", true) != 0)
    ///         {
    ///             s_client.Send(input + "\r\n");
    ///         }
    /// 
    ///         // Disconnect the client on shutdown.
    ///         s_client.Dispose();
    ///     }
    /// 
    ///     static void s_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void s_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void s_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void s_client_SendDataComplete(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine(string.Format("Sent data - {0}", s_client.TextEncoding.GetString(s_client.Client.SendBuffer)));
    ///     }
    /// }
    /// </code>
    /// This example shows how to use <see cref="FileClient"/> for reading data to a file:
    /// <code>
    /// using System;
    /// using GSF;
    /// using GSF.Communication;
    /// 
    /// class Program
    /// {
    ///     static FileClient s_client;
    /// 
    ///     static void Main(string[] args)
    ///     {
    ///         // Initialize the client.
    ///         s_client = new FileClient(@"File=c:\File.txt");
    ///         s_client.Initialize();
    ///         // Register event handlers.
    ///         s_client.ConnectionAttempt += s_client_ConnectionAttempt;
    ///         s_client.ConnectionEstablished += s_client_ConnectionEstablished;
    ///         s_client.ConnectionTerminated += s_client_ConnectionTerminated;
    ///         s_client.ReceiveDataComplete += s_client_ReceiveDataComplete;
    ///         // Connect the client.
    ///         s_client.Connect();
    /// 
    ///         // Wait for client to read data.
    ///         Console.ReadLine();
    /// 
    ///         // Disconnect the client on shutdown.
    ///         s_client.Dispose();
    ///     }
    /// 
    ///     static void s_client_ConnectionAttempt(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client is connecting to file.");
    ///     }
    /// 
    ///     static void s_client_ConnectionEstablished(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client connected to file.");
    ///     }
    /// 
    ///     static void s_client_ConnectionTerminated(object sender, EventArgs e)
    ///     {
    ///         Console.WriteLine("Client disconnected from file.");
    ///     }
    /// 
    ///     static void s_client_ReceiveDataComplete(object sender, EventArgs&lt;byte[], int&gt; e)
    ///     {
    ///         Console.WriteLine(string.Format("Received data - {0}", s_client.TextEncoding.GetString(e.Argument1, 0, e.Argument2)));
    ///     }
    /// }
    /// </code>
    /// </example>
    public class FileClient : ClientBase
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the default value for the <see cref="AutoRepeat"/> property.
        /// </summary>
        public const bool DefaultAutoRepeat = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveOnDemand"/> property.
        /// </summary>
        public const bool DefaultReceiveOnDemand = false;

        /// <summary>
        /// Specifies the default value for the <see cref="ReceiveInterval"/> property.
        /// </summary>
        public const int DefaultReceiveInterval = -1;

        /// <summary>
        /// Specifies the default value for the <see cref="StartingOffset"/> property.
        /// </summary>
        public const long DefaultStartingOffset = 0;

        /// <summary>
        /// Specifies the default value for the <see cref="FileOpenMode"/> property.
        /// </summary>
        public const FileMode DefaultFileOpenMode = FileMode.OpenOrCreate;

        /// <summary>
        /// Specifies the default value for the <see cref="FileShareMode"/> property.
        /// </summary>
        public const FileShare DefaultFileShareMode = FileShare.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="FileAccessMode"/> property.
        /// </summary>
        public const FileAccess DefaultFileAccessMode = FileAccess.ReadWrite;

        /// <summary>
        /// Specifies the default value for the <see cref="ClientBase.ConnectionString"/> property.
        /// </summary>
        public const string DefaultConnectionString = "File=DataFile.txt";

        // Fields
        private bool m_autoRepeat;
        private bool m_receiveOnDemand;
        private double m_receiveInterval;
        private long m_startingOffset;
        private FileMode m_fileOpenMode;
        private FileShare m_fileShareMode;
        private FileAccess m_fileAccessMode;
        private readonly TransportProvider<FileStream> m_fileClient;
        private Dictionary<string, string> m_connectData;
        private readonly Timer m_receiveDataTimer;
        private ManualResetEvent m_connectionHandle;
#if ThreadTracking
        private ManagedThread m_connectionThread;
#else
        private Thread m_connectionThread;
#endif
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        public FileClient()
            : this(DefaultConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        /// <param name="connectString">Connect string of the <see cref="FileClient"/>. See <see cref="DefaultConnectionString"/> for format.</param>
        public FileClient(string connectString)
            : base(TransportProtocol.File, connectString)
        {
            m_autoRepeat = DefaultAutoRepeat;
            m_receiveOnDemand = DefaultReceiveOnDemand;
            m_receiveInterval = DefaultReceiveInterval;
            m_startingOffset = DefaultStartingOffset;
            m_fileOpenMode = DefaultFileOpenMode;
            m_fileShareMode = DefaultFileShareMode;
            m_fileAccessMode = DefaultFileAccessMode;
            m_fileClient = new TransportProvider<FileStream>();
            m_receiveDataTimer = new Timer();
            m_receiveDataTimer.Elapsed += m_receiveDataTimer_Elapsed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileClient"/> class.
        /// </summary>
        /// <param name="container"><see cref="IContainer"/> object that contains the <see cref="FileClient"/>.</param>
        public FileClient(IContainer container)
            : this()
        {
            if (container != null)
                container.Add(this);
        }
        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data is to be repeated endlessly.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="AutoRepeat"/> is enabled when <see cref="FileAccessMode"/> is <see cref="FileAccess.ReadWrite"/></exception>
        [Category("Data"),
        DefaultValue(DefaultAutoRepeat),
        Description("Indicates whether receiving (reading) of data is to be repeated endlessly.")]
        public bool AutoRepeat
        {
            get
            {
                return m_autoRepeat;
            }
            set
            {
                if (value && m_fileAccessMode == FileAccess.ReadWrite)
                    throw new InvalidOperationException("AutoRepeat cannot be enabled when FileAccessMode is FileAccess.ReadWrite");

                m_autoRepeat = value;
            }
        }

        /// <summary>
        /// Gets or sets a boolean value that indicates whether receiving (reading) of data will be initiated manually by calling <see cref="ReadNextBuffer"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="ReceiveInterval"/> will be set to -1 when <see cref="ReceiveOnDemand"/> is enabled.
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveOnDemand),
        Description("Indicates whether receiving (reading) of data will be initiated manually by calling ReceiveData().")]
        public bool ReceiveOnDemand
        {
            get
            {
                return m_receiveOnDemand;
            }
            set
            {
                m_receiveOnDemand = value;

                if (m_receiveOnDemand)
                    // We'll disable receiving data at a set interval if user wants to receive data on demand.
                    m_receiveInterval = -1;
            }
        }

        /// <summary>
        /// Gets or sets the number of milliseconds to pause before receiving (reading) the next available set of data.
        /// </summary>
        /// <remarks>
        /// Set <see cref="ReceiveInterval"/> = -1 to receive (read) data continuously without pausing.
        /// </remarks>
        [Category("Data"),
        DefaultValue(DefaultReceiveInterval),
        Description("The number of milliseconds to pause before receiving (reading) the next available set of data. Set ReceiveInterval = -1 to receive data continuously without pausing.")]
        public double ReceiveInterval
        {
            get
            {
                return m_receiveInterval;
            }
            set
            {
                if (value < 1)
                    m_receiveInterval = -1;
                else
                    m_receiveInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the starting point relative to the beginning of the file from where the data is to be received (read).
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not a positive number.</exception>
        [Category("File"),
        DefaultValue(DefaultStartingOffset),
        Description("The starting point relative to the beginning of the file from where the data is to be received (read).")]
        public long StartingOffset
        {
            get
            {
                return m_startingOffset;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive");

                m_startingOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileMode"/> value to be used when opening the file.
        /// </summary>
        [Category("File"),
        DefaultValue(DefaultFileOpenMode),
        Description("The System.IO.FileMode value to be used when opening the file.")]
        public FileMode FileOpenMode
        {
            get
            {
                return m_fileOpenMode;
            }
            set
            {
                m_fileOpenMode = value;
            }
        }

        /// <summary>
        /// Gets or set the <see cref="FileShare"/> value to be used when opening the file.
        /// </summary>
        [Category("File"),
        DefaultValue(DefaultFileShareMode),
        Description("The System.IO.FileShare value to be used when opening the file.")]
        public FileShare FileShareMode
        {
            get
            {
                return m_fileShareMode;
            }
            set
            {
                m_fileShareMode = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileAccess"/> value to be used when opening the file.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="FileAccessMode"/> is set to <see cref="FileAccess.ReadWrite"/> when <see cref="AutoRepeat"/> is enabled.</exception>
        [Category("File"),
        DefaultValue(DefaultFileAccessMode),
        Description("The System.IO.FileAccess value to be used when opening the file.")]
        public FileAccess FileAccessMode
        {
            get
            {
                return m_fileAccessMode;
            }
            set
            {
                if (value == FileAccess.ReadWrite && m_autoRepeat)
                    throw new InvalidOperationException("FileAccessMode cannot be set to FileAccess.ReadWrite when AutoRepeat is enabled");

                m_fileAccessMode = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileStream"/> object for the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public FileStream Client
        {
            get
            {
                return m_fileClient.Provider;
            }
        }

        /// <summary>
        /// Gets the server URI of the <see cref="FileClient"/>.
        /// </summary>
        [Browsable(false)]
        public override string ServerUri
        {
            get
            {
                return string.Format("{0}://{1}", TransportProtocol, m_connectData["file"]).ToLower();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Reads a number of bytes from the current received data buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">Destination buffer used to hold copied bytes.</param>
        /// <param name="startIndex">0-based starting index into destination <paramref name="buffer"/> to begin writing data.</param>
        /// <param name="length">The number of bytes to read from current received data buffer and write into <paramref name="buffer"/>.</param>
        /// <returns>The number of bytes read.</returns>
        /// <remarks>
        /// This function should only be called from within the <see cref="ClientBase.ReceiveData"/> event handler. Calling this method outside
        /// this event will have unexpected results.
        /// </remarks>
        /// <exception cref="InvalidOperationException">No received data buffer has been defined to read.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public override int Read(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if ((object)m_fileClient.ReceiveBuffer != null)
            {
                int sourceLength = m_fileClient.BytesReceived - ReadIndex;
                int readBytes = length > sourceLength ? sourceLength : length;
                Buffer.BlockCopy(m_fileClient.ReceiveBuffer, ReadIndex, buffer, startIndex, readBytes);

                // Update read index for next call
                ReadIndex += readBytes;

                if (ReadIndex >= m_fileClient.BytesReceived)
                    ReadIndex = 0;

                return readBytes;
            }

            throw new InvalidOperationException("No received data buffer has been defined to read.");
        }

        /// <summary>
        /// Reads next data buffer from the <see cref="FileStream"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="ReadNextBuffer"/> is called when <see cref="FileClient"/> is not connected.</exception>
        /// <exception cref="InvalidOperationException"><see cref="ReadNextBuffer"/> is called when <see cref="ReceiveOnDemand"/> is disabled.</exception>
        public void ReadNextBuffer()
        {
            if (m_receiveOnDemand)
            {
                if (CurrentState == ClientState.Connected)
                    ReadData();
                else
                    throw new InvalidOperationException("Client is currently not connected");
            }
            else
            {
                throw new InvalidOperationException("ReadNextBuffer() cannot be used when ReceiveOnDemand is disabled");
            }
        }

        /// <summary>
        /// Disconnects the <see cref="FileClient"/> from the <see cref="FileStream"/>.
        /// </summary>
        public override void Disconnect()
        {
            if (CurrentState != ClientState.Disconnected)
            {
                m_fileClient.Reset();
                m_receiveDataTimer.Stop();

                if (m_connectionThread != null)
                    m_connectionThread.Abort();

                OnConnectionTerminated();
            }
        }

        /// <summary>
        /// Connects the <see cref="FileClient"/> to the <see cref="FileStream"/> asynchronously.
        /// </summary>
        /// <exception cref="InvalidOperationException">Attempt is made to connect the <see cref="FileClient"/> when it is not disconnected.</exception>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        public override WaitHandle ConnectAsync()
        {
            m_connectionHandle = (ManualResetEvent)base.ConnectAsync();

            m_fileClient.SetReceiveBuffer(ReceiveBufferSize);

#if ThreadTracking
            m_connectionThread = new ManagedThread(OpenFile);
            m_connectionThread.Name = "GSF.Communication.FileClient.OpenFile()";
#else
            m_connectionThread = new Thread(OpenFile);
#endif
            m_connectionThread.Start();

            return m_connectionHandle;
        }

        /// <summary>
        /// Saves <see cref="FileClient"/> settings to the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void SaveSettings()
        {
            base.SaveSettings();
            if (PersistSettings)
            {
                // Save settings under the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings["AutoRepeat", true].Update(m_autoRepeat);
                settings["ReceiveOnDemand", true].Update(m_receiveOnDemand);
                settings["ReceiveInterval", true].Update(m_receiveInterval);
                settings["StartingOffset", true].Update(m_startingOffset);
                settings["FileOpenMode", true].Update(m_fileOpenMode);
                settings["FileShareMode", true].Update(m_fileShareMode);
                settings["FileAccessMode", true].Update(m_fileAccessMode);
                config.Save();
            }
        }

        /// <summary>
        /// Loads saved <see cref="FileClient"/> settings from the config file if the <see cref="ClientBase.PersistSettings"/> property is set to true.
        /// </summary>
        public override void LoadSettings()
        {
            base.LoadSettings();
            if (PersistSettings)
            {
                // Load settings from the specified category.
                ConfigurationFile config = ConfigurationFile.Current;
                CategorizedSettingsElementCollection settings = config.Settings[SettingsCategory];
                settings.Add("AutoRepeat", m_autoRepeat, "True if receiving (reading) of data is to be repeated endlessly, otherwise False.");
                settings.Add("ReceiveOnDemand", m_receiveOnDemand, "True if receiving (reading) of data will be initiated manually, otherwise False.");
                settings.Add("ReceiveInterval", m_receiveInterval, "Number of milliseconds to pause before receiving (reading) the next available set of data.");
                settings.Add("StartingOffset", m_startingOffset, "Starting point relative to the beginning of the file from where the data is to be received (read).");
                settings.Add("FileOpenMode", m_fileOpenMode, "Open mode (CreateNew; Create; Open; OpenOrCreate; Truncate; Append) to be used when opening the file.");
                settings.Add("FileShareMode", m_fileShareMode, "Share mode (None; Read; Write; ReadWrite; Delete; Inheritable) to be used when opening the file.");
                settings.Add("FileAccessMode", m_fileAccessMode, "Access mode (Read; Write; ReadWrite) to be used when opening the file.");
                AutoRepeat = settings["AutoRepeat"].ValueAs(m_autoRepeat);
                ReceiveOnDemand = settings["ReceiveOnDemand"].ValueAs(m_receiveOnDemand);
                ReceiveInterval = settings["ReceiveInterval"].ValueAs(m_receiveInterval);
                StartingOffset = settings["StartingOffset"].ValueAs(m_startingOffset);
                FileOpenMode = settings["FileOpenMode"].ValueAs(m_fileOpenMode);
                FileShareMode = settings["FileShareMode"].ValueAs(m_fileShareMode);
                FileAccessMode = settings["FileAccessMode"].ValueAs(m_fileAccessMode);
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="FileClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_connectionHandle != null)
                            m_connectionHandle.Dispose();

                        if (m_receiveDataTimer != null)
                        {
                            m_receiveDataTimer.Elapsed -= m_receiveDataTimer_Elapsed;
                            m_receiveDataTimer.Dispose();
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        /// <summary>
        /// Validates the specified <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to be validated.</param>
        /// <exception cref="ArgumentException">File property is missing.</exception>
        protected override void ValidateConnectionString(string connectionString)
        {
            m_connectData = connectionString.ParseKeyValuePairs();

            if (!m_connectData.ContainsKey("file"))
                throw new ArgumentException(string.Format("File property is missing (Example: {0})", DefaultConnectionString));
        }

        /// <summary>
        /// Sends (writes) data to the file asynchronously.
        /// </summary>
        /// <param name="data">The buffer that contains the binary data to be sent (written).</param>
        /// <param name="offset">The zero-based position in the <paramref name="data"/> at which to begin sending (writing) data.</param>
        /// <param name="length">The number of bytes to be sent (written) from <paramref name="data"/> starting at the <paramref name="offset"/>.</param>
        /// <returns><see cref="WaitHandle"/> for the asynchronous operation.</returns>
        protected override WaitHandle SendDataAsync(byte[] data, int offset, int length)
        {
            WaitHandle handle;

            // Send data to the file asynchronously.
            handle = m_fileClient.Provider.BeginWrite(data, offset, length, SendDataAsyncCallback, null).AsyncWaitHandle;

            // Notify that the send operation has started.
            m_fileClient.Statistics.UpdateBytesSent(length);
            OnSendDataStart();

            // Return the async handle that can be used to wait for the async operation to complete.
            return handle;
        }

        /// <summary>
        /// Callback method for asynchronous send operation.
        /// </summary>
        private void SendDataAsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                // Send operation is complete.
                m_fileClient.Provider.EndWrite(asyncResult);
                OnSendDataComplete();
            }
            catch (Exception ex)
            {
                // Send operation failed to complete.
                OnSendDataException(ex);
            }
        }

        /// <summary>
        /// Connects to the <see cref="FileStream"/>.
        /// </summary>
        private void OpenFile()
        {
            int connectionAttempts = 0;
            while (MaxConnectionAttempts == -1 || connectionAttempts < MaxConnectionAttempts)
            {
                try
                {
                    OnConnectionAttempt();

                    // Open the file.
                    m_fileClient.Provider = new FileStream(FilePath.GetAbsolutePath(m_connectData["file"]), m_fileOpenMode, m_fileAccessMode, m_fileShareMode);

                    // Move to the specified offset.
                    m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin);

                    m_connectionHandle.Set();
                    OnConnectionEstablished();

                    if (!m_receiveOnDemand)
                    {
                        if (m_receiveInterval > 0)
                        {
                            // Start receiving data at interval.
                            m_receiveDataTimer.Interval = m_receiveInterval;
                            m_receiveDataTimer.Start();
                        }
                        else
                        {
                            // Start receiving data continuously.
                            while (true)
                            {
                                ReadData();         // Read all available data.
                                Thread.Sleep(1000); // Wait for more data to be available.
                            }
                        }
                    }

                    break;  // We're done here.
                }
                catch (ThreadAbortException)
                {
                    // Exit gracefully.
                    break;
                }
                catch (Exception ex)
                {
                    // Keep retrying connecting to the file.
                    Thread.Sleep(1000);
                    connectionAttempts++;
                    OnConnectionException(ex);
                }
            }
        }

        /// <summary>
        /// Receive (reads) data from the <see cref="FileStream"/>.
        /// </summary>
        private void ReadData()
        {
            try
            {
                // Process the entire file content
                while (m_fileClient.Provider.Position < m_fileClient.Provider.Length)
                {
                    // Retrieve data from the file.
                    m_fileClient.BytesReceived = m_fileClient.Provider.Read(m_fileClient.ReceiveBuffer, 0, m_fileClient.ReceiveBufferSize);
                    m_fileClient.Statistics.UpdateBytesReceived(m_fileClient.BytesReceived);

                    // Notify of the retrieved data.
                    OnReceiveDataComplete(m_fileClient.ReceiveBuffer, m_fileClient.BytesReceived);

                    // Re-read the file if the user wants to repeat when done reading the file.
                    if (m_autoRepeat && m_fileClient.Provider.Position == m_fileClient.Provider.Length)
                    {
                        m_fileClient.Provider.Seek(m_startingOffset, SeekOrigin.Begin);
                    }

                    // Stop processing the file if user has either opted to receive data on demand or receive data at a predefined interval.
                    if (m_receiveOnDemand || m_receiveInterval > 0)
                    {
                        break;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Exit gracefully.
            }
            catch (Exception ex)
            {
                // Notify of the exception.
                if (!(ex is NullReferenceException))
                    OnReceiveDataException(ex);
            }
        }

        private void m_receiveDataTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ReadData();
        }

        /// <summary>
        /// Raises the <see cref="ClientBase.ConnectionException"/> event.
        /// </summary>
        /// <param name="ex">Exception to send to <see cref="ClientBase.ConnectionException"/> event.</param>
        protected override void OnConnectionException(Exception ex)
        {
            base.Disconnect();
            base.OnConnectionException(ex);
        }

        #endregion
    }
}
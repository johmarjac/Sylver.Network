﻿using Sylver.Network.Common;
using Sylver.Network.Server.Internal;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sylver.Network.Tests")]
namespace Sylver.Network.Server
{
    /// <summary>
    /// Provides an abstraction to manage a TCP server.
    /// </summary>
    /// <typeparam name="TClient">The client type that the server will manage.</typeparam>
    public class NetServer<TClient> : NetConnection, INetServer
        where TClient : class, INetServerClient
    {
        private readonly NetServerConfiguration _configuration;
        private readonly BufferManager _bufferManager;
        private readonly ConcurrentDictionary<Guid, TClient> _clients;
        private readonly NetServerClientFactory<TClient> _clientFactory;
        private readonly NetServerAcceptor<TClient> _acceptor;
        private readonly NetServerReceiver<TClient> _receiver;


        /// <inheritdoc />
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Creates a new <see cref="NetServer{TUser}"/> instance.
        /// </summary>
        public NetServer(NetServerConfiguration configuration)
        {
            this._configuration = configuration;
            this._bufferManager = new BufferManager(this._configuration.MaximumNumberOfConnections * this._configuration.ClientBufferSize * 2, this._configuration.ClientBufferSize);
            this._acceptor = new NetServerAcceptor<TClient>(this);
            this._acceptor.OnClientAccepted += this.OnClientAccepted;

            this._receiver = new NetServerReceiver<TClient>(this._bufferManager, this._configuration.MaximumNumberOfConnections);

            this._clientFactory = new NetServerClientFactory<TClient>();
        }

        /// <inheritdoc />
        public void Start()
        {
            if (this.IsRunning)
                throw new InvalidOperationException("Server is already running.");

            if (this.Socket != null)
            {
                this.Socket.Dispose();
                this.Socket = null;
            }

            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            this.Socket.Bind(NetHelper.CreateIpEndPoint(this._configuration.Host, this._configuration.Port));
            this.Socket.Listen(this._configuration.Backlog);

            this.IsRunning = true;
            this._acceptor.StartAccept();
        }

        /// <inheritdoc />
        public void Stop()
        {
            // TODO: stop the server.
        }

        /// <summary>
        /// Fired when a client is accepted to the server.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Accepted client socket async event arguments.</param>
        private void OnClientAccepted(object sender, SocketAsyncEventArgs e)
        {
            TClient newClient = this._clientFactory.CreateClient(e.AcceptSocket, null);

            if (!this._clients.TryAdd(newClient.Id, newClient))
            {
                // TODO: send error.
            }

            this._receiver.InitializeClientAndStartReceiving(newClient);
        }
    }
}
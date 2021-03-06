﻿using System;
using ExtendedHubClient.Abstractions;
using ExtendedHubClient.Abstractions.Proxy;
using ExtendedHubClient.Methods;
using ExtendedHubClient.Proxy;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace ExtendedHubClient
{
    /// <summary>
    /// Provide default realization of <see cref="ITypedHubClient{TServerMethods}"/>.
    /// Client methods from <see cref="TClientMethods"/> can only return a Task.
    /// </summary>
    /// <typeparam name="TServerMethods">Interface that provides a view similar to server methods</typeparam>
    /// <typeparam name="TClientMethods">Interface that provides a view similar to the client's methods</typeparam>
    public class TypedHubClient<TServerMethods, TClientMethods> : BaseHubClient, ITypedHubClient<TServerMethods>
        where TServerMethods: class
        where TClientMethods: class
    {
        public new TServerMethods Server => ProxyCreator.CreateProxyForInterface<TServerMethods>(MethodProxy);
        
        protected override IProxyCreator ProxyCreator { get; }
        protected override IMethodProxy MethodProxy { get; }

        public TypedHubClient(string url,
            Action<HttpConnectionOptions> connectionConfiguration = null,
            Action<IHubConnectionBuilder> additionalHubConfiguration = null,
            ILogger logger = null
            )
            : base(url,
                (hub, hubMethod) => 
                    new DefaultMethodManager(hub, hubMethod, typeof(TServerMethods), typeof(TClientMethods)),
                connectionConfiguration,
                additionalHubConfiguration,
                logger)
        {
            MethodProxy = new TypedMethodProxy(Hub, Methods);
            ProxyCreator = new DefaultProxyCreator();
        }
    }
}
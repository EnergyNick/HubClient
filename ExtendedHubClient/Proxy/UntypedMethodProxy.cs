﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExtendedHubClient.Abstractions;
using ExtendedHubClient.Abstractions.Methods;
using ExtendedHubClient.Abstractions.Proxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace ExtendedHubClient.Proxy
{
    public class UntypedMethodProxy : IMethodProxy
    {
        private readonly HubConnection _hub;
        private readonly IMethodsManager _manager;

        public UntypedMethodProxy(HubConnection hub, IMethodsManager manager)
        {
            _hub = hub;
            _manager = manager;
        }

        public Task OnMethodInvoke(string name, IEnumerable<object> arguments)
        {
            var args = arguments?.ToArray() ?? new object[0];

            if (!_manager.CanArgumentsCallMethodFromRegistration(MethodType.Send, name, args))
                throw new ArgumentException(
                    $"Can't call method {name} with arguments: {string.Join(", ", args.Select(x => x?.ToString()))}");

            if (name != nameof(ISendMethodProxy.SendCoreAsync)
                || args.Length < 2
                || !(args[0] is string methodName) 
                || !(args[1] is object[] methodArgs))
                throw new InvalidOperationException(
                    $"{nameof(UntypedMethodProxy)} can only work with untyped {nameof(IHubClient)}");

            return _hub.SendCoreAsync(methodName, methodArgs);
        }

        public async Task<TResult> OnMethodInvokeWithReturnValue<TResult>(string name, IEnumerable<object> arguments)
        {
            var args = arguments?.ToArray() ?? new object[0];

            if (!_manager.CanArgumentsCallMethodFromRegistration(MethodType.Send, name, args))
                throw new ArgumentException(
                    $"Can't call method {name} with arguments: {string.Join(", ", args.Select(x => x?.ToString()))}");

            if (name != nameof(ISendMethodProxy.InvokeCoreAsync)
                || args.Length < 3
                || !(args[0] is string methodName) 
                || !(args[1] is object[] methodArgs)
                || !(args[2] is Type specialReturnType))
                throw new InvalidOperationException(
                    $"{nameof(UntypedMethodProxy)} can only work with untyped {nameof(IHubClient)}");

            return (TResult) await _hub.InvokeCoreAsync(methodName, specialReturnType, methodArgs)
                .ConfigureAwait(false);
        }
    }
}
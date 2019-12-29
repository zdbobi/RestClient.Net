﻿using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;

namespace RestClientDotNet
{
    public class DefaultHttpClientFactory : IHttpClientFactory, IDisposable
    {
        #region Fields
        private bool disposed;
        private readonly ConcurrentDictionary<string, Lazy<HttpClient>> _httpClients;
        private readonly Func<string, Lazy<HttpClient>> _getOrAddFunc;
        #endregion

        #region Constructor
        public DefaultHttpClientFactory() : this(null)
        {
        }

        public DefaultHttpClientFactory(Func<string, Lazy<HttpClient>> func)
        {
            _getOrAddFunc = func;
            _httpClients = new ConcurrentDictionary<string, Lazy<HttpClient>>();            if (_getOrAddFunc != null) return;            _getOrAddFunc = name =>            {                return new Lazy<HttpClient>(() => new HttpClient(), LazyThreadSafetyMode.ExecutionAndPublication);            };
        }
        #endregion

        #region Implementation
        public HttpClient CreateClient(string name)
        {
            if (name == null)            {                throw new ArgumentNullException(nameof(name));            }            return _httpClients.GetOrAdd(name, _getOrAddFunc).Value;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            foreach (var name in _httpClients.Keys)
            {
                _httpClients[name].Value.Dispose();
            }
        }
        #endregion

        #region Public Methods
        public void AddClient(string name, HttpClient httpClient)
        {
            var lazy = new Lazy<HttpClient>(() => httpClient);
            var lazyFunc = new Func<string, Lazy<HttpClient>, Lazy<HttpClient>>((n, l) => lazy);
#pragma warning disable IDE0058 // Expression value is never used
            _httpClients.AddOrUpdate(name, lazy, lazyFunc);
#pragma warning restore IDE0058 // Expression value is never used
        }
        #endregion
    }
}

﻿using System;
using System.Net;
using Raven.Client.Connection;
using Raven.Client.Connection.Profiling;
using Raven.Client.Counters.Actions;
using Raven.Imports.Newtonsoft.Json;

namespace Raven.Client.Counters
{
	public class CountersClient : IHoldProfilingInformation, IDisposable
    {
		private readonly ICounterStore parent;
		
		private readonly ICountersReplicationInformer replicationInformer;

		private readonly int readStripingBase;

	    internal readonly JsonSerializer JsonSerializer;

		internal readonly HttpJsonRequestFactory JsonRequestFactory;

		public string ServerUrl { get; private set; }

		public string CounterStorageName { get; private set; }


		public ICredentials Credentials { get; private set; }

		public string ApiKey { get; private set; }

		public ProfilingInformation ProfilingInformation { get; private set; }

	    public CountersStats Stats { get; private set; }

	    public ReplicationClient Replication { get; private set; }

	    public CountersCommands Commands { get; private set; }

	    public Convention Conventions { get; private set; }

        /// <summary>
        /// Notify when the failover status changed
        /// </summary>
		public event EventHandler<FailoverStatusChangedEventArgs> FailoverStatusChanged
		{
			add { replicationInformer.FailoverStatusChanged += value; }
			remove { replicationInformer.FailoverStatusChanged -= value; }
		}		

		/// <summary>
		/// Allow access to the replication informer used to determine how we replicate requests
		/// </summary>
		public ICountersReplicationInformer ReplicationInformer
		{
			get { return replicationInformer; }
		}
		 
		public CountersClient(ICounterStore parent, string counterStorageName)
        {
	        this.parent = parent;
	        try
	        {
		        JsonRequestFactory = parent.JsonRequestFactory;
		        JsonSerializer = parent.JsonSerializer;
                ServerUrl = parent.Url;
                if (ServerUrl.EndsWith("/"))
                    ServerUrl = ServerUrl.Substring(0, ServerUrl.Length - 1);

	            Credentials = parent.Credentials.Credentials;
                ApiKey = parent.Credentials.ApiKey;

				Conventions = new Convention();
                replicationInformer = new CounterReplicationInformer(Conventions, JsonRequestFactory);
                readStripingBase = replicationInformer.GetReadStripingBase(true);

		        CounterStorageName = counterStorageName;  
				InitializeActions(counterStorageName);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }		

		private void InitializeActions(string counterStorageName)
	    {
		    Stats = new CountersStats(parent, counterStorageName);
		    Replication = new ReplicationClient(parent, counterStorageName);
			Commands = new CountersCommands(parent,this, counterStorageName, replicationInformer);
	    }

	    public void Dispose()
        {
        }
    }
}

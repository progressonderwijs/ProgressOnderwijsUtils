using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using ProgressOnderwijsUtils.Log4Net;
using log4net;

namespace ProgressOnderwijsUtils
{
	public interface IClientHandle<out T>
	{
		void Call(Action<T> call);
		TR Call<TR>(Func<T, TR> call);
	}

	public class ClientHandle<T, TB> : IClientHandle<TB>, IDisposable 
		where T : class 
		where TB : ClientBase<T>, T
	{
		private readonly object monitor;
		private readonly Func<TB> factory;
		private TB client;

		public ClientHandle(Func<TB> factory)
		{
			monitor = new object();
			this.factory = factory;
			Create();
		}

		private void Create()
		{
			client = factory();
			client.InnerChannel.Faulted += Faulted;
		}

		private void Faulted(object sender, EventArgs e)
		{
			lock (monitor)
			{
				client.Abort();
				Create();
			}
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				client.Close();
			}
			catch 
			{
				client.Abort();
			}
		}

		#endregion

		#region Implementation of IClientHandler<out TU>

		public void Call(Action<TB> call)
		{
			try
			{
				call(client);
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				// could be timeout on the channel, retry
				call(client);
			}
		}

		public TR Call<TR>(Func<TB, TR> call)
		{
			try
			{
				return call(client);
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				// could be timeout on the channel, retry
				return call(client);
			}
		}

		#endregion
	}

	public class ClientFactoryHandle<T> : IClientHandle<T>, IDisposable 
	{
		private readonly object monitor;
		private readonly ChannelFactory<T> factory;
		private T client;

		private ICommunicationObject Client { get { return client as ICommunicationObject; } }

		public ClientFactoryHandle(ChannelFactory<T> factory)
		{
			monitor = new object();
			this.factory = factory;
			Create();
		}

		private void Create()
		{
			client = factory.CreateChannel();
			Client.Faulted += Faulted;
		}

		private void Faulted(object sender, EventArgs e)
		{
			lock (monitor)
			{
				Client.Abort();
				Create();
			}
		}

		#region Implementation of IDisposable

		public void Dispose()
		{
			try
			{
				Client.Close();
			}
			catch 
			{
				Client.Abort();
			}
		}

		#endregion

		#region Implementation of IClientHandler<out TU>

		public void Call(Action<T> call)
		{
			try
			{
				call(client);
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				// could be timeout on the channel, retry
				call(client);
			}
		}

		public TR Call<TR>(Func<T, TR> call)
		{
			try
			{
				return call(client);
			}
			catch (FaultException)
			{
				throw;
			}
			catch (CommunicationException)
			{
				// could be timeout on the channel, retry
				return call(client);
			}
		}

		#endregion
	}

	public abstract class AService : IErrorHandler
	{
		protected ILog Log { get; private set; }

		protected AService()
		{
			Log = LogManager.GetLogger(GetType());
			Log.Info(() => string.Format("Created service '{0}'", this));
		}

		#region Implementation of IErrorHandler

		public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
		{
			// do nothing
		}

		public bool HandleError(Exception error)
		{
			Log.Error(() => "unhandled exception", error);
			return false;
		}

		#endregion
	}
}

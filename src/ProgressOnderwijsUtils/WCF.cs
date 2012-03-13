using System;
using System.ServiceModel;

namespace ProgressOnderwijsUtils
{
	public interface IClientHandle<out T>
	{
		void Call(Action<T> call);
		TR Call<TR>(Func<T, TR> call);
	}

	public class ClientHandle<T, TB> : IClientHandle<T>, IDisposable 
		where T : class where TB : ClientBase<T>, T
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
}

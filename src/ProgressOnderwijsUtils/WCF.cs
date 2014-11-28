using System;
using System.ServiceModel;

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
        readonly object monitor;
        readonly Func<TB> factory;
        TB client;

        public ClientHandle(Func<TB> factory)
        {
            monitor = new object();
            this.factory = factory;
            Create();
        }

        void Create()
        {
            client = factory();
            client.InnerChannel.Faulted += Faulted;
        }

        void Faulted(object sender, EventArgs e)
        {
            lock (monitor) {
                client.Abort();
                Create();
            }
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            try {
                client.Close();
            } catch {
                client.Abort();
            }
        }
        #endregion

        #region Implementation of IClientHandler<out TU>
        public void Call(Action<TB> call)
        {
            try {
                call(client);
            } catch (FaultException) {
                throw;
            } catch (CommunicationException) {
                // could be timeout on the channel, retry
                call(client);
            }
        }

        public TR Call<TR>(Func<TB, TR> call)
        {
            try {
                return call(client);
            } catch (FaultException) {
                throw;
            } catch (CommunicationException) {
                // could be timeout on the channel, retry
                return call(client);
            }
        }
        #endregion
    }

    public class ClientFactoryHandle<T> : IClientHandle<T>, IDisposable
    {
        readonly object monitor;
        readonly ChannelFactory<T> factory;
        T client;
        ICommunicationObject Client { get { return client as ICommunicationObject; } }

        public ClientFactoryHandle(ChannelFactory<T> factory)
        {
            monitor = new object();
            this.factory = factory;
            Create();
        }

        void Create()
        {
            client = factory.CreateChannel();
            Client.Faulted += Faulted;
        }

        void Faulted(object sender, EventArgs e)
        {
            lock (monitor) {
                Client.Abort();
                Create();
            }
        }

        #region Implementation of IDisposable
        public void Dispose()
        {
            try {
                Client.Close();
            } catch {
                Client.Abort();
            }
        }
        #endregion

        #region Implementation of IClientHandler<out TU>
        public void Call(Action<T> call)
        {
            try {
                call(client);
            } catch (FaultException) {
                throw;
            } catch (CommunicationException) {
                // could be timeout on the channel, retry
                call(client);
            }
        }

        public TR Call<TR>(Func<T, TR> call)
        {
            try {
                return call(client);
            } catch (FaultException) {
                throw;
            } catch (CommunicationException) {
                // could be timeout on the channel, retry
                return call(client);
            }
        }
        #endregion
    }
}

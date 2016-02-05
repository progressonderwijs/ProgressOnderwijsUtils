using System;
using System.ServiceModel;

namespace ProgressOnderwijsUtils
{
    public interface IClientHandle<out T> : IDisposable
    {
        TR Call<TR>(Func<T, TR> call);
    }

    public class ClientHandle<T, TB> : IClientHandle<TB>
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
            client.InnerChannel.Faulted += (o, args) => Faulted();
        }

        void Faulted()
        {
            lock (monitor) {
                client.Abort();
                Create();
            }
        }

        public void Dispose()
        {
            try {
                client.Close();
            } catch {
                client.Abort();
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
    }

    public class ClientFactoryHandle<T> : IClientHandle<T>
    {
        readonly object monitor;
        readonly ChannelFactory<T> factory;
        T client;
        ICommunicationObject Client => client as ICommunicationObject;

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

        public void Dispose()
        {
            try {
                Client.Close();
            } catch {
                Client.Abort();
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
    }
}

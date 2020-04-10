using System;
using System.Collections.Generic;
using System.IO;

namespace ProgressOnderwijsUtils
{
    public interface IResourceStore
    {
        bool ResourceExists(string filename);
        Stream GetResource(string filename);
        IEnumerable<string> GetResourceNames();
    }

    public abstract class ResourceStore<T> : IResourceStore
        where T : ResourceStore<T>
    {
        protected ResourceStore()
        {
            if (typeof(T) != GetType()) {
                throw new InvalidOperationException(
                    "Invalid inheritance:\n" +
                    GetType().FriendlyName() + " inherits from " +
                    GetType().BaseType?.FriendlyName() + " but it was expected to inherit from " +
                    typeof(ResourceStore<T>).FriendlyName());
            }
        }

        public Stream GetResource(string filename)
            => typeof(T).GetResource(filename) ?? throw new KeyNotFoundException("Resource not found: " + filename);

        public bool ResourceExists(string filename)
            => typeof(T).GetResource(filename) != null;

        public IEnumerable<string> GetResourceNames()
        {
            var nsPrefix = typeof(T).Namespace + ".";
            foreach (var fullname in typeof(T).Assembly.GetManifestResourceNames()) {
                if (fullname.StartsWith(nsPrefix, StringComparison.Ordinal)) {
                    yield return fullname.Substring(nsPrefix.Length);
                }
            }
        }
    }
}

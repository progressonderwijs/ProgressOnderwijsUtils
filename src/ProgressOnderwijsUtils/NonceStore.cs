using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
	public sealed class NonceStoreItem : IEquatable<NonceStoreItem>
	{
		public NonceStoreItem(string context, DateTime? timestamp, string nonce)
		{
			if (string.IsNullOrWhiteSpace(context)) throw new ArgumentException("context is required");
			if (string.IsNullOrWhiteSpace(nonce)) throw new ArgumentException("nonce is required");

			Context = context;
			Timestamp = (timestamp ?? new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc)).ToUniversalTime();
			Nonce = nonce;
		}

		public string Context { get; private set; }
		public DateTime Timestamp { get; private set; }
		public string Nonce { get; private set; }

		#region Implementation of IEquatable<NonceStoreItem>

		public bool Equals(NonceStoreItem other)
		{
			return !ReferenceEquals(other, null) &&
				other.Timestamp.Equals(Timestamp) && Equals(other.Context, Context) && Equals(other.Nonce, Nonce);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as NonceStoreItem);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = Timestamp.GetHashCode();
				result = (result * 397) ^ Context.GetHashCode();
				result = (result * 397) ^ Nonce.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(NonceStoreItem left, NonceStoreItem right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(NonceStoreItem left, NonceStoreItem right)
		{
			return !(left == right);
		}

		#endregion
	}

	public interface INonceStore
	{
		string Generate();

		bool IsOriginal(NonceStoreItem item);
		bool IsInWindow(NonceStoreItem item);
		bool IsNotKnown(NonceStoreItem item);
	}

	public class NonceStore : INonceStore
	{
		private readonly TimeSpan window;
		private readonly int cleanup;
		private int counter;
		private int nonce;
    	private readonly object monitor = new object();
		private readonly ISet<NonceStoreItem> items;

		public NonceStore(TimeSpan? window = null, int cleanup = 100)
		{
			if (cleanup <= 0) throw new ArgumentException();

			this.window = window ?? new TimeSpan(0, 10, 0);
			this.cleanup = cleanup;
			counter = 0;
			nonce = 0;
			monitor = new object();
			items = new HashSet<NonceStoreItem>();
		}

		public string Generate()
		{
			lock (monitor)
			{
				try
				{
					return (++nonce).ToStringInvariant();
				}
				catch (OverflowException)
				{
					nonce = 0;
					return Generate();
				}	
			}
		}

		public bool IsOriginal(NonceStoreItem item)
		{
			return IsInWindow(item) && IsNotKnown(item);
		}

    	public bool IsInWindow(NonceStoreItem item)
    	{
    		return (DateTime.UtcNow - item.Timestamp).Duration() <= window;
    	}

    	public bool IsNotKnown(NonceStoreItem item)
    	{
			lock (monitor)
			{
				bool result = items.Add(item);
				if (result) // to avoid as much busy looping during an attack as possible
				{
					CleanUp();	
				}
				return result;
			}
    	}

		private void CleanUp()
		{
			if (++counter == cleanup)
			{
				counter = 0;
				DateTime now = DateTime.UtcNow;
				foreach (NonceStoreItem expired in items.Where(item => now - item.Timestamp > window).ToArray())
				{
					items.Remove(expired);
				}
			}
		}
	}
}
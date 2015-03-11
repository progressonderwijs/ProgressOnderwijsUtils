using System;
using System.Diagnostics;
using System.Globalization;
using JetBrains.Annotations;

namespace ProgressOnderwijsUtils
{
    public static class IdentifierTools
    {
        public static string DbPrimaryKeyName<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            //
            return typeof(T).Name + "id";
        }

        public static string DbForeignKeyName<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            //
            return typeof(T).Name;
        }
    }

    // de IIdentifier wordt met name in de AutoLoadFromDb gebruikt om de mapping van Sql Server naar .Net uit te voeren
    public interface IIdentifier
    {
        bool HasValue { get; }
        void SetValue(int value);
        int GetValue();
        string DbPrimaryKeyName { get; }
        string DbForeignKeyName { get; }
    }

    public class Identifier
    {
        public static Type BaseType { get { return typeof(int); } }
    }

    [Serializable, DebuggerStepThrough]
    public class Identifier<T> : IIdentifier, IComparable<T>
        where T : Identifier<T>, new()
    {
        public Identifier() { }
        public Identifier(int value) { SetValue(value); }
        public virtual string DbPrimaryKeyName { get { return typeof(T).Name.ToLower(CultureInfo.InvariantCulture) + "id"; } }
        public virtual string DbForeignKeyName { get { return typeof(T).Name.ToLower(CultureInfo.InvariantCulture); } }
        bool valueSet = false;
        int _value;
        public bool HasValue { get { return valueSet; } }

        public void SetValue(int value)
        {
            valueSet = true;
            _value = value;
        }

        public int GetValue()
        {
            if (!valueSet) {
                throw new ArgumentException("De waarde van deze identifier wordt uitgelezen voordat deze is gezet");
            }
            return _value;
        }

        public override int GetHashCode() { return ((int)this).GetHashCode(); }
        public int CompareTo(T other) { return GetValue().CompareTo(other.GetValue()); }

        /// <summary>
        /// De waarde van de identifier als string
        /// </summary>
        public override string ToString() { return ((int)this).ToString(CultureInfo.InvariantCulture); }

        public override bool Equals(object obj)
        {
            var val = (Identifier<T>)obj;
            return (object)val != null && Equals(val);
        }

        public bool Equals(int value) { return value == (int)this; }
        bool Equals(Identifier<T> obj) { return (int)obj == (int)this; }

        // Alleen expliciete casts toestaan
        public static explicit operator Identifier<T>(int value)
        {
            var t = new T();
            t.SetValue(value);
            return t;
        }

        public static explicit operator int(Identifier<T> value) { return value.GetValue(); }

        public static bool operator ==(Identifier<T> a, Identifier<T> b)
        {
            if (((object)a == null) && ((object)b == null)) {
                return true;
            }

            if (((object)a == null) || ((object)b == null)) {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Identifier<T> a, Identifier<T> b) { return (!(a == b)); }

        //
        // These are called by the JIT
        //
#pragma warning disable 169
        //
        // JIT implementation of box valuetype Identifier
        //
        [UsedImplicitly]
        static object Box(int o) { return o; }

        [UsedImplicitly]
        static int Unbox(object o) { return (int)o; }
#pragma warning restore 169
    }
}

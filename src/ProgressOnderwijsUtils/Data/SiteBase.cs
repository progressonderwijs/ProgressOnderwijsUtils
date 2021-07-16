using System;
using System.ComponentModel;

namespace ProgressOnderwijsUtils
{
    public abstract class SiteBase : ISite
    {
        public virtual object GetService(Type serviceType)
            => throw new NotSupportedException();

        public virtual IComponent Component
            => throw new NotSupportedException();

        public virtual IContainer Container
            => null!;

        public virtual bool DesignMode
            => throw new NotSupportedException();

        public virtual string? Name
        {
            get
                => throw new NotSupportedException();
            set
                => throw new NotSupportedException();
        }
    }
}

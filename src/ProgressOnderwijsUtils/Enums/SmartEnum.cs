using System;
using System.Collections.Generic;
using System.Linq;

namespace ProgressOnderwijsUtils
{
    public abstract class SmartEnum
    {
        public int Id { get; }

        public ITranslatable Text { get; }

        protected SmartEnum(int id, ITranslatable text)
        {
            Id = id;
            Text = text;
        }
    }
}

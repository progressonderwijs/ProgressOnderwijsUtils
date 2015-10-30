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

        public override string ToString()
        {
            return Text.Translate(Taal.NL).Text;
        }
    }
}

using System;

namespace ProgressOnderwijsUtils
{
    /// <summary>
    /// Deze class maakt het mogelijk om de tijd terug of vooruit te zetten.
    /// </summary>
    public class ProgressDateTime
    {
        /// <summary>
        /// Het aantal dagen dat de tijd terug of vooruit moet worden gezet
        /// </summary>
        public int DaysToAdd { get; set; }

        /// <summary>
        /// De datum die vanaf nu als huidige datum moet gelden 
        /// (de Datetime wordt afgerond naar hele dagen)
        /// </summary>
        [CodeDieAlleenWordtGebruiktInTests]
        public void TimeTravelToDate(DateTime value)
        {
            // Als het nu 21-9 is en value is 21-8 dan is DaysToAdd -31
            DaysToAdd = value.Date.Subtract(DateTime.Now.Date).Days;
        }

        /// <summary>
        /// De (voor de software) huidige datumtijd
        /// </summary>
        public DateTime Now => DateTime.Now.AddDays(DaysToAdd);

        /// <summary>
        /// De (voor de software) huidige datum
        /// </summary>
        public DateTime Today => DateTime.Today.AddDays(DaysToAdd);
    }

    public sealed class ProgressTimeTraveller : IDisposable
    {
        readonly ProgressDateTime dateTime;
        readonly DateTime travelFromDate;

        public ProgressTimeTraveller(ProgressDateTime dateTime, DateTime travelToDate)
        {
            this.dateTime = dateTime;
            travelFromDate = dateTime.Now;

            dateTime.TimeTravelToDate(travelToDate);
        }

        public ProgressTimeTraveller(ProgressDateTime dateTime, int daysToAdd)
            : this(dateTime, dateTime.Now.AddDays(daysToAdd)) { }

        public void Dispose()
        {
            dateTime.TimeTravelToDate(travelFromDate);
        }
    }
}

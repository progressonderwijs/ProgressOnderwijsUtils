using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgressOnderwijsUtils.Data
{
    interface IBuildableQuery
    {
        void AppendTo(CommandFactory factory);
    }
}

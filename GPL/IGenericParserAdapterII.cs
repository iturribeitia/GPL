using GenericParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPL
{
    interface IGenericParserAdapterII
    {
         string SQL_ConnectionString { get; set; }

        void ExportToTable();
    }
}

using GenericParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GPL
{
    public class GenericParserAdapterII : GenericParserAdapter, IGenericParserAdapterII
    {
        public string SQL_ConnectionString
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public void ExportToTable()
        {
            // https://stackoverflow.com/questions/16696448/how-to-make-a-copy-of-an-object-in-c-sharp

            // Need read just first row to feed all the columns names from 
            var parser = new GenericParserAdapterII();
            parser = (GenericParserAdapterII)this.MemberwiseClone();
            parser.Read();
            //throw new NotImplementedException();
        }
    }
}

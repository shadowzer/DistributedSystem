using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Node
{
    public interface IAPI
    {
        string Get(string id);
        void Put(string id, [FromBody]string value);
        void Delete(string id);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace WebRef.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        IDataProtector _protector;
        public ValuesController(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("ValuesController");
        }

        // GET api/values/5
        [HttpGet("{text}")]
        public ActionResult<object> Get(string text)
        {
            return _protector.Unprotect(text);
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace WebEncrypt.Controllers
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

        [HttpGet("{text}")]
        public ActionResult<object> Get(string text)
        {
            return _protector.Protect(text);
        }
    }
}

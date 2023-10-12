using System;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;

namespace ScSoMe.API.Controllers
{

    [ApiController]
    [Route("")]
    [Produces("application/json")]
    public class iOSDeepLinking : ControllerBase
	{
		public iOSDeepLinking()
		{

		}

        [HttpGet("apple-app-site-association")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public Wrapper Get()
        {
            var det = new details();
            var wrapper = new Wrapper();
            wrapper.applinks = new applinks();
            wrapper.applinks.apps = new List<string>();
            wrapper.applinks.details = new List<details>();
            det.appID = "23V49556TC.com.companyname.scsome.mobileapp";
            det.paths = new List<string>();
            det.paths.Add("*");
            wrapper.applinks.details.Add(det);
            return wrapper;
        }

        public class Wrapper
        {
            public applinks applinks { get; set; }
        }

        public class applinks
        {
            public List<string> apps { get; set; }
            public List<details> details { get; set; }
        }

        public class details
        {
            public string appID { get; set; }
            public List<string> paths { get; set; }
        }
    }
}


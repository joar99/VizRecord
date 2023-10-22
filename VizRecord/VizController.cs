using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VizRecord
{

    [ApiController]
    [Route("api/[controller]")]
    public class VizController : ControllerBase
    {
        private Form1 form;

        public VizController(Form1 form)
        {
            this.form = form;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var monitors = Screen.AllScreens.Select(s => new
            {
                s.DeviceName,
            }).ToList();

            if (!monitors.Any())
            {
                return NotFound();
            } else
            {
                return Ok(monitors);
            }
        }

        [HttpPost("{monitor}")]
        public IActionResult Post([FromBody] JObject data, string monitor)
        {
            if (string.IsNullOrEmpty(monitor))
            {
                return BadRequest("Please provide a monitor name");
            }

            form.POSTRecord(data["monitor"].ToString());

            return Ok("Recording successfully started");
        }


    }
}

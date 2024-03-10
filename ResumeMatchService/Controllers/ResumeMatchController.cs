using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResumeMatchService.Services;

namespace ResumeMatchServices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResumeMatchController : ControllerBase
    {
        private readonly ResumeService _resumeMatchService;

        public ResumeMatchController()
        {
            _resumeMatchService = new ResumeService();
        }
        [HttpGet("Search")]
        public IActionResult GetMatchResult(string docPath, string fileName, string domain, string skill)
        {
            var result = _resumeMatchService.GetResumeMatchRecords(docPath, fileName,domain,skill);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}

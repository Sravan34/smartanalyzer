using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ResumeMatchService.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using ResumeMatchServices.Models;
using Microsoft.AspNetCore.Hosting;
using ResumeMatchService.Models;

namespace ResumeMatchServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResumeMatchController : ControllerBase
    {
        private readonly ResumeService _resumeMatchService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ResumeMatchController(IWebHostEnvironment webHostEnvironment)
        {
            _resumeMatchService = new ResumeService();
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet("SearchAPI")]
        public IActionResult GetMatchResult(string context,string category,double threshold,int noOfMatches,string inputPath)
        {
            string contentRootPath = _webHostEnvironment.ContentRootPath;
            if ((category.ToLowerInvariant() == DocType.Job.ToString().ToLowerInvariant())
             || (category.ToLowerInvariant() == DocType.Resume.ToString().ToLowerInvariant()))
            {

               var  result = _resumeMatchService.GetResumeMatchRecords(context, category, threshold, noOfMatches, inputPath);

                if (result.Count == 0)
                {
                    var message = string.Format("No Results Found ");
                    return NotFound(message);

                }

                return Ok(result);
            }
            else
            {

                return BadRequest("Please check category should be either resume or job!");

            }
        }
    }
}

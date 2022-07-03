using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TopLearn.DataLayer.Context;

namespace TopLearn.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseApiController : ControllerBase
    {
        private TopLearnContext _topLearnContext;

        public CourseApiController(TopLearnContext topLearnContext)
        {
            _topLearnContext = topLearnContext;
        }
        [Produces("application/json")]
        [HttpGet("search")]
        public IActionResult Search()
        {
            try
            {
                string term = HttpContext.Request.Query["term"].ToString();
                var courseTitle = _topLearnContext.Courses
                    .Where(c => c.CourseTitle.Contains(term) || c.Tags.Contains(term) || c.CourseDescription.Contains(term))
                    .Select(c => c.CourseTitle)
                    .ToList();
                return Ok(courseTitle);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}

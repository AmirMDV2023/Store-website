using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TopLearn.Core.Services.Interfaces;
using TopLearn.DataLayer.Entities.Course;

namespace TopLearn.Web.Pages.Admin.Roles
{
    public class DeleteGroupModel : PageModel
    {
        private ICourseService _courseService;

        public DeleteGroupModel(ICourseService courseService)
        {
            _courseService = courseService;
        }
        [BindProperty]
        public CourseGroup CourseGroup { get; set; }
        public void OnGet(int id)
        {
            CourseGroup = _courseService.GetById(id);
        }

        public IActionResult OnPost()
        {
            _courseService.DeleteGroup(CourseGroup);
            return RedirectToPage("Index");
        }
    }
}

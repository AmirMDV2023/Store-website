using Microsoft.AspNetCore.Mvc.RazorPages;
using TopLearn.Core.Security;
using TopLearn.Core.Services.Interfaces;

namespace TopLearn.Web.Pages.Admin
{
    [PermissionChecker(1)]
    public class IndexModel : PageModel
    {
        private IUserService _userService;
        private ICourseService _courseService;
        public IndexModel(IUserService userService,ICourseService courseService)
        {
            _userService = userService;
            _courseService = courseService;
        }
        public int countCourses;
        public int countUsers;
        public void OnGet()
        {
            countCourses = _courseService.GetCourseCount();
            countUsers = _userService.GetCountUsers();
        }
    }
}

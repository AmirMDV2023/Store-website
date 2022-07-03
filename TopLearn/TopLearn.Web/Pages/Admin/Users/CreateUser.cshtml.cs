using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TopLearn.Core.DTOs;
using TopLearn.Core.Security;
using TopLearn.Core.Services.Interfaces;

namespace TopLearn.Web.Pages.Admin.Users
{
    [PermissionChecker(3)]
    public class CreateUserModel : PageModel
    {
        private IUserService _userService;
        private IPermissionService _permissionService;

        public CreateUserModel(IPermissionService permissionService, IUserService userService)
        {
            _permissionService = permissionService;
            _userService = userService;
        }
        [BindProperty]
        public CreateUserViewModel CreateUserViewModel { get; set; }
        public void OnGet()
        {
            ViewData["Roles"] = _permissionService.GetRoles();
        }

        public IActionResult OnPost(List<int> SelectedRoles=null)
        {
            if (!ModelState.IsValid)
                return Page();
            int userId = _userService.AddUserFromAdmin(CreateUserViewModel);

            //Add Roles
            _permissionService.AddRolesToUser(SelectedRoles, userId);

            return Redirect("/Admin/Users");
        }
    }
}

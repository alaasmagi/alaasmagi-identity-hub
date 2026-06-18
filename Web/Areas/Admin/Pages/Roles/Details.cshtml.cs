using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DTO.DataAccess.DTO;
using DataAccess.Context;

namespace Web.Areas.Admin.Pages.Roles
{
    public class DetailsModel : PageModel
    {
        private readonly DataAccess.Context.AppDbContext _context;

        public DetailsModel(DataAccess.Context.AppDbContext context)
        {
            _context = context;
        }

        public AppRoleEntity AppRoleEntity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var approleentity = await _context.Roles
                .Include(role => role.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (approleentity is not null)
            {
                AppRoleEntity = approleentity;

                return Page();
            }

            return NotFound();
        }
    }
}

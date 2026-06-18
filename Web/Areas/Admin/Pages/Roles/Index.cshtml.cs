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
    public class IndexModel : PageModel
    {
        private readonly DataAccess.Context.AppDbContext _context;

        public IndexModel(DataAccess.Context.AppDbContext context)
        {
            _context = context;
        }

        public IList<AppRoleEntity> AppRoleEntity { get;set; } = default!;

        public async Task OnGetAsync()
        {
            AppRoleEntity = await _context.Roles
                .Include(a => a.Client).ToListAsync();
        }
    }
}

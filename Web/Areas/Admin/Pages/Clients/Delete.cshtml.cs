using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DTO.DataAccess.DTO;
using DataAccess.Context;

namespace Web.Areas.Admin.Pages.Clients
{
    public class DeleteModel : PageModel
    {
        private readonly DataAccess.Context.AppDbContext _context;

        public DeleteModel(DataAccess.Context.AppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ClientEntity ClientEntity { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliententity = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);

            if (cliententity is not null)
            {
                ClientEntity = cliententity;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliententity = await _context.Clients.FindAsync(id);
            if (cliententity != null)
            {
                ClientEntity = cliententity;
                _context.Clients.Remove(ClientEntity);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}

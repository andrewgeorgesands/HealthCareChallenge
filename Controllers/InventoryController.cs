using HealthCareChallenge.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthCareChallenge.Controllers;

[Authorize]
public class InventoryController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var inventory = await context.Inventory.ToListAsync();
        return View(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Reset()
    {
        var inventory = await context.Inventory.ToListAsync();
        foreach (var item in inventory)
        {
            item.QuantityInStock = 10;
        }
        await context.SaveChangesAsync();
        TempData["Message"] = "Inventory has been reset to 10 units for all medications.";
        return RedirectToAction(nameof(Index));
    }
}


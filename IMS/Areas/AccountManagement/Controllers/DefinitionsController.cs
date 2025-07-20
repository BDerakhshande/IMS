using ClosedXML.Excel;
using IMS.Areas.AccountManagement.Data;
using IMS.Areas.AccountManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMS.Areas.AccountManagement.Controllers
{
    [Area("AccountManagement")]
    public class DefinitionsController : Controller
    {
        private readonly AccountManagementDbContext _context;

        public DefinitionsController(AccountManagementDbContext context)
        {
            _context = context;
        }

        

        #region Counterparty



        #endregion

        #region CounterpartyCreate
        public IActionResult CounterpartyCreate()
        {
           
            return View();
        }


        #endregion

        #region CounterpartyEdit

        #endregion

        #region DeleteConfirmed

        #endregion
        #region CostCenter

        // GET: Definitions/CostCenter
        public IActionResult CostCenter()
        {
          
            var costCenters = _context.CostCenters.ToList();
            return View(costCenters);
        }

        // GET: Definitions/GetSecondTafzilsByCostCenter
        [HttpGet]
        public IActionResult GetSecondTafzilsByCostCenter(int costCenterId)
        {
           

            var secondTafzils = _context.SecondTafzils
                .Where(s => s.CostCenterId == costCenterId)
                .Include(s => s.Tafzil)
                .Select(s => new
                {
                    s.Code,
                    s.Name,
                    Tafzil = s.Tafzil != null ? new { s.Tafzil.Name } : null
                })
                .ToList();

            return Json(secondTafzils);
        }

        // GET: Definitions/ExportToExcelCostCenter██

        // Export to Excel
        public IActionResult ExportToExcelCostCenter()
        {
          

            var costCenters = _context.CostCenters.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Cost Centers");

                // Set headers
                worksheet.Cell(1, 1).Value = "ردیف";
                worksheet.Cell(1, 2).Value = "نام مرکز هزینه";
                worksheet.Cell(1, 3).Value = "نوع تراکنش";

                // Populate data
                int row = 2;
                int index = 1;
                foreach (var costCenter in costCenters)
                {
                    worksheet.Cell(row, 1).Value = index++;
                    worksheet.Cell(row, 2).Value = costCenter.Name;
                    worksheet.Cell(row, 3).Value = costCenter.Type.ToString();
                    row++;
                }

                // Auto-fit columns
                worksheet.Columns().AdjustToContents();

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Position = 0;
                    var fileName = "CostCenters.xlsx";
                    return File(memoryStream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        #endregion

        #region CostCenterCreate
        public IActionResult CostCenterCreate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CostCenterCreate(CostCenter costCenter)
        {
            if (ModelState.IsValid)
            {
                _context.CostCenters.Add(costCenter);
                _context.SaveChanges();
                return RedirectToAction("CostCenter", "Definitions");
            }
            return View(costCenter);
        }

        #endregion

        #region CostCenterEdit
        public IActionResult CostCenterEdit(int id)
        {
           
            var costCenter = _context.CostCenters.FirstOrDefault(c => c.Id == id);
            if (costCenter == null)
            {
                return NotFound();
            }
            return View(costCenter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CostCenterEdit(int id, CostCenter costCenter)
        {
            if (id != costCenter.Id)
            {
                return NotFound();
            }

            try
            {
                _context.Update(costCenter);
                _context.SaveChanges();
                return RedirectToAction("CostCenter", "Definitions");
            }
            catch (Exception)
            {
                if (!_context.CostCenters.Any(c => c.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }
        #endregion

        #region CostCenterDeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CostCenterDeleteConfirmed(int id)
        {
            var costCenter = _context.CostCenters.Find(id);

            if (costCenter == null)
            {
                TempData["ErrorMessage"] = "مرکز هزینه یافت نشد.";
                return RedirectToAction("CostCenter", "Definitions");
            }

            var hasTransactions = _context.Transactions.Any(t => t.CostCenterId == id);
            if (hasTransactions)
            {
                TempData["ErrorMessage"] = "این مرکز هزینه دارای تراکنش است و نمی‌تواند حذف شود.";
                return RedirectToAction("CostCenter", "Definitions");
            }

            _context.CostCenters.Remove(costCenter);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "مرکز هزینه با موفقیت حذف شد.";
            return RedirectToAction("CostCenter", "Definitions");
        }

        #endregion
    }
}

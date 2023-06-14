using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEST_MVC.Models;
using X.PagedList;

namespace TEST_MVC.Controllers
{
    public class CustomerController : Controller
    {
        private readonly WebAPIContext _db;

        public CustomerController(WebAPIContext context)
        {
            _db = context;
        }

        #region 獲取商品列表

        public async Task<IActionResult> IndexAsync(int? Page)
        {
            var customer = await _db.Customers.OrderBy(x => x.Id).ToPagedListAsync(Page.HasValue ? Page.Value - 0 : 1, 5);
            return View(customer);
        }

        #endregion 獲取商品列表
    }
}
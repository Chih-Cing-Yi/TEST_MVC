using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using TEST_MVC.Models;
using TEST_MVC.Models.ViewModels;
using X.PagedList;

namespace TEST_MVC.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly WebAPIContext _db;

        public ProductsController(WebAPIContext context)
        {
            _db = context;
        }

        #region 獲取商品列表

        // GET: Products
        public async Task<IActionResult> Index(int? Page, string name, string type)
        {
            ViewData["type"] = type;
            ViewData["name"] = name;
            //1.Contains(LIKE查詢 %變數%) 2.StartsWith(LIKE查詢 變數%) 3.EndsWith(LIKE查詢 %變數)
            //Where查詢前先判斷name,type是否為空
            var products = await _db.Products
                .Where(x => (string.IsNullOrEmpty(name) || x.Name.Contains(name))
                && (string.IsNullOrEmpty(type) || x.ProductTypeId == Convert.ToInt16(type)))
                .Join(_db.ProductTypes,
                p => p.ProductTypeId,
                type => type.Id,
                (p, type) => new ProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    ProductTypeId = p.ProductTypeId,
                    ProductTypeName = type.name,
                    CreatDate = p.CreatDate,
                    CreatUser = p.CreatUser,
                    EditDate = p.EditDate,
                    EditUser = p.EditUser,
                }
             ).OrderBy(x => x.Id).ToPagedListAsync(Page.HasValue ? Page.Value - 0 : 1, 5);

            //獲取商品分頁(通常只改最後兩個數字 1是開始 3是一頁五筆資料)
            //var products = await _db.Products.OrderBy(x => x.Id).ToPagedListAsync(Page.HasValue ? Page.Value - 0 : 1, 5);
            //獲取全部商品(不分頁)
            //var products = await _db.Products.ToListAsync();
            return View(products);
        }

        #endregion 獲取商品列表

        #region 獲取商品資料 (axios)

        // GET: Products
        public async Task<IActionResult> DataAll(int? Id)
        {
            var products = await _db.Products.ToListAsync();
            return Json(products);
        }

        // GET: Products
        public async Task<IActionResult> Data(int? Id)
        {
            var products = await _db.Products.FirstOrDefaultAsync(x => x.Id == Id);
            return Json(products);
        }

        #endregion 獲取商品資料 (axios)

        #region 新增商品

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile myfile)
        {
            //找到USER資料
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(User.Identity!.Name));

            if (ModelState.IsValid)
            {
                #region 檔案處理

                //圖片為空就不執行
                if (myfile != null)
                {
                    //保存檔案名稱
                    product.ImageUrl = await SaveImage(myfile);
                }

                #endregion 檔案處理

                Product item = new Product();
                item.Name = product.Name;
                item.Price = product.Price;
                item.Stock = product.Stock;
                item.ProductTypeId = product.ProductTypeId;
                //item.CreatUser = user.Name;
                item.CreatUser = "Simon";
                item.ImageUrl = product.ImageUrl;
                item.CreatDate = DateTime.Now;

                await _db.AddRangeAsync(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));//路由該控制器下的index
            }
            //失敗維持輸入的東西
            return View(product);
        }

        #endregion 新增商品

        #region 編輯商品

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //判斷輸入值是否為空
            if (id == null || _db.Products == null)
            {
                return NotFound();
            }
            //查詢修改項目
            var product = await _db.Products.FindAsync(id);
            //判斷product是否為空
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile myfile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }
            //驗證綁定的值
            if (ModelState.IsValid)
            {
                #region 檔案處理

                //圖片為空就不執行
                if (myfile != null)
                {
                    if (product.ImageUrl != null)//判斷是否有舊圖片
                    {
                        //刪除舊圖片
                        RemoveImage(product.ImageUrl);
                    }
                    //保存檔案名稱
                    product.ImageUrl = await SaveImage(myfile);
                }

                #endregion 檔案處理

                try
                {
                    //查詢要修改的值
                    var item = await _db.Products.FindAsync(product.Id);
                    if (item == null) { return NotFound(); }//404
                    //修改的
                    item.Name = product.Name;
                    item.Price = product.Price;
                    item.Stock = product.Stock;
                    if (myfile != null) { item.ImageUrl = product.ImageUrl; } //有檔案才修改
                    item.EditDate = DateTime.Now;
                    await _db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    TempData["Error"] = e.ToString();
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        #endregion 編輯商品

        #region 刪除商品

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //判斷輸入值是否為空
            if (id == null || _db.Products == null)
            {
                return NotFound();
            }
            //查詢刪除的值
            var product = await _db.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            //判斷product是否為空
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_db.Products == null)
            {
                return Problem("Entity set 'WebAPIContext.Products'  is null.");
            }
            //查詢刪除的值
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                //圖片不為空 就刪除圖片檔案
                if (product.ImageUrl != null) { RemoveImage(product.ImageUrl); }
                _db.Products.Remove(product);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion 刪除商品

        #region 私有方法

        //刪除檔案
        private void RemoveImage(string? fileName)
        {
            if (fileName != null)
            {
                //獲得檔案路徑
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                //刪除檔案
                System.IO.File.Delete(filePath);
            }
        }

        //檔案處理
        private async Task<string> SaveImage(IFormFile myfile)
        {
            //檔案名稱處理 2種方式
            //(1)GUID當作名稱保存
            //(2)檔案名稱用時間保存
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            //保存位址 (Path.Combine路徑拼接功能        用string接其實也可以)
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            //保存檔案
            using (var stream = System.IO.File.Create(filePath))
            {
                await myfile.CopyToAsync(stream);
            }
            return fileName;
        }

        //判斷讀取到的資料是否為空
        private bool ProductExists(int id)
        {
            return (_db.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        #endregion 私有方法
    }
}
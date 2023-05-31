using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TEST_MVC.Data;
using TEST_MVC.Models;

namespace TEST_MVC.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly WebAPIContext _context;

        public ProductsController(WebAPIContext context)
        {
            _context = context;
        }

        #region 獲取商品列表
        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }
        #endregion

        #region 明細表
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        #endregion

        #region 新增商品
        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Stock,ImageUrl,CreatDate,EditDate")] Product product, IFormFile myfile)
        {
            if (ModelState.IsValid)
            {
                #region 檔案處理
                //圖片為空就不執行
                if (myfile != null)
                {
                    //保存檔案名稱
                    product.ImageUrl = await SaveImage(myfile);
                }
                #endregion
                Product item = new Product();
                item.Name = product.Name;
                item.Price = product.Price;
                item.Stock = product.Stock;
                item.ImageUrl = product.ImageUrl;
                item.CreatDate = DateTime.Now;

                await _context.AddRangeAsync(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));//路由
            }
            return View(product);
        }
        #endregion

        #region 編輯商品
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //判斷輸入值是否為空
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }
            //查詢修改項目
            var product = await _context.Products.FindAsync(id);
            //判斷product是否為空
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Stock,ImageUrl,CreatDate,EditDate")] Product product, IFormFile myfile)
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
                #endregion
                try
                {
                    //查詢要修改的值
                    var item = await _context.Products.FindAsync(product.Id);
                    if (item == null) { return NotFound(); }//404 
                    //修改的
                    item.Name = product.Name;
                    item.Price = product.Price;
                    item.Stock = product.Stock;
                    if (myfile != null) { item.ImageUrl = product.ImageUrl; } //有檔案才修改     
                    item.EditDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        #endregion

        #region 刪除商品
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            //判斷輸入值是否為空
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }
            //查詢刪除的值
            var product = await _context.Products
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
            if (_context.Products == null)
            {
                return Problem("Entity set 'WebAPIContext.Products'  is null.");
            }
            //查詢刪除的值
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                //圖片不為空 就刪除圖片檔案
                if (product.ImageUrl != null) { RemoveImage(product.ImageUrl); }
                _context.Products.Remove(product);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

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
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        #endregion
    }
}

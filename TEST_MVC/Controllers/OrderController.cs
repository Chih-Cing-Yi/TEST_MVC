using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using TEST_MVC.Models;
using TEST_MVC.Models.ViewModels;
using X.PagedList;

namespace TEST_MVC.Controllers
{
    public class OrderController : Controller
    {
        //資料庫連線(上efcore 下dapper)
        private readonly WebAPIContext _db;

        private readonly IConfiguration _connconfig;

        public OrderController(WebAPIContext db, IConfiguration connconfig)
        {
            _db = db;
            _connconfig = connconfig;
        }

        #region 獲取列表

        public async Task<IActionResult> Index(int? Page, string id)
        {
            @ViewData["id"] = id;
            //分頁
            var Orders = await _db.OrderMs.Where(x => (string.IsNullOrEmpty(id) || x.Id.StartsWith(id))).OrderBy(x => x.Id).ToPagedListAsync(Page.HasValue ? Page.Value - 0 : 1, 5);
            //獲取全部(不分頁)
            //var Orders = await _db.OrderMs.ToListAsync();
            return View(Orders);
        }

        #endregion 獲取列表

        #region 新增訂單

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderVM orderVM)
        {
            //沒有明細
            if (orderVM.OrderD == null)
            {
                TempData["Error"] = "缺少明細檔";
                return View(orderVM);
            }
            //找到USER資料
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Convert.ToInt64(User.Identity!.Name));
            if (ModelState.IsValid)
            {
                #region 訂單編號

                string orderId = "";
                string toDay = DateTime.Now.ToString("yyyyMMdd");
                string orderDate = "";
                var findOrderId = _db.OrderMs.OrderByDescending(o => o.Id).FirstOrDefault();
                if (findOrderId == null)//資料庫查無主單
                { orderId = toDay + "0001"; }
                else
                { orderDate = findOrderId.Id.Substring(0, 8); }
                if (toDay != orderDate)//沒有同天的訂單
                { orderId = toDay + "0001"; }
                else
                {
                    orderId = findOrderId.Id.ToString();
                    int temp = Convert.ToInt16(orderId.Substring(orderId.Length - 4, 4));//取後4碼流水號
                    temp += 1;
                    string tempStr = temp.ToString().PadLeft(4, '0');//補0補滿4位
                    orderId = toDay + tempStr;
                }

                #endregion 訂單編號

                OrderM item_M = new OrderM();
                //主單
                item_M.Id = orderId;
                item_M.CustomerId = orderVM.OrderM[0].CustomerId;
                item_M.CreactDate = DateTime.Now;
                //item_M.CreactUser = user.Name;
                item_M.CreactUser = "Simon";
                item_M.TotalPrice = orderVM.OrderM[0].TotalPrice;
                await _db.AddRangeAsync(item_M);
                //明細單
                int i = 1;
                foreach (var order in orderVM.OrderD)
                {
                    OrderD item_D = new OrderD();
                    item_D.Id = orderId;
                    item_D.ItemNo = i;
                    item_D.ProductId = order.ProductId;
                    item_D.Price = order.Price;
                    item_D.QTY = order.QTY;
                    item_D.Total = order.Total;
                    i++;
                    await _db.AddRangeAsync(item_D);
                }
                await _db.SaveChangesAsync();

                TempData["Success"] = "新增成功";
                return RedirectToAction(nameof(Index));//路由
            }
            TempData["Error"] = "新增失敗";
            return View(orderVM);
        }

        #endregion 新增訂單

        #region 編輯訂單

        public async Task<IActionResult> Edit(string id)
        {
            //判斷輸入值是否為空
            if (id == null)
            { return NotFound(); }
            //查詢修改項目(查詢跨四個表格這邊用dapper比較方便)
            //查詢語句
            string sql = @"SELECT m.id,m.CustomerId,c.name As CustomerName,
                                        m.TotalPrice,m.CreactDate,m.CreactUser,
                                        m.EditDate,m.EditUser,d.ItemNo,
                                        d.ProductId,p.name As ProductName,d.Price,d.QTY,d.Total
                                        FROM OrderM m
                                        LEFT JOIN OrderD d
                                        ON m.id = d.id
                                        LEFT JOIN Customer c
                                        ON m.customerId = c.id
                                        LEFT JOIN Product p
                                        ON d.productId = p.id
                                        WHERE m.id =@id";
            using (var conn = new SqlConnection(_connconfig.GetConnectionString("2023MVC")))
            {
                var orderVM2 = await conn.QueryAsync<OrderVM2>(sql, new { @id = id });
                return View(orderVM2);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrderVM orderVM)
        {
            if (orderVM.OrderM == null || orderVM.OrderD == null)
            {
                TempData["Error"] = "缺少明細";
                return View();
            }
            //驗證綁定的值
            if (ModelState.IsValid)
            {
                try
                {
                    //查詢要修改的值(主表)
                    var item = await _db.OrderMs.FindAsync(orderVM.OrderM[0].Id);
                    if (item == null) { return NotFound(); }//404
                    //修改的
                    item.CustomerId = orderVM.OrderM[0].CustomerId;
                    item.TotalPrice = orderVM.OrderM[0].TotalPrice;
                    item.EditDate = DateTime.Now;
                    item.CreactUser = "Simon";
                    //明細單
                    //查詢要修改的值(明細表)
                    //先刪除
                    var item_d = await _db.OrderDs.Where(d => d.Id == orderVM.OrderM[0].Id).ToListAsync();
                    _db.OrderDs.RemoveRange(item_d);//刪除
                    //新增
                    int i = 1;
                    foreach (var order in orderVM.OrderD)
                    {
                        OrderD item_D2 = new OrderD();
                        item_D2.Id = item.Id;
                        item_D2.ItemNo = i;
                        item_D2.ProductId = order.ProductId;
                        item_D2.Price = order.Price;
                        item_D2.QTY = order.QTY;
                        item_D2.Total = order.Total;
                        i++;
                        await _db.AddRangeAsync(item_D2);
                    }

                    await _db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    TempData["Error"] = "編輯失敗" + e;
                    return View();
                }
                TempData["Success"] = "編輯成功";
                return RedirectToAction(nameof(Index));
            }
            TempData["Error"] = "編輯失敗";
            return View();
        }

        #endregion 編輯訂單
    }
}
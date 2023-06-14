using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TEST_MVC.Models;

namespace TEST_MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly WebAPIContext _db;

        public LoginController(WebAPIContext db)
        {
            _db = db;
        }

        #region 登入

        public IActionResult index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]//防止XSS、CSRF攻擊
        public async Task<IActionResult> index(User user)
        {
            //找到帳號密碼相同的
            var item = await _db.Users.FirstOrDefaultAsync(u => u.Name == user.Name && u.PassWord == user.PassWord);
            //沒有找到就報錯
            if (item == null)
            {
                ViewData["class"] = "alert-danger";
                ViewData["ErrowMessage"] = "登入失敗";
                return View();
            }

            //要保存的資料屬性
            var claims = new List<Claim>()
            {
                //通常保存主key就好 後續資料都抓取資料庫
                new Claim(ClaimTypes.Name,item.Id.ToString()),//保存主key
                new Claim("FullName", item.Name.ToString()),//保存名稱
                //new Claim("PassWord",item.PassWord),//通常不會保存密碼(測試用)
                //new Claim(ClaimTypes.Role,"Administrator"),//保存身分(通常也從資料庫來)
            };
            //保存cookie
            //1.claimsIdentity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //2.authProperties
            var authProperties = new AuthenticationProperties
            {
                // 這裡可以自訂驗證選項... 5個設定值
                //1.是否可自動更新 Cookie(時效?)
                //AllowRefresh = true,
                //2.IsPersistent 設置 Persistent cookies，否則瀏覽器 session 關閉就失效
                //IsPersistent = true,
                //3.Persistent cookie 可進一步設置失效時間：
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
                //4.IssuedUtc
                //IssuedUtc = <DateTimeOffset>,
                //5.RedirectUri
                //RedirectUri = <string>
            };
            //登入
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            //導向到Products目錄的index
            return RedirectToAction("Index", "Home");
        }

        #endregion 登入

        #region 登出

        public async Task<IActionResult> Logout()
        {
            //登出(刪除其 cookie )
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //導向到Login目錄的index
            return RedirectToAction("Index", "Login");
        }

        #endregion 登出

        #region cookie頁面

        //查看cookie
        public IActionResult Get()
        {
            if (User.Identity!.Name == null) { return View(); }
            //使用者名稱 (這邊保存主KEY) 通常使用這個而已
            var a1 = User.Identity!.Name;
            //認證方法
            var a2 = User.Identity!.AuthenticationType;
            //是否認證
            var a3 = User.Identity!.IsAuthenticated;
            //認證訊息
            var a4 = User.Identity!.ToString();
            //認證時效
            var a5 = "";
            var items = Request.HttpContext.AuthenticateAsync().Result;
            foreach (var p in items.Properties!.Items)
            {
                a5 += p.Key + ":";
                //轉換成時間並轉換成當地時間
                var time = Convert.ToDateTime(p.Value).ToLocalTime();
                a5 += time.ToString();
            }
            ViewData["cookie"] = $"1.使用者名稱:{a1}     " +
                $"2.認證方法:{a2}     3.是否認證:{a3}  4.認證訊息:{a4}    " +
                $"5.cookie時效:{a5}";
            return View();
        }

        #endregion cookie頁面
    }
}
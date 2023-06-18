using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;

namespace Shopping.Controllers
{
    public class LoginController : Controller
    {
        private readonly AdventureWorksLT2016Context _db;

        public LoginController(AdventureWorksLT2016Context context)
        {
            _db = context;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Customer _User)
        {
            //if (_User == null || !ModelState.IsValid) return View();
            if (_User != null)
            {
                var List = from m in _db.Customers
                           where m.FirstName == _User.FirstName && m.LastName == _User.LastName
                           select m;
                Customer _result = List.FirstOrDefault();
                if (_result == null)
                {
                    ViewData["ErrorMessage"] = "帳號或密碼有錯";
                    return View();
                }
                else
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, _User.FirstName),
                        new Claim("SelfDefine_ID", _result.CustomerId.ToString()),    // 購物車會用到 Customer ID
                        new Claim(ClaimTypes.Role, "Administrator")
                    };
                    // 底下的 ** 登入 Login ** 需要下面兩個參數 (1) claimsIdentity  (2) authProperties
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),   // 從現在算起，Cookie何時過期
                        // The time at which the authentication ticket expires. A value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        //IsPersistent = true,
                        // Whether the authentication session is persisted across multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the lifetime of the authentication ticket) or session-based.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http  redirect response value.
                    };

                    // *** 登入 Login *********
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                            new ClaimsPrincipal(claimsIdentity),
                                                            authProperties);

                    //return Content("<h3>恭喜您，登入成功</h3>");
                    // return LocalRedirect(Url.GetLocalUrl(returnUrl));  // 登入成功後，返回原本的網頁

                    return RedirectToAction("Index2", "Login");

                    // 完成這個範例以後，您可以參考這篇文章 - OWIN Forms authentication（作法很類似）
                    // https://blogs.msdn.microsoft.com/webdev/2013/07/03/understanding-owin-forms-authentication-in-mvc-5/
                }

            }
            // Something failed. Redisplay the form.
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            // 自己宣告 Microsoft.AspNetCore.Authentication.Cookies; 命名空間
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View();
            //return RedirectToPage("/Account/SignedOut");   // 登出，跳去另一頁。
        }

        public IActionResult Index()
        {
            return View();  // 任何人（匿名）都可以看見
        }
        //***********************
        [Authorize]
        public IActionResult Index2()
        {
            // 讀取 Cookie的內容
            //return Content(System.Web.HttpUtility.HtmlDecode(Request.Cookies["MIS2000Lab"].ToString()));

            ////**** 如何取出（讀取） 所有（全部）Claims的內容？********************
            //string str = "";
            //ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.User;
            //if (null != principal)   {
            //    foreach (Claim claim in principal.Claims)   {
            //        str += "CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value + "</br>";
            //    }
            //}
            //return Content(str);

            // 讀取自己定義的 Claim 內容
            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.User;
            string str = "";
            if (null != principal)
            {
                //foreach (Claim claim in principal.Claims)
                //{
                //    // 讀取自己定義的 Claim 內容
                //    //if (claim.Type == "SelfDefine_LastName")
                //    //    TempData["SelfDefine_LastName"] = claim.Value;
                    

                //}
                str = principal.FindFirstValue(ClaimTypes.Name);
                ViewData["UserName"] = str;
            }

            return View();  // 登入成功（會員）才可以看見

        }
        //***********************
        [Authorize(Roles = "Administrator")]
        public IActionResult Index3()
        {
            return View();    // 登入成功（會員）才可以看見
        }


        public IActionResult AccessDeny()
        {
            return View();    // 拒絕存取，權限不足、帳號或密碼有錯。搭配 Startup.cs的 .AddCookie()  options.AccessDeniedPath設定
        }

        //=================================================================
        //=== 後台管理區（一對多 報表的呈現）=== // 直接使用 /Models/SalesOrderHeader.cs，最簡單的作法！
        //=================================================================

        public IActionResult IndexReport(int id = 71774)
        // 給 id變數，一個預設值（SalesOrderHeader 資料表 的 訂單ID - SalesOrderID）使用檢視畫面的 List範本
        {
            var _result = from m in _db.SalesOrderHeaders
                          where m.SalesOrderId == id
                          select new SalesOrderHeader
                          {
                              SalesOrderId = m.SalesOrderId,
                              SalesOrderNumber = m.SalesOrderNumber,
                              OrderDate = m.OrderDate,
                              PurchaseOrderNumber = m.PurchaseOrderNumber,
                              CustomerId = m.CustomerId,
                              ShipMethod = m.ShipMethod,
                              SalesOrderDetails = m.SalesOrderDetails,
                              //******************** 重點！！ 一對多，一張「訂單（主檔）」底下有很多「（訂單明細）訂購的產品」
                              // /Models/SalesOrderHeader.cs 的「導覽屬性」對應 -- 訂單的明細檔（SalesOrderDetail）資料表   
                          };
            return View(_result.ToList());
            // 把結果（ "一對多"的列表）呈現出來。
            // 請使用「List」範本。使用的「模型類別」為 /Models目錄下的 SalesOrderHeader
            // 檢視畫面需要 "自己動手" 修改，現有的範本只能當作雛形，沒法 100%套用
        }

        public IActionResult Index2_Details(int id=71774)
            //給 id變數，一個預設值（SalesOrderHeader 資料表 的 訂單ID - SalesOrderID）
        // 跟上面的動作一模一樣，差異有兩點： (1) return View(result.FirstOrDefault());
        //                                 (2) 檢視畫面改用「Details範本」
        {
            var _result = from m in _db.SalesOrderHeaders
                          where m.SalesOrderId == id
                          select new SalesOrderHeader
                          {
                              SalesOrderId = m.SalesOrderId,
                              SalesOrderNumber = m.SalesOrderNumber,
                              OrderDate = m.OrderDate,
                              PurchaseOrderNumber = m.PurchaseOrderNumber,
                              CustomerId = m.CustomerId,
                              ShipMethod = m.ShipMethod,
                              SalesOrderDetails = m.SalesOrderDetails,
                          };

            return View(_result.FirstOrDefault());
        }

        public IActionResult IndexAll()
        // 列出 "全部"訂單（SalesOrderHeader 資料表）
        // 使用檢視畫面的 List範本
        {
            var _result = from m in _db.SalesOrderHeaders
                          select new SalesOrderHeader
                          {
                              SalesOrderId = m.SalesOrderId,
                              SalesOrderNumber = m.SalesOrderNumber,
                              OrderDate = m.OrderDate,
                              PurchaseOrderNumber = m.PurchaseOrderNumber,
                              CustomerId = m.CustomerId,
                              ShipMethod = m.ShipMethod,
                              SalesOrderDetails = m.SalesOrderDetails,
                          };
            //******************** 重點！！ 一對多，一張「訂單（主檔）」底下有很多「（訂單明細）訂購的產品」
            // /Models/SalesOrderHeader.cs 的「導覽屬性」對應 -- 訂單的明細檔（SalesOrderDetail）資料表  
            return View(_result.ToList());  
        }

        public IActionResult Cart()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cart(SalesOrderDetail _CartID)
        {
            // (1) 讀取這個會員的ID，才能把他的個人資料，寫入訂單主檔（Sales）
            string _ID = "";
            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.User;
            if(principal != null)
            {
                foreach(Claim claim in principal.Claims)
                {
                    if(claim.Type == "SelfDefine_ID")
                    {
                        _ID = claim.Value;
                    }
                }
            }
            else
            {
                return Content("找不到這位客戶");
            }
            // (2) 連結DB的 Customer資料表。如果您需要查詢這位會員（登入者）更多的資訊。
            var ListOne = from m in _db.Customers
                                where m.CustomerId == Convert.ToInt32(_ID)
                                select m;

            Customer _result = ListOne.FirstOrDefault();
            // 執行上面的查詢句，得到 "第一筆" 結果。
            //// 測試查詢成果  

            SalesOrderHeader OrderMaster = new SalesOrderHeader
            {
                OrderDate = DateTime.Now,
                DueDate = DateTime.Now,
                ShipDate = DateTime.Now,
                OnlineOrderFlag = true,
                //*************************************************************************
                CustomerId = Convert.ToInt32(_ID),     //**************** 只有這個是關鍵
                //*************************************************************************
                ShipMethod = "CARGO TRANSPORT 5"
                //SubTotal = 0,  // 把SQL Server資料表的 money 格式轉成 C#的decimal。  Convert.ToDecimal(2980.7929)
                //TaxAmt = 0,
                //Freight = 0,
                //TotalDue = 0
            };

            // 產生訂單的 "明細檔（Details）"。一張訂單裡面，你採購了幾樣商品？
            // 必須套用 /Models/SalesOrderHeader.cs 裡面的導覽屬性，所以下面的寫法請留意！
            //************      在訂單中，加入兩項產品
            OrderMaster.SalesOrderDetails.Add(new SalesOrderDetail
            {
                OrderQty = 5,
                ProductId = 680
            });
            OrderMaster.SalesOrderDetails.Add(new SalesOrderDetail
            {
                OrderQty = 10,
                ProductId = 836
            });

            //***********************************************************************
            _db.SalesOrderHeaders.Add(OrderMaster); //*** 只要針對主檔「新增」，明細檔的兩筆產品就一起新增完畢了。很方便！！
            _db.SaveChanges();
            //***********************************************************************
            return Content("一對多訂單，新增成功！請用這段SQL指令去資料庫查詢一下！   SELECT * FROM SalesLT.SalesOrderHeader AS H INNER JOIN SalesLT.SalesOrderDetail AS D ON H.SalesOrderID = D.SalesOrderID Order by H.SalesOrderID desc");
        }
        //=================================================================
        //=== 購物車（Cookie版） ===
        //
        // 最終我還是採用一個資料表來紀錄購物車的內容。
        //     (1)  請自己在 /Models目錄下，新增一個 TestCart.cs類別檔。
        //     (2)  資料庫，也需要手動新增一個對應的 TestCart資料表。
        //=================================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]   // 避免XSS、CSRF攻擊
        public IActionResult CartCookie(IFormCollection _form)
        {
            // (1) 讀取這個會員的ID，才能把他的個人資料，寫入訂單主檔（Sales）
            string _ID = "";
            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.User;
            if (principal != null)
            {
                foreach(Claim claim in principal.Claims) 
                {
                    if(claim.Type == "SelfDefine_ID")
                        _ID = claim.Value;
                }
            }
            else
            {
                return Content("抱歉！找不到這位用戶的ID");
            }

            // (2) 把購買的產品ID（ProductID）與 數量 與 會員ID（CustomerID）存入Cookie
            // *****注意！建議您在加入購物車（TestCart）之前，先檢查這名會員的購物車裡面，是否已經有相同商品？是否重複購買？？
            //          或是把購物車中，舊的那一筆刪除，改放「新加入」的這一筆。
            //var ListAll = from m in _db.TestCarts
            //              where (m.CID == _ID)&&(m.PID == _form["PID"].ToString())
            //              select m;
            //if (ListAll != null)
            //{
            //    return Content("抱歉！在購物車中，您已經採購過相同產品.....");
            //}
            TestCart _cart = new TestCart
            {
                PID = _form["PID"].ToString(),
                QTY = Convert.ToInt32(_form["OrderQty"]),
                CID = _ID

            };
            _db.TestCart.Add(_cart);
            _db.SaveChanges();
            string result = _cart.PID + "***" + _cart.QTY + "***" + _cart.CID;
            return Content(result);
        }
        //=================================================================
        //=== 購物車（結帳、付款） ===
        //=================================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]   // 避免XSS、CSRF攻擊
        public IActionResult CartBuy(IFormCollection _form)
        {
            // (1) 讀取這個會員的ID，才能把他的個人資料，寫入訂單主檔（Sales）
            string _ID = "";
            ClaimsPrincipal principal = (ClaimsPrincipal)HttpContext.User;
            if (principal != null)
            {
                foreach(Claim claim in principal.Claims)
                {
                    if(claim.Type == "SelfDefine_ID")
                    {
                        _ID = claim.Value;
                    }
                }
            }
            else
            {
                return Content("抱歉！找不到這位用戶的ID");
            }
            // (2) 連結DB的 TestCart資料表。把這一名會員以前留在購物車的紀錄，一次結帳。
            var ListAll = from m in _db.TestCart
                          where m.CID == _ID
                          select m;

            // (3) 產生訂單的 "主檔（Master）"。誰訂購、誰下單？何時買？貨要送去哪裡？Total價格多少？
            SalesOrderHeader OrderMaster = new SalesOrderHeader
            {
                // 很多必填欄位，不得為空（not null）。所以只好手動填寫。
                OrderDate = System.DateTime.Now,
                DueDate = System.DateTime.Now,
                ShipDate = System.DateTime.Now,
                OnlineOrderFlag = true,
                //*************************************************************************
                CustomerId = Convert.ToInt32(_ID),    //**************** 只有這個是關鍵
                //*************************************************************************
                ShipMethod = "CARGO TRANSPORT 5"  // ,
                //SubTotal = 0,  // 把SQL Server資料表的 money 格式轉成 C#的decimal。  Convert.ToDecimal(2980.7929)
                //TaxAmt = 0,
                //Freight = 0,
                //TotalDue = 0
            };

            // (4) 產生訂單的 "明細檔（Details）"。一張訂單裡面，你採購了幾樣商品？
            // 必須套用 /Models/SalesOrderHeader.cs 裡面的導覽屬性，所以下面的寫法請留意！
            //********************************************************************************************
            //依照這名會員在購物車（TestCart）裡面的歷史資料，轉入正式的訂單（SalesOrderHeader）。
            foreach (var _cart in ListAll)
            {
                OrderMaster.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    OrderQty = (short)_cart.QTY,
                    ProductId = Convert.ToInt32(_cart.PID)
                });
            }
            //********************************************************************************************
            // (5) 只要針對主檔「新增」，明細檔的多筆產品就一起新增完畢了。很方便！！
            _db.SalesOrderHeaders.Add(OrderMaster);
            // (6) 一旦您把購物車的商品全部結帳，那就要把購物車裡面的紀錄清空！以免下次重複購買。
            // 必須先鎖定、先找到這筆記錄。找得到，才能刪除！
            //_db.TestCart.Where(x => x.CID==_ID).ToList().ForEach(_db.TestCart.delete.DeleteObject);
            var origins = (from m in _db.TestCart
                          where m.CID == _ID
                          select m).ToList();
            _db.TestCart.RemoveRange(origins);  // 批次  刪除多筆記錄。
            _db.SaveChanges();
            return Content("購買成功！請用這段SQL指令去資料庫查詢一下！   SELECT * FROM SalesLT.SalesOrderHeader AS H INNER JOIN SalesLT.SalesOrderDetail AS D ON H.SalesOrderID = D.SalesOrderID Order by H.SalesOrderID desc");

        }
    }
}

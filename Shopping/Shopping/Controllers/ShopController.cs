using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shopping.Models;

namespace Shopping.Controllers
{
    public class ShopController : Controller
    {
        private readonly AdventureWorksLT2016Context _db;

        public ShopController(AdventureWorksLT2016Context context)
        {
            _db = context;
        }

        public IActionResult List()
        {
            //第一種寫法
            //IQueryable<ProductCategory> _productCategory = from _productCategory in _db.ProductCategories
            //                                               select _productCategory;

            //// (1) http://blog.darkthread.net/post-2012-10-23-iqueryable-experiment.aspx
            ////......發現 IQueryable<T> 是在 Server 端作過濾, 再將結果傳回 Client 端, 故若為「資料庫」存取, 應採用 IQueryable<T>
            //// (2) http://jasper-it.blogspot.tw/2015/01/c-ienumerable-ienumerator.html
            ////......在「資料庫」相關的環境下, 用 IQueryable<T> 的效能會比 IEnumerable< T > 來得好.
            ///


            //第二種寫法
            //if(_db.ProductCategories == null)
            //{
            //    return NotFound();
            //}
            //else
            //{
            //    return View(_db.ProductCategories);
            //}

            var List = from m in _db.ProductCategories
                       select m;

            if (List.Any() == false)
            {
                return NotFound();
            }
            else
            {
                return View(List.ToList());
            }
        }

        public IActionResult Details(int? id = 680)
        {
            if(id == null||id.HasValue == false)
            {
                return Content("Error");
            }
            //第一種寫法
            //Product product = _db.Products.Find(id);
            //if (product == null)
            //{
            //    return NotFound();
            //}
            //else
            //{
            //    return View(product);
            //}


            //第二種寫法
            //var Product = from p in _db.Products
            //              where p.ProductId == id
            //              select p;
            //var _result = Product.FirstOrDefault();
            //if (_result == null)
            //{
            //    return NotFound();
            //}
            //else
            //{
            //    return View(_result);
            //}

            //第三種寫法
            //var Product = _db.Products.Where(x => x.ProductId == id);
            //var _result = Product.FirstOrDefault();
            //if (_result == null)
            //{
            //    return Content("Error");
            //}
            //else
            //{
            //    return View(_result);
            //}

            //第四種寫法
            var Product = _db.Products.FirstOrDefault(x => x.ProductId == id);
            if (Product == null)
            {
                return NotFound();
            }
            else
            {
                return View(Product);
            }

            
        }
        //回家作業List Deatails
        public IActionResult ListProductCategoriey(int? id = 21)
        {
            if(id == null||id.HasValue==false) { return Content("Error"); }
            IQueryable<Product> List = from m in _db.Products
                                       where m.ProductCategoryId == id
                                       select m;
            
            if (List == null)
            {
                return NotFound();
            }
            else { return View(List); }
        }

        public IActionResult DetailsPicture(int? id = 713)
        {
            if(id==null||id.HasValue==false)
            {
                return Content("Error");
            }
            var Product = _db.Products.FirstOrDefault(y => y.ProductId == id);
            if (Product == null)
            {
                return NotFound();
            }
            else
            {
                return View(Product);
            }
        }

        public FileContentResult GetImage(int id)
        {
            //第一種寫法
            //IQueryable<Product> requestedPhoto = from _requestedPhoto in _db.Products
            //                                     where _requestedPhoto.ProductId == id   
            //                                     select _requestedPhoto;
            //var _result = requestedPhoto.FirstOrDefault();
            //if (_result != null)
            //{
            //    return File(_result.ThumbNailPhoto, "image/jpeg");// 第二個參數是  MIME Type，固定寫成 image/jpeg 也可運作。
            //}
            //else
            //{
            //    return null;
            //}


            //第二種寫法
            var requestedPhoto = _db.Products.FirstOrDefault(x => x.ProductId == id);
            if (requestedPhoto == null) 
            {
                return null;
            }
            else
            {
                //// requestedPhoto.ThumbnailPhotoFileName 是完整的圖片檔名，我們只要取得最後的「副檔名」即可
                //// 想抓到「主檔名」，請寫成 System.IO.Path.GetFileName(fileName);
                //// 想抓到「.副檔名」，請寫成 System.IO.Path.GetExtension(fileName);
                ///
                string extFile = System.IO.Path.GetExtension(requestedPhoto.ThumbnailPhotoFileName);
                extFile = extFile.Replace(".", "");
                return File(requestedPhoto.ThumbNailPhoto, "image/"+ extFile);
            }
        }
        //========================================
        //== 分頁 ==  LINQ的 .Skip() 與 .Take()
        // https://docs.microsoft.com/zh-tw/dotnet/framework/data/adonet/ef/language-reference/method-based-query-syntax-examples-partitioning
        //========================================
        //== 畫面下方，加入「上一頁」、「下一頁」、每十頁作間隔 ===
        public IActionResult ListPage(int id=1)
        {
            // id變數，目前位於第幾頁？預設為第一頁
            int PageSize = 10; // PageSize變數，每一頁，要展示幾筆記錄？
            int RecordCount = _db.Products.Count();  // RecordCount變數，符合條件的  "總共" 有幾筆記錄？
            int NowPageCount = 0; // NowPageCount，目前正在觀賞這一頁的紀錄

            if (id > 0)
            {
                NowPageCount = (id - 1) * PageSize; // PageSize，每頁展示10筆紀錄（上面設定過了）
            }

            // 這段指令的 .Skip()與 . Take()，其實跟T-SQL指令的 offset...fetch....很類似（SQL 2012起可用）
            var List = (from m in _db.Products
                        orderby m.ProductId  // 若寫 descending ，則是反排序（由大到小）
                        select m).Skip(NowPageCount).Take(PageSize); // .Skip() 從哪裡開始（忽略前面幾筆記錄）。 .Take()呈現幾筆記錄

            if(List == null)
            {
                return NotFound();
            }
            else 
            {
                #region    // 畫面下方的「分頁列」。「每十頁」一間隔，分頁功能

                // Pages變數，「總共需要幾頁」才能把所有紀錄展示完畢？
                int Pages;
                if ((RecordCount % PageSize) > 0)
                {   //-- %，除法，傳回餘數
                    Pages = ((RecordCount / PageSize) + 1);   //-- ( / )除法。傳回整數。  如果無法整除，有餘數，則需要多出一頁來呈現。 
                }
                else
                {
                    Pages = (RecordCount / PageSize);   //-- ( /)除法。傳回整數
                }



                System.Text.StringBuilder sbPageList = new System.Text.StringBuilder();
                if (Pages > 0)
                {   //有傳來「頁數(p)」，而且頁數正確（大於零），出現<上一頁>、<下一頁>這些功能
                    sbPageList.Append("<div align='center'>");

                    //** 可以把檔名刪除，只留下 ?P=  即可！一樣會運作，但IE 11會出現 JavaScript錯誤。**
                    //** 抓到目前網頁的「檔名」。 System.IO.Path.GetFileName(Request.PhysicalPath) **
                    if (id > 1)
                    {   //======== 分頁功能（上一頁 / 下一頁）=========start===                
                        sbPageList.Append("<a href='?id=" + (id - 1) + "'>[<<<上一頁]</a>");
                    }
                    sbPageList.Append("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b><a href='http://127.0.0.1/'>[首頁]</a></b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                    if (id < Pages)
                    {
                        sbPageList.Append("<a href='?id=" + (id + 1) + "'>[下一頁>>>]</a>");
                    }  //======== 分頁功能（上一頁 / 下一頁）=========end====


                    //.................................................................................................................................................................................................
                    //========= MIS2000 Lab.自製的「每十頁」一間隔，分頁功能=========start====
                    sbPageList.Append("<hr width='97%' size=1>");

                    int block_page = 0;
                    block_page = id / 10;   //--只取除法的整數成果（商），若有餘數也不去管它。

                    if (block_page > 0)
                    {
                        sbPageList.Append("<a href='?id=" + (((block_page - 1) * 10) + 9) + "'> [前十頁<<]  </a>&nbsp;&nbsp;");
                    }

                    for (int K = 0; K <= 10; K++)
                    {
                        if ((block_page * 10 + K) <= Pages)
                        {   //--- Pages 資料的總頁數。共需「幾頁」來呈現所有資料？
                            if (((block_page * 10) + K) == id)
                            {   //--- id 就是「目前在第幾頁」
                                sbPageList.Append("[<b>" + id + "</b>]" + "&nbsp;&nbsp;&nbsp;");
                            }
                            else
                            {
                                if (((block_page * 10) + K) != 0)
                                {
                                    sbPageList.Append("<a href='?id=" + (block_page * 10 + K) + "'>" + (block_page * 10 + K) + "</a>");
                                    sbPageList.Append("&nbsp;&nbsp;&nbsp;");
                                }
                            }
                        }
                    }  //for迴圈 end

                    if ((block_page < (Pages / 10)) & (Pages >= (((block_page + 1) * 10) + 1)))
                    {
                        sbPageList.Append("&nbsp;&nbsp;<a href='?id=" + ((block_page + 1) * 10 + 1) + "'>  [>>後十頁]  </a>");
                    }
                    sbPageList.Append("</div>");
                }
                //========= MIS2000 Lab.自製的「每十頁」一間隔，分頁功能=========end====
                #endregion

                ViewBag.PageList = sbPageList.ToString();
                //************** 比上一個範例  多的程式碼。 *****************************************(end)

                return View(List);
            }

        }




    }
}

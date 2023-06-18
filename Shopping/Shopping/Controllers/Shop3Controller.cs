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
    public class Shop3Controller : Controller
    {
        private readonly AdventureWorksLT2016Context _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Shop3Controller(AdventureWorksLT2016Context context, IWebHostEnvironment webHostEnvironment)
        {
            _db = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product _pTable)
        {
            if(_pTable == null && !ModelState.IsValid)
            {
                //   // 搭配 ModelState.IsValid，如果驗證沒過，就出現錯誤訊息。
                //    ModelState.AddModelError("Value1", " 自訂錯誤訊息(1) ");   // 第一個輸入值是 key，第二個是錯誤訊息（字串）
                //    ModelState.AddModelError("Value2", " 自訂錯誤訊息(2) ");
                return View();   // 將錯誤訊息，返回並呈現在「新增」的檢視畫面上
            }
            else
            {
                //第一種寫法
                //_db.Products.Add(_pTable);
                //_db.SaveChanges();  
                //第二種寫法
                _db.Entry(_pTable).State = EntityState.Added;
                _db.SaveChanges();
                return Content(" 新增一筆記錄，成功！");    // 新增成功後，出現訊息（字串）。
                //return RedirectToAction("List");

            }
        }

        public IActionResult CreatePicture()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ******  超級重點！！  檔案上傳的參數名稱「ThumbNailPhoto」
        //              務必跟畫面上<input type="file" name="ThumbNailPhoto" />的名字一模一樣！******
        public IActionResult CreatePicture(Product _pTable,IFormFile ThumbNailPhoto)
        {
            // 檔案上傳 -- IFormFile 需要搭配 Microsoft.AspNetCore.Http命名空間。

            //if ((_pTable != null) && (ModelState.IsValid))   // ModelState.IsValid，通過表單驗證（Server-side validation）需搭配 Model底下類別檔的 [驗證]
            //{

            // 上傳至實體儲存體  https://docs.microsoft.com/zh-tw/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1#upload-small-files-with-buffered-model-binding-to-a-database
            //  https://stackoverflow.com/questions/39322085/how-to-save-iformfile-to-disk/39322161
            //  https://gunnarpeipman.com/aspnet-core-file-uploads/

            // 上傳至資料庫  https://docs.microsoft.com/zh-tw/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1#upload-small-files-with-buffered-model-binding-to-physical-storage
            //  https://stackoverflow.com/questions/38251335/how-to-save-iformfile-to-sqlserver-filestream-table
            //  範例 http://www.binaryintellect.net/articles/2f55345c-1fcb-4262-89f4-c4319f95c5bd.aspx

            if(ThumbNailPhoto.Length > 0)// 檢查 < input type = "file" > 是否輸入檔案？
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    //// 方法一：
                    //ThumbNailPhoto.CopyTo(ms);
                    ////if (ms.Length < 2097152)   {
                    ////    // 限制上傳檔案必須小於2MB。Upload the file if less than 2 MB         
                    //     _pTable.ThumbNailPhoto = ms.ToArray();
                    ////}
                    ////else   {
                    ////    ModelState.AddModelError("File", "The file is too large. 限制上傳檔案必須小於2MB。");
                    ////}

                    //// 方法二：
                    //// https://forums.asp.net/t/2090370.aspx?Inputstream+and+contentlength+is+missing+in+microsoft+aspnet+http+abstractions
                    //ThumbNailPhoto.CopyTo(ms);
                    //_pTable.ThumbNailPhoto = ms.ToArray();

                    //// 方法三：
                    //ThumbNailPhoto.OpenReadStream().CopyTo(ms);
                    //_pTable.ThumbNailPhoto = ms.ToArray();

                    //// 方法四：模仿以前 MVC 5 的寫法 
                    _pTable.ThumbNailPhoto = new byte[ThumbNailPhoto.Length];
                    ThumbNailPhoto.OpenReadStream().Read(_pTable.ThumbNailPhoto, 0, (int)ThumbNailPhoto.Length);
                    //  以前 MVC 5 的寫法 --
                    //_pTable.ThumbNailPhoto = new byte[FileUpload_FileName.ContentLength];
                    //FileUpload_FileName.InputStream.Read(_pTable.ThumbNailPhoto, 0, FileUpload_FileName.ContentLength);
                    // https://forums.asp.net/t/2090370.aspx?Inputstream+and+contentlength+is+missing+in+microsoft+aspnet+http+abstractions
                    //********************************************************
                }
                
            }
            _db.Products.Add(_pTable);
            _db.SaveChanges();

            //// 第二種方法（作法類似後續的 Edit動作）
            //// 資料來源  https://msdn.microsoft.com/en-us/library/jj592676(v=vs.113).aspx
            //_db.Entry(_pTable).State = System.Data.Entity.EntityState.Added;  // 確認新增一筆（狀態：Added）
            //_db.SaveChanges();

            return Content(" 新增一筆記錄，成功！");    // 新增成功後，出現訊息（字串）。
                                              //return RedirectToAction("List");

            //}
            //else
            //{
            //    return View();   // 將錯誤訊息，返回並呈現在「新增」的檢視畫面上
            //}
        }


        //將多個檔案傳到伺服器
        public IActionResult UploadFile() 
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UploadFile(Product _pTable,IFormFile File)
        {
            //檔案儲存路徑
            string strFilePath = _webHostEnvironment.ContentRootPath + @"\wwwroot\UploadFiles\";


            if (File.Length > 0)
            {
                var varFileName = File.FileName;
                
                _pTable.ThumbnailPhotoFileName = varFileName;

                using (var varStream = System.IO.File.Create(strFilePath + varFileName))
                {
                    File.CopyTo(varStream);
                }
            }
            
            _db.Products.Add(_pTable);
            _db.SaveChanges();
            return Ok("上傳成功");
        }

        //===============================================
        //== 搜尋關鍵字。  畫面上有「多個」搜尋條件。
        //== 中文範例   https://www.blueshop.com.tw/board/show.asp?subcde=BRD2012090415385840A&fumcde=FUM20050124192253INM&page=2
        //===============================================

        public IActionResult Search()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(Product _pTable)
        {
            var ListAll = _db.Products.Select(x => x);
            string pName = _pTable.Name;
            string pProductNumber = _pTable.ProductNumber;

            if (!string.IsNullOrWhiteSpace(pName))
            {
                ListAll = ListAll.Where(x=>x.Name.Contains(pName));
            }
            if (!string.IsNullOrWhiteSpace(pProductNumber))
            {
                ListAll = ListAll.Where(x => x.ProductNumber.Contains(pProductNumber));
            }
            if(_pTable != null && ModelState.IsValid) 
            {
                return View("SearchList",ListAll);
            }
            else
            {   // 找不到任何記錄
                return Content("找不到任何記錄");
            }
        }

    }
}

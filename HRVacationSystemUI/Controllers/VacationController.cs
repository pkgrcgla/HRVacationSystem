using HRVacationSystemBL;
using HRVacationSystemDL;
using HRVacationSystemUI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebGrease;

namespace HRVacationSystemUI.Controllers
{
    [Authorize]
    public class VacationController : Controller
    {

       private readonly IVacationManager _vacationManager ;
       private readonly IPersonelManager _personelManager ;
       private readonly IPersonelRoleManager _personelRoleManager ;
       private readonly IPersonelVacationManager _personelVacationManager ;
       private readonly ILogManager _logManager ;

        public VacationController(IVacationManager vacationManager, IPersonelManager personelManager, IPersonelRoleManager personelRoleManager, IPersonelVacationManager personelVacationManager, ILogManager logManager)
        {
            _vacationManager = vacationManager;
            _personelManager = personelManager;
            _personelRoleManager = personelRoleManager;
            _personelVacationManager = personelVacationManager;
            _logManager = logManager;
        }





        //[Authorize]
        [HttpGet]
        public ActionResult Index()
        
        {
            try
            {
                //Giiriş yapan kişi Kim? Sadece kendisine bağlı olan personellerin izinlerini listelsin
                var email = HttpContext.User.Identity.Name;
                var loggedinpersonel = _personelManager.GetPersonelProile(email);

                var list = _personelRoleManager.GetPersonelsofSenior(loggedinpersonel.Id);


                var model = _vacationManager.GetAllVacations(list,DateTime.Now.AddMonths(-2));
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Beklenmedik bir hata oluştu! {ex.Message}");
                return View(new List<PersonelVacation>());
            }
        }

        //[Authorize]
        public ActionResult Approve(int id, bool isApproved)
        {
            try
            {
                if (id <= 0)
                {
                    TempData["ApproveErrorMsg"] = $"id değeri sıfırdan büyük olmalı!";
                    return RedirectToAction("Index");
                }
                var sonuc = _vacationManager.ApproveorDenyVacation(id, isApproved);
                if (sonuc && isApproved)
                {
                    TempData["ApproveSuccessMsg"] = $"İzin Yönetici tarafından  Onaylandı!";
                    return RedirectToAction("Index");
                }
                else if (sonuc && !isApproved)
                {
                    TempData["ApproveSuccessMsg"] = $"İzin Yönetici tarafından REddedildi!";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ApproveErrorMsg"] = $"İşlem hatası oluştu!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ApproveErrorMsg"] = $"Beklenmedik bir hata oluştu! {ex.Message}";
                // ex loglamalıyız
                return RedirectToAction("Index");
            }
        }

        [AllowAnonymous]
        public ActionResult Deneme()
        {
            return View();
        }

        [Authorize]
        public ActionResult CreateVacationRequest()
        {

            List<SelectListItem> vacationTypes = new List<SelectListItem>();
             var result=_vacationManager.GetAllVacationTypes();

            foreach (var item in result)
            {
                vacationTypes.Add(new SelectListItem()
                {
                    Text=item.Name,
                    Value=item.Id.ToString()
                });
            }

            ViewBag.VacationTypes = vacationTypes;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult CreateVacationRequest(PersonelVacation model)
        {
            try
            {
             
                if (!ModelState.IsValid)
                {
                    List<SelectListItem> vacationTypes = new List<SelectListItem>();
                    var result = _vacationManager.GetAllVacationTypes();

                    foreach (var item in result)
                    {
                        vacationTypes.Add(new SelectListItem()
                        {
                            Text = item.Name,
                            Value = item.Id.ToString()
                        });
                    }

                    ViewBag.VacationTypes = vacationTypes;
                    ModelState.AddModelError("", "Bilgileri eksiksiz giriniz!");
                    return View(model);
                }

                //Giriş yapan kişi Kim?
                var email = HttpContext.User.Identity.Name;
                var loggedinpersonel = _personelManager.GetPersonelProile(email);
                model.PersonelId = loggedinpersonel.Id;
                model.CreatedDate = DateTime.Now;

                using (WebClient client = new WebClient())
                {
                    //POST İŞLEMİ
                    string url = $"http://localhost:55778/hg/izin";
                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                    client.Encoding = Encoding.UTF8;
                    var dataString = JsonConvert.SerializeObject(model);
                    string result = client.UploadString(url, "POST", dataString);

                    #region POST NETİCESİNDE ÇIKTI ALINIYOR
                    var response = JsonConvert.DeserializeObject<APIResponse<string>>(result);
                    if (response.IsSuccess)
                        TempData["CreateVacationSuccessMsg"] =response.Message;
                    else
                        TempData["CreateVacationErrorMsg"] = response.Message;

                    #endregion
                }
                return RedirectToAction("Index", "Vacation");
            }
            catch (Exception ex)
            {
                TempData["CreateVacationErrorMsg"] = $"Beklenmedik bir hata oluştu!";
                _logManager.LogMessage($"{DateTime.Now.ToString()} - Vacation/CreateVacationRequest Error - {ex.ToString()}");

                return RedirectToAction("Index");
            }
        }

    }
}
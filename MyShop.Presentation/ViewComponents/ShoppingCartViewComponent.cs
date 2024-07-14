using Microsoft.AspNetCore.Mvc;
using MyShop.BusinessLogic.Repositories;
using MyShop.BusinessLogic.ViewModels;
using MyShop.Presentation.Utilities;
using System.Security.Claims;

namespace MyShop.Presentation.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUniteOfWork _uniteOfWork;
        public ShoppingCartViewComponent(IUniteOfWork uniteOfWork)
        {
            _uniteOfWork = uniteOfWork;

        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                if (HttpContext.Session.GetInt32(SD.SessionKey) != null)
                {
                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
                else
                {
                    HttpContext.Session.SetInt32(SD.SessionKey,
                          _uniteOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value).ToList().Count()
                    );

                    return View(HttpContext.Session.GetInt32(SD.SessionKey));
                }
            }

            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}

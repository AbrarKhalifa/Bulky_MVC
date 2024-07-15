using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork) { 
        
            _unitOfWork = unitOfWork;

        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties:"Product"),
                OrderHeader = new()
            };

            HttpContext.Session.SetInt32(SD.SessionCart,
                   _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId).Count());


            foreach (var cart in shoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }
           
            return View(shoppingCartVM);
        }

        [HttpGet]
        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                shoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(x=>x.Id == userId);

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in shoppingCartVM.shoppingCartList)
            {
                cart.Price = GetPriceBasedQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

            }

            return View(shoppingCartVM);
        }


		[HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.shoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Product");

            shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;

			ApplicationUser  applicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);

			

			foreach (var cart in shoppingCartVM.shoppingCartList)
			{
				cart.Price = GetPriceBasedQuantity(cart);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

			}

            // checking is user is company user or not
            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                // we have to capture payment because this is a regular customer not a company user
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            }
            else
            {
                // it is for company user
				shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;

			}

            _unitOfWork.OrderHeader.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var cart in shoppingCartVM.shoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count

                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                // we have to capture payment because this is a regular customer not a company user
                // stripe logic
                  var domain = "https://localhost:7163/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {

                                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                                CancelUrl = domain + "customer/cart/index",
					            LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
					            Mode = "payment",
				            };

                foreach (var item in shoppingCartVM.shoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                                UnitAmount = (long) (item.Price * 100), // $20.50 => 2050
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title
                                }
                        },

                        Quantity = item.Count
                    };
                    
                    options.LineItems.Add(sessionLineItem);

                }
                var service = new SessionService();
				Session session = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation),new {id = shoppingCartVM.OrderHeader.Id}); // redirect to the comfirmation page
		}


        public IActionResult OrderConfirmation(int id)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                // this is an order by customer

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid")
                {

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id,session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id,SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();

                }
                HttpContext.Session.Clear();

            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u=>u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }
         
		// calculating price based on quantity
		public double GetPriceBasedQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }


        // Plus Minus and remove button of index page

        public IActionResult Plus(int cartid)
        {

            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartid);
            cartFromDb.Count += 1;

            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartid)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartid,tracked:true);

            if(cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart,
                  _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            }
            else
            {
            cartFromDb.Count -= 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);

            }
            _unitOfWork.Save();

            return RedirectToAction("Index"); 
        }


        public IActionResult Remove(int cartid)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartid,tracked:true);
             _unitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart,
                   _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);

              _unitOfWork.Save();

            return RedirectToAction("Index");
        }


    }
}


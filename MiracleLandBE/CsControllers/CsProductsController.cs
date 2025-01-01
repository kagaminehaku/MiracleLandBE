using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiracleLandBE;
using MiracleLandBE.Models;
using MiracleLandBE.MinimalModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using MiracleLandBE.LogicalServices;
using Azure;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Web;

namespace MiracleLandBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CsProductsController : ControllerBase
    {
        private readonly TsmgbeContext _context;
        private readonly string _jwtKey;

        public CsProductsController(TsmgbeContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtKey = configuration["Jwt:Key"];
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CsProducts>>> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new CsProducts
                {
                    Pid = p.Pid,
                    Pname = p.Pname,
                    Pprice = p.Pprice,
                    Pquantity = p.Pquantity,
                    Pinfo = p.Pinfo,
                    Pimg = p.Pimg,
                })
                .ToListAsync();

            return Ok(products);
        }

        // GET: api/CsProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CsProducts>> GetProduct(Guid id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var reproduct = new CsProducts();
            reproduct.Pid = product.Pid;
            reproduct.Pname = product.Pname;
            reproduct.Pprice = product.Pprice;
            reproduct.Pquantity = product.Pquantity;
            reproduct.Pinfo = product.Pinfo;
            reproduct.Pimg = product.Pimg;

            return Ok(reproduct);
        }

        [HttpPost("UpdateShoppingCart")]
        public async Task<ActionResult> UpdateShoppingCart([FromBody] CsProductsToCart productToCart)
        {
            if (string.IsNullOrEmpty(productToCart.token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(productToCart.token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidString = principal.Identity?.Name;

                if (!Guid.TryParse(uidString, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                var existingCartItem = await _context.ShoppingCarts.FirstOrDefaultAsync(cart => cart.Uid == uid && cart.Pid == productToCart.Pid);

                var product = await _context.Products.FindAsync(productToCart.Pid);

                if (product == null)
                {
                    return NotFound("Product not found.");
                }

                if (existingCartItem == null)
                {
                    if (productToCart.Pquantity > 0)
                    {
                        if (product.Pquantity < productToCart.Pquantity)
                        {
                            return BadRequest("Not enough product in stock.");
                        }

                        var newCartItem = new ShoppingCart
                        {
                            Cartitemid = Guid.NewGuid(),
                            Uid = uid,
                            Pid = productToCart.Pid,
                            Pquantity = productToCart.Pquantity
                        };

                        product.Pquantity -= productToCart.Pquantity;

                        _context.ShoppingCarts.Add(newCartItem);
                    }
                }
                else
                {
                    if (productToCart.Pquantity == 0)
                    {
                        product.Pquantity += existingCartItem.Pquantity;
                        _context.ShoppingCarts.Remove(existingCartItem);
                    }
                    else
                    {
                        if (product.Pquantity + existingCartItem.Pquantity < productToCart.Pquantity)
                        {
                            return BadRequest("Not enough product in stock.");
                        }

                        var quantityDifference = productToCart.Pquantity - existingCartItem.Pquantity;
                        product.Pquantity -= quantityDifference;

                        if (existingCartItem.Pquantity + quantityDifference <= 0)
                        {
                            _context.ShoppingCarts.Remove(existingCartItem);
                        }
                        else
                        {
                            existingCartItem.Pquantity += quantityDifference;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Ok("Shopping cart updated successfully.");
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }

        [HttpPatch("UpdatePaymentStatus")]
        public async Task<IActionResult> UpdatePaymentStatus([FromBody] PaymentUpdate paymentUpdate)
        {
            if (paymentUpdate == null || paymentUpdate.OrderNeedPay == Guid.Empty)
            {
                return BadRequest("Invalid input data.");
            }

            var order = await _context.CsOrders.FindAsync(paymentUpdate.OrderNeedPay);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            order.IsPayment = true;

            try
            {
                await _context.SaveChangesAsync();
                return Ok("Payment status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetShoppingCartItems")]
        public async Task<ActionResult<IEnumerable<CsProducts>>> GetShoppingCartItems([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var uidString = principal.Identity?.Name;

                if (!Guid.TryParse(uidString, out var uid))
                {
                    return Unauthorized("Invalid token.");
                }

                var cartItems = await _context.ShoppingCarts
                    .Where(cart => cart.Uid == uid)
                    .Join(_context.Products,
                        cart => cart.Pid,
                        product => product.Pid,
                        (cart, product) => new CsProducts
                        {
                            Pid = product.Pid,
                            Pname = product.Pname,
                            Pprice = product.Pprice,
                            Pquantity = cart.Pquantity,
                            Pinfo = product.Pinfo,
                            Pimg = product.Pimg
                        })
                    .ToListAsync();

                return Ok(cartItems);
            }
            catch (SecurityTokenExpiredException)
            {
                return Unauthorized("Token has expired.");
            }
            catch (Exception ex)
            {
                return Unauthorized($"Token validation failed: {ex.Message}");
            }
        }

        [HttpGet("get-public-ip")]
        public async Task<IActionResult> GetPublicIp()
        {
            try
            {
                var ipAddress = await GetPublicIpAsync();
                return Ok(ipAddress);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        [HttpGet("return")]
        public IActionResult VnPayReturn()
        {
            VnPayLibrary vnpay = new VnPayLibrary();
            // Lấy chuỗi bí mật từ cấu hình
            string vnp_HashSecret = "F1XOCUGAV9K9GH7YLNULHG5XOAIJZNZX"; //Secret Key
            var queryString = HttpUtility.ParseQueryString(Request.QueryString.Value);

            // Lấy dữ liệu từ QueryString
            string vnp_SecureHash = queryString["vnp_SecureHash"];
            string vnp_ResponseCode = queryString["vnp_ResponseCode"];
            string vnp_TransactionStatus = queryString["vnp_TransactionStatus"];
            string vnp_TxnRef = queryString["vnp_TxnRef"];
            string vnp_TransactionNo = queryString["vnp_TransactionNo"];
            string vnp_BankCode = queryString["vnp_BankCode"];
            string vnp_Amount = queryString["vnp_Amount"];
            string vnp_TmnCode = queryString["vnp_TmnCode"];

            // Kiểm tra chữ ký hợp lệ (tạo hàm ValidateSignature)
            bool isValidSignature = Utils.ValidateSignature(queryString, vnp_SecureHash, vnp_HashSecret);

            // Chuẩn bị dữ liệu cho giao diện
            ViewData["IsValidSignature"] = isValidSignature;
            ViewData["ResponseCode"] = vnp_ResponseCode;
            ViewData["TransactionStatus"] = vnp_TransactionStatus;
            ViewData["OrderId"] = vnp_TxnRef;
            ViewData["TransactionNo"] = vnp_TransactionNo;
            ViewData["BankCode"] = vnp_BankCode;
            ViewData["Amount"] = long.Parse(vnp_Amount) / 100;
            ViewData["TerminalID"] = vnp_TmnCode;

            // Hiển thị kết quả ra giao diện Razor View
            return View("VnPayResult");
        }

        public static async Task<string> GetPublicIpAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync("https://api.ipify.org?format=json");

                    // Phân tích cú pháp JSON
                    JObject json = JObject.Parse(response);
                    string ip = json["ip"]?.ToString();

                    return ip;
                }
            }
            catch (Exception ex)
            {
                //return $"Error: {ex.Message}";
                return "";
            }
        }



        [HttpPost("GetPaymentURL")]
        public async Task<IActionResult> GeneratePaymentUrl([FromBody] PaymentRequest paymentRequest)
        {
            var csOrder = await _context.CsOrders.FindAsync(paymentRequest.Orderid);

            if (csOrder == null)
            {
                return NotFound();
            }


            int paymoney = Decimal.ToInt32(csOrder.Total) ;
            try
            {
                VnPayLibrary vnpay = new VnPayLibrary();

                string vnp_Returnurl = "http://localhost:16262/vnpay_return.aspx"; //URL nhan ket qua tra ve 
                string vnp_Url = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"; //URL thanh toan cua VNPAY 
                string vnp_TmnCode = "JSZ9K8HP"; //Ma định danh merchant kết nối (Terminal Id)
                string vnp_HashSecret = "F1XOCUGAV9K9GH7YLNULHG5XOAIJZNZX"; //Secret Key

                //Get payment input
                //OrderInfo order = new OrderInfo();
                //order.OrderId = 456830564576943; // Giả lập mã giao dịch hệ thống merchant gửi sang VNPAY
                //order.Amount = 100000; // Giả lập số tiền thanh toán hệ thống merchant gửi sang VNPAY 100,000 VND
                //order.Status = "0"; //0: Trạng thái thanh toán "chờ thanh toán" hoặc "Pending" khởi tạo giao dịch chưa có IPN
                //order.CreatedDate = DateTime.Now;
                //Save order to db

                //Build URL for VNPAY
                vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", paymoney.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", paymentRequest.PaymentIP);
                vnpay.AddRequestData("vnp_Locale", "en");
                vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan tai Miracle Land cho don hang:" + csOrder.Orderid.ToString());
                vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

                vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
                vnpay.AddRequestData("vnp_TxnRef", csOrder.Orderid.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                //Add Params of 2.1.0 Version
                //Billing

                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
                //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
                return Ok(new { url = paymentUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating payment URL.", error = ex.Message });
            }
        }
    }
}

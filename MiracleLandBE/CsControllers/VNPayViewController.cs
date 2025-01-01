using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MiracleLandBE.LogicalServices;
using MiracleLandBE.MinimalModels;
using MiracleLandBE.Models;

namespace MiracleLandBE.CsControllers
{
    public class VNPayViewController : Controller
    {
        private readonly TsmgbeContext _context;

        public VNPayViewController(TsmgbeContext context)
        {
            _context = context;
        }

        // Action nhận dữ liệu từ VNPay
        [HttpGet]
        [Route("vnpay_return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var queryString = HttpContext.Request.Query;

            // Kiểm tra chữ ký hash (vnp_SecureHash)
            string vnp_HashSecret = "F1XOCUGAV9K9GH7YLNULHG5XOAIJZNZX"; // Đặt chuỗi bí mật từ cấu hình
            VnPayLibrary vnpay = new VnPayLibrary();

            foreach (var key in queryString.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, queryString[key]);
                }
            }

            string vnp_SecureHash = queryString["vnp_SecureHash"];
            bool isValidSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

            // Lấy các thông tin từ dữ liệu trả về
            string orderId = vnpay.GetResponseData("vnp_TxnRef");
            string transactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            string transactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            long amount = long.Parse(vnpay.GetResponseData("vnp_Amount"));
            string bankCode = vnpay.GetResponseData("vnp_BankCode");
            string terminalID = vnpay.GetResponseData("vnp_TmnCode");

            ViewData["IsValidSignature"] = isValidSignature;
            ViewData["OrderId"] = orderId;
            ViewData["TransactionNo"] = transactionNo;
            ViewData["TransactionStatus"] = transactionStatus;
            ViewData["Amount"] = amount;
            ViewData["BankCode"] = bankCode;
            ViewData["TerminalID"] = terminalID;

            // Nếu chữ ký hợp lệ và trạng thái giao dịch thành công
            if (isValidSignature && transactionStatus == "00") // "00" là mã giao dịch thành công
            {
                try
                {
                    // Tìm đơn hàng trong cơ sở dữ liệu
                    var orderGuid = Guid.Parse(orderId); // Chuyển orderId sang dạng Guid
                    var order = await _context.CsOrders.FindAsync(orderGuid);

                    if (order != null)
                    {
                        order.IsPayment = true; // Cập nhật trạng thái thanh toán
                        await _context.SaveChangesAsync();
                        ViewData["PaymentUpdateMessage"] = "Cập nhật trạng thái đơn hàng thành công.";
                    }
                    else
                    {
                        ViewData["PaymentUpdateMessage"] = "Không tìm thấy đơn hàng.";
                    }
                }
                catch (Exception ex)
                {
                    ViewData["PaymentUpdateMessage"] = $"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}";
                }
            }

            // Render ra View để hiển thị kết quả giao dịch
            return View("VnPayResult");
        }
    }
}

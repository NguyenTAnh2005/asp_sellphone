using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace old_phone.Common
{
    public class Utility
    {
        public static string Encrypt(string pass)
        {
            if (string.IsNullOrEmpty(pass)) return "";

            MD5 md5 = new MD5CryptoServiceProvider();

            // Chuyển chuỗi thành mảng byte
            // Máy tính không hiểu chữ cái "A", "B", "C". Nó chỉ hiểu số (Byte).
            // Dòng này chuyển chuỗi "123456" thành một mảng các số: [49, 50, 51, 52, 53, 54]
            byte[] fromData = Encoding.UTF8.GetBytes(pass);

            // Tính toán mã hóa
            // Đây là bước quan trọng nhất. Nó trộn tung mảng số ở trên theo công thức MD5.
            byte[] targetData = md5.ComputeHash(fromData);

            // Vì kết quả 'targetData' vẫn là các con số máy tính (VD: [225, 10, 220...])
            // Chúng ta cần chuyển nó thành chữ cái và số (hệ Hexa) để con người đọc được (VD: "e10adc...")
            string byte2String = null;

            for (int i = 0; i < targetData.Length; i++)
            {
                // "x2": Là lệnh bảo máy tính "Hãy viết số này dưới dạng Hexa (hệ 16) và luôn dùng 2 ký tự".
                // Ví dụ: Số 10 -> thành chữ "0a". Số 255 -> thành chữ "ff".
                byte2String += targetData[i].ToString("x2");
            }

            return byte2String;
        }

        // Ham tao token ngau nhien 
        public static string GenerateNewToken()
        {
            // Tạo một chuỗi GUID ngẫu nhiên và loại bỏ dấu gạch ngang (N)
            return Guid.NewGuid().ToString("N");
        }
    }
}
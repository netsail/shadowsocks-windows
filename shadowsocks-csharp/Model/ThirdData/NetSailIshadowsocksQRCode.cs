using Shadowsocks.Controller;
using Shadowsocks.Model.ThirdData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

/// <summary>
/// http get iss config from QRCode
/// auth hezhifang
/// time 2016-11-13 00:00
/// </summary>
namespace Shadowsocks.Model
{
    class NetSailIshadowsocksQRCode: AstractIshadowsocks
    {
        const int ISS_SERVER_NUM = 3;
        const string QRCODE_URL = @"http://www.shadowsocks8.net/images/server0{0}.png";
        const string ISS_USER_AGENT = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36";
        public NetSailIshadowsocksQRCode(ShadowsocksController controller)
        {
            this.controller = controller;
            this.config = this.controller.GetConfigurationCopy();
        }

        public Server GetServerFromQRCode(string path)
        {
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
            request.UserAgent=ISS_USER_AGENT;
            request.KeepAlive = true;
            request.Timeout = 60000;
            Image image = Image.FromStream(request.GetResponse().GetResponseStream());
            Bitmap target = new Bitmap(image);
            var source = new BitmapLuminanceSource(target);
            var bitmap = new BinaryBitmap(new HybridBinarizer(source));
            QRCodeReader reader = new QRCodeReader();
            var result = reader.decode(bitmap);
            //controller.AddServerBySSURL(result.Text)
            if (result == null) return null;
            return new Server(result.Text);
        }

        public void LoadServerList()
        {
            DateTime maxUpdateTime = DateTime.Now;
            this.newServerList = new List<Server>();
            for (int i=0; i < ISS_SERVER_NUM; i++)
            {
                DateTime updateTime = DateTime.Now;
                Server item = null;
                String path = String.Format(QRCODE_URL, i+1);
                try
                {
                    item = GetServerFromQRCode(path);
                    item.remarks = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception e)
                {
                    Logging.Error(string.Format(@"请求地址:{0}异常:{1}", path, e));
                }
                this.MarkConfigNewTime(item);
                if (item != null)this.newServerList.Add(item);
            }
        }

        public async Task AccessTheWebAsync()
        {
            await Task.Run(() =>
            {
                this.LoadServerList();
            });
        }

    }
}

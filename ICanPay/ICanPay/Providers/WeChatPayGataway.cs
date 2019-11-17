using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace ICanPay.Providers
{
    /// <summary>
    /// ΢��֧������
    /// </summary>
    /// <remarks>
    /// ʹ��ģʽ��ʵ��΢��֧��
    /// </remarks>
    public sealed class WeChatPayGataway : GatewayBase, IPaymentQRCode, IQueryNow
    {

        #region ˽���ֶ�

        private const string PayGatewayUrl = "https://api.mch.weixin.qq.com/pay/unifiedorder";
        private const string QueryGatewayUrl = "https://api.mch.weixin.qq.com/pay/orderquery";

        #endregion


        #region ���캯��

        /// <summary>
        /// ��ʼ��΢��֧������
        /// </summary>
        public WeChatPayGataway()
        {
        }


        /// <summary>
        /// ��ʼ��΢��֧������
        /// </summary>
        /// <param name="gatewayParameterList">����֪ͨ�����ݼ���</param>
        public WeChatPayGataway(Dictionary<string, GatewayParameter> gatewayParameterList)
            : base(gatewayParameterList)
        {
        }

        #endregion


        public override GatewayType GatewayType
        {
            get { return GatewayType.WeChatPay; }
        }

        public override PaymentNotifyMethod PaymentNotifyMethod
        {
            get { return PaymentNotifyMethod.ServerNotify; }
        }

        private WeChatPayMerchant WeChatPayMerchant
        {
            get
            {
                WeChatPayMerchant weChatPayMerchant = Merchant as WeChatPayMerchant;
                if (weChatPayMerchant == null)
                {
                    throw new ArgumentException("�̻��������Ͳ���΢��֧���̻����뽫 Merchant ��������Ϊ WeChatPayMerchant ����", "Merchant");
                }

                return weChatPayMerchant;
            }
        }
            

        public override bool ValidateNotify()
        {
            if (IsSuccessResult())
            {
                ReadNotifyOrder();
                return true;
            }

            return false;
        }

        public string GetPaymentQRCodeContent()
        {
            string createOrderXml = CreateOrder();
            return GetWeChatPayUrl(createOrderXml);
        }

        private string CreateOrder()
        {
            InitPaymentOrderParameter();
            string paymentOrderXml = ConvertGatewayParameterDataToXml();

            return PostRequest(paymentOrderXml, PayGatewayUrl);
        }

        public bool QueryNow()
        {
            string queryOrderResultXml = QueryOrder();
            return CheckQueryResult(queryOrderResultXml);
        }

        private string QueryOrder()
        {
            InitQueryOrderParameter();
            string queryOrderXml = ConvertGatewayParameterDataToXml();

            return PostRequest(queryOrderXml, QueryGatewayUrl);
        }


        /// <summary>
        /// ��ʼ��֧�������Ĳ���
        /// </summary>
        private void InitPaymentOrderParameter()
        {
            SetGatewayParameterValue("mch_id", Merchant.UserName);
            SetGatewayParameterValue("appid", WeChatPayMerchant.AppId);
            SetGatewayParameterValue("nonce_str", GenerateNonceString());
            SetGatewayParameterValue("body", Order.Subject);
            SetGatewayParameterValue("out_trade_no", Order.Id);
            SetGatewayParameterValue("total_fee", Order.Amount * 100);
            SetGatewayParameterValue("spbill_create_ip", "127.0.0.1");
            SetGatewayParameterValue("notify_url", Merchant.NotifyUrl);
            SetGatewayParameterValue("trade_type", "NATIVE");
            SetGatewayParameterValue("product_id", Order.Id);
            SetGatewayParameterValue("sign", GetSign());    // ǩ����Ҫ��������ã�����ȱ�ٲ�����
        }


        /// <summary>
        /// ��ȡ֪ͨ�еĶ������������
        /// </summary>
        private void ReadNotifyOrder()
        {
            Order.Id = GetGatewayParameterValue("out_trade_no");
            Order.Amount = GetGatewayParameterValue<int>("total_fee") * 0.01;
        }


        /// <summary>
        /// ��������ַ���
        /// </summary>
        /// <returns></returns>
        private string GenerateNonceString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }


        /// <summary>
        /// ����������ת����XML
        /// </summary>
        /// <returns></returns>
        private string ConvertGatewayParameterDataToXml()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            StringBuilder xmlBuilder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(xmlBuilder, settings))
            {
                writer.WriteStartElement("xml");
                foreach (var item in GetSortedGatewayParameter())
                {
                    writer.WriteElementString(item.Key, item.Value);
                }
                writer.WriteEndElement();
                writer.Flush();
            }

            return xmlBuilder.ToString();
        }


        /// <summary>
        /// ���ǩ��
        /// </summary>
        /// <returns></returns>
        private string GetSign()
        {
            StringBuilder signBuilder = new StringBuilder();
            foreach (var item in GetSortedGatewayParameter())
            {
                // ��ֵ�Ĳ�����sign����������ǩ��
                if (!string.IsNullOrEmpty(item.Value) && string.Compare("sign", item.Key) != 0)
                {
                    signBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
                }
            }

            signBuilder.Append("key=" + Merchant.Key);
            return Utility.GetMD5(signBuilder.ToString());
        }


        /// <summary>
        /// �ύ����
        /// </summary>
        /// <param name="requestXml">�����XML����</param>
        /// <param name="gatewayUrl">����URL</param>
        /// <returns></returns>
        private string PostRequest(string requestXml, string gatewayUrl)
        {
            byte[] dataByte = Encoding.UTF8.GetBytes(requestXml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(gatewayUrl);
            request.Method = "POST";
            request.ContentType = "text/xml";
            request.ContentLength = dataByte.Length;

            try
            {
                using (Stream outStream = request.GetRequestStream())
                {
                    outStream.Write(dataByte, 0, dataByte.Length);
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        if (reader != null)
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                request.Abort();
            }

            return string.Empty;
        }


        /// <summary>
        /// ���΢��֧����URL
        /// </summary>
        /// <param name="resultXml">�����������ص�����</param>
        /// <returns></returns>
        private string GetWeChatPayUrl(string resultXml)
        {
            // ��Ҫ�����֮ǰ���������Ĳ����������Խ��յ��Ĳ�����ɸ��š�
            ClearAllGatewayParameter();
            ReadResultXml(resultXml);
            if (IsSuccessResult())
            {
                return GetGatewayParameterValue("code_url");
            }

            return string.Empty;
        }

        /// <summary>
        /// ��ȡ�����XML
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        private void ReadResultXml(string xml)
        {
            XmlDocument xmlDocument = Utility.CreateXmlSafeDocument();
            try
            {
                xmlDocument.LoadXml(xml);
            }
            catch (XmlException) { }
            
            if (xmlDocument.FirstChild != null && xmlDocument.FirstChild.ChildNodes != null)
            {
                foreach (XmlNode item in xmlDocument.FirstChild.ChildNodes)
                {
                    SetGatewayParameterValue(item.Name, item.InnerText);
                }
            }
        }


        /// <summary>
        /// �Ƿ����ѳɹ�֧����֧��֪ͨ
        /// </summary>
        /// <returns></returns>
        private bool IsSuccessResult()
        {
            if (CompareGatewayParameterValue("return_code", "SUCCESS")&&
                CompareGatewayParameterValue("result_code", "SUCCESS")&&
                CompareGatewayParameterValue("sign", GetSign()))
            {
                return true;
            }

            return false;
        }
        

        /// <summary>
        /// ����ѯ���
        /// </summary>
        /// <param name="resultXml">��ѯ�����XML</param>
        /// <returns></returns>
        private bool CheckQueryResult(string resultXml)
        {
            // ��Ҫ�����֮ǰ��ѯ�����Ĳ����������Խ��յ��Ĳ�����ɸ��š�
            ClearAllGatewayParameter();
            ReadResultXml(resultXml);
            if (IsSuccessResult())
            {
               if(CompareGatewayParameterValue("out_trade_no", Order.Id) &&
                  CompareGatewayParameterValue("total_fee", Order.Amount * 100))
               {
                   return true;
               }
            }

            return false;
        }

          /// <summary>
        /// ��ʼ����ѯ��������
        /// </summary>
        private void InitQueryOrderParameter()
        {
            SetGatewayParameterValue("appid", WeChatPayMerchant.AppId);
            SetGatewayParameterValue("mch_id", Merchant.UserName);
            SetGatewayParameterValue("out_trade_no", Order.Id);
            SetGatewayParameterValue("nonce_str", GenerateNonceString());
            SetGatewayParameterValue("sign", GetSign());    // ǩ����Ҫ��������ã�����ȱ�ٲ�����
        }
        
        
        /// <summary>
        /// ��ʼ����ʾ�ѳɹ����յ�֧��֪ͨ������
        /// </summary>
        private void InitProcessSuccessParameter()
        {
            SetGatewayParameterValue("return_code", "SUCCESS");
        }


        public override void WriteSucceedFlag()
        {
            // ��Ҫ�����֮ǰ���յ���֪ͨ�Ĳ��������������ɱ�־�ɹ����յ�֪ͨ��XML��ɸ��š�
            ClearAllGatewayParameter();
            InitProcessSuccessParameter();
            HttpContext.Current.Response.Write(ConvertGatewayParameterDataToXml());
        }
    }
}

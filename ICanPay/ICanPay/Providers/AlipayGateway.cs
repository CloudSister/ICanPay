using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ICanPay.Providers
{
    /// <summary>
    /// ֧��������
    /// </summary>
    /// <remarks>
    /// ��ǰ֧������ʵ�ֽ�֧��MD5��Կ��
    /// </remarks>
    public sealed class AlipayGateway : GatewayBase, IPaymentForm, IPaymentUrl
    {

        #region ˽���ֶ�

        private const string PayGatewayUrl = "https://mapi.alipay.com/gateway.do";

        #endregion


        #region ���캯��

        /// <summary>
        /// ��ʼ��֧��������
        /// </summary>
        public AlipayGateway()
        {
        }


        /// <summary>
        /// ��ʼ��֧��������
        /// </summary>
        /// <param name="gatewayParameterList">����֪ͨ�����ݼ���</param>
        public AlipayGateway(Dictionary<string, GatewayParameter> gatewayParameterList)
            : base(gatewayParameterList)
        {
        }

        #endregion


        #region ����

        public override GatewayType GatewayType
        {
            get
            {
                return GatewayType.Alipay;
            }
        }


        public override PaymentNotifyMethod PaymentNotifyMethod
        {
            get
            {
                // ͨ��RequestType��UserAgent���ж��Ƿ�Ϊ������֪ͨ
                if (string.Compare(HttpContext.Current.Request.RequestType, "POST") == 0 &&
                    string.Compare(HttpContext.Current.Request.UserAgent, "Mozilla/4.0") == 0)
                {
                    return PaymentNotifyMethod.ServerNotify;
                }

                return PaymentNotifyMethod.AutoReturn;
            }
        }


        protected override Encoding PageEncoding
        {
            get
            {
                return Encoding.GetEncoding("GB2312");
            }
        }


        private AlipayMerchant AlipayMerchant
        {
            get
            {
                AlipayMerchant alipayMerchant = Merchant as AlipayMerchant;
                if (alipayMerchant == null)
                {
                    throw new ArgumentException("�̻��������Ͳ���֧�����̻����뽫 Merchant ��������Ϊ AlipayMerchant ����", "Merchant");
                }

                return alipayMerchant;
            }
        }


        #endregion


        #region ����

        public string BuildPaymentForm()
        {
            InitOrderParameter();

            return GetFormHtml(PayGatewayUrl);
        }


        /// <summary>
        /// ��ʼ����������
        /// </summary>
        private void InitOrderParameter()
        {
            SetGatewayParameterValue("service", "create_direct_pay_by_user");
            SetGatewayParameterValue("partner", Merchant.UserName);
            SetGatewayParameterValue("notify_url", Merchant.NotifyUrl);
            SetGatewayParameterValue("return_url", Merchant.NotifyUrl);
            SetGatewayParameterValue("seller_email", AlipayMerchant.SellerEmail);
            SetGatewayParameterValue("sign_type", "MD5");
            SetGatewayParameterValue("subject", Order.Subject);
            SetGatewayParameterValue("out_trade_no", Order.Id);
            SetGatewayParameterValue("total_fee", Order.Amount);
            SetGatewayParameterValue("payment_type", "1");
            SetGatewayParameterValue("_input_charset", "gb2312");
            SetGatewayParameterValue("sign", GetOrderSign());    // ǩ����Ҫ��������ã�����ȱ�ٲ�����
        }


        public string BuildPaymentUrl()
        {
            InitOrderParameter();

            return string.Format("{0}?{1}", PayGatewayUrl, GetPaymentQueryString());
        }


        private string GetPaymentQueryString()
        {
            return BuildQueryString(GetSortedGatewayParameter());
        }


        /// <summary>
        /// �������ǩ���Ĳ���
        /// </summary>
        private IDictionary<string, string> GetSignParameter()
        {
            SortedDictionary<string, string> result = new SortedDictionary<string, string>();
            foreach(KeyValuePair<string, string> item in GetSortedGatewayParameter())
            {
                // ������Ϊ sign��sign_type �Ĳ���������ǩ����
                if (string.Compare("sign", item.Key) != 0 && string.Compare("sign_type", item.Key) != 0)
                {
                    result.Add(item.Key, item.Value);
                }
            }

            return result;
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


        /// <summary>
        /// ��ȡ֪ͨ�еĶ������������
        /// </summary>
        private void ReadNotifyOrder()
        {
            Order.Amount = GetGatewayParameterValue<double>("total_fee");
            Order.Id = GetGatewayParameterValue("out_trade_no");
        }


        /// <summary>
        /// �Ƿ����ѳɹ�֧����֧��֪ͨ
        /// </summary>
        /// <returns></returns>
        private bool IsSuccessResult()
        {
            if(ValidateNotifyParameter() && ValidateNotifySign())
            {
                if(ValidateNotifyId())
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// ���֧��֪ͨ���Ƿ�֧���ɹ���ǩ���Ƿ���ȷ��
        /// </summary>
        /// <returns></returns>
        private bool ValidateNotifyParameter()
        {
            // ֧��״̬�Ƿ�Ϊ�ɹ���
            // TRADE_FINISHED����ͨ��ʱ���˵Ľ��׳ɹ�״̬��
            // TRADE_SUCCESS����ͨ�˸߼���ʱ���˻��Ʊ������Ʒ��Ľ��׳ɹ�״̬��
            if (CompareGatewayParameterValue("trade_status", "TRADE_FINISHED") ||
                CompareGatewayParameterValue("trade_status", "TRADE_SUCCESS"))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// ��֤֧����֪ͨ��ǩ��
        /// </summary>
        private bool ValidateNotifySign()
        {
            // ��֤֪ͨ��ǩ��
            if (CompareGatewayParameterValue("sign", GetOrderSign()))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// �����ز����ļ�������
        /// </summary>
        /// <param name="coll">ԭ���ز����ļ���</param>
        private SortedList<string, string> GatewayParameterDataSort(ICollection<GatewayParameter> coll)
        {
            SortedList<string, string> list = new SortedList<string, string>();
            foreach (GatewayParameter item in coll)
            {
                list.Add(item.Name, item.Value);
            }

            return list;
        }


        /// <summary>
        /// ��ö�����ǩ����
        /// </summary>
        private string GetOrderSign()
        {
            // ��� MD5 ֵʱ��Ҫʹ�� GB2312 ���룬����������������ʱ����ʾǩ���쳣������ MD5 ֵ����ΪСд��
            return Utility.GetMD5(BuildQueryString(GetSignParameter()) + Merchant.Key, PageEncoding).ToLower();
        }


        public override void WriteSucceedFlag()
        {
            if (PaymentNotifyMethod == PaymentNotifyMethod.ServerNotify)
            {
                HttpContext.Current.Response.Write("success");
            }
        }


        /// <summary>
        /// ��֤���ص�֪ͨId�Ƿ���Ч
        /// </summary>
        private bool ValidateNotifyId()
        {
            // ������Զ����ص�֪ͨId������֤��1����ʧЧ��
            // �������첽֪ͨ��֪ͨId����������־�ɹ����յ�֪ͨ��success�ַ�����ʧЧ��
            if (string.Compare(Utility.ReadPage(GetValidateNotifyUrl()), "true") == 0)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// �����֤֧����֪ͨ��Url
        /// </summary>
        private string GetValidateNotifyUrl()
        {
            return string.Format("{0}?service=notify_verify&partner={1}&notify_id={2}", PayGatewayUrl, Merchant.UserName,
                                 GetGatewayParameterValue("notify_id"));
        }

        #endregion

    }
}
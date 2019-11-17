using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ICanPay
{
    /// <summary>
    /// ֧�����صĳ������
    /// </summary>
    public abstract class GatewayBase
    {

        #region ˽���ֶ�

        private Merchant _merchant;
        private Order _order;
        private Dictionary<string, GatewayParameter> _gatewayParameterList;
        private const string FormItem = "<input type='hidden' name='{0}' value='{1}'>";

        #endregion


        #region ���캯��


        protected GatewayBase() : this(new Dictionary<string, GatewayParameter>())
        {
        }


        protected GatewayBase(Dictionary<string, GatewayParameter> gatewayParameterList)
        {
            InitHttpContextEncoding();

            _gatewayParameterList = gatewayParameterList;
        }

        #endregion


        #region ����


        /// <summary>
        /// �̼�����
        /// </summary>
        public Merchant Merchant
        {
            get
            {
                if (_merchant == null)
                {
                    _merchant = new Merchant();
                }

                return _merchant;
            }

            set
            {
                _merchant = value;
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        public Order Order
        {
            get
            {
                if (_order == null)
                {
                    _order = new Order();
                }

                return _order;
            }

            set
            {
                _order = value;
            }
        }


        /// <summary>
        /// ֧�����ص�����
        /// </summary>
        public abstract GatewayType GatewayType
        {
            get;
        }


        /// <summary>
        /// ֧��֪ͨ�ķ��ط�ʽ
        /// </summary>
        /// <remarks>
        /// Ŀǰ��֧��������֧���ɹ������Get��Post��ʽ��֧��������ظ��̻���
        /// POST��ʽ�ķ���һ����ͨ�����ط��������ͣ��������Ҫ���̻�����ַ���Ǳ�ʾ�ѳɹ����յ�֧�������
        /// ����һ����ͨ��GET��ʽ���û����ص��̻�����վ����ʱ�����POST����ʱ�ķ�ʽ���������������ѳɹ����յ��ַ�����
        /// ��������û���е�����֣���ʱ��ʾ֧���ɹ���ҳ�潫������ʡ����Կ���ͨ��PaymentNotifyMethod�������ж�
        /// ֧������ķ��ͷ�ʽ���Ծ�����Ӧ���������ѳɹ����յ��ַ����������û���ʾ֧���ɹ���ҳ�档
        /// ����������֪ͨʱ����ΪServerNotify��������û�ͨ���������ת����������֪ͨ��ҳ������ΪAutoReturn��
        /// </remarks>
        public abstract PaymentNotifyMethod PaymentNotifyMethod
        {
            get;
        }



        /// <summary>
        /// ֧�����ص�Get��Post���ݵļ��ϡ�
        /// </summary>
        public ICollection<GatewayParameter> GatewayParameterData
        {
            get
            {
                return _gatewayParameterList.Values;
            }
        }


        /// <summary>
        /// ֧��������ʹ�õı���
        /// </summary>
        /// <remarks>
        /// ���Ķ�֧�������ĵ�������ȷ�ı��룬Ĭ��Ϊ UTF8������ı���ᵼ�³������롣
        /// </remarks>
        protected virtual Encoding PageEncoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }


        #endregion


        #region ����

        /// <summary>
        /// ��ʼ�� HttpContext �ı��룬��������ѯ��������������֪ͨ��ҳ�潫ʹ�����õı��롣
        /// </summary>
        /// <remarks>
        /// ֧��ƽ̨��ʹ�õı������Ķ�֧��ƽ̨�Ŀ����ĵ������� HttpContext ��Ŀ����Ϊ�˽����֧ͬ��ƽ̨�ı��벻һ�µ����⡣
        /// ���û����ȷ����֧��ƽ̨��ʹ�õı���ᵼ�³������롣
        /// </remarks>
        private void InitHttpContextEncoding()
        {
            HttpContext.Current.Request.ContentEncoding = PageEncoding;
            HttpContext.Current.Response.ContentEncoding = PageEncoding;
        }


        /// <summary>
        /// ����Form HTML����
        /// </summary>
        /// <param name="url">���ص�Url</param>
        protected string GetFormHtml(string url)
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine(string.Format("<form name='Gateway' method='post' action ='{0}'>", url));
            foreach (GatewayParameter item in GatewayParameterData)
            {
                if (item.HttpMethod == HttpMethod.None || item.HttpMethod  == HttpMethod.Post)
                {
                    htmlBuilder.AppendLine(string.Format(FormItem, item.Name, item.Value));
                }
            }
            htmlBuilder.AppendLine("</form>");
            htmlBuilder.AppendLine("<script language='javascript' type='text/javascript'>");
            htmlBuilder.AppendLine("document.Gateway.submit();");
            htmlBuilder.AppendLine("</script>");
            htmlBuilder.AppendLine("</body>");

            return htmlBuilder.ToString();
        }


        /// <summary>
        /// ��ð���ĸ�������������ز����ļ���
        /// </summary>
        /// <returns></returns>
        protected SortedDictionary<string, string> GetSortedGatewayParameter()
        {
            SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
            foreach (GatewayParameter item in GatewayParameterData)
            {
                sortedDictionary.Add(item.Name, item.Value);
            }

            return sortedDictionary;
        }



        /// <summary>
        /// �������ز����Ĳ�ѯ�ַ�����
        /// </summary>
        /// <param name="parameterList">���ز����ļ���</param>
        protected string BuildQueryString(ICollection<GatewayParameter> parameterList)
        {
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (GatewayParameter item in parameterList)
            {
                queryStringBuilder.AppendFormat("{0}={1}&", item.Name, item.Value);
            }

            return queryStringBuilder.ToString().TrimEnd('&');
        }


        /// <summary>
        /// �������ز����Ĳ�ѯ�ַ�����
        /// </summary>
        /// <param name="parameterDictionary">���ز����� Dictionary</param>
        protected string BuildQueryString(IDictionary<string, string> parameterDictionary)
        {
            StringBuilder queryStringBuilder = new StringBuilder();
            foreach (var item in parameterDictionary)
            {
                queryStringBuilder.AppendFormat("{0}={1}&", item.Key, item.Value);
            }

            return queryStringBuilder.ToString().TrimEnd('&');
        }


        /// <summary>
        /// �������ط��ص�֪ͨ����֤�����Ƿ�֧���ɹ���
        /// </summary>
        public abstract bool ValidateNotify();


        /// <summary>
        /// �����յ�֧������֪ͨ����֤����ʱ������֧������Ҫ���ʽ�����ʾ�ɹ����յ�����֪ͨ���ַ�����
        /// </summary>
        public abstract void WriteSucceedFlag();


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        protected void SetGatewayParameterValue(string gatewayParameterName, object gatewayParameterValue)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue, HttpMethod.None);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        protected void SetGatewayParameterValue(string gatewayParameterName, string gatewayParameterValue)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue, HttpMethod.None);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        /// <param name="httpMethod">���صĲ��������󷽷�������</param>
        protected void SetGatewayParameterValue(string gatewayParameterName, object gatewayParameterValue, HttpMethod httpMethod)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue.ToString(), httpMethod);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        /// <param name="httpMethod">���صĲ��������󷽷�������</param>
        protected void SetGatewayParameterValue(string gatewayParameterName, string gatewayParameterValue, HttpMethod httpMethod)
        {
            if (_gatewayParameterList.ContainsKey(gatewayParameterName))
            {
                GatewayParameter gatewayParameter = _gatewayParameterList[gatewayParameterName];
                if (string.Compare(gatewayParameter.Value, gatewayParameterValue) != 0 ||
                    gatewayParameter.HttpMethod != httpMethod)
                {
                    gatewayParameter.HttpMethod = httpMethod;
                    gatewayParameter.Value = gatewayParameterValue;
                }
            }
            else
            {
                GatewayParameter gatewayParameter = new GatewayParameter(gatewayParameterName, gatewayParameterValue, httpMethod);
                _gatewayParameterList.Add(gatewayParameterName, gatewayParameter);
            }
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        public string GetGatewayParameterValue(string gatewayParameterName)
        {
            return GetGatewayParameterValue<string>(gatewayParameterName, HttpMethod.None);
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="httpMethod">�����������󷽷�������</param>
        public string GetGatewayParameterValue(string gatewayParameterName, HttpMethod httpMethod)
        {
            return GetGatewayParameterValue<string>(gatewayParameterName, httpMethod);
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        public T GetGatewayParameterValue<T>(string gatewayParameterName)
        {
            return GetGatewayParameterValue<T>(gatewayParameterName, HttpMethod.None);
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="httpMethod">�����������󷽷�������</param>
        public T GetGatewayParameterValue<T>(string gatewayParameterName, HttpMethod httpMethod)
        {
            string gatewayParameterValue = string.Empty;
            if (_gatewayParameterList.ContainsKey(gatewayParameterName))
            {
                GatewayParameter gatewayParameter = _gatewayParameterList[gatewayParameterName];
                if (httpMethod == HttpMethod.None || httpMethod == gatewayParameter.HttpMethod)
                {
                    gatewayParameterValue = gatewayParameter.Value;
                }
            }

            return (T)Convert.ChangeType(gatewayParameterValue, typeof(T));
        }


        /// <summary>
        /// �Ƚ����ز�����ֵ��ָ����ֵ�Ƿ�һ��
        /// </summary>
        /// <typeparam name="T">�Ƚϵ�ֵ������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="compareValue">�Ƚϵ�ֵ</param>
        protected bool CompareGatewayParameterValue<T>(string gatewayParameterName, T compareValue)
        {
            return CompareGatewayParameterValue(gatewayParameterName, HttpMethod.None, compareValue);
        }


        /// <summary>
        /// �Ƚ����ز�����ֵ��ָ����ֵ�Ƿ�һ��
        /// </summary>
        /// <typeparam name="T">�Ƚϵ�ֵ������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="httpMethod">�����������󷽷�������</param>
        /// <param name="compareValue">�Ƚϵ�ֵ</param>
        protected bool CompareGatewayParameterValue<T>(string gatewayParameterName, HttpMethod httpMethod, T compareValue)
        {
            T gatewayParameterValue = GetGatewayParameterValue<T>(gatewayParameterName, httpMethod);

            return gatewayParameterValue.Equals(compareValue);
        }


        /// <summary>
        /// ����������صĲ���
        /// </summary>
        protected void ClearAllGatewayParameter()
        {
            _gatewayParameterList.Clear();
        }


        #endregion

    }
}

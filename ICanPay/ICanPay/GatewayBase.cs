using System.Collections.Generic;
using System.Text;

namespace ICanPay
{
    /// <summary>
    /// ֧�����صĳ������
    /// </summary>
    public abstract class GatewayBase
    {

        #region ˽���ֶ�

        Merchant merchant;
        Order order;
        Dictionary<string, GatewayParameter> gatewayParameterList;
        const string formItem = "<input type='hidden' name='{0}' value='{1}'>";

        #endregion


        #region ���캯��


        protected GatewayBase() : this(new Dictionary<string, GatewayParameter>())
        {
        }


        protected GatewayBase(Dictionary<string, GatewayParameter> gatewayParameterList)
        {
            this.gatewayParameterList = gatewayParameterList;
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
                if (merchant == null)
                {
                    merchant = new Merchant();
                }

                return merchant;
            }

            set
            {
                merchant = value;
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        public Order Order
        {
            get
            {
                if (order == null)
                {
                    order = new Order();
                }

                return order;
            }

            set
            {
                order = value;
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
        /// ֧�����ص�Get��Post���ݵļ��ϣ�Get��ʽ����QueryString��ֵ��Ϊδ���롣
        /// </summary>
        public ICollection<GatewayParameter> GatewayParameterData
        {
            get
            {
                return gatewayParameterList.Values;
            }
        }


        #endregion


        #region ����


        /// <summary>
        /// ����Form HTML����
        /// </summary>
        /// <param name="url">���ص�Url</param>
        protected string GetFormHtml(string url)
        {
            StringBuilder html = new StringBuilder();
            html.AppendLine("<body>");
            html.AppendLine(string.Format("<form name='Gateway' method='post' action ='{0}'>", url));
            foreach (GatewayParameter item in GatewayParameterData)
            {
                if (item.HttpMethod == HttpMethod.None || item.HttpMethod  == HttpMethod.Post)
                {
                    html.AppendLine(string.Format(formItem, item.Name, item.Value));
                }
            }
            html.AppendLine("</form>");
            html.AppendLine("<script language='javascript' type='text/javascript'>");
            html.AppendLine("document.Gateway.submit();");
            html.AppendLine("</script>");
            html.AppendLine("</body>");

            return html.ToString();
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
        /// <param name="gatewayParameterName">���صĲ�������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        public void SetGatewayParameterValue(string gatewayParameterName, object gatewayParameterValue)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue, HttpMethod.None);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���صĲ�������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        public void SetGatewayParameterValue(string gatewayParameterName, string gatewayParameterValue)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue, HttpMethod.None);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���صĲ�������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        /// <param name="httpMethod">���صĲ��������󷽷�������</param>
        public void SetGatewayParameterValue(string gatewayParameterName, object gatewayParameterValue, HttpMethod httpMethod)
        {
            SetGatewayParameterValue(gatewayParameterName, gatewayParameterValue.ToString(), httpMethod);
        }


        /// <summary>
        /// �������ص�����
        /// </summary>
        /// <param name="gatewayParameterName">���صĲ�������</param>
        /// <param name="gatewayParameterValue">���صĲ���ֵ</param>
        /// <param name="httpMethod">���صĲ��������󷽷�������</param>
        public void SetGatewayParameterValue(string gatewayParameterName, string gatewayParameterValue, HttpMethod httpMethod)
        {
            if (gatewayParameterList.ContainsKey(gatewayParameterName))
            {
                GatewayParameter gatewayParameter = gatewayParameterList[gatewayParameterName];
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
                gatewayParameterList.Add(gatewayParameterName, gatewayParameter);
            }
        }


        /// <summary>
        /// ������صĲ���ֵ��û�в���ֵʱ���ؿ��ַ�����Get��ʽ��ֵ��Ϊδ���롣
        /// </summary>
        /// <param name="gatewayParameterName">���صĲ�������</param>
        public string GetGatewayParameterValue(string gatewayParameterName)
        {
            return GetGatewayParameterValue(gatewayParameterName, HttpMethod.None);
        }


        /// <summary>
        /// ������صĲ���ֵ��û�в���ֵʱ���ؿ��ַ�����Get��ʽ��ֵ��Ϊδ���롣
        /// </summary>
        /// <param name="gatewayParameterName">���صĲ�������</param>
        /// <param name="httpMethod">���ص����ݵ����󷽷�������</param>
        public string GetGatewayParameterValue(string gatewayParameterName, HttpMethod httpMethod)
        {
            if (gatewayParameterList.ContainsKey(gatewayParameterName))
            {
                GatewayParameter gatewayParameter = gatewayParameterList[gatewayParameterName];
                if (httpMethod == HttpMethod.None || httpMethod == gatewayParameter.HttpMethod)
                {
                    return gatewayParameter.Value;
                }

            }

            return string.Empty;
        }


        /// <summary>
        /// ����������صĲ���
        /// </summary>
        protected void ClearAllGatewayParameter()
        {
            gatewayParameterList.Clear();
        }

        #endregion

    }
}

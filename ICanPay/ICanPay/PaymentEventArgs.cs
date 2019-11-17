using System;
using System.Collections.Generic;

namespace ICanPay
{
    /// <summary>
    /// ֧���¼����ݵĻ���
    /// </summary>
    public abstract class PaymentEventArgs : EventArgs
    {

        #region �ֶ�

        private string _notifyServerHostAddress;
        protected GatewayBase _gateway;

        #endregion


        #region ���캯��

        /// <summary>
        /// ��ʼ��֧���¼����ݵĻ���
        /// </summary>
        /// <param name="gateway">֧������</param>
        public PaymentEventArgs(GatewayBase gateway)
        {
            this._gateway = gateway;
            _notifyServerHostAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
        }


        #endregion


        #region ����

        /// <summary>
        /// ����֧��֪ͨ������IP��ַ
        /// </summary>
        public string NotifyServerHostAddress
        {
            get
            {
                return _notifyServerHostAddress;
            }
        }


        /// <summary>
        /// ֧�����ص�Get��Post���ݵļ���
        /// </summary>
        public ICollection<GatewayParameter> GatewayParameterData
        {
            get
            {
                return _gateway.GatewayParameterData;
            }
        }

        #endregion


        #region ����

        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        public string GetGatewayParameterValue(string gatewayParameterName)
        {
            return _gateway.GetGatewayParameterValue(gatewayParameterName);
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="httpMethod">���صĲ��������󷽷�������</param>
        public string GetGatewayParameterValue(string gatewayParameterName, HttpMethod httpMethod)
        {
            return _gateway.GetGatewayParameterValue(gatewayParameterName, httpMethod);
        }

        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        public T GetGatewayParameterValue<T>(string gatewayParameterName)
        {
            return _gateway.GetGatewayParameterValue<T>(gatewayParameterName);
        }


        /// <summary>
        /// ������صĲ���ֵ������������ʱ���ؿ��ַ�����
        /// </summary>
        /// <typeparam name="T">���ص���������</typeparam>
        /// <param name="gatewayParameterName">���ز���������</param>
        /// <param name="httpMethod">�����������󷽷�������</param>
        public T GetGatewayParameterValue<T>(string gatewayParameterName, HttpMethod httpMethod)
        {
            return _gateway.GetGatewayParameterValue<T>(gatewayParameterName, httpMethod);
        }


        #endregion

    }
}
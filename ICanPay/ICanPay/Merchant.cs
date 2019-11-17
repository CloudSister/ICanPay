using System;

namespace ICanPay
{
    /// <summary>
    /// �̻�����
    /// </summary>
    public class Merchant
    {

        #region ˽���ֶ�

        private string _userName;
        private string _key;
        private Uri _notifyUrl;

        #endregion


        #region ���캯��

        public Merchant()
        {
        }


        public Merchant(string userName, string key, Uri notifyUrl, GatewayType gatewayType)
        {
            UserName = userName;
            Key = key;
            NotifyUrl = notifyUrl;
            GatewayType = gatewayType;
        }

        #endregion


        #region ����

        /// <summary>
        /// �̻��ʺ�
        /// </summary>
        public string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    throw new ArgumentNullException("UserName", "�̻��ʺ�û������");
                }

                return _userName;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("UserName", "�̻��ʺŲ���Ϊ��");
                }

                _userName = value;
            }
        }


        /// <summary>
        /// �̻���Կ
        /// </summary>
        public string Key
        {
            get
            {
                if (string.IsNullOrEmpty(_key))
                {
                    throw new ArgumentNullException("Key", "�̻���Կû������");
                }

                return _key;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("Key", "�̻���Կ����Ϊ��");
                }

                _key = value;
            }
        }


        /// <summary>
        /// ���ػط�֪ͨURL
        /// </summary>
        public Uri NotifyUrl
        {
            get
            {
                if (_notifyUrl == null)
                {
                    throw new ArgumentNullException("NotifyUrl", "����֪ͨUrlû������");
                }

                return _notifyUrl;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("NotifyUrl", "����֪ͨUrl����Ϊ��");
                }

                _notifyUrl = value;
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        public GatewayType GatewayType { get; set; }

        #endregion

    }
}
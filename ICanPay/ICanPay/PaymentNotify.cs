using System;
using System.Collections.Generic;
using System.Linq;

namespace ICanPay
{
    /// <summary>
    /// ���ط��ص�֧��֪ͨ���ݵĽ���
    /// </summary>
    public class PaymentNotify
    {

        #region ˽���ֶ�

        ICollection<Merchant> merchantList;

        #endregion


        #region ���캯��

        /// <summary>
        /// ��ʼ�������Ķ�������PaymentNotify
        /// </summary>
        public PaymentNotify() : this(new List<Merchant>())
        {
        }

        /// <summary>
        /// ��ʼ�������Ķ�������PaymentNotify
        /// </summary>
        /// <param name="merchant">�̻�����</param>
        public PaymentNotify(Merchant merchant) : this(new List<Merchant>())
        {
            AddMerchant(merchant);
        }


        /// <summary>
        /// ��ʼ�������Ķ�������PaymentNotify
        /// </summary>
        /// <param name="merchantList">������֤֧�����ط������ݵ��̻������б�</param>
        public PaymentNotify(ICollection<Merchant> merchantList)
        {
            this.merchantList = merchantList;
        }

        #endregion


        #region �¼�

        /// <summary>
        /// ���ط��ص�֧��֪ͨ��֤ʧ��ʱ����
        /// </summary>
        public event EventHandler<PaymentFailedEventArgs> PaymentFailed;


        /// <summary>
        /// ���ط��ص�֧��֪ͨ��֤�ɹ�ʱ����
        /// </summary>
        public event EventHandler<PaymentSucceedEventArgs> PaymentSucceed;


        /// <summary>
        /// ����֪ͨ��Ϣ�������޷�ʶ��ʱ����
        /// </summary>
        public event EventHandler<UnknownGatewayEventArgs> UnknownGateway;

        #endregion


        #region ����

        protected virtual void OnPaymentFailed(PaymentFailedEventArgs e)
        {
            EventHandler<PaymentFailedEventArgs> handler = PaymentFailed;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnPaymentSucceed(PaymentSucceedEventArgs e)
        {
            EventHandler<PaymentSucceedEventArgs> handler = PaymentSucceed;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        protected virtual void OnUnknownGateway(UnknownGatewayEventArgs e)
        {
            EventHandler<UnknownGatewayEventArgs> handler = UnknownGateway;
            if (handler != null)
            {
                handler(this, e);
            }
        }


        /// <summary>
        /// ���ղ���֤���ص�֧��֪ͨ
        /// </summary>
        public void Received()
        {
            GatewayBase gateway = NotifyProcess.GetGateway();
            if (gateway.GatewayType != GatewayType.None)
            {
                gateway.Merchant = GetMerchant(gateway.GatewayType);
                if (gateway.ValidateNotify())
                {
                    OnPaymentSucceed(new PaymentSucceedEventArgs(gateway));
                    gateway.WriteSucceedFlag();
                }
                else
                {
                    OnPaymentFailed(new PaymentFailedEventArgs(gateway));
                }
            }
            else
            {
                OnUnknownGateway(new UnknownGatewayEventArgs(gateway));
            }
        }


        /// <summary>
        /// ����̻����ݡ�����ӵ��̻������ظ������ؽ��ᱻɾ��
        /// </summary>
        /// <param name="merchant">�̻�����</param>
        public void AddMerchant(Merchant merchant)
        {
            RemoveMerchant(merchant.GatewayType);
            merchantList.Add(merchant);
        }


        /// <summary>
        /// ����̻����ݡ����ش��ڶ���̻�����ʱ���ص�һ�����޷��ҵ�����null
        /// </summary>
        /// <param name="gatewayType">��������</param>
        /// <returns>���ش��ڶ���̻�����ʱ���ص�һ�����޷��ҵ�����null</returns>
        public Merchant GetMerchant(GatewayType gatewayType)
        {
            return merchantList.FirstOrDefault(m => m.GatewayType == gatewayType);
        }


        /// <summary>
        /// ɾ���̻�����
        /// </summary>
        /// <param name="gatewayType">��������</param>
        public void RemoveMerchant(GatewayType gatewayType)
        {
            Merchant removeMerchant = merchantList.FirstOrDefault(m => m.GatewayType == gatewayType);
            if (removeMerchant != null)
            {
                merchantList.Remove(removeMerchant);
            }
        }

        #endregion

    }
}
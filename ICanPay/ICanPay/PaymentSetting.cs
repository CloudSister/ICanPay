using ICanPay.Providers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using ThoughtWorks.QRCode.Codec;

namespace ICanPay
{
    /// <summary>
    /// ������Ҫ֧���Ķ��������ݣ�����֧������URL��ַ��HTML��
    /// </summary>
    /// <remarks>
    /// ��Ϊ����֧�����صı����֧��GB2312����������֧������ͳһʹ��GB2312���롣
    /// ����Ҫ��֤���HTML�����ҳ��ΪGB2312���룬������ܻ���Ϊ���������޷���������֧��������ʶ��֧�����ص�֧��֪ͨ��
    /// ͨ���� Web.config �е� configuration/system.web �ڵ����� <globalization requestEncoding="gb2312" responseEncoding="gb2312" />
    /// ���Խ�ҳ���Ĭ�ϱ�������ΪGB2312��Ŀǰֻ��ʹ��RMB֧������������֧�����Ķ�������ؽӿ��ĵ��޸ġ�
    /// </remarks>
    public class PaymentSetting
    {

        #region �ֶ�

        GatewayBase gateway;

        #endregion


        #region ���캯��

        public PaymentSetting(GatewayType gatewayType)
        {
            gateway = CreateGateway(gatewayType);
        }


        public PaymentSetting(GatewayType gatewayType, Merchant merchant, Order order)
            : this(gatewayType)
        {
            gateway.Merchant = merchant;
            gateway.Order = order;
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
                return gateway.Merchant;
            }

            set
            {
                gateway.Merchant = value;
            }
        }


        /// <summary>
        /// ��������
        /// </summary>
        public Order Order
        {
            get
            {
                return gateway.Order;
            }

            set
            {
                gateway.Order = value;
            }
        }


        /// <summary>
        /// �Ƿ�֧�ֲ�ѯ����״̬��������֧���������ͨ��֪ͨ���ء�
        /// </summary>
        public bool CanQueryNotify
        {
            get
            {
                if (gateway is IQueryUrl || gateway is IQueryForm)
                {
                    return true;
                }

                return false;
            }
        }


        /// <summary>
        /// �Ƿ�֧��������ѯ����֧��״̬��
        /// </summary>
        public bool CanQueryNow
        {
            get
            {
                return gateway is IQueryNow;
            }
        }

        #endregion


        #region ����

        private GatewayBase CreateGateway(GatewayType gatewayType)
        {
            switch (gatewayType)
            {
                case GatewayType.Alipay:
                    {
                        return new AlipayGateway();
                    }

                case GatewayType.WeChatPay:
                    {
                        return new WeChatPayGataway();
                    }

                case GatewayType.Tenpay:
                    {
                        return new TenpayGateway();
                    }

                case GatewayType.Yeepay:
                    {
                        return new YeepayGateway();
                    }

                default:
                    {
                        return new NullGateway();
                    }
            }
        }


        /// <summary>
        /// ����������
        /// </summary>
        /// <remarks>
        /// ����������Ƕ�����Url��Form������ת����Ӧ֧��ƽ̨������Ƕ�ά�뽫�ڵ�ǰҳ�������ά��ͼƬ��
        /// </remarks>
        public void Payment()
        {
            IPaymentUrl paymentUrl = gateway as IPaymentUrl;
            if (paymentUrl != null)
            {
                HttpContext.Current.Response.Redirect(paymentUrl.BuildPaymentUrl());
                return;
            }

            IPaymentForm paymentForm = gateway as IPaymentForm;
            if (paymentForm != null)
            {
                HttpContext.Current.Response.Write(paymentForm.BuildPaymentForm());
                return;
            }

            IPaymentQRCode paymentQRCode = gateway as IPaymentQRCode;
            if (paymentQRCode != null)
            {
                WriteQRCodeImage(paymentQRCode.GetPaymentQRCodeContent());
                return;
            }

            throw new NotSupportedException(gateway.GatewayType + " û��ʵ��֧���ӿ�");
        }


        /// <summary>
        /// ��ѯ�����������Ĳ�ѯ֪ͨ������֧��֪ͨһ������ʽ���أ��ô���֧��ƽ̨֪ͨһ���ķ������ա������ѯ���������ݡ�
        /// </summary>
        public void QueryNotify()
        {
            IQueryUrl queryUrl = gateway as IQueryUrl;
            if (queryUrl != null)
            {
                HttpContext.Current.Response.Redirect(queryUrl.BuildQueryUrl());
                return;
            }

            IQueryForm queryForm = gateway as IQueryForm;
            if (queryForm != null)
            {
                HttpContext.Current.Response.Write(queryForm.BuildQueryForm());
                return;
            }

            throw new NotSupportedException(gateway.GatewayType + " û��ʵ�ֲ�ѯ�ӿ� IQueryUrl �� IQueryForm");
        }

        
        /// <summary>
        /// ��ѯ������������ö����Ĳ�ѯ�����
        /// </summary>
        /// <returns></returns>
        public bool QueryNow()
        {
            IQueryNow queryNow = gateway as IQueryNow;
            if (queryNow != null)
            {
                return queryNow.QueryNow();
            }

            throw new NotSupportedException(gateway.GatewayType + " û��ʵ�ֲ�ѯ�ӿ� IQueryNow");
        }


        /// <summary>
        /// �ڵ�ǰҳ�������ά��ͼƬ
        /// </summary>
        /// <param name="qrCodeContent">��ά������</param>
        private void WriteQRCodeImage(string qrCodeContent)
        {
            Bitmap image = BuildQRCodeImage(qrCodeContent);
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);

            HttpContext.Current.Response.ContentType = "image/x-png";
            HttpContext.Current.Response.BinaryWrite(ms.GetBuffer());
        }


        /// <summary>
        /// ������ά��ͼƬ
        /// </summary>
        /// <param name="qrCodeContent">��ά������</param>
        private Bitmap BuildQRCodeImage(string qrCodeContent)
        {
            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();
            qrCodeEncoder.QRCodeScale = 4;  // ��ά���С

            return qrCodeEncoder.Encode(qrCodeContent, Encoding.Default);
        }

        #endregion

    }
}

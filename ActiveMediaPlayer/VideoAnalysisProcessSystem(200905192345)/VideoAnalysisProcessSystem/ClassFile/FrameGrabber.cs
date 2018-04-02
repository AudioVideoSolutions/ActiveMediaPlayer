#region ��Ȩ˵��
#endregion

#region �����ռ�
//////////////////////////////////////////////////////////////////////////�Զ����ɵ������ռ�
using System;
using System.Collections.Generic;
using System.Text;
//////////////////////////////////////////////////////////////////////////�ֶ���ӵ������ռ�
using System.Drawing;   //Bitmap��,Imaging�ռ�
using System.Runtime.InteropServices;   //Marshal��
using DexterLib;    //MediaDetClass�� _AMMediaType�ṹ
using VideoAnalysisProcessSystem.ClassFile; //InvalidVideoFileException��




#endregion

namespace VideoAnalysisProcessSystem.ClassFile
{
    /// <summary>
    /// ����Ƶ�ļ���ץȡĳ��ͼƬ������������Ƶ�ļ�������ͼ
    /// ֧�ָ�ʽ��wmv,rm,avi,rmvb,mpg,mepg,asf
    /// ��Ҫʹ�õ���DexterLib�е�ý��̽������(MediaDetClass)�еĺ���
    /// ý��̽������������CLSIDΪCLSID_MediaDet��ʵ�ֵĽӿ�ΪImediaDet���������ֹ���ģʽ����Ϣ��ȡģʽ��ץͼģʽ��
    /// ����������󣬳�ʼΪ��Ϣ��ȡģʽ�����ǿ��Ե��ýӿڷ���EnterBitmapGrabMode��GetBitmapBits��WriteBitmapBits����ץͼģʽ��
    /// ��Ҫע�⣬����ץͼģʽ֮�󣬽������ٷ��ص���Ϣ��ȡģʽ��
    /// ��ý��̽����������ץͼģʽʱ,һ��Ҳ����������Ϣ��ȡģʽ��ѡ��һ����Ҫץȡ�������,Ȼ�����MediaDetClass::WriteBitmapBits()ץȡָ��ʱ�̵���һ֡
    /// </summary>
    public class FrameGrabber
    {
        //////////////////////////////////////////////////////////////////////////���к���       
        /// <summary>
        /// ����Ƶ�ļ����ض�λ�ó�ȡһ֡ͼ��
        /// </summary>
        /// <param name="videoFile">��Ƶ�ļ��ľ���·��</param>
        /// <param name="percentagePosition">��ȡ֡��λ�ã��Ϸ���Χ��0.0-1.0</param>
        /// <param name="streamLength">��Ƶ���ĳ���(��)</param>
        /// <param name="target">����ͼ��Ĵ�С�����Ϊ����Ϊԭ��Ƶ֡�Ĵ�С</param>
        /// <param name="videoformate">��Ƶ�ĸ�ʽ����Ҫ���ж��ж���asf��ʽ��Ƶ</param>
        /// <returns></returns>
        public static Bitmap GetFrameFromVideo(string videoFile, int currentFrameNo, Size target,string videoformate)
        {
            //��Ƶ�ļ��ľ���·��Ϊ��ʱ���׳��쳣
            if (string.IsNullOrEmpty(videoFile))
            {
                throw new ArgumentNullException("videoFile");
            }
            
            try
            {
                MediaDetClass mediaDet; //����ý��̽������Ķ���
                _AMMediaType mediaType;
                //����Ƶ��ʱ����ȡ��Ӧ����Ϣ
                if (OpenVideoStream(videoFile, out mediaDet, out mediaType))
                {
                    double streamLength = mediaDet.StreamLength;   //��ȡ��ǰ��Ƶ������ʱ�䳤��


                    double frameRate;      //��ȡ��Ƶ�������֡��
                    if (videoformate == "asf")
                    {
                        frameRate = 30;     //Ĭ��Ϊ30
                    }
                    else
                    {
                        frameRate = mediaDet.FrameRate;
                    }

                    double dlTotalFrames = Math.Floor(streamLength * frameRate);    //��ȡ��Ƶ��֡��(������ʵ�ʵ���֡�����1)
                    int itTotalFrames = Convert.ToInt32(dlTotalFrames); //����Ƶ��֡��ת��Ϊ����
                    if (currentFrameNo < 0 || currentFrameNo > itTotalFrames)
                    {
                        throw new ArgumentOutOfRangeException("currentFrameNo", currentFrameNo, "Invalid FrameNo!");
                    }
                    double percentagePosition = currentFrameNo / dlTotalFrames;
                    Size videoFrameSize = GetVideoSize(mediaType);
                    //��ȡ��Ƶ֡�Ĵ�С
                    if (target == Size.Empty)
                    {
                        target = videoFrameSize; 
                    }
                    else
                    {
                        target = ScaleToFit(target, videoFrameSize);
                    }

                    //ʹ��MediaDetClass::GetBitmapBits()��ȡ��Ƶ֡
                    unsafe
                    {
                        int bmpInfoHeaderSize = sizeof(WinStructs.BITMAPINFOHEADER); //
                        //��ȡͼ�񻺳�����С(����ͼ��ͷ)
                        int bufferSize = ((target.Width * target.Height) * 24) / 8 + bmpInfoHeaderSize;   //��ЧmediaDet.GetBitmapBits(0d, ref bufferSize, ref *buffer, target.Width, target.Height)
                        //�����㹻�ڴ����洢��Ƶ֡
                        IntPtr frameBuffer = Marshal.AllocHGlobal(bufferSize);
                        byte* pFrameBuffer = (byte*)frameBuffer.ToPointer();
                        //ȡ����Ƶ֡������frameBuffer(BITMAPINFOHEADER�ṹ����)
                        mediaDet.GetBitmapBits(streamLength * percentagePosition, ref bufferSize, ref *pFrameBuffer, target.Width, target.Height);

                        int bytes = target.Width * target.Height * 3;   //��ȡͼ�����ݵĴ�С
                        byte[] rgbValues = new byte[bytes];
                        Marshal.Copy(frameBuffer, rgbValues, 0, bytes); //���ݴ�InPtrָ�����ڴ渴�Ƶ�һά�ֽ�������
                        double colorTemp = 0;   //���ڱ������صĻҶ�ֵ
                        for (int i = 0; i < rgbValues.Length; i += 3)
                        {
                            //��һά�ֽ������а�BGR���У���[0]ΪB,[1]ΪG,[2]ΪR��iһ������3.
                            colorTemp = rgbValues[i + 2] * 0.299 + rgbValues[i + 1] * 0.587 + rgbValues[i] * 0.114;
                            rgbValues[i + 2] = rgbValues[i + 1] = rgbValues[i] = (byte)colorTemp;
                        }
                        Marshal.Copy(rgbValues, 0, frameBuffer, bytes); //���ݴ�һά�ֽ����鸴�Ƶ�InPtrָ�����ڴ���
                        Bitmap bmp = new Bitmap(target.Width, target.Height, target.Width * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb,new IntPtr(pFrameBuffer + bmpInfoHeaderSize) );
                        bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
                        Marshal.FreeHGlobal(frameBuffer);
                        return bmp;
                    }
                }
            }
            catch (COMException ex)
            {
                throw new InvalidVideoFileException(ErrorProcess.GetErrorMsg((uint)ex.ErrorCode), ex);
            }
            throw new InvalidVideoFileException("û����Ƶ���ļ���");
        }


        /// <summary>
        /// �����Ƶ�ļ��Ļ�����Ϣ
        /// </summary>
        /// <param name="videoFile">��Ƶ�ļ��ľ���·��</param>
        /// <returns>��Ƶ�ļ��Ļ�����Ϣ</returns>
        public static WinStructs.VIDEOBASICINFO GetVideoBasicInfo(string videoFile)
        {
            MediaDetClass mediaDet;
            _AMMediaType mediaType;
            WinStructs.VIDEOBASICINFO videoBasicInfo;
            if (OpenVideoStream(videoFile, out mediaDet, out mediaType))
            {
                videoBasicInfo.fileName = mediaDet.Filename;
                videoBasicInfo.frameRate = mediaDet.FrameRate;
                videoBasicInfo.frameRate = 20;
                videoBasicInfo.frameSize = GetVideoSize(mediaType);
                videoBasicInfo.duringTime = mediaDet.StreamLength;
                videoBasicInfo.totalFrames = Convert.ToInt32(Math.Floor(20 * mediaDet.StreamLength));
            }
            else
            {
                videoBasicInfo.fileName = "";
                videoBasicInfo.frameRate = 0.0;
                videoBasicInfo.frameSize = new Size(0, 0);
                videoBasicInfo.duringTime = 0.0;
                videoBasicInfo.totalFrames = 0;
            }
            return videoBasicInfo;
        }


        //////////////////////////////////////////////////////////////////////////˽�к���
        /// <summary>
        /// �������Ƶ�ļ��ɹ�,���������ʵ������Ϊһ���Ϸ��Ķ���
        /// </summary>
        /// <param name="videoFile">��Ƶ�ļ��ľ���·��</param>
        /// <param name="mediaDetClass">MediaDetClass���ʵ��</param>
        /// <param name="aMMediaType">��Ƶ����</param>
        /// <returns>trueΪ�ҵ���Ƶ����falseΪû�з�����Ƶ��(��֧�ִ���Ƶ��ʽ)</returns>
        private static bool OpenVideoStream(string videoFile, out MediaDetClass mediaDetClass, out _AMMediaType aMMediaType)
        {
            MediaDetClass mediaDet = new MediaDetClass();
            mediaDet.Filename = videoFile;  //������Ƶ�ļ�
            int streamsNumber = mediaDet.OutputStreams; //���������ĸ���(ֻͳ��ý������ΪMEDIATYPE_Video��MEDIATYPE_Audio����)
            //�����Ƶ�ļ���,��ץȡһ֡
            for (int i = 0; i < streamsNumber; i++)
            {
                mediaDet.CurrentStream = i;
                _AMMediaType mediaType = mediaDet.StreamMediaType;
                //������ý������Ϊ��Ƶʱ����������
                if (mediaType.majortype == MayorTypes.MEDIATYPE_Video)
                {
                    mediaDetClass = mediaDet;
                    aMMediaType = mediaType;
                    return true;
                }
            }
            //û�з�����Ƶ��ʱ�������ÿ�
            mediaDetClass = null;
            aMMediaType = new _AMMediaType();
            return false;
        }

        /// <summary>
        /// ����Ƶ�ļ���������Ƶ֡�Ĵ�С(��͸�)
        /// </summary>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        private static Size GetVideoSize(_AMMediaType mediaType)
        {
            WinStructs.VIDEOINFOHEADER videoInfo = (WinStructs.VIDEOINFOHEADER)Marshal.PtrToStructure(mediaType.pbFormat, typeof(WinStructs.VIDEOINFOHEADER));
            return new Size(videoInfo.bmiHeader.biWidth, videoInfo.bmiHeader.biHeight);
        }

        /// <summary>
        /// �滮Ϊ���ʴ�С
        /// </summary>
        /// <param name="target">Ŀ���С</param>
        /// <param name="original">Դ��С</param>
        /// <returns>ʵ�ʴ�С</returns>
        private static Size ScaleToFit(Size target, Size original)
        {
            if (target.Height * original.Width > target.Width * original.Height)
            {
                target.Height = target.Width * original.Height / original.Width;
            }
            else
            {
                target.Width = target.Height * original.Width / original.Height;
            }
            return target;
        }



        //////////////////////////////////////////////////////////////////////////��������
        /// <summary>
        /// �����Ƶ�ļ�֡��С
        /// </summary>
        /// <param name="videoFile">��Ƶ�ļ��ľ���·��</param>
        /// <returns>��Ƶ�ļ�֡��С,����Size.Empty��ʾû����Ƶ����֧�ִ���Ƶ��ʽ</returns>
        public static Size GetVideoSize(string videoFile)
        {
            MediaDetClass mediaDet;
            _AMMediaType mediaType;
            if (OpenVideoStream(videoFile, out mediaDet, out mediaType))
            {
                return GetVideoSize(mediaType);
            }

            return Size.Empty;
        }
    }
}

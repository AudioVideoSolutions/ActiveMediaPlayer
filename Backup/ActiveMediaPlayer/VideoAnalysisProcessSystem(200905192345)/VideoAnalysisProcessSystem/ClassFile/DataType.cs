#region �����ռ�
//////////////////////////////////////////////////////////////////////////�Զ����ɵ������ռ�
using System;
using System.Collections.Generic;
using System.Text;
//////////////////////////////////////////////////////////////////////////�ֶ���ӵ������ռ�
using System.Runtime.InteropServices;   //StructLayout��
using System.Drawing;   //Size����
#endregion


namespace VideoAnalysisProcessSystem.ClassFile
{
    /// <summary>
    /// ����uuids.h��������
    /// </summary>
    public class MayorTypes
    {
        public static Guid MEDIATYPE_Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
    }


    /// <summary>
    /// ����Ƶ��ص���Ϣ
    /// </summary>
    public class WinStructs
    {
        /// <summary>
        /// ��Ƶ��Ϣͷ�ṹ����������Ƶͼ���bitmap����ɫ��Ϣ
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct VIDEOINFOHEADER
        {
            public RECT rcSource;   //�˾��ο���ѡ��ԭʼ��Ƶ����һ����
            public RECT rcTarget;   //�˾��ο���ָ��Ŀ����Ƶ����
            public uint dwBitRate;  //��Ƶ���������ʣ���λΪbits/s
            public uint dwBitErrorRate; //��Ƶ�������ݴ����ʣ���λΪbits/s
            public long avgTimePerFrame;    //��Ƶ֡��ƽ����ʾʱ�䣬��100nsΪ��λ
            public BITMAPINFOHEADER bmiHeader;  //��Ƶ֡ͷ��Ϣ
        };

        /// <summary>
        /// ����
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            int left;
            int top;
            int right;
            int bottom;
        };

        /// <summary>
        /// ��Ƶ������Ϣ
        /// </summary>
        //[StructLayout(LayoutKind.Sequential)]
        public struct VIDEOBASICINFO
        {
            public string fileName;
            public double frameRate;
            public Size frameSize;
            public double duringTime;
            public int totalFrames;
        };

        /// <summary>
        /// bitmap��Ϣͷ�ṹ��������bitmapͼ���ά�Ⱥ���ɫ��Ϣ��
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize; //��ʾ�ṹ����Ҫ���ֽ���
            public int biWidth; //ͼ����
            public int biHeight;    //ͼ��߶�
            public ushort biPlanes; //��ʾĿ���豸��ƽ��������ֵ����Ϊ1
            public ushort biBitCount;   //��ʾÿ�����ص�bits��
            public uint biCompression;  //��ѹ����Ƶ��YUV��Ƶ��˵����FOURCC����
            public uint biSizeImage;    //��ʾͼ��Ĵ�С(��λΪbytes)����Ϊ��ѹ����RGBλͼʱ��0
            public int biXPelsPerMeter; //��ʾĿ���豸ˮƽ�ֱ���(pixels/meter)
            public int biYPelsPerMeter; //��ʾĿ���豸��ֱ�ֱ���(pixels/meter)
            public uint biClrUsed;  //��ʾ��ɫ���е���ɫ��
            public uint biClrImportant; //��ʾ��Ҫ����ɫ��(��ֵΪ0����������ɫ����Ҫ)
        };
    }
}

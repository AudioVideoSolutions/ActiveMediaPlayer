#region ��Ȩ˵��
#endregion


#region �����ռ�
//////////////////////////////////////////////////////////////////////////�Զ����ɵ������ռ�
using System;
using System.Collections.Generic;
using System.Text;
//////////////////////////////////////////////////////////////////////////�ֶ���ӵ������ռ�
using System.Drawing; //Size, Bitmap
using System.Drawing.Imaging;   //PixelFormat, BitmapData, ImageLockMode, LockBits, UnlockBits
using System.Runtime.InteropServices; //[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
using System.ComponentModel;  //[Category("***"), ReadOnly(true), Description("***")]
using DirectShowLib;  //DsError, MediaType, AMMediaType, FormatType, WaveFormatEx, VideoInfoHeader   
using DirectShowLib.DES;  //IMediaDet
#endregion


namespace VideoAnalysisProcessSystem.ClassFile
{
    /// <summary>
    /// ����ý��ĸ�������
    /// </summary>
    public class MediaProperty
    {
        internal string fileName;
        internal Guid audioSubType;
        internal int channels;
        internal float samplesPerSec;
        internal int bitsPerSample;
        internal TimeSpan audioLength;
        internal Guid videoSubType;
        internal Size resolution;
        internal int bitsPerPixel;
        internal string fourCC;
        internal TimeSpan videoLength;
        internal Bitmap snapshot;

        [Category("General"), ReadOnly(true), Description("The file name with its path")]
        public string FileName
        {
            get { return fileName; }
        }

        [Category("Audio"), ReadOnly(true), Description("Audio subtype GUID")]
        public Guid AudioSubType
        {
            get { return audioSubType; }
        }

        [Category("Audio"), ReadOnly(true), Description("Audio channel count")]
        public int Channels
        {
            get { return channels; }
        }

        [Category("Audio"), ReadOnly(true), Description("Samples per second in kHz")]
        public float SamplesPerSec
        {
            get { return samplesPerSec; }
        }

        [Category("Audio"), ReadOnly(true), Description("How many bits are used per samples")]
        public int BitsPerSample
        {
            get { return bitsPerSample; }
        }

        [Category("Audio"), ReadOnly(true), Description("The audio stream length")]
        public TimeSpan AudioLength
        {
            get { return audioLength; }
        }

        [Category("Video"), ReadOnly(true), Description("Video subtype GUID")]
        public Guid VideoSubType
        {
            get { return videoSubType; }
        }

        [Category("Video"), ReadOnly(true), Description("Specifies the width and height of the bitmap, in pixels")]
        public Size Resolution
        {
            get { return resolution; }
        }

        [Category("Video"), ReadOnly(true), Description("Specifies the number of bits per pixel (bpp)")]
        public int BitsPerPixel
        {
            get { return bitsPerPixel; }
        }

        [Category("Video"), ReadOnly(true), Description("The Video's FOURCC code")]
        public string FourCC
        {
            get { return fourCC; }
        }

        [Category("Video"), ReadOnly(true), Description("The video stream length")]
        public TimeSpan VideoLength
        {
            get { return videoLength; }
        }

        [Category("Video"), ReadOnly(true), Description("A snapshot of the video stream at half its duration")]
        public Bitmap Snapshot
        {
            get { return snapshot; }
        }

    }

    /// <summary>
    /// ����ý������
    /// </summary>
    public sealed class MediaDescription
    {
        /// <summary>
        /// ��ȡý��ĸ�������
        /// </summary>
        /// <param name="fileName">��Ƶ�ļ���(�����ļ�·��)</param>
        /// <returns>ý��ĸ�������</returns>
         public static MediaProperty GetMediaProperty(string fileName)
         {
             int hr = 0;    //���պ�������ֵ���Դ��жϵ��ú����ɹ����
             MediaProperty mediaProperty = new MediaProperty();
             IMediaDet mediaDet = null;

             try
             {
                 mediaDet = (IMediaDet)new MediaDet();  //����DirectShow��ý��̽�����
                 hr = mediaDet.put_Filename(fileName);  //����ý��̽�������ļ����������ļ�·����
                 if (hr < 0)
                 {
                     DsError.ThrowExceptionForHR(hr);
                 }
                 mediaProperty.fileName = fileName;

                 int streamCount;
                 hr = mediaDet.get_OutputStreams(out streamCount);
                 DsError.ThrowExceptionForHR(hr);

                 for (int i = 0; i < streamCount; i++)
                 {
                     hr = mediaDet.put_CurrentStream(i);
                     DsError.ThrowExceptionForHR(hr);

                     Guid streamType;
                     hr = mediaDet.get_StreamType(out streamType);
                     DsError.ThrowExceptionForHR(hr);

                     if (streamType == MediaType.Audio)
                     {
                         UpdateAudioPart(mediaDet, mediaProperty);
                     }
                     else if (streamType == MediaType.Video)
                     {
                         UpdateVideoPart(mediaDet, mediaProperty);
                     }
                     else
                         continue;
                 }

                 if (mediaProperty.videoSubType != Guid.Empty)
                     mediaProperty.snapshot = GetSnapshot(mediaDet, mediaProperty.resolution.Width, mediaProperty.resolution.Height, mediaProperty.videoLength.TotalSeconds / 2);
             }
             finally
             {
                 if (mediaDet != null)
                     Marshal.ReleaseComObject(mediaDet);
             }

             return mediaProperty;
         }

        private static void UpdateAudioPart(IMediaDet mediaDet, MediaProperty mediaProperty)
         {
             int hr = 0;
             AMMediaType mediaType = new AMMediaType();

             hr = mediaDet.get_StreamMediaType(mediaType);
             DsError.ThrowExceptionForHR(hr);

             mediaProperty.audioSubType = mediaType.subType;

             double streamLength;
             hr = mediaDet.get_StreamLength(out streamLength);
             DsError.ThrowExceptionForHR(hr);

             mediaProperty.audioLength = TimeSpan.FromSeconds(streamLength);

             if (mediaType.formatType == FormatType.WaveEx)
             {
                 WaveFormatEx waveFormatEx = (WaveFormatEx)Marshal.PtrToStructure(mediaType.formatPtr, typeof(WaveFormatEx));
                 mediaProperty.channels = waveFormatEx.nChannels;
                 mediaProperty.samplesPerSec = ((float)waveFormatEx.nSamplesPerSec) / 1000;
                 mediaProperty.bitsPerSample = waveFormatEx.wBitsPerSample;
             }
         }

        private static void UpdateVideoPart(IMediaDet mediaDet, MediaProperty mediaProperty)
         {
             int hr = 0;
             AMMediaType mediaType = new AMMediaType();

             hr = mediaDet.get_StreamMediaType(mediaType);
             DsError.ThrowExceptionForHR(hr);

             mediaProperty.videoSubType = mediaType.subType;

             double streamLength;
             hr = mediaDet.get_StreamLength(out streamLength);
             DsError.ThrowExceptionForHR(hr);

             mediaProperty.videoLength = TimeSpan.FromSeconds(streamLength);

             if (mediaType.formatType == FormatType.VideoInfo)
             {
                 VideoInfoHeader videoHeader = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));

                 mediaProperty.resolution = new Size(videoHeader.BmiHeader.Width, videoHeader.BmiHeader.Height);
                 mediaProperty.bitsPerPixel = videoHeader.BmiHeader.BitCount;
                 mediaProperty.fourCC = FourCCToString(videoHeader.BmiHeader.Compression);
             }
         }

         private static string FourCCToString(int fourcc)
         {
             byte[] bytes = new byte[4];

             bytes[0] = (byte)(fourcc & 0x000000ff); fourcc = fourcc >> 8;
             bytes[1] = (byte)(fourcc & 0x000000ff); fourcc = fourcc >> 8;
             bytes[2] = (byte)(fourcc & 0x000000ff); fourcc = fourcc >> 8;
             bytes[3] = (byte)(fourcc & 0x000000ff);

             return Encoding.ASCII.GetString(bytes);
         }

         private static Bitmap GetSnapshot(IMediaDet mediaDet, int width, int height, double position)
         {
             int hr = 0;
             Bitmap bitmap = null;
             int bufferSize = 0;
             IntPtr buffer = IntPtr.Zero;

             try
             {
                 hr = mediaDet.GetBitmapBits(position, out bufferSize, IntPtr.Zero, width, height);
                 if (hr == 0)
                 {
                     buffer = Marshal.AllocCoTaskMem(bufferSize);
                     hr = mediaDet.GetBitmapBits(position, out bufferSize, buffer, width, height);

                     BitmapInfoHeader bitmapHeader = (BitmapInfoHeader)Marshal.PtrToStructure(buffer, typeof(BitmapInfoHeader));
                     IntPtr bitmapData;

                     if (IntPtr.Size == 4)
                         bitmapData = new IntPtr(buffer.ToInt32() + bitmapHeader.Size);
                     else
                         bitmapData = new IntPtr(buffer.ToInt64() + bitmapHeader.Size);

                     bitmap = new Bitmap(bitmapHeader.Width, bitmapHeader.Height, PixelFormat.Format24bppRgb);
                     BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmapHeader.Width, bitmapHeader.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                     /*
                     for (int i = 0; i < width * height * 3 ; i++)
                     {
                       byte b = Marshal.ReadByte(bitmapData, i);
                       Marshal.WriteByte(bmpData.Scan0, i, b);
                     }
                     */

                     CopyMemory(bmpData.Scan0, bitmapData, width * height * 3);
                     bitmap.UnlockBits(bmpData);

                     bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
                 }
             }
             finally
             {
                 if (buffer != IntPtr.Zero)
                     Marshal.FreeCoTaskMem(buffer);
             }
             return bitmap;
         }

         [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
         private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

    }
}

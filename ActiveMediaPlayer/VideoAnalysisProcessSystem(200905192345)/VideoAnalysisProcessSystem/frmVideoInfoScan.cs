#region �����ռ�
//////////////////////////////////////////////////////////////////////////�Զ����ɵ������ռ�
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//////////////////////////////////////////////////////////////////////////�ֶ���ӵ������ռ�
using VideoAnalysisProcessSystem.ClassFile; //MediaProperty, MediaDescription, WinStructs, FrameGrabber
using DexterLib;    //MediaDetClass�� _AMMediaType�ṹ
using Microsoft.DirectX.AudioVideoPlayback;

#endregion

namespace VideoAnalysisProcessSystem
{
    public partial class frmVideoInfoScan : Form
    {
        private string videoFileName = "";
        public frmVideoInfoScan()
        {
            InitializeComponent();
        }

        public frmVideoInfoScan(string fileName)
        {
            InitializeComponent();
            videoFileName = fileName;
        }

        /// <summary>
        /// ��Ƶ��Ϣ����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmVideoInfoScan_Load(object sender, EventArgs e)
        {
            try
            {
                //////////////////////////////////////////////////////////////////////////�ڿؼ�����ʾ��Ƶ�ļ��ĸ�������
                MediaProperty mediaProperty = MediaDescription.GetMediaProperty(videoFileName);
                propertyGrid_Media.SelectedObject = mediaProperty;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                this.Close();
                this.Dispose();
                GC.Collect();
            }
        }

        /// <summary>
        /// ��Ƶ��Ϣ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonVideoInfoScan_Click(object sender, EventArgs e)
        {
            try
            {
                Microsoft.DirectX.AudioVideoPlayback.Video video = new Video(videoFileName);
                double frameRate = 1/video.AverageTimePerFrame;
                //////////////////////////////////////////////////////////////////////////�ڿؼ��н�����Ƶ���
                WinStructs.VIDEOBASICINFO videoBasicInfo;
                videoBasicInfo = FrameGrabber.GetVideoBasicInfo(videoFileName);
                if (videoBasicInfo.totalFrames == 0)
                {
                    throw new ArgumentException("Total Frames is 0!");
                }
                int intervalFrameNum = Convert.ToInt32(textBox_IntervalNum.Text.Trim());
                for (int i = 1; i <= videoBasicInfo.totalFrames; i += intervalFrameNum)
                {
                    this.pictureBox1.Refresh();
                    this.pictureBox1.Image = FrameGrabber.GetFrameFromVideo(videoBasicInfo.fileName, i, Size.Empty, string.Empty);
                    this.pictureBox1.Image.Save("D:\\frames\\" + i.ToString() + ".bmp");
                }
            }
            catch (VideoAnalysisProcessSystem.ClassFile.InvalidVideoFileException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (StackOverflowException)
            {
                MessageBox.Show("The target image size is too big", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// �ر���Ƶ����Ի���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            GC.Collect();
        }     
    }
}
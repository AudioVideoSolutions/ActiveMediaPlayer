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

#endregion


namespace VideoAnalysisProcessSystem
{
    public partial class frmMainForm : Form
    {
        public frmMainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ��Ƶ�ļ�/�ļ���Ϣ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_FileInfoScan_Click(object sender, EventArgs e)
        {
            openFileDialog_VideoFile.Filter = "AVI File|*.avi|MPG File|*.mpg|WMV File|*.wmv|MPEG File|*.mpeg |RM File|*.rm|RMVB File|*.rmvb|ASF File|*.asf|All File|*.*";
            openFileDialog_VideoFile.InitialDirectory = "F:\\";
            openFileDialog_VideoFile.Title = "Open Video File";
            if (openFileDialog_VideoFile.ShowDialog() == DialogResult.OK)
            {
                frmVideoInfoScan videoInfoScan = new frmVideoInfoScan(openFileDialog_VideoFile.FileName);
                videoInfoScan.ShowDialog(); 
            }           
        }

        private void MenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
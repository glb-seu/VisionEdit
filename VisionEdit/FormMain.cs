﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonMethods;
using VisionEdit.FormLib;
using WeifenLuo.WinFormsUI.Docking;

namespace VisionEdit
{
    public partial class FormMain : Form
    {
        #region 变量定义
        private string m_DockPath { get; set; } = string.Empty;
        public static FormImageWindow myFormImageWindow = new FormImageWindow();
        public static FormJobManage myFormJobManage = new FormJobManage();
        public static FormLog myFormLog = new FormLog();
        public FormToolBox myFormToolBox = new FormToolBox(myFormLog, myFormJobManage);
        #endregion

        public FormMain()
        {
            InitializeComponent();
            m_DockPath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "DockPanel.config");
            InitDockPanel();
            _instance = this;
        }
        /// <summary>
        /// 窗体对象实例
        /// </summary>
        private static FormMain _instance;
        public static FormMain Instance
        {
            get
            {
                lock(_instance)
                {
                    if (_instance == null)
                        _instance = new FormMain();
                    return _instance;
                }
            }
        }


        private void FormMain_Load(object sender, EventArgs e)
        {
            // 窗体加载到主窗体
            myFormToolBox.Show(this.dockPanel1, DockState.DockLeft);
            myFormJobManage.Show(this.dockPanel1, DockState.DockRight);
            myFormImageWindow.Show(this.dockPanel1, DockState.Document);
            myFormLog.Show(this.dockPanel1, DockState.DockBottom);
            // 初始化JOB
            CreateInitJob();
        }

        #region 按照配置文件初始化Dockpanel
        private void InitDockPanel()
        {
            try
            {
                //根据配置文件动态加载浮动窗体
                this.dockPanel1.LoadFromXml(this.m_DockPath, delegate (string persistString)
                {
                    //功能窗体
                    if (persistString == typeof(FormToolBox).ToString())
                    {
                        return myFormToolBox;
                    }
                    if (persistString == typeof(FormJobManage).ToString())
                    {
                        return myFormJobManage;
                    }
                    if (persistString == typeof(FormLog).ToString())
                    {
                        return myFormLog;
                    }
                    if (persistString == typeof(FormImageWindow).ToString())
                    {
                        return myFormImageWindow;
                    }
                    //主框架之外的窗体不显示
                    return null;
                });
            }
            catch (Exception)
            {
                //配置文件不存在或配置文件有问题时 按系统默认规则加载子窗体
                myFormToolBox.Show(this.dockPanel1, DockState.DockLeft);
                myFormJobManage.Show(this.dockPanel1, DockState.DockRight);
                myFormLog.Show(this.dockPanel1, DockState.DockBottom);
                myFormImageWindow.Show(this.dockPanel1, DockState.Document);
            }
        }
        #endregion

        /// <summary>
        /// 关闭时保存当前panel配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(File.Exists(m_DockPath))
            {
                dockPanel1.SaveAsXml(this.m_DockPath);
            } 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.lbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void CreateInitJob()
        {
            // 初始化加载默认Job
            myFormJobManage.tabControl1.TabPages.Add("defultJob");
            GlobalParams.myJobTreeView = new TreeView();
            GlobalParams.myVisionJob = new VisionJob(GlobalParams.myJobTreeView, myFormLog, myFormImageWindow, "defultJob");
            myFormJobManage.tabControl1.TabPages[0].Controls.Add(GlobalParams.myJobTreeView);
            GlobalParams.myJobTreeView.Dock = DockStyle.Fill;
            GlobalParams.myJobTreeView.ImageList = myFormToolBox.imageListTool;
            GlobalParams.myJobTreeView.Font = new Font("微软雅黑", 9, FontStyle.Bold);

            GlobalParams.myJobTreeView.Scrollable = true;
            GlobalParams.myJobTreeView.ItemHeight = 20;
            GlobalParams.myJobTreeView.ShowLines = false;
            GlobalParams.myJobTreeView.AllowDrop = true;
            //myTreeView.ImageList = Job.imageList;

            // 在窗体UI出现变化时，更新画线
            GlobalParams.myJobTreeView.AfterSelect += GlobalParams.myVisionJob.tvw_job_AfterSelect;
            GlobalParams.myJobTreeView.ChangeUICues += GlobalParams.myVisionJob.MyJobTreeView_ChangeUICues;
            myFormJobManage.SizeChanged += GlobalParams.myVisionJob.tbc_jobs_SelectedIndexChanged;
            //节点间拖拽
            GlobalParams.myJobTreeView.ItemDrag += new ItemDragEventHandler(GlobalParams.myVisionJob.TvwJob_ItemDrag);
            GlobalParams.myJobTreeView.DragEnter += new DragEventHandler(GlobalParams.myVisionJob.TvwJob_DragEnter);
            GlobalParams.myJobTreeView.DragDrop += new DragEventHandler(GlobalParams.myVisionJob.TvwJob_DragDrop);

            //以下事件为画线事件
            GlobalParams.myJobTreeView.MouseMove += GlobalParams.myVisionJob.DrawLineWithoutRefresh;
            GlobalParams.myJobTreeView.AfterExpand += GlobalParams.myVisionJob.Draw_Line;
            GlobalParams.myJobTreeView.AfterCollapse += GlobalParams.myVisionJob.Draw_Line;
            // 在流程节点上操作时
            GlobalParams.myJobTreeView.MouseDoubleClick += OperateJob.TreeViewJob_DoubleClick; ;
            GlobalParams.myJobTreeView.MouseClick += GlobalParams.myVisionJob.tvw_job_MouseClick;
            Application.DoEvents();

            //默认添加ImageAcquistionTool工具
            myFormToolBox.Add_Tool(ToolType.HalconTool);
            myFormToolBox.Add_Tool(ToolType.FindLine);
            //默认选中第一个工具节点
            GlobalParams.myJobTreeView.SelectedNode = GlobalParams.myJobTreeView.Nodes[0];

            //展开已默认添加的工具的输入输出项
            GlobalParams.myJobTreeView.ExpandAll();
        }

        private void MyJobTreeView_DoubleClick(object sender, EventArgs e)
        {
            
        }
    }
}

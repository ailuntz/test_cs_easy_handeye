using Microsoft.VisualBasic.ApplicationServices;
using Opc.Ua;
using Opc.Ua.Client;
using pho.api.csharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using test_cs_easy_handeye.img_ico;
using test_cs_easy_handeye;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using Frame = pho.api.csharp.Frame;
using HandEyeCalibration;
using static HandEyeCalibration.ConversionMatrix;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Diagnostics;
using static OpenCvSharp.ML.DTrees;

using OpenCvSharp;
using Size = OpenCvSharp.Size;
using Microsoft.VisualBasic;
using static HandEyeCalibration.Definetype;
using static HandEyeCalibration.ConversionAngle;
using static HandEyeCalibration.HandEyeCalibration;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static OpenCvSharp.Stitcher;
using OpenCvSharp.Flann;
using System.Drawing.Drawing2D;
using static System.Net.WebRequestMethods;

namespace test_cs_easy_handeye
{
    public partial class Form1 : Form
    {

        #region Constructor
        public Form1()
        {


            InitializeComponent();
            Icon = ClientUtils.GetAppIcon();
        }
        #endregion

        #region opc

        #region Load Show Close

        private void Form1_Load(object sender, EventArgs e)
        {

            BrowseNodesTV.Enabled = false;

            BrowseNodesTV.ImageList = new ImageList();

            //Image ico_Class_489 = Image.FromFile("C:\\Users\\Administrator\\Desktop\\test_cs_easy_handeye2\\img_ico\\Class_489.png");

            BrowseNodesTV.ImageList.Images.Add("Class_489", ImgIco.ico_Class_489);

            BrowseNodesTV.ImageList.Images.Add("ClassIcon", ImgIco.ico_ClassIcon);

            BrowseNodesTV.ImageList.Images.Add("brackets", ImgIco.ico_brackets);

            BrowseNodesTV.ImageList.Images.Add("VirtualMachine", ImgIco.ico_VirtualMachine);

            BrowseNodesTV.ImageList.Images.Add("Enum_582", ImgIco.ico_Enum_582);

            BrowseNodesTV.ImageList.Images.Add("Method_636", ImgIco.ico_Method_636);

            BrowseNodesTV.ImageList.Images.Add("Module_648", ImgIco.ico_Module_648);

            BrowseNodesTV.ImageList.Images.Add("Loading", ImgIco.ico_Loading);

            // 判断是否允许更改
            if (!string.IsNullOrEmpty(textBox_Address.Text)) textBox1.ReadOnly = true;
            // Opc Ua 服务的初始化
            OpcUaClientInitialization();
        }


        private string GetImageKeyFromDescription(ReferenceDescription target, NodeId sourceId)
        {
            if (target.NodeClass == NodeClass.Variable)
            {
                DataValue dataValue = m_OpcUaClient.ReadNode((NodeId)target.NodeId);

                if (dataValue.WrappedValue.TypeInfo != null)
                {
                    if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                    {
                        return "Enum_582";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                    {
                        return "brackets";
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                    {
                        return "Module_648";
                    }
                    else
                    {
                        return "ClassIcon";
                    }
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Object)
            {
                if (sourceId == ObjectIds.ObjectsFolder)
                {
                    return "VirtualMachine";
                }
                else
                {
                    return "ClassIcon";
                }
            }
            else if (target.NodeClass == NodeClass.Method)
            {
                return "Method_636";
            }
            else
            {
                return "ClassIcon";
            }
        }

        private void FormBrowseServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_OpcUaClient.Disconnect();
        }

        #endregion

        #region   OPC UA client


        /// <summary>
        /// Opc客户端的核心类
        /// </summary>
        private OpcUaClient m_OpcUaClient = null;

        /// <summary>
        /// 初始化
        /// </summary>
        private void OpcUaClientInitialization()
        {
            m_OpcUaClient = new OpcUaClient();
            m_OpcUaClient.OpcStatusChange += M_OpcUaClient_OpcStatusChange1; ;
            m_OpcUaClient.ConnectComplete += M_OpcUaClient_ConnectComplete;
        }

        /// <summary>
        /// 连接服务器结束后马上浏览根节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_ConnectComplete(object sender, EventArgs e)
        {
            try
            {
                OpcUaClient client = (OpcUaClient)sender;
                if (client.Connected)
                {
                    // populate the browse view.
                    PopulateBranch(ObjectIds.ObjectsFolder, BrowseNodesTV.Nodes);
                    BrowseNodesTV.Enabled = true;
                }
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
            }
        }

        /// <summary>
        /// OPC 客户端的状态变化后的消息提醒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void M_OpcUaClient_OpcStatusChange1(object sender, OpcUaStatusEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    M_OpcUaClient_OpcStatusChange1(sender, e);
                }));
                return;
            }

            if (e.Error)
            {
                toolStripStatusLabel1.BackColor = Color.Red;
            }
            else
            {
                toolStripStatusLabel1.BackColor = SystemColors.Control;
            }

            toolStripStatusLabel_opc.Text = e.ToString();
        }


        private ReferenceDescriptionCollection GetReferenceDescriptionCollection(NodeId sourceId)
        {
            TaskCompletionSource<ReferenceDescriptionCollection> task = new TaskCompletionSource<ReferenceDescriptionCollection>();

            // find all of the components of the node.
            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.Aggregates;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.View | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            // find all nodes organized by the node.
            BrowseDescription nodeToBrowse2 = new BrowseDescription();

            nodeToBrowse2.NodeId = sourceId;
            nodeToBrowse2.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse2.ReferenceTypeId = ReferenceTypeIds.Organizes;
            nodeToBrowse2.IncludeSubtypes = true;
            nodeToBrowse2.NodeClassMask = (uint)(NodeClass.Object | NodeClass.Variable | NodeClass.Method | NodeClass.View | NodeClass.ReferenceType | NodeClass.ObjectType | NodeClass.VariableType | NodeClass.DataType);
            nodeToBrowse2.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);
            nodesToBrowse.Add(nodeToBrowse2);

            // fetch references from the server.
            ReferenceDescriptionCollection references = FormUtils.Browse(m_OpcUaClient.Session, nodesToBrowse, false);
            return references;
        }

        /// <summary>
        /// 0:NodeClass  1:Value  2:AccessLevel  3:DisplayName  4:Description
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        private DataValue[] ReadOneNodeFiveAttributes(List<NodeId> nodeIds)
        {
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
            foreach (var nodeId in nodeIds)
            {
                NodeId sourceId = nodeId;
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.NodeClass
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.Value
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.AccessLevel
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.DisplayName
                });
                nodesToRead.Add(new ReadValueId()
                {
                    NodeId = sourceId,
                    AttributeId = Attributes.Description
                });
            }

            // read all values.
            m_OpcUaClient.Session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out DataValueCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results.ToArray();
        }


        /// <summary>
        /// 读取一个节点的指定属性
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        private DataValue ReadNoteDataValueAttributes(NodeId nodeId, uint attribute)
        {
            NodeId sourceId = nodeId;
            ReadValueIdCollection nodesToRead = new ReadValueIdCollection();


            ReadValueId nodeToRead = new ReadValueId();
            nodeToRead.NodeId = sourceId;
            nodeToRead.AttributeId = attribute;
            nodesToRead.Add(nodeToRead);

            int startOfProperties = nodesToRead.Count;

            // find all of the pror of the node.
            BrowseDescription nodeToBrowse1 = new BrowseDescription();

            nodeToBrowse1.NodeId = sourceId;
            nodeToBrowse1.BrowseDirection = BrowseDirection.Forward;
            nodeToBrowse1.ReferenceTypeId = ReferenceTypeIds.HasProperty;
            nodeToBrowse1.IncludeSubtypes = true;
            nodeToBrowse1.NodeClassMask = 0;
            nodeToBrowse1.ResultMask = (uint)BrowseResultMask.All;

            BrowseDescriptionCollection nodesToBrowse = new BrowseDescriptionCollection();
            nodesToBrowse.Add(nodeToBrowse1);

            // fetch property references from the server.
            ReferenceDescriptionCollection references = FormUtils.Browse(m_OpcUaClient.Session, nodesToBrowse, false);

            if (references == null)
            {
                return null;
            }

            for (int ii = 0; ii < references.Count; ii++)
            {
                // ignore external references.
                if (references[ii].NodeId.IsAbsolute)
                {
                    continue;
                }

                ReadValueId nodeToRead2 = new ReadValueId();
                nodeToRead2.NodeId = (NodeId)references[ii].NodeId;
                nodeToRead2.AttributeId = Attributes.Value;
                nodesToRead.Add(nodeToRead2);
            }

            // read all values.
            DataValueCollection results = null;
            DiagnosticInfoCollection diagnosticInfos = null;

            m_OpcUaClient.Session.Read(
                null,
                0,
                TimestampsToReturn.Neither,
                nodesToRead,
                out results,
                out diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToRead);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToRead);

            return results[0];
        }


        #endregion

        #region Menu Click Event

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // press exit menu button
            Close();
        }



        #endregion

        #region Press Connect Click Button

        private async void button13_Click(object sender, EventArgs e)
        {
            // connect to server
            //using (FormConnectSelect formConnectSelect = new FormConnectSelect(m_OpcUaClient))
            //{
            //if (formConnectSelect.ShowDialog() == DialogResult.OK)
            //{
            try
            {
                // 匿名登录
                UserIdentityToken UserIdentityTokentemp = new AnonymousIdentityToken();
                m_OpcUaClient.UserIdentity = new UserIdentity(UserIdentityTokentemp);
                await m_OpcUaClient.ConnectServer(textBox_Address.Text);
                button13.BackColor = Color.LimeGreen;
                CommunicationsuccessfulRobot = true;
                label42.Text = "已连接";
                label6.Text = "已连接";
                label6.BackColor = Color.LimeGreen;
                label42.BackColor = Color.LimeGreen;

            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(Text, ex);
            }
            //}
            //}
        }

        private async void button14_Click(object sender, EventArgs e)
        {
            // connect to server
            //using (FormConnectSelect formConnectSelect = new FormConnectSelect(m_OpcUaClient))
            //{
            //if (formConnectSelect.ShowDialog() == DialogResult.OK)
            //{
            try
            {
                // 用户名密码登录
                m_OpcUaClient.UserIdentity = new UserIdentity(textBox19.Text, textBox20.Text);
                await m_OpcUaClient.ConnectServer(textBox_Address.Text);
                button14.BackColor = Color.LimeGreen;
                CommunicationsuccessfulRobot = true;
                label42.Text = "已连接";
                label6.Text = "已连接";
                label42.BackColor = Color.LimeGreen;
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(Text, ex);
            }
            //}
            //}
        }


        private async void button15_Click(object sender, EventArgs e)
        {
            // 证书登录
            try
            {
                X509Certificate2 certificate = new X509Certificate2(textBox22.Text, textBox21.Text, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
                m_OpcUaClient.UserIdentity = new UserIdentity(certificate);
                DialogResult = DialogResult.OK;
                await m_OpcUaClient.ConnectServer(textBox_Address.Text);
                button15.BackColor = Color.LimeGreen;
                CommunicationsuccessfulRobot = true;
                label42.Text = "已连接";
                label6.Text = "已连接";
                label42.BackColor = Color.LimeGreen;
                return;
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }


        #endregion

        #region 填入分支

        /// <summary>
        /// Populates the branch in the tree view.
        /// </summary>
        /// <param name="sourceId">The NodeId of the Node to browse.</param>
        /// <param name="nodes">The node collect to populate.</param>
        private async void PopulateBranch(NodeId sourceId, TreeNodeCollection nodes)
        {
            nodes.Clear();
            nodes.Add(new TreeNode("Browsering...", 7, 7));
            // fetch references from the server.
            TreeNode[] listNode = await Task.Run(() =>
            {
                ReferenceDescriptionCollection references = GetReferenceDescriptionCollection(sourceId);
                List<TreeNode> list = new List<TreeNode>();
                if (references != null)
                {
                    // process results.
                    for (int ii = 0; ii < references.Count; ii++)
                    {
                        ReferenceDescription target = references[ii];
                        TreeNode child = new TreeNode(Utils.Format("{0}", target));

                        child.Tag = target;
                        string key = GetImageKeyFromDescription(target, sourceId);
                        child.ImageKey = key;
                        child.SelectedImageKey = key;

                        // if (target.NodeClass == NodeClass.Object || target.NodeClass == NodeClass.Unspecified || expanded)
                        // {
                        //     child.Nodes.Add(new TreeNode());
                        // }

                        //if (!checkBox1.Checked)
                        //{
                        if (GetReferenceDescriptionCollection((NodeId)target.NodeId).Count > 0)
                        {
                            child.Nodes.Add(new TreeNode());
                        }
                        //}
                        //else
                        //{
                        // child.Nodes.Add(new TreeNode());
                        //}


                        list.Add(child);
                    }
                }

                return list.ToArray();
            });


            // update the attributes display.
            // DisplayAttributes(sourceId);
            nodes.Clear();
            nodes.AddRange(listNode.ToArray());
        }



        #endregion

        #region 节点打开的时候操作

        private void BrowseNodesTV_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode tn = BrowseNodesTV.GetNodeAt(e.X, e.Y);
                if (tn != null)
                {
                    BrowseNodesTV.SelectedNode = tn;
                }
            }
        }

        private void BrowseNodesTV_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {

                // check if node has already been expanded once.
                if (e.Node.Nodes.Count != 1)
                {
                    return;
                }

                if (e.Node.Nodes.Count > 0)
                {
                    if (e.Node.Nodes[0].Text != String.Empty)
                    {
                        return;
                    }
                }

                // get the source for the node.
                ReferenceDescription reference = e.Node.Tag as ReferenceDescription;

                if (reference == null || reference.NodeId.IsAbsolute)
                {
                    e.Cancel = true;
                    return;
                }

                // populate children.
                PopulateBranch((NodeId)reference.NodeId, e.Node.Nodes);
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(this.Text, exception);
            }
        }





        private void BrowseNodesTV_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                RemoveAllSubscript();
                // get the source for the node.
                ReferenceDescription reference = e.Node.Tag as ReferenceDescription;

                if (reference == null || reference.NodeId.IsAbsolute)
                {
                    return;
                }

                // populate children.
                ShowMember((NodeId)reference.NodeId);
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
            }
        }


        private void ClearDataGridViewRows(int index)
        {
            for (int i = dataGridView1.Rows.Count - 1; i >= index; i--)
            {
                if (i >= 0)
                {
                    dataGridView1.Rows.RemoveAt(i);
                }
            }
        }

        #endregion

        #region 点击树节点后在数据表显示

        /// <summary>
        /// 点击了节点名称前的内容进行复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void label2_Click(object sender, EventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(textBox_nodeId.Text))
        //    {
        //        Clipboard.SetText(textBox_nodeId.Text);
        //    }
        //}
        private async void ShowMember(NodeId sourceId)
        {

            textBox_nodeId.Text = sourceId.ToString();

            // dataGridView1.Rows.Clear();
            int index = 0;
            ReferenceDescriptionCollection references;
            try
            {
                references = await Task.Run(() =>
                {
                    return GetReferenceDescriptionCollection(sourceId);
                });
            }
            catch (Exception exception)
            {
                ClientUtils.HandleException(Text, exception);
                return;
            }


            if (references?.Count > 0)
            {
                // 获取所有要读取的子节点
                List<NodeId> nodeIds = new List<NodeId>();
                for (int ii = 0; ii < references.Count; ii++)
                {
                    ReferenceDescription target = references[ii];
                    nodeIds.Add((NodeId)target.NodeId);
                }

                DateTime dateTimeStart = DateTime.Now;

                // 获取所有的值
                DataValue[] dataValues = await Task.Run(() =>
                {
                    return ReadOneNodeFiveAttributes(nodeIds);
                });

                label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";

                // 显示
                for (int jj = 0; jj < dataValues.Length; jj += 5)
                {
                    AddDataGridViewNewRow(dataValues, jj, index++, nodeIds[jj / 5]);
                }

            }
            else
            {
                // 子节点没有数据的情况
                try
                {
                    DateTime dateTimeStart = DateTime.Now;
                    DataValue dataValue = m_OpcUaClient.ReadNode(sourceId);

                    if (dataValue.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
                    {
                        // 数组显示
                        AddDataGridViewArrayRow(sourceId, out index);
                    }
                    else
                    {
                        // 显示单个数本身
                        label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";
                        AddDataGridViewNewRow(ReadOneNodeFiveAttributes(new List<NodeId>() { sourceId }), 0, index++, sourceId);
                    }
                }
                catch (Exception exception)
                {
                    ClientUtils.HandleException(Text, exception);
                    return;
                }
            }

            ClearDataGridViewRows(index);

        }


        private void AddDataGridViewNewRow(DataValue[] dataValues, int startIndex, int index, NodeId nodeId)
        {
            // int index = dataGridView1.Rows.Add();
            while (index >= dataGridView1.Rows.Count)
            {
                dataGridView1.Rows.Add();
            }
            DataGridViewRow dgvr = dataGridView1.Rows[index];
            dgvr.Tag = nodeId;

            if (dataValues[startIndex].WrappedValue.Value == null) return;
            NodeClass nodeclass = (NodeClass)dataValues[startIndex].WrappedValue.Value;

            dgvr.Cells[1].Value = dataValues[3 + startIndex].WrappedValue.Value;
            dgvr.Cells[5].Value = dataValues[4 + startIndex].WrappedValue.Value;
            dgvr.Cells[4].Value = GetDiscriptionFromAccessLevel(dataValues[2 + startIndex]);

            if (nodeclass == NodeClass.Object)
            {
                dgvr.Cells[0].Value = ImgIco.ico_ClassIcon;
                dgvr.Cells[2].Value = "";
                dgvr.Cells[3].Value = nodeclass.ToString();
            }
            else if (nodeclass == NodeClass.Method)
            {
                dgvr.Cells[0].Value = ImgIco.ico_Method_636;
                dgvr.Cells[2].Value = "";
                dgvr.Cells[3].Value = nodeclass.ToString();
            }
            else if (nodeclass == NodeClass.Variable)
            {
                DataValue dataValue = dataValues[1 + startIndex];

                BuiltInType Null = default;
                if (dataValue.WrappedValue.TypeInfo.BuiltInType != Null)
                {
                    dgvr.Cells[3].Value = dataValue.WrappedValue.TypeInfo.BuiltInType;
                    // dgvr.Cells[3].Value = dataValue.Value.GetType().ToString();
                    if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.Scalar)
                    {
                        dgvr.Cells[2].Value = dataValue.WrappedValue.Value;
                        dgvr.Cells[0].Value = ImgIco.ico_Enum_582;
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.OneDimension)
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = ImgIco.ico_brackets_Square_16xMD;
                    }
                    else if (dataValue.WrappedValue.TypeInfo.ValueRank == ValueRanks.TwoDimensions)
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = ImgIco.ico_Module_648;
                    }
                    else
                    {
                        dgvr.Cells[2].Value = dataValue.Value.GetType().ToString();
                        dgvr.Cells[0].Value = ImgIco.ico_ClassIcon;
                    }
                }
                else
                {
                    dgvr.Cells[0].Value = ImgIco.ico_ClassIcon;
                    dgvr.Cells[2].Value = dataValue.Value;
                    dgvr.Cells[3].Value = "null";
                }
            }
            else
            {
                dgvr.Cells[2].Value = "";
                dgvr.Cells[0].Value = ImgIco.ico_ClassIcon;
                dgvr.Cells[3].Value = nodeclass.ToString();
            }
        }

        private void AddDataGridViewArrayRow(NodeId nodeId, out int index)
        {

            DateTime dateTimeStart = DateTime.Now;
            DataValue[] dataValues = ReadOneNodeFiveAttributes(new List<NodeId>() { nodeId });
            label_time_spend.Text = (int)(DateTime.Now - dateTimeStart).TotalMilliseconds + " ms";

            DataValue dataValue = dataValues[1];

            if (dataValue.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
            {
                string access = GetDiscriptionFromAccessLevel(dataValues[2]);
                BuiltInType type = dataValue.WrappedValue.TypeInfo.BuiltInType;
                object des = dataValues[4].Value ?? "";
                object dis = dataValues[3].Value ?? type;

                Array array = dataValue.Value as Array;
                int i = 0;
                foreach (object obj in array)
                {
                    while (i >= dataGridView1.Rows.Count)
                    {
                        dataGridView1.Rows.Add();
                    }

                    DataGridViewRow dgvr = dataGridView1.Rows[i];

                    dgvr.Tag = null;

                    dgvr.Cells[0].Value = ImgIco.ico_Enum_582;
                    dgvr.Cells[1].Value = $"{dis} [{i++}]";
                    dgvr.Cells[2].Value = obj;
                    dgvr.Cells[3].Value = type;
                    dgvr.Cells[4].Value = access;
                    dgvr.Cells[5].Value = des;
                }
                index = i;
            }
            else
            {
                index = 0;
            }
        }

        private string GetDiscriptionFromAccessLevel(DataValue value)
        {
            if (value.WrappedValue.Value != null)
            {
                switch ((byte)value.WrappedValue.Value)
                {
                    case 0: return "None";
                    case 1: return "CurrentRead";
                    case 2: return "CurrentWrite";
                    case 3: return "CurrentReadOrWrite";
                    case 4: return "HistoryRead";
                    case 8: return "HistoryWrite";
                    case 12: return "HistoryReadOrWrite";
                    case 16: return "SemanticChange";
                    case 32: return "StatusWrite";
                    case 64: return "TimestampWrite";
                    default: return "None";
                }
            }
            else
            {
                return "null";
            }
        }



        #endregion

        #region 订阅刷新块


        private List<string> subNodeIds = new List<string>();
        private bool isSingleValueSub = false;

        private void RemoveAllSubscript()
        {
            m_OpcUaClient?.RemoveAllSubscription();
        }


        private void SubCallBack(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(SubCallBack), key, monitoredItem, eventArgs);
                return;
            }


            MonitoredItemNotification notification = eventArgs.NotificationValue as MonitoredItemNotification;
            string nodeId = monitoredItem.StartNodeId.ToString();

            int index = subNodeIds.IndexOf(nodeId);
            if (index >= 0)
            {
                if (isSingleValueSub)
                {
                    if (notification.Value.WrappedValue.TypeInfo?.ValueRank == ValueRanks.OneDimension)
                    {
                        Array array = notification.Value.WrappedValue.Value as Array;
                        int i = 0;
                        foreach (object obj in array)
                        {
                            DataGridViewRow dgvr = dataGridView1.Rows[i];
                            dgvr.Cells[2].Value = obj;
                            i++;
                        }
                    }
                    else
                    {
                        dataGridView1.Rows[index].Cells[2].Value = notification.Value.WrappedValue.Value;
                    }
                }
                else
                {
                    dataGridView1.Rows[index].Cells[2].Value = notification.Value.WrappedValue.Value;
                }
            }
        }


        private async void button17_Click(object sender, EventArgs e)
        {
            if (m_OpcUaClient != null)
            {
                RemoveAllSubscript();
                if (button17.BackColor != Color.LimeGreen)
                {
                    button17.BackColor = Color.LimeGreen;
                    // 判断当前的选择
                    if (string.IsNullOrEmpty(textBox_nodeId.Text)) return;


                    ReferenceDescriptionCollection references;
                    try
                    {
                        references = await Task.Run(() =>
                        {
                            return GetReferenceDescriptionCollection(new NodeId(textBox_nodeId.Text));
                        });
                    }
                    catch (Exception exception)
                    {
                        ClientUtils.HandleException(Text, exception);
                        return;
                    }

                    subNodeIds = new List<string>();
                    if (references?.Count > 0)
                    {
                        isSingleValueSub = false;
                        // 获取所有要订阅的子节点
                        for (int ii = 0; ii < references.Count; ii++)
                        {
                            ReferenceDescription target = references[ii];
                            subNodeIds.Add(((NodeId)target.NodeId).ToString());
                        }
                    }
                    else
                    {
                        isSingleValueSub = true;
                        // 子节点没有数据的情况
                        subNodeIds.Add(textBox_nodeId.Text);
                    }

                    m_OpcUaClient.AddSubscription("subTest", subNodeIds.ToArray(), SubCallBack);
                }
                else
                {
                    button17.BackColor = SystemColors.Control;
                }
            }
        }

        #endregion

        #region 点击了表格修改数据

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value is BuiltInType builtInType)
            {
                dynamic value = null;
                if (dataGridView1.Rows[e.RowIndex].Tag is NodeId nodeId)
                {
                    // 节点
                    try
                    {
                        value = GetValueFromString(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), builtInType);
                    }
                    catch
                    {
                        MessageBox.Show("Invalid Input Value");
                        return;
                    }

                    if (!m_OpcUaClient.WriteNode(nodeId.ToString(), value))
                    {
                        MessageBox.Show("Failed to write value");
                    }
                }
                else
                {
                    // 点击了数组修改
                    IList<string> list = new List<string>();

                    for (int jj = 0; jj < dataGridView1.RowCount; jj++)
                    {
                        list.Add(dataGridView1.Rows[jj].Cells[e.ColumnIndex].Value.ToString());
                    }

                    try
                    {
                        value = GetArrayValueFromString(list, builtInType);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Invalid Input Value: " + ex.Message);
                        return;
                    }

                    if (!m_OpcUaClient.WriteNode(textBox_nodeId.Text, value))
                    {
                        MessageBox.Show("Failed to write value");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid data type");
            }

            //MessageBox.Show(
            //    "Type:" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.GetType().ToString() + Environment.NewLine +
            //    "Value:" + dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
        }

        private dynamic GetValueFromString(string value, BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Boolean:
                    {
                        return bool.Parse(value);
                    }
                case BuiltInType.Byte:
                    {
                        return byte.Parse(value);
                    }
                case BuiltInType.DateTime:
                    {
                        return DateTime.Parse(value);
                    }
                case BuiltInType.Double:
                    {
                        return double.Parse(value);
                    }
                case BuiltInType.Float:
                    {
                        return float.Parse(value);
                    }
                case BuiltInType.Guid:
                    {
                        return Guid.Parse(value);
                    }
                case BuiltInType.Int16:
                    {
                        return short.Parse(value);
                    }
                case BuiltInType.Int32:
                    {
                        return int.Parse(value);
                    }
                case BuiltInType.Int64:
                    {
                        return long.Parse(value);
                    }
                case BuiltInType.Integer:
                    {
                        return int.Parse(value);
                    }
                case BuiltInType.LocalizedText:
                    {
                        return value;
                    }
                case BuiltInType.SByte:
                    {
                        return sbyte.Parse(value);
                    }
                case BuiltInType.String:
                    {
                        return value;
                    }
                case BuiltInType.UInt16:
                    {
                        return ushort.Parse(value);
                    }
                case BuiltInType.UInt32:
                    {
                        return uint.Parse(value);
                    }
                case BuiltInType.UInt64:
                    {
                        return ulong.Parse(value);
                    }
                case BuiltInType.UInteger:
                    {
                        return uint.Parse(value);
                    }
                default: throw new Exception("Not supported data type");
            }
        }


        private dynamic GetArrayValueFromString(IList<string> values, BuiltInType builtInType)
        {
            switch (builtInType)
            {
                case BuiltInType.Boolean:
                    {
                        bool[] result = new bool[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = bool.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Byte:
                    {
                        byte[] result = new byte[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = byte.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.DateTime:
                    {
                        DateTime[] result = new DateTime[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = DateTime.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Double:
                    {
                        double[] result = new double[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = double.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Float:
                    {
                        float[] result = new float[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = float.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Guid:
                    {
                        Guid[] result = new Guid[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = Guid.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int16:
                    {
                        short[] result = new short[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = short.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int32:
                    {
                        int[] result = new int[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = int.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Int64:
                    {
                        long[] result = new long[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = long.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.Integer:
                    {
                        int[] result = new int[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = int.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.LocalizedText:
                    {
                        return values.ToArray();
                    }
                case BuiltInType.SByte:
                    {
                        sbyte[] result = new sbyte[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = sbyte.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.String:
                    {
                        return values.ToArray();
                    }
                case BuiltInType.UInt16:
                    {
                        ushort[] result = new ushort[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = ushort.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInt32:
                    {
                        uint[] result = new uint[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = uint.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInt64:
                    {
                        ulong[] result = new ulong[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = ulong.Parse(values[i]);
                        }
                        return result;
                    }
                case BuiltInType.UInteger:
                    {
                        uint[] result = new uint[values.Count];
                        for (int i = 0; i < values.Count; i++)
                        {
                            result[i] = uint.Parse(values[i]);
                        }
                        return result;
                    }
                default: throw new Exception("Not supported data type");
            }
        }

        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 1].Value is BuiltInType builtInType)
            {
                if (
                    builtInType == BuiltInType.Boolean ||
                    builtInType == BuiltInType.Byte ||
                    builtInType == BuiltInType.DateTime ||
                    builtInType == BuiltInType.Double ||
                    builtInType == BuiltInType.Float ||
                    builtInType == BuiltInType.Guid ||
                    builtInType == BuiltInType.Int16 ||
                    builtInType == BuiltInType.Int32 ||
                    builtInType == BuiltInType.Int64 ||
                    builtInType == BuiltInType.Integer ||
                    builtInType == BuiltInType.LocalizedText ||
                    builtInType == BuiltInType.SByte ||
                    builtInType == BuiltInType.String ||
                    builtInType == BuiltInType.UInt16 ||
                    builtInType == BuiltInType.UInt32 ||
                    builtInType == BuiltInType.UInt64 ||
                    builtInType == BuiltInType.UInteger
                    )
                {

                }
                else
                {
                    e.Cancel = true;
                    MessageBox.Show("Not support the Type of modify value!");
                    return;
                }
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("Not support the Type of modify value!");
                return;
            }


            if (!dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex + 2].Value.ToString().Contains("Write"))
            {
                e.Cancel = true;
                MessageBox.Show("Not support the access of modify value!");
            }
        }



        #endregion



        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }


        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click_1(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }

        private void label31_Click(object sender, EventArgs e)
        {

        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void label29_Click(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox24_Enter(object sender, EventArgs e)
        {

        }




        private void button16_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox22.Text = openFileDialog.FileName;
                }
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            string endpointUrl = new DiscoverServerDlg().ShowDialog(m_OpcUaClient.AppConfig, null);

            if (endpointUrl != null)
            {
                textBox_Address.Text = endpointUrl;
            }
        }

        //读结点
        private void button8_Click(object sender, EventArgs e)
        {
            DataValue dataValue = m_OpcUaClient.ReadNode(new NodeId(textBox15.Text));
            textBox16.Text = dataValue.WrappedValue.Value.ToString();
        }

        //订阅
        private void button9_Click(object sender, EventArgs e)
        {
            // sub
            //m_OpcUaClient.AddSubscription("A", textBox4.Text, SubCallback);
            MonitorNodeTags = new string[]
{
                textBox18.Text,
                textBox24.Text,
};
            m_OpcUaClient.AddSubscription("B", MonitorNodeTags, SubCallback);
        }

        // 缓存的批量订阅的节点
        private string[] MonitorNodeTags = null;
        private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(SubCallback), key, monitoredItem, args);
                return;
            }

            if (key == "A")
            {
                // 如果有多个的订阅值都关联了当前的方法，可以通过key和monitoredItem来区分
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    textBox17.Text = notification.Value.WrappedValue.Value.ToString();
                }
            }
            else if (key == "B")
            {
                // 需要区分出来每个不同的节点信息
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (monitoredItem.StartNodeId.ToString() == MonitorNodeTags[0])
                {
                    textBox17.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == MonitorNodeTags[1])
                {
                    textBox25.Text = notification.Value.WrappedValue.Value.ToString();
                }
            }
        }

        //移除订阅
        private void button10_Click(object sender, EventArgs e)
        {
            // remove sub
            //m_OpcUaClient.RemoveSubscription("A");
            m_OpcUaClient.RemoveSubscription("B");
        }



        /*
        private void test()
        {
            OpcUaHelper.Forms.FormBrowseServer formBrowseServer = new Forms.FormBrowseServer("opc.tcp://127.0.0.1:62541/SharpNodeSettings/OpcUaServer");
            formBrowseServer.ShowDialog();
        }


        private void test1()
        {
            try
            {
                short value = m_OpcUaClient.ReadNode<short>("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度");
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }

        private async void test2()
        {
            try
            {
                short value = await m_OpcUaClient.ReadNodeAsync<short>("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度");
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }

        private void test3()
        {
            try
            {
                m_OpcUaClient.WriteNode("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度", (short)123);
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }

        private void test4()
        {
            try
            {
                // 添加所有的读取的节点，此处的示例是类型不一致的情况
                List<NodeId> nodeIds = new List<NodeId>();
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/机器人关节"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/cvsdf"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/条码"));
                nodeIds.Add(new NodeId("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/开关量"));

                // dataValues按顺序定义的值，每个值里面需要重新判断类型
                List<DataValue> dataValues = m_OpcUaClient.ReadNodes(nodeIds.ToArray());
                // 然后遍历你的数据信息
                foreach (var dataValue in dataValues)
                {
                    // 获取你的实际的数据
                    object value = dataValue.WrappedValue.Value;
                }




                // 如果你批量读取的值的类型都是一样的，比如float，那么有简便的方式
                List<string> tags = new List<string>();
                tags.Add("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗");
                tags.Add("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速");

                // 按照顺序定义的值
                List<float> values = m_OpcUaClient.ReadNodes<float>(tags.ToArray());

            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }


            try
            {
                // 此处演示写入一个short，2个float类型的数据批量写入操作
                bool success = m_OpcUaClient.WriteNodes(new string[] {
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度",
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗",
                    "ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速"},
                    new object[] {
                        (short)1234,
                        123.456f,
                        123f
                    });
                if (success)
                {
                    // 写入成功
                }
                else
                {
                    // 写入失败，一个失败即为失败
                }
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }


            try
            {
                // 此处演示读取历史数据的操作，读取8月18日12点到13点的数据，如果想要读取成功，该节点是支持历史记录的
                List<float> values = m_OpcUaClient.ReadHistoryRawDataValues<float>("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速",
                    new DateTime(2018, 8, 18, 12, 0, 0), new DateTime(2018, 8, 18, 13, 0, 0)).ToList();

            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }


        }

        public void test5()
        {

            try
            {
                OpcNodeAttribute[] nodeAttributes = m_OpcUaClient.ReadNoteAttributes("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度");
                foreach (var item in nodeAttributes)
                {
                    Console.Write(string.Format("{0,-30}", item.Name));
                    Console.Write(string.Format("{0,-20}", item.Type));
                    Console.Write(string.Format("{0,-20}", item.StatusCode));
                    Console.WriteLine(string.Format("{0,20}", item.Value));
                }

                // 输出如下
                //  Name                          Type                StatusCode                         Vlaue

                //  NodeClass                     Int32               Good                                   2
                //  BrowseName                    QualifiedName       Good                              2:温度
                //  DisplayName                   LocalizedText       Good                                温度
                //  Description                   LocalizedText       Good                                    
                //  WriteMask                     UInt32              Good                                  96
                //  UserWriteMask                 UInt32              Good                                  96
                //  Value                         Int16               Good                              -11980
                //  DataType                      NodeId              Good                                 i=4
                //  ValueRank                     Int32               Good                                  -1
                //  ArrayDimensions               Null                Good                                    
                //  AccessLevel                   Byte                Good                                   3
                //  UserAccessLevel               Byte                Good                                   3
                //  MinimumSamplingInterval       Double              Good                                   0
                //  Historizing                   Boolean             Good                               False
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }

        public void test6()
        {

            try
            {
                ReferenceDescription[] references = m_OpcUaClient.BrowseNodeReference("ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端");
                foreach (var item in references)
                {
                    Console.Write(string.Format("{0,-30}", item.NodeClass));
                    Console.Write(string.Format("{0,-30}", item.BrowseName));
                    Console.Write(string.Format("{0,-20}", item.DisplayName));
                    Console.WriteLine(string.Format("{0,-20}", item.NodeId.ToString()));
                }

                ;
                // 输出如下
                //  NodeClass                     BrowseName                      DisplayName           NodeId

                //  Variable                      2:温度                          温度                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/温度
                //  Variable                      2:风俗                          风俗                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/风俗
                //  Variable                      2:转速                          转速                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/转速
                //  Variable                      2:机器人关节                    机器人关节            ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/机器人关节
                //  Variable                      2:cvsdf                         cvsdf                 ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/cvsdf
                //  Variable                      2:条码                          条码                  ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/条码
                //  Variable                      2:开关量                        开关量                ns=2;s=Devices/分厂一/车间二/ModbusTcp客户端/开关量
            }
            catch (Exception ex)
            {
                ClientUtils.HandleException(this.Text, ex);
            }
        }*/



        #endregion

        #region phoxi

        #region phoxi变量

        private static PhoXiFactory Factory = new PhoXiFactory();
        private PhoXi PhoXiDevice = Factory.CreateAndConnectFirstAttached();


        #endregion


        #region 加载图片

        //在线连接相机
        private void button7_Click(object sender, EventArgs e)
        {

            //Check if the PhoXi Control is running
            if (!Factory.isPhoXiControlRunning()) return;
            Console.WriteLine("PhoXi Control is running");
            //Get List of available devices on the network

            PhoXiDeviceInformation[] DeviceList = Factory.GetDeviceList();
            Console.WriteLine("PhoXi Factory found {0} devices by GetDeviceList call.", DeviceList.Length);
            Console.WriteLine();
            for (int i = 0; i < DeviceList.Length; i++)
            {
                Console.WriteLine("Device: {0}", i);
                Console.WriteLine("  Name:                    " + (String)DeviceList[i].Name);
                Console.WriteLine("  Hardware Identification: " + (String)DeviceList[i].HWIdentification);
                Console.WriteLine("  Type:                    " + (String)DeviceList[i].Type);
                Console.WriteLine("  Firmware version:        " + (String)DeviceList[i].FirmwareVersion);
                Console.WriteLine("  Variant:                 " + (String)DeviceList[i].Variant);
                Console.WriteLine("  IsFileCamera:            " + (DeviceList[i].IsFileCamera ? "Yes" : "No"));
                Console.WriteLine("  Feaure-Alpha:            " + (DeviceList[i].CheckFeature("Alpha") ? "Yes" : "No"));
                Console.WriteLine("  Feaure-Color:            " + (DeviceList[i].CheckFeature("Color") ? "Yes" : "No"));
                Console.WriteLine("  Status:                  " + (DeviceList[i].Status.Attached ? "Attached to PhoXi Control. " : "Not Attached to PhoXi Control. ") + (DeviceList[i].Status.Ready ? "Ready to connect" : "Occupied"));
                Console.WriteLine();
            }

            //Try to connect Device opened in PhoXi Control, if Any

            if (PhoXiDevice != null)
            {
                Console.WriteLine("You have already PhoXi device opened in PhoXi Control, the API Example is connected to device: " + (String)PhoXiDevice.HardwareIdentification);
            }
            else
            {
                Console.WriteLine("You have no PhoXi device opened in PhoXi Control, the API Example will try to connect to last device in device list");
                if (DeviceList.Length > 0)
                {
                    PhoXiDevice = Factory.CreateAndConnectFirstAttached();
                }
            }
            if (PhoXiDevice == null)
            {
                Console.WriteLine("No device is connected!");
                return;
            }
            CommunicationsuccessfulCam = true;
            label44.Text = "已连接";
            label44.BackColor = Color.LimeGreen;
            label9.Text = "已连接";
            label9.BackColor = Color.LimeGreen;
            //PhoXiDevice.Disconnect();

        }


        //在线触发拍照
        private void button19_Click(object sender, EventArgs e)
        {
            if (PhoXiDevice.isConnected())
            {
                Console.WriteLine("Your device is connected");
                if (PhoXiDevice.isAcquiring())
                {
                    PhoXiDevice.StopAcquisition();
                }
                Console.WriteLine("Starting Software trigger mode");
                PhoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
                PhoXiDevice.ClearBuffer();
                PhoXiDevice.StartAcquisition();
                if (PhoXiDevice.isAcquiring())
                {
                    Console.WriteLine("Triggering the 1-th frame");
                    int FrameID = PhoXiDevice.TriggerFrame();
                    if (FrameID < 0)
                    {
                        //If negative number is returned trigger was unsuccessful
                        Console.WriteLine("Trigger was unsuccessful! code={0}", FrameID);
                    }
                    else
                    {
                        Console.WriteLine("Frame was triggered, Frame Id: {0}", FrameID);
                    }
                    Console.WriteLine("Waiting for frame 1");
                    Frame MyFrame = PhoXiDevice.GetSpecificFrame(FrameID, PhoXiTimeout.Value.Infinity);

                    if (MyFrame != null)
                    {
                        Console.WriteLine("Frame retrieved");
                        Console.WriteLine("  Frame params: ");
                        Console.WriteLine("    Frame Index: {0}", MyFrame.Info.FrameIndex);
                        Console.WriteLine("    Frame Timestamp: {0}", MyFrame.Info.FrameTimestamp);
                        Console.WriteLine("    Frame Duration: {0}", MyFrame.Info.FrameDuration);
                        Console.WriteLine("    Frame Resolution: {0} x {1}", MyFrame.GetResolution().Width, MyFrame.GetResolution().Height);
                        Console.WriteLine("    Sensor Position: {0}; {1}; {2}", MyFrame.Info.SensorPosition.x, MyFrame.Info.SensorPosition.y, MyFrame.Info.SensorPosition.z);
                        Console.WriteLine("    Total scan count: {0}", MyFrame.Info.TotalScanCount);
                        if (!MyFrame.Empty())
                        {
                            Console.WriteLine("  Frame data: ");
                            if (!MyFrame.PointCloud.Empty())
                            {
                                Console.WriteLine("    PointCloud: {0} x {1} Type: {2}", MyFrame.PointCloud.Size.Width, MyFrame.PointCloud.Size.Height, PointCloud32f.GetElementName());
                            }
                            if (!MyFrame.NormalMap.Empty())
                            {
                                Console.WriteLine("    NormalMap: {0} x {1} Type: {2}", MyFrame.NormalMap.Size.Width, MyFrame.NormalMap.Size.Height, NormalMap32f.GetElementName());
                            }
                            if (!MyFrame.DepthMap.Empty())
                            {
                                Console.WriteLine("    DepthMap: {0} x {1} Type: {2}", MyFrame.DepthMap.Size.Width, MyFrame.DepthMap.Size.Height, DepthMap32f.GetElementName());
                            }
                            if (!MyFrame.ConfidenceMap.Empty())
                            {
                                Console.WriteLine("    ConfidenceMap: {0} x {1} Type: {2}", MyFrame.ConfidenceMap.Size.Width, MyFrame.ConfidenceMap.Size.Height, ConfidenceMap32f.GetElementName());
                            }
                            if (!MyFrame.Texture.Empty())
                            {
                                float[] curBitmaparry = MyFrame.Texture.GetDataCopy();
                                Texture32f curBitmap = MyFrame.Texture;

                                Bitmap Bitmaptemp = BGR24ToBitmap(curBitmaparry, curBitmap.Size.Width, curBitmap.Size.Height);
                                //pictureBox2.
                                pictureBox1.Image = Bitmaptemp;

                                Console.WriteLine("    Texture: {0} x {1} Type: {2}", MyFrame.Texture.Size.Width, MyFrame.Texture.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.TextureRGB.Empty())
                            {
                                Console.WriteLine("    TextureRGB: {0} x {1} Type: {2}", MyFrame.TextureRGB.Size.Width, MyFrame.TextureRGB.Size.Height, Texture32f.GetElementName());
                            }
                            if (!MyFrame.ColorCameraImage.Empty())
                            {

                            }
                        }
                        else
                        {
                            Console.WriteLine("Frame is empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve the frame!");
                    }
                }



                PhoXiDevice.StopAcquisition();
            }
            //PhoXiDevice.Disconnect();
        }

        //数组数据转Bitmap
        public Bitmap BGR24ToBitmap(float[] imgBGR, int vw, int vh)
        {

            int p = 0;
            int vW = vw;
            int vH = vh;


            ////量化
            Srcquantification Srcquantificationtemp = new Srcquantification();

            Srcquantificationtemp.Srcmax = 3.402823466e+38F;
            Srcquantificationtemp.Srcmin = 1.175494351e-38F;
            Dstquantification Dstquantificationtemp = new Dstquantification();

            Dstquantificationtemp.Dstmax = 255;
            Dstquantificationtemp.Dstmin = 0;

            Utilsquantification.Quantification(Srcquantificationtemp, Dstquantificationtemp, out float Scalin, out float zero);

            float[] src = { 2.1f, 8.6f };
            sbyte[] dst = new sbyte[src.Length];

            Utilsquantification.Quantificationout(Scalin, zero, src, out dst);
            ////



            Bitmap bmp = new Bitmap(vW, vH, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            if (imgBGR != null)
            {
                //构造一个位图数组进行数据存储
                byte[] rgbvalues = new byte[imgBGR.Length*3];

                //对每一个像素的颜色进行转化
                for (int i = 0; i < rgbvalues.Length; i += 3)
                {
                    rgbvalues[i] = (byte)imgBGR[i/3];
                    rgbvalues[i + 1] = (byte)imgBGR[i/3];
                    rgbvalues[i + 2] = (byte)imgBGR[i/3];
                }

                //位图矩形
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //以可读写的方式将图像数据锁定
                System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                //得到图形在内存中的首地址
                IntPtr ptr = bmpdata.Scan0;

                //将被锁定的位图数据复制到该数组内
                //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, imgBGR.Length);
                //把处理后的图像数组复制回图像
                System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr, imgBGR.Length);
                //解锁位图像素
                bmp.UnlockBits(bmpdata);

            }
            return bmp;
        }

        /*public Bitmap BGR24ToBitmap(float[] imgBGR, int vw, int vh)
        {

            int p = 0;
            int vW = vw;
            int vH = vh;


            Bitmap bmp = new Bitmap(vW, vH, System.Drawing.Imaging.PixelFormat.Format48bppRgb);

            if (imgBGR != null)
            {
                //构造一个位图数组进行数据存储
                short[] rgbvalues = new short[imgBGR.Length];

                //对每一个像素的颜色进行转化
                for (int i = 0; i < rgbvalues.Length; i += 3)
                {
                    rgbvalues[i] = (short)imgBGR[i + 2];
                    rgbvalues[i + 1] = (short)imgBGR[i + 1];
                    rgbvalues[i + 2] = (short)imgBGR[i];
                }

                //位图矩形
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //以可读写的方式将图像数据锁定
                System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                //得到图形在内存中的首地址
                IntPtr ptr = bmpdata.Scan0;

                //将被锁定的位图数据复制到该数组内
                //System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, imgBGR.Length);
                //把处理后的图像数组复制回图像
                System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, ptr, imgBGR.Length);
                //解锁位图像素
                bmp.UnlockBits(bmpdata);

            }
            return bmp;
        }*/


        public byte[] bitmap2BGR24(Bitmap img)
        {
            byte[] bgrBytes = new byte[0];
            Bitmap bmp = (Bitmap)img;

            if (bmp != null)
            {
                //位图矩形
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                //以可读写的方式将图像数据锁定
                System.Drawing.Imaging.BitmapData bmpdata = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                //构造一个位图数组进行数据存储
                int bLength = bmp.Width * bmp.Height * 3;
                byte[] rgbVal = new byte[bLength];
                bgrBytes = new byte[bLength];
                //得到图形在内存中的首地址
                IntPtr ptr = bmpdata.Scan0;
                //将被锁定的位图数据复制到该数组内
                System.Runtime.InteropServices.Marshal.Copy(bmpdata.Scan0, rgbVal, 0, bLength);
                //把处理后的图像数组复制回图像
                //System.Runtime.InteropServices.Marshal.Copy(rgbVal, 0, ptr, bytes);
                //解锁位图像素
                bmp.UnlockBits(bmpdata);

                //对每一个像素的rgb to bgr的转换
                for (int i = 0; i < rgbVal.Length; i += 3)
                {
                    bgrBytes[i] = rgbVal[i + 2];
                    bgrBytes[i + 1] = rgbVal[i + 1];
                    bgrBytes[i + 2] = rgbVal[i];
                }

            }
            return bgrBytes;
        }

        //从文件中加载
        private void button20_Click(object sender, EventArgs e)
        {
            //创建OpenFileDialog对象
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //创建一个筛选器
            openFileDialog1.Filter = "JPEG文件|*.jpg*|BMP文件|*.bmp*|PNG文件|*.png*";
            //dialog.Filter = "所有文件(*.*)|*.*"
            //设置对话框标题
            openFileDialog1.Title = "打开`图片`:";


            //启用帮助按钮
            openFileDialog1.ShowHelp = true;

            //如果结果为打开，则选定文件
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string curFileName = openFileDialog1.FileName;
                Bitmap curBitmap = (Bitmap)Image.FromFile(curFileName);

                if (curBitmap != null)
                {
                    //pictureBox2.
                    pictureBox2.Image = curBitmap;
                }
            }


        }



        #endregion

        #endregion

        #region calibration

        #region 变量声明null
        public static string Picturestoragepath = string.Empty;
        public static string Correctionpath = string.Empty;
        public static int patternSizeWidth = 9;//棋盘格模板宽度
        public static int patternSizeHigh = 6;//棋盘格模板高度
        public static float size_grid = 0.15F;//棋盘格每个格子的大小

        Size imgsizetemp = new Size(2064, 1544);//图片大小
        List<string> img_paths = new List<string>();



        Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        Mat distCoeffs = new Mat(1, 5, MatType.CV_32FC1);

        Point2f[] cp_int = new Point2f[patternSizeWidth * patternSizeHigh];
        Point3f[] cp_world = new Point3f[patternSizeWidth * patternSizeHigh];

        List<Mat> points_world_list = new();
        List<Mat> points_pixel_list = new();

        double[] R = null;
        double[,] T = null;

        Robotpose3D[] _rtgripper2base = new Robotpose3D[0];
        Campose3D[] _rtTarget2cam = new Campose3D[0];


        string reprojectFileName = string.Empty;

        string Calibrationmode = "eyeinhand";//标定方式
        bool CommunicationsuccessfulRobot = false;//机器手通信成功
        bool CommunicationsuccessfulCam = false;//相机通信成功
        bool noSelectinternalparameter = true;//未选取内参

        #endregion

        #region 获取内参
        //选择图片存储目录
        private void ChoicePicturestoragepath()
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "选择图片存储目录"; ;
            //string path = string.Empty;
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                Picturestoragepath = dialog1.SelectedPath;//获取选中文件路径
            }
            textBox23.Text = Picturestoragepath;
            textBox26.Text = Picturestoragepath;
        }
        //选择图片存储目录
        private void button21_Click(object sender, EventArgs e)
        {
            ChoicePicturestoragepath();
        }
        //选择图片存储目录
        private void button23_Click(object sender, EventArgs e)
        {
            ChoicePicturestoragepath();
        }

        //保存图片
        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                //保存图片到本地文件夹
                //System.IO.MemoryStream ms = new System.IO.MemoryStream(pictureBox2.Image);
                //System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                //保存到磁盘文件
                if (Picturestoragepath != string.Empty)
                {

                    string imagePath = System.IO.Path.Combine(Picturestoragepath, "图像" + DateTime.Now.ToString("yyyyMMdd") + ".Bmp");
                    pictureBox1.Image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Bmp);
                }


                MessageBox.Show("图片已保存至：" + Picturestoragepath);
            }
            catch (Exception exception)
            {
            }
        }
        //棋盘格模板高度
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            patternSizeHigh = (int)numericUpDown1.Value;
        }

        //棋盘格模板宽度
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            patternSizeWidth = (int)numericUpDown2.Value;
        }

        //棋盘格每个格子的大小
        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            size_grid = (float)numericUpDown3.Value;
        }

        //读取文件夹图片
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                img_paths.Clear();
                dataGridView3.Rows.Clear();
                if (Picturestoragepath != string.Empty)
                {
                    string[] extensions = { "jpg", "png", "jpeg", "Bitmap" };
                    foreach (string extension in extensions)
                    {
                        img_paths.AddRange(Directory.GetFiles(Picturestoragepath, $"*.{extension}"));
                    }

                    if (img_paths.Count != 0)
                    {
                        for (int i = 0; i < img_paths.Count; i++)
                        {
                            int index = dataGridView3.Rows.Add();
                            DataGridViewRow dgvr = dataGridView3.Rows[index];
                            dgvr.Cells[0].Value = index;
                            dgvr.Cells[1].Value = (img_paths[i].Replace(Picturestoragepath, string.Empty));
                            dgvr.Cells[2].Value = "";
                        }
                    }
                    else
                    {
                        Debug.Assert(img_paths.Count == 0, "No images for calibration found!");
                    }
                }
                else
                {
                    Debug.Assert(img_paths.Count == 0, "No Pictures path for found!");
                }

            }
            catch (Exception)
            {

                throw;
            }

        }


        //选中展示图片
        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (tabControl11.SelectedIndex == 0 || tabControl11.SelectedIndex == 2)
            {
                if (img_paths.Count != 0)
                {
                    Mat img = Cv2.ImRead(img_paths[dataGridView3.CurrentCell.RowIndex]);
                    var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img);//mat转bitmap
                    pictureBox3.Image = bitmap;
                }
            }
            else if (tabControl11.SelectedIndex == 1)
            {
            }
        }

        //设置角点行列值
        private void setupcpintworld()
        {
            // Cp_int：int形式的角点，以‘int’形式保存世界空间角点的坐标，如(0,0,0), (1,0,0), (2,0,0) ...., (10,7,0)

            for (int i = 0; i < patternSizeHigh; i++)
            {
                for (int j = 0; j < patternSizeWidth; j++)
                {
                    cp_int[i * patternSizeWidth + j] = new Point2f();
                    cp_int[i * patternSizeWidth + j].X = j;
                    cp_int[i * patternSizeWidth + j].Y = i;
                }
            }
            // Cp_world：世界空间中的角点，保存世界空间中角点的坐标

            for (int i = 0; i < patternSizeWidth * patternSizeHigh; i++)
            {
                cp_world[i] = new Point3f();
                //cp_world[i].X = (float)Math.Round((cp_int[i].X * size_grid),5);
                cp_world[i].X = cp_int[i].X * size_grid;
                //cp_world[i].Y = (float)Math.Round((cp_int[i].Y * size_grid),5);
                cp_world[i].Y = cp_int[i].Y * size_grid;
                cp_world[i].Z = 0;
            }
        }

        //寻找棋盘格角点
        private bool FindChessboardCornerstemp(Mat img, out List<Point3f> points_world_temp, out List<Point2f> points_pixel_temp, PictureBox PictureBoxtemp = null)
        {
            points_world_temp = null;
            points_pixel_temp = null;
            bool ret = false;
            try
            {
                if (img != null)
                {
                    points_world_temp = new List<Point3f>();
                    points_pixel_temp = new List<Point2f>();
                    Mat gray_img = new Mat();
                    imgsizetemp.Width = img.Width;
                    imgsizetemp.Height = img.Height;
                    //gray_img = img;
                    Cv2.CvtColor(img, gray_img, ColorConversionCodes.BGR2GRAY);

                    //Cv2.FindCirclesGrid (gray_img, patternSize, corners, FindCirclesGridFlags.AsymmetricGrid);
                    ret = Cv2.FindChessboardCorners(gray_img, new Size(patternSizeWidth, patternSizeHigh), out Point2f[] cp_img, ChessboardFlags.AdaptiveThresh | ChessboardFlags.FastCheck | ChessboardFlags.NormalizeImage);

                    if (ret)
                    {
                        //亚像素精确化
                        IEnumerable<Point2f> cornersSubPix = new List<Point2f>();
                        Cv2.CornerSubPix(gray_img, cp_img, new Size(5, 5), new Size(-1, -1), TermCriteria.Both(30, 0.1));

                        //Buried a hole without taking into account the alignment of corners
                        for (int i = 0; i < cp_img.Length; i++)
                        {
                            points_world_temp.Add(cp_world[i]);
                            points_pixel_temp.Add(cp_img[i]);
                        }
                        // 查看角点检测结果
                        if (PictureBoxtemp != null)
                        {
                            Cv2.DrawChessboardCorners(img, new Size(patternSizeWidth, patternSizeHigh), cp_img, ret);
                            //var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(source);//bitmap转mat
                            //Cv2.CvtColor(gray_img, gray_img, ColorConversionCodes.RGBA2RGB);//mat转三通道mat
                            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img);//mat转bitmap

                            //Cv2.ImShow("FoundCorners", gray_img);
                            //Cv2.WaitKey(500);
                            PictureBoxtemp.Image = bitmap;
                            Cv2.WaitKey(300);
                        }
                    }

                    else if (!ret)
                    {
                        // 查看角点检测结果
                        if (PictureBoxtemp != null)
                        {
                            Cv2.DrawChessboardCorners(img, new Size(patternSizeWidth, patternSizeHigh), cp_img, ret);
                            //var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(source);//bitmap转mat
                            //Cv2.CvtColor(gray_img, gray_img, ColorConversionCodes.RGBA2RGB);//mat转三通道mat
                            var bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(img);//mat转bitmap

                            //Cv2.ImShow("FoundCorners", gray_img);
                            //Cv2.WaitKey(500);
                            PictureBoxtemp.Image = bitmap;
                            Cv2.WaitKey(300);
                        }

                        return false;
                    }

                }
                else
                {
                    return false;
                }

                return true;

            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        //计算内参矩阵
        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                setupcpintworld();

                List<List<Point3f>> points_world = new(); // the points in world space
                List<List<Point2f>> points_pixel = new();
                points_world_list = new();
                points_pixel_list = new();

                for (int i = 0; i < img_paths.Count; i++)
                {
                    Mat img = Cv2.ImRead(img_paths[i]);

                    // 如果ret为True，则保存
                    bool ret = FindChessboardCornerstemp(img, out List<Point3f> points_world_temp, out List<Point2f> points_pixel_temp, pictureBox3);

                    if (ret)
                    {

                        DataGridViewRow dgvr = dataGridView3.Rows[i];
                        dgvr.Cells[2].Value = "已找到全部角点 (^_^) ";

                        points_world.Add(points_world_temp);
                        points_pixel.Add(points_pixel_temp);
                    }
                    else if (!ret)
                    {
                        DataGridViewRow dgvr = dataGridView3.Rows[i];
                        dgvr.Cells[2].Value = "未找到全部角点 (>﹏<)";
                    }
                }

                for (int i = 0; i < points_world.Count; i++)
                {
                    Point3f[] points_world_single_arry = points_world[i].ToArray();
                    Mat m = new Mat(points_world_single_arry.Length, 1, MatType.CV_32FC3, points_world_single_arry);
                    points_world_list.Add(m);

                    Point2f[] points_pixel_single_arry = points_pixel[i].ToArray();
                    Mat n = new Mat(points_pixel_single_arry.Length, 1, MatType.CV_32FC2, points_pixel_single_arry);
                    points_pixel_list.Add(n);

                }
                Mat cameraMatrixtemp = new Mat(3, 3, MatType.CV_64FC1);
                Mat distCoeffstemp = new Mat(1, 5, MatType.CV_64FC1);
                var rms = Cv2.CalibrateCamera(points_world_list, points_pixel_list, imgsizetemp, cameraMatrixtemp, distCoeffstemp, out Mat[] rvecs, out Mat[] tvecs, CalibrationFlags.FixIntrinsic);

                cameraMatrix = cameraMatrixtemp;
                distCoeffs = distCoeffstemp;

                if (comboBox4.Items.Count == 2)
                {
                    comboBox4.Items.Add("相机标定结果");
                }
                comboBox4.SelectedIndex = 2;
                ShowinternalparametertoForm();

                if (comboBox6.Items.Count == 2)
                {
                    comboBox6.Items.Add("相机标定结果");
                }
                comboBox6.SelectedIndex = 2;
                ShowdistCoeffstoForm();




                richTextBox1.Text = "误差：" + rms.ToString();

                //double[,] tempcameraMatrix= ConvertMat2Array(cameraMatrix);
                //Console.WriteLine("intrinsic matrix: \n{0}", tempcameraMatrix);
                double[,] tempcameraMatrix = ConvertMattoArray(cameraMatrix, false);
                double[,] tempdistCoeffs = ConvertMattoArray(distCoeffs, false);

                double[,] temprvecs = ConvertMattoArray(rvecs[0], false);
                double[,] temptvecs = ConvertMattoArray(tvecs[0], false);

                tabControl8.SelectedIndex = 0;

                DisplaymatrixdatatorichTextBox(tempcameraMatrix, richTextBox2);
                DisplaymatrixdatatorichTextBox(tempdistCoeffs, richTextBox3);


            }
            catch (Exception)
            {

                throw;
            }

        }


        #endregion

        //从表格获取内参矩阵
        private void GetinternalparameterfromForm()
        {
            double[,] cameraMatrixtemp = new double[3, 3];
            for (int i = 0; i < dataGridView4.RowCount; i++)
            {
                for (int j = 0; j < dataGridView4.ColumnCount; j++)
                {
                    cameraMatrixtemp[i, j] = double.Parse(dataGridView4.Rows[i].Cells[j].Value.ToString());
                }
            }
            cameraMatrix = new Mat(3, 3, MatType.CV_64FC1, cameraMatrixtemp);
        }

        //从相机获取内参矩阵
        private void GetinternalparameterfromCam()
        {

        }


        //将内参矩阵展示到表格
        private void ShowinternalparametertoForm()
        {
            double[,] cameraMatrixtemp = ConvertMattoArray(cameraMatrix, false);
            for (int i = 0; i < dataGridView4.RowCount; i++)
            {
                for (int j = 0; j < dataGridView4.ColumnCount; j++)
                {
                    dataGridView4.Rows[i].Cells[j].Value = cameraMatrixtemp[i, j].ToString();
                }
            }
        }

        //从表格获取畸变矩阵
        private void GetdistCoeffsfromForm()
        {
            double[,] distCoeffstemp = new double[1, 5];
            for (int i = 0; i < dataGridView5.ColumnCount; i++)
            {
                distCoeffstemp[0, i] = double.Parse(dataGridView5.Rows[0].Cells[i].Value.ToString());
            }
            distCoeffs = new Mat(1, 5, MatType.CV_64FC1, distCoeffstemp);

        }

        //从相机获取畸变矩阵
        private void GetdistCoeffsfromCam()
        {

        }

        //将畸变矩阵展示到表格
        private void ShowdistCoeffstoForm()
        {
            double[,] distCoeffstemp = ConvertMattoArray(distCoeffs, false);
            for (int i = 0; i < dataGridView5.ColumnCount; i++)
            {
                dataGridView5.Rows[0].Cells[i].Value = distCoeffstemp[0, i].ToString();
            }
        }



        #region 获取外参 & 计算重投影
        //选择内参矩阵
        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex == 0)
            {
                while (dataGridView4.RowCount < 3)
                {
                    dataGridView4.Rows.Add();
                }
                for (int i = 0; i < dataGridView4.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView4.ColumnCount; j++)
                    {
                        if (i == j)
                        {
                            dataGridView4.Rows[i].Cells[j].Value = 1;
                        }
                        else
                        {
                            dataGridView4.Rows[i].Cells[j].Value = 0;
                        }

                    }
                }



            }
            else if (comboBox4.SelectedIndex == 1)
            {
                //zhendefule、hunluandeluoji  o.O
                GetinternalparameterfromCam();
                ShowinternalparametertoForm();

            }
            else if (comboBox4.SelectedIndex == 2)
            {

            }
            /*{
                dataGridView4.ReadOnly = true;
                while (dataGridView4.RowCount < 3)
                {
                    dataGridView4.Rows.Add();
                }
                if (usedcameraMatrix)
                {
                    reprojectedcameraMatrix = cameraMatrix;
                    double[,] cameraMatrixtemp = ConvertMattoArray(cameraMatrix, false);
                    for (int i = 0; i < dataGridView4.RowCount; i++)
                    {
                        for (int j = 0; j < dataGridView4.ColumnCount; j++)
                        {
                            dataGridView4.Rows[i].Cells[j].Value = cameraMatrixtemp[i, j].ToString();
                        }
                    }
                }

            }*/

        }


        //选择畸变矩阵
        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox6.SelectedIndex == 0)
            {

                while (dataGridView5.RowCount < 1)
                {
                    dataGridView5.Rows.Add();
                }
                for (int i = 0; i < dataGridView5.ColumnCount; i++)
                {
                    dataGridView5.Rows[0].Cells[i].Value = 0;
                }
            }
            else if (comboBox6.SelectedIndex == 1)
            {
                GetdistCoeffsfromCam();
                ShowdistCoeffstoForm();

            }
            else if (comboBox4.SelectedIndex == 2)
            {

            }
        }


        //获取外参 & 计算重投影 加载图片
        private void button25_Click(object sender, EventArgs e)
        {
            //创建OpenFileDialog对象
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            //创建一个筛选器
            openFileDialog1.Filter = "JPEG文件|*.jpg*|BMP文件|*.bmp*|PNG文件|*.png*";
            //dialog.Filter = "所有文件(*.*)|*.*"
            //设置对话框标题
            openFileDialog1.Title = "打开`图片`:";


            //启用帮助按钮
            openFileDialog1.ShowHelp = true;

            //如果结果为打开，则选定文件
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                reprojectFileName = openFileDialog1.FileName;
                Bitmap curBitmap = (Bitmap)Image.FromFile(reprojectFileName);

                if (curBitmap != null)
                {
                    //pictureBox2.
                    pictureBox3.Image = curBitmap;
                }

                dataGridView3.Rows.Clear();
                if (reprojectFileName != string.Empty)
                {
                    int index = dataGridView3.Rows.Add();
                    DataGridViewRow dgvr = dataGridView3.Rows[index];
                    dgvr.Cells[0].Value = index;
                    dgvr.Cells[1].Value = reprojectFileName;
                    dgvr.Cells[2].Value = "";
                }
                else
                {
                    Debug.Assert(img_paths.Count == 0, "No Pictures path for found!");
                }
            }
        }

        //获取外参 & 计算重投影方法//
        private void Calculateexternalparameters(Mat Bitmaptemp)
        {
            setupcpintworld();

            Mat reproject_img = Bitmaptemp;

            // 如果ret为True，则保存
            bool ret = FindChessboardCornerstemp(reproject_img, out List<Point3f> points_world_temp, out List<Point2f> points_pixel_temp, pictureBox3);

            if (ret)
            {
                if (ret)
                {
                    DataGridViewRow dgvr = dataGridView3.Rows[0];
                    dgvr.Cells[2].Value = "已找到全部角点 (^_^) ";
                }
                else if (!ret)
                {
                    DataGridViewRow dgvr = dataGridView3.Rows[0];
                    dgvr.Cells[2].Value = "未找到全部角点 (>﹏<)";
                }

                var objPtsMat = InputArray.Create<Point3f>(points_world_temp, MatType.CV_32FC3);
                var imgPtsMat = InputArray.Create<Point2f>(points_pixel_temp, MatType.CV_32FC2);

                //var distMat = Mat.FromArray(dist);
                //var distMat = Mat.Zeros(5, 0, MatType.CV_64FC1);

                var rvecMat = new Mat();
                var tvecMat = new Mat();

                GetinternalparameterfromForm();
                GetdistCoeffsfromForm();

                // 平面时 旋转及平移向量 SolvePnPFlags.DLS  贼大// Iterative接近CalibrateCamera   //  p3p平面报错  //                  
                Cv2.SolvePnP(objPtsMat, imgPtsMat, cameraMatrix, distCoeffs, rvecMat, tvecMat, false, SolvePnPFlags.Iterative);

                //获取外参
                Mat rvec = new Mat();
                Cv2.Rodrigues(rvecMat, rvec);
                double[,] Rtemp = new double[3, 3];
                Rtemp = ConvertMattoArray(rvec, false);

                R = RotationVectorToQuaternion(Rtemp);


                T = ConvertMattoArray(tvecMat, false);

                tabControl8.SelectedIndex = 2;

                DisplaymatrixdatatorichTextBox(Rtemp, richTextBox4);
                DisplaymatrixdatatorichTextBox(T, richTextBox5);
                //

                //

                //计算重投影
                Point3f[] points_world_single_arry = points_world_temp.ToArray();
                Mat points_world_single_mat = new Mat(points_world_single_arry.Length, 1, MatType.CV_32FC3, points_world_single_arry);
                Point2f[] points_pixel_single_arry = points_pixel_temp.ToArray();
                Mat points_pixel_single_mat = new Mat(points_pixel_single_arry.Length, 1, MatType.CV_32FC2, points_pixel_single_arry);

                Mat reprojected_points = new Mat(points_world_temp.Count, 1, MatType.CV_32FC2);

                Cv2.ProjectPoints(points_world_single_mat, rvecMat, tvecMat, cameraMatrix, distCoeffs, reprojected_points);
                double error = Cv2.Norm(points_pixel_single_mat, reprojected_points, NormTypes.L2) / ((points_world_temp.Count) * 2);

                double mean_error = Math.Sqrt(error);

                richTextBox1.Text = "误差: " + mean_error.ToString();
            }
            return;

        }


        //展示矩阵数据到富文本
        private void DisplaymatrixdatatorichTextBox(double[,] datatemp, RichTextBox RichTextBoxtemp)
        {
            string tempstring = "";
            for (int i = 0; i < datatemp.GetLength(0); i++)
            {
                for (int j = 0; j < datatemp.GetLength(1); j++)
                {
                    tempstring += "[" + i + "," + j + "]" + datatemp[i, j].ToString() + "\n";
                }
            }
            RichTextBoxtemp.Text = tempstring;

        }

        //获取外参 & 计算重投影
        private void button26_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {

                Mat reproject_img = Cv2.ImRead(reprojectFileName);

                Calculateexternalparameters(reproject_img);

            }
        }



        #endregion

        #region 图片矫正
        //选择矫正后图片目录
        private void button28_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "选择矫正后图片目录";
            //string path = string.Empty;
            if (dialog1.ShowDialog() == DialogResult.OK)
            {

                textBox27.Text = dialog1.SelectedPath;//获取选中文件路径
            }
        }

        //选择矫正前图片目录
        private void button24_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog1 = new FolderBrowserDialog();
            dialog1.Description = "选择矫正前图片目录"; ;
            //string path = string.Empty;
            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                textBox28.Text = dialog1.SelectedPath;//获取选中文件路径
            }
        }

        //矫正图片加载
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                img_paths.Clear();
                dataGridView3.Rows.Clear();
                if (textBox28.Text != string.Empty)
                {
                    string[] extensions = { "jpg", "png", "jpeg", "Bitmap" };
                    foreach (string extension in extensions)
                    {
                        img_paths.AddRange(Directory.GetFiles(Picturestoragepath, $"*.{extension}"));
                    }

                    if (img_paths.Count != 0)
                    {
                        for (int i = 0; i < img_paths.Count; i++)
                        {
                            int index = dataGridView3.Rows.Add();
                            DataGridViewRow dgvr = dataGridView3.Rows[index];
                            dgvr.Cells[0].Value = index;
                            dgvr.Cells[1].Value = (img_paths[i].Replace(Picturestoragepath, string.Empty));
                            dgvr.Cells[2].Value = "准备矫正";
                        }
                    }
                    else
                    {
                        Debug.Assert(img_paths.Count == 0, "No images for calibration found!");
                    }
                }
                else
                {
                    Debug.Assert(img_paths.Count == 0, "No Pictures path for found!");
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        //矫正图片
        private void button27_Click(object sender, EventArgs e)
        {
            GetinternalparameterfromForm();
            GetdistCoeffsfromForm();

            if (textBox27.Text != string.Empty && textBox28.Text != string.Empty)
            {
                for (int i = 0; i < img_paths.Count; i++)
                {
                    string img_name = Path.GetFileName(img_paths[i]);
                    Mat img = Cv2.ImRead(img_paths[i]);
                    Mat newcameramtx = Cv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, img.Size(), 1, img.Size(), out _, false);
                    Mat dst = new Mat();
                    Cv2.Undistort(img, dst, cameraMatrix, distCoeffs, newcameramtx);
                    // 剪裁图像
                    // int x = roi.X;
                    // int y = roi.Y;
                    // int width = roi.Width;
                    // int height = roi.Height;
                    // dst = dst[new Rect(x, y, width, height)];
                    Cv2.ImWrite(Path.Combine(textBox27.Text, img_name), dst);

                    DataGridViewRow dgvr = dataGridView3.Rows[i];
                    dgvr.Cells[2].Value = "矫正完成";
                }

                MessageBox.Show("已将去失真图像保存到: " + textBox27.Text);

            }
            else
            {
                MessageBox.Show("路径不存在");
            }

        }



        #endregion





        #endregion
        //内参 外参 展示板切换
        private void tabControl11_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl11.SelectedIndex == 0)
            {
                tabPage23.Text = "内参矩阵";
                tabPage24.Text = "畸变矩阵";
            }
            else
            {
                tabPage23.Text = "旋转矩阵";
                tabPage24.Text = "平移矩阵";
            }
        }

        //标定方式
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Calibrationmode = comboBox1.SelectedItem as string;
        }

        //机器人数据格式选择
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    tabControl4.SelectTab(0);
                    break;

                case 1:
                    tabControl4.SelectTab(0);
                    break;

                case 2:
                    tabControl4.SelectTab(1);
                    break;
                case 3:
                    tabControl4.SelectTab(1);
                    break;
                case 4:
                    tabControl4.SelectTab(1);
                    break;
                case 5:
                    tabControl4.SelectTab(1);
                    break;
                case 6:
                    tabControl4.SelectTab(1);
                    break;
                case 7:
                    tabControl4.SelectTab(1);
                    break;
                case 8:
                    tabControl4.SelectTab(2);
                    break;
            }
        }

        //相机数据格式选择
        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox9.SelectedIndex)
            {
                case 0:
                    tabControl5.SelectTab(0);
                    break;

                case 1:
                    tabControl5.SelectTab(0);
                    break;

                case 2:
                    tabControl5.SelectTab(1);
                    break;
                case 3:
                    tabControl5.SelectTab(1);
                    break;
                case 4:
                    tabControl5.SelectTab(1);
                    break;
                case 5:
                    tabControl5.SelectTab(1);
                    break;
                case 6:
                    tabControl5.SelectTab(1);
                    break;
                case 7:
                    tabControl5.SelectTab(1);
                    break;
                case 8:
                    tabControl5.SelectTab(2);
                    break;

            }
        }


        //机器人点数据新增行
        private void button29_Click(object sender, EventArgs e)
        {
            switch (tabControl4.SelectedIndex)
            {
                case 0:
                    dataGridView6.Rows.Add();
                    break;

                case 1:
                    dataGridView6.Rows.Add();
                    break;

                case 2:
                    dataGridView8.Rows.Add();
                    break;
                case 3:
                    dataGridView8.Rows.Add();
                    break;
                case 4:
                    dataGridView8.Rows.Add();
                    break;
                case 5:
                    dataGridView8.Rows.Add();
                    break;
                case 6:
                    dataGridView8.Rows.Add();
                    break;
                case 7:
                    dataGridView8.Rows.Add();
                    break;
            }
        }
        //相机点数据新增行
        private void button32_Click(object sender, EventArgs e)
        {
            switch (tabControl5.SelectedIndex)
            {
                case 0:
                    dataGridView9.Rows.Add();
                    break;

                case 1:
                    dataGridView9.Rows.Add();
                    break;

                case 2:
                    dataGridView10.Rows.Add();
                    break;
                case 3:
                    dataGridView10.Rows.Add();
                    break;
                case 4:
                    dataGridView10.Rows.Add();
                    break;
                case 5:
                    dataGridView10.Rows.Add();
                    break;
                case 6:
                    dataGridView10.Rows.Add();
                    break;
                case 7:
                    dataGridView10.Rows.Add();
                    break;
            }
        }

        //机器人点数据删除选中行
        private void button30_Click(object sender, EventArgs e)
        {
            switch (tabControl4.SelectedIndex)
            {
                case 0:
                    dataGridView6.Rows.Remove(dataGridView6.CurrentRow);
                    break;

                case 1:
                    dataGridView6.Rows.Remove(dataGridView6.CurrentRow);
                    break;

                case 2:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;
                case 3:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;
                case 4:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;
                case 5:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;
                case 6:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;
                case 7:
                    dataGridView8.Rows.Remove(dataGridView8.CurrentRow);
                    break;

            }
        }

        //相机点数据删除选中行
        private void button31_Click(object sender, EventArgs e)
        {
            switch (tabControl5.SelectedIndex)
            {
                case 0:
                    dataGridView9.Rows.Remove(dataGridView9.CurrentRow);
                    break;

                case 1:
                    dataGridView9.Rows.Remove(dataGridView9.CurrentRow);
                    break;

                case 2:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
                case 3:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
                case 4:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
                case 5:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
                case 6:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
                case 7:
                    dataGridView10.Rows.Remove(dataGridView10.CurrentRow);
                    break;
            }
        }

        //
        Robotpose3D Robotpose3Dtemp = new Robotpose3D();

        //相机取图
        private Bitmap Takepictures()
        {
            Bitmap Bitmaptemp = null;
            if (PhoXiDevice.isConnected())
            {
                if (PhoXiDevice.isAcquiring())
                {
                    PhoXiDevice.StopAcquisition();
                }
                PhoXiDevice.TriggerMode = PhoXiTriggerMode.Value.Software;
                PhoXiDevice.ClearBuffer();
                PhoXiDevice.StartAcquisition();
                if (PhoXiDevice.isAcquiring())
                {
                    int FrameID = PhoXiDevice.TriggerFrame();
                    Frame MyFrame = PhoXiDevice.GetSpecificFrame(FrameID, PhoXiTimeout.Value.Infinity);

                    if (MyFrame != null)
                    {
                        if (!MyFrame.Empty())
                        {

                            if (!MyFrame.Texture.Empty())
                            {

                                float[] curBitmaparry = MyFrame.Texture.GetDataCopy();
                                Texture32f curBitmap = MyFrame.Texture;

                                Bitmaptemp = BGR24ToBitmap(curBitmaparry, curBitmap.Size.Width, curBitmap.Size.Height);

                            }
                        }
                        else
                        {
                            Console.WriteLine("Frame is empty.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve the frame!");
                    }
                }



                PhoXiDevice.StopAcquisition();
            }
            //PhoXiDevice.Disconnect();

            return Bitmaptemp;
        }


        //采集 机器人 相机 姿态
        private void button1_Click(object sender, EventArgs e)
        {
            if (CommunicationsuccessfulRobot && RobotMonitorNodeTags != null)
            {

                int index = 0;
                if (comboBox2.SelectedIndex < 2)
                {
                    index = dataGridView6.Rows.Add();
                    dataGridView6.Rows[index].Cells[0].Value = textBoxRobotposex.Text;
                    dataGridView6.Rows[index].Cells[1].Value = textBoxRobotposey.Text;
                    dataGridView6.Rows[index].Cells[2].Value = textBoxRobotposez.Text;
                    dataGridView6.Rows[index].Cells[3].Value = textBoxRobotposeq1.Text;
                    dataGridView6.Rows[index].Cells[4].Value = textBoxRobotposeq2.Text;
                    dataGridView6.Rows[index].Cells[5].Value = textBoxRobotposeq3.Text;
                    dataGridView6.Rows[index].Cells[6].Value = textBoxRobotposeq4.Text;
                }
                else
                {
                    index = dataGridView8.Rows.Add();
                    dataGridView8.Rows[index].Cells[0].Value = textBoxRobotposex.Text;
                    dataGridView8.Rows[index].Cells[1].Value = textBoxRobotposey.Text;
                    dataGridView8.Rows[index].Cells[2].Value = textBoxRobotposez.Text;
                    dataGridView8.Rows[index].Cells[3].Value = textBoxRobotposeq1.Text;
                    dataGridView8.Rows[index].Cells[4].Value = textBoxRobotposeq2.Text;
                    dataGridView8.Rows[index].Cells[5].Value = textBoxRobotposeq3.Text;
                }
            }

            if (CommunicationsuccessfulCam)
            {
                Bitmap Bitmaptemp = Takepictures();

                Mat Bitmapmat = OpenCvSharp.Extensions.BitmapConverter.ToMat(Bitmaptemp);

                Calculateexternalparameters(Bitmapmat);
                //


                int index = 0;
                tabControl5.SelectedIndex = 0;
                comboBox9.SelectedIndex = 0;

                if (comboBox9.SelectedIndex < 2)
                {
                    index = dataGridView9.Rows.Add();
                    dataGridView9.Rows[index].Cells[0].Value = T[0, 0].ToString();
                    dataGridView9.Rows[index].Cells[1].Value = T[1, 0].ToString();
                    dataGridView9.Rows[index].Cells[2].Value = T[2, 0].ToString();
                    dataGridView9.Rows[index].Cells[3].Value = R[0].ToString();
                    dataGridView9.Rows[index].Cells[4].Value = R[1].ToString();
                    dataGridView9.Rows[index].Cells[5].Value = R[2].ToString();
                    dataGridView9.Rows[index].Cells[6].Value = R[3].ToString();
                }

            }

        }

        //手眼标定结果计算
        private void button2_Click(object sender, EventArgs e)
        {
            tabControl8.SelectedIndex = 2;
            richTextBox4.Text = "计算中";
            richTextBox5.Text = "计算中";

            //构建机器手位姿数据
            if (comboBox2.SelectedIndex < 2 && dataGridView6.Rows.Count > 2)
            {

                _rtgripper2base = new Robotpose3D[dataGridView6.Rows.Count];

                for (int i = 0; i < dataGridView6.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView6.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView6.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView6.Rows[i].Cells[j].Value = 0;
                        }
                    }
                    Robotpose3D _rtgripper2basetemp = new Robotpose3D();
                    _rtgripper2basetemp.X = double.Parse(dataGridView6.Rows[i].Cells[0].Value.ToString());
                    _rtgripper2basetemp.Y = double.Parse(dataGridView6.Rows[i].Cells[1].Value.ToString());
                    _rtgripper2basetemp.Z = double.Parse(dataGridView6.Rows[i].Cells[2].Value.ToString());

                    if (comboBox2.SelectedIndex == 0)
                    {
                        _rtgripper2basetemp.W = double.Parse(dataGridView6.Rows[i].Cells[3].Value.ToString());
                        _rtgripper2basetemp.Q1 = double.Parse(dataGridView6.Rows[i].Cells[4].Value.ToString());
                        _rtgripper2basetemp.Q2 = double.Parse(dataGridView6.Rows[i].Cells[5].Value.ToString());
                        _rtgripper2basetemp.Q3 = double.Parse(dataGridView6.Rows[i].Cells[6].Value.ToString());
                    }
                    else
                    {
                        _rtgripper2basetemp.Q1 = double.Parse(dataGridView6.Rows[i].Cells[3].Value.ToString());
                        _rtgripper2basetemp.Q2 = double.Parse(dataGridView6.Rows[i].Cells[4].Value.ToString());
                        _rtgripper2basetemp.Q3 = double.Parse(dataGridView6.Rows[i].Cells[5].Value.ToString());
                        _rtgripper2basetemp.W = double.Parse(dataGridView6.Rows[i].Cells[6].Value.ToString());
                    }
                    _rtgripper2base[i] = _rtgripper2basetemp;
                }
            }
            else if (comboBox2.SelectedIndex > 1 && comboBox2.SelectedIndex < 8 && dataGridView8.Rows.Count > 2)
            {

                _rtgripper2base = new Robotpose3D[dataGridView8.Rows.Count];
                for (int i = 0; i < dataGridView8.Rows.Count; i++)
                {
                    Robotpose3D _rtgripper2basetemp = new Robotpose3D();

                    for (int j = 0; j < dataGridView8.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView8.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView8.Rows[i].Cells[j].Value = 0;
                        }
                    }
                    _rtgripper2basetemp.X = double.Parse(dataGridView8.Rows[i].Cells[0].Value.ToString());
                    _rtgripper2basetemp.Y = double.Parse(dataGridView8.Rows[i].Cells[1].Value.ToString());
                    _rtgripper2basetemp.Z = double.Parse(dataGridView8.Rows[i].Cells[2].Value.ToString());


                    double rx = double.Parse(dataGridView8.Rows[i].Cells[3].Value.ToString());
                    double ry = double.Parse(dataGridView8.Rows[i].Cells[4].Value.ToString());
                    double rz = double.Parse(dataGridView8.Rows[i].Cells[5].Value.ToString());

                    string[] Rotationmode = new string[] { "xyz", "xzy", "yxz", "yzx", "zxy", "zyx" };

                    Quaternion Robotpose3Dquat = EulerToQuaternion(rx, ry, rz, comboBox7.SelectedItem.ToString(), Rotationmode[comboBox2.SelectedIndex - 2]);

                    _rtgripper2basetemp.W = Robotpose3Dquat.w;
                    _rtgripper2basetemp.Q1 = Robotpose3Dquat.x;
                    _rtgripper2basetemp.Q2 = Robotpose3Dquat.y;
                    _rtgripper2basetemp.Q3 = Robotpose3Dquat.z;

                    _rtgripper2base[i] = _rtgripper2basetemp;
                }
            }
            else if (comboBox2.SelectedIndex == 8 && dataGridView2.Rows.Count > 2)
            {

                _rtgripper2base = new Robotpose3D[dataGridView2.Rows.Count];
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    Robotpose3D _rtgripper2basetemp = new Robotpose3D();

                    for (int j = 0; j < dataGridView2.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView2.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView2.Rows[i].Cells[j].Value = 0;
                        }
                    }
                    _rtgripper2basetemp.X = double.Parse(dataGridView2.Rows[i].Cells[0].Value.ToString());
                    _rtgripper2basetemp.Y = double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString());
                    _rtgripper2basetemp.Z = double.Parse(dataGridView2.Rows[i].Cells[2].Value.ToString());


                    double rx = double.Parse(dataGridView2.Rows[i].Cells[3].Value.ToString());
                    double ry = double.Parse(dataGridView2.Rows[i].Cells[4].Value.ToString());
                    double rz = double.Parse(dataGridView2.Rows[i].Cells[5].Value.ToString());

                    Mat RotVector = new Mat(1, 3, MatType.CV_64FC1);
                    Mat Rotationmatrix = new Mat(3, 3, MatType.CV_64FC1);
                    double[,] Rotationmatrixarry = null;
                    RotVector.At<double>(0, 0) = rx;
                    RotVector.At<double>(0, 1) = ry;
                    RotVector.At<double>(0, 2) = rz;
                    RotVector.At<double>(0, 0) /= (180 / Math.PI);      //度转弧度
                    RotVector.At<double>(0, 1) /= (180 / Math.PI);
                    RotVector.At<double>(0, 2) /= (180 / Math.PI);

                    Rotationmatrix = RotVectorToRotationmatrix(RotVector, comboBox7.SelectedItem.ToString());
                    Rotationmatrixarry = ConvertMattoArray(Rotationmatrix, false);

                    double[] Robotpose3Dquatarry = RotationVectorToQuaternion(Rotationmatrixarry);

                    _rtgripper2basetemp.W = Robotpose3Dquatarry[0];
                    _rtgripper2basetemp.Q1 = Robotpose3Dquatarry[1];
                    _rtgripper2basetemp.Q2 = Robotpose3Dquatarry[2];
                    _rtgripper2basetemp.Q3 = Robotpose3Dquatarry[3];

                    _rtgripper2base[i] = _rtgripper2basetemp;
                }
            }

            //构建相机位姿数据
            if (comboBox9.SelectedIndex < 2 && dataGridView9.Rows.Count > 2)
            {
                _rtTarget2cam = new Campose3D[dataGridView9.Rows.Count];

                for (int i = 0; i < dataGridView9.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView9.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView9.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView9.Rows[i].Cells[j].Value = 0;
                        }
                    }

                    Campose3D _rtTarget2camtemp = new Campose3D();

                    _rtTarget2camtemp.X = double.Parse(dataGridView9.Rows[i].Cells[0].Value.ToString());
                    _rtTarget2camtemp.Y = double.Parse(dataGridView9.Rows[i].Cells[1].Value.ToString());
                    _rtTarget2camtemp.Z = double.Parse(dataGridView9.Rows[i].Cells[2].Value.ToString());
                    if (comboBox9.SelectedIndex == 0)
                    {
                        _rtTarget2camtemp.W = double.Parse(dataGridView9.Rows[i].Cells[3].Value.ToString());
                        _rtTarget2camtemp.Q1 = double.Parse(dataGridView9.Rows[i].Cells[4].Value.ToString());
                        _rtTarget2camtemp.Q2 = double.Parse(dataGridView9.Rows[i].Cells[5].Value.ToString());
                        _rtTarget2camtemp.Q3 = double.Parse(dataGridView9.Rows[i].Cells[6].Value.ToString());
                    }
                    else
                    {
                        _rtTarget2camtemp.Q1 = double.Parse(dataGridView9.Rows[i].Cells[3].Value.ToString());
                        _rtTarget2camtemp.Q2 = double.Parse(dataGridView9.Rows[i].Cells[4].Value.ToString());
                        _rtTarget2camtemp.Q3 = double.Parse(dataGridView9.Rows[i].Cells[5].Value.ToString());
                        _rtTarget2camtemp.W = double.Parse(dataGridView9.Rows[i].Cells[6].Value.ToString());
                    }
                    _rtTarget2cam[i] = _rtTarget2camtemp;
                }
            }
            else if (comboBox9.SelectedIndex > 1 && comboBox9.SelectedIndex < 8 && dataGridView10.Rows.Count > 2)
            {
                _rtTarget2cam = new Campose3D[dataGridView10.Rows.Count];
                for (int i = 0; i < dataGridView10.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView10.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView10.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView10.Rows[i].Cells[j].Value = 0;
                        }
                    }

                    Campose3D _rtTarget2camtemp = new Campose3D();

                    _rtTarget2camtemp.X = double.Parse(dataGridView10.Rows[i].Cells[0].Value.ToString());
                    _rtTarget2camtemp.Y = double.Parse(dataGridView10.Rows[i].Cells[1].Value.ToString());
                    _rtTarget2camtemp.Z = double.Parse(dataGridView10.Rows[i].Cells[2].Value.ToString());


                    double rx = double.Parse(dataGridView10.Rows[i].Cells[3].Value.ToString());
                    double ry = double.Parse(dataGridView10.Rows[i].Cells[4].Value.ToString());
                    double rz = double.Parse(dataGridView10.Rows[i].Cells[5].Value.ToString());

                    string[] Rotationmode = new string[] { "xyz", "xzy", "yxz", "yzx", "zxy", "zyx" };

                    Quaternion Campose3Dquat = EulerToQuaternion(rx, ry, rz, comboBox8.SelectedItem.ToString(), Rotationmode[comboBox9.SelectedIndex - 2]);

                    _rtTarget2camtemp.W = Campose3Dquat.w;
                    _rtTarget2camtemp.Q1 = Campose3Dquat.x;
                    _rtTarget2camtemp.Q2 = Campose3Dquat.y;
                    _rtTarget2camtemp.Q3 = Campose3Dquat.z;

                    _rtTarget2cam[i] = _rtTarget2camtemp;
                }
            }
            else if (comboBox9.SelectedIndex == 8 && dataGridView7.Rows.Count > 2)
            {
                _rtTarget2cam = new Campose3D[dataGridView7.Rows.Count];
                for (int i = 0; i < dataGridView7.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView7.Rows[i].Cells.Count; j++)
                    {
                        if (dataGridView7.Rows[i].Cells[j].Value == null)
                        {
                            dataGridView7.Rows[i].Cells[j].Value = 0;
                        }
                    }

                    Campose3D _rtTarget2camtemp = new Campose3D();

                    _rtTarget2camtemp.X = double.Parse(dataGridView7.Rows[i].Cells[0].Value.ToString());
                    _rtTarget2camtemp.Y = double.Parse(dataGridView7.Rows[i].Cells[1].Value.ToString());
                    _rtTarget2camtemp.Z = double.Parse(dataGridView7.Rows[i].Cells[2].Value.ToString());


                    double rx = double.Parse(dataGridView7.Rows[i].Cells[3].Value.ToString());
                    double ry = double.Parse(dataGridView7.Rows[i].Cells[4].Value.ToString());
                    double rz = double.Parse(dataGridView7.Rows[i].Cells[5].Value.ToString());

                    Mat RotVector = new Mat(1, 3, MatType.CV_64FC1);
                    Mat Rotationmatrix = new Mat(3, 3, MatType.CV_64FC1);
                    double[,] Rotationmatrixarry = null;
                    RotVector.At<double>(0, 0) = rx;
                    RotVector.At<double>(0, 1) = ry;
                    RotVector.At<double>(0, 2) = rz;
                    RotVector.At<double>(0, 0) /= (180 / Math.PI);      //度转弧度
                    RotVector.At<double>(0, 1) /= (180 / Math.PI);
                    RotVector.At<double>(0, 2) /= (180 / Math.PI);

                    Rotationmatrix = RotVectorToRotationmatrix(RotVector, comboBox7.SelectedItem.ToString());
                    Rotationmatrixarry = ConvertMattoArray(Rotationmatrix, false);

                    double[] Campose3Dquatarry = RotationVectorToQuaternion(Rotationmatrixarry);

                    _rtTarget2camtemp.W = Campose3Dquatarry[0];
                    _rtTarget2camtemp.Q1 = Campose3Dquatarry[1];
                    _rtTarget2camtemp.Q2 = Campose3Dquatarry[2];
                    _rtTarget2camtemp.Q3 = Campose3Dquatarry[3];

                    _rtTarget2cam[i] = _rtTarget2camtemp;
                }
            }


            string[] Calibrationmode = new string[] { "eyeinhand", "handtoeye" };

            double[,] R_cam2base = null;
            double[,] T_cam2base = null;

            double[,] R_cam2tool = null;
            double[,] T_cam2tool = null;

            if (_rtgripper2base.Length == _rtTarget2cam.Length && _rtTarget2cam.Length > 2)
            {
                if (comboBox1.SelectedIndex == 1)
                {
                    (R_cam2base, T_cam2base) = HandtoEyeCalibration(_rtgripper2base, _rtTarget2cam, Calibrationmode[comboBox1.SelectedIndex]);
                }
                else if (comboBox1.SelectedIndex == 0)
                {
                    (R_cam2tool, T_cam2tool) = HandtoEyeCalibration(_rtgripper2base, _rtTarget2cam, Calibrationmode[comboBox1.SelectedIndex]);
                }

            }


            //It's finally coming to an end.

            if (comboBox1.SelectedIndex == 1 && R_cam2base != null && T_cam2base != null)
            {
                DisplaymatrixdatatorichTextBox(R_cam2base, richTextBox4);
                DisplaymatrixdatatorichTextBox(T_cam2base, richTextBox5);
            }
            else if (comboBox1.SelectedIndex == 0 && R_cam2tool != null & T_cam2tool != null)
            {
                DisplaymatrixdatatorichTextBox(R_cam2tool, richTextBox4);
                DisplaymatrixdatatorichTextBox(T_cam2tool, richTextBox5);
            }

        }

        //机器人坐标订阅
        private void button33_Click(object sender, EventArgs e)
        {
            // sub
            tabControl12.SelectTab(1);
            RobotMonitorNodeTags = new string[]
            {
                textBox29.Text,
                textBox30.Text,
                textBox31.Text,
                textBox32.Text,
                textBox33.Text,
                textBox34.Text,
                textBox35.Text,

            };
            m_OpcUaClient.AddSubscription("Robot", RobotMonitorNodeTags, RobotSubCallback);
        }

        //缓存的批量订阅的节点
        private string[] RobotMonitorNodeTags = null;

        //机器人坐标订阅返回值
        private void RobotSubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(RobotSubCallback), key, monitoredItem, args);
                return;
            }
            if (key == "Robot")
            {
                // 需要区分出来每个不同的节点信息
                MonitoredItemNotification notification = args.NotificationValue as MonitoredItemNotification;
                if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[0])
                {
                    textBoxRobotposex.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[1])
                {
                    textBoxRobotposey.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[2])
                {
                    textBoxRobotposez.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[3])
                {
                    textBoxRobotposeq1.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[4])
                {
                    textBoxRobotposeq2.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[5])
                {
                    textBoxRobotposeq3.Text = notification.Value.WrappedValue.Value.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == RobotMonitorNodeTags[6])
                {
                    textBoxRobotposeq4.Text = notification.Value.WrappedValue.Value.ToString();
                }

            }
        }

        //机器人坐标取消订阅
        private void button34_Click(object sender, EventArgs e)
        {
            tabControl12.SelectTab(0);
            m_OpcUaClient.RemoveSubscription("Robot");
        }

        //初始化内参选择
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                tabPage23.Text = "内参矩阵";
                tabPage24.Text = "畸变矩阵";
            }
            else if (tabControl1.SelectedIndex == 2)
            {
            }

            if (noSelectinternalparameter)
            {
                noSelectinternalparameter = false;

                dataGridView4.ReadOnly = false;
                while (dataGridView4.RowCount < 3)
                {
                    dataGridView4.Rows.Add();
                }
                for (int i = 0; i < dataGridView4.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView4.ColumnCount; j++)
                    {
                        if (i == j)
                        {
                            dataGridView4.Rows[i].Cells[j].Value = 1;
                        }
                        else
                        {
                            dataGridView4.Rows[i].Cells[j].Value = 0;
                        }

                    }
                }


                dataGridView5.ReadOnly = false;
                while (dataGridView5.RowCount < 1)
                {
                    dataGridView5.Rows.Add();
                }
                for (int i = 0; i < dataGridView5.ColumnCount; i++)
                {
                    dataGridView5.Rows[0].Cells[i].Value = 0;
                }

                GetinternalparameterfromForm();
                GetdistCoeffsfromForm();
                patternSizeWidth = (int)numericUpDown1.Value;//棋盘格模板宽度
                patternSizeHigh = (int)numericUpDown2.Value;//棋盘格模板高度
                size_grid = (float)numericUpDown3.Value;//棋盘格每个格子的大小

                comboBox2.SelectedIndex = 0;
                comboBox7.SelectedIndex = 0;

                comboBox9.SelectedIndex = 0;
                comboBox8.SelectedIndex = 0;
                comboBox1.SelectedIndex = 0;

                textBox29.Text = "ns=3;i=1008";
                textBox35.Text = "ns=3;i=1014";
                textBox30.Text = "ns=3;i=1009";
                textBox34.Text = "ns=3;i=1013";
                textBox31.Text = "ns=3;i=1010";
                textBox33.Text = "ns=3;i=1012";
                textBox32.Text = "ns=3;i=1011";

                dataGridView3.Rows.Clear();
                dataGridView9.Rows.Clear();

                Fillexperimentaldata();

            }



        }

        //end 填充实验数据
        private void Fillexperimentaldata()
        {
            //string Robotpose3DdataTypes = "quat";
            string Robotpose3DdataTypes = "euler";
            //string Robotpose3DdataTypes = "RotVector";

            //string Campose3DdataTypes = "quat";
            string Campose3DdataTypes = "euler";
            //string Campose3DdataTypes = "RotVector";

            //Robotpose3D数组形式数据
            //* x, y, z, rx, ry, rz//Robotpose3D第一组数据 eye-in-hand//rad//结果R=rx, ry, rz ,rw=-0.3546426, 0.3806739, 0.6086649, 0.5990351//T=tx, ty, tz= 149.8673153645521, 18.5544654988438, 308.2450983655534
            double[,] _gripper2base_array_data = new double[,] { {144.4521161, 66.1347948, -465.7051590, -0.4743987, -0.2368566, -0.0782465},
                                                             {150.2226899, 62.4463700, -484.2951369, -0.4732508,-0.2369567, -0.073467},
                                                             {206.6077654, -27.9324378, -448.4078075, -0.6758548, -0.2354039, -0.1120374},
                                                             {-43.4263308, 41.8964311, -453.4463616, -0.5101464, 0.2718147, 0.0053353},
                                                             {207.5704729, 7.3753215, -406.3695738, -0.7015643, -0.362299, 0.0441629},
                                                             {221.8741936, 106.2104049, -413.5291980, -0.4975038, -0.4841192, -0.072782},
                                                             {168.6236789, 124.7429837, -444.4091351, -0.3671233, -0.427913, -0.0212725},
                                                             {213.8876994, -13.8574024, -444.9478678, -0.6308002, -0.246069, -0.2131169},
                                                            };//*/
            /* x, y, z, rx, ry, rz//Robotpose3D第二组数据 hand-to-eye//rad//结果R=rx, ry, rz ,rw=0.1425639, 0.1237163, -0.0709875, 0.9794542//T=tx, ty, tz= 689.2978893974176, 386.9843567965046, 573.5786306809142
            double[,] _gripper2base_array_data = new double[,] { {377.18,346.69, 581.09, -2.1895155, 1.4149385, -0.7845255},
                                                     {390.68, 344.43, 577.58, -2.0561725,1.4149385, -0.7840018},
                                                     {383.86, 351.50, 566.52, -1.1843802, 1.4220942, 1.6646948},
                                                     {402.21, 291.88, 557.76, -1.4454816, 1.1091567, -1.3946927},
                                                    };//*/

            /* x, y, z, rx, ry, rz//zyx//Robotpose3D第三组数据（现场采集） hand-to-eye//rad//结果R=\
            double[,] _gripper2base_array_data = new double[,] {
                {867.699,   71.162,  385.037, 180, 0,   0},
                {855.409,   74.817 , 391.161, 172.903 ,-8.948 ,-10.337 },
                {1055.17,   80.918,  395.283, -167.921,    0.348 ,  159.597 },
                {874.167,   203.736, 405.933, -165.132,    -4.456 , -22.881 },
                {836.035 ,  75.508 , 385.724 ,-173.825 ,   0.219  , 16.821 },
                {815.745 ,  110.001, 393.791 ,-174.991 ,   -4.518 , 32.231   },
                {962.724 ,  24.351  ,388.702 ,-176.485 ,   -2.584 , 136.069  },
                {908.375 ,  100.09 , 368.998 ,-169.627 ,   8.902  , 80.819   },
                {859.499 ,  50.482  ,382.074 ,-178.799 ,   -0.525 , -9.949   },
                {1060.93  , -22.56  ,387.697 ,-174.637 ,   -1.123 , 70.961   },
                { 888.886 , 63.374 , 396.199 ,-177.892 ,   -7.702  ,57.681  },
                {856.113  , 96.102 , 353.72  ,170.778 ,5.543   ,81.936   },
                {984.187 ,  34.762 , 363.128 ,-171.736 ,   10.596 , 94.496 },
                { 1031.59 , -39.38 , 410.165 ,-173.574  ,  -14.636 ,115.58},
                { 1103.34 , 125.125 ,378.396 ,-178.559  ,  1.379  , 150.637 }
            };//*/

            /* x, y, z, rx, ry, rz//Robotpose3D第四组数据（张扬） eye-in-hand//rad//结果R=//T=tx, ty, tz=-0.0601215675088463,-0.3223385694500517, 0.10481985839295936
            double[,] _gripper2base_array_data = new double[,] {
               {-0.15242, -0.088, 1.09041, -16.102, 25.659, 123.69},
               {-0.088, -0.15242, 1.09041, -26.565, 14.478, 153.435},
               {0.000, -0.176, 1.09041, -30.00, 0.00, -180.00},
               {0.088, -0.15242, 1.09041, -26.565, -14.478, -153.435 },
               {0.15242, -0.088, 1.09041, -16.102, -25.659, -123.69},
               {-0.176, 0.000, 1.09041, 0.00, 30.00, 90.00 },
               {-0.176, 0.29538, 1.011126, -30.00, 30.00, 90.00},
               {-0.16716, 0.2668, 1.02776, -46.102, 25.659, 123.69 },
               {-0.143, 0.24588, 1.03984, -56.565, 14.478, 153.435 },
               {-0.2119, 0.26178, 0.98782, -83.948, -18.937, 126.052},
           };//*/

            /* x, y, z, rx, ry, rz, rw//Robotpose3D第五组数据 eye-in-hand//rad//结果R= //T=tx, ty, tz= -0.261030962036173, -1.0424308926443318, 0.5365339946917028
            double[,] _gripper2base_array_data = new double[,] {
               {0.4285232296,0.1977596461,-0.5595823047,0.4738016082,0.3209333657,-0.2268795901,-0.7880605703},
               {0.2265712272,0.0261943502,-0.6661910850,0.5222343809,0.0461668455,-0.1315038186,-0.8413362107},
               {0.1058885008,0.0527681997,-0.7419267756,0.5081638565,-0.0263721340,-0.1696570449,-0.8439730402},
               {0.1127224767,-0.0420359377,-0.7413071786,0.5101706595,-0.0322252464,-0.1864402870,-0.8390038445 },
               {0.2592932751,-0.0329068529,-0.7162865014,0.5101641882,0.1265739325,-0.0153077050,-0.8505746380},
               {-0.1724239544,0.0084144761,-0.6998332592,0.4989369998,-0.0973210400,-0.1244194561,-0.8521210503 },
               {-0.0534271258,0.0771779706,-0.6845820453,0.5195499916,0.0511217081,-0.1691595167,-0.8359661686},
               {0.2598500029,0.0933230213,-0.6638257382,0.5673912325,0.1686973705,-0.1427337629,-0.7932436318},
            };//*/

            /* x, y, z, rx, ry, rz旋转向量//Robotpose3D第六组数据 eye-in-hand//rad//结果R= 
            double[,] _gripper2base_array_data = new double[,] {
               {0.400422,-0.0262847,0.365594,3.12734,-0.0857978,-0.0168582},
               {0.423347,-0.16857,0.340184,-2.73844,0.089013,0.123284},
               {0.42540,0.19543,0.29062,2.44351,-0.1777,-0.0722},
               {0.58088,-0.0541,0.32633,-2.9303,0.06957,0.98985},
               {0.2760,-0.033,0.3301,3.0813,0.0724,0.6077},
               {0.54011,0.09071,0.34623,2.62279,0.75876,-0.6927 },
               {0.57732,-0.1346,0.37990,-2.6764,1.04868,0.82177},
               {0.27844,-0.1371,0.28799,-0.8233,2.26319,0.36110},
            };//*/

            /* x, y, z, rx, ry, rz//zyx//Robotpose3D第七组数据(鱼香ros) eye-in-hand//rad//结果R= //T=tx, ty, tz=-0.020797737310176513,0.008898274229924556,0.0832451390072893
            double[,] _gripper2base_array_data = new double[,] {
               {1.1988093940033604, -0.42405585264804424, 0.18828251788562061, 151.3390418721659, -18.612399542280507, 153.05074895025035},
               {1.1684831621733476, -0.183273375514656, 0.12744868246620855, -161.57083804238462, 9.07159838346732, 89.1641128844487},
               {1.1508343174145468, -0.22694301453461405, 0.26625166858469146, 177.8815855486261, 0.8991159570568988, 77.67286224959672},
            };//*/


            //Campose3D数组形式数据
            //* x, y, z, rx, ry, rz//Campose3D第一组数据 eye-in-hand//rad
            double[,] _gTarget2cam_array_data = new double[,] { {63.0860126137940, 28.9610333193357, 228.975775155220, -0.1689782, -0.0952185, -2.9322395},
                                                             {59.7442727478359, 35.5178297229849, 212.756111469042, -0.1531844,-0.09427, -2.9339173},
                                                             {-11.9672134192024, 40.7904688256321, 233.030780465295, -0.1634773, -0.0894502, -2.7370669},
                                                             {23.1804880318842, 22.0425812434410, 248.264174105070, -0.3378254, 0.3893458, -2.8390293},
                                                             {-26.1091716393323, 46.8289595121710, 293.129807580481, 0.053972, -0.0675675, -2.6800584},
                                                             {54.9945317361700, 59.3875027697906, 267.443543451789, -0.0190344, -0.2970604, -2.8931871},
                                                             {102.142407851122, 23.4048279722072, 240.333220152591, -0.0602194,-0.2573558, -3.0144933},
                                                             {15.6263703407415, 39.1663493411513, 228.761080543106, -0.2303424, -0.1602727, -2.8007439},
                                                            };//*/
            /* x, y, z, rx, ry, rz//Campose3D第二组数据 hand-to-eye//rad//
            double[,] _gTarget2cam_array_data = new double[,] { {-48.4528696972388, -32.9428131930746, 161.065631617250, -0.1588792, -0.0973543,0.1438789},
                                                             {-62.0013541116839, -34.1299719569008, 164.980813774003, -0.0257775,-0.083593, 0.1545468},
                                                             {-55.0705381175398, -26.7901391225223, 143.932686495397, -0.0436629, 0.0437994, 0.1565832},
                                                             {-59.7908161911226, -10.7263017825663, 154.702510082749, -0.0523258, 0.0306989, 0.4666219},
                                                            };//*/
            /* x, y, z, rx, ry, rz//Campose3D第三组数据 hand-to-eye//rad//
            double[,] _gTarget2cam_array_data = new double[,] { 
                {27.6324, -53.0011, 450.756, 0.121504, -0.644529, 0.751784, 0.068111 },
                {-11.0989, -58.8588, 486.592, 0.120802 ,-0.578598, 0.805448 ,-0.043418 },
                {105.903, 202.833 ,440.508, 0.113889, 0.844049, 0.4874 ,-0.192487 },
                {9.23523 ,61.6075 ,450.489, 0.264242, -0.475601, 0.835247, 0.079638 },
                {32.2274 ,-11.2691, 439.197, -0.141923, 0.741567, -0.644095, -0.122793  },
                {44.0665 ,57.3595 ,458.317, -0.151062, 0.825444, -0.533026, -0.108193 },
                { 70.2627 ,164.667, 454.991, 0.016103, 0.936717, 0.317122, -0.147433 },
                { 130.146, 149.865, 404.127, 0.01959, 0.961815, -0.17028, -0.213384 },
                {-7.24689 ,-83.1921 ,444.861, 0.139143, -0.576354 ,0.802849, 0.062355  },
                { 272.997 ,-12.7755, 452.168, -0.073126, 0.957137, -0.235625, -0.151721 },
                {123.27 ,64.9186 ,474.978, -0.136874, 0.926738 ,-0.335978 ,-0.097673 },
                {87.2197 ,161.189 ,432.953, 0.021355, 0.987728 ,-0.149382 ,-0.04028  },
                { 178.767, 105.118 ,400.958, 0.066204 ,0.978165, -0.058339, -0.188168 },
                {171.641, 73.7655 ,494.311, -0.114663, 0.961687, 0.166144, -0.185488  },
                {188.657 ,245.794, 446.236, 0.063376, 0.893822 ,0.429315, -0.112931  }
                };//*/

            /* x, y, z, rx, ry, rz//zyx//Campose3D第四组数据 eye-in-hand//rad//
            double[,] _gTarget2cam_array_data = new double[,] { 
                {-0.53251566, -0.22288134, 2.99978054, -2.156571314953374, 17.96836783899952, 2.202110000510372},
                {-0.44328823, -0.22978619, 2.4122036, 10.04677425761553, 13.217722658140007, -0.7569076141309594},
                {-0.46990779, -0.32840453, 2.51890204, -0.5969738940715268, 19.56503505626854, 0.8598698487893537},
                {-0.48391721, -0.40351177, 2.27804699, 7.556253345854567, 16.256789925212452, 0.033714555538883036},
                {-0.57208587, -0.41269304, 2.58571284, 2.8633130363738593, 15.726493994549273, -0.7115981116920139 },
                {-0.49422756, -0.1702189, 2.87315438, -2.027545230194479, 16.468005086809477, -1.6960828431755508 },
                {-0.51349264, -0.28100387, 2.58778224, -1.2724871238262014, 21.200229674555533, -3.904702217196404 },
                {-0.53673736, -0.3091575, 2.71723579, -1.9171757971606331, 25.09560299293162, -3.955331059805344 },
                {-0.69796816, -0.40532881, 2.3216764, 2.493097502965668, 4.673219102172305, -0.6131885996737292},
                {-0.69438171, -0.16458288, 2.32894208, 3.074091031173201, 6.698654128807315, -1.3210092006224454},
                };//*/

            /* x, y, z, rx, ry, rz, rw//Campose3D第五组数据 eye-in-hand//rad//
            double[,] _gTarget2cam_array_data = new double[,] { 
                {0.4277459616,-0.0267754900,0.0822384718,0.3555198802,-0.2313832954,-0.6738796808,-0.6049409568},
                {0.4610891450,-0.0089776615,0.0394863697,-0.4168581229,0.4389318994,0.4953132086,0.6230833961},
                {0.4159190432,0.0112235849,0.0112504715,-0.3559197420,0.4898053987,0.4542453721,0.6535081871},
                {0.4143974465,0.0094036803,0.1073298638,-0.3438949365,0.4875942762,0.4507613906,0.6639294113},
                {0.3473267442,-0.1866610227,0.0651366494,0.4966101920,-0.4291949558,-0.5361047392,-0.5308123169 },
                {0.3883165305,-0.2249779584,0.0093256602,-0.3595692001,0.5529429756,0.4090559739,0.6305848604 },
                {0.4309364801,-0.2354261800,-0.0925819097,-0.3831460128,0.4244707870,0.5043032275,0.6470718186},
                {0.4362292324,-0.0778107597,-0.1007970399,0.4568889439,-0.3144470665,-0.5302104578,-0.6412896427 },
                };//*/

            /* x, y, z, rx, ry, rz旋转向量//Campose3D第六组数据 hand-to-eye//rad//
            double[,] _gTarget2cam_array_data = new double[,] {
               {0.11725,0.05736,0.32176,0.02860,-0.1078,3.06934},
               {0.07959,0.06930,0.29829,0.66126,-0.2127,2.99042},
               {0.14434,0.03227,0.40377,-0.9465,-0.1744,2.80668},
               {0.11008,0.05605,0.35730,0.12422,-1.0665,2.87604},
               {0.1131,0.0438171,0.299624,-0.058278,-0.514154,-3.06309},
               {0.057039,0.109464,0.415275,0.414777,0.750109,-2.49495 },
               {0.13702,0.00520,0.38190,0.26431,-0.7554,2.26885},
               {-0.03670,-0.04433,0.237292,0.807501,-0.27347,0.614594},
                };//*/

            /* x, y, z, rx, ry, rz//zyx//Campose3D第七组数据 eye-in-hand//rad//
            double[,] _gTarget2cam_array_data = new double[,] {
               {-0.16249272227287292, -0.047310635447502136, 0.4077761471271515, -56.98037030812389, -6.16739631361851, -115.84333735802369},
               {0.03955405578017235, -0.013497642241418362, 0.33975949883461, -100.87129330834215, -17.192685528625265, -173.07354634882094},
               {-0.08517949283123016, 0.00957852229475975, 0.46546608209609985, -90.85270962096058, 0.9315977976503153, 175.2059707654342},
                };//*/




            try
            {
                if (_gripper2base_array_data != null && _gTarget2cam_array_data != null)
                {

                    if (Robotpose3DdataTypes == "quat")
                    {
                        dataGridView6.Rows.Clear();
                        for (int i = 0; i < (_gripper2base_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView6.Rows.Add();
                            DataGridViewRow dgvr = dataGridView6.Rows[index];
                            for (int j = 0; j < _gripper2base_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gripper2base_array_data[i, j];
                            }
                        }
                    }
                    else if (Robotpose3DdataTypes == "euler")
                    {
                        dataGridView8.Rows.Clear();
                        for (int i = 0; i < (_gripper2base_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView8.Rows.Add();
                            DataGridViewRow dgvr = dataGridView8.Rows[index];
                            for (int j = 0; j < _gripper2base_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gripper2base_array_data[i, j];
                            }
                        }
                    }
                    else if (Robotpose3DdataTypes == "RotVector")
                    {
                        dataGridView2.Rows.Clear();
                        for (int i = 0; i < (_gripper2base_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView2.Rows.Add();
                            DataGridViewRow dgvr = dataGridView2.Rows[index];
                            for (int j = 0; j < _gripper2base_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gripper2base_array_data[i, j];
                            }
                        }
                    }

                    if (Campose3DdataTypes == "quat")
                    {
                        dataGridView9.Rows.Clear();
                        for (int i = 0; i < (_gTarget2cam_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView9.Rows.Add();
                            DataGridViewRow dgvr = dataGridView9.Rows[index];
                            for (int j = 0; j < _gTarget2cam_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gTarget2cam_array_data[i, j];
                            }
                        }
                    }
                    else if (Campose3DdataTypes == "euler")
                    {
                        dataGridView10.Rows.Clear();
                        for (int i = 0; i < (_gTarget2cam_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView10.Rows.Add();
                            DataGridViewRow dgvr = dataGridView10.Rows[index];
                            for (int j = 0; j < _gTarget2cam_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gTarget2cam_array_data[i, j];
                            }
                        }
                    }
                    else if (Campose3DdataTypes == "RotVector")
                    {
                        dataGridView7.Rows.Clear();
                        for (int i = 0; i < (_gTarget2cam_array_data.GetLength(0)); i++)
                        {
                            int index = dataGridView7.Rows.Add();
                            DataGridViewRow dgvr = dataGridView7.Rows[index];
                            for (int j = 0; j < _gTarget2cam_array_data.GetLength(1); j++)
                            {
                                dgvr.Cells[j].Value = _gTarget2cam_array_data[i, j];
                            }
                        }
                    }

                }
                else
                {
                    Debug.Assert(true, "No data!");
                }

            }
            catch (Exception)
            {

                throw;
            }



        }

    }
}

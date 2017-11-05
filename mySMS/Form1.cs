using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace mySMS
{
    public partial class Form1 : Form
    {
        private string url = "http://utf8.api.smschinese.cn/?";
        private string strUid = "Uid=";
        private string strKey = "&key=12e8c5a290b27437b257";
        private string strMob = "&smsMob=";
        private string strContent = "&smsText=";
        public Form1()
        {
            InitializeComponent();
        }

        public string GetHtmlFromUrl(string url)
        {
            string strRet = null;
            if (url == null || url.Trim().ToString() == "")
            {
                return strRet;
            }
            string targeturl = url.Trim().ToString();
            try
            {
                HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(targeturl);
                hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                hr.Method = "GET";
                hr.Timeout = 30 * 60 * 1000;
                WebResponse hs = hr.GetResponse();
                Stream sr = hs.GetResponseStream();
                StreamReader ser = new StreamReader(sr, Encoding.Default);
                strRet = ser.ReadToEnd();
            }
            catch (Exception ex)
            {
                strRet = null;
            }
            return strRet;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(txtPhone.Text.ToString().Trim() != "" && txtMsg.Text.ToString().Trim() != "" && txtSign.Text.ToString().Trim() != "")
            {
                url = url + strUid + "dluthwy" + strKey + strMob + txtPhone.Text + strContent + txtMsg.Text + "【"+ txtSign.Text + "】";
                string Result = GetHtmlFromUrl(url);
                if (Result == "-1")
                    MessageBox.Show("没有该用户账户");
                else if (Result == "-2")
                    MessageBox.Show("不是账户登陆密码");
                else if (Result == "-21")
                    MessageBox.Show("MD5接口密钥加密不正确");
                else if (Result == "-3")
                    MessageBox.Show("短信数量不足，请充值");
                else if (Result == "-11")
                    MessageBox.Show("该用户被禁用");
                else if (Result == "-14")
                    MessageBox.Show("短信内容出现非法字符");
                else if (Result == "-4")
                    MessageBox.Show("手机号格式不正确");
                else if (Result == "-6")
                    MessageBox.Show("IP限制");
                else if (Convert.ToInt32(Result) > 0)
                    MessageBox.Show("已成功发送" + Result + "条信息");
            }
            else
            {
                MessageBox.Show("发送格式不正确！");
            }
        }

        int sendNum;
        private void button1_Click(object sender, EventArgs e)//从Excel导入数据
        {
            try
            {
                sendNum = 0;
                DataTable dt = getData().Tables[0];
                Regex regex = new Regex(@"^[1][3-8]\d{9}$");
                string allPhone = "";
                foreach(DataRow row in dt.Rows)
                {
                    for(int i = 0;i < dt.Columns.Count; i ++)
                    {
                        if(regex.IsMatch(row[i].ToString()))
                        {                   
                            allPhone += row[i].ToString() + ",";
                            sendNum++;
                        }
                    }
                }
                MessageBox.Show("导入" + sendNum.ToString() + "位联系人");
                txtPhone.Text = allPhone;
            }
            catch (Exception ex)
            {
                return;
            }         
        }

        public DataSet getData()
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Excel(*.xlsx)|*.xlsx|Excel(*.xls)|*.xls";
            file.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            file.Multiselect = false;
            if (file.ShowDialog() == DialogResult.Cancel)
                return null;
            var path = file.FileName;
            string fileSuffix = System.IO.Path.GetExtension(path);
            if (string.IsNullOrEmpty(fileSuffix))
                return null;
            using (DataSet ds = new DataSet())
            {
                string strConn = "Provider=Microsoft.Ace.OleDb.12.0;" + "data source=" + path + ";Extended Properties='Excel 12.0; HDR=NO; IMEX=1'";
                OleDbConnection conn = new OleDbConnection(strConn);
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;
                strExcel = "select * from [sheet1$]";
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                myCommand.Fill(ds, "table1");
                return ds;
            }
        }
    }
}

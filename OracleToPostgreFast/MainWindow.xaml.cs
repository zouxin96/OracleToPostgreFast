using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OracleToPostgreFast
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// ///缺少原来就有引号和注释的处理 引号处理或许可以在最后把所有的[""] 替换成["]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

       
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Exchange();
        }


        private void  Exchange(bool isTest=false){
          
            bool startFromAS = cbAsStart.IsChecked==true;

            re.Text = OracleToPostgreFastLibrary.OracleToPostgreFastHelper.Exchange(text.Text, startFromAS,false,false);
          

}

        private void Button_Click_Test(object sender, RoutedEventArgs e)
        {

            string tmp = re.Text;
            if (!string.IsNullOrEmpty(a1.Text)) {
                tmp = tmp.Replace((a1.Text), a2.Text);
            }
            if (!string.IsNullOrEmpty(b1.Text))
            {
                tmp = tmp.Replace((b1.Text), b2.Text);
            }
            if (!string.IsNullOrEmpty(c1.Text))
            {
                tmp = tmp.Replace((c1.Text), c2.Text);
            }
            re_Copy1.Text = tmp;
            //  Exchange(true);
        }

        private void Button_Click_join(object sender, RoutedEventArgs e)
        {
            //try {

                string changeTmp = "(复杂文本临时替换)";
            //单词缓存清空
            tmpWord = "";
            //输入缓存 加个空格便于结尾处理

            //不对 这样无法处理嵌套 暂时替换嵌套处理
            string sqlText = tbJoinO.Text + " ";//替换掉，
            if (tbJoinO_CopyChange.Text.Length > 0)
            {
                sqlText = sqlText.Replace(tbJoinO_CopyChange.Text, changeTmp);
            }
            string result = "";

            string[] sqlTextSplitBYFROM = sqlText.SplitByFirst("FROM" );
            string select = sqlTextSplitBYFROM[0];
            string[] SelectLines = sqlTextSplitBYFROM[0].Split('\n');
            string caps = "".PadLeft(SelectLines[SelectLines.Length - 1].Length);
            string[] sqlTextSplitBYWHERE = sqlTextSplitBYFROM[1].SplitByFirst("WHERE");
            string strWhere = sqlTextSplitBYWHERE[1];
            string strFROM = sqlTextSplitBYWHERE[0];
            ///where部分 去掉换行后按and分割
            string[] strWhereLines = strWhere.Split(new[] { "AND" }, StringSplitOptions.RemoveEmptyEntries);
                //，分割from 
            string[] strFROMLines = strFROM.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> TableAsNameAndOriginalName = new Dictionary<string, string>();
            List<string> joinTableAsNames = new List<string>();
            foreach (string strFROMLineItem in strFROMLines)
            {
                string[] SplitFROMLineItem = strFROMLineItem.Replace("\r\n", "").Split(' ');
                string OriginalName = "";
                string AsName = "";
                foreach (string str in SplitFROMLineItem)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                       // if (OriginalName == "")
                       // {
                       //     OriginalName = str;
                       // }
                        AsName = str.Trim();
                    }
                }
                    OriginalName = strFROMLineItem.Trim().Substring(0, strFROMLineItem.Trim().Length - AsName.Length).Trim();

                if (string.IsNullOrEmpty(AsName) || string.IsNullOrWhiteSpace(AsName))
                {

                }
                else
                {
                    TableAsNameAndOriginalName.Add(AsName, OriginalName);
                }
            }
            bool childConditionFlag = false;
            int JoinType = 0;//0不链接1内2左3右
            Dictionary<string, List<string>> joinStrAndOns = new Dictionary<string, List<string>>();
            List<string> whereStrings = new List<string>();
            foreach (string strWhereLineItem in strWhereLines)
            {
                JoinType = 0;
                string joinTabelAsName = "";
                //按等号切割  不含=号的且不全是空格的直接丢到where部分  
                bool isJoinOn = false;
                //一个and条件有多行 视为复杂情况暂不处理
                if (strWhereLineItem.Trim().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
                {

                }
                else if (strWhereLineItem.Contains("="))
                {

                    string[] SplitWhereLineItemWord = strWhereLineItem.Split('=');

                    if (SplitWhereLineItemWord[0].Contains("(+)"))
                    {
                        joinTabelAsName = SplitWhereLineItemWord[0].Split('.')[0].Trim();
                        joinTableAsNames.Add(joinTabelAsName);
                        isJoinOn = true;
                        JoinType = 3;
                    }
                    else if (SplitWhereLineItemWord[1].Contains("(+)"))
                    {
                        joinTabelAsName = SplitWhereLineItemWord[1].Split('.')[0].Trim();
                        joinTableAsNames.Add(joinTabelAsName);
                        isJoinOn = true;
                        JoinType = 2;
                    }
                    else if (SplitWhereLineItemWord[0].Contains("\".\"") && SplitWhereLineItemWord[1].Contains("\".\""))
                    {
                        isJoinOn = true;
                        string tLeft = SplitWhereLineItemWord[0].Trim().Split('.')[0].Trim();
                        string tRight = SplitWhereLineItemWord[1].Trim().Split('.')[0].Trim();
                        JoinType = 1;
                        foreach (string TableAsName in TableAsNameAndOriginalName.Keys)
                        {

                            if (TableAsName == tLeft)
                            {
                                joinTabelAsName = tRight;
                                joinTableAsNames.Add(joinTabelAsName);
                                break;
                            };
                            if (TableAsName == tRight)
                            {
                                joinTabelAsName = tLeft;
                                joinTableAsNames.Add(joinTabelAsName);
                                break;
                            };
                        }
                    }
                }
                else
                {

                }
                if (isJoinOn)
                {
                    string joinStrHalf = "";
                    if (JoinType == 2)
                    {
                        joinStrHalf = "LEFT JOIN ";

                    }
                    else if (JoinType == 3)
                    {
                        joinStrHalf = "RIGHT JOIN ";

                    }
                    else if (JoinType == 1)
                    {
                        joinStrHalf = "INNER JOIN ";
                    }
                    joinStrHalf = joinStrHalf + TableAsNameAndOriginalName[joinTabelAsName] + " AS " + joinTabelAsName;
                    if (joinStrAndOns.ContainsKey(joinStrHalf))
                    {
                        joinStrAndOns[joinStrHalf].Add(strWhereLineItem.Replace("(+)", ""));
                    }
                    else
                    {
                        joinStrAndOns.Add(joinStrHalf, new List<string> { strWhereLineItem.Replace("(+)", "") });
                    }
                }
                else
                {
                    whereStrings.Add(strWhereLineItem);
                }
            }
            result = select;
            result += ("FROM\n\r");
            foreach (string asName in TableAsNameAndOriginalName.Keys)
            {
                if (joinTableAsNames.Contains(asName))
                {

                }
                else
                {
                    result += caps + "  " + TableAsNameAndOriginalName[asName] + " AS " + asName + ",\r\n";
                }

            }
            result = result.Substring(0, result.Length - 3);

            if (joinStrAndOns.Keys.Count > 0)
            {
                foreach (string joinStrHalf in joinStrAndOns.Keys)
                {
                    result += ("\r\n" + caps + joinStrHalf + " ON ");

                    foreach (string OneOnStr in joinStrAndOns[joinStrHalf])
                    {
                        result += OneOnStr.Replace("\r\n","") + " AND\r\n";
                    }
                    result = result.Substring(0, result.Length - 6);

                }
            }


            if (whereStrings.Count > 0)
            {
                result += (caps + "WHERE\n\r");  
                foreach (string str in whereStrings)
                {
                    result += str + " AND";
                }
                result = result.Substring(0, result.Length - 4);
            }
            tbJoinR.Text = result.Replace(changeTmp, tbJoinO_CopyChange.Text);
           // }
           // catch (Exception ex){
           //     MessageBox.Show("异常" +",可能是格式问题");
           // }
        }



    }
}

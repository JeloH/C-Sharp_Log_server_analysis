using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

 
using System.Data.OleDb;
using System.Configuration;

using System.Text.RegularExpressions;


using System.Windows.Forms.DataVisualization.Charting;


namespace WindowsFormsApplication12
{
    public partial class Form1 : Form
    {


        DataSet ds;
        DataView dv;
        OleDbDataAdapter da;

        string cmdSql;

        static int sql_injection = 0;
        static int Xss = 0;


        string s3 = @"provider=microsoft.jet.oledb.4.0;" + @"data source=..\\Debug\\report__9.mdb";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //    str1 = ConfigurationManager.ConnectionStrings["cn1"].ConnectionString;
            OleDbConnection con = new OleDbConnection(s3);
            OleDbCommand com = new OleDbCommand("select * from tb2", con);

            ds = new DataSet();
            da = new OleDbDataAdapter(com);
            da.Fill(ds);

            dataGridView1.DataSource = ds;
            dataGridView1.DataMember = ds.Tables[0].TableName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectPersonelPrintCard("select * from tb2");
        }



        public void SelectPersonelPrintCard(string strCodeSql)
        {


            try
            {

                OleDbConnection con = new OleDbConnection(s3);
    
                con.Open();
                //جلوگیری از تکرار کد_ملی

                OleDbDataAdapter da = new OleDbDataAdapter(strCodeSql, con);
                DataTable dt = new DataTable();




                da.Fill(dt);
                if (dt.Rows.Count == 0)
                {

                   // MsgBox.ShowMessage(this.Handle.ToInt32(), "چنین شماره عضویتی وجود ندارد", " توجه ", "تایید", "", "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

                }

                else
                {

                    if (dt.Rows[0]["field1"].ToString() != "")
                    {



                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                          // MessageBox.Show(  );


                            if (checkForSQLInjection(dt.Rows[i]["field1"].ToString()) == "True")
                            {

                                listBox1.Items.Add(dt.Rows[i]["field1"].ToString());

                                Form1.sql_injection = Form1.sql_injection + 1;
                            }



                            if (checkFor_XSS(dt.Rows[i]["field1"].ToString()) == "True")
                            {

                                listBox2.Items.Add(dt.Rows[i]["field1"].ToString());

                                Form1.sql_injection = Form1.Xss + 1;
                            }
                   
                        
                        }
 

 
                    }


                    label3.Text = listBox1.Items.Count.ToString();
                    label4.Text = listBox2.Items.Count.ToString();





                }

            }

            catch (Exception ex)
            {

              //  MsgBox.ShowMessage(this.Handle.ToInt32(), ex.Message.ToString(), " خطا ", "تایید", "", "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.RightAlign);

            }

        }


        public static string checkForSQLInjection(string userInput)
        {


            bool isSQLInjection = false;
            string[] sqlCheckList =  {"select", "drop", "insert", "delete","or",};
                                  
 
            string CheckString = userInput.Replace("'", "''");
            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
            {
                if ((CheckString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    isSQLInjection = true;
                }
            }
            return Convert.ToString(isSQLInjection);
        }

  public static string checkFor_XSS(string userInput)
  {
    bool isXss = false;
    string[] XssCheckList = { "<script>" , "<script", "/script>",
   "document.write","alert(" , "alert",
   "javascript", "location", "document.location","document.cookie", "function" 
   ,"windows.open","function",
   ".location"};

   string CheckString = userInput.Replace("'", "''");
    for (int i = 0; i <= XssCheckList.Length - 1; i++)
       {
         if ((CheckString.IndexOf(XssCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
          {
            isXss = true;
          }
        }
        return Convert.ToString(isXss);
      }

 

 
  private void Form1_Load(object sender, EventArgs e)
  {





      

  }

  private void button3_Click(object sender, EventArgs e)
  {
        
  }


  private bool checkForXSS1(string value)
  {
      return value.IndexOf("s") != -1;
  }

  public static string XSSProtect(string input)
  {
      string returnVal = input ?? "";

      returnVal = Regex.Replace(returnVal, @"\<script(.*?)\>(.*?)\<\/script(.*?)\>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
      returnVal = Regex.Replace(returnVal, @"\<style(.*?)\>(.*?)\<\/style(.*?)\>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

      // Thanks to Rubens Farias (http://stackoverflow.com/users/113794/rubens-farias) for help with this part
      while (Regex.IsMatch(returnVal, @"(<[\s\S]*?) on.*?\=(['""])[\s\S]*?\2([\s\S]*?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase))
      {
          returnVal = Regex.Replace(returnVal, @"(<[\s\S]*?) on.*?\=(['""])[\s\S]*?\2([\s\S]*?>)",
                          delegate(Match match)
                          {
                              return String.Concat(match.Groups[1].Value, match.Groups[3].Value);
                          }, RegexOptions.Compiled | RegexOptions.IgnoreCase);
      }

      return returnVal;
  }

  private bool checkForXSS(string value)
  {
      Regex regex = new Regex(@"/((\%3C)|<)[^\n]+((\%3E)|>)/I");

      if (regex.Match(value).Success) return true;

      return false;
  }

  private void button4_Click(object sender, EventArgs e)
  {
      this.Text = listBox1.Items.Count.ToString();

      Series series1 = new Series("Spiral");

      series1.ChartType = SeriesChartType.Column;

      //  series1.Points.AddXY("Normal", "");  
      series1.Points.AddXY("SqlInjection", Form1.sql_injection.ToString());
    //  series1.Points.AddXY("Ddos", "2");
      series1.Points.AddXY("XSS", Form1.Xss.ToString());

      series1.Points[0].Color = System.Drawing.Color.Blue;
    //  series1.Points[1].Color = System.Drawing.Color.Red;
      series1.Points[1].Color = System.Drawing.Color.Yellow;

      chart1.Series.Add(series1);



  }

  
  private void label1_Click(object sender, EventArgs e)
  {

  }


    }
}

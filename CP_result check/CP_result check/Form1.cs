using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Windows;
namespace CP_result_check
{
    public partial class Form1 : Form
    {
        public string targetDirectory = "";
        public string judgelot = "";
        public string pathAndlot = "";
        public string Testdatestring = "";
        public string destPath = "";
        public string destPathAll=""; //add lot
        public string PartNumberResult = "";
        public string TestDateforamtNormalize = "";
        public string TestStampforamtNormalize = "";
        public string PartBinningResult = "";
        public string TestLot ="";
        public string tempforlot = "";//Create File Name
        public string DisplayReuslt = "";
       // public bool ReadFile = false;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            
            dataGridView1.Rows.Clear();
            targetDirectory = "C:\\Users\\NI-SouthenCT\\Documents\\GitHub\\th_cp2_sts\\P2_cp2_prod\\logs";
            ///"C:\\Users\\user\\Documents\\Python Scripts\\";//data output root
            ////C:\Users\NI-SouthenCT\Documents\GitHub\th_cp2_sts\P2_cp2_prod\logs
            destPath = "C:\\TempData\\";
            LottextBox.Text = "";

        }
        private void Runbutton_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            dataGridView1.DefaultCellStyle.BackColor = Color.White;

            CheckInput();
            SearchFile(targetDirectory, Testdatestring, destPath);
            OpenAndReadFile(); //open and save
            string Gotoreadfile = "C:\\TempData\\";
            CountYield(Gotoreadfile, judgelot);
            string Openroot=Gotoreadfile + "yield_"+ judgelot+".csv";
            if(!File.Exists(Openroot)) goto ENDEXIT;
            ////////////////SHOW IN OI
            using (StreamReader sr = new StreamReader(Openroot))
            {
                
                string[] parts = sr.ReadToEnd().Split(';');
                foreach (string Resultline in parts)
                {
                    string[] Resultlinetemp = Resultline.Split(',');
                    if (Resultlinetemp[0]!= "Lot-Wafer" && Resultlinetemp[0] !="\r\n")
                    {
                        dataGridView1.Rows.Add(Resultlinetemp);
                    }
                    
                }
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    double datacolor = 0;
                    double databincolor = 0;
                    datacolor = Convert.ToDouble(dataGridView1.Rows[i].Cells[1].Value);
                    databincolor = Convert.ToDouble(dataGridView1.Rows[i].Cells[3].Value);
                    if (datacolor > 60)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Green;
                    }
                    //else if(databincolor >= 3)
                    //{
                    //    dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    //}
                    else
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }
                    if(databincolor >= 3)
                    {
                        dataGridView1.Rows[i].DefaultCellStyle.Font = new Font(Font.FontFamily, Font.Size, FontStyle.Bold);
                        dataGridView1.Rows[i].DefaultCellStyle.ForeColor = Color.Yellow;
                    }
                }


            }
            ////////////////Copy File
            string[] localfiles = Directory.GetFiles(Gotoreadfile, "*.CSV");
            foreach (string fileName in localfiles)
            {
                //copy to local
                if (fileName.Contains(judgelot))
                {
                    string destFile = "C:\\BK_YieldCheck\\" + Path.GetFileName(fileName);//back up path
                    File.Copy(fileName, destFile, true);
                } 
            }
            goto ReallyEND;
            ENDEXIT:
            MessageBox.Show("Can't Find file!!");
            ReallyEND:
            MessageBox.Show("Please check result!");

        }
        public void OpenAndReadFile()
        {
            bool ReadFile = false;

            // Open and Read
            string[] localfiles = Directory.GetFiles(@destPath, "P2_cp2_prod*site0*.csv");
            Array.Sort(localfiles, StringComparer.InvariantCulture);
            List<string> localfilelist = new List<string>();
            for (int j = 0; j < localfiles.Length; j++)
            {
                //by Eachfile
                string[] info = localfiles[j].Split(new string[] { Testdatestring + "\\" }, StringSplitOptions.None);
                //if file=input lot go next , if no go skip
                if (ReadFile == true) goto ENDReadfile;
                //by Read Each file
                string[] dataline = System.IO.File.ReadAllLines(localfiles[j]);
                List<string> ResultList = new List<string>();

                for (int k = 0; k < dataline.Length; k++)
                {
                    int tempforclear = dataline.Length;
                    string[] datalinesplit = dataline[k].Split(',');
                    ////(1)Part Number, B32250-01W01X1Y0
                    if (datalinesplit[0] == "Part Number  ")
                    {
                        string[] Temp = datalinesplit[1].Split(',');
                        PartNumberResult = Temp[0];
                        string[] TestLottemp = PartNumberResult.Split('-');
                        tempforlot = TestLottemp[0];
                        //Check lot input be correct!!!
                        //if (tempforlot != LottextBox.Text) 
                        if (tempforlot == judgelot)
                        {
                            ReadFile = true;
                        }
                        else
                        {
                            //ReadFile = true;
                            Array.Clear(dataline, 0, tempforclear);
                            goto skipline;
                        }
                        destPathAll = destPath + "\\" + tempforlot + ".csv";
                        if (!File.Exists(destPathAll))
                        {
                            StreamWriter Temptestfile = new System.IO.StreamWriter(destPathAll);
                            Temptestfile.WriteLine("PartNumber,TestDate,TestTime,PartBinning");
                            Temptestfile.Close();
                        }
                       
                    }
                    ////(2)/Test Date    ,5 / 29 / 2019
                    if (datalinesplit[0] == "Test Date    ")
                    {
                        string[] TestDateforamt = datalinesplit[1].Split('/');
                        TestDateforamtNormalize = TestDateforamt[0] + "_" + TestDateforamt[1] + "_" + TestDateforamt[2];
                    }
                    ////(3)////Time Stamp,4:09:31 PM
                    if (datalinesplit[0] == "Time Stamp   ")
                    {
                        string[] TestStampforamt = datalinesplit[1].Split(':');
                        string[] AMPMformat = TestStampforamt[2].Split(' ');
                        if (AMPMformat[1] == "PM")
                        {
                            int testHour = Int32.Parse(TestStampforamt[0]);
                            testHour = testHour + 12;
                            TestStampforamt[0] = testHour.ToString();
                        }
                        TestStampforamtNormalize = TestStampforamt[0]  + TestStampforamt[1];
                    }
                    ////(4)Part Binning        ,HW: 1:Pass           ,SW: 1:Pass
                    if (datalinesplit[0] == "Part Binning        ")
                    {
                        string[] temp = datalinesplit[2].Split(',');
                        PartBinningResult = temp[0];
                    }
                    string Resultline = PartNumberResult + "," + TestDateforamtNormalize + "," + TestStampforamtNormalize + "," + PartBinningResult;
                    if (PartNumberResult != "" && TestDateforamtNormalize != "" && TestStampforamtNormalize != "" && PartBinningResult != "")
                    {
                        ResultList.Add(Resultline);
                        PartNumberResult = "";
                        TestDateforamtNormalize = "";
                        TestStampforamtNormalize = "";
                        PartBinningResult = "";
                    }
                }
                //Chekck
                goto ENDParsing;
          
            ENDParsing:
                //Sort File
                ResultList.Sort();
                ///Remove Duplicate
                ///test result in the same date
                for (int i = 0; i < ResultList.Count(); i++)
                {
                    string test1 = ResultList[i];
                    string[] temptest1 = test1.Split(',');
                    string comparetemp1 = temptest1[0] + "_" + temptest1[2];
                    int z = 0;
                    z = i + 1;
                    if (z == ResultList.Count()) goto skipline2;
                    string test2 = ResultList[z];
                    string[] temptest2 = test2.Split(',');
                    string comparetemp2 = temptest2[0] + "_" + temptest2[2];
                    if (temptest1[0] == temptest2[0])
                    {
                        //Check Day
                        int testHour1 = Int32.Parse(temptest1[2]);
                        int testHour2 = Int32.Parse(temptest2[2]);
                        
                        if (temptest1[1] == temptest2[1])///the same day
                        {
                            if (testHour2 > testHour1)
                            {
                                ResultList.RemoveAt(i);
                            }

                        }
                        else
                        {
                            //different day
                            if (testHour2 < testHour1)
                            {
                                ResultList.RemoveAt(i);
                            }

                        }
                     
                    }
                }
            skipline2:
                using (StreamWriter Temptestfile = File.AppendText(destPathAll))
                {
                    foreach (string Resultline in ResultList)
                    {
                        Temptestfile.WriteLine(Resultline);
                    }
                    Temptestfile.Close();
                }
            skipline:
                MessageBox.Show("NextFile");

            }
            ENDReadfile:
            MessageBox.Show("GO to CHECK Yield");
        }
        public void CheckInput()
        {
            ///////// Input BY op
            /////Months -MonthstextBox
            /////Days-DaytextBox
            judgelot = LottextBox.Text.ToUpper();
            string Monthstemp = MonthstextBox.Text;
            int MonthstempInt = Int32.Parse(Monthstemp);
            string Daytemp = DaytextBox.Text;
            Testdatestring = Daytemp + Monthstemp + "2019"; //"29May2019";
            ///////////////////////////////
            int[] MonthNumber = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            string[] MonthString = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            for (int x = 1; x < MonthNumber.Length; x++)
            {
                if (MonthstempInt == MonthNumber[x])
                {
                    Monthstemp = MonthString[x];
                }
            }
            ////////////////

            Testdatestring = Daytemp + Monthstemp + "2019"; //"29May2019";

            if ((judgelot == "") || (Testdatestring == ""))
            {
                MessageBox.Show("Lot or Time, Can't be empty!!");
                Environment.Exit(Environment.ExitCode);
            }
        }
        public static void SearchFile(string targetDirectory, string Testdatestring,string @destPath)
        {
            //clear TempData
            string[] tempfiles = Directory.GetFiles(@destPath, "*.csv");
            foreach (string tempfile in tempfiles)
            {
                File.Delete(tempfile);
            }
            //Copy
            string[] txtFiles = Directory.GetFiles(@targetDirectory, "*P2_cp2_prod*site0*.csv"); 
            foreach (string fileName in txtFiles)
            {
                if (fileName.Contains(Testdatestring))
                {
                    //copy to local
                    string destFile = @destPath + Path.GetFileName(fileName);
                    File.Copy(fileName, destFile, true);
                }
            }
        }
        public static void CountYield(string Gotoreadfile,string judgelot)
        {

            int TempBINCOUNT1 = 0;
            int TempBINCOUNT7 = 0;
            int TempBINCOUNT3 = 0;
            int TempBINCOUNT4 = 0;
            int TempBINCOUNT8 = 0;
            int TempBINCOUNT2 = 0;
            int TempBINCOUNT6 = 0;
            int TempBINCOUNT5 = 0;
            int TempBINCOUNT9 = 0;
            int TempBINCOUNT10 = 0;
            int TempBINCOUNT20 = 0;
            int TempBINCOUNT99 = 0;
            judgelot = judgelot + ".csv";
            string[] txtFiles = Directory.GetFiles(@Gotoreadfile, judgelot);
            if (txtFiles.Count() == 0) goto showerror;
            //////////////////////////////////////////////////////////////////
            string[] CPRecordLines = System.IO.File.ReadAllLines(txtFiles[0]);
            int x=0;
            for (int j = 1; j < CPRecordLines.Length; j++)
            {
               
                string[] CPBINreuslt = CPRecordLines[j].Split(','); //Bin
                string[] LotwithWafertemp=CPBINreuslt[0].Split('-'); // lot and wafer
                string[] LotwithWafer = LotwithWafertemp[1].Split('W'); // lot and wafer
                string[] CPBINreusltSplit = CPBINreuslt[3].Split(':');
                /////////////////////////////////
                ///
                
                int casebin = Int32.Parse(CPBINreusltSplit[1]);//  CPBINreusltSplit[0];
                decimal totalcount = 0;
                decimal totalYield = 0m;
                string exportLotWafer = LotwithWafertemp[0] + "-" + LotwithWafer[0];
                string tempforExport;
                if (x <= 28)
                {
                    //}
                    switch (casebin)
                    {
                        case 1: //PASS BIN1
                            TempBINCOUNT1 = TempBINCOUNT1 + 1;
                            break;
                        case 3://
                            TempBINCOUNT3 = TempBINCOUNT3 + 1;
                            break;
                        case 7://
                            TempBINCOUNT7 = TempBINCOUNT7 + 1;
                            break;
                        case 4://
                            TempBINCOUNT4 = TempBINCOUNT4 + 1;
                            break;
                        case 8://
                            TempBINCOUNT8 = TempBINCOUNT8 + 1;
                            break;
                        case 5://
                            TempBINCOUNT5 = TempBINCOUNT5 + 1;
                            break;
                        case 6: //
                            TempBINCOUNT6 = TempBINCOUNT6 + 1;
                            break;
                        case 2://
                            TempBINCOUNT2 = TempBINCOUNT2 + 1;
                            break;
                        case 9://
                            TempBINCOUNT9 = TempBINCOUNT9 + 1;
                            break;
                        case 10://
                            TempBINCOUNT10 = TempBINCOUNT10 + 1;
                            break;
                        case 20://
                            TempBINCOUNT20 = TempBINCOUNT20 + 1;
                            break;
                        case 99://
                            TempBINCOUNT99 = TempBINCOUNT99 + 1;
                            break;
                    }
                    totalcount = TempBINCOUNT1 + TempBINCOUNT3 + TempBINCOUNT7 + TempBINCOUNT4 + TempBINCOUNT8 + TempBINCOUNT5+ TempBINCOUNT6 + TempBINCOUNT2 + TempBINCOUNT9 + TempBINCOUNT10 + TempBINCOUNT20 + TempBINCOUNT99;
                    totalYield = (TempBINCOUNT1 / totalcount)* 100; //%
                    
                    x = x + 1;
                    if (x == 28)
                    {
                        
                        string exportYieldpath = "C:\\TempData\\" +"yield_"+ judgelot;
                        if (!File.Exists(exportYieldpath))
                        {
                            StreamWriter Temptestfile = new System.IO.StreamWriter(exportYieldpath);
                            Temptestfile.WriteLine("Lot-Wafer,Yield,SWBIN1-Pass,SWBIN2-Continuity Fail,SWBIN3-Power Short Fail,SWBIN4-Leakage Fail,SWBIN5-Vo/Vi Level Fail,SWBIN6-Critical Pattern Fail,SWBIN7-UPU Pattern Fail,SWBIN8-mBIST Pattern Fail,SWBIN9-PowerUp Fail,SWBIN10-PowerDown Fail,SWBIN20-Other Fail,SWBIN99-Generic Error;");//(tempforExport);
                            Temptestfile.Close();
                        }
                        using (StreamWriter Temptestfile = File.AppendText(exportYieldpath))
                        {
                            tempforExport = exportLotWafer + "," + totalYield +","+ TempBINCOUNT1 + "," + TempBINCOUNT2 + "," + TempBINCOUNT3 + "," + TempBINCOUNT4 + "," + TempBINCOUNT5 + "," + TempBINCOUNT6 + "," + TempBINCOUNT7 + "," + TempBINCOUNT8 + "," + TempBINCOUNT9 + "," + TempBINCOUNT10 + "," + TempBINCOUNT20 + "," + TempBINCOUNT99 + ",;";
                            Temptestfile.WriteLine(tempforExport);
                            Temptestfile.Close();
                        }

                        totalcount = 0;
                        totalYield = 0;
                        TempBINCOUNT1 = 0;
                        TempBINCOUNT2 = 0;
                        TempBINCOUNT3 = 0;
                        TempBINCOUNT4 = 0;
                        TempBINCOUNT5 = 0;
                        TempBINCOUNT6 = 0;
                        TempBINCOUNT7 = 0;
                        TempBINCOUNT8 = 0;
                        TempBINCOUNT9 = 0;
                        TempBINCOUNT10 = 0;
                        TempBINCOUNT20 = 0;
                        TempBINCOUNT99 = 0;
                        x = 0;
                    }

                }
            }
        showerror:
            MessageBox.Show("Check Yield Done!!!");
        }

 
    }
}

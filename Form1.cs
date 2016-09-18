using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RunTimer
{
    public partial class Form1 : Form
    {
        private FileStream m_timesStream;
        private StreamWriter m_timesWriter;
        private FileStream m_finisherStream; 
        private StreamWriter m_numbersWriter;
        private static int m_backupFileNumber = 0;

        public Form1()
        {
            InitializeComponent();
            m_timesStream = new FileStream(ConfigurationManager.AppSettings["times"], FileMode.Create, FileAccess.Write, FileShare.Read);
            m_timesWriter = new StreamWriter(m_timesStream);
            m_finisherStream = new FileStream(ConfigurationManager.AppSettings["finishers"], FileMode.Create, FileAccess.Write, FileShare.Read);
            m_numbersWriter = new StreamWriter(m_finisherStream);
            m_timesWriter.AutoFlush = true;
            m_numbersWriter.AutoFlush = true;
        }

        private void CreateResultList()
        {
            FileStream finisherStream = new FileStream(ConfigurationManager.AppSettings["finishers"], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader finisherReader = new StreamReader(finisherStream);
            FileStream timesStream = new FileStream(ConfigurationManager.AppSettings["times"], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader timesReader = new StreamReader(timesStream);
            StreamReader startlistReader = new StreamReader(ConfigurationManager.AppSettings["startlist"], Encoding.Default);
            StreamReader startTimesReader = new StreamReader(ConfigurationManager.AppSettings["starttimes"]);
            StreamWriter resultsWriter = new StreamWriter(ConfigurationManager.AppSettings["results"], false, Encoding.Default);
            try
            {
                resultsWriter.AutoFlush = true;
                textBox3.Clear();
                if (!string.IsNullOrWhiteSpace(this.textBox1.Text))
                {
                    string[] lines = this.textBox1.Text.Split(new string[] { "\r", "\n", "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                    using (
                        StreamWriter backupWriter =
                            new StreamWriter(
                                ConfigurationManager.AppSettings["finishers"].Substring(0,
                                    ConfigurationManager.AppSettings["finishers"].Length - 4) + "_" + m_backupFileNumber +
                                ".txt"))
                    {
                        foreach (string l in lines)
                        {
                            m_numbersWriter.WriteLine(l);
                            backupWriter.WriteLine(l);
                        }
                        backupWriter.Close();
                    }
                }

                Dictionary<int, DateTime> startTimes = new Dictionary<int, DateTime>();
                string startTimeLine;
                startTimesReader.ReadLine();
                while (!string.IsNullOrEmpty(startTimeLine = startTimesReader.ReadLine()))
                {
                    string[] startTimeParts = startTimeLine.Split(';');

                    startTimes.Add(Convert.ToInt32(startTimeParts[0]), Convert.ToDateTime(startTimeParts[1]));
                }
                Dictionary<int, Runner> runners = new Dictionary<int, Runner>();
                startlistReader.ReadLine();
                string starter;
                while (!string.IsNullOrEmpty(starter = startlistReader.ReadLine()))
                {
                    string[] starterParts = starter.Split(';');
                    int startNumber = -1;
                    try
                    {
                        if (string.IsNullOrWhiteSpace(starterParts[3]))
                        {
                            continue;
                        }
                        startNumber = Convert.ToInt32(starterParts[3]);
                    }
                    catch (Exception)
                    {
                    }
                    int category = -1;
                    try
                    {
                        category = Convert.ToInt32(starterParts[2]);
                    }
                    catch (Exception)
                    {
                    }
                    DateTime startTime = startTimes.ContainsKey(category) ? startTimes[category] : startTimes.Values.Min();
                    runners.Add(startNumber, new Runner()
                    {
                        Name = starterParts[0],
                        StartNumber = startNumber,
                        StartTime = startTime,
                        Category = category
                    });

                }

                string finisher;
                string result;
                while (!string.IsNullOrEmpty(finisher = finisherReader.ReadLine()))
                {
                    string timeLine = timesReader.ReadLine();
                    DateTime time = Convert.ToDateTime(timeLine);
                    try
                    {
                        int startNumber = Convert.ToInt32(finisher);
                        Runner runner;
                        if (runners.ContainsKey(startNumber))
                        {
                            runner = runners[startNumber];
                        }
                        else
                        {
                            runner = new Runner()
                            {
                                Name = "okänd löpare",
                                StartNumber = startNumber,
                                StartTime = startTimes.Values.Min()
                            };
                        }
                        result = runner.StartNumber + ";" + runner.Name + ";" + runner.Category + ";" + (time - runner.StartTime);
                        resultsWriter.WriteLine(result);
                        textBox3.AppendText(runner.StartNumber + "\t" + runner.Name + "\t" + runner.Category + "\t" + (time - runner.StartTime) + "\r\n");
                    }
                    catch (Exception)
                    {

                    }
                }

            }
            finally 
            {
                if (finisherReader != null)
                {
                    finisherReader.Close();
                    finisherReader.Dispose();
                }
                if (finisherStream != null)
                {
                    finisherStream.Close();
                    finisherStream.Dispose();
                }
                if (timesReader != null)
                {
                    timesReader.Close();
                    timesReader.Dispose();
                }
                if (timesStream != null)
                {
                    timesStream.Close();
                    timesStream.Dispose();
                }
                if (startlistReader != null)
                {
                    startlistReader.Close();
                    startlistReader.Dispose();
                }
                if (startTimesReader != null)
                {
                    startTimesReader.Close();
                    startTimesReader.Dispose();
                }
                if (resultsWriter != null)
                {
                    resultsWriter.Close();
                    resultsWriter.Dispose();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.ForeColor = Color.Red;
            var tapTime = DateTime.Now;
            m_timesWriter.WriteLine(tapTime);
            textBox2.AppendText(tapTime.ToLongTimeString() + "\r\n");
            Thread.Sleep(10);
            this.ForeColor = Color.Green;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                CreateResultList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ooops, det där gick inte bra. Låt den knappen vara tills vidare...");
            }
        }

    }
}

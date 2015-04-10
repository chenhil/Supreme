using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Supreme.Properties;
using System.Collections;
using System.Reflection;

namespace Supreme
{
    public partial class Form1 : Form
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool InternetSetCookie(string lpszUrlName, string lpszCookieName, string lpszCookieData);

        List<string> Uris = new List<string>();
        List<string> sizechart = new List<string>();
        //string nDateTime = DateTime.Now.ToString("hh:mm:ss.ffff tt") + " - ";
        int intOriginalExStyle = -1;
        bool bEnableAntiFlicker = true;
        string size = null;
        
        public Form1()
        {
            Form1.CheckForIllegalCrossThreadCalls = false;
            //ToggleAntiFlicker(false);

            InitializeComponent();
            appendtext("Application Loaded");

            //delete all the cookies before we start the program
            string[] Cookies = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            int notDeleted = 0;
            foreach (string CookieFile in Cookies)
            {
                try
                {
                    System.IO.File.Delete(CookieFile);

                }
                catch (Exception ex)
                {
                    notDeleted++;
                }

            }

            //this.ResizeBegin += new EventHandler(Form1_ResizeBegin);
            //this.ResizeEnd += new EventHandler(Form1_ResizeEnd);


            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            //SetStyle(ControlStyles.Opaque, true);
            //SetStyle(ControlStyles.ResizeRedraw, true);

           


            //change the default text of the dropdown list
            comboBoxState.Text = "OR";
            comboBoxCountry.Text = "USA";
        }




      
        protected override CreateParams CreateParams
        {
            get
            {
                if (intOriginalExStyle == -1)
                {
                    intOriginalExStyle = base.CreateParams.ExStyle;
                }
                CreateParams cp = base.CreateParams;

                if (bEnableAntiFlicker)
                {
                    cp.ExStyle |= 0x02000000; //WS_EX_COMPOSITED
                }
                else
                {
                    cp.ExStyle = intOriginalExStyle;
                }

                return cp;
            }
        }


        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            ToggleAntiFlicker(true);
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            ToggleAntiFlicker(false);
        }

        private void ToggleAntiFlicker(bool Enable)
        {
            bEnableAntiFlicker = Enable;
            //hacky, but works
            //this.MaximizeBox = true;
        }


        private void appendtext(string mystr)
        {
            Int32 maxsize = 5000;
            Int32 dropsize = maxsize / 4;
            if (richTextBox1.Text.Length > maxsize)
            {
                Int32 endmarker = richTextBox1.Text.IndexOf('\n', dropsize) + 1;
                if (endmarker < dropsize)
                    endmarker = dropsize;
                richTextBox1.Select(0, endmarker);
                richTextBox1.Cut();

            }
            try
            {
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectionStart = 0;
                richTextBox1.AppendText((DateTime.Now.ToString("hh:mm:ss tt") + " - " + mystr));
                richTextBox1.AppendText(Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }
            catch (Exception ex)
            {

            }
            /*
            if (richTextBox1.Text.Length > 0)
            {
                richTextBox1.AppendText(Environment.NewLine);
            }

            richTextBox1.AppendText(DateTime.Now.ToString("hh:mm:ss tt") + " - " + mystr);
            richTextBox1.ScrollToCaret();
             * */
        }

        private Boolean geturl(DoWorkEventArgs e)
        {
          
            //string size = comboBoxSize.GetItemText(comboBoxSize.SelectedItem);
            //string item = textBoxkeyword.Text;
            string color = textBoxcolor.Text.ToLower();
            //string tempcomp = item.Replace(" ", "-").ToLower();
            if (backgroundWorker1.CancellationPending)
            {
                e.Cancel = true;
                return true;

            }
       


            if(textBoxkeyword.Text.Contains("http://www.supremenewyork.com/"))
            {


                //still need to compare the colors
                Uris.Add(textBoxkeyword.Text);
                if (getsize(textBoxkeyword.Text, e))
                {
                    appendtext("Size Found adding product to cart.");
                    addtocart();
                    return true;
                }
                else
                {
                    appendtext("Product Not Live yet.");

                    return false;
                }
            
               
         
            }
            else
            {
                if (textBoxkeyword.Text.Contains(" "))
                {
                    string[] tempcomp = textBoxkeyword.Text.Split(null);
                    var web = new HtmlWeb();
                    var doc = web.Load("http://www.supremenewyork.com/shop/all");
                    appendtext("Looking for product.");

                    var nodes = doc.DocumentNode.SelectNodes(string.Format("//a[contains(@href,'{0}')]", tempcomp[0]));

                    if (nodes == null)
                    {
                        appendtext("Unable to find product");
                        return false;
                    }

                    foreach (var node in nodes)
                    {
                        string producturl = node.Attributes["href"].Value;
                        if (producturl.Contains(color) && producturl.Contains(tempcomp[1]))
                        {
                            appendtext("Product Found");
                            Uris.Add(producturl);
                            if (getsize(producturl, e))
                            {
                                appendtext("Size Found adding product to cart.");
                                addtocart();
                                return true;
                            }
                            else
                            {
                                
                                return false;
                            }

                        }
                    }
                    appendtext("Product Found but matching color");
                    return false;

                }

                else
                {
                    //only one keyword
                    var web = new HtmlWeb();
                    var doc = web.Load("http://www.supremenewyork.com/shop/all");

                    appendtext("Looking for product.");
                    var nodes = doc.DocumentNode.SelectNodes(string.Format("//a[contains(@href,'{0}')]", textBoxkeyword.Text));

                    if (nodes == null)
                    {
                        appendtext("Unable to find product");
                        return false;
                    }

                    foreach (var node in nodes)
                    {
                        string producturl = node.Attributes["href"].Value;
                        if (producturl.Contains(color))
                        {
                            appendtext("Product Found");
                            Uris.Add("http://www.supremenewyork.com" + producturl);
                            if (getsize("http://www.supremenewyork.com" + producturl, e))
                            {
                                appendtext("Size Found adding product to cart.");
                                addtocart();
                                return true;
                            }
                            else
                            {

                                //appendtext("Invalid Size/Sold out. Try Again");
                                return false;
                            }

                        }
                    }
                    appendtext("Product Found but matching color");
                    return false;
                }



            }






        }



        private Boolean getsize(string url, DoWorkEventArgs e)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var options = doc.DocumentNode.SelectNodes("//option");

            //item sold out, option can't find any nodes
            if (options == null)
            {
                //appendtext("Sold out. " );
                return false;
            }

            foreach(var node in options)
            {
                if (node.NextSibling != null)
                {
                    string text = node.NextSibling.InnerText;
                    if (text.IndexOf(size,StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sizechart.Add(node.Attributes["value"].Value);
                        return true;
                    }
                }
        
            }

            //not returning true not false since node is empty
            //there are sizes left but not the size user wants 
            appendtext("Invalid Size. Try Again");
            return false;

        }



        private void addtocart()
        {
            //three data for post, size, authtoken, and URT8
            string source;

            //get the cookie first
            CookieCollection cookies = new CookieCollection();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Uris[0]);
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookies);

            //get the auth token
            using (StreamReader authreader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                source = authreader.ReadToEnd();
            }
            //need the auth token
            string token = Regex.Match(source, "authenticity_token.+?value=\"(.+?)\"").Groups[1].Value;

            //need the POST url 
            string action = Regex.Match(source, "UTF-8.+?action=\"(.+?)\"").Groups[1].Value;

            //get the reponse from the server and save the cookies from the first request
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            cookies = response.Cookies;
            response.Close();

            string myuri = Uris[0];

            string formparam = string.Format("utf8=%E2%9C%93&authenticity_token={0}&size={1}&commit=add to cart", token, sizechart[0]);
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create("http://www.supremenewyork.com"+ action);

            //webreq.CookieContainer.Add(cookies);
            webreq.CookieContainer = request.CookieContainer;
            webreq.Method = "POST"; //set a POST method
            webreq.Referer = Uris[0];
            webreq.ContentType = "application/x-www-form-urlencoded";
            webreq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36";
            webreq.KeepAlive = true;
            webreq.AllowAutoRedirect = true;

            byte[] bytes = Encoding.UTF8.GetBytes(formparam);
            webreq.ContentLength = bytes.Length;


            //write
            Stream postdata = webreq.GetRequestStream(); //open connection
            postdata.Write(bytes, 0, bytes.Length); //send the data
            postdata.Close();

            //get the final response from the server
            HttpWebResponse resp = (HttpWebResponse)webreq.GetResponse();
            webreq.CookieContainer.Add(resp.Cookies);
            resp.Close();


            //check if item has been added to cart
            HttpWebRequest webreq2 = (HttpWebRequest)WebRequest.Create("http://www.supremenewyork.com/shop/cart");
            webreq2.CookieContainer = webreq.CookieContainer;
            HttpWebResponse resp2 = (HttpWebResponse)webreq2.GetResponse();
            StreamReader _answer2 = new StreamReader(webreq2.GetResponse().GetResponseStream());
            Stream answer2 = resp2.GetResponseStream();
            string reply2 = _answer2.ReadToEnd();
            resp2.Close();



            //string item = textBoxkeyword.Text;
            //string color = textBoxcolor.Text.ToLower();
            if (reply2.Contains(textBoxcolor.Text.ToLower()))
            {
                //proceed to check out

                //update user
                appendtext("Item: " + textBoxkeyword.Text + " " + textBoxcolor.Text + " " + size + " added to cart");
                appendtext("Please check out the item");


                //getting all the cookies from the cookie container
                Hashtable table = (Hashtable)webreq2.CookieContainer.GetType().InvokeMember("m_domainTable",
                                                             BindingFlags.NonPublic |
                                                             BindingFlags.GetField |
                                                             BindingFlags.Instance,
                                                             null,
                                                             webreq2.CookieContainer,
                                                             new object[] { });

               

                
                string cookie_string = string.Empty;
                foreach (var key in table.Keys)
                {
                    foreach (Cookie cookie in webreq2.CookieContainer.GetCookies(new Uri(string.Format("http://{0}/", key))))
                    {
                        cookie_string += cookie.ToString() + ";";

                        //InternetSetCookie({0}, {1}, cookie.Name, cookie.Value);
                        InternetSetCookie("http://www.supremenewyork.com/shop/cart", cookie.Name, cookie.Value);

                        InternetSetCookie("http://www.supremenewyork.com/checkout", cookie.Name, cookie.Value);


                    }
                }

                webBrowser1.Navigate("http://www.supremenewyork.com/checkout");

                //problem, the cookies at checkout are getting saved

            } 

            else
            {
                MessageBox.Show("No item on sale");
            }


        }


   

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxkeyword.Text))
            {
                MessageBox.Show("Please Fill Out Keyword");
            }
            else if (size == null)
            {
                MessageBox.Show("Please Select a Size");
            }
            else
            {
                backgroundWorker1.RunWorkerAsync();
                button1.Enabled = false;
                buttonStop.Enabled = true;
            }
            
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            
            while (!geturl(e))
            {
                Thread.Sleep(100);
            }
            //geturl(e);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Cancelled)
            {
                MessageBox.Show("Cancelled");
                buttonStop.Enabled = false; ;
                button1.Enabled = true;

            }
            else if (e.Error != null) //check if the worker has been canceled or if an error occurred
            {
                MessageBox.Show("Error. Details " + (e.Error as Exception).ToString());
            }
            else
            {
                string end = (string)e.Result;
                //MessageBox.Show("Copped");
                button1.Enabled = true;
                buttonStop.Enabled = false; ;

            }

        }

      

        private void textBoxCardNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void textBoxCCV_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();

        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            //fill in all the information at check out
            webBrowser1.Document.All.GetElementsByName("order[terms]")[0].InvokeMember("Click");

            webBrowser1.Document.GetElementById("order_billing_name").SetAttribute("value", textBoxFN.Text + " " + textBoxLN.Text);
            webBrowser1.Document.GetElementById("order_email").SetAttribute("value", textBoxEmail.Text);
            webBrowser1.Document.GetElementById("order_tel").SetAttribute("value", textBoxTelephone.Text);
            webBrowser1.Document.GetElementById("bo").SetAttribute("value", textBoxAddress1.Text);
            webBrowser1.Document.GetElementById("order_billing_zip").SetAttribute("value", textBoxZip.Text);
            webBrowser1.Document.GetElementById("order_billing_city").SetAttribute("value", textBoxCity.Text);
            webBrowser1.Document.GetElementById("order_billing_state").SetAttribute("value", "OR");
            webBrowser1.Document.GetElementById("order_billing_country").SetAttribute("value", "USA");
            webBrowser1.Document.GetElementById("credit_card_type").SetAttribute("value", comboBoxPaymentType.SelectedItem.ToString());
            webBrowser1.Document.GetElementById("onb").SetAttribute("value", textBoxCardNum.Text);
            webBrowser1.Document.GetElementById("credit_card[month]").SetAttribute("value", comboBoxExpMonth.SelectedItem.ToString());
            webBrowser1.Document.GetElementById("credit_card[year]").SetAttribute("value", comboBoxyear.SelectedItem.ToString());
            webBrowser1.Document.GetElementById("number_v").SetAttribute("value", textBoxCCV.Text);

            if (checkBoxAutoCheckout.Checked)
            {
                webBrowser1.Document.All.GetElementsByName("commit")[0].InvokeMember("Click");

            }
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings.Default["Address"] = textBoxAddress1.Text;
            Settings.Default["Zip"] = textBoxZip.Text;
            Settings.Default["First"] = textBoxFN.Text;
            Settings.Default["Last"] = textBoxLN.Text;
            Settings.Default["Telephone"] = textBoxTelephone.Text;
            Settings.Default["Email"] = textBoxEmail.Text;
            Settings.Default["City"] = textBoxCity.Text;
            Settings.Default["CardNum"] = textBoxCardNum.Text;
            Settings.Default["CVV"] = textBoxCCV.Text;
            Settings.Default["ExpMonth"] = comboBoxExpMonth.SelectedItem.ToString();
            Settings.Default["ExpYear"] = comboBoxyear.SelectedItem.ToString();
            Settings.Default["Payment"] = comboBoxPaymentType.SelectedItem.ToString();

            Settings.Default.Save();
            MessageBox.Show("Saved Successful");

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            textBoxAddress1.Text = Settings.Default["Address"].ToString();
            textBoxZip.Text = Settings.Default["Zip"].ToString();
            textBoxFN.Text = Settings.Default["First"].ToString();
            textBoxLN.Text = Settings.Default["Last"].ToString();
            textBoxTelephone.Text = Settings.Default["Telephone"].ToString();
            textBoxEmail.Text = Settings.Default["Email"].ToString();
            textBoxCity.Text = Settings.Default["City"].ToString();
            comboBoxExpMonth.SelectedIndex = comboBoxExpMonth.Items.IndexOf(Settings.Default["ExpMonth"].ToString());
            comboBoxyear.SelectedIndex = comboBoxyear.Items.IndexOf(Settings.Default["ExpYear"].ToString());
            comboBoxPaymentType.SelectedIndex = comboBoxPaymentType.Items.IndexOf(Settings.Default["Payment"].ToString());
            textBoxCCV.Text = Settings.Default["CVV"].ToString();
            textBoxCardNum.Text = Settings.Default["CardNum"].ToString();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();

            comboBoxExpMonth.SelectedIndex = 0;
            comboBoxyear.SelectedIndex = 0;
            comboBoxPaymentType.SelectedIndex = 0;

        }

        private void ClearTextBoxes()
        {
            Action<Control.ControlCollection> func = null;

            func = (controls) =>
            {
                foreach (Control control in controls)
                    if (control is TextBox)
                        (control as TextBox).Clear();
                    else
                        func(control.Controls);
            };

            func(Controls);
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {
   
            //need to check of any of the boxes are empty
            if (String.IsNullOrEmpty(textBoxFN.Text) || String.IsNullOrEmpty(textBoxLN.Text) || String.IsNullOrEmpty(textBoxEmail.Text) || String.IsNullOrEmpty(textBoxTelephone.Text) ||
                String.IsNullOrEmpty(textBoxAddress1.Text) || String.IsNullOrEmpty(textBoxCity.Text) || String.IsNullOrEmpty(textBoxZip.Text) || String.IsNullOrEmpty(textBoxCardNum.Text) ||
                String.IsNullOrEmpty(textBoxCCV.Text)
                )
            {
                MessageBox.Show("Please Fill Out All The Payment Information");

            }

            else
            {

                saveFileDialog1.Filter = "txt|*.txt";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    TextWriter tw = new StreamWriter(saveFileDialog1.FileName);

                    // write a line of text to the file
                    tw.WriteLine(textBoxFN.Text);
                    tw.WriteLine(textBoxLN.Text);
                    tw.WriteLine(textBoxEmail.Text);
                    tw.WriteLine(textBoxTelephone.Text);
                    tw.WriteLine(textBoxAddress1.Text);
                    tw.WriteLine(textBoxCity.Text);
                    tw.WriteLine(textBoxZip.Text);

                    //skipping State and Country since those will be default OR and USA
                    tw.WriteLine(comboBoxPaymentType.GetItemText(comboBoxPaymentType.SelectedItem));
                    tw.WriteLine(textBoxCardNum.Text);
                    tw.WriteLine(comboBoxExpMonth.GetItemText(comboBoxExpMonth.SelectedItem));
                    tw.WriteLine(comboBoxyear.GetItemText(comboBoxyear.SelectedItem));
                    tw.WriteLine(textBoxCCV.Text);

                    //tw.WriteLine(textBoxZip.Text);
                    // close the stream
                    tw.Close();
                    MessageBox.Show("Saved to " + saveFileDialog1.FileName, "Saved Log File", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
      
        }


        private void buttonLoad_Click(object sender, EventArgs e)
        {
            //openFileDialog1.FileName = "txt|*.txt";
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string filepath = openFileDialog1.FileName;
                string[] lines = System.IO.File.ReadAllLines(filepath);

                textBoxFN.Text = lines[0];
                textBoxLN.Text = lines[1];
                textBoxEmail.Text = lines[2];
                textBoxTelephone.Text = lines[3];
                textBoxAddress1.Text = lines[4];
                textBoxCity.Text = lines[5];
                textBoxZip.Text = lines[6];
                comboBoxPaymentType.SelectedIndex = comboBoxPaymentType.Items.IndexOf(lines[7]);
                textBoxCardNum.Text = lines[8];
                comboBoxExpMonth.SelectedIndex = comboBoxExpMonth.Items.IndexOf(lines[9]);
                comboBoxyear.SelectedIndex = comboBoxyear.Items.IndexOf(lines[10]);
                textBoxCCV.Text = lines[11];

            }


        }

        private void pictureBoxSmall_Click(object sender, EventArgs e)
        {
            size = "Small";
            pictureBoxSmall.Image = Properties.Resources.Logosmallclick;

            //set all other image to default
            pictureBoxMedium.Image = Properties.Resources.Logomedium;
            pictureBoxLarge.Image = Properties.Resources.Logolarge;
            pictureBoxXL.Image = Properties.Resources.Logoxl;

        }

        private void pictureBoxMedium_Click(object sender, EventArgs e)
        {
            size = "Medium";
            pictureBoxMedium.Image = Properties.Resources.Logomediumclick;

            //set all other image to default
            pictureBoxSmall.Image = Properties.Resources.Logosmall;
            pictureBoxLarge.Image = Properties.Resources.Logolarge;
            pictureBoxXL.Image = Properties.Resources.Logoxl;
            pictureBoxMISC.Image = Properties.Resources.LogoMisc;

        }

        private void pictureBoxLarge_Click(object sender, EventArgs e)
        {
            size = "Large";
            pictureBoxLarge.Image = Properties.Resources.Logolargeclick;

            //set all other image to default
            pictureBoxSmall.Image = Properties.Resources.Logosmall;
            pictureBoxMedium.Image = Properties.Resources.Logomedium;
            pictureBoxXL.Image = Properties.Resources.Logoxl;
            pictureBoxMISC.Image = Properties.Resources.LogoMisc;


        }

        private void pictureBoxXL_Click(object sender, EventArgs e)
        {
            size = "XLarge";
            pictureBoxXL.Image = Properties.Resources.logoxlclick;

            //set all other image to default
            pictureBoxSmall.Image = Properties.Resources.Logosmall;
            pictureBoxMedium.Image = Properties.Resources.Logomedium;
            pictureBoxLarge.Image = Properties.Resources.Logolarge;
            pictureBoxMISC.Image = Properties.Resources.LogoMisc;
            
        }
        private void pictureBoxMISC_Click(object sender, EventArgs e)
        {
            size = "Misc";

            pictureBoxMISC.Image = Properties.Resources.LogoMiscClick;

            pictureBoxSmall.Image = Properties.Resources.Logosmall;
            pictureBoxMedium.Image = Properties.Resources.Logomedium;
            pictureBoxLarge.Image = Properties.Resources.Logolarge;
            pictureBoxXL.Image = Properties.Resources.Logoxl;

        }

        private void fAQSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FAQ about = new FAQ();
            about.Show();
        }

        private void earlyLinksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.heatedsneaks.com/supreme-early-links.html");
        }

        private void contactUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.meatspin.com");

        }

        
 

   

       
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace IPManger
{
    public partial class main : Form
    {
        Dictionary<string, NetworkInterface> networkInterfaces = new Dictionary<string, NetworkInterface>();
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        RegistryKey hkml = Registry.LocalMachine;
        RegistryKey regInterface = null;

        public main()
        {
            InitializeComponent();
        }

        private void autoreadCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox autoread = sender as CheckBox;
            if (autoread.CheckState == CheckState.Checked)
            {
                readButton.Enabled = false;
            }
            else {
                readButton.Enabled = true;
            }
        }

        private void main_Load(object sender, EventArgs e)
        {
            getAdapterInfo();
            getREGInterface();
            networkListbox.SelectedIndex = 0;
        }

        private void getAdapterInfo() {
            //NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    || adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit)
                {
                    //MessageBox.Show(adapter.ToString());
                    networkInterfaces.Add(adapter.Name, adapter);
                    networkListbox.Items.Add(adapter.Name);
                    //showAdapterIPInfo(adapter);
                }
            }

        }

        //清除内容
        private void clearTextbox() {
            ipAddressTextbox.Text = "";
            submaskTextBox.Text = "";
            ipGatewayTextbox.Text = "";
            dns1TextBox.Text = "";
            dns2TextBox.Text = "";
            dns3TextBox.Text = "";
            dns4TextBox.Text = "";
        }

        //读取给定的网卡信息
        private void showAdapterIPInfo(NetworkInterface adapter) {
            clearTextbox();
            IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
            UnicastIPAddressInformationCollection ipCollection = adapterProperties.UnicastAddresses;
            GatewayIPAddressInformationCollection gatewayColletion = adapterProperties.GatewayAddresses;
            //MessageBox.Show(adapter.Id);
            foreach (var item in ipCollection) {
                if (item.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    if (item.Address.ToString().Substring(0, 7) == "169.254")
                    {
                        try
                        {
                            string[] temp = (string[])regInterface.OpenSubKey(adapter.Id, true).GetValue("IPAddress");
                            ipAddressTextbox.Text = temp[0];
                        }
                        catch
                        {
                            ipAddressTextbox.Text = "";
                        }
                    }
                    else { ipAddressTextbox.Text = item.Address.ToString(); }
                    
                    try
                    {
                        submaskTextBox.Text = item.IPv4Mask.ToString();
                        //submaskTextBox.Text = item.IPv4Mask.ToString();
                    }
                    catch {
                        try
                        {
                            string[] temp = (string[])regInterface.OpenSubKey(adapter.Id, true).GetValue("SubnetMask");
                            submaskTextBox.Text = temp[0];
                        }
                        catch
                        {
                            submaskTextBox.Text = "";
                        }
                        //MessageBox.Show("没有连接网络,本程序可能不能正常使用,请连接网络后重启启动本程序");
                        //Application.Exit();
                    }
                   
                }
            }

            foreach (var item in gatewayColletion)
            {
                ipGatewayTextbox.Text = item.Address.ToString();
            }

            IPAddressCollection dnss = adapterProperties.DnsAddresses;

            //TextBox[] dnsArr;
            List<TextBox> dnsList = new List<TextBox>(new TextBox[]{ dns1TextBox,dns2TextBox, dns3TextBox, dns4TextBox});
            //int i = 0;
            foreach (IPAddress dns in dnss) {
                foreach (TextBox d in dnsList) {
                    if (d.TextLength == 0) {
                        d.Text = dns.ToString();
                        break;
                    }
                }
            }
        }

        private void setAdapterIPInfo(NetworkInterface adapter)
        {
            
        }

        ////todo
        private void setAdapterIPInfo()
        {
            //Console.WriteLine("11111");
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapter");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                try
                {
                    MessageBox.Show(mo.GetPropertyValue("NetConnectionID").ToString() );
                    foreach (PropertyData a in mo.Properties)
                    {
                        Console.WriteLine("key={0},value={1}", a.Name, a.Value);
                        //MessageBox.Show(a.Value.ToString());
                    }
                }
                catch { }
                continue;
            }
        }

        private void networkListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox network = sender as ListBox;
            if (autoreadCheckbox.Checked){
                showAdapterIPInfo(networkInterfaces[network.SelectedItem.ToString()]);
            }
                
        }

        private void readButton_Click(object sender, EventArgs e)
        {
            showAdapterIPInfo(networkInterfaces[networkListbox.SelectedItem.ToString()]);
        }

        private bool checkIP(string e) {
            IPAddress ip;
            if (IPAddress.TryParse(e, out ip))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void writeButton_Click(object sender, EventArgs e)
        {
            //setAdapterIPInfo();
            //return;
            ManagementBaseObject inPar = null;
            ManagementBaseObject outPar = null;

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            string[] ip = new string[] { ipAddressTextbox.Text };
            string[] submask = new string[] { submaskTextBox.Text };
            string[] gateway = new string[] { ipGatewayTextbox.Text };
            if (!checkIP(ip[0])) {
                MessageBox.Show("IP格式错误");
                return;
            }
            if (!checkIP(gateway[0]))
            {
                MessageBox.Show("网关格式错误");
                return;
            }
            if (!checkMask(submask[0]))
            {
                MessageBox.Show("掩码格式错误");
                return;
            }

            List<string> dnss = new List<string>();
            if (checkIP(dns1TextBox.Text)) {
                dnss.Add(dns1TextBox.Text);
            } else {
                return;
            }

            if (checkIP(dns2TextBox.Text))
            {
                dnss.Add(dns2TextBox.Text);
            }
 
            if (checkIP(dns3TextBox.Text))
            {
                dnss.Add(dns3TextBox.Text);
            }

            if (checkIP(dns4TextBox.Text))
            {
                dnss.Add(dns4TextBox.Text);
            }


            string[] dnssArr = dnss.ToArray();


            foreach (ManagementObject mo in moc)
            {
                //MessageBox.Show((string)mo["SettingID"]);
                //Console.WriteLine((string)mo["SettingID"]);
                //break;
                string description = (string)mo["Description"];
                if (description != networkInterfaces[networkListbox.SelectedItem.ToString()].Description) continue;
                //if (!(bool)mo["IPEnabled"]) continue;

                /*Console.WriteLine("1111111111111111111111111111111");
                foreach (PropertyData a in mo.Properties)
                {
                    try
                    {
                        Console.WriteLine("key={0},value={1}",a.Name, a.Value);
                    }
                    catch { }
                }
                continue;*/

                inPar = mo.GetMethodParameters("EnableStatic");
                inPar["IPAddress"] = ip;//ip地址  
                inPar["SubnetMask"] = submask; //子网掩码   
                outPar = mo.InvokeMethod("EnableStatic", inPar, null);//执行  
                if (int.Parse(outPar.GetPropertyValue("ReturnValue").ToString()) > 1)
                {
                    //MessageBox.Show(dnsserver);
                    regInterface.OpenSubKey(networkInterfaces[networkListbox.SelectedItem.ToString()].Id, true).SetValue("IPAddress", ip);
                    regInterface.OpenSubKey(networkInterfaces[networkListbox.SelectedItem.ToString()].Id, true).SetValue("SubnetMask", submask);
                }

                inPar = mo.GetMethodParameters("SetGateways");
                inPar["DefaultIPGateway"] = gateway; //设置网关地址 1.网关;2.备用网关  
                outPar = mo.InvokeMethod("SetGateways", inPar, null);//执行 
                if (int.Parse(outPar.GetPropertyValue("ReturnValue").ToString()) > 1)
                {
                    //MessageBox.Show(dnsserver);
                    regInterface.OpenSubKey(networkInterfaces[networkListbox.SelectedItem.ToString()].Id, true).SetValue("DefaultGateway", gateway);
                }

                inPar = mo.GetMethodParameters("SetDNSServerSearchOrder");
                inPar["DNSServerSearchOrder"] = dnssArr; //设置DNS  1.DNS 2.备用DNS  
                ManagementBaseObject DNSresult = mo.InvokeMethod("SetDNSServerSearchOrder", inPar, null);// 执行
                if (int.Parse(DNSresult.GetPropertyValue("ReturnValue").ToString()) > 1) {
                    //MessageBox.Show(DNSresult.GetPropertyValue("ReturnValue").ToString());
                    string dnsserver = null;
                    foreach (string t in dnssArr) {
                        dnsserver = dnsserver + ',' + t;
                    }
                    dnsserver = dnsserver.TrimStart(',');
                    dnsserver = dnsserver.TrimEnd(',');
                    //MessageBox.Show(dnsserver);
                    regInterface.OpenSubKey(networkInterfaces[networkListbox.SelectedItem.ToString()].Id, true).SetValue("NameServer", dnsserver);
                }
                MessageBox.Show("已执行写入动作");
                Application.Exit();//重启读取配置
                System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                return; //只设置一张网卡，不能多张。 
            }
            MessageBox.Show("本程序不支持此台电脑");
        }

        private void getREGInterface() {
            regInterface = hkml.OpenSubKey("SYSTEM", true)
                .OpenSubKey("CurrentControlSet", true)
                .OpenSubKey("services", true)
                .OpenSubKey("Tcpip", true)
                .OpenSubKey("Parameters", true)
                .OpenSubKey("Interfaces", true);
        }

        public bool checkMask(string mask)
        {
            string[] vList = mask.Split('.');
            if (vList.Length != 4) return false;

            bool vZero = false; // 出现0 
            for (int j = 0; j < vList.Length; j++)
            {
                int i;
                if (!int.TryParse(vList[j], out i)) return false;
                if ((i < 0) || (i > 255)) return false;
                if (vZero)
                {
                    if (i != 0) return false;
                }
                else
                {
                    for (int k = 7; k >= 0; k--)
                    {
                        if (((i >> k) & 1) == 0) // 出现0 
                        {
                            vZero = true;
                        }
                        else
                        {
                            if (vZero) return false; // 不为0 
                        }
                    }
                }
            }

            return true;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

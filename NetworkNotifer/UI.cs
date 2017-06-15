using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;



namespace NetworkNotifer
{
    public partial class UI : Form
    {
        Controller contobj;
        DataTable dt;
        NotifyIcon notification;
        MacLookup vendorsearch;
        public UI()
        {
            InitializeComponent();

            vendorsearch = new MacLookup();

            notification = new NotifyIcon()
            {
                Visible = true,
                Icon = System.Drawing.SystemIcons.Information,
                BalloonTipTitle = Values.AppName,
                Text = Values.AppName
            };

            Text = Values.AppName;

            dt = new DataTable();
            //dt.Columns.Add(Values.Column_xid);
            dt.Columns.Add(Values.Column_hostname);
            dt.Columns.Add(Values.Column_IP);
            dt.Columns.Add(Values.Column_MAC);
            dt.Columns.Add(Values.Column_vendor);
            dt.Columns.Add(Values.Column_Status);
            dataGridView1.DataSource = dt;

            //dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.MinimumWidth = this.Width / 5;
            }

            contobj = new Controller(this);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void NotifyConnecting(object m)
        {
            DHCPMessage d = (DHCPMessage)m;
            AddDevice(d.IpAddr, d.MacAddr, d.HostName, d.TransactionID, false);
        }

        public void NotifyConnected(object m)
        {
            DHCPMessage d = (DHCPMessage)m;
            AddDevice(d.IpAddr, d.MacAddr, d.HostName, d.TransactionID, true);
            ShowMessage(d.HostName + " connected to your network.", 4);
        }

        public void NotifyAdded(object m)
        {
            IPMacPair ipmac = (IPMacPair)m;
            AddDevice(ipmac.IP, ipmac.MAC, null, null, true);
        }

        public void NotifyIPConnected(object m)
        {
            string mac = (string)m;
            var resrow = dt.AsEnumerable().First(dr => (string)dr[Values.Column_MAC] == mac);
            resrow[Values.Column_Status] = Values.status_connected;
            string name = string.IsNullOrEmpty(resrow[Values.Column_hostname] as string) ? resrow[Values.Column_IP] as string : resrow[Values.Column_hostname] as string;
            ShowMessage(name + " connected to your network.", 4);
        }

        public void NotifyIPDisonnected(object m)
        {
            string mac = (string)m;
            var resrow = dt.AsEnumerable().First(dr => (string)dr[Values.Column_MAC] == mac);
            resrow[Values.Column_Status] = Values.status_disconnected;
            string name = string.IsNullOrEmpty(resrow[Values.Column_hostname] as string) ? resrow[Values.Column_IP] as string : resrow[Values.Column_hostname] as string;
            ShowMessage(name + " disconnected from your network.", 4);
        }

        public void OnNetworkChanged()
        {
            MessageBox.Show("Network Connection changed");
            dt.Clear();
        }

        public void AddDevice(string ip, string mac, string host, string XID, bool stat)
        {
            DataRow dr = dt.AsEnumerable().FirstOrDefault(row => row[Values.Column_IP] != null && (string)row[Values.Column_IP] == ip);
            if (dr != null) dr.Delete();

            dr = dt.AsEnumerable().FirstOrDefault(row => (string)row[Values.Column_MAC] == mac);

            if (dr == null)
            {
                dr = dt.NewRow();
                //dr[Values.Column_xid] = XID;
                dr[Values.Column_vendor] = vendorsearch.GetVendorName(mac);
                dr[Values.Column_hostname] = host;
                dr[Values.Column_MAC] = mac;
                dr[Values.Column_Status] = stat ? Values.status_connected : Values.status_connecting;
                dr[Values.Column_IP] = ip;
                dt.Rows.Add(dr);
            }
            else
            {
                //dr[Values.Column_xid] = XID;
                dr[Values.Column_hostname] = host;
                dr[Values.Column_vendor] = vendorsearch.GetVendorName(mac);
                dr[Values.Column_Status] = stat ? Values.status_connected : Values.status_connecting;
                dr[Values.Column_MAC] = mac;
                dr[Values.Column_IP] = ip;
            }

        }

        public void RemoveDevice(object d)
        {
            string mac = (string)d;
            dt.Rows.Remove(dt.AsEnumerable().FirstOrDefault(row => (string)row[Values.Column_MAC] == mac));
        }

        private void ShowMessage(string message, int time)
        {
            notification.BalloonTipText = message;
            notification.ShowBalloonTip(time);
        }

        private void UI_FormClosed(object sender, FormClosedEventArgs e)
        {
            notification.Dispose();
        }
    }
}

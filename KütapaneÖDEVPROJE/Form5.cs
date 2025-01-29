using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace KütapaneÖDEVPROJE
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
            dataGridView1.CellMouseDoubleClick += dataGridView1_CellMouseDoubleClick;
            dataGridView2.CellFormatting += dataGridView2_CellFormatting;
        }

        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView2.Columns[e.ColumnIndex].Name == "Bitiş Tarihi") 
            {
                if (e.Value != null && DateTime.TryParse(e.Value.ToString(), out DateTime bitisTarihi))
                {
                    if (bitisTarihi >= DateTime.Now)
                    {
                        e.CellStyle.BackColor = Color.Green;
                        e.CellStyle.ForeColor = Color.White;
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.Red;
                        e.CellStyle.ForeColor = Color.White;
                    }
                }
            }
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            this.Text = "Kütüphane Uygulaması Versiyon " + Assembly.GetExecutingAssembly().GetName().Version;
            GetAllBooks();
            GetAllRezervasyon();
            dataGridView1.Columns["ID"].Visible = false;
            dataGridView2.Columns["ID"].Visible = false;
            Bilgiler();
        }
        private void Bilgiler()
        {
            SqlConnection con = new SqlConnection(Kütüphane.constr2);

            SqlCommand com = new SqlCommand("SELECT ID, Adı, Soyadı, Meslek, Şehir, DoğumTarihi FROM User1 WHERE ID = @ID", con);
            com.Parameters.AddWithValue("@ID", Kütüphane.UserID);


            con.Open();
            SqlDataReader dr = com.ExecuteReader();
            if (dr.Read())
            {
                label6.Text = dr["Adı"].ToString();
                label7.Text = dr["Soyadı"].ToString();
                label8.Text = dr["Meslek"].ToString();
                label9.Text = dr["Şehir"].ToString();
                label10.Text = dr["DoğumTarihi"].ToString();
            }
            else
            {
                MessageBox.Show("Kullanıcı bulunamadı.");
            }
            dr.Close();

        }

        private void GetAllRezervasyon()
        {
            dataGridView2.DataSource = Kütüphane.Getrezerv().Tables[0];
            dataGridView2.Refresh();
        }

        private void GetAllBooks()
        {
            dataGridView1.DataSource = Kütüphane.GetBooks().Tables[0];
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                string kitapAdi = row.Cells["Kitap Adı"].Value.ToString();
                string barkodNo = row.Cells["Barkod Numarası"].Value.ToString();
                string yazar = row.Cells["Yazar"].Value.ToString();
                string kütüphane = row.Cells["Kütüphane"].Value.ToString();

                Form6 yeni = new Form6(kitapAdi, barkodNo, yazar, kütüphane);
                yeni.Form9Submitted += Form9SubmittedHandler;
                yeni.Show();
            }
        }

        private void Form9SubmittedHandler(object sender, Form9SubmittedEventArgs e)
        {
            GetAllRezervasyon();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Giriş sayfasına yönlendirileceksiniz \nDevam etmek istiyor musunuz?", "UYARI", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Form1 yeni = new Form1();
                yeni.Show();
                this.Hide();
            }
        }

        private void dataGridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rowIndex = dataGridView2.HitTest(e.X, e.Y).RowIndex;

                if (rowIndex >= 0)
                {
                    dataGridView2.ClearSelection();
                    dataGridView2.Rows[rowIndex].Selected = true;

                    DataGridViewRow row = dataGridView2.Rows[rowIndex];
                    DateTime bitisTarihi;
                    bool isContinuingReservation = DateTime.TryParse(row.Cells["Bitiş Tarihi"].Value.ToString(), out bitisTarihi) && bitisTarihi >= DateTime.Now;

                    ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
                    if (isContinuingReservation)
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem("Rezervasyonu İptal Et");
                        menuItem.Click += DeleteRezerv1;
                        contextMenuStrip.Items.Add(menuItem);
                    }
                    else
                    {
                        ToolStripMenuItem menuItem = new ToolStripMenuItem("Rezervasyon süreniz dolmuştur");
                        menuItem.Click += (s, args) => MessageBox.Show("Rezervasyon süreniz dolmuştur.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        contextMenuStrip.Items.Add(menuItem);
                    }

                    contextMenuStrip.Show(dataGridView2, e.Location);
                }
            }
        }

        private void DeleteRezerv1(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                int rezervasyonID = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells["ID"].Value);

                DialogResult result = MessageBox.Show("Rezervasyonu iptal etmek istediğinize emin misiniz?", "Rezervasyon İptali", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Kütüphane.DeleteRezerv(rezervasyonID);

                    GetAllRezervasyon();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {

            Kütüphane.serach = !Kütüphane.serach;
            if (Kütüphane.serach)
            {
                textBox1.ResetText();
            }
            groupBox1.Visible = Kütüphane.serach;

            GetAllBooks();

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = string.Empty;
            }
            else
            {
                string rowFilter = string.Format("[Kitap Adı] LIKE '%{0}%' OR [Barkod Numarası] LIKE '%{0}%' OR [Yazar] LIKE '%{0}%' OR [Kütüphane] LIKE '%{0}%'", filterText);
                (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = rowFilter;
            }
        }
    }
}


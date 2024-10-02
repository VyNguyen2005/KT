using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;

namespace KT1_HienVy
{
    public partial class Form1 : Form
    {
        string connection = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=..\..\..\DATA\QLSV.mdb";
        DataSet ds = new DataSet();
        OleDbDataAdapter adpSinhVien, adpKhoa, adpKetQua, adpMonHoc;
        OleDbCommandBuilder cmbMonHoc;
        BindingSource bs = new BindingSource();
        int stt;
        public Form1()
        {
            InitializeComponent();
            bs.CurrentChanged += Bs_CurrentChanged;
        }

        private void Bs_CurrentChanged(object sender, EventArgs e)
        {
            lblSTT.Text = bs.Position + 1 + "/" + bs.Count;
            if(bs.Current != null)
            {
                if (bs.Current is DataRowView rowView)
                {
                    string maMH = rowView["MaMH"].ToString(); 
                    txtMaMH.Text = maMH; 

                    txtTSSV.Text = TinhTSSV(maMH).ToString();
                    txtDiemMax.Text = TinhDiemMax(maMH).ToString();
                }
            }
            
            
        }

        private object TinhDiemMax(string mamh)
        {
            double kq;
            Object diemmax = ds.Tables["KETQUA"].Compute("max(Diem)", "MaMH='" + mamh + "'");
            if (diemmax == DBNull.Value)
                kq = 0;
            else
                kq = Convert.ToDouble(diemmax);
            return kq;
        }

        private object TinhTSSV(string mamh)
        {
            int kq;
            Object tssv = ds.Tables["KETQUA"].Compute("count(MaSV)", "MaMH='" + mamh + "'");
            if (tssv == DBNull.Value)
                kq = 0;
            else
                kq = Convert.ToInt32(tssv);
            return kq;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Khoi_Tao_Doi_Tuong();
            Doc_Du_Lieu();
            Moc_Noi_Quan_He_Cac_Bang();
            Khoi_Tao_BindingSource();
            Lien_Ket_Dieu_Khien();
            bdnMonHoc.BindingSource = bs;
        }

        private void Lien_Ket_Dieu_Khien()
        {
            foreach (Control control in this.Controls)
            {
                if(control is TextBox && control.Name != "txtLoaiMH" && control.Name != "txtTSSV" && control.Name != "txtDiemMax")
                {
                    control.DataBindings.Add("text", bs, control.Name.Substring(3), true);
                }
            }
            Binding bdLoaiMH = new Binding("text", bs, "LoaiMH", true);
            bdLoaiMH.Format += BdLoaiMH_Format;
            bdLoaiMH.Parse += BdLoaiMH_Parse;
            txtLoaiMH.DataBindings.Add(bdLoaiMH);
        }

        private void BdLoaiMH_Parse(object sender, ConvertEventArgs e)
        {
            if (e.Value == null) return;
            e.Value = e.Value.ToString().ToUpper() == "BẮT BUỘC" ? true : false;
        }

        private void BdLoaiMH_Format(object sender, ConvertEventArgs e)
        {
            if (e.Value == DBNull.Value || e.Value == null) return;
            e.Value = (Boolean)e.Value ? "Bắt buộc" : "Tùy chọn";
        }

        private void Khoi_Tao_BindingSource()
        {
            bs.DataSource = ds;
            bs.DataMember = "MONHOC";
        }

        private void btnTruoc_Click(object sender, EventArgs e)
        {
            bs.MovePrevious();
        }

        private void btnDau_Click(object sender, EventArgs e)
        {
            bs.MoveFirst();
        }

        private void btnSau_Click(object sender, EventArgs e)
        {
            bs.MoveNext();
        }

        private void btnCuoi_Click(object sender, EventArgs e)
        {
            bs.MoveLast();
        }

        private void btnKhong_Click(object sender, EventArgs e)
        {
            bs.CancelEdit();
            txtMaMH.ReadOnly = true;
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult tl = MessageBox.Show("Bạn có muốn thoát chương trình này?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(tl == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnHuy_Click(object sender, EventArgs e)
        {
            DataRow dataRow = (bs.Current as DataRowView).Row;
            DataRow[] mang_dong = dataRow.GetChildRows("FK_MH_KQ");
            if(mang_dong.Length > 0)
            {
                MessageBox.Show("Không thể hủy môn học này vì đã có trong kết quả thi", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error); ;
            }
            else
            {
                DialogResult tl;
                tl = MessageBox.Show("Bạn có chắc muốn hủy môn học này?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if(tl == DialogResult.Yes)
                {
                    bs.RemoveCurrent();
                    int n = adpMonHoc.Update(ds, "MONHOC");
                    if(n > 0)
                    {
                        MessageBox.Show("Hủy môn học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.None);
                    }
                }
            }
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            txtMaMH.ReadOnly = false;
            stt = bs.Position;
            bs.AddNew();
            txtMaMH.Focus();
        }

        private void btnGhi_Click(object sender, EventArgs e)
        {
            if (txtMaMH.ReadOnly == false)
            {
                DataRow dataRow = ds.Tables["MONHOC"].Rows.Find(txtMaMH.Text);
                if (dataRow != null)
                {
                    MessageBox.Show("Mã môn học bị trùng!", "Thông báo lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtMaMH.Focus();
                    return;
                }
            }
            bs.EndEdit();
            int n = adpMonHoc.Update(ds, "MONHOC");
            if (n > 0)
            {
                MessageBox.Show("Cập nhật (THÊM/SỬA) thành công");
            }
            txtMaMH.ReadOnly = true;
        }

        private void Moc_Noi_Quan_He_Cac_Bang()
        {
            ds.Relations.Add("KH_SV", ds.Tables["KHOA"].Columns["MaKH"], ds.Tables["SINHVIEN"].Columns["MaKH"], true);
            ds.Relations.Add("SV_KQ", ds.Tables["SINHVIEN"].Columns["MaSV"], ds.Tables["KETQUA"].Columns["MaSV"], true);
            ds.Relations.Add("MH_KQ", ds.Tables["MONHOC"].Columns["MaMH"], ds.Tables["KETQUA"].Columns["MaMH"], true);
            ds.Relations["KH_SV"].ChildKeyConstraint.DeleteRule = Rule.None;
            ds.Relations["SV_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
            ds.Relations["MH_KQ"].ChildKeyConstraint.DeleteRule = Rule.None;
        }

        private void Doc_Du_Lieu()
        {
            adpKhoa.FillSchema(ds, SchemaType.Source, "KHOA");
            adpKhoa.Fill(ds, "KHOA");

            adpSinhVien.FillSchema(ds, SchemaType.Source, "SINHVIEN");
            adpSinhVien.Fill(ds, "SINHVIEN");

            adpKetQua.FillSchema(ds, SchemaType.Source, "KETQUA");
            adpKetQua.Fill(ds, "KETQUA");

            adpMonHoc.FillSchema(ds, SchemaType.Source, "MONHOC");
            adpMonHoc.Fill(ds, "MONHOC");
        }

        private void Khoi_Tao_Doi_Tuong()
        {
            adpSinhVien = new OleDbDataAdapter("select * from SINHVIEN", connection);
            adpKhoa = new OleDbDataAdapter("select * from KHOA", connection);
            adpKetQua = new OleDbDataAdapter("select * from KETQUA", connection);
            adpMonHoc = new OleDbDataAdapter("select * from MONHOC", connection);

            cmbMonHoc = new OleDbCommandBuilder(adpMonHoc);
        }
    }
}

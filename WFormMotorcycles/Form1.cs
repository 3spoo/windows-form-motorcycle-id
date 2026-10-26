using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFormMotorcycles
{
    public partial class Form1 : Form
    {
        List<Index> INDEXES;

        const string FILE_DATA = "data.dat";
        const string FILE_INDEX = "index.dat";

        const int DIM_BRAND_MODEL = 20;
        const int DIM_RECORD = 4 + 20 * 4 + 4 + 16 + 1;
        const int DIM_INDEX = 4 + 8;

        public class MotorCycle
        {
            private int _code;

            public int Code
            {
                get { return _code; }
            }

            private string _brand;

            public string Brand
            {
                get { return _brand; }
                set { _brand = value; }
            }

            private string _model;

            public string Model
            {
                get { return _model; }
                set { _model = value; }
            }

            private int _engine_capacity;

            public int EngineCapacity
            {
                get { return _engine_capacity; }
                set { _engine_capacity = value; }
            }

            private decimal _price;

            public decimal Price
            {
                get { return _price; }
                set { _price = value; }
            }

            private bool _is_active;

            public bool IsActive
            {
                get { return _is_active; }
                set { _is_active = value; }
            }

            public MotorCycle(int c, string b, string m, int ec, decimal p)
            {
                _code = c;
                _brand = b;
                _model = m;
                _engine_capacity = ec;
                _price = p;
                _is_active = true;
            }

            public MotorCycle(int c, string b, string m, int ec, decimal p, bool ia)
            {
                _code = c;
                _brand = b;
                _model = m;
                _engine_capacity = ec;
                _price = p;
                _is_active = ia;
            }
        }

        public class Index
        {
            private int _code;

            public int Code
            {
                get { return _code; }
            }

            private long _position;

            public long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            public Index(int c, long p)
            {
                _code = c;
                _position = p;
            }
        }

        public Form1()
        {
            InitializeComponent();
            CreateEnvironment();
        }

        public void CreateEnvironment()
        {
            using (FileStream fs = new FileStream(FILE_INDEX, FileMode.OpenOrCreate))
            {
                INDEXES = new List<Index>();

                if (fs.Length % DIM_INDEX != 0)
                {
                    MessageBox.Show("FILE IS CORRUPTED", "ERROR");
                    return;
                }

                if (fs.Length > 0)
                {
                    for (int i = 0; i * DIM_INDEX < fs.Length; i++)
                    {
                        fs.Seek(i * DIM_INDEX, SeekOrigin.Begin);
                        Index tmp = ReadIndex(fs, i);

                        if (tmp != null)
                            INDEXES.Add(tmp);
                    }

                    INDEXES.Sort((a, b)
                        => a.Code.CompareTo(b.Code));
                }
            }

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.OpenOrCreate))
            {
                if (fs.Length % DIM_RECORD != 0)
                {
                    MessageBox.Show("FILE IS CORRUPTED", "ERROR");
                    return;
                }

                foreach (Index i_tmp in INDEXES)
                {
                    MotorCycle mc_tmp = ReadMotorCycle(fs, i_tmp.Position);

                    if (mc_tmp != null && mc_tmp.IsActive)
                        dataGridView1.Rows.Add(mc_tmp.Code, mc_tmp.Brand, mc_tmp.Model, mc_tmp.EngineCapacity, mc_tmp.Price + " $");
                }
            }
        }

        public Index ReadIndex(FileStream fs, long position)
        {
            fs.Seek(position * DIM_INDEX, SeekOrigin.Begin);

            using (BinaryReader br = new BinaryReader(fs, Encoding.Unicode, true))
            {
                try
                {
                    return new Index(br.ReadInt32(), br.ReadInt64());
                }
                catch
                {
                    return null;
                }
            }
        }

        public void WriteAllIndexes(FileStream fs, List<Index> indexes)
        {
            int i = 0;
            foreach (Index i_tmp in indexes)
            {
                fs.Seek(i * DIM_INDEX, SeekOrigin.Begin);
                WriteIndex(fs, i_tmp, i);

                i++;
            }
        }

        public void WriteIndex(FileStream fs, Index index, long position)
        {
            fs.Seek(position * DIM_INDEX, SeekOrigin.Begin);

            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.Unicode, true))
            {
                bw.Write(index.Code);
                bw.Write(index.Position);
            }
        }

        public MotorCycle ReadMotorCycle(FileStream fs, long position)
        {
            fs.Seek(position * DIM_RECORD, SeekOrigin.Begin);

            using (BinaryReader br = new BinaryReader(fs, Encoding.Unicode, true))
            {
                try
                {
                    return new MotorCycle(br.ReadInt32(), ReadString(br, DIM_BRAND_MODEL), ReadString(br, DIM_BRAND_MODEL), br.ReadInt32(), br.ReadDecimal(), br.ReadBoolean());
                }
                catch
                {
                    return null;
                }
            }
        }

        public void WriteMotorCycle(FileStream fs, MotorCycle motorCycle, long position)
        {
            fs.Seek(position * DIM_RECORD, SeekOrigin.Begin);

            using (BinaryWriter bw = new BinaryWriter(fs, Encoding.Unicode, true))
            {
                bw.Write(motorCycle.Code);
                WriteString(bw, motorCycle.Brand, DIM_BRAND_MODEL);
                WriteString(bw, motorCycle.Model, DIM_BRAND_MODEL);
                bw.Write(motorCycle.EngineCapacity);
                bw.Write(motorCycle.Price);
                bw.Write(motorCycle.IsActive);
            }
        }

        public string ReadString(BinaryReader br, int length)
        {
            byte[] buffer = br.ReadBytes(length * 2);
            return Encoding.Unicode.GetString(buffer).Trim('\0');
        }

        public void WriteString(BinaryWriter bw, string value, int length)
        {
            value = value.PadRight(length).Substring(0, length);
            bw.Write(Encoding.Unicode.GetBytes(value));
        }

        public int BinarySearchIndex(List<Index> indexes, int code)
        {
            int left = 0;
            int right = indexes.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;

                if (indexes[mid].Code == code)
                    return mid;

                if (indexes[mid].Code < code)
                    left = mid + 1;
                else
                    right = mid - 1;
            }

            return -1;
        }

        private void bAdd_Click(object sender, EventArgs e)
        {
            short code = Convert.ToInt16(nudCode.Value);

            if (BinarySearchIndex(INDEXES, code) != -1)
            {
                MessageBox.Show("THIS MOTORCYCLE ALREADY EXISTS", "ERROR");
                return;
            }

            string brand = txtBrand.Text;
            string model = txtModel.Text;

            if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(model))
            {
                MessageBox.Show("BRAND AND MODEL MUST BE AT LEAST ONE CHAR", "ERROR");
                return;
            }

            short engine_capacity = Convert.ToInt16(nudEngine.Value);
            decimal price = nudPrice.Value;

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                long position = fs.Length / DIM_RECORD;

                MotorCycle mc_new = new MotorCycle(code, brand, model, engine_capacity, price);
                WriteMotorCycle(fs, mc_new, position);

                Index i_new = new Index(code, position);
                INDEXES.Add(i_new);

                INDEXES.Sort((a, b)
                        => a.Code.CompareTo(b.Code));

                // UPDATE DATA_GRID
                dataGridView1.Rows.Add(mc_new.Code, mc_new.Brand, mc_new.Model, mc_new.EngineCapacity, mc_new.Price + " $");
            }

            using (FileStream fs = new FileStream(FILE_INDEX, FileMode.Open))
            {
                WriteAllIndexes(fs, INDEXES);
            }

            // RESET
            nudCode.Value = 0;
            txtBrand.Text = "";
            txtModel.Text = "";
            nudEngine.Value = 50;
            nudPrice.Value = 500;
        }

        private void bFind_Click(object sender, EventArgs e)
        {
            int code = (int)nudCode.Value;

            int index = BinarySearchIndex(INDEXES, code);

            if (index == -1)
            {
                MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                return;
            }

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                Index i_tmp = INDEXES[index];
                MotorCycle mc_target = ReadMotorCycle(fs, i_tmp.Position);

                if (mc_target == null || !mc_target.IsActive)
                {
                    MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                    return;
                }

                nudCode.Value = mc_target.Code;
                txtBrand.Text = mc_target.Brand;
                txtModel.Text = mc_target.Model;
                nudEngine.Value = mc_target.EngineCapacity;
                nudPrice.Value = mc_target.Price;
            }
        }

        private void bEdit_Click(object sender, EventArgs e)
        {
            int code = (int)nudCode.Value;

            int index = BinarySearchIndex(INDEXES, code);

            if (index == -1)
            {
                MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                return;
            }

            string brand = txtBrand.Text;
            string model = txtModel.Text;

            if (string.IsNullOrEmpty(brand) || string.IsNullOrEmpty(model))
            {
                MessageBox.Show("BRAND AND MODEL MUST BE AT LEAST ONE CHAR", "ERROR");
                return;
            }

            int engine_capacity = (int)nudEngine.Value;
            decimal price = nudPrice.Value;
            Index i_tmp = INDEXES[index];

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                MotorCycle mc_target = ReadMotorCycle(fs, i_tmp.Position);

                if (mc_target == null || !mc_target.IsActive)
                {
                    MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                    return;
                }

                mc_target.Brand = brand;
                mc_target.Model = model;
                mc_target.EngineCapacity = engine_capacity;
                mc_target.Price = price;

                WriteMotorCycle(fs, mc_target, i_tmp.Position);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                if (Convert.ToInt32(row.Cells["code"].Value) == code)
                {
                    row.Cells["brand"].Value = brand;
                    row.Cells["model"].Value = model;
                    row.Cells["engine_capacity"].Value = engine_capacity;
                    row.Cells["price"].Value = price + " $";
                    break;
                }
            }

            nudCode.Value = 0;
            txtBrand.Text = "";
            txtModel.Text = "";
            nudEngine.Value = 50;
            nudPrice.Value = 500;
        }

        private void bDelete_Click(object sender, EventArgs e)
        {
            int code = (int)nudCode.Value;

            int index = BinarySearchIndex(INDEXES, code);

            if (index == -1)
            {
                MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                return;
            }

            Index i_tmp = INDEXES[index];

            using (FileStream fs = new FileStream(FILE_DATA, FileMode.Open))
            {
                MotorCycle mc_target = ReadMotorCycle(fs, i_tmp.Position);

                if (mc_target == null || !mc_target.IsActive)
                {
                    MessageBox.Show("MOTORCYCLE NOT FOUND", "ERROR");
                    return;
                }

                mc_target.IsActive = false;
                WriteMotorCycle(fs, mc_target, i_tmp.Position);
            }

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                if (Convert.ToInt32(row.Cells["code"].Value) == code)
                {
                    dataGridView1.Rows.Remove(row);
                    break;
                }
            }

            nudCode.Value = 0;
            txtBrand.Text = "";
            txtModel.Text = "";
            nudEngine.Value = 50;
            nudPrice.Value = 500;
        }
    }
}

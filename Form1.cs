using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace colored {
	public partial class Form1 : Form {
		private List<Label> labels = null;
		private int cindex = -1;
		private bool needSave = false;
		private string filename;
		private int last_x;
		private int last_y;

		public Form1()
		{
			InitializeComponent();
		}

		private void openToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			openFileDialog.ShowDialog();
		}

		private Color getColorAt(int i, long fileLength, byte[] arr)
		{
			byte r, g, b;

			try {
				r = arr[i * 4];
			} catch (Exception) {
				r = 0;
			}

			try {
				g = arr[i * 4 + 1];
			} catch (Exception) {
				g = 0;
			}

			try {
				b = arr[i * 4 + 2];
			} catch (Exception) {
				b = 0;
			}

			return Color.FromArgb(r, g, b);
		}

		private void createLabel(int i, byte[] data, int x, int y, long flength)
		{
			labels.Add(new Label());
			labels[i].SetBounds(x, y, 40, 40);
			labels[i].BackColor = getColorAt(i, flength, data);
			labels[i].Click += new EventHandler(labelsOnClick);
			labels[i].Name = i + "";
			this.panel1.Controls.Add(labels[i]);
		}

		private void openFile(object sender, CancelEventArgs e)
		{
			FileStream file = null;

			try {
				file = File.OpenRead(openFileDialog.FileName);

				byte[] data = new byte[file.Length];

				if (file.Read(data, 0, (int)file.Length) != file.Length) {
					throw new Exception("short read");
				}

				int n = (int)((file.Length / 4) + ((file.Length % 4) == 0 ? 0 : 1));

				labels = new List<Label>(n);

				for (int i = 0, y = 0, x = 0; i < n; ) {
					try {
						createLabel(i, data, x, y, file.Length);
						++i;

						if ((i % 4) == 0) {
							y += 50;
							x = 0;
						} else {
							x += 50;
						}

					} catch (Exception xx) {
						MessageBox.Show(xx.ToString());
						break;
					}
				}

				filename = file.Name;
			} catch (IOException x) {
				MessageBox.Show(x.ToString());
			} catch (Exception x) {
				MessageBox.Show(x.ToString());
				if (file != null) {
					file.Dispose();
				}
			}
		}

		private void labelsOnClick(object sender, EventArgs e)
		{
			if (sender is Label) {
				Label sn = (Label)sender;
				try {
					if (Convert.ToInt32(sn.Name) == cindex)
						return;

					if (cindex != -1) {
						labels[cindex].BorderStyle = BorderStyle.None;
					}

					cindex = Convert.ToInt32(sn.Name);
					sn.BorderStyle = BorderStyle.Fixed3D;
					if (sn.Focused) {
						sn.Focus();
					}

					this.button1.Enabled = true;
				} catch (Exception ex) {
					MessageBox.Show(ex.ToString());
				}
			}
		}

		private void changeColor(object sender, EventArgs e)
		{
			if (filename != null && cindex != -1) {
				if (colorDialog.ShowDialog() == DialogResult.OK) {
					labels[cindex].BackColor = colorDialog.Color;
					needSave = true;
				}
			}
		}

		private void save__(string path)
		{
			byte[] data = new byte[labels.Count() * 4];

			for (int i = 0; i < labels.Count(); ++i) {
				data[i * 4 + 0] = labels[i].BackColor.R;
				data[i * 4 + 1] = labels[i].BackColor.G;
				data[i * 4 + 2] = labels[i].BackColor.B;
				data[i * 4 + 3] = labels[i].BackColor.A;
			}

			try {
				FileStream f = File.OpenWrite(path);
				f.Write(data, 0, data.Length);
				f.Dispose();
			} catch (Exception x) {
				MessageBox.Show(x.ToString());
			}
		}

		private void save(object sender, EventArgs e)
		{
			if (needSave && filename != null) {
				save__(filename);
			}
		}

		private void saveAs(object sender, CancelEventArgs e)
		{
			if ((saveFileDialog.FileName == filename && !needSave) || filename == null)
				return;
			save__(saveFileDialog.FileName);
			filename = saveFileDialog.FileName;
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveFileDialog.ShowDialog();
		}
		
		private void close__()
		{
			for (int i = 0; i < labels.Count(); ++i) {
				labels[i].Dispose();
			}
			filename = null;
			labels.Clear();
			labels = null;
			cindex = -1;
			needSave = button2.Enabled = button3.Enabled = button1.Enabled = false;
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace colored {
	public partial class Form1 : Form {
		private List<Label> labels = null;
		private int cindex = -1;
		private bool needSave = false;
		private string filename;
		private int nextX;
		private int nextY;

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
				b = arr[i * 4];
			} catch (Exception) {
				b = 0;
			}

			try {
				g = arr[i * 4 + 1];
			} catch (Exception) {
				g = 0;
			}

			try {
				r = arr[i * 4 + 2];
			} catch (Exception) {
				r = 0;
			}

			return Color.FromArgb(r, g, b);
		}

		private void createLabel(int i, byte[] data, int x, int y, long flength)
		{
			labels.Add(new Label());
			labels[i].SetBounds(x, y, 40, 40);
			labels[i].BackColor = getColorAt(i, flength, data);
			labels[i].Click += new EventHandler(labelsOnClick);
			labels[i].DoubleClick += new EventHandler(labelsDoubleClick);
			labels[i].Name = i + "";
			this.panel1.Controls.Add(labels[i]);
		}

		private void openFile(object sender, CancelEventArgs e)
		{
			FileStream file = null;
			close();

			try {
				file = File.OpenRead(openFileDialog.FileName);

				byte[] data = new byte[file.Length];

				if (file.Read(data, 0, (int)file.Length) != file.Length) {
					throw new Exception("short read");
				}

				int n = (int)((file.Length / 4) + ((file.Length % 4) == 0 ? 0 : 1));

				labels = new List<Label>(n);

				int i;
				i = nextX = nextY = 0;
				while (i < n) {
					try {
						createLabel(i, data, nextX, nextY, file.Length);
						++i;

						if ((i % 4) == 0) {
							nextY += 50;
							nextX = 0;
						} else {
							nextX += 50;
						}

					} catch (Exception xx) {
						MessageBox.Show(xx.Source + ":\n" + xx.Message);
						break;
					}
				}

				filename = file.Name;
			} catch (IOException x) {
				MessageBox.Show(x.Source + ":\n" + x.Message);
			} catch (Exception x) {
				MessageBox.Show(x.Source + ":\n" + x.Message);
			} finally {
				if (file != null)
					file.Dispose();
			}
		}

		private void labelsOnClick(object sender, EventArgs e)
		{
			if (sender is Label) {
				Label sn = (Label)sender;
				try {
					if (Convert.ToInt32(sn.Name) == cindex)
						return;

					if (cindex > -1 && cindex < labels.Count) {
						labels[cindex].BorderStyle = BorderStyle.None;
					} else {
						button3.Enabled = button1.Enabled = true;
					}

					cindex = Convert.ToInt32(sn.Name);
					textBox1.Text = labels[cindex].BackColor.A.ToString();
					label1.Enabled = textBox1.Enabled = true;
					sn.BorderStyle = BorderStyle.Fixed3D;
					if (sn.Focused) {
						this.panel1.Focus();
					}
				} catch (Exception ex) {
					MessageBox.Show(ex.Source + ":\n" + ex.Message);
				}
			}
		}

		private void changeColor(object sender, EventArgs e)
		{
			if (cindex > -1 && cindex < labels.Count) {
				colorDialog.Color = labels[cindex].BackColor;
				if (colorDialog.ShowDialog() == DialogResult.OK) {
					int r = colorDialog.Color.R, g = colorDialog.Color.G, b = colorDialog.Color.B, a = labels[cindex].BackColor.A;
					labels[cindex].BackColor = Color.FromArgb(a, r, g, b);
					needSave = true;
				}
			}
		}

		private void write(string path)
		{
			byte[] data = new byte[labels.Count * 4];
			FileStream file = null;

			for (int i = 0; i < labels.Count; ++i) {
				data[i * 4 + 0] = labels[i].BackColor.B;
				data[i * 4 + 1] = labels[i].BackColor.G;
				data[i * 4 + 2] = labels[i].BackColor.R;
				data[i * 4 + 3] = labels[i].BackColor.A;
			}

			try {
				file = File.OpenWrite(path);
				file.Write(data, 0, data.Length);
				needSave = false;
			} catch (Exception x) {
				MessageBox.Show(x.Source + ":\n" + x.Message);
			} finally {
				if (file != null)
					file.Dispose();
			}
		}

		private void save(object sender, EventArgs e)
		{
			if (needSave) {
				if (filename == null) {
					saveFileDialog.ShowDialog();
				} else {
					write(filename);
				}
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			saveFileDialog.ShowDialog();
		}

		private void close()
		{
			if (labels != null) {
				if (needSave) {
					if (MessageBox.Show(this, "Save changes?", "Save?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
						if (filename == null) {
							saveFileDialog.ShowDialog();
						} else {
							write(filename);
						}
					}
				}
				for (int i = 0; i < labels.Count; ++i) {
					labels[i].Dispose();
				}
				filename = null;
				labels.Clear();
				cindex = -1;
				needSave = button3.Enabled = button1.Enabled = false;
				nextX = nextY = 0;
			}

		}

		private void labelsDoubleClick(object Sender, EventArgs e)
		{
			changeColor(null, null);
		}

		private void addBox(object sender, EventArgs e)
		{
			if (labels == null)
				labels = new List<Label>(16);

			int n = labels.Count;

			if (colorDialog.ShowDialog() != DialogResult.OK)
				return;

			labels.Add(new Label());
			labels[n].SetBounds(nextX, nextY, 40, 40);
			labels[n].Click += new EventHandler(labelsOnClick);
			labels[n].DoubleClick += new EventHandler(labelsDoubleClick);
			labels[n].Name = n + "";
			this.panel1.Controls.Add(labels[n]);

			if ((labels.Count % 4) == 0) {
				nextY += 50;
				nextX = 0;
			} else {
				nextX += 50;
			}

			needSave = true;

			if (cindex > -1) {
				for (int i = labels.Count - 1; i > cindex + 1; --i) {
					labels[i].BackColor = labels[i - 1].BackColor;
				}
				labels[cindex + 1].BackColor = colorDialog.Color;
			} else {
				labels[n].BackColor = colorDialog.Color;
			}
		}

		private void delBox(object sender, EventArgs e)
		{
			if (cindex > -1) {
				for (int i = cindex; i + 1 < labels.Count; ++i) {
					labels[i].BackColor = labels[i + 1].BackColor;
				}

				labels[labels.Count - 1].Dispose();
				labels.RemoveAt(labels.Count - 1);

				if (cindex >= labels.Count) {
					if (cindex - 1 != -1)
						labelsOnClick(labels[cindex - 1], null);
				}

				if (nextX == 0) {
					nextX = 150;
					nextY -= 50;
				} else {
					nextX -= 50;
				}
			}

			if (labels.Count == 0) {
				cindex = -1;
				label1.Enabled = textBox1.Enabled = button3.Enabled = button1.Enabled = false;
				textBox1.Clear();
			}

			needSave = true;
		}

		private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
		{
			write(saveFileDialog.FileName);
			filename = saveFileDialog.FileName;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			close();
		}

		private void closeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			close();
		}

		private void textBox1_KeyDown(object sender, KeyEventArgs e)
		{
		}

		private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!(sender is TextBox)) {
				return;
			}

			TextBox tb = (TextBox)sender;
			if (13 == (int)e.KeyChar) {
				uint a = UInt32.Parse(tb.Text);
				if (a > 255) {
					e.Handled = false;
				} else {
					if (cindex > -1 && cindex < labels.Count) {
						labels[cindex].BackColor = Color.FromArgb((int)a, labels[cindex].BackColor.R, labels[cindex].BackColor.G, labels[cindex].BackColor.B);
						needSave = true;
					}
					e.Handled = true;
				}
			} else if (Char.IsDigit(e.KeyChar) || Char.IsControl(e.KeyChar)) {
				e.Handled = false;
			} else {
				e.Handled = true;
			}
		}
	}
}

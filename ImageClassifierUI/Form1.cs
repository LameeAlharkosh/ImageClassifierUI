using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ImageClassifierUI
{
    // 1. كلاس الفورم (Form1) يجب أن يكون هو الأول دائماً لكي يعمل الـ Designer
    public partial class Form1 : Form
    {
        private PredictionEngine<ImageData, ImagePrediction>? _engine;
        private Button btnSelect = new();
        private PictureBox pictureBox = new();
        private Label lblResult = new();
        private Label lblConfidence = new();

        public Form1()
        {
            InitializeComponent();
            BuildUI();
            LoadModel();
        }

        void BuildUI()
        {
            this.Text = "Image Classifier - Cats vs Dogs";
            this.Size = new Size(500, 620);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label();
            lblTitle.Text = "Cat vs Dog Classifier";
            lblTitle.Size = new Size(440, 35);
            lblTitle.Location = new Point(20, 10);
            lblTitle.ForeColor = Color.White;
            lblTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            pictureBox.Size = new Size(440, 320);
            pictureBox.Location = new Point(20, 50);
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox.BackColor = Color.FromArgb(50, 50, 50);
            pictureBox.BorderStyle = BorderStyle.FixedSingle;

            btnSelect.Text = "Select Image";
            btnSelect.Size = new Size(440, 50);
            btnSelect.Location = new Point(20, 385);
            btnSelect.BackColor = Color.FromArgb(0, 120, 215);
            btnSelect.ForeColor = Color.White;
            btnSelect.FlatStyle = FlatStyle.Flat;
            btnSelect.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSelect.Cursor = Cursors.Hand;
            btnSelect.Click += BtnSelect_Click;

            lblResult.Text = "Please select an image to classify";
            lblResult.Size = new Size(440, 50);
            lblResult.Location = new Point(20, 450);
            lblResult.ForeColor = Color.White;
            lblResult.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblResult.TextAlign = ContentAlignment.MiddleCenter;

            lblConfidence.Text = "";
            lblConfidence.Size = new Size(440, 30);
            lblConfidence.Location = new Point(20, 508);
            lblConfidence.ForeColor = Color.LightGray;
            lblConfidence.Font = new Font("Segoe UI", 11);
            lblConfidence.TextAlign = ContentAlignment.MiddleCenter;

            this.Controls.AddRange([lblTitle, pictureBox, btnSelect, lblResult, lblConfidence]);
        }

        void LoadModel()
        {
            try
            {
                string modelPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..", "..", "..", "..",
                    "Image Classifier", "bin", "Debug", "net10.0", "model.zip"
                );

                if (!File.Exists(modelPath))
                    modelPath = "model.zip";

                var mlContext = new MLContext();
                var model = mlContext.Model.Load(modelPath, out _);
                _engine = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);

                lblResult.Text = "Model loaded. Select an image!";
                lblResult.ForeColor = Color.LightGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load model:\n{ex.Message}\n\nMake sure model.zip exists.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void BtnSelect_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";
            dialog.Title = "Select an image to classify";

            if (dialog.ShowDialog() != DialogResult.OK) return;

            string path = dialog.FileName;
            pictureBox.Image = Image.FromFile(path);

            if (_engine == null)
            {
                lblResult.Text = "Model not loaded!";
                lblResult.ForeColor = Color.Red;
                return;
            }

            var prediction = _engine.Predict(new ImageData { ImagePath = path });
            float confidence = prediction.Score?.Max() ?? 0;

            bool isCat = prediction.PredictedLabel?.ToLower() == "cat";
            lblResult.Text = isCat ? "Cat" : "Dog";
            lblResult.ForeColor = isCat
                ? Color.FromArgb(255, 180, 0)
                : Color.FromArgb(100, 200, 255);

            lblConfidence.Text = $"Confidence: {confidence:P1}";
        }
    }

    // 2. الكلاسات الإضافية توضع دائماً في أسفل الملف بعد الانتهاء من الفورم
    public class ImageData
    {
        public string ImagePath { get; set; } = "";
        public string Label { get; set; } = "";
    }

    public class ImagePrediction
    {
        [ColumnName("PredictedLabel")]
        public string PredictedLabel { get; set; } = "";
        public float[] Score { get; set; } = [];
    }
}
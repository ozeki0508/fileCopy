using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FileCopy
{
    public partial class Form1 : Form
    {
        private const string DefaultText = "検索するフォルダを「参照」またはドラッグ＆ドロップで指定";
        private const string DefaultText2 = "▼検索するフォルダより配下のパスを指定";

        // コンストラクタ
        public Form1()
        {
            InitializeComponent();
            this.label1.Text = DefaultText;
            this.label2.Text = DefaultText2;
        }

        // 参照
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "参照";
            dialog.SelectedPath = @"C:\Users\Owner\Desktop";

            if (dialog.ShowDialog() != DialogResult.OK) return;

            this.label1.Text = dialog.SelectedPath;
            dialog.Dispose();
        }

        // ファイルコピー実行
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var selectedPath = this.label1.Text;

                // エラーチェック
                if (selectedPath == DefaultText)
                {
                    MessageBox.Show("フォルダ名は必須入力だよ", "エラー", MessageBoxButtons.OK);
                    return;
                }

                var allPath = new List<string>();
                if (!string.IsNullOrWhiteSpace(this.textBox1.Text))
                {
                    // 下記だと"\r\n"が無効で例外
                    //var sr = new StreamReader(this.textBox1.Text);

                    foreach (var line in this.textBox1.Lines)
                    {
                        // 一行ずつファイル名一覧に追加
                        allPath.Add(line);
                    }
                }

                if (allPath.Any())
                {
                    var fullPath = selectedPath + @"\" + allPath[0];
                    if (File.Exists(fullPath) == false)
                    {
                        // 一行目のパスがさっそく存在しない場合エラー
                        MessageBox.Show("1行目から存在しないパスだよ\r\n" + fullPath,
                            "エラー",
                            MessageBoxButtons.OK);
                        return;
                    }
                }

                // ファイルコピー処理
                string destDir = "";
                if (allPath.Any())
                {
                    var treePathData = new TreePathData();
                    foreach (var path in allPath)
                    {
                        // フォルダ階層を作成
                        TreePathData.CreateTreePath(0, path, treePathData);
                    }

                    // コピー先フォルダを作成
                    var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    destDir = desktop + @"\VerXXX";
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // ディレクトリコピー
                    this.DirectoryCopy(selectedPath, treePathData, destDir);
                    // ファイルコピー
                    this.FileCopy(selectedPath, treePathData, destDir);
                }

                MessageBox.Show("コピーしました。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // コピー先をエクスプローラーで開く
                if (!string.IsNullOrWhiteSpace(destDir))
                {
                    System.Diagnostics.Process.Start(destDir);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.GetBaseException().ToString(), "例外", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// ディレクトリのコピー
        /// </summary>
        /// <param name="upperPath">上位パス</param>
        /// <param name="sourcePath">コピー元フォルダ階層</param>
        /// <param name="destPath">コピー先パス</param>
        private void DirectoryCopy(string upperPath, TreePathData sourcePath, string destPath)
        {
            // コピー先ディレクトリ
            var sb = new StringBuilder();
            sb.Append(destPath);
            if (sourcePath.IsValidPath)
            {
                sb.Append(@"\");
                sb.Append(sourcePath.DirName);
            }
            DirectoryInfo destDirectory = new DirectoryInfo(sb.ToString());

            if (sourcePath.IsValidPath)
            {
                // コピー元ディレクトリ
                sb.Clear();
                sb.Append(upperPath);
                sb.Append(@"\");
                sb.Append(sourcePath.DirName);
                DirectoryInfo sourceDirectory = new DirectoryInfo(sb.ToString());

                // コピー先のディレクトリを作成する
                if (destDirectory.Exists == false)
                {
                    destDirectory.Create();
                    destDirectory.Attributes = sourceDirectory.Attributes;
                }

                // 上位のパスにカレントパスをくっつける
                sb.Clear();
                sb.Append(upperPath);
                sb.Append(@"\");
                sb.Append(sourcePath.DirName);
                upperPath = sb.ToString();
            }

            foreach (var childPath in sourcePath.ChildList)
            {
                if (childPath.IsFile) continue;

                // 下位のディレクトリをコピー（再帰を使用）
                this.DirectoryCopy(upperPath, childPath, destDirectory.FullName);
            }
        }

        /// <summary>
        /// ファイルコピー
        /// </summary>
        /// <param name="upperPath">上位パス</param>
        /// <param name="sourcePath">コピー元フォルダ階層</param>
        /// <param name="destPath">コピー先パス</param>
        private void FileCopy(string upperPath, TreePathData sourcePath, string destPath)
        {
            // コピー先ファイル
            var sb = new StringBuilder();
            sb.Append(destPath);
            if (sourcePath.IsValidPath)
            {
                sb.Append(@"\");
                sb.Append(sourcePath.DirName);
            }
            FileInfo destFile = new FileInfo(sb.ToString());

            if (sourcePath.IsValidPath)
            {
                var bolCopy = true;
                if (!sourcePath.IsFile) bolCopy = false;
                if (!this.checkBox1.Checked)
                {
                    if (sourcePath.IsUnneedFile) bolCopy = false;
                }

                if (bolCopy)
                {
                    // コピー元ファイル
                    sb.Clear();
                    sb.Append(upperPath);
                    sb.Append(@"\");
                    sb.Append(sourcePath.DirName);
                    FileInfo sourceFile = new FileInfo(sb.ToString());

                    File.Copy(sourceFile.FullName, destFile.FullName, true);
                }

                // 上位のパスにカレントパスをくっつける
                sb.Clear();
                sb.Append(upperPath);
                sb.Append(@"\");
                sb.Append(sourcePath.DirName);
                upperPath = sb.ToString();
            }

            foreach (var childPath in sourcePath.ChildList)
            {
                // 子要素のファイルコピー（再帰を使用）
                this.FileCopy(upperPath, childPath, destFile.FullName);
            }
        }

        private void label1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void label1_DragDrop(object sender, DragEventArgs e)
        {
            // ファイルが渡されていなければ、何もしない
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var selectedPath = e.Data.GetData(DataFormats.FileDrop) as string[];
            DirectoryInfo dir = new DirectoryInfo(selectedPath[0]);

            if (dir.Attributes == FileAttributes.Directory)
            {
                // 渡されたファイルがディレクトリの場合のみ、処理を行う
                this.label1.Text = selectedPath[0];
            }
        }
    }
}

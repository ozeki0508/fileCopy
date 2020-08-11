using System.Collections.Generic;

namespace FileCopy
{
    /// <summary>
    /// フォルダ階層クラス
    /// </summary>
    public class TreePathData
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TreePathData()
        {
            this.Hierarchy = -1;
            this.DirName = "ParentNode";
            this.ParentNode = null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private TreePathData(int hierarchy, string dirName, TreePathData parentNode)
        {
            this.Hierarchy = hierarchy;
            this.DirName = dirName;
            this.ParentNode = parentNode;
        }

        /// <summary>階層</summary>
        public int Hierarchy { get; set; }

        /// <summary>この要素の格納値（ディレクトリ名）</summary>
        public string DirName { get; set; }

        /// <summary>この要素の親要素</summary>
        public TreePathData ParentNode { get; set; }

        /// <summary>子要素リスト</summary>
        public List<TreePathData> ChildList { get; set; } = new List<TreePathData>();

        /// <summary>ファイルかどうか</summary>
        public bool IsFile
        {
            get { return this.ChildList.Count == 0; }
        }

        /// <summary>
        /// 同じ要素が存在するかどうか
        /// </summary>
        /// <returns></returns>
        private bool Exists(int hierarchy, string dirName, out TreePathData treePath)
        {
            treePath = null;

            if (this.Hierarchy == hierarchy
                && this.DirName == dirName)
            {
                treePath = this;
                return true;
            }

            foreach(var child in this.ChildList)
            {
                if (child.Hierarchy == hierarchy
                    && child.DirName == dirName)
                {
                    treePath = child;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// フォルダ階層の作成
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <param name="copyPath"></param>
        public static void CreateTreePath(int hierarchy, string copyPath, TreePathData parentNode)
        {
            // パスの先頭から"\"で区切ったディレクトリ名orファイル名だけ取得
            var length = copyPath.IndexOf(@"\");
            if (length == -1) length = copyPath.Length;
            var dirName = copyPath.Substring(0, length);

            // フォルダ階層作成
            TreePathData treePath = null;
            TreePathData findPath = null;
            if (parentNode.Exists(hierarchy, dirName, out findPath))
            {
                treePath = findPath;
            }
            else
            {
                treePath = new TreePathData(hierarchy, dirName, parentNode);
                parentNode.ChildList.Add(treePath);
            }

            // 元のパスからディレクトリ名orファイル名と、先頭の"\"を除外
            // ファイルまで完了したらブランクをセット
            if (copyPath.IndexOf(@"\") > -1)
            {
                copyPath = copyPath.Substring(length + 1);
            }
            else
            {
                copyPath = "";
            }

            if (!string.IsNullOrWhiteSpace(copyPath))
            {
                // 下位のフォルダ階層の作成（再帰を使用）
                CreateTreePath(hierarchy + 1, copyPath, treePath);
            }
        }

    }
}

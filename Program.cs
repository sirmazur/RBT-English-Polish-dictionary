using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections;

class program
{
    public class RedBlackTree<TKey, TValue> where TKey : IComparable<TKey>
    {
        private enum NodeColor
        {
            Red,
            Black
        }

        private class RedBlackTreeNode
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public RedBlackTreeNode Left { get; set; }
            public RedBlackTreeNode Right { get; set; }
            public NodeColor Color { get; set; }
            public int Size { get; set; }

            public RedBlackTreeNode(TKey key, TValue value, NodeColor color, int size)
            {
                Key = key;
                Value = value;
                Color = color;
                Size = size;
            }
        }

        private RedBlackTreeNode root;

        public bool IsEmpty
        {
            get { return root == null; }
        }
        
        public TValue Get(TKey key)
        {
            RedBlackTreeNode node = Get(root, key);
            if (node == null)
            {
                throw new KeyNotFoundException();
            }
            return node.Value;
        }

        private RedBlackTreeNode Get(RedBlackTreeNode node, TKey key)
        {
            if (node == null)
            {
                return null;
            }

            int cmp = key.CompareTo(node.Key);
            if (cmp < 0)
            {
                return Get(node.Left, key);
            }
            else if (cmp > 0)
            {
                return Get(node.Right, key);
            }
            else
            {
                return node;
            }
        }

        public bool Contains(TKey key)
        {
            RedBlackTreeNode node = Get(root, key);
            return node != null;
        }

        public void Put(TKey key, TValue value)
        {
            root = Put(root, key, value);
            root.Color = NodeColor.Black;
        }

        private RedBlackTreeNode Put(RedBlackTreeNode node, TKey key, TValue value)
        {
            if (node == null)
            {
                return new RedBlackTreeNode(key, value, NodeColor.Red, 1);
            }

            int cmp = key.CompareTo(node.Key);
            if (cmp < 0)
            {
                node.Left = Put(node.Left, key, value);
            }
            else if (cmp > 0)
            {
                node.Right = Put(node.Right, key, value);
            }
            else
            {
                node.Value = value;
            }

            if (IsRed(node.Right) && !IsRed(node.Left))
            {
                node = RotateLeft(node);
            }
            if (IsRed(node.Left) && IsRed(node.Left.Left))
            {
                node = RotateRight(node);
            }
            if (IsRed(node.Left) && IsRed(node.Right))
            {
                FlipColors(node);
            }
            node.Size = 1 + Size(node.Left) + Size(node.Right);

            return node;
        }
        private RedBlackTreeNode DeleteMin(RedBlackTreeNode node)
        {
            if (node.Left == null)
            {
                return null;
            }

            if (!IsRed(node.Left) && !IsRed(node.Left.Left))
            {
                node = MoveRedLeft(node);
            }

            node.Left = DeleteMin(node.Left);
            return Balance(node);
        }
        private RedBlackTreeNode DeleteMax(RedBlackTreeNode node)
        {
            if (IsRed(node.Left))
            {
                node = RotateRight(node);
            }

            if (node.Right == null)
            {
                return null;
            }

            if (!IsRed(node.Right) && !IsRed(node.Right.Left))
            {
                node = MoveRedRight(node);
            }

            node.Right = DeleteMax(node.Right);
            return Balance(node);
        }

        public void Delete(TKey key)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException();
            }

            if (!Contains(key))
            {
                return;
            }

            if (!IsRed(root.Left) && !IsRed(root.Right))
            {
                root.Color = NodeColor.Red;
            }

            root = Delete(root, key);
            if (!IsEmpty)
            {
                root.Color = NodeColor.Black;
            }
        }

        private RedBlackTreeNode Delete(RedBlackTreeNode node, TKey key)
        {
            if (key.CompareTo(node.Key) < 0)
            {
                if (!IsRed(node.Left) && !IsRed(node.Left.Left))
                {
                    node = MoveRedLeft(node);
                }
                node.Left = Delete(node.Left, key);
            }
            else
            {
                if (IsRed(node.Left))
                {
                    node = RotateRight(node);
                }
                if (key.CompareTo(node.Key) == 0 && (node.Right == null))
                {
                    return null;
                }
                if (!IsRed(node.Right) && !IsRed(node.Right.Left))
                {
                    node = MoveRedRight(node);
                }
                if (key.CompareTo(node.Key) == 0)
                {
                    RedBlackTreeNode x = Min(node.Right);
                    node.Key = x.Key;
                    node.Value = x.Value;
                    node.Right = DeleteMin(node.Right);
                }
                else
                {
                    node.Right = Delete(node.Right, key);
                }
            }
            return Balance(node);
        }

        public TKey MinKey
        {
            get
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException();
                }
                return Min(root).Key;
            }
        }

        private RedBlackTreeNode Min(RedBlackTreeNode node)
        {
            if (node.Left == null)
            {
                return node;
            }
            return Min(node.Left);
        }

        public TKey MaxKey
        {
            get
            {
                if (IsEmpty)
                {
                    throw new InvalidOperationException();
                }
                return Max(root).Key;
            }
        }

        private RedBlackTreeNode Max(RedBlackTreeNode node)
        {
            if (node.Right == null)
            {
                return node;
            }
            return Max(node.Right);
        }

        public IEnumerable<TKey> Keys()
        {
            if (IsEmpty)
            {
                yield break;
            }

            Stack<RedBlackTreeNode> stack = new Stack<RedBlackTreeNode>();
            RedBlackTreeNode node = root;
            while (stack.Count > 0 || node != null)
            {
                if (node != null)
                {
                    stack.Push(node);
                    node = node.Left;
                }
                else
                {
                    node = stack.Pop();
                    yield return node.Key;
                    node = node.Right;
                }
            }
        }

        private bool IsRed(RedBlackTreeNode node)
        {
            if (node == null)
            {
                return false;
            }
            return node.Color == NodeColor.Red;
        }

        private RedBlackTreeNode RotateLeft(RedBlackTreeNode node)
        {
            RedBlackTreeNode x = node.Right;
            node.Right = x.Left;
            x.Left = node;
            x.Color = node.Color;
            node.Color = NodeColor.Red;
            x.Size = node.Size;
            node.Size = 1 + Size(node.Left) + Size(node.Right);
            return x;
        }

        private RedBlackTreeNode RotateRight(RedBlackTreeNode node)
        {
            RedBlackTreeNode x = node.Left;
            node.Left = x.Right;
            x.Right = node; x.Color = node.Color;
            node.Color = NodeColor.Red;
            x.Size = node.Size;
            node.Size = 1 + Size(node.Left) + Size(node.Right);
            return x;
        }

        private void FlipColors(RedBlackTreeNode node)
        {
            node.Color = NodeColor.Red;
            node.Left.Color = NodeColor.Black;
            node.Right.Color = NodeColor.Black;
        }

        private RedBlackTreeNode MoveRedLeft(RedBlackTreeNode node)
        {
            FlipColors(node);
            if (IsRed(node.Right.Left))
            {
                node.Right = RotateRight(node.Right);
                node = RotateLeft(node);
                FlipColors(node);
            }
            return node;
        }

        private RedBlackTreeNode MoveRedRight(RedBlackTreeNode node)
        {
            FlipColors(node);
            if (IsRed(node.Left.Left))
            {
                node = RotateRight(node);
                FlipColors(node);
            }
            return node;
        }

        private RedBlackTreeNode Balance(RedBlackTreeNode node)
        {
            if (IsRed(node.Right))
            {
                node = RotateLeft(node);
            }
            if (IsRed(node.Left) && IsRed(node.Left.Left))
            {
                node = RotateRight(node);
            }
            if (IsRed(node.Left) && IsRed(node.Right))
            {
                FlipColors(node);
            }

            node.Size = 1 + Size(node.Left) + Size(node.Right);
            return node;
        }

        private int Size(RedBlackTreeNode node)
        {
            return node == null ? 0 : node.Size;
        }
    }
    class TwoLanguageDictionary
    {
        private class Entry
        {
            public string English;
            public List<string> Polish;

            public Entry(string english)
            {
                English = english;
                Polish = new List<string>();
            }
        }

        private RedBlackTree<string, Entry> tree;

        public TwoLanguageDictionary()
        {
            tree = new RedBlackTree<string, Entry>();
        }

        public void AddTranslation(string english, string polish)
        {


            if (tree.Contains(english)==true)
            { Entry entry = tree.Get(english); entry.Polish.Add(polish); }
            else
            {
                Entry entry = new Entry(english);
                tree.Put(english, entry);
                entry.Polish.Add(polish);
            }

        }
        public void DeleteTranslation(string english, string polish)
        {
            if (tree.Contains(english)==true)
            { Entry entry = tree.Get(english); entry.Polish.Remove(polish); if (entry.Polish.Count == 0) tree.Delete(english); }
            else
            {
                Entry entry = new Entry(english);
                entry.Polish.Remove(polish);
                if (entry.Polish.Count == 0)
                {
                    tree.Delete(english);
                }
            }
        }
       

        public IEnumerable<string> GetTranslations(string word)
        {
            try
            {
                Entry entry = tree.Get(word);
                return entry != null ? entry.Polish : Enumerable.Empty<string>();
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("No translations...");
                Thread.Sleep(2000);
                Console.Clear();
                return Enumerable.Empty<string>();
            }

        }

        public IEnumerable<string> EnglishWords()
        {
            return tree.Keys();
        }

        public IEnumerable<string> PolishTranslations()
        {
            foreach (string english in EnglishWords())
            {
                foreach (string polish in GetTranslations(english))
                {
                    yield return polish;
                }
            }
        }
    }
    static void Main(string[] args)
    {
        string word_1=null, word_2=null;
        int choice = 0;
        TwoLanguageDictionary dictionary = new TwoLanguageDictionary();
        ImportTranslations(dictionary);
        while (true)
        {

            Console.WriteLine("1:Find translations\n2:Add translation\n3:Remove translation\n4.Export to file");
            choice=Convert.ToInt32(Console.ReadLine());
            Console.Clear();
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Insert english word:");
                    word_1 = Console.ReadLine();
                    foreach (string translation in dictionary.GetTranslations(word_1))
                    {
                        Console.WriteLine(translation);
                    }
                    break;
                case 2:
                    Console.WriteLine("Insert english word:");
                    word_1 = Console.ReadLine();
                    WriteFile(word_1);
                    Console.Clear();
                    Console.WriteLine("Insert polish translation:");
                    word_2 = Console.ReadLine();
                    WriteFile(word_2);
                    Console.Clear();
                    dictionary.AddTranslation(word_1, word_2);
                    break;
                case 3:
                    Console.WriteLine("Insert english word:");
                    word_1 = Console.ReadLine();
                    Console.Clear();
                    Console.WriteLine("Insert polish translation:");
                    word_2 = Console.ReadLine();
                    Console.Clear();
                    dictionary.DeleteTranslation(word_1, word_2);
                    break;
                case 4:
                    ImportTranslations(dictionary);
                    break;
                default:
                    break;
            }
        }
    }
    static void WriteFile(string text)
    {
        var dir = Directory.GetCurrentDirectory();
        var file = Path.Combine(dir, "File.dat");
        try
        {
            FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write);
            if (fs.CanWrite)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(text+"\n");
                fs.Write(buffer,0,buffer.Length);
            }
        fs.Flush();
            fs.Close();
        }
        catch(Exception ex)
        {
            Console.WriteLine(Environment.NewLine + ex.Message);
        }
    }
    static void EmptyFile()
    {
        File.Delete("File.dat");
    }
    public static List<string> ReadStringsFromBinaryFile(string filePath)
    {
        List<string> strings = new List<string>();
        try
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    strings.Add(reader.ReadLine());
                }
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(Environment.NewLine + e.Message);
        }

        return strings;
    }
    static void ImportTranslations(TwoLanguageDictionary dictionary)
    {
        List<string> FileContent = ReadStringsFromBinaryFile("File.dat");
        for (int i = 1; i<FileContent.Count; i+=2)
        {
            string word_1, word_2;
            word_1=FileContent[i-1];
            word_2=FileContent[i];
            dictionary.AddTranslation(word_1, word_2);
        }
    }
    static void ExportTranslations(TwoLanguageDictionary dictionary)
    {
        List<string> translations= new List<string>();
        Console.WriteLine("Exporting dictionary in progress...");
        translations.Concat(dictionary.PolishTranslations());
        foreach(string translation in translations)
        {
            WriteFile(translation);
        }
        Thread.Sleep(500);
        Console.Clear();
    }
}
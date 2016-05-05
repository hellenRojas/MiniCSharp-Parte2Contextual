using AnalizadorSintactico;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using FarsiLibrary.Win;
using FastColoredTextBoxNS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AnalisisSintactico
{
    public partial class Form1 : Form
    {
        ArrayList listFileName = new ArrayList();

        Style invisibleCharsStyle = new InvisibleCharsRenderer(Pens.Gray);
        Color currentLineColor = Color.FromArgb(100, 210, 210, 255);
        Color changedLineColor = Color.FromArgb(255, 230, 230, 255);
     
        public Form1()
        {
            InitializeComponent();
        }

        private Style sameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(50, Color.Gray)));
        private void CreateTab(string fileName)
        {
            try
            {
                var tb = new FastColoredTextBox();
                tb.Font = new Font("Consolas", 10.75f);
                tb.ContextMenuStrip = null;
                tb.Dock = DockStyle.Fill;
                tb.BorderStyle = BorderStyle.Fixed3D;
                //tb.VirtualSpace = true;
                tb.LeftPadding = 17;
                tb.Language = Language.CSharp;
                tb.AddStyle(sameWordsStyle);//same words style
                var tab = new FATabStripItem(fileName != null ? Path.GetFileName(fileName) : "[nuevo]", tb);
                tab.Tag = fileName;
                if (fileName != null)
                    tb.OpenFile(fileName);
                tb.Tag = new TbInfo();
                tb.KeyUp += new KeyEventHandler(tb_keyup);
                tb.MouseClick += new MouseEventHandler(tb_mouseclick);
                tabcode.AddTab(tab);
                tabcode.SelectedItem = tab;
                tb.Focus();
                tb.DelayedTextChangedInterval = 1000;
                tb.DelayedEventsInterval = 500;
                tb.TextChangedDelayed += new EventHandler<TextChangedEventArgs>(tb_TextChangedDelayed);
                tb.SelectionChangedDelayed += new EventHandler(tb_SelectionChangedDelayed);
                tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                tb.MouseMove += new MouseEventHandler(tb_MouseMove);
                tb.ChangedLineColor = changedLineColor;


            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Retry)
                    CreateTab(fileName);
            }

        }

        FastColoredTextBox CurrentTB
        {
            get
            {
                if (tabcode.SelectedItem == null)
                    return null;
                return (tabcode.SelectedItem.Controls[0] as FastColoredTextBox);
            }

            set
            {
                tabcode.SelectedItem = (value.Parent as FATabStripItem);
                value.Focus();
            }
        }

        void tb_keyup(object sender, KeyEventArgs e) {
            int a=((FastColoredTextBox)sender).Selection.Start.iLine +1;
            int b = ((FastColoredTextBox)sender).Selection.Start.iChar +1;
            numc.Text = b.ToString();
            numl.Text=a.ToString();
        }
        void tb_mouseclick(object sender, MouseEventArgs e)
        {
            int a = ((FastColoredTextBox)sender).Selection.Start.iLine + 1;
            int b = ((FastColoredTextBox)sender).Selection.Start.iChar +1;
            numc.Text = b.ToString();
            numl.Text = a.ToString();
        }
        void tb_MouseMove(object sender, MouseEventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            var place = tb.PointToPlace(e.Location);
            var r = new Range(tb, place, place);

            string text = r.GetFragment("[a-zA-Z]").Text;

        }
        void popupMenu_Opening(object sender, CancelEventArgs e)
        {
            //---block autocomplete menu for comments
            //get index of green style (used for comments)
            var iGreenStyle = CurrentTB.GetStyleIndex(CurrentTB.SyntaxHighlighter.GreenStyle);
            if (iGreenStyle >= 0)
                if (CurrentTB.Selection.Start.iChar > 0)
                {
                    //current char (before caret)
                    var c = CurrentTB[CurrentTB.Selection.Start.iLine][CurrentTB.Selection.Start.iChar - 1];
                    //green Style
                    var greenStyleIndex = Range.ToStyleIndex(iGreenStyle);
                    //if char contains green style then block popup menu
                    if ((c.style & greenStyleIndex) != 0)
                        e.Cancel = true;
                }
        }

        void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.OemMinus)
            {

                e.Handled = true;

            }

            if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.OemMinus)
            {

                e.Handled = true;
            }

            if (e.KeyData == (Keys.K | Keys.Control))
            {
                //forced show (MinFragmentLength will be ignored)
                (CurrentTB.Tag as TbInfo).popupMenu.Show(true);
                e.Handled = true;
            }
        }
        DateTime lastNavigatedDateTime = DateTime.Now;

        void tb_SelectionChangedDelayed(object sender, EventArgs e)
        {
            var tb = sender as FastColoredTextBox;
            //remember last visit time
            if (tb.Selection.IsEmpty && tb.Selection.Start.iLine < tb.LinesCount)
            {
                if (lastNavigatedDateTime != tb[tb.Selection.Start.iLine].LastVisit)
                {
                    tb[tb.Selection.Start.iLine].LastVisit = DateTime.Now;
                    lastNavigatedDateTime = tb[tb.Selection.Start.iLine].LastVisit;
                }
            }

            //highlight same words
            tb.VisibleRange.ClearStyle(sameWordsStyle);
            if (!tb.Selection.IsEmpty)
                return;//user selected diapason
            //get fragment around caret
            var fragment = tb.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            Range[] ranges = tb.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();

            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(sameWordsStyle);
        }

        void tb_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            FastColoredTextBox tb = (sender as FastColoredTextBox);
            //rebuild object explorer
            string text = (sender as FastColoredTextBox).Text;


            //show invisible chars
            HighlightInvisibleChars(e.ChangedRange);
        }


        private void HighlightInvisibleChars(Range range)
        {

        }

        List<ExplorerItem> explorerList = new List<ExplorerItem>();

        private void ReBuildObjectExplorer(string text)
        {
            try
            {
                List<ExplorerItem> list = new List<ExplorerItem>();
                int lastClassIndex = -1;
                //find classes, methods and properties
                Regex regex = new Regex(@"^(?<range>[\w\s]+\b(class|struct|enum|interface)\s+[\w<>,\s]+)|^\s*(public|private|internal|protected)[^\n]+(\n?\s*{|;)?", RegexOptions.Multiline);
                foreach (Match r in regex.Matches(text))
                    try
                    {
                        string s = r.Value;
                        int i = s.IndexOfAny(new char[] { '=', '{', ';' });
                        if (i >= 0)
                            s = s.Substring(0, i);
                        s = s.Trim();

                        var item = new ExplorerItem() { title = s, position = r.Index };
                        if (Regex.IsMatch(item.title, @"\b(class|struct|enum|interface)\b"))
                        {
                            item.title = item.title.Substring(item.title.LastIndexOf(' ')).Trim();
                            item.type = ExplorerItemType.Class;
                            list.Sort(lastClassIndex + 1, list.Count - (lastClassIndex + 1), new ExplorerItemComparer());
                            lastClassIndex = list.Count;
                        }
                        else
                            if (item.title.Contains(" event "))
                            {
                                int ii = item.title.LastIndexOf(' ');
                                item.title = item.title.Substring(ii).Trim();
                                item.type = ExplorerItemType.Event;
                            }
                            else
                                if (item.title.Contains("("))
                                {
                                    var parts = item.title.Split('(');
                                    item.title = parts[0].Substring(parts[0].LastIndexOf(' ')).Trim() + "(" + parts[1];
                                    item.type = ExplorerItemType.Method;
                                }
                                else
                                    if (item.title.EndsWith("]"))
                                    {
                                        var parts = item.title.Split('[');
                                        if (parts.Length < 2) continue;
                                        item.title = parts[0].Substring(parts[0].LastIndexOf(' ')).Trim() + "[" + parts[1];
                                        item.type = ExplorerItemType.Method;
                                    }
                                    else
                                    {
                                        int ii = item.title.LastIndexOf(' ');
                                        item.title = item.title.Substring(ii).Trim();
                                        item.type = ExplorerItemType.Property;
                                    }
                        list.Add(item);
                    }
                    catch { ;}

                list.Sort(lastClassIndex + 1, list.Count - (lastClassIndex + 1), new ExplorerItemComparer());

                BeginInvoke(
                    new Action(() =>
                    {
                        explorerList = list;

                    })
                );
            }
            catch { ;}
        }

        enum ExplorerItemType
        {
            Class, Method, Property, Event
        }

        class ExplorerItem
        {
            public ExplorerItemType type;
            public string title;
            public int position;
        }

        class ExplorerItemComparer : IComparer<ExplorerItem>
        {
            public int Compare(ExplorerItem x, ExplorerItem y)
            {
                return x.title.CompareTo(y.title);
            }
        }


        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void compilar(FATabStripItem tab)
        {
            
          
            var tb = (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null)
            {
                MessageBox.Show("archivo no guardado");
            }
            else
            {
                error.Clear();
                treeView1.Nodes.Clear();
                StreamReader inputStream = new StreamReader(tab.Tag.ToString());
                AntlrInputStream input = new AntlrInputStream(inputStream.ReadToEnd());
                try
                {
                    Lexer1 lexer = new Lexer1(input);
                    CommonTokenStream tokens = new CommonTokenStream(lexer);
                    Parser1 parser = new Parser1(tokens);
                    /*
                    IList<IToken> listatokens = lexer.GetAllTokens();
                    
                    foreach (IToken token in listatokens)
                    {
                        Console.WriteLine("Token: " + token.Type + " , Lexema: " + token.Text);
                    }
                    
                    */
                    Console.WriteLine("************************************************");
                    error.AppendText("Compilando...\n");
                    parser.RemoveErrorListeners();
                    parser.AddErrorListener(ParserErrorListener.Instancia);
                    parser.ErrorHandler = new DefaultErrorStrategy1();
                    
                    IParseTree tree = parser.program();
                  

                    PrettyPrint p = new PrettyPrint(treeView1);
                    p.Visit(tree);

                    AContextual v = new AContextual();
                    v.Visit(tree);

                    error.AppendText(v.msgError);
                    
                    error.AppendText("Fin de compilación...\n");

                }
                catch (Exception e)
                {
                    error.AppendText("Compilación Erronea...\n");
                    TreeNode er = new TreeNode("Errores en el código");
                    treeView1.Nodes.Add(er);
                }
                inputStream.Close();
            }
        }


        public class InvisibleCharsRenderer : Style
        {
            Pen pen;

            public InvisibleCharsRenderer(Pen pen)
            {
                this.pen = pen;
            }

            public override void Draw(Graphics gr, Point position, Range range)
            {
                var tb = range.tb;
                using (Brush brush = new SolidBrush(pen.Color))
                    foreach (var place in range)
                    {
                        switch (tb[place].c)
                        {
                            case ' ':
                                var point = tb.PlaceToPoint(place);
                                point.Offset(tb.CharWidth / 2, tb.CharHeight / 2);
                                gr.DrawLine(pen, point.X, point.Y, point.X + 1, point.Y);
                                break;
                        }

                        if (tb[place.iLine].Count - 1 == place.iChar)
                        {
                            var point = tb.PlaceToPoint(place);
                            point.Offset(tb.CharWidth, 0);
                            gr.DrawString("¶", tb.Font, brush, point);
                        }
                    }
            }
        }

        public class TbInfo
        {
            public AutocompleteMenu popupMenu;
        }


        private bool Save(FATabStripItem tab)
        {
            var tb = (tab.Controls[0] as FastColoredTextBox);
            if (tab.Tag == null)
            {
                SaveFileDialog s = new SaveFileDialog();
                s.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                if (s.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return false;
                tab.Title = Path.GetFileName(s.FileName);
                tab.Tag = s.FileName;
            }

            try
            {
                File.WriteAllText(tab.Tag as string, tb.Text);
                tb.IsChanged = false;
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry)
                    return Save(tab);
                else
                    return false;
            }

            tb.Invalidate();

            return true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

 

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            if (o.ShowDialog() == DialogResult.OK)
                CreateTab(o.FileName);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CreateTab(null);
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (tabcode.SelectedItem != null)
            {
                Save(tabcode.SelectedItem);
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            CurrentTB.Cut();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            CurrentTB.Copy();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            CurrentTB.SelectAll();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            CurrentTB.Paste();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            CurrentTB.Undo();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            CurrentTB.Redo();
        }

        private void tabcode_TabStripItemClosing(TabStripItemClosingEventArgs e)
        {
            if ((e.Item.Controls[0] as FastColoredTextBox).IsChanged)
            {
                switch (MessageBox.Show("Desea guardar antes de cerrar: " + e.Item.Title + " ?", "Save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        if (!Save(e.Item))
                            e.Cancel = true;
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            if (tabcode.SelectedItem != null)
            {
                compilar(tabcode.SelectedItem);
            }
        }
    }
}

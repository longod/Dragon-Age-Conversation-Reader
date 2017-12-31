// (c) hikami, aka longod
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dao_reader {
    public partial class Form1 : Form {
        string version = "0.8.1";
        string exe_path;
        string exe_dir;
        string config_path;
        string lookup_path;
        Loockup lookup;
        Setting.Config config = new Setting.Config();
        SearchBox search;
        SearchBox.Option search_option = new SearchBox.Option();
 
        public Form1() {
            InitializeComponent();

            this.Text += " " + version;


            setStatus( "" );

            //setStatus( "Loading..." );

            //System.Reflection.Assembly assem = System.Reflection.Assembly.GetExecutingAssembly();
            //string exe_path = assem.Location;

            exe_path = Application.ExecutablePath;
            exe_dir = System.IO.Path.GetDirectoryName( exe_path );
            config_path = exe_dir + @"\config.xml";
            lookup_path = exe_dir + @"\loockup.xml";

            if ( System.IO.File.Exists( config_path ) ) {
                Setting.Config temp = xml.Xml.Read<Setting.Config>( config_path );
                if ( temp != null ) { // �������������s�������O�ł������� InvalidOperationException
                    config = temp;
                }
            } else {
                xml.Xml.Write<Setting.Config>( config_path, config );
            }
            
            
            // �J�����g�ɂ��邩���s�t�@�C���ɂ��邩�Y�� �f�o�b�O���̓J�����g���y�Ȃ񂾂���
            //string exe_dir = System.IO.Directory.GetCurrentDirectory();
            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo( exe_dir );
            System.IO.FileInfo[] files = info.GetFiles("*.txt");
            System.IO.DirectoryInfo[] dirs = info.GetDirectories();
            List<string> match_list = new List<string>();
            foreach ( System.IO.FileInfo fi in files ) {
                string name = System.IO.Path.GetFileNameWithoutExtension( fi.Name ); // �ǂ�����info�ɂ���K�v�Ȃ����� �ł�string�擾���Ē���info����Ă����Ȃ񂾂�
                foreach ( System.IO.DirectoryInfo di in dirs ) {
                    if ( name == di.Name ) {
                        match_list.Add( fi.Name );
                        // list���Ȃɂ������Ń��[�h����Ⴈ��
                        setup( fi.FullName, di.FullName );
                    }
                }
            }

            if ( System.IO.File.Exists( lookup_path ) ) {
                lookup = xml.Xml.Read<Loockup>( lookup_path );
            }
            // �e���͖�������O���o��
            //http://yk.tea-nifty.com/netdev/2009/03/xmlserializer-a.html

            // �F�X�ȕ�����SuspendLayout/ResumeLayout�Ń��[�h���̉�ʍ\�z���~�߂č������ł������Ȃ񂾂�
            //System.IO.Directory.GetDirectories();
            // gffread���������āAtoolset�C���X�g�[���ґO��̂���y���ԃt�@�C���\�z�R�}���h����Ă��������Ȃ�

            //color
            setColor();


            // add handler
            toolStripMenuItemEdit.DropDownOpening += new EventHandler( toolStripMenuItemEdit_DropDownOpening );

        }

        TreeView getSelectedTree() {
            TreeView tree = null;
            if ( tabControl.SelectedTab != null ) {
                TabPage tab = tabControl.SelectedTab;
                tree = tab.Controls[ tab.Name ] as TreeView; // ���ꖼ�Ȃ�Ƃ����O�񂾂�
            }
            return tree;
        }
        TreeNode getSelectedNode() {
            TreeNode node = null;
            TreeView tree = getSelectedTree();
            if ( tree != null ) {
                node = tree.SelectedNode;
            }
            return node;
        }

        void toolStripMenuItemEdit_DropDownOpening( object sender, EventArgs e ) {
            // ���邢�͖����ꍇ�ł��N���b�v�{�[�h�ɂ���ꍇ�͂�����g���Ƃ��H
            if ( search_option.search_word != null && search_option.search_word.Length > 0 ) {
                menuItemSearchNext.Enabled = true;
                menuItemSearchPrev.Enabled = true;
            } else {
                menuItemSearchNext.Enabled = false;
                menuItemSearchPrev.Enabled = false;
            }

            TreeNode node = getSelectedNode();
            if ( node != null ) {
                menuItemCopy.Enabled = true;
                menuItemCopyAll.Enabled = true;
                menuItemCopyAllTab.Enabled = true;
                menuItemExpand.Enabled = true;
                menuItemExpandAll.Enabled = true;
                menuItemCollapse.Enabled = true;
                menuItemCollapseAll.Enabled = true;
            } else {
                menuItemCopy.Enabled = false;
                menuItemCopyAll.Enabled = false;
                menuItemCopyAllTab.Enabled = false;
                menuItemExpand.Enabled = false;
                menuItemExpandAll.Enabled = false;
                menuItemCollapse.Enabled = false;
                menuItemCollapseAll.Enabled = false;
            }
        }

        void treeView_MouseDown( object sender, MouseEventArgs e ) {
            // ���E�ǂ���ł������͓����ɂ��悤
            if ( e.Button == MouseButtons.Left || e.Button == MouseButtons.Right ) {
                TreeView tree = sender as TreeView;
                if ( tree != null ) {
                    Point p = tree.PointToClient( Cursor.Position );
                    //TreeNode node = tree.GetNodeAt( p );
                    TreeViewHitTestInfo info = tree.HitTest( p );
                    //info.Location == TreeViewHitTestLocations.Label;

                    if ( info.Node != null ) {
                        tree.SelectedNode = info.Node;
                    }
                }
            }
        }

        void treeView_AfterSelect( object sender, TreeViewEventArgs e ) {
            // conversation�̏ꍇ�͏�ɔ��f����
            TreeNode node = e.Node;
            ConversationNode conv = node as ConversationNode;
            string status = "";
            if ( conv != null ) {
                //textBoxID.Text = conv.id.ToString();
                uint id = conv.id;

                string jpfile = null;
                if ( lookup != null && tabControl.SelectedTab.Name != null ) {
                    foreach ( Loockup.Module mod in lookup.module ) {
                        if ( mod.name == tabControl.SelectedTab.Name ) {
                            foreach ( Loockup.Module.Set set in mod.set ) {
                                if ( set.min_id <= id ) {
                                    // �I�[��0�ɂƂ肠�����A�����max_id�͊܂߂Ȃ�
                                    if ( set.max_id == 0 || id < set.max_id ) {
                                        jpfile = set.file;
                                        break;
                                    }
                                }
                            }
                            if ( jpfile != null ) {
                                break;
                            }
                        }
                    }
                }

                textBoxID.Text = id.ToString();
                if ( jpfile != null && conv.conversation != null ) {
                    textBoxID.Text += " : " + jpfile;
                }
                textBoxSpeaker.Text = conv.speaker;
                textBoxListener.Text = conv.listener;
                textBoxConversation.Text = conv.conversation;

                status = id.ToString();
            }

            while ( node != null ) {
                if ( node.GetType() == typeof(ConversationNode) ) {
                } else {
                    if ( status.Length > 0 ) {
                        status = node.Text + " > " + status;
                    } else {
                        status = node.Text;
                    }
                }
                node = node.Parent;
            }
            setStatus( status );
        }

        void treeView_BeforeExpand( object sender, TreeViewCancelEventArgs e ) {
            TreeNode node = e.Node;

            TreeView tree = sender as TreeView;
            if ( tree == null ) {
                return;
            }

            Module module = getModule( tree.Name );
            if ( module == null ) {
                return;
            }

            // �L�����Z�����Ȃ��Ǝ��ʂ��J���� '*'�{�^���𕕈󂷂邵��
            // �S���\������Ȃ��͕̂���΂����̂����A���Ă��d���͑���������
            if ( node == module.tree.Nodes[ 0 ] ) {
                //e.Cancel = true;
            }

            if ( ( string )node.Tag == "dlg" ) {
                if ( node.Nodes.Count == 1 ) {
                    if ( ( string )node.Nodes[ 0 ].Tag == "dummy" ) {

                        // load dlg reftext
                        node.Nodes.Clear();
                        //string path = module_directory + "/" + node.Parent.Text + "/" + node.Text + ".txt";
                        string path = module.directory + "/" + node.FullPath + ".txt";

                        setStatus( "Loading: " + node.FullPath );
                        this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��

                        uint[] start = null;
                        ConversationNode[] nodes = null;


                        //Module module = module_map[];
                        loadDlg( path, ref start, ref nodes, module );

                        // error?
                        // �N���A�[���Ă����Ȃ��Ǝ��s�����ꍇ�Ƀ_�~�[���������Ⴄ����
                        if ( start == null || nodes == null ) {
                            return;
                        }

                        module.tree.SuspendLayout();

                        createDlgTree( ref start, ref nodes, ref node );

                        module.tree.ResumeLayout();
                        module.tree.PerformLayout();

                        //setStatus( "Done." );

                    }

                }
#if false
                // ����F���ďd����[
                if ( config.Color.Conv.UseCustom ) {
                    // �ċA�I�ɂ���ƂȂ� �Ƃ������ύX��̔��f�ō�����������Ȃ����ꍇ�͍���������S���ċN�ł����邩�炻�����΂����邾��
                    for ( int i = 0; i < node.Nodes.Count; ++i ) {
                        ConversationNode cn = node.Nodes[ i ] as ConversationNode;
                        if ( cn != null && cn.conversation != null ) {
                            // ����new���Ȃ��Ă����O��1���邾���ł������
                            System.Drawing.ColorConverter conv = new ColorConverter();
                            if ( i % 2 == 0 ) {
                                node.Nodes[ i ].ForeColor = ( System.Drawing.Color )conv.ConvertFromString( config.Color.Conv.Odd.ForeColor );
                                node.Nodes[ i ].BackColor = ( System.Drawing.Color )conv.ConvertFromString( config.Color.Conv.Odd.BackColor );
                            } else {
                                node.Nodes[ i ].ForeColor = ( System.Drawing.Color )conv.ConvertFromString( config.Color.Conv.Even.ForeColor );
                                node.Nodes[ i ].BackColor = ( System.Drawing.Color )conv.ConvertFromString( config.Color.Conv.Even.BackColor );
                            }
                        }
                    }
                }
#endif

                // dlg�J����x�ɖ���F���ďd����[
                for ( int i = 0; i < node.Nodes.Count; ++i ) {
                    setColor( node.Nodes[ i ], i );
                }
            }
        }

        private void menuItemExit_Click( object sender, EventArgs e ) {
            this.Close();
        }

        private void menuItemSearch_Click( object sender, EventArgs e ) {
            if ( search != null && search.Visible ) {
                search.Focus();
            } else {
                // �N���b�v�{�[�h���甽�f
                string word = System.Windows.Forms.Clipboard.GetText();
                if ( word != null ) {
                    word = word.Replace( "\r", "" );
                    word = word.Replace( "\n", "" );
                    search_option.search_word = word;
                }
                search = new SearchBox( search_option ); // ��蒼���Ȃ��ƃC���X�^���X�����Ă��j������Ă�Ƃ������₪�镜�A������ɂ́H
                search.FormClosing += new FormClosingEventHandler(search_FormClosing);
                search.ButtonSearch.Click += new EventHandler( ButtonSearch_Click );
                search.ButtonSearchPrev.Click += new EventHandler( ButtonSearchPrev_Click );
                search.TextBoxSearchWord.KeyDown += new KeyEventHandler( TextBoxSearchWord_KeyDown );
                // �ǂ������̈ʒu�ɏo�������̂����A��ʊO�ɏo��Ƌl�ނ��Ȃ���������m���鏈�������̂͌ゾ
                search.Show( this );
                // ����Location�̃Z�b�g�͕\���ザ��Ȃ��ƗL���ɂȂ�Ȃ���S�~�������邩��
            }

        }

        void search_FormClosing( object sender, FormClosingEventArgs e ) {
            search.restore( search_option );
        }

        void TextBoxSearchWord_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.KeyCode == System.Windows.Forms.Keys.Enter ) {
                ButtonSearch_Click( sender, null );
            }
        }

        void ButtonSearch_Click( object sender, EventArgs e ) {
            search.restore( search_option );
            menuItemSearchNext_Click( sender, e );
        }

        void ButtonSearchPrev_Click( object sender, EventArgs e ) {
            search.restore( search_option );
            menuItemSearchPrev_Click( sender, e );
        }


        private void menuItemSearchNext_Click( object sender, EventArgs e ) {
            if ( search_option.search_word != null && search_option.search_word.Length > 0 ) {
                TreeView tree = getSelectedTree();
                if ( tree == null ) {
                    return;
                }

                Module module = getModule( tree.Name );
                if ( module == null ) {
                    return;
                }

                setStatus( "searching: " + search_option.search_word );
                this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��
                TreeNode node = searchNext( search_option, getSelectedNode(), module );

                if ( node != null ) {
                    tree.Focus();
                    tree.SelectedNode = node;
                } else {
                    // notfound
                    setStatus( "not found: " + search_option.search_word );
                }
            }
        }

        private void menuItemSearchPrev_Click( object sender, EventArgs e ) {
            if ( search_option.search_word != null && search_option.search_word.Length > 0 ) {
                TreeView tree = getSelectedTree();
                if ( tree == null ) {
                    return;
                }

                Module module = getModule( tree.Name );
                if ( module == null ) {
                    return;
                }
                
                setStatus( "searching: " + search_option.search_word );
                this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��
                TreeNode node = searchPrev( search_option, getSelectedNode(), module );

                if ( node != null ) {
                    tree.Focus();
                    tree.SelectedNode = node;
                } else {
                    // notfound
                    setStatus( "not found: " + search_option.search_word );
                }
            }
        }

        private void menuItemCopy_Click( object sender, EventArgs e ) {
            TreeView tree = getSelectedTree();
            // tab�Ƀt�H�[�J�X�������Ă��L���ɂ���H
            if ( tree != null && tree.Focused ) {
                
                TreeNode node = getSelectedNode();

                // beforeexpand��search�ƂŎ����悤�ȃR�[�h�����邩��܂Ƃ߂����̂���
                if ( node != null ) {
                    // ���ʂ̃R�s�[
                    // conversation node�̏ꍇ�͂����Ə��Ƃ肽����

                    ConversationNode cn = node as ConversationNode;
                    string text = "";
                    if ( cn != null ) {

                        // �R�s�y
                        uint id = cn.id;
                        string jpfile = null;
                        if ( lookup != null && tabControl.SelectedTab.Name != null ) {
                            foreach ( Loockup.Module mod in lookup.module ) {
                                if ( mod.name == tabControl.SelectedTab.Name ) {
                                    foreach ( Loockup.Module.Set set in mod.set ) {
                                        if ( set.min_id <= id ) {
                                            // �I�[��0�ɂƂ肠�����A�����max_id�͊܂߂Ȃ�
                                            if ( set.max_id == 0 || id < set.max_id ) {
                                                jpfile = set.file;
                                                break;
                                            }
                                        }
                                    }
                                    if ( jpfile != null ) {
                                        break;
                                    }
                                }
                            }
                        }

                        text += "[" + cn.id + " : " + jpfile + "]";
                        text += " (" + cn.speaker + ")";
                        text += "->(" + cn.listener + ")";
                        text += "\r\n";
                        text += cn.conversation;
                        text += "\r\n";
                    } else {
                        text = node.Text;
                    }

                    System.Windows.Forms.Clipboard.SetText( text );
                }
                return;
            }
            if ( textBoxID.Focused ) {
                textBoxID.Copy();
                return;
            }
            if ( textBoxConversation.Focused ) {
                textBoxConversation.Copy();
                return;
            }
            if ( textBoxSpeaker.Focused ) {
                textBoxSpeaker.Copy();
                return;
            }
            if ( textBoxListener.Focused ) {
                textBoxListener.Copy();
                return;
            }
        }

        private void menuItemCopyAll_Click( object sender, EventArgs e ) {

            TreeView tree = getSelectedTree();
            if ( tree != null && tree.Focused ) {
                Module module = getModule( tree.Name );
                if ( module == null ) {
                    return; // gdgd
                }
                TreeNode node = getSelectedNode();
                if ( node != null ) {
                    // "rim"�Ȃ璼���̂��ł��Ȃ���S�ǂ݂��Ȃ���
                    if ( ( string )node.Tag == "rim" ) {
                        for ( int i = 0; i < node.Nodes.Count; ++i ) {
                            TreeNode dlg = node.Nodes[ i ];
                            if ( ( string )dlg.Tag == "dlg" ) {
                                if ( dlg.Nodes.Count == 1 ) {
                                    if ( ( string )dlg.Nodes[ 0 ].Tag == "dummy" ) {

                                        // load dlg reftext
                                        dlg.Nodes.Clear();
                                        //string path = module_directory + "/" + node.Parent.Text + "/" + node.Text + ".txt";
                                        string path = module.directory + "/" + dlg.FullPath + ".txt";

                                        setStatus( "Loading: " + dlg.FullPath );
                                        this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��

                                        uint[] start = null;
                                        ConversationNode[] nodes = null;


                                        //Module module = module_map[];
                                        loadDlg( path, ref start, ref nodes, module );

                                        // error?
                                        // �N���A�[���Ă����Ȃ��Ǝ��s�����ꍇ�Ƀ_�~�[���������Ⴄ����
                                        if ( start == null || nodes == null ) {
                                            return;
                                        }

                                        module.tree.SuspendLayout();

                                        createDlgTree( ref start, ref nodes, ref dlg );
                                        //node.Nodes[ i ] = dlg; // ���f����

                                        module.tree.ResumeLayout();
                                        module.tree.PerformLayout();

                                        //setStatus( "Done." );

                                    }

                                }
                            }

                        }
                    }
                    if ( ( string )node.Tag == "dlg" ) {
                        if ( node.Nodes.Count == 1 ) {
                            if ( ( string )node.Nodes[ 0 ].Tag == "dummy" ) {

                                // load dlg reftext
                                node.Nodes.Clear();
                                //string path = module_directory + "/" + node.Parent.Text + "/" + node.Text + ".txt";
                                string path = module.directory + "/" + node.FullPath + ".txt";

                                setStatus( "Loading: " + node.FullPath );
                                this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��

                                uint[] start = null;
                                ConversationNode[] nodes = null;


                                //Module module = module_map[];
                                loadDlg( path, ref start, ref nodes, module );

                                // error?
                                // �N���A�[���Ă����Ȃ��Ǝ��s�����ꍇ�Ƀ_�~�[���������Ⴄ����
                                if ( start == null || nodes == null ) {
                                    return;
                                }

                                module.tree.SuspendLayout();

                                createDlgTree( ref start, ref nodes, ref node );

                                module.tree.ResumeLayout();
                                module.tree.PerformLayout();

                                //setStatus( "Done." );

                            }

                        }
                    }
                    setStatus( "Copy..." );
                    statusStrip.Refresh();

                    // �ċN�ŃR���o�C��
                    string text = "";
                    convineText( ref text, node, -1 );

                    //System.Console.WriteLine( text );
                    System.Windows.Forms.Clipboard.SetText( text );

                    setStatus( "Done." );

                }
            }
        }
        private void menuItemCopyTabAll_Click( object sender, EventArgs e ) {

            TreeView tree = getSelectedTree();
            if ( tree != null && tree.Focused ) {
                Module module = getModule( tree.Name );
                if ( module == null ) {
                    return; // gdgd
                }
                TreeNode node = getSelectedNode();
                if ( node != null ) {
                    // "rim"�Ȃ璼���̂��ł��Ȃ���S�ǂ݂��Ȃ���
                    if ( ( string )node.Tag == "rim" ) {
                        for ( int i = 0; i < node.Nodes.Count; ++i ) {
                            TreeNode dlg = node.Nodes[ i ];
                            if ( ( string )dlg.Tag == "dlg" ) {
                                if ( dlg.Nodes.Count == 1 ) {
                                    if ( ( string )dlg.Nodes[ 0 ].Tag == "dummy" ) {

                                        // load dlg reftext
                                        dlg.Nodes.Clear();
                                        //string path = module_directory + "/" + node.Parent.Text + "/" + node.Text + ".txt";
                                        string path = module.directory + "/" + dlg.FullPath + ".txt";

                                        setStatus( "Loading: " + dlg.FullPath );
                                        this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��

                                        uint[] start = null;
                                        ConversationNode[] nodes = null;


                                        //Module module = module_map[];
                                        loadDlg( path, ref start, ref nodes, module );

                                        // error?
                                        // �N���A�[���Ă����Ȃ��Ǝ��s�����ꍇ�Ƀ_�~�[���������Ⴄ����
                                        if ( start == null || nodes == null ) {
                                            return;
                                        }

                                        module.tree.SuspendLayout();

                                        createDlgTree( ref start, ref nodes, ref dlg );
                                        //node.Nodes[ i ] = dlg; // ���f����

                                        module.tree.ResumeLayout();
                                        module.tree.PerformLayout();

                                        //setStatus( "Done." );

                                    }

                                }
                            }

                        }
                    }
                    if ( ( string )node.Tag == "dlg" ) {
                        if ( node.Nodes.Count == 1 ) {
                            if ( ( string )node.Nodes[ 0 ].Tag == "dummy" ) {

                                // load dlg reftext
                                node.Nodes.Clear();
                                //string path = module_directory + "/" + node.Parent.Text + "/" + node.Text + ".txt";
                                string path = module.directory + "/" + node.FullPath + ".txt";

                                setStatus( "Loading: " + node.FullPath );
                                this.Update(); // update���Ȃ��Ə�̂����f����Ȃ� ��肷����Əd���͂��Ȃ̂Ńs���|�C���g��

                                uint[] start = null;
                                ConversationNode[] nodes = null;


                                //Module module = module_map[];
                                loadDlg( path, ref start, ref nodes, module );

                                // error?
                                // �N���A�[���Ă����Ȃ��Ǝ��s�����ꍇ�Ƀ_�~�[���������Ⴄ����
                                if ( start == null || nodes == null ) {
                                    return;
                                }

                                module.tree.SuspendLayout();

                                createDlgTree( ref start, ref nodes, ref node );

                                module.tree.ResumeLayout();
                                module.tree.PerformLayout();

                                //setStatus( "Done." );

                            }

                        }
                    }

                    setStatus( "Copy..." );
                    statusStrip.Refresh();

                    // �ċN�ŃR���o�C��
                    string text = "";
                    convineText( ref text, node, node.Level );

                    //System.Console.WriteLine( text );
                    System.Windows.Forms.Clipboard.SetText( text );
                    setStatus( "Done." );

                }
            }
        }

        // �ړ������悤
        void convineText( ref string text, TreeNode node, int level ) {
            // �ŏ��̃��x����-1�Ȃ�C���f���g����
            // ����Ȃ炻�����Ƃ���
            string tab = "";
            if ( level > -1 ) {
                for ( int i = 0; i < node.Level - level; ++i ) {
                    tab += "\t";
                }
            }

            ConversationNode cn = node as ConversationNode;
            text += tab;
            if ( cn != null ) {

                // �R�s�y
                uint id = cn.id; // �R�s�[���Ɩ�薳���̂��[�H��������node���������ꍇ�͒l������Ȃ����
                string jpfile = null;
                if ( lookup != null && tabControl.SelectedTab.Name != null ) {
                    foreach ( Loockup.Module mod in lookup.module ) {
                        if ( mod.name == tabControl.SelectedTab.Name ) {
                            foreach ( Loockup.Module.Set set in mod.set ) {
                                if ( set.min_id <= id ) {
                                    // �I�[��0�ɂƂ肠�����A�����max_id�͊܂߂Ȃ�
                                    if ( set.max_id == 0 || id < set.max_id ) {
                                        jpfile = set.file;
                                        break;
                                    }
                                }
                            }
                            if ( jpfile != null ) {
                                break;
                            }
                        }
                    }
                }

                text += "[" + cn.id + " : " + jpfile + "]";
                text += " (" + cn.speaker + ")";
                text += "->(" + cn.listener + ")";
                text += "\r\n";
                text += tab;
                text += cn.conversation;
            } else {
                text += node.Text;
            }

            text += "\r\n";
            
            for( int i = 0; i < node.Nodes.Count; ++i ) {
                convineText( ref text, node.Nodes[i], level );
            }
            //text += "\r\n";
        }

        private void menuItemExpandAll_Click( object sender, EventArgs e ) {
            TreeNode node = getSelectedNode();
            if ( node != null ) {
                node.ExpandAll();
            }
        }

        private void menuItemExpand_Click( object sender, EventArgs e ) {
            TreeNode node = getSelectedNode();
            if ( node != null ) {
                node.Expand();
            }
        }

        private void menuItemCollapseAll_Click( object sender, EventArgs e ) {
            TreeNode node = getSelectedNode();
            if ( node != null ) {
                node.Collapse();
            }
        }

        private void menuItemCollapse_Click( object sender, EventArgs e ) {
            TreeNode node = getSelectedNode();
            if ( node != null ) {
                node.Collapse(true);
            }
        }

        private void menuItemVersion_Click( object sender, EventArgs e ) {
            About about = new About( version );
            about.ShowDialog( this );
        }

        private void menuItemSetting_Click( object sender, EventArgs e ) {
            Setting.Config temp = config.clone();
            Setting setting = new Setting( temp );
            DialogResult result = setting.ShowDialog( this );
            if ( result == DialogResult.OK ) {
                setting.restore( ref config );
                xml.Xml.Write<Setting.Config>( config_path, config );

                // �f�t�H���g�J���[�ɖ߂��ɂ̓m�[�h����ĂȂ��ւ��Ƃ����Ȃ��Ƒʖڂ����Ȃ̂ōċN�������ł�����
                //setColor();
            }
        }

        void setColor() {

            ColorConverter conv = new ColorConverter();
            // textbox
            if ( config.Color.Base.UseCustom ) {
                textBoxID.ForeColor = ( Color )conv.ConvertFromString( config.Color.Base.ForeColor );
                textBoxID.BackColor = ( Color )conv.ConvertFromString( config.Color.Base.BackColor );
                textBoxSpeaker.ForeColor = textBoxID.ForeColor;
                textBoxSpeaker.BackColor = textBoxID.BackColor;
                textBoxListener.ForeColor = textBoxID.ForeColor;
                textBoxListener.BackColor = textBoxID.BackColor;
                textBoxConversation.ForeColor = textBoxID.ForeColor;
                textBoxConversation.BackColor = textBoxID.BackColor;
            }

            foreach ( Module mod in module_map.Values ) {
                if ( config.Color.Base.UseCustom ) {
                    mod.tree.ForeColor = ( Color )conv.ConvertFromString( config.Color.Base.ForeColor );
                    mod.tree.BackColor = ( Color )conv.ConvertFromString( config.Color.Base.BackColor );
                }
                for ( int i = 0; i < mod.tree.Nodes.Count; ++i ) {
                    setColor( mod.tree.Nodes[ i ], i );
                }
            }
        }
        // ������ colorconverter��member�ŗp�ӂ��Ă����Ă����͂������悤��
        // �Ƃ������ϊ��ς݃J���[��
        void setColor( TreeNode node, int index ) {
            ColorConverter conv = new ColorConverter();
            if ( config.Color.Base.UseCustom ) {
                node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Base.ForeColor );
                node.BackColor = ( Color )conv.ConvertFromString( config.Color.Base.BackColor );
            }
            ConversationNode n = node as ConversationNode;
            if ( n != null ) {
                if ( index % 2 == 0 ) {
                    if ( config.Color.Conv.Odd.UseCustom ) {
                        node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Conv.Odd.ForeColor );
                        node.BackColor = ( Color )conv.ConvertFromString( config.Color.Conv.Odd.BackColor );
                    }
                    if ( n.conversation == null && config.Color.Empty.Odd.UseCustom ) {
                        node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Empty.Odd.ForeColor );
                        node.BackColor = ( Color )conv.ConvertFromString( config.Color.Empty.Odd.BackColor );
                    }
                } else {
                    if ( config.Color.Conv.Even.UseCustom ) {
                        node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Conv.Even.ForeColor );
                        node.BackColor = ( Color )conv.ConvertFromString( config.Color.Conv.Even.BackColor );
                    }
                    if ( n.conversation == null && config.Color.Empty.Odd.UseCustom ) {
                        node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Empty.Even.ForeColor );
                        node.BackColor = ( Color )conv.ConvertFromString( config.Color.Empty.Even.BackColor );
                    }
                }
            } else {
                string tag = node.Tag as string;
                switch ( tag ) {
                    case "rim":
                        if ( index % 2 == 0 ) {
                            if ( config.Color.Rim.Odd.UseCustom ) {
                                node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Rim.Odd.ForeColor );
                                node.BackColor = ( Color )conv.ConvertFromString( config.Color.Rim.Odd.BackColor );
                            }
                        } else {
                            if ( config.Color.Rim.Even.UseCustom ) {
                                node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Rim.Even.ForeColor );
                                node.BackColor = ( Color )conv.ConvertFromString( config.Color.Rim.Even.BackColor );
                            }
                        }
                        break;
                    case "dlg":
                        if ( index % 2 == 0 ) {
                            if ( config.Color.Dlg.Odd.UseCustom ) {
                                node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Dlg.Odd.ForeColor );
                                node.BackColor = ( Color )conv.ConvertFromString( config.Color.Dlg.Odd.BackColor );
                            }
                        } else {
                            if ( config.Color.Dlg.Even.UseCustom ) {
                                node.ForeColor = ( Color )conv.ConvertFromString( config.Color.Dlg.Even.ForeColor );
                                node.BackColor = ( Color )conv.ConvertFromString( config.Color.Dlg.Even.BackColor );
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            // �ċA
            for ( int i = 0; i < node.Nodes.Count; ++i ) {
                // �Ȃ��Ă��͂����Ă����͂�����
                if ( ( string )node.Nodes[ i ].Tag != "dummy" ) {
                    setColor( node.Nodes[ i ], i );
                }
            }
        }


    }
}
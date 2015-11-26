using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cmmInterpreter.Process
{
    class Grammar//语法分析，使用递归下降子程序法
    {
        private int n = 0;                                                          //当前Token指针
        private List<string> symTable = new List<string>();		                    //Token类型表
        private Dictionary<string, ID> idTable = new Dictionary<string, ID>();      //变量表
        public int errNum = 0;                                                      //错误个数
        private string errStr = "";                                                 //错误输出
        public TreeView gTree = null;
        private TreeNode begin = null;
        private List<MidCode> instruArr = new List<MidCode>();                      //存放中间代码

        public void grammarAnalyze()
        {
            init();
            /*
            resultStr += "----------------------------";
            resultStr += " 开始分析 ";
            resultStr += "----------------------------"; 
            resultStr += "\r\n";
            */
            Program();
            Match((int)Symbol.EOF, ref begin);
        }
        private void init()
        {
            //进行清理，以免之前就进行过语法分析，消除干扰
            n = 0;
            errNum = 0;
            errStr = "";

            symTable.Clear();
            idTable.Clear();

            symTable.Add("int");
            symTable.Add("real");
            symTable.Add("void");
            symTable.Add("if");
            symTable.Add("else");
            symTable.Add("while");
            symTable.Add("for");
            symTable.Add("read");
            symTable.Add("write");
            symTable.Add("return");
            symTable.Add("++");
            symTable.Add("--");
            symTable.Add("标识符");
            symTable.Add("整数");
            symTable.Add("小数");
            symTable.Add("+");
            symTable.Add("-");
            symTable.Add("*");
            symTable.Add("/");
            symTable.Add("=");
            symTable.Add("==");
            symTable.Add("!=");
            symTable.Add(">");
            symTable.Add(">=");
            symTable.Add("<");
            symTable.Add("<=");
            symTable.Add(",");
            symTable.Add(";");
            symTable.Add("(");
            symTable.Add(")");
            symTable.Add("{");
            symTable.Add("}");
            symTable.Add("[");
            symTable.Add("]");
            symTable.Add("EOF");

            //初始化语法树
            gTree = new TreeView();
            gTree.Dock = DockStyle.Fill;
            begin = new TreeNode("语法树");
            gTree.Nodes.Add(begin);
        }
        private void Program()
        {
            int type = Global.tokenArr[n].type;
            if (isFunDclr(type))
            {
                FunDclr(ref begin);
                Program();
            }
            else if (isBlock(type))
            {
                Block(ref begin);
                Program();
            }
            else if (type == (int)Symbol.EOF)
                return;
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 文件遇到意外的文件结尾\r\n";
            }
        }

        private bool isFunDclr(int type)
        {
            if (type == (int)Symbol.VOID || type == (int)Symbol.INT || type == (int)Symbol.REAL)
                if (n + 2 < Global.tokenArr.Count && Global.tokenArr[n + 2].type == (int)Symbol.LPAREN)
                    return true;
            return false;
        }
        private void FunDclr(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "函数声明");

            int type; //用于与返回值类型进行比较
            if (Global.tokenArr[n].type == (int)Symbol.INT)
                type = (int)Symbol.INTEGER;
            else if (Global.tokenArr[n].type == (int)Symbol.REAL)
                type = (int)Symbol.REALNUM;
            else
                type = (int)Symbol.VOID;
            Type(ref tn);

            //这里不用判别是不是id就可以加入符号表，因为已经符合FunDclr的要求了（通过了isFunDclr）
            int record = n;
            NewID(Global.tokenArr[n - 1].type, ref tn);//参数为函数返回值类型
            idTable[Global.tokenArr[record].src].vcodeAssignLine = Global.tokenArr[record].lineNum;

            //保存现场
            Dictionary<string, ID> tempTable = new Dictionary<string, ID>(idTable, null);//用于暂存符号表
            ID.RefreshCount(0-ID.GetCount());
            ClearCase();

            TreeNode tn2 = NewTreeNode(ref tn, "参数列表");
            Match((int)Symbol.LPAREN, ref tn2);
            VarListDclr(ref tn2);
            idTable[Global.tokenArr[record].src].numOfVar = ID.GetCount();//获取现在临时id表中的变量个数，即新加入的变量个数
            Match((int)Symbol.RPAREN, ref tn2);
              
            int returnType = FunBody(ref tn);
            if ((type == (int)Symbol.REALNUM && returnType != (int)Symbol.REALNUM && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.INTEGER && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.VOID && returnType != (int)Symbol.VOID))
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : 函数返回值类型与实际返回值类型不匹配\r\n";
            }
              
            //恢复现场
            idTable[Global.tokenArr[record].src].isDefined = true;//已声明
            idTable = tempTable;//还原符号表
            ID.RefreshCount(tempTable.Count());
        }
        private void ClearCase()//去除不可使用的变量，即外层变量
        {
            string[] temp = idTable.Keys.ToArray();
            for (int i = 0; i < temp.Count(); i++ )
            {
                if (idTable[temp[i]].vcodeAssignLine == -1)
                    idTable.Remove(temp[i]);
            }
        }
        private void VarListDclr(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.INT || type == (int)Symbol.REAL)
            {
                TreeNode tn = NewTreeNode(ref root, "参数声明");
                Type(ref tn);
                Token record = Global.tokenArr[n];
                NewID(type, ref tn);
                idTable[record.src].isDefined = true;//已声明

                VarListDclr_(ref root);
            }
            //else // ε
            if (Global.tokenArr[n].type != (int)Symbol.RPAREN)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 形参列表错误\r\n";
                //容错处理，跳过整个形参列表
                while (Global.tokenArr[n].type != (int)Symbol.RPAREN)
                    ++n;
            }
        }
        private void VarListDclr_(ref TreeNode root)
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA, ref root);
                VarListDclr(ref root);
            }
            //else // ε 
        }
        private int FunBody(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "函数体");

            Match((int)Symbol.LBRACE, ref tn);
            SubBlock(ref tn);

            int returnType = Return(ref tn);
            Match((int)Symbol.RBRACE, ref tn);

            return returnType;
        }
        private int Return(ref TreeNode root)//匹配return语句并返回 返回值类型
        {
            TreeNode tn = NewTreeNode(ref root, "返回语句");
            Match((int)Symbol.RETURN, ref tn);

            int type = Global.tokenArr[n].type;
            if (isExpr(type))
            {
                Expr(ref tn);
                Match((int)Symbol.SEMICOLON, ref tn);
                return (int)Symbol.INTEGER;
            }
            else if (type == (int)Symbol.SEMICOLON)
            {
                Empty(ref tn);
                return (int)Symbol.VOID;
            }
            else//return的后跟符号错误
            {
                //容错处理，在10个字符以内出现分号，则可不影响后续分析
                int count = n;
                while (count - n < 10 && count < Global.tokenArr.Count()-1)
                {
                    if (Global.tokenArr[count].type == (int)Symbol.SEMICOLON)
                        break;
                    ++count;
                }
                if (count - n < 10)//若在10个字符以内找到了分号，则将其匹配
                {
                    n = count;
                    Match((int)Symbol.SEMICOLON, ref tn);
                }
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : return表达式错误\r\n";
                return (int)Symbol.SEMICOLON;
            }
        }

        private bool isBlock(int type)
        {
            return type == (int)Symbol.LBRACE || isStat(type);
        }
        private void Block(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            
            if (type == (int)Symbol.LBRACE)
            {
                TreeNode tn = NewTreeNode(ref root, "代码块");

                Match((int)Symbol.LBRACE, ref tn);
                SubBlock(ref tn);
                Match((int)Symbol.RBRACE, ref tn);
            }
            else if (isStat(type))
                Stat(ref root);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
        }
        private bool isSubBlock(int type)
        {
            return isStat(type);
        }
        private void SubBlock(ref TreeNode root)
        {
            if (isStat(Global.tokenArr[n].type))//这里要判断一次，因为SubBlock在没有判断的时候也调用了
            {
                Stat(ref root);
                SubBlock(ref root);
            }
            //else // ε
        }

        private bool isStat(int type)
        {
            return type == (int)Symbol.READ || type == (int)Symbol.WRITE || type == (int)Symbol.WHILE || type == (int)Symbol.FOR || type == (int)Symbol.IF || type == (int)Symbol.IDENT || type == (int)Symbol.VOID || type == (int)Symbol.REAL || type == (int)Symbol.INT || type == (int)Symbol.SEMICOLON || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Stat(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "基本语句");
            int type = Global.tokenArr[n].type;

            //Statement	--->	 Read | Write | While | For | If | FunCall | VarDclr | Assign | Empty | Factor_2
            if (type == (int)Symbol.READ)
                Read(ref tn);
            else if (type == (int)Symbol.WRITE)
                Write(ref tn);
            else if (type == (int)Symbol.WHILE)
                While(ref tn);
            else if (type == (int)Symbol.FOR)
                For(ref tn);
            else if (type == (int)Symbol.IF)
                If(ref tn);
            else if (type == (int)Symbol.VOID || type == (int)Symbol.REAL || type == (int)Symbol.INT)
                VarDclr(ref tn);
            else if (type == (int)Symbol.SEMICOLON)
                Empty(ref tn);
            else if (type == (int)Symbol.IDENT)
            {
                if (Global.tokenArr[n + 1].type == (int)Symbol.LPAREN)
                    FunCall(ref tn);
                else
                {
                    Assign(ref tn);//返回值得改一下， 不然；无法同一层
                    Match((int)Symbol.SEMICOLON, ref tn);//这里不能传tn
                }
            }
            else if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)
            {
                Factor_2(ref tn);
                Match((int)Symbol.SEMICOLON, ref tn);
            }
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
        }
        
        private void Read(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "Read语句");

            Match((int)Symbol.READ, ref tn);

            string src = Global.tokenArr[n].src;
            if (idTable.ContainsKey(src))//在变量表里
            {
                if (isDclred(src) && isCommonID(src))
                {
                    Id(ref tn);
                    idTable[src].isDefined = true;
                }
                else if (isDclred(src) && isArrayID(src))   //变量已声明，为数组id
                {
                    TreeNode tn2 = NewTreeNode(ref tn, "数组元素");
                    Id(ref tn);
                    int type = Global.tokenArr[n].type;
                    if (type == (int)Symbol.LBRAKET)//为数组下标
                    {
                        int index = idTable[src].lenght;
                        ArrIndex(index, ref tn2);
                    }//else // ε
                    idTable[src].isDefined = true;
                }
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 此处需要一个普通变量或数组元素\r\n";
                ++n;//改为10个以内找到；的
            }
            Match((int)Symbol.SEMICOLON, ref tn);
        }
        private void Write(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "Write语句");
            Match((int)Symbol.WRITE, ref tn);
            if (isExpr(Global.tokenArr[n].type))
                Expr(ref tn);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 此处需要一个普通变量或数组元素\r\n";
                ++n;//改为10个以内找到；的
            }
            Match((int)Symbol.SEMICOLON, ref tn);
        }
        private void While(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "While语句");
            Match((int)Symbol.WHILE, ref tn);

            TreeNode tn2 = NewTreeNode(ref tn, "条件判断");
            Match((int)Symbol.LPAREN,ref tn2);
            BoolExpr(ref tn2);
            Match((int)Symbol.RPAREN, ref tn2);

            Block(ref tn);
        }
        private void For(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "For语句");
            Match((int)Symbol.FOR, ref tn);

            TreeNode tn2 = NewTreeNode(ref tn, "For循环条件");
            Match((int)Symbol.LPAREN, ref tn2);
            TreeNode tn2_1 = NewTreeNode(ref tn2, "第一循环条件");
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.IDENT)
            {
                Assign(ref tn2_1);
                Match((int)Symbol.SEMICOLON, ref tn2_1);
            }
            else if (type == (int)Symbol.SEMICOLON)
                Empty(ref tn2_1);
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : for循环的第一个条件错误\r\n";
                //容错处理，在10个字符以内出现分号，则可不影响后续分析
                int count = n;
                while (count - n < 10 && count < Global.tokenArr.Count()-1)
                {
                    if (Global.tokenArr[count].type == (int)Symbol.SEMICOLON)
                        break;
                    ++count;
                }
                if (count - n < 10)//若在10个字符以内找到了分号，则将其匹配
                {
                    n = count;
                    Match((int)Symbol.SEMICOLON, ref tn2);
                }
            }

            TreeNode tn2_2 = NewTreeNode(ref tn2, "第二循环条件");
            BoolExpr(ref tn2_2);
            Match((int)Symbol.SEMICOLON, ref tn2);

            TreeNode tn2_3 = NewTreeNode(ref tn2, "第三循环条件");
            if (Global.tokenArr[n].type == (int)Symbol.IDENT)
                Assign(ref tn2_3);
            else if (Global.tokenArr[n].type == (int)Symbol.RPAREN) ;//kong
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : for循环的第三条件错误\r\n";
                //容错处理，在10个字符以内出现右括号，则可不影响后续分析
                int count = n;
                while (count - n < 10 && count < Global.tokenArr.Count() - 1)
                {
                    if (Global.tokenArr[count].type == (int)Symbol.SEMICOLON)
                        break;
                    ++count;
                }
                if (count - n < 10)//若在10个字符以内找到了右括号，忽略之前的字符
                    n = count;
            }
            Match((int)Symbol.RPAREN, ref tn2);
            Block(ref tn);
        }
        private void If(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "If语句");
            Match((int)Symbol.IF, ref tn);

            TreeNode tn2 = NewTreeNode(ref tn, "条件判断");
            Match((int)Symbol.LPAREN, ref tn2);
            BoolExpr(ref tn2);
            Match((int)Symbol.RPAREN, ref tn2);

            Block(ref tn);
            If_(ref tn);
        }
        private void If_(ref TreeNode root)
        {
            if (Global.tokenArr[n].type == (int)Symbol.ELSE)
            {
                Match((int)Symbol.ELSE, ref root);
                Block(ref root);
            }
            //else // ε
        }

        //FunCall	--->		Id ‘(’ VarList ‘)’ ‘;’
        private void FunCall(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "函数调用语句");

            int record = n;
            string src = Global.tokenArr[record].src;
            if (isDclred(src) && isFunID(src))
            {
                Id(ref tn);
                int num = idTable[src].numOfVar;//参数个数
                FunCallBody(num, ref tn);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 非函数标识符不可被调用\r\n";
            }
            Match((int)Symbol.SEMICOLON, ref tn);
        }
        private void FunCallBody(int varNum, ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "参数列表");

            int realNum = 0;
            Match((int)Symbol.LPAREN, ref tn);
            realNum += VarList(ref tn);
            Match((int)Symbol.RPAREN, ref tn);

            if (realNum != varNum)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 函数调用形参与实参个数不相等\r\n";
            }
        }
        private int VarList(ref TreeNode root)
        {
            int realNum = 0;
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.IDENT || isNum(type))
            {
                TreeNode tn = NewTreeNode(ref root, "参数");

                realNum += 1;
                if (type == (int)Symbol.IDENT)
                    Id(ref tn);
                else if (isNum(type))
                    Num(ref tn);

                realNum += VarList_(ref root);
            }
            //else // ε 
            return realNum;
        }
        private int VarList_(ref TreeNode root)
        {
            int realNum = 0;
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA, ref root);
                realNum += VarList(ref root);
            }
            //else // ε 
            return realNum;
        }

        private void VarDclr(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "变量声明语句");

            int type = Global.tokenArr[n].type;
            Type(ref tn);
            NewID(type, ref tn);

            VarDclr_(type, ref tn);
            Match((int)Symbol.SEMICOLON, ref tn);
        }
        private void VarDclr_(int kind, ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.SEMICOLON)          //ε,结束声明变量
            {
                return;
            }  
            else
            {
                if (type == (int)Symbol.BECOMES)        //普通变量声明并赋值
                {
                    int record = n - 1;//变量位置
                    TreeNode tn = NewTreeNode(ref root, "普通变量初始化");
                    Match((int)Symbol.BECOMES, ref tn);

                    //这里是一个表达式，或者是一个负值...
                    if (isExpr(Global.tokenArr[n].type))
                        Expr(ref tn);
                    else
                    {
                        Match((int)Symbol.MINUS, ref tn);
                        Num(ref tn);
                    }
                    idTable[Global.tokenArr[record].src].isDefined = true;//已声明
                }
                else if (type == (int)Symbol.LBRAKET)   //数组变量声明(与赋值)
                {
                    TreeNode tn = NewTreeNode(ref root, "数组下标");

                    Match((int)Symbol.LBRAKET,ref tn);
                    if (Global.tokenArr[n].type == (int)Symbol.INTEGER)
                    {
                        int x = int.Parse(Global.tokenArr[n].src);          //不会有异常，因为肯定是integer类型了
                        idTable[Global.tokenArr[n - 2].src].lenght = x;     //数组长度+1，最后记得减回来啊...
                        ID.RefreshCount(x - 1);                             //之前放Id的时候已经放过1长度了,在HandleId里
                        Match((int)Symbol.INTEGER, ref tn);
                    }
                    else
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : 下标类型不为整型\r\n";
                    }
                    Match((int)Symbol.RBRAKET, ref tn);

                    VarDclr_2(idTable[Global.tokenArr[n - 4].src].lenght, ref tn);
                }
                else if (type == (int)Symbol.COMMA) ;    //继续声明变量
                else
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
                }

                VarDclr_3(kind, ref root);
            }
        }
        private void VarDclr_2(int length, ref TreeNode root)//数组赋值
        {
            if (Global.tokenArr[n].type == (int)Symbol.BECOMES)
            {
                int record = n - 4;
                TreeNode tn = NewTreeNode(ref root, "数组变量初始化");
          
                Match((int)Symbol.BECOMES,ref tn);

                TreeNode tn2 = new TreeNode("初始化列表");
                tn.Nodes.Add(tn2);
                Match((int)Symbol.LBRACE, ref tn2);
                int realLen = 0;
                realLen += NumList(ref tn2);
                Match((int)Symbol.RBRACE, ref tn2);
                idTable[Global.tokenArr[record].src].isDefined = true;//已声明

                if (realLen > length)//初始化元素个数越界
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 数组初始化元素个数越界\r\n";
                }
            }
            //else // ε
        }
        private void VarDclr_3(int kind, ref TreeNode root)//继续声明
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA, ref root);
                NewID(kind, ref root);
                VarDclr_(kind, ref root);
            }
            //else // ε
        }
        private void NewID(int kind, ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "声明变量");
            HandleId(Global.tokenArr[n], kind);
            Id(ref tn);
        }

        private int NumList(ref TreeNode root)
        {
            int len = 0; 

            if (isNum(Global.tokenArr[n].type))
            {
                TreeNode tn = NewTreeNode(ref root, "初始化参数");
                len += 1;
                Num(ref tn);
                len += NumList_(ref root);
            }
            //else // ε
            return len;
        }
        private int NumList_(ref TreeNode root)
        {
            int len = 0;
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                len += 1;
                Match((int)Symbol.COMMA, ref root);
                TreeNode tn = NewTreeNode(ref root, "初始化参数");
                Num(ref tn);
                len += NumList_(ref root);
            }
            //else // ε
            return len;
        }

        private void Assign(ref TreeNode root)//还要考虑数组的赋值啊...
        {
            TreeNode tn = NewTreeNode(ref root, "赋值语句");

            string src = Global.tokenArr[n].src;
            if(Global.tokenArr[n].type == (int)Symbol.IDENT && isDclred(src))
            {
                if (isCommonID(src))
                    Id(ref tn);
                else if (isArrayID(src))
                {
                    Id(ref tn);
                    SupID_(src, ref tn);
                }
                else
                {
                    ++n;//跳过这个错误
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : '" + src + "' 左值错误，不能为普通标识符或数组标识符之外的标识符\r\n";
                }
            }

            Match((int)Symbol.BECOMES, ref tn);
            //这里是一个表达式，或者是一个负值...
            if (isExpr(Global.tokenArr[n].type))
                Expr(ref tn);
            else
            {
                Match((int)Symbol.MINUS, ref tn);
                Num(ref tn);
            }
            if (idTable.ContainsKey(src))
                idTable[src].isDefined = true;
        }

        private void Empty(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "空语句");
            Match((int)Symbol.SEMICOLON, ref root);
        }

        private void BoolExpr(ref TreeNode root)
        {
            if (isExpr(Global.tokenArr[n].type))
            {
                Expr(ref root);
                RelOp(ref root);
                Expr(ref root);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 需要一个布尔表达式\r\n";
            }
        }
        private bool isExpr(int type)//Expr的first集
        {
            return type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Expr(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "表达式语句");

            Item(ref tn);
            Expr_(ref tn);
        }
        private void Expr_(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.PLUS || type == (int)Symbol.MINUS)
            {
                TreeNode tn = NewTreeNode(ref root, "运算符");
                if (type == (int)Symbol.PLUS)
                    Match((int)Symbol.PLUS, ref tn);
                else
                    Match((int)Symbol.MINUS, ref tn);

                Item(ref root);
                Expr_(ref root);
            }
            //else // ε
        }
        private void Item(ref TreeNode root)
        {
            Factor(ref root);
            Item_(ref root);
        }
        private void Item_(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.TIMES || type == (int)Symbol.DIV)
            {
                TreeNode tn = NewTreeNode(ref root, "运算符");
                if (type == (int)Symbol.TIMES)
                    Match((int)Symbol.TIMES, ref tn);
                else
                    Match((int)Symbol.DIV, ref tn);

                Factor(ref root);
                Item_(ref root);
            }
            //else // ε
        }
        private void Factor(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "表达式子项");

            int type = Global.tokenArr[n].type;
            if (isNum(type) || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN)//Factor_的first集
                Factor_(ref tn);
            else if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)//Factor_2的first集
                Factor_2(ref tn);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
        }
        private void Factor_(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (isNum(type))
            {
                TreeNode tn = NewTreeNode(ref root, "数字");
                Num(ref tn);
            }
            else if (type == (int)Symbol.IDENT)
            {
                string src = n > 0 ? Global.tokenArr[n].src : "_";//标识符token的值，用_来作标志值，因为如果为这个肯定过不了词法分析
                Id(ref root);//改，返回自己的node
                if(isDefined(src))
                    SupID_(src, ref root);//改
            }
            else if (type == (int)Symbol.LPAREN)
            {
                Match((int)Symbol.LPAREN,ref root);
                Expr(ref root);
                Match((int)Symbol.RPAREN, ref root);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
        }
        private void Factor_2(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "表达式子项");

            int num = 0;
            int n_record  = n;
            int type = Global.tokenArr[n].type;
            
            int splus_record = n;
            if (type == (int)Symbol.SPLUS)
                Match((int)Symbol.SPLUS, ref root);
            else //type == (int)Symbol.SMINUS
                Match((int)Symbol.SMINUS, ref root);

            string src = Global.tokenArr[n].src;
            if (isDefined(src))
            {
                if (isCommonID(src))
                    Id(ref tn);
                else if (isArrayID(src))
                {
                    Id(ref tn);
                    int index = idTable[src].lenght;
                    ArrIndex(index, ref tn);
                }
                else
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 需要一个可变左值\r\n";
                }
            }

            Token t = new Token(num.ToString(), (int)Symbol.INTEGER, Global.tokenArr[n_record].lineNum);
        }
        private void SupID_(string src,ref TreeNode root)
        {
            if (isDclred(src))
            {
                if (isArrayID(src))     //为数组id
                {
                    int index = idTable[src].lenght;
                    ArrIndex(index, ref root);
                }
                else if (isFunID(src))  //为函数id
                {
                    TreeNode tn = NewTreeNode(ref root, "函数调用返回值");
                    int num = idTable[src].numOfVar;//参数个数
                    FunCallBody(num, ref root);
                }
                else                  //ε，为普通id，没有后缀
                {
                    TreeNode tn = NewTreeNode(ref root, "普通变量");
                    if (Global.tokenArr[n].type == (int)Symbol.LBRAKET)
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : 非数组标识符不可使用数组下标\r\n";
                    }
                    else if (Global.tokenArr[n].type == (int)Symbol.LPAREN)
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : 非函数标识符不可被调用\r\n";
                    }
                }
            }
        }
        private void ArrIndex(int totalNum,ref TreeNode root)//数组下标判别
        {
            TreeNode tn = NewTreeNode(ref root, "数组下标");
            Match((int)Symbol.LBRAKET, ref tn);

            int newtype = Global.tokenArr[n].type;  //下标类型
            string src = Global.tokenArr[n].src;    //下标的值，为数值或id名

            if (newtype == (int)Symbol.INTEGER && int.Parse(src) >= 0)  //下标为正整数
            {
                TreeNode tn2 = new TreeNode("正整数");
                tn.Nodes.Add(tn2);
                Match((int)Symbol.INTEGER,ref tn2);
                if (int.Parse(src) >= totalNum)
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 数组下标越界\r\n";
                }
            }
            else if (idTable[src].type == (int)Symbol.INT && isDefined(src) && isCommonID(src))     //下标为标识符 
            {
                TreeNode tn2 = new TreeNode("整形变量");
                tn.Nodes.Add(tn2);
                Id(ref tn2);       //这个下标必须为整形变量，现在还不能检测出这是不是一个正整数，运行时检测  
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 下标错误 或 参数错误\r\n";
                ++n;//改，10个以内找到】
            }

            Match((int)Symbol.RBRAKET,ref tn);
        }
        private void RelOp(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "关系运算符");
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.GTR)
                Match((int)Symbol.GTR, ref tn);
            else if (type == (int)Symbol.GEQ)
                Match((int)Symbol.GEQ, ref tn);
            else if (type == (int)Symbol.LES)
                Match((int)Symbol.LES, ref tn);
            else if (type == (int)Symbol.LEQ)
                Match((int)Symbol.LEQ, ref tn);
            else if (type == (int)Symbol.EQL)
                Match((int)Symbol.EQL, ref tn);
            else if (type == (int)Symbol.NEQ)
                Match((int)Symbol.NEQ, ref tn);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 需要一个关系运算符\r\n"; 
            }
        }

        private void Type(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "类型声明");
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.VOID)
                Match((int)Symbol.VOID, ref tn);
            else if(type == (int)Symbol.INT)
                Match((int)Symbol.INT, ref tn);
            else if (type == (int)Symbol.REAL)
                Match((int)Symbol.REAL, ref tn);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 类型错误\r\n";
            }
        }

        private bool isNum(int type)
        {
            if (type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM)
                return true;
            return false;
        }
        private void Num(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.INTEGER)
                Match((int)Symbol.INTEGER, ref root);
            else if (type == (int)Symbol.REALNUM)
                Match((int)Symbol.REALNUM, ref root);
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 此处需要一个数字\r\n";
                ++n;
            }
        }

        private void Id(ref TreeNode root)//判别是否声明并吞入
        {
            TreeNode tn = NewTreeNode(ref root, "标识符");
            string src = Global.tokenArr[n].src;
            if (isDclred(src))
                Match((int)Symbol.IDENT, ref tn);
        }
        private bool isFunID(string src)
        {
            //参数个数不为-1即为函数标识符
            return idTable[src].numOfVar != -1;
        }
        private bool isArrayID(string src)
        {
            //数组长度在赋值的时候+1了，不可能是1，至少是2，最后记得减回来
            return idTable[src].lenght != 1;
        }
        private bool isCommonID(string src)
        {
            return idTable[src].numOfVar == -1 && idTable[src].lenght == 1;
        }
        private bool isDclred(string src)
        {
            if (!idTable.ContainsKey(src))
            {
                ++n;
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 变量未声明:"+src+"\r\n";
                return false;
            }
            return true;
        }
        private bool isDefined(string src)
        {
            if (idTable.ContainsKey(src))
            {
                if (idTable[src].isDefined == false)
                {
                    if (isArrayID(src))
                        n += 3;
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 变量未初始化:" + src + "\r\n";
                    return false;
                }
                return true;
            }
            else
                return false; 
        }

        //--------------------------------------------------------------------------------------
        private void Match(int type, ref TreeNode root)
        {
            int realType = Global.tokenArr[n].type;
            if (realType == type)
            {
                TreeNode tn = NewTreeNode(ref root, Global.tokenArr[n].src);
                ++n;
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 应为 '" + symTable[type - 1] + "', 但得到了 '" + symTable[realType - 1] + "'\r\n";
            }
        }
        private void HandleId(Token token, int type, int len = 1, int assign = -1)
        {
            if(idTable.ContainsKey(token.src))
	        {
		        ++errNum;
                errStr += "line " + token.lineNum + " : 变量 \"" + token.src + "\" 重复定义\r\n";
                return;
	        }

            ID id = new ID(token.src, idTable.Count, type, len, assign);
            idTable.Add(token.src, id);
            ID.RefreshCount(len);
        }
        private TreeNode NewTreeNode(ref TreeNode root, string src)
        {
            TreeNode tn = new TreeNode(src);
            root.Nodes.Add(tn);
            return tn;
        }
        //------------------------------------------------------------------------------------
        /*
        在main.cs中调用，格式化输出语法错误
        */
        public string formateError()
        {
            string errResult = "\r\n";
            errResult += "======================语法语义分析： ";
            errResult += "Error（" + errNum + "）";
            errResult += "======================";
            errResult += "\r\n\r\n";
            errResult += errStr;
            return errResult;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CMMInterpreter.Process
{
    class Grammar//语法分析，使用递归下降子程序法
    {
        private int n = 0;                                                          //当前Token指针
        private List<string> symTable = new List<string>();		                    //Token类型表
        private bool isInExpr = false;
        public int errNum = 0;                                                      //错误个数
        public string errStr = "";                                                  //错误输出
        private int exprType = 0;                                                   //0为int，1为real
        public TreeView gTree = null;
        private TreeNode begin = null;

        public void grammarAnalyze()
        {
            init();
            Program();
            if(isFunMain())
                FunMain(ref begin);
            Match((int)Symbol.EOF, ref begin);
        }
        private void init()
        {
            //进行清理，以免之前就进行过语法分析，消除干扰
            n = 0;
            isInExpr = false;
            errNum = 0;
            errStr = "";
            exprType = 0;
            ID.RefreshCount(0-ID.GetCount());

            symTable.Clear();
            Global.idTable.Clear();

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
            symTable.Add("标识符");
            symTable.Add("整数");
            symTable.Add("小数");
            symTable.Add("++");
            symTable.Add("--");
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
            if (isVarDclr())
            {
                VarDclr(ref begin);
                Program();
            }
            else if (type == (int)Symbol.EOF || isFunMain())
                return;
            else if (isFunDclr(type))
            {
                FunDclr(ref begin);
                Program();
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t此处不支持该语句\r\n";
                if(TolerateErr((int)Symbol.SEMICOLON))
                    ++n;
            }
        }

        private bool isFunMain()
        {
            if (n + 1 >= Global.tokenArr.Count)
                return false;

            bool isVoid = Global.tokenArr[n].type == (int)Symbol.VOID;
            bool isId = Global.tokenArr[n+1].type == (int)Symbol.IDENT;
            bool isMainSrc = Global.tokenArr[n+1].src == "main";
            if (isVoid && isId && isMainSrc)
                return true;
            return false;
        }
        private void FunMain(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "Main函数");

            //返回类型
            int type = (int)Symbol.VOID; //用于与返回值类型进行比较
            Type(ref tn);

            //这里不用判别是不是id就可以加入符号表，因为已经符合FunDclr的要求了（通过了isFunDclr）
            int record = n;//main
            NewID(Global.tokenArr[n - 1].type, ref tn);//参数为函数返回值类型
            Global.idTable[Global.tokenArr[record].src].vcodeAssignLine = Global.tokenArr[record].lineNum;
            Global.idTable[Global.tokenArr[record].src].isDefined = false;//未声明，从而禁止main函数的调用和递归调用

            //参数列表：为空
            Match((int)Symbol.LPAREN, ref tn);
            Match((int)Symbol.RPAREN, ref tn);

            //函数体
            int returnType = FunBody(ref tn);
            if ((type == (int)Symbol.REALNUM && returnType != (int)Symbol.REALNUM && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.INTEGER && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.VOID && returnType != (int)Symbol.VOID))
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : \t函数返回值类型与实际返回值类型不匹配\r\n";
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
            Global.idTable[Global.tokenArr[record].src].vcodeAssignLine = Global.tokenArr[record].lineNum;
            Global.idTable[Global.tokenArr[record].src].isDefined = true;

            //保存现场
            Dictionary<string, ID> tempTable = new Dictionary<string, ID>(Global.idTable, null);//用于暂存符号表
            ID.RefreshCount(0-ID.GetCount());//变量清零
            ClearCase();

            TreeNode tn2 = NewTreeNode(ref tn, "参数列表");
            Match((int)Symbol.LPAREN, ref tn2);
            VarListDclr(ref tn2);
            Global.idTable[Global.tokenArr[record].src].numOfVar = ID.GetCount();//获取现在临时id表中的变量个数，即新加入的变量个数
            Match((int)Symbol.RPAREN, ref tn2);
              
            int returnType = FunBody(ref tn);
            if ((type == (int)Symbol.REALNUM && returnType != (int)Symbol.REALNUM && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.INTEGER && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.VOID && returnType != (int)Symbol.VOID))
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : \t函数返回值类型与实际返回值类型不匹配\r\n";
            }
              
            //恢复现场
            Global.idTable[Global.tokenArr[record].src].isDefined = true;//已声明
            Global.idTable = tempTable;//还原符号表
            ID.RefreshCount(tempTable.Count());
        }
        private void ClearCase()//去除不可使用的变量，即外层变量,不包括函数id
        {
            string[] temp = Global.idTable.Keys.ToArray();
            for (int i = 0; i < temp.Count(); i++ )
            {
                if (Global.idTable[temp[i]].vcodeAssignLine == -1)
                    Global.idTable.Remove(temp[i]);
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
                Global.idTable[record.src].isDefined = true;//已声明

                VarListDclr_(ref root);
            }
            //else // ε
            if (Global.tokenArr[n].type != (int)Symbol.RPAREN)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t形参列表错误\r\n";
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
                if(exprType == 0)
                    return (int)Symbol.INTEGER;
                else
                    return (int)Symbol.REALNUM;
            }
            else if (type == (int)Symbol.SEMICOLON)
            {
                Empty(ref tn);
                return (int)Symbol.VOID;
            }
            else//return的后跟符号错误
            {
                //容错处理，在10个字符以内出现；或}，则可不影响后续分析
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \treturn表达式错误\r\n";
                bool findSem = TolerateErr((int)Symbol.SEMICOLON);
                if (findSem)
                    Match((int)Symbol.SEMICOLON, ref tn);
                else
                    TolerateErr((int)Symbol.RBRACE);
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t未知错误\r\n";
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
            else if (type == (int)Symbol.REAL || type == (int)Symbol.INT)
                VarDclr(ref tn);
            else if (type == (int)Symbol.SEMICOLON)
                Empty(ref tn);
            else if (type == (int)Symbol.IDENT)
            {
                if (Global.tokenArr[n + 1].type == (int)Symbol.LPAREN)
                {
                    FunCall(ref tn);
                    exprType = 0;
                }
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t未知错误\r\n";
            }
        }
        
        private void Read(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "Read语句");

            Match((int)Symbol.READ, ref tn);

            string src = Global.tokenArr[n].src;
            if (Global.idTable.ContainsKey(src))//在变量表里
            {
                if (isDclred(src) && isCommonID(src))
                {
                    Id(ref tn);
                    Global.idTable[src].isDefined = true;
                }
                else if (isDclred(src) && isArrayID(src))   //变量已声明，为数组id
                {
                    TreeNode tn2 = NewTreeNode(ref tn, "数组元素");
                    Id(ref tn);
                    int type = Global.tokenArr[n].type;
                    if (type == (int)Symbol.LBRAKET)//为数组下标
                    {
                        int index = Global.idTable[src].lenght;
                        ArrIndex(index, ref tn2);
                    }//else // ε
                    Global.idTable[src].isDefined = true;
                }
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t此处需要一个普通变量或数组元素\r\n";
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t此处需要一个普通变量或数组元素\r\n";
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \tfor循环的第一个条件错误\r\n";
                //容错处理，在10个字符以内出现分号，则可不影响后续分析
                if (TolerateErr((int)Symbol.SEMICOLON))
                    Match((int)Symbol.SEMICOLON, ref tn2);
            }

            TreeNode tn2_2 = NewTreeNode(ref tn2, "第二循环条件");
            BoolExpr(ref tn2_2);
            Match((int)Symbol.SEMICOLON, ref tn2);

            TreeNode tn2_3 = NewTreeNode(ref tn2, "第三循环条件");
            if (Global.tokenArr[n].type == (int)Symbol.IDENT)
                Assign(ref tn2_3);
            else if (Global.tokenArr[n].type == (int)Symbol.SPLUS || Global.tokenArr[n].type == (int)Symbol.SMINUS)
                Factor_2(ref tn2_3);
            else if (Global.tokenArr[n].type == (int)Symbol.RPAREN) ;//kong
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \tfor循环的第三条件错误\r\n";
                TolerateErr((int)Symbol.RPAREN);//容错处理，在10个字符以内出现右括号，则可不影响后续分析
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
                int num = Global.idTable[src].numOfVar;//参数个数
                FunCallBody(num, ref tn);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t非函数标识符不可被调用\r\n";
            }
            Match((int)Symbol.SEMICOLON, ref tn);
        }
        private void FunCallBody(int varNum, ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "参数列表");

            if (Global.idTable[Global.tokenArr[n - 1].src].type == (int)Symbol.REAL)
                exprType = 1;
            if (Global.idTable[Global.tokenArr[n - 1].src].type == (int)Symbol.VOID && isInExpr == true)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n - 1].lineNum + " : \t函数‘" + Global.tokenArr[n - 1] .src+ "’无返回值，不可参与表达式运算\r\n";
            }

            int realNum = 0;
            Match((int)Symbol.LPAREN, ref tn);
            realNum += VarList(ref tn);
            Match((int)Symbol.RPAREN, ref tn);

            if (realNum != varNum)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t函数调用形参与实参个数不相等\r\n";
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

        private bool isVarDclr()
        {
            if (n + 2 >= Global.tokenArr.Count)
                return false;

            int type = Global.tokenArr[n].type;
            bool isVarType = type == (int)Symbol.REAL || type == (int)Symbol.INT;

            int follow = Global.tokenArr[n+2].type;
            bool isVar = follow == (int)Symbol.BECOMES || follow == (int)Symbol.SEMICOLON || follow == (int)Symbol.COMMA || follow == (int)Symbol.LBRAKET;
            
            if (isVarType && isVar)
                return true;
            return false;
        }
        private void VarDclr(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "变量声明语句");

            int record = n;
            int type = Global.tokenArr[n].type;//变量类型
            Type(ref tn);
            NewID(type, ref tn);
            VarDclr_(type, ref tn);
            Match((int)Symbol.SEMICOLON, ref tn);//;

            if (type == (int)Symbol.INT && exprType == 1)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : \t无法将real类型赋值给int类型变量\r\n";
            }
            exprType = 0;
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
                    Expr(ref tn);
                    Global.idTable[Global.tokenArr[record].src].isDefined = true;//已声明
                }
                else if (type == (int)Symbol.LBRAKET)   //数组变量声明(与赋值)
                {
                    TreeNode tn = NewTreeNode(ref root, "数组下标");

                    Match((int)Symbol.LBRAKET,ref tn);//[
                    if (Global.tokenArr[n].type == (int)Symbol.INTEGER)
                    {
                        int x = int.Parse(Global.tokenArr[n].src);          //不会有异常，因为肯定是integer类型了
                        Global.idTable[Global.tokenArr[n - 2].src].lenght = x;     
                        ID.RefreshCount(x - 1);                             //之前放Id的时候已经放过1长度了,在HandleId里
                        Match((int)Symbol.INTEGER, ref tn);//]
                    }
                    else
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : \t下标类型不为整型\r\n";
                    }
                    Match((int)Symbol.RBRAKET, ref tn);

                    VarDclr_2(Global.idTable[Global.tokenArr[n - 4].src].lenght, ref tn);
                }
                else if (type == (int)Symbol.COMMA) ;    //继续声明变量
                else
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : \t未知错误\r\n";
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
                Global.idTable[Global.tokenArr[record].src].isDefined = true;//已声明

                if (realLen > length)//初始化元素个数越界
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : \t数组初始化元素个数越界\r\n";
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

            int record = n;
            int type = 0;
            string src = Global.tokenArr[n].src;
            if(Global.tokenArr[n].type == (int)Symbol.IDENT && isDclred(src))
            {
                type = Global.idTable[Global.tokenArr[record].src].type;
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
                    errStr += "line " + Global.tokenArr[n].lineNum + " : \t'" + src + "' 左值错误，不能为普通标识符或数组标识符之外的标识符\r\n";
                }
            }

            Match((int)Symbol.BECOMES, ref tn);
            if (isExpr(Global.tokenArr[n].type))
                Expr(ref tn);

            if (type == (int)Symbol.INT && exprType == 1)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : \t无法将real类型赋值给int类型变量\r\n";
            }
            exprType = 0;//将real判断 归位

            if (Global.idTable.ContainsKey(src))
                Global.idTable[src].isDefined = true;
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t需要一个布尔表达式\r\n";
            }
        }
        private bool isExpr(int type)//Expr的first集
        {
            return type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Expr(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "表达式语句");
            isInExpr = true;
            Item(ref tn);
            Expr_(ref tn);
            isInExpr = false;
        }
        private void Item(ref TreeNode root)
        {
            Factor(ref root);
            Item_(ref root);
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t未知错误\r\n";
            }
        }
        private void Factor_(ref TreeNode root)
        {
            int type = Global.tokenArr[n].type;
            if (isNum(type))
            {
                if (type == (int)Symbol.REALNUM)
                    exprType = 1;
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t未知错误\r\n";
            }
        }
        private void Factor_2(ref TreeNode root)
        {
            TreeNode tn = NewTreeNode(ref root, "自增自减表达式");

            int n_record  = n;
            int type = Global.tokenArr[n].type;
            
            int splus_record = n;
            if (type == (int)Symbol.SPLUS)
                Match((int)Symbol.SPLUS, ref tn);
            else //type == (int)Symbol.SMINUS
                Match((int)Symbol.SMINUS, ref tn);

            string src = Global.tokenArr[n].src;
            if (Global.idTable.ContainsKey(src))
            {
                if (isCommonID(src))
                    Id(ref tn);
                else if (isArrayID(src))
                {
                    Id(ref tn);
                    int index = Global.idTable[src].lenght;
                    ArrIndex(index, ref tn);
                }
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t需要一个可变左值\r\n";
                ++n;//容错处理
            }
        }
        private void SupID_(string src,ref TreeNode root)
        {
            if (isDclred(src))
            {
                if (isArrayID(src))     //为数组id
                {
                    int len = Global.idTable[src].lenght;//数组长度
                    ArrIndex(len, ref root);
                }
                else if (isFunID(src))  //为函数id
                {
                    TreeNode tn = NewTreeNode(ref root, "函数调用返回值");
                    int num = Global.idTable[src].numOfVar;//参数个数
                    FunCallBody(num, ref root);
                }
                else                  //ε，为普通id，没有后缀
                {
                    TreeNode tn = NewTreeNode(ref root, "普通变量");
                    if (Global.tokenArr[n].type == (int)Symbol.LBRAKET)
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : \t非数组标识符不可使用数组下标\r\n";
                    }
                    else if (Global.tokenArr[n].type == (int)Symbol.LPAREN)
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : \t非函数标识符不可被调用\r\n";
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

            if (newtype == (int)Symbol.INTEGER)     //下标为整数
            {
                TreeNode tn2;
                if (int.Parse(src) < 0 || int.Parse(src) >= totalNum)
                {
                    tn2 = new TreeNode("整数");
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : \t数组下标越界\r\n";
                }
                else
                    tn2 = new TreeNode("正整数");
                tn.Nodes.Add(tn2);
                Match((int)Symbol.INTEGER, ref tn2);
            }
            else if (newtype == (int)Symbol.IDENT && isDefined(src) && Global.idTable[src].type == (int)Symbol.INT && isCommonID(src))     //下标为标识符 
            {
                TreeNode tn2 = new TreeNode("整形变量");
                tn.Nodes.Add(tn2);
                Id(ref tn2);       //语法分析时不能检测出是否越界，运行时检测  
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t下标错误 或 参数错误\r\n";
                //容错处理，在10个字符以内出现]，则可不影响后续分析
                TolerateErr((int)Symbol.RBRAKET);
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t需要一个关系运算符\r\n"; 
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t类型错误\r\n";
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
            {
                TreeNode tn = NewTreeNode(ref root, "整数");
                Match((int)Symbol.INTEGER, ref tn);
            }
            else if (type == (int)Symbol.REALNUM)
            {
                TreeNode tn = NewTreeNode(ref root, "实数");
                Match((int)Symbol.REALNUM, ref tn);
            }
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t此处需要一个数字\r\n";
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
            return Global.idTable[src].numOfVar != -1;
        }
        private bool isArrayID(string src)
        {
            //申请长度不为-1即为数组标识符
            return Global.idTable[src].lenght != -1;
        }
        private bool isCommonID(string src)
        {
            return Global.idTable[src].numOfVar == -1 && Global.idTable[src].lenght == -1;
        }
        private bool isDclred(string src)
        {
            if (!Global.idTable.ContainsKey(src))
            {
                ++n;
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t变量未声明:"+src+"\r\n";
                return false;
            }
            return true;
        }
        private bool isDefined(string src)
        {
            if (Global.idTable.ContainsKey(src))
            {
                if (Global.idTable[src].isDefined == false)
                {
                    if (isArrayID(src))//跳过错误
                        n += 3;
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : \t变量未初始化:" + src + "\r\n";
                    return false;
                }
                return true;
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t变量未声明:" + src + "\r\n";
                return false;
            }
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
                errStr += "line " + Global.tokenArr[n].lineNum + " : \t应为 '" + symTable[type - 1] + "', 但得到了 '" + symTable[realType - 1] + "'\r\n";
            }
        }
        private bool TolerateErr(int type)
        {
            //容错处理，在10个字符以内出现分号，则可不影响后续分析
            bool result = false;
            int count = n;
            while (count - n < 10 && count < Global.tokenArr.Count() - 1)
            {
                if (Global.tokenArr[count].type == type)
                    break;
                ++count;
            }
            if (count - n < 10)//若在10个字符以内找到了type，定位到type所在位置
            {
                n = count;
                result = true;
            }
            return result;
        }
        private void HandleId(Token token, int type, int len = -1, int assign = -1)
        {
            if (Global.idTable.ContainsKey(token.src))
            {
                ++errNum;
                errStr += "line " + token.lineNum + " : \t变量 \"" + token.src + "\" 重复定义\r\n";
                return;
	        }

            ID id = new ID(token.src, ID.GetCount(), type, len, assign);
            Global.idTable.Add(token.src, id);
            //ID.RefreshCount(len);
        }
        private TreeNode NewTreeNode(ref TreeNode root, string src)
        {
            TreeNode tn = new TreeNode(src);
            root.Nodes.Add(tn);
            return tn;
        }
    }
}

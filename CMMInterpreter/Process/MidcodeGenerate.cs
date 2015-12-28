using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace CMMInterpreter.Process
{
    class MidcodeGenerate
    {
        private int n = 0;                                                              //当前Token指针
        private int tempNum = 0;
        private bool isInExpr = false;
        private bool isInMain = false;
        private Dictionary<string, string> opArr = new Dictionary<string, string>();    //符号表
        private Dictionary<string, ID> funcTable = new Dictionary<string, ID>();        //函数变量表
        private List<Token> exprArr = new List<Token>();
        public DataGridView midcodeGrid = new DataGridView();
        

        public void midcodeGenerate()
        {
            init();
            Global.midcodeArr.Add(new MidCode("ALC"));
            Program();
            
            if (isFunMain())
                FunMain();

            Global.midcodeArr[0].op2 = "" + ID.GetCount();
            ++n;//EOF
        }
        private void init()
        {
            //进行清理，以免之前就进行过语法分析，消除干扰
            Global.idTable.Clear();
            n = 0;
            isInExpr = false;
            tempNum = 0;
            opArr.Clear();
            Global.midcodeArr.Clear();
            midcodeGrid.Rows.Clear();
            midcodeGrid.Columns.Clear();
            ID.RefreshCount(0 - ID.GetCount());

            //创建Grid
            midcodeGrid.RowHeadersVisible = false;        //去除行头
            midcodeGrid.ScrollBars = ScrollBars.Vertical; //不显示水平滚动条
            midcodeGrid.BorderStyle = BorderStyle.None;   //去除边框显示
            midcodeGrid.ReadOnly = true;
            string[] arr = {"Num", "OP", "ARG1", "ARG2","RESULT" };
            for (int i = 0; i < arr.Length; i++)    //新建列
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = arr[i];
                col.HeaderText = arr[i];
                col.Width = 100;
                midcodeGrid.Columns.Add(col);
            }
            //midcodeGrid.Columns[0].Width = 50;//行号一列宽度为50

            opArr.Add("+", "ADD");
            opArr.Add("-", "SUB");
            opArr.Add("*", "MUL");
            opArr.Add("/", "DIV");
            opArr.Add("<", "LES");
            opArr.Add("<=", "LEQ");
            opArr.Add(">", "GTR");
            opArr.Add(">=", "GEQ");
            opArr.Add("==", "EQL");
            opArr.Add("!=", "NEQ");
            opArr.Add("=", "ASN");
            opArr.Add("jump", "JMP");
            opArr.Add("jumpc", "JPC");
        }
        private void Program()
        {
            int type = Global.tokenArr[n].type;
            if (isVarDclr())
            {
                VarDclr();
                Program();
            }
            else if (type == (int)Symbol.EOF || isFunMain())
                return;
            else if (isFunDclr(type))
            {
                FunDclr();
                Program();
            }
        }

        private bool isFunMain()
        {
            if (n + 1 >= Global.tokenArr.Count)
                return false;

            bool isVoid = Global.tokenArr[n].type == (int)Symbol.VOID;
            bool isId = Global.tokenArr[n + 1].type == (int)Symbol.IDENT;
            bool isMainSrc = Global.tokenArr[n + 1].src == "main";
            if (isVoid && isId && isMainSrc)
                return true;
            return false;
        }
        private void FunMain()
        {
            isInMain = true;

            //记录函数id的起始指令行
            ++n;//type
            int id_record = n;//main
            HandleId(Global.tokenArr[id_record], Global.tokenArr[id_record - 1].type);
            Global.idTable[Global.tokenArr[id_record].src].vcodeAssignLine = Global.midcodeArr.Count + 1;
            n += 3;//main()

            //函数体处理
            FunBody(id_record);
        }

        private bool isFunDclr(int type)
        {
            if (type == (int)Symbol.VOID || type == (int)Symbol.INT || type == (int)Symbol.REAL)
                if (n + 2 < Global.tokenArr.Count && Global.tokenArr[n + 2].type == (int)Symbol.LPAREN)
                    return true;
            return false;
        }
        private void FunDclr()
        {
            //生成JUMP中间代码,获取JUMP中间代码的行号(为下标+1)
            Global.midcodeArr.Add(new MidCode(opArr["jump"]));
            int jump_record = Global.midcodeArr.Count;
            ++n;//type

            //记录函数id的起始指令行
            int id_record = n;
            HandleId(Global.tokenArr[id_record], Global.tokenArr[id_record-1].type);
            Global.idTable[Global.tokenArr[id_record].src].vcodeAssignLine = jump_record + 1;
            n += 2;//id(

            //分配空间
            Global.midcodeArr.Add(new MidCode("ALC"));
            int alc_record = Global.midcodeArr.Count;
            Global.idTable[Global.tokenArr[id_record].src].vcodeAssignLine = alc_record;
            

            //保存现场
            Dictionary<string, ID> tempTable = new Dictionary<string, ID>(Global.idTable, null);//用于暂存符号表
            ID.RefreshCount(0 - ID.GetCount());
            ClearCase();
            int tempTempNum = tempNum;
            tempNum = 0;
            
            //参数处理
            VarListDclr();
            Global.idTable[Global.tokenArr[id_record].src].numOfVar = ID.GetCount();//获取现在临时id表中的变量个数，即新加入的变量个数            
            ++n;//)

            //处理临时变量下标
            int tempParamNum = ID.GetCount();
            HandleParamOff(tempParamNum);
            ID.RefreshCount(0 - tempParamNum);

            

            //函数体处理
            FunBody(id_record);

            //修改JUMP中间指令的跳转行和ACL中间指令的参数
            Global.midcodeArr[jump_record - 1].op2 = "" + (Global.midcodeArr.Count + 1);
            Global.midcodeArr[alc_record - 1].op2 = "" + ID.GetCount();

            //恢复现场
            Global.idTable = tempTable;//还原符号表
            ID.RefreshCount(0 - ID.GetCount());
            ID.RefreshCount(tempTable.Count());
            tempNum = tempTempNum;
        }
        private void ClearCase()//去除不可使用的变量，即外层变量
        {
            string[] temp = Global.idTable.Keys.ToArray();
            for (int i = 0; i < temp.Count(); i++)
            {
                if (Global.idTable[temp[i]].vcodeAssignLine == -1)
                    Global.idTable.Remove(temp[i]);
            }
        }
        private void VarListDclr()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.INT || type == (int)Symbol.REAL)
            {
                n += 2;//type id
                HandleId(Global.tokenArr[n-1], Global.tokenArr[n-2].type);
                VarListDclr_();
            }
            //else // ε
        }
        private void VarListDclr_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                ++n;//,
                VarListDclr();
            }
            //else // ε 
        }
        private void FunBody(int id)
        {
            ++n;//{
            SubBlock();
            Return(id);
            ++n;//}
        }
        private void Return(int id)//匹配return语句并返回 返回值类型
        {
            int return_record = n;
            ++n;//return

            int type = Global.tokenArr[n].type;
            if (isExpr(type))
            {
                Expr();
                HandleExp();
            }
            Token t = new Token("" + Global.idTable[Global.tokenArr[id].src].numOfVar, (int)Symbol.INTEGER, Global.idTable[Global.tokenArr[id].src].numOfVar);
            exprArr.Add(t);//参数个数
            exprArr.Add(Global.tokenArr[return_record]);
            if (!isInMain)
                HandleExp();
            Reset();
            ++n;//;
        }
        private void HandleParamOff(int num)
        {
            string[] temp = Global.idTable.Keys.ToArray();
            for (int i = 0; i < temp.Count(); i++)
                Global.idTable[temp[i]].index -= num;
        }

        private bool isBlock(int type)
        {
            return type == (int)Symbol.LBRACE || isStat(type);
        }
        private void Block()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.LBRACE)
            {
                ++n;
                SubBlock();
                ++n;
            }
            else// if (isStat(type))
                Stat();
        }
        private bool isSubBlock(int type)
        {
            return isStat(type);
        }
        private void SubBlock()
        {
            if (isStat(Global.tokenArr[n].type))//这里要判断一次，因为SubBlock在没有判断的时候也调用了
            {
                Stat();
                SubBlock();
            }
            //else // ε
        }

        private bool isStat(int type)
        {
            return type == (int)Symbol.READ || type == (int)Symbol.WRITE || type == (int)Symbol.WHILE || type == (int)Symbol.FOR || type == (int)Symbol.IF || type == (int)Symbol.IDENT || type == (int)Symbol.VOID || type == (int)Symbol.REAL || type == (int)Symbol.INT || type == (int)Symbol.SEMICOLON || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Stat()
        {
            int type = Global.tokenArr[n].type;

            //Statement	--->	 Read | Write | While | For | If | FunCall | VarDclr | Assign | Empty | Factor_2
            if (type == (int)Symbol.READ)
                Read();
            else if (type == (int)Symbol.WRITE)
                Write();
            else if (type == (int)Symbol.WHILE)
                While();
            else if (type == (int)Symbol.FOR)
                For();
            else if (type == (int)Symbol.IF)
                If();
            else if (type == (int)Symbol.VOID || type == (int)Symbol.REAL || type == (int)Symbol.INT)
                VarDclr();
            else if (type == (int)Symbol.SEMICOLON)
                ++n;//;
            else if (type == (int)Symbol.IDENT)
            {
                if (Global.tokenArr[n + 1].type == (int)Symbol.LPAREN)
                    FunCall();
                else
                    Assign();
                ++n;//;
            }
            else if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)
            {
                Factor_2();
                ++n;
            }
        }

        private void Read()
        {
            int read_record = n;
            ++n;//read
            string src = Global.tokenArr[n].src;
            if (Global.idTable.ContainsKey(src))//在变量表里
            {
                if (isCommonID(src))
                    Id();
                else if (isArrayID(src))   //变量已声明，为数组id
                    ArrWithIndex();
            }
            ++n;//;
            exprArr.Add(Global.tokenArr[read_record]);
            HandleExp();
            Reset();
        }
        private void Write()
        {
            int write_record = n;
            ++n;//write
            Expr();
            ++n;//;

            string src = exprArr[exprArr.Count - 1].src;
            if (Global.idTable.ContainsKey(src) && isFunID(src))
                printMidCode_FunCall(exprArr.Count - 1);

            exprArr.Add(Global.tokenArr[write_record]);
            HandleExp();
            Reset();
        }
        private void While()
        {
            int while_record = Global.midcodeArr.Count+1;
            n += 2;//while(
            BoolExpr();
            Token t = exprArr[0];
            Reset();
            int jump_record = Global.midcodeArr.Count;
            Global.midcodeArr.Add(new MidCode(opArr["jumpc"], t.src, "-"));
            ++n;//)
            Block();
            Global.midcodeArr.Add(new MidCode(opArr["jump"], "-", "" + while_record));
            Global.midcodeArr[jump_record].op2 = "" + (Global.midcodeArr.Count + 1);
        }
        private void For()
        {
            n += 2;//for(
            if (Global.tokenArr[n].type == (int)Symbol.IDENT)
                Assign();
            ++n;//;

            int for_record = Global.midcodeArr.Count + 1;
            BoolExpr();
            Token t = exprArr[0];
            Reset();
            int jumpc_record = Global.midcodeArr.Count;
            Global.midcodeArr.Add(new MidCode(opArr["jumpc"], t.src, "-"));
            ++n;//;

            int assign_record = n;
            while (Global.tokenArr[n].type != (int)Symbol.LBRACE) ++n;
            Block();
            int end_record = n;

            n = assign_record;
            if (Global.tokenArr[n].type == (int)Symbol.IDENT)
                Assign();
            else if (Global.tokenArr[n].type == (int)Symbol.SPLUS || Global.tokenArr[n].type == (int)Symbol.SMINUS)
                Factor_2();
            n = end_record;

            Global.midcodeArr.Add(new MidCode(opArr["jump"], "-", "" + for_record));
            Global.midcodeArr[jumpc_record].op2 = "" + (Global.midcodeArr.Count + 1);
        }
        private void If()
        {
            n += 2;//if(
            BoolExpr();
            Token t = exprArr[0];
            Reset();
            int jumpc_record = Global.midcodeArr.Count;
            Global.midcodeArr.Add(new MidCode(opArr["jumpc"], t.src, "-"));
            ++n;//)
            Block();
            Global.midcodeArr[jumpc_record].op2 = "" + (Global.midcodeArr.Count + 1);
            int jump_record = Global.midcodeArr.Count;
            Global.midcodeArr.Add(new MidCode(opArr["jump"], "-", "-"));
            If_();
            Global.midcodeArr[jump_record].op2 = "" + (Global.midcodeArr.Count + 1);
        }
        private void If_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.ELSE)
            {
                ++n;
                Block();
            }
            //else // ε
        }

        //FunCall	--->		Id ‘(’ VarList ‘)’ ‘;’
        private void FunCall()
        {
            int record = n;
            ++n;//Id();
            FunCallBody();

            exprArr.Add(Global.tokenArr[record]);//id放入表达式栈
            if (!isInExpr)
            {
                printMidCode_FunCall(exprArr.Count - 1);
                Reset();
            }
        }
        private void FunCallBody()
        {
            ++n;//(
            VarList();
            ++n;//)
        }
        private void VarList()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.IDENT || isNum(type))
            {
                if (type == (int)Symbol.IDENT)
                    Id();
                else if (isNum(type))
                    Num();

                VarList_();
            }
        }
        private void VarList_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                ++n;//;
                VarList();
            }
        }

        private bool isVarDclr()
        {
            if (n + 2 >= Global.tokenArr.Count)
                return false;

            int type = Global.tokenArr[n].type;
            bool isVarType = type == (int)Symbol.REAL || type == (int)Symbol.INT;

            int follow = Global.tokenArr[n + 2].type;
            bool isVar = follow == (int)Symbol.BECOMES || follow == (int)Symbol.SEMICOLON || follow == (int)Symbol.COMMA || follow == (int)Symbol.LBRAKET;

            if (isVarType && isVar)
                return true;
            return false;
        }
        private void VarDclr()
        {
            ++n;//type
            int id_record = n;
            HandleId(Global.tokenArr[id_record], Global.tokenArr[id_record - 1].type);
            ++n;//Id();
            VarDclr_(Global.tokenArr[id_record - 1].type);
            ++n;//;
            HandleExp();
            Reset();
        }
        private void VarDclr_(int kind)
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.SEMICOLON)              //ε,结束声明变量
                return;
            else
            {
                if (type == (int)Symbol.BECOMES)            //普通变量声明并赋值
                {
                    int record = n;//记录=位置
                    exprArr.Add(Global.tokenArr[record - 1]);//放入id
                    ++n;//=
                    Expr();
                    exprArr.Add(Global.tokenArr[record]);   //放入=
                }
                else if (type == (int)Symbol.LBRAKET)       //数组变量声明(与赋值)//------------------待修改：数组赋值
                {
                    int arr_record = n - 1;//arr_id
                    ++n;//[
                    int x = int.Parse(Global.tokenArr[n].src);
                    Global.idTable[Global.tokenArr[n - 2].src].lenght = x;//数组长度
                    ID.RefreshCount(x - 1);
                    n += 2;//num]
                    VarDclr_2();//数组赋值
                }
                VarDclr_3(kind);//继续声明
            }
        }
        private void VarDclr_2()//数组赋值
        {
            if (Global.tokenArr[n].type == (int)Symbol.BECOMES)
            {
                int record = n - 4;//id
                n += 2;//={
                NumList();
                ++n;//}
                exprArr.Add(Global.tokenArr[record]);
                printMidcode_ArrAsn();
            }
            //else // ε
        }
        private void VarDclr_3(int kind)//继续声明
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                ++n;//,
                HandleId(Global.tokenArr[n], kind);
                ++n;//id
                VarDclr_(kind);
            }
            //else // ε
        }
        private void Assign()//还要考虑数组的赋值啊...
        {
            int id_record = n;
            SupID();//其实这里面的函数处理在这里不会用到
            int eq_record = n;//=
            ++n;//=
            Expr();
            exprArr.Add(Global.tokenArr[eq_record]);//放入=

            HandleExp();
            Reset();
        }

        private void NumList()//用于数组初始化
        {
            if (isNum(Global.tokenArr[n].type))
            {
                Num();
                NumList_();
            }
            //else // ε
        }
        private void NumList_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                ++n;//,
                Num();
                NumList_();
            }
            //else // ε
        }

        private void BoolExpr()
        {
            Expr();
            int record = n;
            ++n;//r-op
            Expr();

            exprArr.Add(Global.tokenArr[record]);
            HandleExp();
        }
        private bool isExpr(int type)//Expr的first集
        {
            return type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Expr()
        {
            isInExpr = true;
            Item();
            Expr_();
            isInExpr = false;
        }
        private void Item()
        {
            Factor();
            Item_();
        }
        private void Expr_()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.PLUS || type == (int)Symbol.MINUS)
            {
                int record = n;
                ++n;//+/-
                Item();
                exprArr.Add(Global.tokenArr[record]);//放入+/-
                Expr_();
            }
            //else // ε
        }
        private void Item_()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.TIMES || type == (int)Symbol.DIV)
            {
                int record = n;
                ++n;//*//
                Factor();
                exprArr.Add(Global.tokenArr[record]);//放入*//
                Item_();
            }
            //else // ε
        }
        private void Factor()
        {
            int type = Global.tokenArr[n].type;
            if (isNum(type) || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN)//Factor_的first集
                Factor_();
            else// if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)//Factor_2的first集
                Factor_2();
        }
        private void Factor_()
        {
            int type = Global.tokenArr[n].type;
            if (isNum(type))
                Num();
            else if (type == (int)Symbol.IDENT)
                SupID();
            else if (type == (int)Symbol.LPAREN)
            {
                ++n;//(
                Expr();
                ++n;//)
            }
        }
        private void Factor_2()//自增自减(普通id和数组变量)
        {
            int n_record = n;
            int type = Global.tokenArr[n].type;

            int sop_record = n;
            ++n;//type == (int)Symbol.SMINUS || (int)Symbol.SPLUS

            string src = Global.tokenArr[n].src;
            if (isCommonID(src))
                Id();
            else if (isArrayID(src))
                ArrWithIndex();
            exprArr.Add(Global.tokenArr[sop_record]);

            if (!isInExpr)
            {
                HandleExp();
                Reset();
            } 
        }
        private void SupID()//处理数组变量和函数调用
        {
            Token token = Global.tokenArr[n];
            if (isArrayID(token.src))       //为数组变量
                ArrWithIndex();
            else if (isFunID(token.src))    //为函数调用
                FunCall();
            else                            //普通变量
            {
                ++n;//id
                exprArr.Add(token);
            }
        }
        private void ArrWithIndex()//数组变量处理
        {
            n += 2;//id[
            exprArr.Add(Global.tokenArr[n]);    //放入index
            n += 2;//num/id ]
            exprArr.Add(Global.tokenArr[n-4]);  //放入id
        }

        private bool isNum(int type)
        {
            if (type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM)
                return true;
            return false;
        }
        private void Num()
        {
            exprArr.Add(Global.tokenArr[n]); 
            ++n;
        }

        private void Id()//判别是否声明并吞入
        {
            exprArr.Add(Global.tokenArr[n]);
            ++n;
        }
        private void HandleId(Token token, int type, int len = -1, int assign = -1)
        {
            ID id = new ID(token.src, ID.GetCount(), type, len, assign);
            Global.idTable.Add(token.src, id);
        }
        private bool isFunID(string src)
        {
            //参数个数不为-1即为函数标识符
            return Global.idTable.ContainsKey(src) && Global.idTable[src].numOfVar != -1;
        }
        private bool isArrayID(string src)
        {
            //申请长度不为-1即为数组标识符
            return Global.idTable.ContainsKey(src) && Global.idTable[src].lenght != -1;
        }
        private bool isCommonID(string src)
        {
            return Global.idTable.ContainsKey(src) && Global.idTable[src].numOfVar == -1 && Global.idTable[src].lenght == -1;
        }

        private void HandleExp()
        {
            if (exprArr.Count == 0)
                return;

            while (exprArr.Count != 1)//只剩下一个数时，说明被处理得只剩一个最终结果了
            {
                int point;
                for (point = 0;
                    point !=exprArr.Count && !(((int)Symbol.SPLUS <= exprArr[point].type && exprArr[point].type <= (int)Symbol.LEQ) || ((int)Symbol.READ <= exprArr[point].type && exprArr[point].type <= (int)Symbol.RETURN) || ((int)Symbol.INT <= exprArr[point].type && exprArr[point].type <= (int)Symbol.VOID));
                    ++point)
                    ;//取得栈中第一个符号位置, 或者函数调用位置

                if (point == exprArr.Count && exprArr[point-1].src[0] == '@')
                    exprArr.RemoveAt(point - 1);
                else if (point == exprArr.Count)
                {
                    string id_src = exprArr[point - 1].src;
                    if (Global.idTable[id_src].numOfVar != 0)
                        printMidCode_FunCall(point - 1);
                    else
                        printMidcode_ArrAsn();
                }
                else if (isSOP(exprArr[point].type))//单目运算符，包括read write return int float void
                    printMidCode_SOP(point);
                else// if (PLUS <= exprArr[point].type && exprArr[point].type <= NEQUAL || exprArr[point].type == ASSIGN)//双目运算符
                    printMidCode(point);//第一个运算符在表达式栈里的下标
            }
            //if (exprArr[0].src[0] != '@')//剩余的操作符 or 只有一个实际变量入栈 （没有操作完）
            //    genInstru(0);
        }
        private void Reset()
        {
            exprArr.Clear();
            tempNum = 0;//每次处理完一个表达式后就重置临时变量下标
        }
        private bool isSOP(int type)
        {
            bool istype = (int)Symbol.INT <= type && type <= (int)Symbol.VOID;
            bool issingle = (int)Symbol.READ <= type && type <= (int)Symbol.RETURN;
            bool issop = (int)Symbol.SPLUS <= type && type <= (int)Symbol.SMINUS;
            return istype || issingle || issop;
        }
        private bool isReal(Token token)
        {
            bool result = false;
            if (token.type == (int)Symbol.IDENT && Global.idTable.ContainsKey(token.src))
                result = Global.idTable[token.src].type == (int)Symbol.REAL;//为1说明是浮点类型
            else if(token.src[0] == '@' && token.src.Length > 1)
                result = token.src[1] == 'R';//为临时real变量
            else
                result = token.type == (int)Symbol.REALNUM;
            return result;
        }
        private void printMidCode_SOP(int point)
        {
            int backSteps = BackStep(point - 1);
            bool flag = isReal(exprArr[point - 1]);

            if (exprArr[point].type == (int)Symbol.RETURN)
            {
                if (exprArr.Count == 3)
                {
                    int returnIndex = point - 2;
                    string result = "";
                    if (exprArr[returnIndex].type == (int)Symbol.IDENT && exprArr[returnIndex].src[0] != '@')//返回一个变量
                    {
                        string frontFix = GetFrontFix(exprArr[returnIndex]);
                        result = frontFix + Global.idTable[exprArr[returnIndex].src].index;
                    }
                    else// if (exprArr[returnIndex].src[0] == '@' || 常数)    //返回表达式计算值
                        result = exprArr[returnIndex].src;

                    Global.midcodeArr.Add(new MidCode("RET", exprArr[point - 1].src, result));
                    exprArr.RemoveAt(point - 1);//移除参数个数
                    --point;
                }
                else if (exprArr.Count == 2)
                    Global.midcodeArr.Add(new MidCode("RET", exprArr[point - 1].src));
            }
            else if (exprArr[point].type == (int)Symbol.WRITE)
            {
                string result;
                if (exprArr[point - 1].src[0] == '@')   //为临时变量
                {
                    result = exprArr[point - 1].src;
                    Global.midcodeArr.Add(new MidCode("WRT", "-", result));
                }
                else                                    //为数字或变量(普通变量或数组变量)
                {
                    if (exprArr[point - 1].type == (int)Symbol.IDENT)
                    {
                        int first = point - 1;
                        int idIndex = Global.idTable[exprArr[first].src].index;
                        string frontFix = GetFrontFix(exprArr[first]);
                        if (exprArr[first].type == (int)Symbol.IDENT && isArrayID(exprArr[first].src))//数组变量
                        {
                            //下标：若是id则形成Ix的格式(只能是I类型，否则语法会报错)，若为num则直接使用
                            string offIndex = (exprArr[first - 1].type == (int)Symbol.IDENT && exprArr[first - 1].src[0] != '@') ? "I" + Global.idTable[exprArr[first - 1].src].index : exprArr[first - 1].src;
                            Global.midcodeArr.Add(new MidCode("WRT", offIndex, frontFix + idIndex));
                            exprArr.RemoveAt(first - 1);//去除下标
                            exprArr[point - backSteps - 1].src = "@";
                        }
                        else
                            Global.midcodeArr.Add(new MidCode("WRT", "-", frontFix + idIndex));//普通id
                    }
                    else//为数字
                        Global.midcodeArr.Add(new MidCode("WRT", "-", exprArr[point - 1].src));
                }
            }
            else if (exprArr[point].type == (int)Symbol.READ)
            {
                int first = point - 1;
                int idIndex = Global.idTable[exprArr[first].src].index;
                string frontFix = GetFrontFix(exprArr[first]);
                if (exprArr[first].type == (int)Symbol.IDENT && isArrayID(exprArr[first].src))//数组变量
                {
                    //下标：若是id则形成Ix的格式(只能是I类型，否则语法会报错)，若为num则直接使用
                    string offIndex = (exprArr[first - 1].type == (int)Symbol.IDENT && exprArr[first - 1].src[0] != '@') ? "I" + Global.idTable[exprArr[first - 1].src].index : exprArr[first - 1].src;
                    Global.midcodeArr.Add(new MidCode("RED", offIndex, frontFix + idIndex));
                    exprArr.RemoveAt(first - 1);//去除下标
                    exprArr[point - backSteps - 1].src = "@";
                }
                else
                    Global.midcodeArr.Add(new MidCode("RED", "-", frontFix + idIndex));//普通id
            }
            else if (exprArr[point].type == (int)Symbol.SPLUS || exprArr[point].type == (int)Symbol.SMINUS)
            {
                string kindSrc;
                if (exprArr[point].type == (int)Symbol.SPLUS)
                    kindSrc = "+";
                else
                    kindSrc = "-";
                int first = point - 1;
                string frontFix = GetFrontFix(exprArr[first]);
                int idIndex = Global.idTable[exprArr[first].src].index; 
                if (exprArr[first].type == (int)Symbol.IDENT && isArrayID(exprArr[first].src))  //数组变量
                {
                    //下标：若是id则形成Ix的格式(只能是I类型，否则语法会报错)，若为num则直接使用
                    string offIndex = (exprArr[first - 1].type == (int)Symbol.IDENT && exprArr[first - 1].src[0] != '@') ? "I" + Global.idTable[exprArr[first - 1].src].index : exprArr[first - 1].src;
                    printMidCode_ArrGet(first);
                    Global.midcodeArr.Add(new MidCode(opArr[kindSrc], "@" + frontFix + (tempNum - 1), "1", "@" + frontFix + (tempNum - 1)));
                    Global.midcodeArr.Add(new MidCode(opArr["="], frontFix + idIndex, "@" + frontFix + (tempNum - 1), offIndex));
                    exprArr.RemoveAt(first - 1);//去除下标
                    exprArr[point - backSteps - 1].src = "@";//将数组id改为临时变量
                }
                else                                                                            //普通变量
                {
                    Global.midcodeArr.Add(new MidCode(opArr[kindSrc], frontFix + idIndex, "1", "@" + tempNum));
                    Global.midcodeArr.Add(new MidCode(opArr["="], frontFix + idIndex, "@" + tempNum++));
                }
            }
            if (tempNum >= 2)
                tempNum = 1;
            exprArr.RemoveAt(point - backSteps);        //移除符号
        }
        private void printMidCode(int point)//point为运算符所在下标
        {
	        int first = getFirstId(point - 1);
	        int second = point - 1;//second即为point-1，肯定是运算符的前一个，因为数组是先把下标放进来的
            
            int firstBackStep = BackStep(first);
            int secBackStep = BackStep(second);
	        
            bool left = isReal(exprArr[first]);
            bool right = isReal(exprArr[second]);
	        bool intOPfloat = left ^ right;

            if (exprArr[point].type == (int)Symbol.BECOMES)//1.为赋值符号=----------------------------------------------
	        {
                if (exprArr[second].src[0] != '@')//被赋的 值 不为临时变量
                {
                    if (exprArr[second].type == (int)Symbol.IDENT && isArrayID(exprArr[second].src))
                        printMidCode_ArrGet(second);
                    else if (exprArr[second].type == (int)Symbol.IDENT && isFunID(exprArr[second].src))
                        printMidCode_FunCall(second);//传入函数名标识符在表达式栈中的下标
                    else if (exprArr[second].type == (int)Symbol.IDENT)//普通变量
                        exprArr[second].src = GetFrontFix(exprArr[second]) + Global.idTable[exprArr[second].src].index;
                }

                //real = int [第一个操作数的类型不会比第二个操作数类型低(由语法分析决定)]
                if (intOPfloat && !right)
                {
                    Global.midcodeArr.Add(new MidCode("ITR", exprArr[second - secBackStep].src, "-", "@R" + tempNum));
                    exprArr[second - secBackStep].src = "@R" + tempNum++;
                    exprArr[second - secBackStep].type = (int)Symbol.IDENT;//临时变量
                }

                int idIndex = Global.idTable[exprArr[first].src].index;
                string asnFrontFix = GetFrontFix(exprArr[first]);
                if (exprArr[first].type == (int)Symbol.IDENT && isArrayID(exprArr[first].src))//数组id赋值
		        {
                    //若为标识符，则生成标识符位置，否则不变化
                    string indexFrontFix = GetFrontFix(exprArr[first - 1]);
                    string offIndex = (exprArr[first - 1].type == (int)Symbol.IDENT && exprArr[first - 1].src[0] != '@') ? indexFrontFix + Global.idTable[exprArr[first - 1].src].index : exprArr[first - 1].src;
                    Global.midcodeArr.Add(new MidCode(opArr["="], asnFrontFix + idIndex, exprArr[second - secBackStep].src, offIndex));
			        first -= 1;
                    exprArr.RemoveAt(first);
		        }else
                    Global.midcodeArr.Add(new MidCode(opArr["="], asnFrontFix + idIndex, exprArr[second - secBackStep].src));//普通id赋值

                point = point - firstBackStep - secBackStep;
                exprArr[first].src = "@";	//将处理过的第一个操作数改为临时变量
                exprArr[first].type = (int)Symbol.IDENT;	//将其类型改为 变量 （临时变量）
	        }
            else if ((int)Symbol.PLUS <= exprArr[point].type && exprArr[point].type <= (int)Symbol.LEQ)//2.为运算符+ - * / < <= > >= != ==
            {
                if (exprArr[first].src[0] != '@')//第一个操作数不为临时变量
                {
                    genInstru(first);
                    first = first - firstBackStep;
                }
                if (intOPfloat && !left)//化简之后是：!a && b
                {
                    if (exprArr[first].src[0] == '@')//为临时变量
                    {
                        string sourceIndex = exprArr[first].src.Substring(2).ToString();
                        Global.midcodeArr.Add(new MidCode("ITR", exprArr[first].src, "-", "@R" + sourceIndex));
                        exprArr[first].src = "@R" + sourceIndex;
                    }
                    else
                    {
                        Global.midcodeArr.Add(new MidCode("ITR", exprArr[first].src, "-", "@R" + tempNum));
                        exprArr[first].src = "@R" + tempNum++;
                        exprArr[first].type = (int)Symbol.IDENT;//临时变量
                    }
                }

                if (exprArr[second - firstBackStep].src[0] != '@')//第二个操作数不为临时变量
                {
                    genInstru(second - firstBackStep);
                    second = second - firstBackStep - secBackStep;
                }
                if (intOPfloat && !right)//化简之后是：a && !b
                {
                    Global.midcodeArr.Add(new MidCode("ITR", exprArr[second].src, "-", "@R" + tempNum));
                    exprArr[second].src = "@R" + tempNum++;
                    exprArr[second].type = (int)Symbol.IDENT;//临时变量
                }

                point = point - firstBackStep - secBackStep;
                string result;//临时变量使用的优化
                string firstSrc = (exprArr[first].type == (int)Symbol.IDENT && exprArr[first].src[0] != '@') ? GetFrontFix(exprArr[first]) + Global.idTable[exprArr[first].src].index : exprArr[first].src;
                string secondSrc = (exprArr[second].type == (int)Symbol.IDENT && exprArr[second].src[0] != '@') ? GetFrontFix(exprArr[second]) + Global.idTable[exprArr[second].src].index : exprArr[second].src;
                if ((int)Symbol.EQL <= exprArr[point].type && exprArr[point].type <= (int)Symbol.LEQ)   //关系运算
                    result = "@" + tempNum++;
                else                                                                                    //逻辑运算
                {
                    int which = first;
                    if (exprArr[first].src[0] != '@' && exprArr[second].src[0] == '@')//第一个操作数不为临时变量，第二个操作数为临时变量
                        which = second;
                    if (left || right)  //至少有一个real类型
                        result = (exprArr[first].src[0] == '@' || exprArr[second].src[0] == '@') ? exprArr[which].src : "@R" + tempNum++;//都不为临时变量就生成新临时变量
                    else                //都为int类型
                        result = (exprArr[first].src[0] == '@' || exprArr[second].src[0] == '@') ? exprArr[which].src : "@I" + tempNum++;//都不为临时变量则生成新临时变量
                }
                Global.midcodeArr.Add(new MidCode(opArr[exprArr[point].src], firstSrc, secondSrc, result));
                exprArr[first].src = result;
                exprArr[first].type = (int)Symbol.IDENT;//临时变量

                if (tempNum >= 2)
                    tempNum = 1;
            }
	        exprArr.RemoveAt(point);        //移除符号
            exprArr.RemoveAt(point - 1);    //移除符号前一个操作数(第二个操作数)
        }
        private void printMidcode_ArrAsn()
        {
            List<Token> tmpExprArr = new List<Token>(exprArr);
            int indexNum = 0;
            Token arr_id = new Token(exprArr[exprArr.Count - 1]);//不能直接赋值，要深复制
            Token asn = new Token("=", (int)Symbol.BECOMES, arr_id.lineNum);
            while (tmpExprArr.Count > 1)//到最后只剩一个id
            {
                Reset();
                Token index = new Token("" + indexNum, (int)Symbol.INTEGER, arr_id.lineNum);
                Token id = new Token(arr_id);
                exprArr.Add(index);
                exprArr.Add(id);
                exprArr.Add(tmpExprArr[0]);
                exprArr.Add(asn);
                HandleExp();
                tmpExprArr.RemoveAt(0);
                ++indexNum;
            }
        }
        private void printMidCode_ArrGet(int point)//id下标
        {
            int baseIndex = Global.idTable[exprArr[point].src].index;   //id位置
            string frontFix = GetFrontFix(exprArr[point]);

            if (exprArr[point - 1].type == (int)Symbol.IDENT)           //index为id
            {
                int offIndex = Global.idTable[exprArr[point - 1].src].index;
                string offFix = GetFrontFix(exprArr[point - 1]);
                Global.midcodeArr.Add(new MidCode("ARR", frontFix + baseIndex, offFix + offIndex, "@" + frontFix + tempNum));
            }
            else                                                        //index为数字
            {
                int num = int.Parse(exprArr[point - 1].src);
                Global.midcodeArr.Add(new MidCode("ARR", frontFix + baseIndex, "" + num, "@" + frontFix + tempNum));
            }

            exprArr[point].src = "@" + frontFix + tempNum++;
            exprArr.RemoveAt(point - 1);
        }
        private void printMidCode_FunCall(int point)
        {
            int num = Global.idTable[exprArr[point].src].numOfVar;  //获取该函数的参数个数
            for (int i = num; i > 0; --i)                       //按从左至右的顺序处理参数
            {
                string param;
                if (Global.idTable.ContainsKey(exprArr[point - i].src)) //参数为id
                {
                    string frontFix = GetFrontFix(exprArr[point - i]);
                    param = frontFix + Global.idTable[exprArr[point - i].src].index;
                }
                else                                                    //参数为num
                    param = exprArr[point - i].src;
                Global.midcodeArr.Add(new MidCode("PRM", "-", param));
                exprArr.RemoveAt(point - i);
                --point;//point现在还是指向函数src
            }
            Global.midcodeArr.Add(new MidCode("CAL", "-", "" + Global.idTable[exprArr[point].src].vcodeAssignLine));//此时point-num是现在函数id位置
            string funcFrontFix = GetFrontFix(exprArr[point]);
            exprArr[point].src = "@" + funcFrontFix + tempNum++;
        }
        private int getFirstId(int point)//point：符号token的前一个token指针
        {
            if (exprArr[point].type == (int)Symbol.IDENT && isArrayID(exprArr[point].src))		//符号前一个token为数组ID
                return point - 2;		//返回除了数组标识符和下标变量外的最后一个token下标
            else if (exprArr[point].type == (int)Symbol.IDENT && isFunID(exprArr[point].src))	//符号前一个token为函数名称ID
            {
                int tmp = Global.idTable[exprArr[point].src].numOfVar;//获取该函数的参数个数
                return point - tmp - 1;	//返回除了函数标识符和实参变量外的最后一个token下标
            }
            else// if (exprArr[point].type == (int)Symbol.IDENT || exprArr[point].type == (int)Symbol.REALNUM || exprArr[point].type == (int)Symbol.INTEGER)//符号前一个token为变量或数字
                return point - 1;		//返回这个token的前一下标
        }
        private int BackStep(int point)
        {
            if (exprArr[point].type == (int)Symbol.IDENT && isArrayID(exprArr[point].src))
                return 1;
            else if (exprArr[point].type == (int)Symbol.IDENT && isFunID(exprArr[point].src))
                return Global.idTable[exprArr[point].src].numOfVar;//返回参数个数
            else// if (exprArr[point].type == (int)Symbol.IDENT || exprArr[point].type == (int)Symbol.REALNUM || exprArr[point].type == (int)Symbol.INTEGER)
                return 0;
        }
        private void genInstru(int point)
        {
            if (exprArr[point].type == (int)Symbol.IDENT && isArrayID(exprArr[point].src))
                printMidCode_ArrGet(point);
            else if (exprArr[point].type == (int)Symbol.IDENT && isFunID(exprArr[point].src))
                printMidCode_FunCall(point);
        }
        private string GetFrontFix(Token t) 
        {
            if (isReal(t))
                return "R";
            else
                return "I";
        }

        /*
        在main.cs中调用，格式化输出中间代码
        */
        public void formateMidcode()
        {
            for (int i = 0; i < Global.midcodeArr.Count; ++i)
            {
                //创建一行，并设置属性
                DataGridViewRow dr = new DataGridViewRow();
                dr.CreateCells(midcodeGrid);
                dr.Cells[0].Value = i+1;
                dr.Cells[1].Value = Global.midcodeArr[i].src;
                dr.Cells[2].Value = Global.midcodeArr[i].op1;
                dr.Cells[3].Value = Global.midcodeArr[i].op2;
                dr.Cells[4].Value = Global.midcodeArr[i].result;
                dr.Height = 20;
                midcodeGrid.Rows.Add(dr);
            } 
        }

        public void SaveMidDefault()
        {
            string outTxt = GetOutTxt();

            string filePath = @"in.mic";
            FileStream localFile = new FileStream(filePath, FileMode.Create);//创建新文件或覆盖旧文件
            StreamWriter swLocal = new StreamWriter(localFile);
            try
            {
                swLocal.Write(outTxt);
                swLocal.Flush();
            }
            finally
            {
                swLocal.Close();
                localFile.Close();
            }
        }
        public string GetOutTxt()
        {
            string outTxt = "";
            for (int i = 0; i < Global.midcodeArr.Count; ++i)
            {
                outTxt += Global.midcodeArr[i].src + "\t";
                outTxt += Global.midcodeArr[i].op1 + "\t";
                outTxt += Global.midcodeArr[i].op2 + "\t";
                outTxt += Global.midcodeArr[i].result;
                if (i != Global.midcodeArr.Count)
                    outTxt += "\r\n";
            }
            return outTxt;
        }
    }
}

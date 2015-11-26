using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace cmmInterpreter.Process
{
    class MidCodeGenerate//中间代码生成
    {
        private int n = 0;                                                          //当前Token指针
        private List<string> symTable = new List<string>();		                    //Token类型表
        private Dictionary<string, ID> idTable = new Dictionary<string, ID>();      //变量表
        public int errNum = 0;                                                      //错误个数
        private string errStr = "";                                                  //错误输出
        private string resultStr = "";
        private int tabCount = 0;//用于记录\t的个数

        public void midcodeGenerate()
        {
            init();
            resultStr += "----------------------------";
            resultStr += " 开始分析 ";
            resultStr += "----------------------------"; 
            resultStr += "\r\n";
            Program();
            Match((int)Symbol.EOF);
        }
        private void init()
        {
            //进行清理，以免之前就进行过语法分析，消除干扰
            n = 0;
            errNum = 0;
            errStr = "";
            resultStr = "";
            tabCount = 0;

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
        }
        private void Program()
        {
            resultStr += "\r\n";
            tabCount = 0;//缩进清零
            int type = Global.tokenArr[n].type;
            if (isFunDclr(type))
            {
                FunDclr();
                Program();
            }
            else if (isBlock(type))
            {
                Block();
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
                if (Global.tokenArr[n + 2].type == (int)Symbol.LPAREN)
                    return true;
            return false;
        }
        private void FunDclr()
        {
            HandleTab();
            resultStr += tabCount + ":函数声明\r\n";
            ++tabCount;

            int type; //用于与返回值类型进行比较
            if (Global.tokenArr[n].type == (int)Symbol.INT)
                type = (int)Symbol.INTEGER;
            else if (Global.tokenArr[n].type == (int)Symbol.REAL)
                type = (int)Symbol.REALNUM;
            else
                type = (int)Symbol.VOID;
            Type();

            //这里不用判别是不是id就可以加入符号表，因为已经符合FunDclr的要求了（通过了isFunDclr）
            int record = n;
            NewID(Global.tokenArr[n - 1].type);//参数为函数返回值类型
            idTable[Global.tokenArr[record].src].vcodeAssignLine = Global.tokenArr[record].lineNum;

            //保存现场
            Dictionary<string, ID> tempTable = new Dictionary<string, ID>(idTable, null);//用于暂存符号表
            ID.RefreshCount(0-ID.GetCount());
            ClearCase();

            HandleTab();
            resultStr += tabCount + ":参数列表\r\n";
            ++tabCount;
            Match((int)Symbol.LPAREN);
            VarListDclr();
            idTable[Global.tokenArr[record].src].numOfVar = ID.GetCount();//获取现在临时id表中的变量个数，即新加入的变量个数
            Match((int)Symbol.RPAREN);
            --tabCount;
              
            int returnType = FunBody();
            if ((type == (int)Symbol.REALNUM && returnType != (int)Symbol.REALNUM && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.INTEGER && returnType != (int)Symbol.INTEGER) || (type == (int)Symbol.VOID && returnType != (int)Symbol.VOID))
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 函数返回值类型与实际返回值类型不匹配\r\n";
            }
              
            //恢复现场
            idTable[Global.tokenArr[record].src].isDefined = true;//已声明
            idTable = tempTable;//还原符号表
            ID.RefreshCount(tempTable.Count());
            --tabCount;
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
        private void VarListDclr()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.INT || type == (int)Symbol.REAL)
            {
                HandleTab();
                resultStr += tabCount + ":参数声明\r\n";
                ++tabCount;
                Type();
                Token record = Global.tokenArr[n];
                NewID(type);
                idTable[record.src].isDefined = true;//已声明
                --tabCount;

                VarListDclr_();
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
        private void VarListDclr_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA);
                VarListDclr();
            }
            //else // ε 
        }
        private int FunBody()
        {
            HandleTab();
            resultStr += tabCount + ":函数体\r\n";
            ++tabCount;

            Match((int)Symbol.LBRACE);
            SubBlock();

            int returnType = Return();
            Match((int)Symbol.RBRACE);

            --tabCount;
            return returnType;
        }
        private int Return()//匹配return语句并返回 返回值类型
        {
            HandleTab();
            resultStr += tabCount + ":返回语句\r\n";
            ++tabCount;
            Match((int)Symbol.RETURN);

            int type = Global.tokenArr[n].type;
            if (isExpr(type))
            {
                Expr();
                Match((int)Symbol.SEMICOLON);
                --tabCount;
                return (int)Symbol.INTEGER;
            }
            else if (type == (int)Symbol.SEMICOLON)
            {
                Empty();
                --tabCount;
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
                    Match((int)Symbol.SEMICOLON);
                }
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : return表达式错误\r\n";
                --tabCount;
                return (int)Symbol.SEMICOLON;
            }
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
                HandleTab();
                resultStr += tabCount + ":代码块\r\n";
                ++tabCount;

                Match((int)Symbol.LBRACE);
                SubBlock();
                Match((int)Symbol.RBRACE);
            }
            else if (isStat(type))
                Stat();
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
            --tabCount;
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
            HandleTab();
            resultStr += tabCount + ":基本语句\r\n";
            ++tabCount;
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
                Empty();
            else if (type == (int)Symbol.IDENT)
            {
                if (Global.tokenArr[n + 1].type == (int)Symbol.LPAREN)
                    FunCall();
                else
                {
                    Assign();
                    Match((int)Symbol.SEMICOLON);
                }
            }
            else if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)
            {
                Factor_2();
                Match((int)Symbol.SEMICOLON);
            }
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
            --tabCount;
        }
        
        private void Read()
        {
            HandleTab();
            resultStr += tabCount + ":Read语句\r\n";
            ++tabCount;

            Match((int)Symbol.READ);

            int record = n;
            string src = Global.tokenArr[record].src;
            if (isDclred(src) && isCommonID(src))
            {
                Id();
                idTable[src].isDefined = true;
            }
            else if (isDclred(src) && isArrayID(src))   //变量已声明，为数组id
            {
                HandleTab();
                resultStr += tabCount + ":数组元素\r\n";
                ++tabCount;
                Id();
                int type = Global.tokenArr[n].type;
                if (type == (int)Symbol.LBRAKET)//为数组下标
                {
                    int index = idTable[src].lenght;
                    ArrIndex(index);
                }//else // ε
                --tabCount;
                idTable[src].isDefined = true;
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[record].lineNum + " : 数组id未声明，找不到为src的数组变量\r\n";
            }
            Match((int)Symbol.SEMICOLON);
            --tabCount;
        }
        private void Write()
        {
            HandleTab();
            resultStr += tabCount + ":Write语句\r\n";
            ++tabCount;
            Match((int)Symbol.WRITE);
            Expr();
            Match((int)Symbol.SEMICOLON);
            --tabCount;
        }
        private void While()
        {
            HandleTab();
            resultStr += tabCount + ":While语句\r\n";
            ++tabCount;

            Match((int)Symbol.WHILE);
            Match((int)Symbol.LPAREN);
            BoolExpr();
            Match((int)Symbol.RPAREN);

            Block();
            --tabCount;
        }
        private void For()
        {
            HandleTab();
            resultStr += tabCount + ":For语句\r\n";
            ++tabCount;
            Match((int)Symbol.FOR);

            HandleTab();
            resultStr += tabCount + ":For循环条件\r\n";
            ++tabCount;
            Match((int)Symbol.LPAREN);
            //Assign | Empty
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.IDENT)
            {
                Assign();
                Match((int)Symbol.SEMICOLON);
            }
            else if (type == (int)Symbol.SEMICOLON)
                Empty();
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
                    Match((int)Symbol.SEMICOLON);
                }
            }
                
            BoolExpr();
            Match((int)Symbol.SEMICOLON);

            if (Global.tokenArr[n].type == (int)Symbol.IDENT)
                Assign();
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
            Match((int)Symbol.RPAREN);
            --tabCount;

            Block();
            --tabCount;
        }
        private void If()
        {
            HandleTab();
            resultStr += tabCount + ":If语句\r\n";
            ++tabCount;

            Match((int)Symbol.IF);
            Match((int)Symbol.LPAREN);
            
            BoolExpr();
            Match((int)Symbol.RPAREN);

            Block();
            If_();
            --tabCount;
        }
        private void If_()
        {
            if (Global.tokenArr[n].type == (int)Symbol.ELSE)
            {
                Match((int)Symbol.ELSE);
                Block();
            }
            //else // ε
        }

        //FunCall	--->		Id ‘(’ VarList ‘)’ ‘;’
        private void FunCall()
        {
            HandleTab();
            resultStr += tabCount + ":函数调用语句\r\n";
            ++tabCount;

            int record = n;
            string src = Global.tokenArr[record].src;
            if (isDclred(src) && isFunID(src))
            {
                Id();
                int num = idTable[src].numOfVar;//参数个数
                FunCallBody(num);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 非函数标识符不可被调用\r\n";
            }
            Match((int)Symbol.SEMICOLON);
            --tabCount;
        }
        private void FunCallBody(int varNum)
        {
            HandleTab();
            resultStr += tabCount + ":参数列表\r\n";
            ++tabCount;

            int realNum = 0;
            Match((int)Symbol.LPAREN);
            realNum += VarList();
            Match((int)Symbol.RPAREN);

            if (realNum != varNum)
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 函数调用形参与实参个数不相等\r\n";
            }
            --tabCount;
        }
        private int VarList()
        {
            int realNum = 0;
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.IDENT || isNum(type))
            {
                HandleTab();
                resultStr += tabCount + ":参数\r\n";

                realNum += 1;
                if (type == (int)Symbol.IDENT)
                    Id();
                else if (isNum(type))
                    Num();

                realNum += VarList_();
            }
            //else // ε 
            return realNum;
        }
        private int VarList_()
        {
            int realNum = 0;
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA);
                realNum += VarList();
            }
            //else // ε 
            return realNum;
        }

        private void VarDclr()
        {
            HandleTab();
            resultStr += tabCount + ":变量声明语句\r\n";
            ++tabCount;

            int type = Global.tokenArr[n].type;
            Type();
            NewID(type);

            VarDclr_(type);
            Match((int)Symbol.SEMICOLON);
            --tabCount;
        }
        private void VarDclr_(int kind)
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
                    HandleTab();
                    resultStr += tabCount + ":普通变量初始化\r\n";
                    ++tabCount;
                    Match((int)Symbol.BECOMES);
                    Expr();
                    idTable[Global.tokenArr[record].src].isDefined = true;//已声明
                    --tabCount;
                }
                else if (type == (int)Symbol.LBRAKET)   //数组变量声明(与赋值)
                {
                    HandleTab();
                    resultStr += tabCount + ":数组下标\r\n";
                    ++tabCount;

                    Match((int)Symbol.LBRAKET);
                    if (Global.tokenArr[n].type == (int)Symbol.INTEGER)
                    {
                        int x = int.Parse(Global.tokenArr[n].src);          //不会有异常，因为肯定是integer类型了
                        idTable[Global.tokenArr[n - 2].src].lenght = x;     //数组长度+1，最后记得减回来啊...
                        ID.RefreshCount(x - 1);                             //之前放Id的时候已经放过1长度了,在HandleId里
                        Match((int)Symbol.INTEGER);
                    }
                    else
                    {
                        ++errNum;
                        errStr += "line " + Global.tokenArr[n].lineNum + " : 下标类型不为整型\r\n";
                    }
                    Match((int)Symbol.RBRAKET);
                    --tabCount;

                    VarDclr_2(idTable[Global.tokenArr[n - 4].src].lenght);
                }
                else if (type == (int)Symbol.COMMA) ;    //继续声明变量
                else
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
                }

                VarDclr_3(kind);
            }
        }
        private void VarDclr_2(int length)//数组赋值
        {
            if (Global.tokenArr[n].type == (int)Symbol.BECOMES)
            {
                int record = n - 4;
                HandleTab();
                resultStr += tabCount + ":数组变量初始化\r\n";
                ++tabCount;
          
                Match((int)Symbol.BECOMES);

                HandleTab();
                resultStr += tabCount + ":初始化列表\r\n";
                ++tabCount;
                Match((int)Symbol.LBRACE);
                int realLen = 0;
                realLen += NumList();
                Match((int)Symbol.RBRACE);
                idTable[Global.tokenArr[record].src].isDefined = true;//已声明
                //HandleExprArrAssign();
                --tabCount;

                if (realLen > length)//初始化元素个数越界
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 数组初始化元素个数越界\r\n";
                }
                --tabCount;
            }
            //else // ε
        }
        private void VarDclr_3(int kind)//继续声明
        {
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                Match((int)Symbol.COMMA);
                NewID(kind);
                VarDclr_(kind);
            }
            //else // ε
        }
        private void NewID(int kind)
        {
            HandleTab();
            resultStr += tabCount + ":声明变量\r\n";
            ++tabCount;
            HandleId(Global.tokenArr[n], kind);
            Id();
            --tabCount;
        }

        private int NumList()
        {
            int len = 0; 

            if (isNum(Global.tokenArr[n].type))
            {
                HandleTab();
                resultStr += tabCount + ":初始化参数\r\n";
                len += 1;
                Num();
                len += NumList_();
            }
            //else // ε
            return len;
        }
        private int NumList_()
        {
            int len = 0;
            if (Global.tokenArr[n].type == (int)Symbol.COMMA)
            {
                len += 1;
                Match((int)Symbol.COMMA);
                HandleTab();
                resultStr += tabCount + ":初始化参数\r\n";
                Num();
                len += NumList_();
            }
            //else // ε
            return len;
        }

        private void Assign()//还要考虑数组的赋值啊...
        {
            string src = Global.tokenArr[n].src;
            if(Global.tokenArr[n].type == (int)Symbol.IDENT && isDclred(src))
            {
                if (isCommonID(src))
                    Id();
                else if (isArrayID(src))
                {
                    Id();
                    SupID_(src);
                }
                else
                {
                    ++n;//跳过这个错误
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : '" + src + "' 左值错误，不能为普通标识符或数组标识符之外的标识符\r\n";
                }
            }

            Match((int)Symbol.BECOMES);
            Expr();
            if (idTable.ContainsKey(src))
                idTable[src].isDefined = true;
        }

        private void Empty()
        {
            Match((int)Symbol.SEMICOLON);
        }

        private void BoolExpr()
        {
            HandleTab();
            resultStr += tabCount + ":条件判断\r\n";
            ++tabCount;
            Expr();
            RelOp();
            Expr();
            --tabCount;
        }
        private bool isExpr(int type)//Expr的first集
        {
            return type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN || type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS;
        }
        private void Expr()
        {
            HandleTab();
            resultStr += tabCount + ":表达式语句\r\n";
            ++tabCount;

            Item();
            Expr_();

            --tabCount;
        }
        private void Expr_()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.PLUS || type == (int)Symbol.MINUS)
            {
                HandleTab();
                resultStr += tabCount + ":运算符\r\n";
                ++tabCount;
                if (type == (int)Symbol.PLUS)
                    Match((int)Symbol.PLUS);
                else
                    Match((int)Symbol.MINUS);
                --tabCount;

                Item();
                Expr_();
            }
            //else // ε
        }
        private void Item()
        {
            Factor();
            Item_();
        }
        private void Item_()
        {
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.TIMES || type == (int)Symbol.DIV)
            {
                HandleTab();
                resultStr += tabCount + ":运算符\r\n";
                ++tabCount;
                if (type == (int)Symbol.TIMES)
                    Match((int)Symbol.TIMES);
                else
                    Match((int)Symbol.DIV);
                --tabCount;

                Factor();
                Item_();
            }
            //else // ε
        }
        private void Factor()
        {
            HandleTab();
            resultStr += tabCount + ":表达式子项\r\n";
            ++tabCount;

            int type = Global.tokenArr[n].type;
            if (isNum(type) || type == (int)Symbol.IDENT || type == (int)Symbol.LPAREN)//Factor_的first集
                Factor_();
            else if (type == (int)Symbol.SPLUS || type == (int)Symbol.SMINUS)//Factor_2的first集
                Factor_2();
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }

            --tabCount;
        }
        private void Factor_()
        {
            int type = Global.tokenArr[n].type;
            if (isNum(type))
            {
                HandleTab();
                resultStr += tabCount + ":数字\r\n";
                Num();
            }
            else if (type == (int)Symbol.IDENT)
            {
                string src = n > 0 ? Global.tokenArr[n].src : "_";//标识符token的值，用_来作标志值，因为如果为这个肯定过不了词法分析
                Id();
                if(isDefined(src))
                    SupID_(src);
            }
            else if (type == (int)Symbol.LPAREN)
            {
                Match((int)Symbol.LPAREN);
                Expr();
                Match((int)Symbol.RPAREN);
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 未知错误\r\n";
            }
        }
        private void Factor_2()
        {
            HandleTab();
            resultStr += tabCount + ":表达式子项\r\n";
            ++tabCount;

            int num = 0;
            int n_record  = n;
            int type = Global.tokenArr[n].type;
            
            int splus_record = n;
            if (type == (int)Symbol.SPLUS)
                Match((int)Symbol.SPLUS);
            else //type == (int)Symbol.SMINUS
                Match((int)Symbol.SMINUS);

            string src = Global.tokenArr[n].src;
            if (isDefined(src))
            {
                if (isCommonID(src))
                    Id();
                else if (isArrayID(src))
                {
                    Id();
                    int index = idTable[src].lenght;
                    ArrIndex(index);
                }
                else
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 需要一个可变左值\r\n";
                }
            }

            Token t = new Token(num.ToString(), (int)Symbol.INTEGER, Global.tokenArr[n_record].lineNum);

            --tabCount;
        }
        private void SupID_(string src)
        {
            if (isDclred(src))
            {
                if (isArrayID(src))     //为数组id
                {
                    int index = idTable[src].lenght;
                    ArrIndex(index);
                }
                else if (isFunID(src))  //为函数id
                {
                    HandleTab();
                    resultStr += tabCount + ":函数调用返回值\r\n";
                    int num = idTable[src].numOfVar;//参数个数
                    FunCallBody(num);
                }
                else                  //ε，为普通id，没有后缀
                {
                    HandleTab();
                    resultStr += tabCount + ":普通变量\r\n";
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
        private void ArrIndex(int totalNum)//数组下标判别
        {
            HandleTab();
            resultStr += tabCount + ":数组下标\r\n";
            ++tabCount;
            Match((int)Symbol.LBRAKET);

            int newtype = Global.tokenArr[n].type;  //下标类型
            string src = Global.tokenArr[n].src;    //下标的值，为数值或id名

            if (newtype == (int)Symbol.INTEGER && int.Parse(src) >= 0)  //下标为正整数
            {
                HandleTab();
                resultStr += tabCount + ":正整数\r\n";
                ++tabCount;
                Match((int)Symbol.INTEGER);
                if (int.Parse(src) >= totalNum)
                {
                    ++errNum;
                    errStr += "line " + Global.tokenArr[n].lineNum + " : 数组下标越界\r\n";
                }
                --tabCount;
            }
            else if (idTable[src].type == (int)Symbol.INT && isDefined(src) && isCommonID(src))     //下标为标识符 
            {
                HandleTab();
                resultStr += tabCount + ":整型变量\r\n";
                ++tabCount;
                Id();       //这个下标必须为整形变量，现在还不能检测出这是不是一个正整数，运行时检测  
                --tabCount;
            }
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 下标错误 或 参数错误\r\n";
            }

            Match((int)Symbol.RBRAKET);
            --tabCount;
        }
        private void RelOp()
        {
            HandleTab();
            resultStr += tabCount + ":关系运算符\r\n";
            ++tabCount;
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.GTR)
                Match((int)Symbol.GTR);
            else if (type == (int)Symbol.GEQ)
                Match((int)Symbol.GEQ);
            else if (type == (int)Symbol.LES)
                Match((int)Symbol.LES);
            else if (type == (int)Symbol.LEQ)
                Match((int)Symbol.LEQ);
            else if (type == (int)Symbol.EQL)
                Match((int)Symbol.EQL);
            else if (type == (int)Symbol.NEQ)
                Match((int)Symbol.NEQ);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 需要一个关系运算符\r\n"; 
            }
            --tabCount;
        }

        private void Type()
        {
            HandleTab();
            resultStr += tabCount + ":类型声明\r\n";
            ++tabCount;
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.VOID)
                Match((int)Symbol.VOID);
            else if(type == (int)Symbol.INT)
                Match((int)Symbol.INT);
            else if (type == (int)Symbol.REAL)
                Match((int)Symbol.REAL);
            else
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 类型错误\r\n";
            }
            --tabCount;
        }

        private bool isNum(int type)
        {
            if (type == (int)Symbol.INTEGER || type == (int)Symbol.REALNUM)
                return true;
            return false;
        }
        private void Num()
        {
            ++tabCount;
            int type = Global.tokenArr[n].type;
            if (type == (int)Symbol.INTEGER)
                Match((int)Symbol.INTEGER);
            else if (type == (int)Symbol.REALNUM)
                Match((int)Symbol.REALNUM);
            else //Error
            {
                ++errNum;
                errStr += "line " + Global.tokenArr[n].lineNum + " : 此处需要一个数字\r\n";
            }
            --tabCount;
        }

        private void Id()//判别是否声明并吞入
        {
            HandleTab();
            resultStr += tabCount + ":标识符\r\n";
            ++tabCount;
            string src = Global.tokenArr[n].src;
            if (isDclred(src))
                Match((int)Symbol.IDENT);
            --tabCount;
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

        private void Match(int type)
        {
            int realType = Global.tokenArr[n].type;
            if (realType == type)
            {
                HandleTab();
                resultStr += tabCount + ":" + Global.tokenArr[n].src + "\r\n";
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

        private void HandleTab()
        {
            for (int i = 0; i < tabCount; i++)
                resultStr += "    ";
        }
        //------------------------------------------------------------------------------------
        /*
        在main.cs中调用，格式化输出语法分析结果
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
        public string formateResult()
        {
            string result = "\r\n";
            result += "=====================语法语义分析： ";
            if(errNum != 0)
                result += "Error（" + errNum + "）";
            else
                result += "Success  ";
            result += "=====================";
            result += "\r\n\r\n";
            result += resultStr;
            result += "\r\n";
            result += "----------------------------";
            result += " 结束分析 ";
            result += "----------------------------"; 
            result += "\r\n\r\n";
            result += errStr;
            return result;
        }
    }
}

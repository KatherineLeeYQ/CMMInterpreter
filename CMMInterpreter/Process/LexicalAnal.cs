using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Windows.Forms;

namespace CMMInterpreter.Process
{
    class Lexical//词法分析
    {
        private char[] current;				                    //代码缓冲区	
        private int len;                                        //代码缓冲区长度
        private int index;                                      //当前字符指针
        private string sym;					                    //当前符号
        private int line;					                    //当前符号行号
        private bool isOver;                                    //多行注释是否结束
        public int errNum = 0;					                //现有错误数量
        public string errStr = "";                              //错误输出
        public string lexResult = "";                           //词法分析结果
        public static Dictionary<string, int> keyTable = new Dictionary<string, int>();     //关键字种类
        public DataGridView lGrid = new DataGridView();             //创建Grid

        /*
            初始化全局变量
         */
        public void init(string s)
        {
            //进行清理，以免之前就进行过词法分析，消除干扰
            lexResult = "";
            current = s.ToCharArray();
            len = current.Length;
            index = 0;
            sym = "";
            line = 1;
            isOver = true;//暂无多行注释
            errNum = 0;
            errStr = "";
            Global.tokenArr.Clear();
            keyTable.Clear();
            lGrid.Rows.Clear();
            lGrid.Columns.Clear();

            //创建Grid
            lGrid.RowHeadersVisible = false;        //去除行头
            lGrid.ScrollBars = ScrollBars.Vertical; //不显示水平滚动条
            lGrid.BorderStyle = BorderStyle.None;   //去除边框显示
            lGrid.ReadOnly = true;
            string[] arr = { "Line", "Token", "LexAnal" };
            for (int i = 0; i < arr.Length; i++)    //新建列
            {
                DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
                col.Name = arr[i];
                col.HeaderText = arr[i];
                int widthLen = 0;
                switch (i)
                {
                    case 0: widthLen = 60; break;
                    case 1: widthLen = 150; break;
                    case 2: widthLen = 200; break;
                }
                col.Width = widthLen;
                lGrid.Columns.Add(col);
            }

            keyTable.Add("ident", (int)Symbol.IDENT);
            keyTable.Add("integer", (int)Symbol.INTEGER);
            keyTable.Add("realnum", (int)Symbol.REALNUM);

            //关键字
            keyTable.Add("int", (int)Symbol.INT);
            keyTable.Add("real", (int)Symbol.REAL);
            keyTable.Add("void", (int)Symbol.VOID);
            keyTable.Add("if", (int)Symbol.IF);
            keyTable.Add("else", (int)Symbol.ELSE);
            keyTable.Add("while", (int)Symbol.WHILE);
            keyTable.Add("for", (int)Symbol.FOR);
            keyTable.Add("read", (int)Symbol.READ);
            keyTable.Add("write", (int)Symbol.WRITE);
            keyTable.Add("return", (int)Symbol.RETURN);

            //操作符
            keyTable.Add("--", (int)Symbol.SMINUS);//单目减号
            keyTable.Add("++", (int)Symbol.SPLUS);//单目加号
            keyTable.Add("+", (int)Symbol.PLUS);
            keyTable.Add("-", (int)Symbol.MINUS);
            keyTable.Add("*", (int)Symbol.TIMES);
            keyTable.Add("/", (int)Symbol.DIV);
            keyTable.Add("=", (int)Symbol.BECOMES);
            keyTable.Add("==", (int)Symbol.EQL);
            keyTable.Add("!=", (int)Symbol.NEQ);
            keyTable.Add(">", (int)Symbol.GTR);
            keyTable.Add(">=", (int)Symbol.GEQ);
            keyTable.Add("<", (int)Symbol.LES);
            keyTable.Add("<=", (int)Symbol.LEQ);

            //非空界符
            keyTable.Add(",", (int)Symbol.COMMA);
            keyTable.Add(";", (int)Symbol.SEMICOLON);
            keyTable.Add("(", (int)Symbol.LPAREN);
            keyTable.Add(")", (int)Symbol.RPAREN);
            keyTable.Add("{", (int)Symbol.LBRACE);
            keyTable.Add("}", (int)Symbol.RBRACE);
            keyTable.Add("[", (int)Symbol.LBRAKET);
            keyTable.Add("]", (int)Symbol.RBRAKET);

            //结束符号
            keyTable.Add("EOF", (int)Symbol.EOF);
        }

        //判断当前字符--------------------------------------------------------------------------------------

        /*
        跳过空白符
        */
        private void removeSpace()
        {
            while (index < len && isSpace(current[index]))
            {
                ++index;
            }
        }

        /*
        跳过注释
        */
        private void removeComment()
        {
            removeSpace();
            if (index < len && current[index] == '/')
            {
                if (index < len - 1)    //还剩两个以上字符
                {
                    if (current[index + 1] == '/')       //单行注释
                    {
                        while (index < len && current[index++] != '\n') ;//忽略整行(一直++直到为换行符或文件结束，然后再++)   
                        line++;
                    }
                    else if (current[index + 1] == '*')  //多行注释
                    {
                        isOver = false;
                        index += 2;     //去掉已经知道的‘/*’
                        char pre = current[index];
                        char now = current[++index];
                        while (index < len - 1 && isOver == false)  //还剩两个以上字符
                        {
                            if (pre != '*' || now != '/')
                            {
                                if (now == '\n')
                                {
                                    line++;
                                }
                                pre = now;
                                now = current[++index];
                            }
                            else //pre == '*' && now == '/'
                            {
                                isOver = true;                      //多行注释正确结束
                                break;
                            }
                        }
                        ++index;
                    }
                    else //为除号
                        return;
                    
                    if (index - 1 >= 0 && current[index - 1] == '*')
                    {
                        //多行注释没有开始标志，无效表达式
                        ++index;//跳过/
                        errNum++;
                        errStr += "行:" + line + "\t\t无效表达式\r\n";
                    }
                    removeComment();//避免注释后又跟注释
                }
                if (!isOver)//词法报错，多行注释未结束
                {
                    errNum++;
                    errStr += "行:" + line + "\t\t多行注释未结束\r\n";
                }
                
            }
            removeSpace();
        }

        /*
        判断是否为空白界符：'\r\n'、'\t'、' '(3个)
        */
        private bool isSpace(char c)
        {
            if (c == '\n')
            {
                ++line;
            }
            return c == '\n' || c == '\t' || c == '\r' || c == ' ';
        }

        /*
        判断是否为非空符号：+、-、*、/、=、！、>、<、{、}、(、)、[、]、;、,(16个)
        */
        private bool isSignal(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/' || c == '=' || c == '!' || c == '>' || c == '<' || c == '{' || c == '}' || c == '(' || c == ')' || c == '[' || c == ']' || c == ';' || c == ',';
        }

        /*
        判断是否为复合操作符：'=='、'!='、'<='、'>='(4个)
        */
        private bool isCompoundOper()
        {
            bool isEql = current[index] == '=' && current[index + 1] == '=';
            bool isNotEql = current[index] == '!' && current[index + 1] == '=';
            bool isGrtEql = current[index] == '>' && current[index + 1] == '=';
            bool isLesEql = current[index] == '<' && current[index + 1] == '=';
            bool isSinglePlus = current[index] == '+' && current[index + 1] == '+' && isSingleOp();
            bool isSingleMinus = current[index] == '-' && current[index + 1] == '-' && isSingleOp();
            if (isEql || isNotEql || isGrtEql || isLesEql || isSinglePlus || isSingleMinus)
                return true;
            return false;
        }

        //判断sym（利用正则表达式）------------------------------------------------------------------------------------

        /*
        判断该sym是否为整数
        */
        private bool isInt(string s)
        {
            Regex intPattern = new Regex("^-?(0|([1-9]\\d*))$");
            return intPattern.IsMatch(s);
        }

        /*
        判断该sym是否为实数
        */
        private bool isReal(string s)
        {
            Regex realPattern = new Regex("(^-?(0|([1-9]\\d*))\\.[0-9]+$)");
            return realPattern.IsMatch(s);
        }

        /*
        判断该sym是否为标识符: 以字母开头，不以下划线结尾
        */
        private bool isIdent(string s)
        {
            Regex identPattern = new Regex("^[a-zA-Z]+(_*[a-zA-Z0-9]+|[a-zA-Z0-9]*)*$");
            return identPattern.IsMatch(s);
        }

        //步骤--------------------------------------------------------------------------------------

        /*
        跳过注释、空白，取出一个有意义的符号（关键字、标识符、数字、操作符、非空白界符）
        */
        private string getsym()
        {
            string word = "";
            removeComment();                                //跳过注释

            //在下次遇到空白符和非空界符前，不会再有空白符
            if (index < len && isSignal(current[index]))    //为：(非空)界符
            {
                if (isCompoundOper())       //为组合符号: '=='、'!='、'<='、'>='
                {
                    word += current[index++];
                    word += current[index++];
                }
                else                        //为单个符号: +、-、*、/、=、>、<、{、}、(、)、[、]、;、,                                    
                {
                    word += current[index++];
                    if (word == "-")
                    {
                        string src = Global.tokenArr[Global.tokenArr.Count - 1].src;//前一token的内容
                        if(src == "=" || src == "*" || src == "/" || src == "+" || src == "[")
                            word += getsym();
                    }
                }
            }
            else                            //为：关键字、标识符、数字串、错误Token
            {
                if (index < len)
                {
                    do//第一次不会是(非空)界符 或 空白符
                    {
                        word += current[index];
                        index++;
                    } while (index < len && !isSignal(current[index]) && !isSpace(current[index]));
                }            
            }
            return word;
        }

        /*
        将读取的符号放入Token数组中（在此处判断sym的类型）
        */
        private void addToToken(string s)
        {
            //值、类型、行号
            Token temp = new Token(s, -1, -1);//缺省类型为-1(错误类型)

            if (keyTable.ContainsKey(s)) //为关键字、操作符、非空界符
            {
                temp.type = keyTable[s];
            }
            else if (isIdent(s))                //为标识符
            {               
                temp.type = (int)Symbol.IDENT;
            }
            else if (isInt(s))                  //为整数
            {               
                temp.type = (int)Symbol.INTEGER;
            }
            else if (isReal(s))                 //为实数
            {
                temp.type = (int)Symbol.REALNUM;
            }

            temp.lineNum = line;
            Global.tokenArr.Add(temp);
        }

        /*
        判断是否为单目加减符号：'++'、'--'
         */
        private bool isSingleOp()
        {
            int index = Global.tokenArr.Count();
            if (index == 0)//单目运算符出现在第一个(虽然这样是语法错误的，但现在只看词法)
                return true;
            else
            {
                int aheadType = Global.tokenArr[index - 1].type;//最后一个加入的token
                //+ - * / =
                bool isOp = (aheadType == (int)Symbol.PLUS || aheadType == (int)Symbol.MINUS || aheadType == (int)Symbol.TIMES || aheadType == (int)Symbol.DIV || aheadType == (int)Symbol.BECOMES);
                //return write
                bool isWord = (aheadType == (int)Symbol.RETURN || aheadType == (int)Symbol.WRITE);
                //; {} [ (
                bool isSeperator = (aheadType == (int)Symbol.SEMICOLON || aheadType == (int)Symbol.LBRACE || aheadType == (int)Symbol.RBRACE || aheadType == (int)Symbol.LBRAKET || aheadType == (int)Symbol.LPAREN);
                return isOp || isWord || isSeperator;
            }
        }
        /*
        判断是否为符号token
         */
        private bool isOp(int type)
        {
            return type == (int)Symbol.PLUS || type == (int)Symbol.MINUS || type == (int)Symbol.TIMES || type == (int)Symbol.DIV || type == (int)Symbol.LES || type == (int)Symbol.LEQ || type == (int)Symbol.GTR || type == (int)Symbol.GEQ || type == (int)Symbol.EQL || type == (int)Symbol.NEQ || type == (int)Symbol.BECOMES;
        }

        /*
        通过种类编号获得Token的种类, 或进行错误处理
        */
        public void explainType(int t)
        {
            switch (t)
            {
                case (int)Symbol.INT:       lexResult+="整型关键字\r\n";    break;
                case (int)Symbol.REAL:      lexResult+="实数关键字\r\n";    break;
                case (int)Symbol.VOID:      lexResult+="空关键字\r\n";      break;
                case (int)Symbol.IF:        lexResult+="条件判断\r\n";      break;
                case (int)Symbol.ELSE:      lexResult+="条件分支\r\n";      break;
                case (int)Symbol.WHILE:     lexResult+="循环语句\r\n";      break;
                case (int)Symbol.READ:      lexResult+="输入语句\r\n";      break;
                case (int)Symbol.WRITE:     lexResult+="输出语句\r\n";      break;
                case (int)Symbol.RETURN:    lexResult+="返回语句\r\n";      break;
                case (int)Symbol.SPLUS:     lexResult+="自增运算符\r\n";    break;
                case (int)Symbol.SMINUS:    lexResult+="自减运算符\r\n";    break;
                case (int)Symbol.IDENT:     lexResult+="标识符\r\n";        break;
                case (int)Symbol.INTEGER:   lexResult+="整数\r\n";          break;
                case (int)Symbol.REALNUM:   lexResult+="实数\r\n";          break;
                case (int)Symbol.PLUS:
                case (int)Symbol.MINUS:
                case (int)Symbol.TIMES:
                case (int)Symbol.DIV:       lexResult+="数值运算符\r\n";    break;
                case (int)Symbol.BECOMES:   lexResult+="赋值符号\r\n";      break;
                case (int)Symbol.EQL:
                case (int)Symbol.NEQ:
                case (int)Symbol.GTR:
                case (int)Symbol.GEQ:
                case (int)Symbol.LES:
                case (int)Symbol.LEQ:       lexResult+="比较运算符\r\n";    break;
                case (int)Symbol.COMMA:     lexResult+="逗号\r\n";          break;
                case (int)Symbol.SEMICOLON: lexResult+="分号\r\n";          break;
                case (int)Symbol.LPAREN:    lexResult+="左圆括号\r\n";      break;
                case (int)Symbol.RPAREN:    lexResult+="右圆括号\r\n";      break;
                case (int)Symbol.LBRACE:    lexResult+="左花括号\r\n";      break;
                case (int)Symbol.RBRACE:    lexResult+="右花括号\r\n";      break;
                case (int)Symbol.LBRAKET:   lexResult+="左方括号\r\n";      break;
                case (int)Symbol.RBRAKET:   lexResult+="右方括号\r\n";      break;
                case -1:                    lexResult+="错误Token\r\n";     break;
                default:                    lexResult += "\r\n";            break;
            }
        }

        //主调函数--------------------------------------------------------------------------------------

        /*
        词法分析，连续分析字符取词素，放入Token数组中
        */
        public void lexicalAnalyze(string s)
        {
            init(s);
            while (index != len)    //未到文件尾
            {
                sym = getsym();                             //步骤1：连续分析字符取词素------------------

                //是关键字、标识符、数串(2种)、操作符、非空界符的一种
                if (sym != "")
                {
                    addToToken(sym);                        //步骤2：放入Token数组中---------------------
                }
            }

            for (int i = 0; i < Global.tokenArr.Count; i++) //步骤3：获取词法分析结果--------------------
            {
                lexResult += "行:" + Global.tokenArr[i].lineNum + "\t\t";
                lexResult += Global.tokenArr[i].src + "\t\t";
                explainType(Global.tokenArr[i].type);       

                if (Global.tokenArr[i].type == -1)
                {
                    errNum++;
                    errStr += "行:" + Global.tokenArr[i].lineNum + "\t\t";
                    errStr += Global.tokenArr[i].src + "\t\t" + "非法Token\r\n";
                }
            }
        }

        /*
        在main.cs中调用，格式化输出词法分析结果
        */
        public void formateResult(string codeIn, string lResult) {
            string[] lineArr = Regex.Split(codeIn.Trim(), "\r\n", RegexOptions.IgnoreCase);
            string[] resultArr = Regex.Split(lResult, "\r\n", RegexOptions.IgnoreCase);

            int resultIndex = 0;
            Regex regex;
            for (int i = 0; i < lineArr.Length; i++)
            {
                //创建一行，并设置属性
                DataGridViewRow dr = new DataGridViewRow();
                dr.CreateCells(lGrid);
                dr.Cells[0].Value = "行:" + (i + 1);
                dr.Cells[1].Value = lineArr[i].Trim();
                dr.Height = 20;
                if (lineArr[i].Trim() != "")
                    dr.DefaultCellStyle.BackColor = System.Drawing.Color.Silver;
                lGrid.Rows.Add(dr);

                regex = new Regex("行:" + (i+1)+".*");
                while (regex.IsMatch(resultArr[resultIndex]))
                {
                    string[] sublineArr = Regex.Split(resultArr[resultIndex], "\t\t", RegexOptions.IgnoreCase);
                    DataGridViewRow drSub = new DataGridViewRow();
                    drSub.CreateCells(lGrid);
                    drSub.Cells[0].Value = "   " + (i + 1);
                    drSub.Cells[1].Value = sublineArr[1].Trim();
                    drSub.Cells[2].Value = sublineArr[2].Trim();
                    drSub.Height = 18;
                    lGrid.Rows.Add(drSub);
                    resultIndex++;
                }
            }

            //人为加入一个Token:EOF，作为结束符号
            Token temp = new Token("EOF", keyTable["EOF"], line);
            Global.tokenArr.Add(temp);
        }
    }
}

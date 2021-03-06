﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Collections;

namespace CMMInterpreter.Process
{
    public enum Symbol //词法分析类型
    {
        INT=1, REAL, VOID, IF, ELSE, WHILE, FOR, READ, WRITE, RETURN,                   //关键字(1-10)
        IDENT,                                                                          //标识符(11)
        INTEGER, REALNUM,                                                               //数串：整数串、实数串(12-13)
        SPLUS, SMINUS, PLUS, MINUS, TIMES, DIV, BECOMES, EQL, NEQ, GTR, GEQ, LES, LEQ,  //操作符 13 (14-26)
        COMMA, SEMICOLON, LPAREN, RPAREN, LBRACE, RBRACE, LBRAKET, RBRAKET,             //非空界符
        EOF//结束符号，便于语法分析，词法分析没用到(35)
    }
   
    public class Token
    {
        public string src { get; set; }			//值
        public int type { get; set; }			//类型
        public int lineNum { get; set; }	    //行号

        public Token(string s, int t, int l)
        {
            src = s;
            type = t;
            lineNum = l;
        }
        public Token(Token tmp)
        {
            this.src = tmp.src;
            this.type = tmp.type;
            this.lineNum = tmp.lineNum;
        }
    }

    public class ID
    {
        public string src;          //标识符
        public int type;            //种类   
        public int index;           //在变量表中的下标
        public int numOfVar;        //默认-1，函数调用的实参个数
        public int vcodeAssignLine; //默认-1，函数声明将改变vcodeAssignLine
        public int lenght;          //默认-1，数组声明将改变length
        public bool isDefined;
        private static int count = 0;
        public ID(string s, int i, int t, int l = -1, int a = -1, int n = -1)
        {
            ++count;

            src = s;
            index = i;
            type = t;

            lenght = l;
            vcodeAssignLine = a;
            numOfVar = n;
            isDefined = false;
        }
	    public static int GetCount()
        {
            return count;
        }
	    public static void RefreshCount(int num)
        {
            count += num;
        }
    }

    public class MidCode
    {
        public string src { get; set; }
        public string op1 { get; set; }
        public string op2 { get; set; }
        public string result { get; set; }

        public MidCode(string s = "-", string o1 = "-", string o2 = "-", string r = "-")
        {
            src = s;
            op1 = o1;
            op2 = o2;
            result = r;
        }
    }

    public enum Instruction { LIT=1, CAL, RET, JMP, JPC, OPR, WRT, IPT, RTI, ITR, ALC };  //虚拟机指令
    public class VCode
    {   
        public Instruction instr;	//指令
        public int level;			//层差
        public int a;				//数
        public VCode(Instruction i, int l = -1, int b = -1)
        {
            instr = i;
            level = l;
            a = b;
        }
    }

    public class Global
    {
        public static List<Token> tokenArr = new List<Token>();		                        //Token集合：词法分析生成
        public static Dictionary<string, ID> idTable = new Dictionary<string, ID>();        //变量表：语法分析生成
        public static List<MidCode> midcodeArr = new List<MidCode>();	                    //中间代码：中间代码生成
        public static List<VCode> vcodeArr = new List<VCode>();                             //虚拟机指令表
    }
}

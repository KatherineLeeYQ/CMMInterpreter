#include <iostream>
#include <fstream>
#include <stdlib.h>
#include <string>
#include <sstream>
#include <io.h>
#include <conio.h>
#include <stdio.h>
#include <time.h>
#include <windows.h>
#include <thread>
#include "Memory.h"
#include "Frame.h"
#include "VCode.h"
using namespace std;

#ifndef MAX_MEMORY
#define MAX_MEMORY 10000
#endif
#define INST_MAX 1000

//全局静态
static Memory* myMemory;							//内存池,初始化内存为MAX_MEMORY（10000）
static Frame* myFrame;								//段
static VCode* micodeInsts;							//指令集合，最多1000条指令
static int InstCount;								//指令数,即指令偏移量
static int instEspCount;							//记录所在指令行；

/***********************************************************************
整型计算器：
1-4：	进行逻辑运算（加减乘除），将计算结果存于栈顶
5-10：	进行关系运算，true：栈顶为1，false：栈顶为0
************************************************************************/
void errorHandle(string message,int level = 0){
    string levelString = "";
	level == 1 ? levelString = "WARNING!" : levelString = "ERROR!";
	std::cout<<levelString<<endl<<"执行到第 ";
	std::cout<<instEspCount == 0 ? instEspCount+"" : "??" ;
	std::cout<<" 行时: " + message<<endl;
}
//封装myFrame->movePtr()，并统一处理异常信息
bool moveMyPagePtr(int c){
	myFrame->movePtr(c);//ebp没变，esp增加4*c，maxMemory也跟着增加这么多
	if(myFrame->msgOfPage == 1){
		errorHandle("堆栈溢出...");
		return 0;	//失败
	}
	return 1;		//成功
}
//封装myFrame->pat(),并统一处理异常信息
void* patMyPage(int i,void * tmpebp){//定位值+2，ebp
	void* tmpSavePagePtr = myFrame->pat(i,tmpebp);
	if(myFrame->msgOfPage == 2){
		errorHandle("访问地址越界...");
		return NULL;
	}
	return tmpSavePagePtr;
}

//通过指针偏移获取变量地址
void* GetLocator(string arg)
{
	int locator = atoi(arg.substr(1).c_str());
	if (arg[0] == 'I' || arg[0] == 'R')				//变量表变量
	{
		if (atoi(arg.substr(1).c_str()) >= 0)	//局部变量
			return (int*)myFrame->ebp + 1 + atoi(arg.substr(1).c_str());
		else
			return (int*)myFrame->ebp - 1 + atoi(arg.substr(1).c_str());
	}
	else if (arg[0] == '@')			//栈顶值
		return myFrame->esp;
	else							//为数值
		return NULL;
}

/***********************************************************************
整型计算器：
1-4：	进行逻辑运算（加减乘除），将计算结果存于栈顶
5-10：	进行关系运算，true：栈顶为1，false：栈顶为0
************************************************************************/
void ManipulateArg_Int(string arg)
{
	if (arg[0] != '@')//不为临时变量
	{
		//myFrame->movePtr(1);
		int pNum;
		if (arg[0] == 'I')	//为变量
		{
			void* locator = GetLocator(arg);
			pNum = myFrame->getInt(locator);
		}
		else				//为常量
			pNum = atoi(arg.c_str());
		myFrame->setInt(pNum);
	}
	//else //若为临时变量则不需处理
}
bool IntCalculator(int oprNum, string arg1, string arg2)
{
	ManipulateArg_Int(arg1);
	myFrame->movePtr(1);
	ManipulateArg_Int(arg2);
	int op_a = myFrame->getInt(myFrame->esp);//第二个参数arg2
	myFrame->movePtr(-1);
	int op_b = myFrame->getInt(myFrame->esp);//第一个参数arg1

	//操作栈顶和次栈顶值
	switch(oprNum){
	case 1:
		myFrame->setInt(op_b + op_a);break;
	case 2:
		myFrame->setInt(op_b - op_a);break;
	case 3:
		myFrame->setInt(op_b * op_a);break;
	case 4:
		if(op_a == 0){errorHandle("除数为0！");return 0;}
		myFrame->setInt(op_b / op_a);break;
	case 5:
		if(op_b < op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 6:
		if(op_b <= op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 7:
		if(op_b > op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 8:
		if(op_b >= op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 9:
		if(op_b == op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 10:
		if(op_b != op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	}
	return 1;
}

/***********************************************************************
浮点计算器：
12-15：	进行逻辑运算（加减乘除），将计算结果存于栈顶
16-21：	进行关系运算，true：栈顶为1，false：栈顶为0
************************************************************************/
void ManipulateArg_Real(string arg)
{
	if (arg[0] != '@')
	{
		myFrame->movePtr(1);
		float pNum;
		if (arg[0] == 'R')
		{
			void* locator = GetLocator(arg);
			pNum = myFrame->getReal(locator);
		}
		else
			pNum = (float)atof(arg.c_str());
		myFrame->setReal(pNum);
	}
	//else //若为临时变量则不需处理
}
bool RealCalculator(int oprNum, string arg1, string arg2){
	ManipulateArg_Real(arg1);
	ManipulateArg_Real(arg2);
	float op_a = myFrame->getReal(myFrame->esp);
	myFrame->movePtr(-1);
	float op_b = myFrame->getReal(myFrame->esp); 

	//操作栈顶和次栈顶值
	switch(oprNum){
	case 12:
		myFrame->setReal(op_b + op_a);break;
	case 13:
		myFrame->setReal(op_b - op_a);break;
	case 14:
		myFrame->setReal(op_b * op_a);break;
	case 15:
		if(op_a == 0){errorHandle("除数为0！");return 0;}
		myFrame->setReal(op_b / op_a);break;
	case 16:
		if(op_b < op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 17:
		if(op_b <= op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 18:
		if(op_b > op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 19:
		if(op_b >= op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 20:
		if(op_b == op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	case 21:
		if(op_b != op_a)
			myFrame->setInt(1);
		else
			myFrame->setInt(0);
		break;
	}
	return 1;
}

void ForRelCalculate(VCode* instEsp, int type){
	bool isRealArg = isReal(instEsp->arg1) || isReal(instEsp->arg2);
	if (isRealArg)
		RealCalculator(type, instEsp->arg1, instEsp->arg2);
	else
		IntCalculator(type, instEsp->arg1, instEsp->arg2);
}
void ForCalculate(VCode* instEsp, int type){
	if (instEsp->result[1] == 'R')
		RealCalculator(type+11, instEsp->arg1, instEsp->arg2);
	else
		IntCalculator(type, instEsp->arg1, instEsp->arg2);
}

void SetEspNum(string base, string off){
	//输出值
	void* locator1 = GetLocator(base);
	if (locator1 == NULL)//为数值
	{
		//myFrame->movePtr(1);
		if (isRealNum(base))
			myFrame->setReal((float)atof(base.c_str()));
		else
			myFrame->setInt(atoi(base.c_str()));
	}
	else //为变量
	{
		//为数组变量，加上偏移地址
		if (off != "-")
		{
			int index = 0;
			if (off[0] == 'I')	//下标为变量
				index = myFrame->getInt(GetLocator(off));
			else
				index = atoi(off.c_str());
			locator1 = (int*)locator1 + index;
		}
		std::memcpy(myFrame->esp, locator1, 4); 
	}
}

bool progRun(){
	try{
	    time_t startTime = time(NULL);
		time_t endTime;
		double waitTime = 0;
		double runTime = 0;
		for(VCode* instEsp = (micodeInsts + instEspCount - 1)//当前指令
			;instEspCount <= InstCount
			;instEspCount++ && (instEsp = (micodeInsts + instEspCount - 1)))//若当前指令行(以1开始的)小于总指令数
		{																	//获取当前指令instEsp
			if(instEsp->op == "CAL"){
				//返回后需执行的指令行，即CAL的下一行
				int RetInstEspCount = instEspCount+1;	

				//存储返回指令行
				//if(!moveMyPagePtr(1))goto progEnd;
				myFrame->setInt(RetInstEspCount);		

				//开始新帧, 存储函数调用前，父函数%ebp
				if(!moveMyPagePtr(1))goto progEnd;
				myFrame->setInt((int)myFrame->ebp);	
				myFrame->ebp = myFrame->esp;
				moveMyPagePtr(1);

				instEspCount = atoi(instEsp->arg2.c_str()) - 1;//跳转到函数开始行，-1因为for循环还会将其++
				continue;
			}
			else if (instEsp->op == "PRM")
			{
				void* locator = GetLocator(instEsp->arg2);
				if (locator == NULL)
				{
					if (isRealNum(instEsp->arg2))
						myFrame->setReal((float)atof(instEsp->arg2.c_str()));
					else
						myFrame->setInt(atoi(instEsp->arg2.c_str()));
				}
				else{
					if (isReal(instEsp->arg2))
						myFrame->setReal(myFrame->getReal(locator));
					else
						myFrame->setInt(myFrame->getInt(locator));
				}
				if(!moveMyPagePtr(1))goto progEnd;
			}
			else if(instEsp->op == "ALC"){
				myFrame->init(myFrame->ebp, 0, atoi(instEsp->arg2.c_str())*4);//从(ebp + 1)开始，申请一块内存并将其清零
				moveMyPagePtr(1);
				continue;
			}
			else if(instEsp->op == "RET"){
				//有返回值，则将要返回的数存在esp处
				if (instEsp->arg2 != "-")
					SetEspNum(instEsp->arg2, instEsp->result);

				//获取本函数ebp处存储的返回行号
				void* tmpVoidPtr;
				if((tmpVoidPtr = patMyPage(-1, myFrame->ebp)) == NULL) goto progEnd;
				instEspCount = (myFrame->getInt(tmpVoidPtr))-1;	

				//暂存本函数%esp
				void* tmpEsp = myFrame->esp;

				//切换回父函数esp，并将esp移至第一个参数处
				myFrame->esp = (int*)myFrame->ebp - 1;
				if(!moveMyPagePtr(0 - atoi(instEsp->arg1.c_str())))goto progEnd;

				//有返回值，则写回返回值
				if (instEsp->arg2 != "-")
					memcpy(myFrame->esp, tmpEsp, 4);

				//获取父函数%ebp，切换回去
				myFrame->ebp = (void*)myFrame->getInt(myFrame->ebp);

				continue;
			}
			else if(instEsp->op == "JMP"){
				instEspCount = atoi(instEsp->arg2.c_str()) - 1;//-1是因为下次循环开始要++
				continue;
			}
			else if(instEsp->op == "JPC"){
				//若栈顶值为0，表示false，跳转
				if (myFrame->getInt(myFrame->esp) == 0)
					instEspCount = atoi(instEsp->arg2.c_str()) - 1;//-1是因为下次循环开始要++
				continue;
			}
			else if(instEsp->op == "ITR"){
				//将栈顶元素由int转换为real
				SetEspNum(instEsp->arg1, "-");
				int tmpInt = myFrame->getInt(myFrame->esp);
				float tmpReal = (float)tmpInt;
				myFrame->setReal(tmpReal);
			}
			else if (instEsp->op == "RED")
			{
				//挂起
				std::cout<<"<- ";
				string inString;
				endTime = time(NULL);
				runTime += difftime(endTime,startTime);

				//等待输入
				startTime = time(NULL);
				cin >> inString;
				endTime = time(NULL);
				waitTime += difftime(endTime,startTime);

				//恢复运行
				startTime = time(NULL);
				void* locator = GetLocator(instEsp->arg2);
				//为数组变量，加上偏移地址
				if (instEsp->arg1 != "-")
				{
					int index = 0;
					if (instEsp->arg1[0] == 'I')	//下标为变量
						index = myFrame->getInt(GetLocator(instEsp->arg1));
					else
						index = atoi(instEsp->arg1.c_str());
					locator = (int*)locator + index;
				}
				istringstream stringIn(inString);
				if (isRealVar(instEsp->arg2))
				{
					float num;
					stringIn >> num;
					myFrame->setReal(num);
				}
				else
				{
					int num;
					stringIn >> num;
					myFrame->setInt(num);
				}
				//进行赋值
				memcpy(locator,myFrame->esp,4);
			}
			else if(instEsp->op == "WRT"){//WRT		1/Vb/-		Va/1/@0		-
				SetEspNum(instEsp->arg2, instEsp->arg1);

				if (isRealVar(instEsp->arg2))	
					std::cout << "-> " << myFrame->getReal(myFrame->esp)<<endl;
				else
					std::cout << "-> " << myFrame->getInt(myFrame->esp)<<endl;
			}
			else if (instEsp->op == "ARR")//ARR		1/Vb/-		Va/1		@..
			{
				SetEspNum(instEsp->arg1, instEsp->arg2);
				VCode* instEspPre = micodeInsts + instEspCount - 2;
				if (instEspPre->op == "ARR")
					moveMyPagePtr(-1);
				VCode* instEspNext = micodeInsts + instEspCount;
				if (instEspNext->op == "ARR")
					moveMyPagePtr(1);
			}
			else if(instEsp->op == "ASN"){//ASN		Va			Vb/@1/1		-/1/Vc
				//被赋值的变量
				void* locator1 = GetLocator(instEsp->arg1);
				if (instEsp->result != "-")			//数组赋值
				{
					int index = 0;
					if (instEsp->result[0] == 'I')	//且下标为变量
						index = myFrame->getInt(GetLocator(instEsp->result));
					else
						index = atoi(instEsp->result.c_str());
					locator1 = (int*)locator1 + index;
				}

				if (instEsp->arg2[0] != '@')
					SetEspNum(instEsp->arg2, "-");
					
				//进行赋值
				memcpy(locator1,myFrame->esp,4);
			}
			else if(instEsp->op == "ADD"){//ADD		Va/@0/@R0/0		Vb/@1/@R1/1		@2/@R2
				ForCalculate(instEsp, 1);
			}
			else if(instEsp->op == "SUB"){
				ForCalculate(instEsp, 2);
			}
			else if(instEsp->op == "MUL"){
				ForCalculate(instEsp, 3);
			}
			else if(instEsp->op == "DIV"){
				ForCalculate(instEsp, 4);
			}
			else if(instEsp->op == "LES"){
				ForCalculate(instEsp, 5);
			}
			else if(instEsp->op == "LEQ"){
				ForCalculate(instEsp, 6);
			}
			else if(instEsp->op == "GTR"){
				ForCalculate(instEsp, 7);
			}
			else if(instEsp->op == "GEQ"){
				ForCalculate(instEsp, 8);
			}
			else if(instEsp->op == "EQL"){
				ForCalculate(instEsp, 9);
			}
			else if(instEsp->op == "NEQ"){
				ForCalculate(instEsp, 10);
			}
		}
		endTime = time(NULL);
	    runTime += difftime(endTime,startTime);
	    std::cout << "#程序运行结束.\n" << endl;
	    std::cout << "#程序运行用时 " << runTime << " 秒." << endl;
	    std::cout << "#程序挂起时长 " << waitTime << " 秒." << endl;
		std::cout << "#堆栈最大使用量 " << myFrame->getMaxMem() << " 字节" << endl;
	}catch(...){
		std::cout << "#中间代码异常." << endl;
    }
    progEnd:
	delete [] micodeInsts;
	std::cout<<endl;
	myFrame->refresh();
	return true;
}

extern "C" __declspec(dllexport) int __stdcall main(){
	myMemory = new Memory(MAX_MEMORY);
	std::cout << "#虚拟机初始化中..." << endl;
	myFrame = new Frame(0,9000,myMemory);
	std::cout << "#虚拟机初始化完成..." << endl;

	//初始化信息
	micodeInsts = new VCode[INST_MAX];
	InstCount = 0;						
	instEspCount = 1;		

	//准备micode数组
	std::cout << "#正在加载： " << "in.mic" << endl;
	ifstream micode("in.mic");
	string tmpS;
	while(!micode.eof()){
		getline(micode,tmpS);//读取一行中间代码
		if(tmpS != "")
			setVCode(tmpS,micodeInsts + InstCount);
		InstCount++;
	}
	micode.close();
	std::cout << "#中间代码加载完成.\n" << endl;

	//开始运行指令
	std::cout<<"#程序开始执行:" << endl;
	progRun();

	//撤销资源
	free(myMemory);
	micodeInsts = NULL;
	myMemory = NULL;
	std::cout << "资源已释放." << endl;
	std::cout << "虚拟机退出成功.\n" << endl;
	std::system("pause");
	return 0;
}
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

//ȫ�־�̬
static Memory* myMemory;							//�ڴ��,��ʼ���ڴ�ΪMAX_MEMORY��10000��
static Frame* myFrame;								//��
static VCode* micodeInsts;							//ָ��ϣ����1000��ָ��
static int InstCount;								//ָ����,��ָ��ƫ����
static int instEspCount;							//��¼����ָ���У�

/***********************************************************************
���ͼ�������
1-4��	�����߼����㣨�Ӽ��˳�����������������ջ��
5-10��	���й�ϵ���㣬true��ջ��Ϊ1��false��ջ��Ϊ0
************************************************************************/
void errorHandle(string message,int level = 0){
    string levelString = "";
	level == 1 ? levelString = "WARNING!" : levelString = "ERROR!";
	std::cout<<levelString<<endl<<"ִ�е��� ";
	std::cout<<instEspCount == 0 ? instEspCount+"" : "??" ;
	std::cout<<" ��ʱ: " + message<<endl;
}
//��װmyFrame->movePtr()����ͳһ�����쳣��Ϣ
bool moveMyPagePtr(int c){
	myFrame->movePtr(c);//ebpû�䣬esp����4*c��maxMemoryҲ����������ô��
	if(myFrame->msgOfPage == 1){
		errorHandle("��ջ���...");
		return 0;	//ʧ��
	}
	return 1;		//�ɹ�
}
//��װmyFrame->pat(),��ͳһ�����쳣��Ϣ
void* patMyPage(int i,void * tmpebp){//��λֵ+2��ebp
	void* tmpSavePagePtr = myFrame->pat(i,tmpebp);
	if(myFrame->msgOfPage == 2){
		errorHandle("���ʵ�ַԽ��...");
		return NULL;
	}
	return tmpSavePagePtr;
}

//ͨ��ָ��ƫ�ƻ�ȡ������ַ
void* GetLocator(string arg)
{
	int locator = atoi(arg.substr(1).c_str());
	if (arg[0] == 'I' || arg[0] == 'R')				//���������
	{
		if (atoi(arg.substr(1).c_str()) >= 0)	//�ֲ�����
			return (int*)myFrame->ebp + 1 + atoi(arg.substr(1).c_str());
		else
			return (int*)myFrame->ebp - 1 + atoi(arg.substr(1).c_str());
	}
	else if (arg[0] == '@')			//ջ��ֵ
		return myFrame->esp;
	else							//Ϊ��ֵ
		return NULL;
}

/***********************************************************************
���ͼ�������
1-4��	�����߼����㣨�Ӽ��˳�����������������ջ��
5-10��	���й�ϵ���㣬true��ջ��Ϊ1��false��ջ��Ϊ0
************************************************************************/
void ManipulateArg_Int(string arg)
{
	if (arg[0] != '@')//��Ϊ��ʱ����
	{
		//myFrame->movePtr(1);
		int pNum;
		if (arg[0] == 'I')	//Ϊ����
		{
			void* locator = GetLocator(arg);
			pNum = myFrame->getInt(locator);
		}
		else				//Ϊ����
			pNum = atoi(arg.c_str());
		myFrame->setInt(pNum);
	}
	//else //��Ϊ��ʱ�������账��
}
bool IntCalculator(int oprNum, string arg1, string arg2)
{
	ManipulateArg_Int(arg1);
	myFrame->movePtr(1);
	ManipulateArg_Int(arg2);
	int op_a = myFrame->getInt(myFrame->esp);//�ڶ�������arg2
	myFrame->movePtr(-1);
	int op_b = myFrame->getInt(myFrame->esp);//��һ������arg1

	//����ջ���ʹ�ջ��ֵ
	switch(oprNum){
	case 1:
		myFrame->setInt(op_b + op_a);break;
	case 2:
		myFrame->setInt(op_b - op_a);break;
	case 3:
		myFrame->setInt(op_b * op_a);break;
	case 4:
		if(op_a == 0){errorHandle("����Ϊ0��");return 0;}
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
�����������
12-15��	�����߼����㣨�Ӽ��˳�����������������ջ��
16-21��	���й�ϵ���㣬true��ջ��Ϊ1��false��ջ��Ϊ0
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
	//else //��Ϊ��ʱ�������账��
}
bool RealCalculator(int oprNum, string arg1, string arg2){
	ManipulateArg_Real(arg1);
	ManipulateArg_Real(arg2);
	float op_a = myFrame->getReal(myFrame->esp);
	myFrame->movePtr(-1);
	float op_b = myFrame->getReal(myFrame->esp); 

	//����ջ���ʹ�ջ��ֵ
	switch(oprNum){
	case 12:
		myFrame->setReal(op_b + op_a);break;
	case 13:
		myFrame->setReal(op_b - op_a);break;
	case 14:
		myFrame->setReal(op_b * op_a);break;
	case 15:
		if(op_a == 0){errorHandle("����Ϊ0��");return 0;}
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
	//���ֵ
	void* locator1 = GetLocator(base);
	if (locator1 == NULL)//Ϊ��ֵ
	{
		//myFrame->movePtr(1);
		if (isRealNum(base))
			myFrame->setReal((float)atof(base.c_str()));
		else
			myFrame->setInt(atoi(base.c_str()));
	}
	else //Ϊ����
	{
		//Ϊ�������������ƫ�Ƶ�ַ
		if (off != "-")
		{
			int index = 0;
			if (off[0] == 'I')	//�±�Ϊ����
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
		for(VCode* instEsp = (micodeInsts + instEspCount - 1)//��ǰָ��
			;instEspCount <= InstCount
			;instEspCount++ && (instEsp = (micodeInsts + instEspCount - 1)))//����ǰָ����(��1��ʼ��)С����ָ����
		{																	//��ȡ��ǰָ��instEsp
			if(instEsp->op == "CAL"){
				//���غ���ִ�е�ָ���У���CAL����һ��
				int RetInstEspCount = instEspCount+1;	

				//�洢����ָ����
				//if(!moveMyPagePtr(1))goto progEnd;
				myFrame->setInt(RetInstEspCount);		

				//��ʼ��֡, �洢��������ǰ��������%ebp
				if(!moveMyPagePtr(1))goto progEnd;
				myFrame->setInt((int)myFrame->ebp);	
				myFrame->ebp = myFrame->esp;
				moveMyPagePtr(1);

				instEspCount = atoi(instEsp->arg2.c_str()) - 1;//��ת��������ʼ�У�-1��Ϊforѭ�����Ὣ��++
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
				myFrame->init(myFrame->ebp, 0, atoi(instEsp->arg2.c_str())*4);//��(ebp + 1)��ʼ������һ���ڴ沢��������
				moveMyPagePtr(1);
				continue;
			}
			else if(instEsp->op == "RET"){
				//�з���ֵ����Ҫ���ص�������esp��
				if (instEsp->arg2 != "-")
					SetEspNum(instEsp->arg2, instEsp->result);

				//��ȡ������ebp���洢�ķ����к�
				void* tmpVoidPtr;
				if((tmpVoidPtr = patMyPage(-1, myFrame->ebp)) == NULL) goto progEnd;
				instEspCount = (myFrame->getInt(tmpVoidPtr))-1;	

				//�ݴ汾����%esp
				void* tmpEsp = myFrame->esp;

				//�л��ظ�����esp������esp������һ��������
				myFrame->esp = (int*)myFrame->ebp - 1;
				if(!moveMyPagePtr(0 - atoi(instEsp->arg1.c_str())))goto progEnd;

				//�з���ֵ����д�ط���ֵ
				if (instEsp->arg2 != "-")
					memcpy(myFrame->esp, tmpEsp, 4);

				//��ȡ������%ebp���л���ȥ
				myFrame->ebp = (void*)myFrame->getInt(myFrame->ebp);

				continue;
			}
			else if(instEsp->op == "JMP"){
				instEspCount = atoi(instEsp->arg2.c_str()) - 1;//-1����Ϊ�´�ѭ����ʼҪ++
				continue;
			}
			else if(instEsp->op == "JPC"){
				//��ջ��ֵΪ0����ʾfalse����ת
				if (myFrame->getInt(myFrame->esp) == 0)
					instEspCount = atoi(instEsp->arg2.c_str()) - 1;//-1����Ϊ�´�ѭ����ʼҪ++
				continue;
			}
			else if(instEsp->op == "ITR"){
				//��ջ��Ԫ����intת��Ϊreal
				SetEspNum(instEsp->arg1, "-");
				int tmpInt = myFrame->getInt(myFrame->esp);
				float tmpReal = (float)tmpInt;
				myFrame->setReal(tmpReal);
			}
			else if (instEsp->op == "RED")
			{
				//����
				std::cout<<"<- ";
				string inString;
				endTime = time(NULL);
				runTime += difftime(endTime,startTime);

				//�ȴ�����
				startTime = time(NULL);
				cin >> inString;
				endTime = time(NULL);
				waitTime += difftime(endTime,startTime);

				//�ָ�����
				startTime = time(NULL);
				void* locator = GetLocator(instEsp->arg2);
				//Ϊ�������������ƫ�Ƶ�ַ
				if (instEsp->arg1 != "-")
				{
					int index = 0;
					if (instEsp->arg1[0] == 'I')	//�±�Ϊ����
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
				//���и�ֵ
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
				//����ֵ�ı���
				void* locator1 = GetLocator(instEsp->arg1);
				if (instEsp->result != "-")			//���鸳ֵ
				{
					int index = 0;
					if (instEsp->result[0] == 'I')	//���±�Ϊ����
						index = myFrame->getInt(GetLocator(instEsp->result));
					else
						index = atoi(instEsp->result.c_str());
					locator1 = (int*)locator1 + index;
				}

				if (instEsp->arg2[0] != '@')
					SetEspNum(instEsp->arg2, "-");
					
				//���и�ֵ
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
	    std::cout << "#�������н���.\n" << endl;
	    std::cout << "#����������ʱ " << runTime << " ��." << endl;
	    std::cout << "#�������ʱ�� " << waitTime << " ��." << endl;
		std::cout << "#��ջ���ʹ���� " << myFrame->getMaxMem() << " �ֽ�" << endl;
	}catch(...){
		std::cout << "#�м�����쳣." << endl;
    }
    progEnd:
	delete [] micodeInsts;
	std::cout<<endl;
	myFrame->refresh();
	return true;
}

extern "C" __declspec(dllexport) int __stdcall main(){
	myMemory = new Memory(MAX_MEMORY);
	std::cout << "#�������ʼ����..." << endl;
	myFrame = new Frame(0,9000,myMemory);
	std::cout << "#�������ʼ�����..." << endl;

	//��ʼ����Ϣ
	micodeInsts = new VCode[INST_MAX];
	InstCount = 0;						
	instEspCount = 1;		

	//׼��micode����
	std::cout << "#���ڼ��أ� " << "in.mic" << endl;
	ifstream micode("in.mic");
	string tmpS;
	while(!micode.eof()){
		getline(micode,tmpS);//��ȡһ���м����
		if(tmpS != "")
			setVCode(tmpS,micodeInsts + InstCount);
		InstCount++;
	}
	micode.close();
	std::cout << "#�м����������.\n" << endl;

	//��ʼ����ָ��
	std::cout<<"#����ʼִ��:" << endl;
	progRun();

	//������Դ
	free(myMemory);
	micodeInsts = NULL;
	myMemory = NULL;
	std::cout << "��Դ���ͷ�." << endl;
	std::cout << "������˳��ɹ�.\n" << endl;
	std::system("pause");
	return 0;
}
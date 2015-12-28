#include <string>
#include <sstream>
#include <limits>
using namespace std;

//指令结构体
struct VCode{
	string op;
	string arg1;
	string arg2;
	string result;
};

bool isRealNum(string sample){
	size_t found;
	if((found = sample.find(".")) == string::npos)
	    return false;	//int
	return true;		//real
}

bool isRealVar(string sample){
	if(sample.find("R") == string::npos)
		return false;	//int
	return true;		//real
}

bool isReal(string sample){
	return isRealVar(sample) || isRealNum(sample);
}

int string2Int(string sample){
	istringstream s(sample);
	int tmpInt;
	s>>tmpInt;
	s.clear();
	return tmpInt; 
}

float string2Float(string sample){
	istringstream s(sample);
	float tmpFloat;
	s>>tmpFloat;
	s.clear();
	return tmpFloat;
}

int GetPos(string arg){
	if (arg == "-")		//未使用该字段
		return INT_MIN;
	else{				//使用了该字段
		int beginIndex = 0;
		if (arg[0] == 'V')		//普通变量
			beginIndex = 0;
		else if (arg[0] == '@')	//临时变量
		{
			if (arg[1] == 'R')	//real临时变量
				beginIndex = 2;
			else
				beginIndex = 1;
		}
		else if (arg == "-")	//未使用该字段
			beginIndex = INT_MIN;
		else					//数值
			beginIndex = 0;
		return atoi(arg.substr(beginIndex).c_str());
	}
}

VCode* setVCode(string line, VCode* midcode){
	char* s = new char[line.length() + 1];//C风格字符串
	strcpy(s, line.c_str());
	char* token = strtok(s, "\t");		//OP
	midcode->op = token;				
	token = strtok(NULL,"\t");			//ARG1
	midcode->arg1 = token;
	token = strtok(NULL, "\t");			//ARG2
	midcode->arg2 = token;
	token = strtok(NULL, "\t");			//RESULT
	midcode->result = token;
	return midcode;
}
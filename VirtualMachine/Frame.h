#include "Memory.h"
#include <iostream>
#include <string>
#pragma once

class Frame
{
public:
	void* belowBounder;
	void* aboveBounder;
	void* ebp;
	void* esp;
	int msgOfPage;
	void* maxMemory;

	Frame(int,int,Memory*);
	~Frame(void);
	bool movePtr(int);
	void setReal(float);
	void setInt(int);
	float getReal(void*);
	int getInt(void*);
	void* pat(int,void*);
	bool init(void*,int,int);
	bool refresh();
	int getMaxMem();
};

Frame::Frame(int ad,int bd,Memory* myMemory)//ad为初始段地址，bd为段大小，myMemory为申请的内存对象
{
	belowBounder = myMemory->require(ad,bd);				//段起始地址
	aboveBounder = (void*)((int*)belowBounder + bd/4);		//段终止地址
	refresh();
}
Frame::~Frame(void){}

bool Frame::refresh(){//将该段恢复初始化
	ebp = maxMemory = esp = belowBounder;
	msgOfPage = 0;
	return true;
}
bool Frame::movePtr(int a){
	msgOfPage = 0;
	//移动4个字节
	if(a == 0)return true;
	int* b = (int*) esp;//存储esp现在存储的地址位置
	if((int*)belowBounder - b > a  || (int*)aboveBounder - b < a ){
	   /*std::cout<<"访问地址越界..."<<endl;*/
	   msgOfPage = 1;
	   return false;
	}
	b = b + a;//加a个四字节，因为是int类型的指针，一个偏移就是四个字节
	esp = (void*)b;
	if(esp >= maxMemory)maxMemory = esp;
	return true;
}
void Frame::setReal(float a){
    *(float*)esp = a;
}
void Frame::setInt(int a){
	*(int*)esp = a;//将esp变量中存放的地址 所指向的内存 存放的值 改为am
}
float Frame::getReal(void* p){
	return *(float*)p;
}
int Frame::getInt(void* p){
    return *(int*)p;
}
void* Frame::pat(int i,void * tmpebp){//相对定位值，ebp
	msgOfPage = 0;
	int* tmpIntPtr = (int*)tmpebp;//保存ebp
	void* patResult = (void*)(tmpIntPtr + i);
	if(patResult < belowBounder  || patResult > aboveBounder ){
	   msgOfPage = 2;
	   return NULL;
	}
	return patResult;
}
bool Frame::init(void* buffer,int c,int count){//esp，0，分配的内存大小(字节)
	this->movePtr(count/4);
	if(msgOfPage != 1)
		memset((void*)(((int*)buffer)+1),c,count);//新分配count大小的区域，并清零
	return 1;
}
int Frame::getMaxMem(){
	return ((int*)maxMemory - (int*)belowBounder + 1)*4;
}



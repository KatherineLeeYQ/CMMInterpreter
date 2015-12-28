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

Frame::Frame(int ad,int bd,Memory* myMemory)//adΪ��ʼ�ε�ַ��bdΪ�δ�С��myMemoryΪ������ڴ����
{
	belowBounder = myMemory->require(ad,bd);				//����ʼ��ַ
	aboveBounder = (void*)((int*)belowBounder + bd/4);		//����ֹ��ַ
	refresh();
}
Frame::~Frame(void){}

bool Frame::refresh(){//���öλָ���ʼ��
	ebp = maxMemory = esp = belowBounder;
	msgOfPage = 0;
	return true;
}
bool Frame::movePtr(int a){
	msgOfPage = 0;
	//�ƶ�4���ֽ�
	if(a == 0)return true;
	int* b = (int*) esp;//�洢esp���ڴ洢�ĵ�ַλ��
	if((int*)belowBounder - b > a  || (int*)aboveBounder - b < a ){
	   /*std::cout<<"���ʵ�ַԽ��..."<<endl;*/
	   msgOfPage = 1;
	   return false;
	}
	b = b + a;//��a�����ֽڣ���Ϊ��int���͵�ָ�룬һ��ƫ�ƾ����ĸ��ֽ�
	esp = (void*)b;
	if(esp >= maxMemory)maxMemory = esp;
	return true;
}
void Frame::setReal(float a){
    *(float*)esp = a;
}
void Frame::setInt(int a){
	*(int*)esp = a;//��esp�����д�ŵĵ�ַ ��ָ����ڴ� ��ŵ�ֵ ��Ϊam
}
float Frame::getReal(void* p){
	return *(float*)p;
}
int Frame::getInt(void* p){
    return *(int*)p;
}
void* Frame::pat(int i,void * tmpebp){//��Զ�λֵ��ebp
	msgOfPage = 0;
	int* tmpIntPtr = (int*)tmpebp;//����ebp
	void* patResult = (void*)(tmpIntPtr + i);
	if(patResult < belowBounder  || patResult > aboveBounder ){
	   msgOfPage = 2;
	   return NULL;
	}
	return patResult;
}
bool Frame::init(void* buffer,int c,int count){//esp��0��������ڴ��С(�ֽ�)
	this->movePtr(count/4);
	if(msgOfPage != 1)
		memset((void*)(((int*)buffer)+1),c,count);//�·���count��С�����򣬲�����
	return 1;
}
int Frame::getMaxMem(){
	return ((int*)maxMemory - (int*)belowBounder + 1)*4;
}



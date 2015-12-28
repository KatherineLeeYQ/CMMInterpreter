#include <stdlib.h>
#include <iostream>
#define DATATYPE_SIZE 4
#define MAX_MEMORY 10000
using namespace std;
#pragma once

class Memory
{
public:
    int _size;
	void* m_base;
	void* require(int,int);
	Memory(int);
	~Memory(void);
};

Memory::Memory(int tmp)
{
	_size = tmp;
	m_base = NULL;
	if((m_base = malloc(_size))== NULL){//根据所申请的内存大小MAX_MEMORY（100000）开辟空间
	    std::cout << "malloc failed..."<<endl;
		system("pause");
		exit(0);
	}
}
Memory::~Memory(void)
{
	free(m_base);
}

void* Memory::require(int start,int rsize){
	//判断是否溢出之类的
	int* intPtr = (int*)m_base;		//构造函数中申请内存后返回的地址基址
	return (void*)(intPtr+start);	//偏移start后的地址
}

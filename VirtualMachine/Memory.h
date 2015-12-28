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
	if((m_base = malloc(_size))== NULL){//������������ڴ��СMAX_MEMORY��100000�����ٿռ�
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
	//�ж��Ƿ����֮���
	int* intPtr = (int*)m_base;		//���캯���������ڴ�󷵻صĵ�ַ��ַ
	return (void*)(intPtr+start);	//ƫ��start��ĵ�ַ
}

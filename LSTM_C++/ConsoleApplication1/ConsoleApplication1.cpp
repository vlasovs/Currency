// ConsoleApplication1.cpp : Этот файл содержит функцию "main". Здесь начинается и заканчивается выполнение программы.
//

#include <iostream>

#include <windows.h>
#include <fstream>
#include <string>
#include <sysinfoapi.h>

#include "Matrix.h"
#include "Neuron.h"

//---------------------------------------------------------------------------
using namespace std;
const int look_back = 3;

_SYSTEM_INFO si;
int NumberOfProc;
NN* nn;
vector<vector<double> > tests;

void Init(void){	
	GetSystemInfo(&si);
	NumberOfProc = si.dwNumberOfProcessors;
	//String s=L"NumberOfProc: "+IntToStr(NumberOfProc);
	//ShowMessage(s);
	nn = new NN;
	nn->Load("NN.txt");
}
//---------------------------------------------------------------------------

void ParseLine(string line, double& start, double& min, double& max, double& finish) {
	string s;
	int i = 1;
	while (line[i] != '\"') i++;
	i += 2;
	int t = i;
	while (line[i] != ',') i++;
	s = line.substr(t, i - t);
	start = atof(s.c_str());
	i++;
	t = i;
	while (line[i] != ',') i++;
	s = line.substr(t, i - t);
	min = atof(s.c_str());
	i++;
	t = i;
	while (line[i] != ',') i++;
	s = line.substr(t, i - t);
	max = atof(s.c_str());
	i++;
	t = i;
	i = line.length();
	s = line.substr(t, i - t);
	finish = atof(s.c_str());
}

void __fastcall loadTests(int count)
{
	tests.clear();
	vector<double> records;	
	for (int i = 0; i < count; i++) {
		records.clear();
		string s;
		char buf[100];
		_itoa_s(i + 1, buf, 10);
		s = buf;
		s = "Result" + s + ".txt";
		string line;
		ifstream f;
		f.open(s.c_str());
		getline(f, line);
		while (getline(f, line)) {
			double start1, min1, max1, finish1;
			ParseLine(line, start1, min1, max1, finish1);
			records.push_back(start1);
			records.push_back(min1);
			records.push_back(max1);
			records.push_back(finish1);
		}
		f.close();
		double x2, value;
		value = x2 = records[0];
		for (int i = 0; i < records.size() / 4; i++) {
			double x1 = records[4 * i];
			records[4 * i] -= x2;
			records[4 * i + 1] -= x2;
			records[4 * i + 2] -= x2;
			records[4 * i + 3] -= x2;
			x2 = x1;
		}
		//vector<double> test(look_back * 4 + 1);
		//int k = records.size() / 4 - look_back;
		int k = records.size() / 4;
		for (int i = 1; i < k; i++) {
			int c = 0;
			vector<double> test(i * 4 + 1);
			for (int j = 0; j < i; j++) {

				//for (int j = 0; j < look_back; j++)	{
					//test[c++] = (records[4*(i + j)] - minG) / (maxG - minG);
					//test[c++] = (records[4*(i + j) + 1] - minG) / (maxG - minG);
					//test[c++] = (records[4*(i + j) + 2] - minG) / (maxG - minG);
					//test[c++] = (records[4*(i + j) + 3] - minG) / (maxG - minG);

					//test[c++] = (records[4*(j)] - minG) / (maxG - minG);
					//test[c++] = (records[4*(j) + 1] - minG) / (maxG - minG);
					//test[c++] = (records[4*(j) + 2] - minG) / (maxG - minG);
					//test[c++] = (records[4*(j) + 3] - minG) / (maxG - minG);

				test[c++] = records[4 * (j)];
				test[c++] = records[4 * (j)+1];
				test[c++] = records[4 * (j)+2];
				test[c++] = records[4 * (j)+3];

			}
			//test[c++] = (records[4*(i) + 3] - minG) / (maxG - minG);
			test[c++] = records[4 * (i)+3];
			//test[c++] = (records[4*(i + look_back) + 3] - minG) / (maxG - minG);
			tests.push_back(test);
		}
	}	
	/*
	for (int i = 0; i < tests.size(); i++) {
		for (int j = 0; j < tests[i].size(); j++) {
			cout << tests[i][j] << " ";
		}
		cout << endl;
	}*/
}

//---------------------------------------------------------------------------

void shuffle(vector<int>& arr) {
	for (int i = arr.size() - 1; i >= 1; i--)
	{
		int j = rand() % (i + 1);
		int tmp = arr[j];
		arr[j] = arr[i];
		arr[i] = tmp;
	}
}

//---------------------------------------------------------------------------

void conjugate_gradient(double dropout, int tcount, int ts)
{
	int size = 4 * look_back;
	double Alpha = 1.0 / 3.0;
	double Step = 0.001;
	int tt = tests.size();
	if (ts > tt) {
		ts = tt;
	}
	double* delta = new double[tt];
	double* enter = new double[4];
	double* ans = new double[1];
	//nn->ApplyDropout(dropout);
	for (int t = 0; t < tcount; t++) {
		nn->FreeDelta();
		nn->ApplyDropout(dropout);
		vector<int> x(tt);
		for (int i = 0; i < tt; i++) {
			x[i] = i;
		}
		shuffle(x);
		for (int i = 0; i < tt; i++) {
			int index = x[i];
			nn->Clear();
			size = tests[index].size() - 1;
			int tc = tests[index].size() / 4;
			for (int k = 0; k < tc; k++) {
				//for (int k = 0; k < look_back; k++) {
				for (int j = 0; j < 4; j++) {
					enter[j] = tests[index][k * 4 + j];
				}
				nn->Execute(enter);
			}
			ans[0] = tests[index][size];
			delta[i] = ans[0];
			delta[i] -= nn->GetAnswer(tc - 1)[0];
			nn->ClearDeltaSigma();
			//nn->Backpropagation(delta + i, nn->ExtractLayer(nn->LayersCount() - 1)->GetEntersCount() - 1, 0);
			nn->CalcNumericalDelta(tests[index], ans, Alpha);
			nn->CalcDelta(Alpha);
			nn->AplyError(Step);
		}
		cout << NormL2_(delta, tt) << "\n" << NormC_(delta, tt) << "\n" << (t + 1) << " \n" << " \n";
	}
	delete[] delta;
	delete[] enter;
	delete[] ans;
}
//---------------------------------------------------------------------------

void clearNN(void)
{
	nn->Fill(-1);
}

//---------------------------------------------------------------------------

void saveNN(void)
{
	nn->Save("NN.txt");
}

//---------------------------------------------------------------------------

void Levenberg_Marquardt(double dropout, int Iteration, int tests_)
{
	/*
		nn->Load("LSTM.txt");
		double enter2[2]={1,2};
		nn->Clear();
		nn->Execute(enter2,0.5);
		double enter3[2]={0.5,3};
		nn->Execute(enter3,1.25);
		double Gradient[200];
		nn->GetGradient(Gradient,0);
		return; */
	long long t = GetTickCount64();	 
	int Amount = nn->GetCoeffAmount();
	int k = Amount;
	double Lambda = 10.0;
	int size = 4 * look_back;		
	double* Jacobi;
	int tt = tests.size();	
	if (tests_ > tt) {
		tests_ = tt;		
	}
	int mt = tests_ * k;
	CreateMatrix(Jacobi, tests_, k);
	double* JacobiT;
	CreateMatrix(JacobiT, k, tests_);
	double* ans;
	CreateMatrix(ans, tests_, 1);
	double* enter = new double[size];
	double* err = new double[k];
	double* M;
	CreateMatrix(M, k, k);
	double* M2;
	CreateMatrix(M2, k, k);
	double* D1;
	CreateMatrix(D1, k, 1);
	double* B;
	CreateMatrix(B, 1, k);
	int* p = new int[k];
	double* Delta = new double[k];
	double* vect1;
	CreateMatrix(vect1, 1, tests_);
	//nn->ApplyDropout(dropout);
	for (int t = 0; t < Iteration; t++) {
		nn->ApplyDropout(dropout);
		k = Amount - nn->DropCount();
		if (k == 0) continue;
		vector<int> x(tt);
		for (int i = 0; i < tt; i++) {
			x[i] = i;
		}
		shuffle(x);
		for (int i = 0; i < tests_; i++) {
			int index = x[i];
			nn->Clear();
			size = tests[index].size() - 1;
			int tc = tests[index].size() / 4;
			for (int p = 0; p < tc; p++) {
				//for (int k = 0; k < look_back; k++) {
				for (int j = 0; j < 4; j++) {
					enter[j] = tests[index][p * 4 + j];
				}
				nn->Execute(enter);
			}
			double r = nn->GetAnswer(tc - 1)[0];
			ans[i] = tests[index][size] - r;
			//nn->GetGradient(Jacobi,i*k);
			nn->ClearDeltaSigma();
			nn->Backpropagation(0, nn->ExtractLayer(nn->LayersCount() - 1)->GetEntersCount() - 1, 0, 0);
			nn->GetGradientWithout(Jacobi, i * k);
			//nn->GetNumericalGradient(tests[i],Jacobi,i*k);
		}
		/*
		Memo1->Lines->Clear();
		for (int i = 0; i < tests.size(); i++) {
			String s="";
			for (int j = 0; j < k; j++) {
				s+=FloatToStr(Jacobi[i*k+j])+" ";
			}
			Memo1->Lines->Add(s);
		}
		Memo1->Lines->Add("");
		*/
		Transpose(Jacobi, JacobiT, tests_, k);
		//Mul(JacobiT, Jacobi, M1, k, k, tests.size());
		//NumberOfProc
		Mul_Threading(JacobiT, Jacobi, M, k, k, tests_, NumberOfProc);
		for (int i = 0; i < k; i++)
		{
			M[i * k + i] *= (1 + Lambda);
		}
		Mul(JacobiT, ans, D1, k, 1, tests_);
		Transpose(D1, B, k, 1);
		//GetLU(M, p, k, k);
		Copy(M, M2, k, k);
		GetLU_Threading(M, p, k, NumberOfProc);
		/*
		Memo1->Lines->Clear();
		for (int i = 0; i < k; i++) {
			String s="";
			for (int j = 0; j < k; j++) {
				s+=FloatToStr(M[i*k+j])+" ";
			}
			Memo1->Lines->Add(s);
		}
		*/
		GetAnswer(M, p, B, Delta, k);
		GetError(M2, B, Delta, err, k);
		double error = NormC_(err, k);
		if (error > 1.0) {
			continue;
		}
		nn->SetDifferenceWithout(Delta);
		double nc = NormC_(Delta, k);
		double step = 1 / nc;
		if (step > 1) step = 1;
		//nn->AplyError(0.01);
		nn->AplyError(step);
		//nn->AplyError(1.0);
		Transpose(ans, vect1, tests_, 1);
		cout<<"\n" << k << "\n" << NormL2_(vect1, tests_) << " " << NormC_(vect1, tests_) << "\n" << NormL2_(Delta, k) << " " << NormC_(Delta, k) << "\n" <<
			error << "\n" << (t + 1) << " \n" << " \n";
	}	
	DestroyMatrix(Jacobi, tests_, k);
	DestroyMatrix(JacobiT, k, tests_);
	DestroyMatrix(ans, tests_, 1);
	DestroyMatrix(M2, k, k);
	delete[] enter;
	//DestroyMatrix(M1, k, k);
	DestroyMatrix(M, k, k);
	DestroyMatrix(D1, k, 1);
	DestroyMatrix(B, 1, k);
	delete[] p;
	delete[] Delta;
	delete[] err;

	DestroyMatrix(vect1, 1, tests_);

	t = GetTickCount64() - t;

	cout<< "\n" << t << " ms\n";

}
//---------------------------------------------------------------------------


int main()
{
	Init();
	//clearNN();
	//saveNN();
	int tc = 105;
	loadTests(tc);
	//loadTests(105);
	conjugate_gradient(0, 10000, tc);
	//Levenberg_Marquardt(0, 100, 100000);
	//saveNN();
    std::cout << "Hello World!\n";
	if (nn) delete nn;
}

// Запуск программы: CTRL+F5 или меню "Отладка" > "Запуск без отладки"
// Отладка программы: F5 или меню "Отладка" > "Запустить отладку"

// Советы по началу работы 
//   1. В окне обозревателя решений можно добавлять файлы и управлять ими.
//   2. В окне Team Explorer можно подключиться к системе управления версиями.
//   3. В окне "Выходные данные" можно просматривать выходные данные сборки и другие сообщения.
//   4. В окне "Список ошибок" можно просматривать ошибки.
//   5. Последовательно выберите пункты меню "Проект" > "Добавить новый элемент", чтобы создать файлы кода, или "Проект" > "Добавить существующий элемент", чтобы добавить в проект существующие файлы кода.
//   6. Чтобы снова открыть этот проект позже, выберите пункты меню "Файл" > "Открыть" > "Проект" и выберите SLN-файл.

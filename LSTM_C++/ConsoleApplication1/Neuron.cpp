//---------------------------------------------------------------------------
#pragma hdrstop

#include "Neuron.h"

#include <io.h>
#include <iostream>
#include <sstream>
#include <fstream>

//---------------------------------------------------------------------------
#pragma package(smart_init)

double SigmaFunc(double Sigma) {
	if (Sigma > logl(1.0E300)) {
		return 1;
	}
	if (Sigma < -logl(1.0E300)) {
		return 0;
	}
	double res = 1.0 / (1.0 + exp(-Sigma));
	return res;
}

double TanhFunc(double Sigma) {
	if (Sigma > logl(1.0E300)) {
		return 1;
	}
	if (Sigma < -logl(1.0E300)) {
		return -1;
	}
	double e1 = exp(Sigma);
	double e2 = exp(-Sigma);
	double res = (e1 - e2) / (e1 + e2);
	return res;
}

string ReadWord(string& s) {
	int k;
	k = 0;
	s += " ";
	while (k + 1 < (int)s.length() && !((s[k] == ' ') && (s[k + 1] != ' ')))
		k++;
	string result = s.substr(0, k);
	s = s.substr(k + 1, s.length() - k - 1);
	return result;
}

void SwapComma(string& s) {
	for (int i = 0; i < s.length(); i++) {
		if (s[i] == ',')
			s[i] = '.';
	}
}

void SwapDot(string& s) {
	for (int i = 0; i < s.length(); i++) {
		if (s[i] == '.')
			s[i] = ',';
	}
}

void SwapComma(wstring& s) {
	for (int i = 0; i < s.length(); i++) {
		if (s[i] == L',')
			s[i] = L'.';
	}
}

void SwapDot(wstring& s) {
	for (int i = 0; i < s.length(); i++) {
		if (s[i] == L'.')
			s[i] = L',';
	}
}
/*
bool FileExists(const char *fname) {
	return access(fname, 0) != -1;
}
*/
Layer::Layer(void) : EnterCount(0), Amount(0), next(0), prev(0) {
}

const int Layer::flag = - (1 << 25);

string Layer::GetName(void) {
	return name;
}

void Layer::SetCoeff(double val, int i) {
	w[i] = val;
}

double Layer::GetCoeff(int i) {
	return w[i];
}

void Layer::SetDelta(double val, int count) {
	DeltaW[count] = val;
}

double Layer::GetDelta(int count) {
	return DeltaW[count];
}

Layer::~Layer(void) {
	if (EnterCount) {
		delete[] w;
		delete[] Dropout;
		delete[] DeltaW;		
		Clear();
	}
}

void Layer::SetNeighbors(Layer* next, Layer* prev) {
	this->next = next;
	this->prev = prev;
}

void Layer::SetEnter(const double* enter) {
	if (enter) {
		double* Enter = new double[EnterCount];
		for (int k = 0; k < EnterCount; k++) {
			Enter[k] = enter[k];
		}
		Enters.push_back(Enter);
		double* Out = new double[ExitCount];
		for (int k = 0; k < ExitCount; k++) {
			Out[k] = 0;
		}
		Outs.push_back(Out);		
	}
	double* DeltaSigma = new double[ExitCount];
	for (int i = 0; i < ExitCount; i++) {
		DeltaSigma[i] = 0;
	}
	wDeltaSigma.push_back(DeltaSigma);	
}

int Layer::GetCoeffAmount() {
	return Amount;
}

int Layer::GetEnterCount(void) {
	return EnterCount;
}

int Layer::GetNeuronCount(void) {
	return NeuronCount;
}

int Layer::GetExitCount(void) {
	return ExitCount;
}

void Layer::Fill(int flag, double Amplitude) {
	for (int k = 0; k < Amount; k++) {
		double f = 0;
		if (flag < 0)
			f = ((double)rand() / RAND_MAX * 2 - 1.0) * Amplitude;
		else if (flag == 0)
			f = 0;
		else if (flag == 1)
			f = 1;
		else if (flag == 2)
			f = 0.5;
		SetCoeff(f, k);
	}
}

double* Layer::GetAnswer(int stage) {
	return Outs[stage];
}

double* Layer::GetDeltaSigma(int stage) {
	return wDeltaSigma[stage];
}

double* Layer::GetDelta() {
	return DeltaW;
}

void Layer::FreeDelta(void) {
	if (DeltaW) {
		for (int k = 0; k < Amount; k++) {
			DeltaW[k] = 0;
		}
	}
	/*
	for (int i = 0; i < wDeltaSigma.size(); i++)
	{
		for (int k = 0; k < Amount; k++) {
			wDeltaSigma[i][k] = 0;
		}
	}*/
}

void Layer::SetDifference(const double* Delta) {
	for (int k = 0; k < Amount; k++) {
		SetDelta(Delta[k], k);
	}
}

void Layer::AplyError(double h) {
	for (int k = 0; k < Amount; k++) {
		double kof = GetCoeff(k);
		double d = GetDelta(k);
		kof = kof + h * d;
		SetCoeff(kof, k);
	}
}

void Layer::GetLayerInfo(int& n, int& m, double*& array) {
	array = w;
	n = ExitCount;
	m = Amount / n;
}

void Layer::ApplyDropout(double x) {
	int amount = GetCoeffAmount();
	int c = 0;
	for (int i = 0; i < amount; i++) {
		double r = (double)rand() / RAND_MAX;
		if (r < x) {
			Dropout[i] = true;
			c++;
		}
		else {
			Dropout[i] = false;
		}
	}
	pd = (double)c / amount;
	dropcount = c;

}

int Layer::DropCount(void) {
	return dropcount;
}

bool Layer::IsDrop(int count) {
	return Dropout[count];
}

void Layer::Clear(void) {
	while (!Enters.empty()) {
		delete[] Enters.back();
		Enters.pop_back();			
	}
	while (!Outs.empty()) {
		delete[] Outs.back();
		Outs.pop_back();
	}
	while (!wDeltaSigma.empty()) {
		delete[] wDeltaSigma.back();
		wDeltaSigma.pop_back();
	}
}

int Layer::GetEntersCount(void) {
	return Enters.size();
}

/*
int Layer::PrevisionCount(void) {
	int count = EnterCount;
	if (prev) {
		count += prev->PrevisionCount();
	}
	return count;
}

void Layer::GetPrevisionInfo(double* v, int offset) {
	for (int i = 0; i < EnterCount; i++) {	
		v[i + offset - EnterCount] = Enter[i];		
	}
	if (prev) {
		prev->GetPrevisionInfo(v,offset - EnterCount);
	}
}

void Layer::SetPrevisionInfo(double* v, int offset) {
	for (int i = 0; i < EnterCount; i++) {
		Enter[i] = v[i + offset - EnterCount];
	}
	for (int i = 0; i < ExitCount; i++) {
		Out[i] = v[i + offset];
	}
	if (prev) {
		prev->SetPrevisionInfo(v, offset - EnterCount);
	}
}
*/

Dense::Dense(void) : Layer() {
	name = "Dense";
}

void Dense::SetEnterCount(int EnterCount, int NeuronCount, int ExitCount) {
	this->EnterCount = EnterCount;
	this->NeuronCount = NeuronCount;
	this->ExitCount = ExitCount;

	Amount = NeuronCount * (EnterCount + 1);
	int a = Amount;
	w = new double[a];
	Dropout = new bool[a];
	DeltaW = new double[a];
	//Out = new double[ExitCount];
	//DeltaSigma = new double[ExitCount];
}

void Dense::Execute(double* Enter) {
	SetEnter(Enter);
	int count1 = 0;
	for (int i = 0; i < NeuronCount; i++) {
		double sum = 0;
		for (int e = 0; e < EnterCount; e++) {
			if (!Dropout[count1]) {
				sum += Enter[e] * w[count1++] / (1 - pd);
			}
			else {
				count1++;
			}
		}
		if (!Dropout[count1]) {
			sum += w[count1++] / (1 - pd);
		}
		else {
			count1++;
		}
		Outs.back()[i] = sum;
	}
	if (next) {
		next->Execute(Outs.back());
	}
}

double* Dense::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	for (int e = 0; e < NeuronCount; e++) {
		double sum = 0;
		double dfo = 1;
		if (ds) {
			dfo *= ds[e];
		}
		sum = dfo; //NeuronCount
		if (exit == flag) {
			wDeltaSigma[stage][e] += sum;
		}
		else {			
			wDeltaSigma[stage][e] += e == exit ? sum : 0;
		}
	}
	double* ds2 = new double[EnterCount];
	for (int k = 0; k < EnterCount; k++) {
		ds2[k] = 0;
		for (int n = 0; n < NeuronCount; n++) {
			double fs = wDeltaSigma[stage][n];
			double s = 0;
			if (!Dropout[n * (EnterCount + 1) + k]) {
				s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
			}
			ds2[k] += s;
		}
	}
	if (prev) {
		ds = prev->Backpropagation(ds2, stage, last_stage);
		delete[] ds2;
		ds2 = 0;
	}
	else {
		ds = ds2;
	}
	return ds;
}

void Dense::GetGradient(double* Gradient, int offset) {
	for (int i = 0; i < Amount; i++) {
		Gradient[offset + i] = 0;
	}
	for (int s = 0; s < wDeltaSigma.size(); s++) {
		for (int i = 0; i < NeuronCount; i++) {
			double dfo = 0;
			dfo = wDeltaSigma[s][i];
			for (int l = 0; l < EnterCount; l++) {
				int t = i * (EnterCount + 1) + l;
				//			if (!Dropout[t]) {
				Gradient[offset + t] += Enters[s][l] * dfo;
				//			}
			}
			int t = i * (EnterCount + 1) + EnterCount;
			//		if (!Dropout[t]) {
			Gradient[offset + t] += dfo;
			//		}
		}
	}
	for (int i = 0; i < Amount; i++) {
		if (Dropout[i]) {
			Gradient[offset + i] = 0;
		}
		else {
			Gradient[offset + i] /= 1 - pd;
		}
	}
}

void Dense::CalcDelta(double Alpha) {
	int count = 0;
	for (int i = 0; i < NeuronCount; i++) {
		for (int l = 0; l < EnterCount; l++) {
			if (!Dropout[count]) {
				double dfo = 0;
				for (int s = 0; s < wDeltaSigma.size(); s++) {
					dfo += Enters[s][l] * wDeltaSigma[s][i];
				}
				DeltaW[count] = Alpha * (DeltaW[count]) + dfo;
			}
			count++;
		}
		if (!Dropout[count]) {
			double dfo = 0;
			for (int s = 0; s < wDeltaSigma.size(); s++) {
				dfo += wDeltaSigma[s][i];
			}
			DeltaW[count] = Alpha * (DeltaW[count]) + dfo;
		}
		count++;
	}

}

void Dense::ClearDeltaSigma(void) {
	for (int s = 0; s < wDeltaSigma.size(); s++) {
		for (int i = 0; i < NeuronCount; i++) {
			wDeltaSigma[s][i] = 0;
		}
	}
}

DenseSoftmax::DenseSoftmax(void) : Dense() {
	name = "DenseSoftmax";
}

void DenseSoftmax::Execute(double* Enter) {
	SetEnter(Enter);
	int count1 = 0;
	double sum1 = 0;
	for (int i = 0; i < NeuronCount; i++) {
		double sum2 = 0;
		for (int e = 0; e < EnterCount; e++) {
			if (!Dropout[count1]) {
				sum2 += Enter[e] * w[count1++] / (1 - pd);
			}
			else {
				count1++;
			}
		}
		if (!Dropout[count1]) {
			sum2 += w[count1++] / (1 - pd);
		}
		else {
			count1++;
		}
		Outs.back()[i] = exp(sum2);
		sum1 += Outs.back()[i];
	}
	for (int i = 0; i < NeuronCount; i++) {
		Outs.back()[i] /= sum1;
	}
	if (next) {
		next->Execute(Outs.back());
	}
}

double* DenseSoftmax::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	for (int e = 0; e < NeuronCount; e++) {
		double sum = 0;
		double dfo;
		if (ds && exit == flag) {
			for (int q = 0; q < NeuronCount; q++) {
				double r = (e == q ? 1 : 0);
				dfo = Outs[stage][q] * (r - Outs[stage][e]);
				dfo *= ds[q];
				sum += dfo;//NeuronCount;
			}
		}
		else {
			double r = e == exit ? 1 : 0;
			dfo = Outs[stage][exit] * (r - Outs[stage][e]);
			sum += dfo;
		}
		wDeltaSigma[stage][e] += sum;
	}

	double* ds2 = new double[EnterCount];
	for (int k = 0; k < EnterCount; k++) {
		ds2[k] = 0;
		for (int n = 0; n < NeuronCount; n++) {
			double fs = wDeltaSigma[stage][n];
			double s = 0;
			if (!Dropout[n * (EnterCount + 1) + k]) {
				s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
			}
			ds2[k] += s;
		}
	}

	if (prev) {		
		ds = prev->Backpropagation(ds2, stage, last_stage);
		delete[] ds2;
		ds2 = 0;
	}
	else {
		ds = ds2;
	}

	return ds;
}

/********************************************************************/

DenseSigmoid::DenseSigmoid(void) : Dense() {
	name = "DenseSigmoid";
}

void DenseSigmoid::Execute(double* Enter) {
	SetEnter(Enter);
	int count1 = 0;
	for (int i = 0; i < NeuronCount; i++) {
		double sum2 = 0;
		for (int e = 0; e < EnterCount; e++) {
			if (!Dropout[count1]) {
				sum2 += Enter[e] * w[count1++] / (1 - pd);
			}
			else {
				count1++;
			}
		}
		if (!Dropout[count1]) {
			sum2 += w[count1++] / (1 - pd);
		}
		else {
			count1++;
		}
		Outs.back()[i] = SigmaFunc(sum2);
	}
	if (next) {
		next->Execute(Outs.back());
	}
}

double* DenseSigmoid::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	//int e=exit;
	for (int e = 0; e < NeuronCount; e++) {
		double sum = 0;
		double dfo = Outs[stage][e] * (1 - Outs[stage][e]);
		if (ds) {
			dfo *= ds[e];
		}
		if (exit == flag || exit == e) {
			wDeltaSigma[stage][e] = dfo;
		}
		else {
			wDeltaSigma[stage][e] = 0;
		}
	}

	double* ds2 = new double[EnterCount];
	for (int k = 0; k < EnterCount; k++) {
		ds2[k] = 0;
		for (int n = 0; n < NeuronCount; n++) {
			double fs = wDeltaSigma[stage][n];
			double s = 0;
			if (!Dropout[n * (EnterCount + 1) + k]) {
				s = fs * w[n * (EnterCount + 1) + k] / (1 - pd);
			}
			ds2[k] += s;
		}
	}

	if (prev) {
		ds = prev->Backpropagation(ds2, stage, last_stage);
		delete[] ds2;
		ds2 = 0;
	}
	else {
		ds = ds2;
	}

	return ds;
}

/********************************************************************/

LSTM::LSTM(void) :Layer(), Exit(0), C(0) {
	name = "LSTM";
	c1 = 0;
		de = 0;
		dc = 0;
		prevf = 0;
}

LSTM::~LSTM(void) {
	if (EnterCount) {
		Clear();
	}
}

void LSTM::Clear(void) {
	while (!Stack.empty()) {
		for (int i = 0; i < ExitCount; i++) {
			delete[] Sigma[i].back();
			delete[] DeltaSigmas[i].back();			
			Sigma[i].pop_back();
			DeltaSigmas[i].pop_back();
			Pipe[i].pop_back();
		}
		delete[] Stack.back();
		Stack.pop_back();		
	}
	while (!Enters.empty()) {
		delete[] Enters.back();
		Enters.pop_back();
	}
	while (!Outs.empty()) {
		delete[] Outs.back();
		Outs.pop_back();
	}
	while (!wDeltaSigma.empty()) {
		delete[] wDeltaSigma.back();
		wDeltaSigma.pop_back();
	}
	for (int i = 0; i < ExitCount; i++) {
		Exit[i] = 0;
		C[i] = 0;
	}
}

void LSTM::SetEnter(const double* enter) {
	double* Enter = new double[EnterCount];
	for (int k = 0; k < EnterCount; k++) {
		Enter[k] = enter[k];
	}
	Enters.push_back(Enter);
	double* Out = new double[ExitCount];
	for (int k = 0; k < ExitCount; k++) {
		Out[k] = 0;
	}
	Outs.push_back(Out);
}

void LSTM::SetEnterCount(int EnterC, int NeuronCount, int ExitCount) {
	this->ExitCount = ExitCount;
	EnterCount = EnterC;
	CoeffCount = (ExitCount / NeuronCount + EnterC + 1);
	this->NeuronCount = NeuronCount;
	Amount = 4 * CoeffCount * ExitCount;
	int a = Amount;
	w = new double[a];
	Dropout = new bool[a];
	DeltaW = new double[a];
	//DeltaSigma = new double[ExitCount];
	Sigma.resize(ExitCount);
	DeltaSigmas.resize(ExitCount);
	Pipe.resize(ExitCount);
	Exit.resize(ExitCount);
	C.resize(ExitCount);	
	//Outs.push_back(new double[ExitCount]);

	if (c1) {
		delete[] c1;
		delete[] de;
		delete[] dc;
		delete[] prevf;
	}

	c1 = new double[ExitCount];
	de = new double[ExitCount];
	dc = new double[ExitCount];
	prevf = new double[ExitCount];
}

int LSTM::NewStage(void) {
	Stack.push_back(new double[ExitCount + EnterCount]);
	for (int j = 0; j < ExitCount; j++) {
		Stack.back()[j] = Exit[j];
	}
	for (int i = 0; i < ExitCount; i++) {
		Sigma[i].push_back(new double[4]);
		DeltaSigmas[i].push_back(new double[4]);
		for (int j = 0; j < 4; j++) {
			Sigma[i].back()[j] = 0;
			DeltaSigmas[i].back()[j] = 0;
		}
		Pipe[i].push_back(C[i]);
	}	
	return 0;
}

void LSTM::Execute(double* Enter) {
	SetEnter(Enter);
	NewStage();
	int count1 = 0;
	int cur = Stack.size() - 1;
	for (int k = 0; k < EnterCount; k++) {
		Stack[cur][k + ExitCount] = Enter[k];
	}
	int tcount = ExitCount / NeuronCount;
	for (int i = 0; i < ExitCount; i++) {
		int group = i / tcount;
		for (int k = 0; k < 4; k++) {
			double sum = 0;
			for (int j = 0; j < tcount; j++) {
				int p = group * tcount + j;
				double e = this->Stack[cur][p];
				double ww = 0;
				if (!Dropout[count1]) {
					ww = w[count1++] / (1 - pd);
				}
				else {
					count1++;
				}
				sum += ww * e;
			}
			for (int j = 0; j < EnterCount; j++) {
				double e = this->Stack[cur][ExitCount + j];
				double ww = 0;
				if (!Dropout[count1]) {
					ww = w[count1++] / (1 - pd);
				}
				else {
					count1++;
				}
				sum += ww * e;
			}
			if (!Dropout[count1]) {
				sum += w[count1++] / (1 - pd);
			}
			else {
				count1++;
			}
			Sigma[i][cur][k] = sum;
		}
		double* ss = Sigma[i][cur];
		double s;
		s = ss[0];
		double s1 = SigmaFunc(s);
		s = ss[1];
		double s2 = SigmaFunc(s);
		s = ss[2];
		double t1 = TanhFunc(s);
		s = ss[3];
		double s3 = SigmaFunc(s);
		double c1 = s1 * C[i] + s2 * t1;
		double t2 = TanhFunc(c1);
		double e1 = s3 * t2;
		Exit[i] = e1;
		C[i] = c1;
	}
	for (int i = 0; i < ExitCount; i++) {
		Outs.back()[i] = Exit[i];
	}
	if (next) {
		next->Execute(Outs.back());
	}
}

double* LSTM::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	int count = ExitCount / NeuronCount;
	if (stage == Stack.size() - 1) {				
		for (int l = 0; l < ExitCount; l++) {
			c1[l] = C[l];
			if (ds) {
				de[l] = ds[l];
			}
			else {
				de[l] = 1;
			}
			dc[l] = 0;
			prevf[l] = 0;
		}
	} else if (stage == last_stage) {
		for (int l = 0; l < ExitCount; l++) {			
			de[l] += ds[l];
		}
	}

	for (int i = stage; i >= last_stage; i--)
	{
		for (int l = 0; l < ExitCount; l++) {
			double tc = TanhFunc(c1[l]);
			double _tc = 1 - tc * tc;
			double* ss = Sigma[l][i];
			double* dss = DeltaSigmas[l][i];
			double f = SigmaFunc(ss[0]); //f
			double _f = f * (1 - f);
			double q = SigmaFunc(ss[1]); //i
			double _q = q * (1 - q);
			double g = TanhFunc(ss[2]);  //g
			double _g = 1 - g * g;
			double o = SigmaFunc(ss[3]); //o
			double _o = o * (1 - o);
			dc[l] = de[l] * o * _tc + dc[l] * prevf[l];
			c1[l] = Pipe[l][i];
			double res1 = dc[l] * _f * c1[l];
			dss[0] += res1;
			double res2 = dc[l] * _q * g;
			dss[1] += res2;
			double res3 = dc[l] * _g * q;
			dss[2] += res3;
			double res4 = de[l] * _o * tc;
			dss[3] += res4;
			prevf[l] = f;
		}
		for (int p = 0; p < ExitCount; p++) {
			de[p] = 0;
			int number = p % count;
			int group = p / count;
			for (int l = 0; l < count; l++) {
				for (int k = 0; k < 4; k++) {
					int r = group * count + l;
					int t = (4 * r + k) * CoeffCount + number;
					if (!Dropout[t]) {
						de[p] += DeltaSigmas[r][i][k] * w[t] / (1 - pd);
					}
				}
			}
		}
	}
	if (prev) {
		double* ds2 = new double[EnterCount];
		int c = stage;
		//int i = c;
		for (int i = c; i >= last_stage; i--)
		{
			for (int e = 0; e < EnterCount; e++) {
				ds2[e] = 0;
				for (int l = 0; l < ExitCount; l++) {
					for (int k = 0; k < 4; k++) {
						double dss = DeltaSigmas[l][i][k];
						int t = (4 * l + k) * CoeffCount + e + count;
						if (!Dropout[t]) {
							ds2[e] += dss * w[t] / (1 - pd);
						}
					}
				}
			}
			double* tmp = prev->Backpropagation(ds2, i, i);
			if (tmp) delete[] tmp;
		}
		delete[] ds2;
	}
	return 0;
}

void LSTM::GetGradient(double* Gradient, int offset) {
	for (int i = 0; i < Amount; i++) {
		Gradient[offset + i] = 0;
	}
	int tcount = ExitCount / NeuronCount;
	for (int l = 0; l < ExitCount; l++) {
		double ds1, e;
		int group = l / tcount;
		for (int n = 0; n < Stack.size(); n++) {
			for (int k = 0; k < 4; k++) {
				ds1 = DeltaSigmas[l][n][k];
				double ed;
				for (int j = 0; j < tcount; j++) {
					int p = group * tcount + j;
					e = Stack[n][p];
					ed = e * ds1;
					int t = (4 * l + k) * CoeffCount + j;
					//					if (!Dropout[t]) {
					Gradient[offset + t] += ed;
					//					}
				}
				for (int i = 0; i < EnterCount; i++) {
					e = Stack[n][ExitCount + i];
					ed = e * ds1;
					int t = (4 * l + k) * CoeffCount + tcount + i;
					//					if (!Dropout[t]) {
					Gradient[offset + t] += ed;
					//					}
				}
				int t = (4 * l + k + 1) * CoeffCount - 1;
				//				if (!Dropout[t]) {
				Gradient[offset + t] += ds1;
				//				}
			}
		}
	}
	for (int i = 0; i < Amount; i++) {
		if (Dropout[i]) {
			Gradient[offset + i] = 0;
		}
		else {
			Gradient[offset + i] /= 1 - pd;
		}
	}
}

void LSTM::CalcDelta(double Alpha) {
	int index;
	double* Gradient = new double[Amount];
	for (int i = 0; i < Amount; i++) {
		Gradient[i] = 0;
	}
	GetGradient(Gradient, 0);
	for (int k = 0; k < Amount; k++) {
		if (!Dropout[k]) {
			DeltaW[k] = Alpha * DeltaW[k] + Gradient[k];
		}
	}
	delete[] Gradient;
}

void LSTM::ClearDeltaSigma(void) {
	for (int l = 0; l < ExitCount; l++) {
		for (int i = 0; i < Stack.size(); i++) {
			for (int k = 0; k < 4; k++) {
				DeltaSigmas[l][i][k] = 0;
			}
		}
	}
}

/********************************************************************/
// Embedding

Embedding::Embedding(void) :Layer() {
	name = "Embedding";
}

Embedding::~Embedding(void) {
	//Nothing
}

void Embedding::SetEnterCount(int EnterC, int NeuronC, int ExitC) {
	EnterCount = EnterC;
	NeuronCount = NeuronC;
	ExitCount = ExitC;
	//Enter = new double[1];
	int a = Amount = NeuronCount * ExitCount;
	w = new double[a];
	Dropout = new bool[a];
	DeltaW = new double[a];
	//DeltaSigma = new double[a];
	//Out = new double[ExitCount];
}

void Embedding::Execute(double* Enter) {
	SetEnter(Enter);
	for (int e = 0; e < EnterCount; e++) {
		for (int i = 0; i < ExitCount; i++) {
			int t = (int)Enter[e] * ExitCount + i;
			if (Dropout[t]) {
				Outs.back()[i] = 0;
			}
			else {
				Outs.back()[i] = w[t] / (1 - pd);
			}
		}
		if (next) {
			next->Execute(Outs.back());
		}
	}
}

double* Embedding::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	for (int e = 0; e < EnterCount; e++) {
		for (int i = 0; i < ExitCount; i++) {
			int t = (int)Enters[stage][e] * ExitCount + i;
			if (!Dropout[t]) {
				wDeltaSigma[stage][t] += ds[i];
			}
		}
	}
	return 0;
}

void Embedding::GetGradient(double* Gradient, int offset) {
	for (int i = 0; i < Amount; i++) {
		for (int s = 0; s < wDeltaSigma.size(); s++) {			
			double dfo = wDeltaSigma[s][i];
			if (!Dropout[i]) {
				Gradient[offset + i] += dfo;
			}
		}
	}
}

void Embedding::CalcDelta(double Alpha) {
	for (int i = 0; i < Amount; i++) {
		double dfo = 0;
		for (int s = 0; s < wDeltaSigma.size(); s++) {
			dfo += wDeltaSigma[s][i];
		}
		if (!Dropout[i]) {
			DeltaW[i] = Alpha * (DeltaW[i]) + dfo;
		}
	}
}

void Embedding::ClearDeltaSigma(void) {
	for (int i = 0; i < Amount; i++) {
		for (int s = 0; s < wDeltaSigma.size(); s++) {
			wDeltaSigma[s][i] = 0;
		}
	}
}

void Embedding::GetLayerInfo(int& n, int& m, double*& array) {
	array = w;
	n = NeuronCount;
	m = Amount / n;
}

// Adder

Adder::Adder(void) :Dense() {
	//Nothing
}

void Adder::SetCount(int Count){
	EnterCount = Count;
	NeuronCount = Count;
	ExitCount = Count;
	Amount = Count;	
}

void Adder::SetOffset(int offset, int count) {
	this->offset = offset;
	this->count = count;
}

double* Adder::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	for (int i = 0; i < count; i++) {
		wDeltaSigma[stage][i + offset] += ds[i];
	}
	return 0;
}
double* Adder::GetDeltaSigma(int stage) {
	return wDeltaSigma[stage];
}

// Flatten

Flatten::Flatten(void) :Layer() {
	name = "Flatten";
}

Flatten::~Flatten(void) {
	if (EnterCount) {	
		Clear();
		int s = layers.size();
		for (int i = 0; i < s; i++) {
			delete layers[i];
		}
		delete[] g;
		delete[] v;
		delete[] m;
		EnterCount = 0;		
	}
}

void Flatten::SetEnter(double* enter) {
	if (enter) {
		double* Enter = new double[EnterCount];
		for (int k = 0; k < EnterCount; k++) {
			Enter[k] = enter[k];
		}
		Enters.push_back(Enter);
		double* Out = new double[ExitCount];
		for (int k = 0; k < ExitCount; k++) {
			Out[k] = 0;
		}

		Outs.push_back(Out);
	}
}

int Flatten::FindIndex(int& count) {
	int a = layers[0]->GetCoeffAmount();
	int i = 0;
	while (count >= a) {
		count -= a;
		i++;
		a = layers[i]->GetCoeffAmount();
	}
	return i;
}

void Flatten::SetCoeff(double val, int count) {
	int i = FindIndex(count);
	layers[i]->SetCoeff(val, count);
}

double Flatten::GetCoeff(int count) {
	int i = FindIndex(count);
	return layers[i]->GetCoeff(count);
}

void Flatten::SetDelta(double val, int count) {
	int i = FindIndex(count);
	layers[i]->SetDelta(val, count);
}

double Flatten::GetDelta(int count) {
	int i = FindIndex(count);
	return layers[i]->GetDelta(count);
}

int Flatten::GetCoeffAmount(void) {
	return Amount;
}

void Flatten::Clear(void) {
	for (int i = 0; i < layers.size(); i++) {
		layers[i]->Clear();
	}
	while (!Enters.empty()) {
		delete[] Enters.back();
		Enters.pop_back();
	}
	while (!Outs.empty()) {
		delete[] Outs.back();
		Outs.pop_back();
	}
	while (!wDeltaSigma.empty()) {
		double* r = wDeltaSigma.back();
		delete[] r;
		wDeltaSigma.pop_back();
	}
	a.Clear();
}

void Flatten::SetEnterCount(int EnterCount, int NeuronCount, int ExitCount) {
	this->ExitCount = ExitCount;
	this->EnterCount = EnterCount;
	this->NeuronCount = NeuronCount;
	w = 0;
	DeltaW = 0;
	//DeltaSigma = new double[NeuronCount];
	//Enter = 0;
	//Out = new double[ExitCount];
	int am = 0;
	for (int i = 0; i < layers.size(); i++) {
		am += layers[i]->GetCoeffAmount();
	}
	Amount = am;
	g = new double[am];
	v = new double[am];
	m = new double[am];
	a.SetCount(EnterCount);
}

void Flatten::Execute(double* Enter) {	
	a.SetEnter(0);
	SetEnter(Enter);
	int size = layers.size();
	int count_e = 0;
	int count_x = 0;
	for (int i = 0; i < size; i++) {
		int ec = layers[i]->GetEnterCount();
		int xc = layers[i]->GetExitCount();
		layers[i]->Execute(Enter + count_e);
		int lc = layers[i]->GetEntersCount();
		for (int j = 0; j < xc; j++) {
			double ex = layers[i]->GetAnswer(lc-1)[j];
			Outs.back()[j + count_x] = ex;
		}
		count_e += ec;
		count_x += xc;
	}
	if (next) {
		next->Execute(Outs.back());
	}
}

double* Flatten::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	//ClearDeltaSigma();
	double* tmp;
	int s = layers.size();
	int count_e = 0;
	int count_p = 0;

	for (int i = 0; i < s; i++) {
		int e = layers[i]->GetEnterCount();
		int p = layers[i]->GetExitCount();
		a.SetOffset(count_e, e);
		layers[i]->SetNeighbors(0, &a);
		double* ds1 = 0;
		if (ds) ds1 = ds + count_p;
		if (exit == flag)
			layers[i]->Backpropagation(ds1, stage, stage);
		else
			layers[i]->Backpropagation(ds1, stage, stage, exit - count_p);
		count_p += p;
		count_e += e;
	}
	
	if (stage == 0 && prev) {
		int c = GetEntersCount() - 1;
		for (int s = c; s >= 0; s--) {
			double max = 0;
			for (int i = 0; i < EnterCount; i++) {
				if (max < fabs(a.GetDeltaSigma(s)[i])) {
					max = fabs(a.GetDeltaSigma(s)[i]);
				}
			}
			if (max > 0) {
				double* tmp = prev->Backpropagation(a.GetDeltaSigma(s), s, s);
				if (tmp)
					delete[] tmp;
			}
		}
	}
	return 0;
}

int Flatten::DropCount(void) {
	int dc = 0;
	for (int i = 0; i < layers.size(); i++) {
		dc += layers[i]->DropCount();
	}
	return dc;
}

void Flatten::GetGradient(double* Gradient, int offset) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->GetGradient(Gradient, offset);
		offset += layers[i]->GetCoeffAmount();
	}
}

void Flatten::GetGradientWithout(double* Gradient, int offset) {
	int a = GetCoeffAmount();
	double* gr = new double[a];
	GetGradient(gr, 0);
	int s = layers.size();
	int k = 0;
	int o = 0;
	for (int i = 0; i < s; i++) {
		int p = layers[i]->GetCoeffAmount();
		for (int j = 0; j < p; j++) {
			if (!layers[i]->IsDrop(j)) {
				Gradient[offset + k] = gr[o + j];
				k++;
			}
		}
		o += p;
	}
	delete[] gr;
}

void Flatten::SetDifferenceWithout(double* Delta) {
	int p = GetCoeffAmount() - DropCount();
	for (int i = Amount - 1; i >= 0; i--) {
		if (!IsDrop(i)) {
			SetDelta(Delta[--p], i);
		}
		else {
			SetDelta(0, i);
		}
	}
}

void Flatten::CalcDelta(double Alpha) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->CalcDelta(Alpha);
	}
}

void Flatten::FreeDelta(void) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->FreeDelta();
	}
	for (int i = 0; i < Amount; i++) {
		g[i] = 0;
		v[i] = 0;
		m[i] = 0;
	}
}

void Flatten::ClearDeltaSigma(void) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->ClearDeltaSigma();
	}
	
	for (int s = 0; s < wDeltaSigma.size(); s++) {
		for (int i = 0; i < EnterCount; i++) {
			wDeltaSigma[s][i] = 0;			
		}
	}
	a.ClearDeltaSigma();	
}

void Flatten::ApplyDropout(double x) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->ApplyDropout(x);
	}
}

bool Flatten::IsDrop(int count) {
	int i = FindIndex(count);
	return layers[i]->IsDrop(count);
}

void Flatten::GetInfo(int layer, int& n, int& m, double*& array) {
	layers[layer]->GetLayerInfo(n, m, array);
}

Layer* Flatten::ExtractLayer(int i) {
	return layers[i];
}

void Flatten::AddLayer(Layer* l) {
	layers.push_back(l);
}

int Flatten::LayersCount(void) {
	return layers.size();
}

void Flatten::Linking(Layer* next, Layer* prev) {
	int lsize = this->layers.size();
	for (int i = 0; i < lsize; i++) {
		this->layers[i]->SetNeighbors(0, 0);
	}
}

NN::NN(void) :Flatten() {
	name = "NN";
}

NN::~NN(void) {
	if (EnterCount) {
		Clear();
		int s = layers.size();
		for (int i = 0; i < s; i++) {
			delete layers[i];
		}
		delete[] g;
		delete[] v;
		delete[] m;
		EnterCount = 0;
	}
}

void NN::Linking(Layer* next, Layer* prev) {
	int lsize = this->layers.size();
	for (int i = 0; i < lsize; i++) {
		Layer* next1 = next;
		Layer* prev1 = prev;
		if (i > 0) {
			prev1 = layers[i - 1];
		}
		if (i < layers.size() - 1) {
			next1 = layers[i + 1];
		}
		this->layers[i]->SetNeighbors(next1, prev1);
	}
}

void NN::Load(string FileName) {
	int ec1, nc1, xc1, first = true;
	stack<Flatten*> ls;
	ls.push(this);
	stack<int> sec;
	stack<int> snc;
	stack<int> sxc;
	Layer* prev = 0;
	//if (FileExists(FileName.c_str()))
	{
		int s = layers.size();
		for (int i = 0; i < s; i++) {
			delete layers[i];
		}
		layers.clear();
		ifstream f;
		f.open(FileName.c_str());
		string text;
		while (std::getline(f, text)) {
			Layer* l;
			if (text == "Embedding") {
				l = new Embedding;
			}
			else if (text == "LSTM") {
				l = new LSTM;
			}
			else if (text == "DenseSoftmax") {
				l = new DenseSoftmax;
			}
			else if (text == "DenseSigmoid") {
				l = new DenseSigmoid;
			}
			else if (text == "Dense") {
				l = new Dense;
			}
			else if (text == "Flatten") {
				ls.push(new Flatten);
				l = ls.top();
			}
			else if (text == "Exit") {
				Flatten* f = ls.top();
				f->SetEnterCount(sec.top(), snc.top(), sxc.top());
				ls.pop();
				ls.top()->AddLayer(f);
				sec.pop(); snc.pop(); sxc.pop();
				continue;
			}
			else {
				break;
			}
			int ec, nc, xc;
			string text, s, word;
			std::getline(f, text);
			ec = atoi(text.c_str());
			std::getline(f, text);
			nc = atoi(text.c_str());
			std::getline(f, text);
			xc = atoi(text.c_str());
			if (first) {
				ec1 = ec;
				nc1 = nc;
				first = false;
			}
			xc1 = xc;
			if (l->GetName() == "Flatten") {
				sec.push(ec);
				snc.push(nc);
				sxc.push(xc);
				continue;
			}
			l->SetEnterCount(ec, nc, xc);
			std::getline(f, text);
			s = text;
			SwapComma(s);
			int count1 = l->GetCoeffAmount();
			for (int r = 0; r < count1; r++) {
				word = ReadWord(s);
				double we = atof(word.c_str());
				l->SetCoeff(we, r);
			}
			ls.top()->AddLayer(l);
			prev = l;
		}
	}
	SetEnterCount(ec1, nc1, xc1);
	Linking(0, 0);
}

void NN::Save(string FileName) {
	std::stringstream ss;
	ss.precision(16);
	stack<Flatten*> ls;
	stack<int> index;
	ls.push(this);
	int i = 0;
	while (!ls.empty()) {
		Flatten* f = ls.top();
		Layer* l = f->ExtractLayer(i);
		ss << l->GetName() << endl;
		ss << l->GetEnterCount() << endl;
		ss << l->GetNeuronCount() << endl;
		ss << l->GetExitCount() << endl;
		if (l->GetName() == "Flatten") {
			index.push(i);
			ls.push((Flatten*)l);
			i = 0;
			continue;
		}
		int Amount = l->GetCoeffAmount();
		for (int k = 0; k < Amount; k++) {
			ss << l->GetCoeff(k) << " ";
		}
		ss << endl;
		i++;
		while (i >= ls.top()->LayersCount()) {
			ls.pop();
			if (ls.empty()) break;
			ss << "Exit" << endl;
			i = index.top() + 1;
			index.pop();
		}
	}
	std::string s = ss.str();
	SwapDot(s);
	ofstream f;
	f.open(FileName.c_str());
	f << s;
	f.close();
}

void NN::GetNumericalGradient(const vector<double>& tests, double* Gradient, int offset) {
	double delta = 1.0E-6;
	double* enter = new double[20];
	double* Gr = new double[Amount];
	//int ec=EnterCount;
	Clear();
	int tc = tests.size() / 4;
	for (int k = 0; k < tc; k++) {
		//for (int k = 0; k < look_back; k++) {
		for (int j = 0; j < 4; j++) {
			enter[j] = tests[k * 4 + j];
		}
		Execute(enter);
	}
	double* tmp = Backpropagation(0, tc - 1, 0, 0);
	if (tmp) delete[] tmp;	
	GetGradient(Gr, 0);
	for (int i = 0; i < Amount; i++) {
		double derivative;
		double ww = GetCoeff(i);
		SetCoeff(ww - delta, i);
		Clear();
		for (int k = 0; k < tc; k++) {
			//for (int k = 0; k < look_back; k++) {
			for (int j = 0; j < 4; j++) {
				enter[j] = tests[k * 4 + j];
			}
			Execute(enter);
		}
		double r1 = GetAnswer(tc - 1)[0];
		SetCoeff(ww + delta, i);
		Clear();
		for (int k = 0; k < tc; k++) {
			//for (int k = 0; k < look_back; k++) {
			for (int j = 0; j < 4; j++) {
				enter[j] = tests[k * 4 + j];
			}
			Execute(enter);
		}
		double r2 = GetAnswer(tc - 1)[0];
		derivative = (r2 - r1) / (2 * delta);
		SetCoeff(ww, i);
		Gradient[offset + i] = derivative;
		if (fabs(derivative - Gr[i]) > 1.0E-9) {
			double d = derivative;
			double r = Gr[i];
			double tmp = r / d;
			d -= r;
			d = d;
		}
	}
	delete[] enter;
	double sum = 0;
	for (int i = 0; i < Amount; i++) {
		double d = Gradient[offset + i];
		d -= Gr[i];
		sum += fabs(d);
	}
	sum = sum;
	delete[] Gr;
}

void NN::CalcNumericalDelta(const vector<double>& tests, double* r, double Alpha) {
	double* Gradient = new double[Amount];
	for (int i = 0; i < Amount; i++) {
		Gradient[i] = 0;
	}
	GetNumericalGradient(tests, Gradient, 0);
	for (int k = 0; k < Amount; k++) {
		double kof = GetDelta(k);
		kof = kof * Alpha + Gradient[k] * r[0];
		SetDelta(kof, k);
	}
	delete[] Gradient;
}

void NN::AdaMax(int n, double beta1, double beta2) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->CalcDelta(0);
	}
	int fc = Amount;

	//double beta1=0.9l;
	//double beta2=0.999l;
	double epsilon = 1E-8l;

	double gmax = 0;
	for (int k = 0; k < fc; k++) {
		g[k] = GetDelta(k);
		if (fabsl(g[k]) > gmax) gmax = fabsl(g[k]);
	}

	// AdaMax
	for (int k = 0; k < fc; k++) {
		m[k] = beta1 * m[k] + (1 - beta1) * g[k];
		if (beta2 * v[k] > gmax)
			v[k] = beta2 * v[k];
		else
			v[k] = gmax;
		double delta = (1.0 / (1.0 - pow(beta1, n + 1))) * m[k] / v[k];
		SetDelta(delta, k);
	}
}

void NN::Execute(double* Enter) {
	SetEnter(Enter);
	layers[0]->Execute(Enter);
	int last = layers.size() - 1;
	//int s = layers[last]->GetNeuronCount();	
	int ec = GetEntersCount();
	int xc = GetExitCount();
	for (int i = 0; i < xc; i++) {
		double ex = layers[last]->GetAnswer(ec - 1)[i];
		Outs.back()[i] = ex;
	}
}

double* NN::Backpropagation(double* ds, int stage, int last_stage, int exit) {
	int s = layers.size() - 1;
	double* tmp = layers[s]->Backpropagation(ds, stage, last_stage, exit);
	if (tmp) {
		delete[] tmp;
	}
	return 0;
}

void NN::Adam(int n, double beta1, double beta2) {
	int s = layers.size();
	for (int i = 0; i < s; i++) {
		layers[i]->CalcDelta(0);
	}
	int fc = Amount;

	//double beta1=0.9l;
	//double beta2=0.999l;
	double epsilon = 1E-8l;

	double gmax = 0;
	for (int k = 0; k < fc; k++) {
		g[k] = GetDelta(k);
		if (fabsl(g[k]) > gmax) gmax = fabsl(g[k]);
	}
	// Adam
	for (int k = 0; k < fc; k++) {
		m[k] = beta1 * m[k] + (1 - beta1) * g[k];
		v[k] = beta2 * v[k] + (1 - beta2) * g[k] * g[k];
		double m_;
		double v_;
		m_ = m[k] / (1 - powl(beta1, n + 1));
		v_ = v[k] / (1 - powl(beta2, n + 1));
		double delta = m_ / (sqrt(v_) + epsilon);
		SetDelta(delta, k);
	}
}

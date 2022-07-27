//---------------------------------------------------------------------------
#ifndef NeuronH
#define NeuronH
//---------------------------------------------------------------------------

#include<string>
#include<stack>
#include<vector>

using namespace std;
double SigmaFunc(double Sigma);
double TanhFunc(double Sigma);
string ReadWord(string& s);
void SwapDot(string& s);
void SwapDot(wstring& s);
void SwapComma(string& s);
void SwapComma(wstring& s);

	class Layer {
	public:
		static const int flag;
		Layer(void);
		virtual ~Layer(void);
		virtual void SetCoeff(double val, int count);
		virtual double GetCoeff(int count);
		virtual void SetDelta(double val, int count);
		virtual double GetDelta(int count);

		void SetNeighbors(Layer* next, Layer* prev);
		int GetCoeffAmount(void);
		int GetEnterCount(void);
		int GetNeuronCount(void);
		int GetExitCount(void);
		void Fill(int flag, double Amplitude = 1.0);
		double* GetAnswer(int stage);
		double* GetDeltaSigma(int stage);
		double* GetDelta(void);
		virtual void FreeDelta(void);
		void SetDifference(const double* Delta);
		void AplyError(double h);
		string GetName(void);
		int GetEntersCount(void);
		//virtual void SetEnter(const double* enter, const int count);
		virtual void SetEnter(const double* enter);
		virtual void GetLayerInfo(int& n, int& m, double*& array);
		virtual void ApplyDropout(double x);
		virtual int DropCount(void);
		virtual	bool IsDrop(int count);
		virtual void Clear(void);
		virtual void ClearDeltaSigma(void) = 0;
		virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount) = 0;
		virtual void Execute(double* Enter) = 0;
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag) = 0;
		virtual void GetGradient(double* Gradient, int offset) = 0;
		virtual void CalcDelta(double Alpha) = 0;				
		//virtual int PrevisionCount(void);
		//virtual void GetPrevisionInfo(double* v, int offset);
		//virtual void SetPrevisionInfo(double* v, int offset);
	protected:
		int EnterCount;
		int NeuronCount;
		int ExitCount;
		int Amount;
		double* w;
		bool* Dropout;
		double* DeltaW;
		//double* DeltaSigma;
		vector<double*> wDeltaSigma;
		//double* Enter;
		vector<double*>  Enters;
		vector<double*>  Outs;
		Layer* next;
		Layer* prev;
		string name;
		double pd;
		int dropcount;		
	};

	class Dense : public Layer {
	public:
		Dense(void);
		virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		virtual void GetGradient(double* Gradient, int offset);
		virtual void CalcDelta(double Alpha);		
		virtual void ClearDeltaSigma(void);
	};

	class DenseSoftmax : public Dense {
	public:
		DenseSoftmax(void);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
	};

	class DenseSigmoid : public Dense {
	public:
		DenseSigmoid(void);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
	};

	class LSTM : public Layer {
	public:
		LSTM(void);
		virtual ~LSTM(void);
		void SetEnter(const double* enter);
		virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		virtual void GetGradient(double* Gradient, int offset);
		virtual void CalcDelta(double Alpha);
		virtual void Clear(void);
		void ClearDeltaSigma(void);
	private:
		int CoeffCount;
		vector<double*> Stack;
		vector<vector<double*> > Sigma;
		vector<vector<double*> > DeltaSigmas;
		vector<vector<double> >  Pipe;		
		vector<double> Exit;
		vector<double> C;
		int NewStage(void);

		double* c1;
		double* de;
		double* dc;
		double* prevf;
	};

	class Embedding : public Layer {
	public:
		Embedding(void);
		virtual ~Embedding(void);
		virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		virtual void GetGradient(double* Gradient, int offset);
		virtual void CalcDelta(double Alpha);		
		virtual void ClearDeltaSigma(void);
		virtual void GetLayerInfo(int& n, int& m, double*& array);
	};
	class Adder : public Dense {
	public:
		Adder(void);
		void SetCount(int Count);
		void SetOffset(int offset, int count);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		double* GetDeltaSigma(int stage);
	private:
		int offset;
		int count;
	};

	class Flatten : public Layer {
	public:
		Flatten(void);
		virtual ~Flatten(void);
		void SetEnter(double* enter);
		virtual void SetCoeff(double val, int count);
		virtual double GetCoeff(int count);
		virtual void SetDelta(double val, int count);
		virtual double GetDelta(int count);
		int GetCoeffAmount(void);
		virtual void SetEnterCount(int EnterCount, int NeuronCount, int ExitCount);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		virtual void GetGradient(double* Gradient, int offset);
		virtual void GetGradientWithout(double* Gradient, int offset);
		virtual void CalcDelta(double Alpha);		
		virtual void Clear(void);
		virtual void FreeDelta(void);
		virtual void ClearDeltaSigma(void);
		virtual void ApplyDropout(double x);
		virtual int DropCount(void);
		virtual bool IsDrop(int count);
		void GetInfo(int layer, int& n, int& m, double*& array);
		void SetDifferenceWithout(double* Delta);
		Layer* ExtractLayer(int i);
		void AddLayer(Layer* l);
		int LayersCount(void);
	protected:
		Adder a;
		double* g;
		double* v;
		double* m;
		vector<Layer*> layers;
		int FindIndex(int& count);
		virtual void Linking(Layer* next, Layer* prev);
	};
	

	class NN : public Flatten {
	public:
		NN(void);
		virtual ~NN(void);
		void Load(string FileName);
		void Save(string FileName);
		void GetNumericalGradient(const vector<double>& tests, double* Gradient, int offset);
		void CalcNumericalDelta(const vector<double>& tests, double* r, double Alpha);
		virtual void Execute(double* Enter);
		virtual double* Backpropagation(double* ds, int stage, int last_stage, int exit = flag);
		void AdaMax(int n, double beta1, double beta2);
		void Adam(int n, double beta1, double beta2);
	protected:
		virtual void Linking(Layer* next, Layer* prev);
	};


#endif
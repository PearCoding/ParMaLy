struct ParseResult {
	bool Successful;
	void* Return;
};

class Parser {
protected:
/* Tokens returned by the lexer */
enum BUToken {
	_BUT_EOF=0,
	BUT_0,	// (
	BUT_1,	// )
};

	virtual BUToken nextToken(void** param) = 0;
public:
	ParseResult parse();
};

/* Utility functions */
typedef uint64_t table_entry_t;
typedef uint64_t state_t;
enum ActionType {AT_INVALID=0, AT_ACCEPT, AT_REDUCE, AT_SHIFT};
inline ActionType extractAction(table_entry_t entry) { return (ActionType)((0xC000000000000000 & entry) >> 0x3E); }
inline state_t extractState(table_entry_t entry) { return (0x3FFFFFFFFFFFFFFF & entry); }

/* Rule groups defined by the grammar */
enum BURuleGroup {
	BURG_0=0,	// goal
	BURG_1=1,	// list
	BURG_2=2,	// pair
};

/* Action Table */
constexpr int LookaheadCount = 1;
constexpr int TokenCount = 3;
constexpr int StateCount = 8;
static uint64_t indirectTokenMap[3]={
1,0,2
};
const static table_entry_t actionTable[]={
	0xC000000000000003, 0, 0, 
	0xC000000000000003, 0x4000000000000000, 0, 
	0x8000000000000002, 0x8000000000000002, 0, 
	0xC000000000000003, 0, 0xC000000000000006, 
	0x8000000000000001, 0x8000000000000001, 0, 
	0, 0, 0xC000000000000007, 
	0x8000000000000004, 0x8000000000000004, 0x8000000000000004, 
	0x8000000000000003, 0x8000000000000003, 0x8000000000000003, 
};

/* Goto Table */
constexpr int GroupCount = 3;
const static uint64_t indirectGroupMap[3]={
0,1,2
};
const static state_t gotoTable[]={
	0, 0x1, 0x2, 
	0, 0, 0x4, 
	0, 0, 0, 
	0, 0, 0x5, 
	0, 0, 0, 
	0, 0, 0, 
	0, 0, 0, 
	0, 0, 0, 
};

/* Glue Tables */
const static BURuleGroup rule2GroupTable[]={
BURG_0,BURG_1,BURG_1,BURG_2,BURG_2
};
const static uint64_t ruleBetaTable[]={
1,2,1,3,2
};

/* Parse Function */
ParseResult Parser::parse() {
	void* lexerParam = nullptr;
	std::stack<uint64_t> stack;
	stack.push(_BUT_EOF);
	stack.push(0);
	BUToken currentToken = nextToken(&lexerParam);
	while(true){
		state_t state = stack.top();
		table_entry_t entry = actionTable[state*TokenCount + indirectTokenMap[currentToken]];
		ActionType action = extractAction(entry);
		uint64_t suffix = extractState(entry);
		switch(action){
		case AT_INVALID: return ParseResult{false, nullptr};
		case AT_ACCEPT:
			return ParseResult{true, nullptr};
		case AT_REDUCE: {
			for(int i = 0; i < 2*ruleBetaTable[suffix]; ++i) stack.pop();
			state = stack.top();
			uint64_t grp = rule2GroupTable[suffix];
			stack.push(grp);
			stack.push(gotoTable[state*GroupCount + indirectGroupMap[grp]]);
			} break;
		case AT_SHIFT:
			stack.push(currentToken);
			stack.push(suffix);
			currentToken = nextToken(&lexerParam);
			break;
		}
	}
	return ParseResult{true, nullptr};
};


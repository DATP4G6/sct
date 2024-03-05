grammar Sct;

start: program EOF;
program: function program | agent_def program |;

// Functions
function:
	FUNCTION ID LPAREN args_def RPAREN RIGHT_ARROW type LCURLY statement RCURLY;

FUNCTION: 'function';
RIGHT_ARROW: '->';

// All argument definitions are here
args_def: args_def_p |;
args_def_p: type ID | type ID COMMA args_def_p;

args_entity: args_entity_p |;
args_entity_p: ID COLON expression args_entity_pp;
args_entity_pp: COMMA args_entity_p |;

args_call: args_call_p |;
args_call_p: expression args_call_pp;
args_call_pp: COMMA args_call_p |;

type: T_INT | T_FLOAT | T_VOID;
T_INT: 'int';
T_FLOAT: 'float';
T_VOID: 'void';

// Statements
statement: statement_p statement_pp;
statement_pp: statement |;
statement_p:
	expression ';'
	| declaration
	| assignment
	| if
	| while
	| enter
	| return
	| create
	| destroy
	| exit;

declaration: type ID ASSIGN expression SEMI;
assignment: ID ASSIGN expression SEMI;
if: IF LPAREN expression RPAREN LCURLY statement RCURLY else;
else: ELSE LCURLY statement RCURLY |;
while: WHILE LPAREN expression RPAREN LCURLY statement RCURLY;
enter: ENTER ID SEMI;
return: RETURN return_p SEMI;
return_p: expression |;
create: CREATE entity_create SEMI;
destroy: DESTROY SEMI;
exit: EXIT SEMI;

ASSIGN: '=';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';
ENTER: 'enter';
CREATE: 'create';
DESTROY: 'destroy';
EXIT: 'exit';
// Societal Construction Tool 

// Expressions
expression: exp;
exp:
	LIT
	| ID
	| LPAREN exp RPAREN
	| call
	| unary_minus = MINUS exp
	| not = NOT exp
	| exp mult = MULT exp
	| exp div = DIV exp
	| exp mod = MOD exp
	| exp plus = PLUS exp
	| exp minus = MINUS exp
	| exp gt = GT exp
	| exp lt = LT exp
	| exp gte = GTE exp
	| exp lte = LTE exp
	| exp eq = EQ exp
	| exp neq = NEQ exp
	| exp and = AND exp
	| exp or = OR exp
	| entity_predicate;

MULT: '*';
DIV: '/';
PLUS: '+';
MINUS: '-';
MOD: '%';
AND: '&&';
OR: '||';
EQ: '==';
NEQ: '!=';
GT: '>';
LT: '<';
GTE: '>=';
LTE: '<=';
NOT: '!';

call: ID LPAREN args_call RPAREN;

// Agents
agent_def:
	AGENT ID LPAREN args_def RPAREN LCURLY agent_body RCURLY;
agent_body: state agent_body | function agent_body |;
state: STATE ID LCURLY statement RCURLY;

AGENT: 'agent';
STATE: 'state';

// Not using literals probably allows WS arround '::'
entity_create: ID DBL_COLON ID LPAREN args_entity RPAREN;
entity_predicate:
	ID DBL_COLON (ID | QUESTION) LPAREN args_entity RPAREN;

// Current implementation allows '?' to be used as an ID instead of only in predicate states
ID: [a-zA-Z_][a-zA-Z_0-9]*;
// ID: [a-zA-Z_][a-zA-Z_0-9]* | [?];
LIT: INT | FLOAT;
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;

WS: [ \t\r\n]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;

QUESTION: '?';
LPAREN: '(';
RPAREN: ')';
LCURLY: '{';
RCURLY: '}';
SEMI: ';';
COMMA: ',';
COLON: ':';
DBL_COLON: '::';
grammar sct;

program: function program | agent_def program | EOF;

function:
	'function ' ID '(' args_def ') ->' type ' {' statement '}';

args_def: args_def_p |;
args_def_p: type ID | type ID ', ' args_def_p;

args_entity: args_entity_p |;
args_entity_p: ID ':' expression args_entity_pp;
args_entity_pp: ',' args_entity_p |;

args_call: args_call_p |;
args_call_p: expression args_call_pp;
args_call_pp: ',' args_call_p |;

type: 'int' | 'float' | 'void';

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

// Sub statements

declaration: type ID '=' expression ';';
assignment: ID '=' expression ';';
if: 'if' '(' expression ')' '{' statement '}' else;
else: 'else' '{' statement '}' |;
while: 'while' '(' expression ')' '{' statement '}';
enter: 'enter' ID ';';
return: 'return' return_p ';';
return_p: expression |;
create: 'create' entity_create ';';
destroy: 'destroy;';
exit: 'exit;';

// end sub statement expression[int pr]: ID | Num | ;
expression: exp;
exp:
	exp mult = '*' exp
	| exp div = '/' exp
	| exp plus = '+' exp
	| exp minus = '-' exp
	| exp eq = '==' exp
	| exp neq = '!=' exp
	| exp gt = '>' exp
	| exp lt = '<' exp
	| exp gte = '>=' exp
	| exp lte = '<=' exp
	| unary_minus = '-' exp
	| not = '!' exp
	| call
	| '(' exp ')'
	| ID
	| LIT;

call: ID '(' args_call ')';

agent_def: 'agent' ID '(' args_def ') {' agent_body '}';
agent_body: state agent_body | function agent_body |;
state: 'state' ID '{' statement '}';

entity_create: ID '::' ID '(' args_entity ')';
entity_predicate: ID '::' ID_p '(' args_entity ')';

ID: [a-zA-Z_][a-zA-Z_0-9]*;
ID_p: ID | '?';
LIT: INT | FLOAT;
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;

WS: [ \t\r\n]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;
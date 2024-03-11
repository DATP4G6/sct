grammar Sct;

start: program EOF;
program: function program | agent_def program |;

// Functions
function:
	FUNCTION ID LPAREN args_def RPAREN RIGHT_ARROW type LCURLY statement_list RCURLY;

// All argument definitions are here
// Maybe these should have seperate list and element rules like statement_list?
args_def: (type ID (COMMA type ID)*)?;
args_entity: (ID COLON expression (COMMA ID COLON expression)*)?;
args_call: (expression (COMMA expression)*)?;

type: T_INT | T_FLOAT | T_VOID;

// Statements
statement_list: statement+;
statement:
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
if: IF LPAREN expression RPAREN LCURLY statement_list RCURLY else;
else: ELSE LCURLY statement_list RCURLY |;
while: WHILE LPAREN expression RPAREN LCURLY statement_list RCURLY;
enter: ENTER ID SEMI;
return: RETURN (expression)? SEMI;
create: CREATE entity_create SEMI;
destroy: DESTROY SEMI;
exit: EXIT SEMI;
// Societal Construction Tool 

// Expressions
expression: exp;
exp:
	LIT							# LitteralExpression
	| ID						# IDExpression
	| LPAREN exp RPAREN			# ParenthesisExpression
	| LPAREN type RPAREN exp	# TypecastExpression
	| call						# CallExpression
	| unary_minus = MINUS exp	# UnaryMinusExpression
	| not = NOT exp				# LogicalNotExpression
	| exp mult = MULT exp		# MultiplicationExpression
	| exp div = DIV exp			# DivisionExpression
	| exp mod = MOD exp			# ModExpression
	| exp plus = PLUS exp		# PlusExpression
	| exp minus = MINUS exp		# MinusExpression
	| exp gt = GT exp			# GreaterThanExpression
	| exp lt = LT exp			# LessThanExpression
	| exp gte = GTE exp			# GreaterThanEqualExpression
	| exp lte = LTE exp			# LessThanEqualExpression
	| exp eq = EQ exp			# EqualExpression
	| exp neq = NEQ exp			# NotEqualExpression
	| exp and = AND exp			# LogicalAndExpression
	| exp or = OR exp			# LogicalOrExpression
	| entity_predicate			# EntityPredicateExpression;

call: ID LPAREN args_call RPAREN;

// Agents
agent_def:
	AGENT ID LPAREN args_def RPAREN LCURLY agent_body RCURLY;
agent_body:
	state agent_body
	| function agent_body
	| decorator agent_body
	|;
decorator: DECORATOR ID LCURLY statement_list RCURLY;
state: (state_decorator)* STATE ID LCURLY statement_list RCURLY;
state_decorator: AT ID;

// Not using literals probably allows WS arround '::'
entity_create: ID DBL_COLON ID LPAREN args_entity RPAREN;
entity_predicate:
	ID DBL_COLON (ID | QUESTION) LPAREN args_entity RPAREN;

// Current implementation allows '?' to be used as an ID instead of only in predicate states

FUNCTION: 'function';
RIGHT_ARROW: '->';

T_INT: 'int';
T_FLOAT: 'float';
T_VOID: 'void';

ASSIGN: '=';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';
ENTER: 'enter';
CREATE: 'create';
DESTROY: 'destroy';
EXIT: 'exit';

CAST: 'cast';
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

AGENT: 'agent';
STATE: 'state';
DECORATOR: 'decorator';
AT: '@';

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
ID: [a-zA-Z_][a-zA-Z_0-9]*;
LIT: INT | FLOAT;
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;

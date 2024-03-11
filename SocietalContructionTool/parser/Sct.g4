grammar Sct;

start: (function | class_def)* EOF;

// Functions
function:
	FUNCTION ID LPAREN args_def RPAREN RIGHT_ARROW type LCURLY statement_list RCURLY;

// All argument definitions are here
args_def: (type ID (COMMA type ID)*)?;
args_agent: (ID COLON expression (COMMA ID COLON expression)*)?;
args_call: (expression (COMMA expression)*)?;

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
if:
	IF LPAREN expression RPAREN LCURLY statement_list RCURLY (
		elseif
		| else
	)?;
elseif:
	ELSE IF LPAREN expression RPAREN LCURLY statement_list RCURLY (
		elseif
		| else
	)?;
else: ELSE LCURLY statement_list RCURLY;
while:
	WHILE LPAREN expression RPAREN LCURLY statement_list RCURLY;
enter: ENTER ID SEMI;
return: RETURN expression? SEMI;
create: CREATE agent_create SEMI;
destroy: DESTROY SEMI;
exit: EXIT SEMI;

// Expressions
expression:
	LIT									# LitteralExpression
	| ID								# IDExpression
	| LPAREN expression RPAREN			# ParenthesisExpression
	| LPAREN type RPAREN expression		# TypecastExpression
	| call								# CallExpression
	| unary_minus = MINUS expression	# UnaryMinusExpression
	| not = NOT expression				# LogicalNotExpression
	| expression op = MULT expression	# BinaryExpression
	| expression op = DIV expression	# BinaryExpression
	| expression op = MOD expression	# BinaryExpression
	| expression op = PLUS expression	# BinaryExpression
	| expression op = MINUS expression	# BinaryExpression
	| expression op = GT expression		# BinaryExpression
	| expression op = LT expression		# BinaryExpression
	| expression op = GTE expression	# BinaryExpression
	| expression op = LTE expression	# BinaryExpression
	| expression op = EQ expression		# BinaryExpression
	| expression op = NEQ expression	# BinaryExpression
	| expression op = AND expression	# BinaryExpression
	| expression op = OR expression		# BinaryExpression
	| agent_predicate					# AgentPredicateExpression;

call: ID LPAREN args_call RPAREN;

// Class
class_def:
	CLASS ID LPAREN args_def RPAREN LCURLY class_body RCURLY;
class_body: (state | function | decorator)*;

decorator: DECORATOR ID LCURLY statement_list RCURLY;
state: (state_decorator)* STATE ID LCURLY statement_list RCURLY;
state_decorator: AT ID;

// Not using literals probably allows WS arround '::'
agent_create: ID DBL_COLON ID LPAREN args_agent RPAREN;
agent_predicate:
	ID DBL_COLON (ID | QUESTION) LPAREN args_agent RPAREN;

FUNCTION: 'function';
RIGHT_ARROW: '->';

type: T_INT | T_FLOAT | T_VOID;
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

CLASS: 'class';
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
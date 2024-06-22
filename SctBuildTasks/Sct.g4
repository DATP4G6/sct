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
statement_list: statement*;
statement:
    expression_statement SEMI   # ExpressionStatement
    | declaration               # StatementTag
    | assignment                # StatementTag
    | if                        # StatementTag
    | while                     # StatementTag
    | enter                     # StatementTag
    | conditional_enter         # StatementTag
    | return                    # StatementTag
    | create                    # StatementTag
    | destroy                   # StatementTag
    | exit                      # StatementTag
    | break                     # StatementTag
    | continue                  # StatementTag;

// All expressions which are allowed as standalone statements
expression_statement: call_expression;

declaration:
    type ID ASSIGN expression SEMI # variableDeclaration;
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
conditional_enter: expression DOUBLE_RIGHT_ARROW ID SEMI;
return: RETURN expression? SEMI;
create: CREATE agent_create SEMI;
destroy: DESTROY SEMI;
exit: EXIT SEMI;
break: BREAK SEMI;
continue: CONTINUE SEMI;

// Expressions
expression:
    literal                                                         # LiteralExpression
    | ID                                                            # IDExpression
    | LPAREN expression RPAREN                                      # ParenthesisExpression
    | LPAREN type RPAREN expression                                 # TypecastExpression
    | call_expression                                               # Ignore
    | unary_minus = MINUS expression                                # UnaryMinusExpression
    | not = NOT expression                                          # LogicalNotExpression
    | HASH expression                                               # HashPredicateExpression
    | expression op = (MULT | DIV | MOD) expression                 # BinaryExpression
    | expression op = (PLUS | MINUS) expression                     # BinaryExpression
    | expression op = (GT | LT | GTE | LTE | EQ | NEQ) expression   # BooleanExpression
    | expression op = AND expression                                # BooleanExpression
    | expression op = OR expression                                 # BooleanExpression
    | agent_predicate                                               # AgentPredicateExpression
    ;

call_expression: ID LPAREN args_call RPAREN #CallExpression;

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
DOUBLE_RIGHT_ARROW: '=>';

type: T_INT | T_FLOAT | T_VOID | T_PREDICATE;
T_INT: 'int';
T_FLOAT: 'float';
T_VOID: 'void';
T_PREDICATE: 'Predicate';

ASSIGN: '=';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';
ENTER: 'enter';
CREATE: 'create';
DESTROY: 'destroy';
EXIT: 'exit';
CONTINUE: 'continue';
BREAK: 'break';

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

CLASS: 'species';
STATE: 'state';
DECORATOR: 'decorator';
AT: '@';

WS: [ \t\r\n]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;

HASH: '#';
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
literal: INT | FLOAT;
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;

//Catches all tokens not specified above
UNIDENTIFIED:.;

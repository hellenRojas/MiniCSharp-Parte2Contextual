lexer grammar Lexer1;

WS:	(' ')-> channel(HIDDEN)
;

NEWLINE 
: ('\n'|'\r'|'\t')-> channel(HIDDEN)
;

COMMMETBLOCK
: ('/*' ('0'..'9'|'A'..'Z'| 'a'..'z'|'\n'|'\r'|'\t'|' ' |CARACTERESCOMENTBLOCK|COMMMETBLOCK)* '*/')-> channel(HIDDEN)
;


COMMMET
: ('//' ('0'..'9'|'A'..'Z'| 'a'..'z'|' ' |PrintableChar|COMENTBLOCKCHAR1|COMENTBLOCKCHAR1)* )-> channel(HIDDEN)
;




IN
: 'in'
;


VOID
: 'void'
;


CONDICION_IF
: 'if'
;

CONDICION_ELSE_IF
: 'else if'
;

CONDICION_ELSE
: 'else'
;

CICLO_WHILE
: 'while'
;

CICLO_FOR
: 'for'
;
CICLO_FOREACH
: 'foreach'
;
BREAK
: 'break'
;

RETURN
: 'return'
;

READ
: 'read'
;

WRITE
: 'write'
;

CLASE
: 'class'
;

NEW
: 'new'
;

CONSTANTE
: 'const'
;
TRUE
: 'true'
;

FALSE
: 'false'
;



PyCOMA : ';' ;
COMA : ',' ;
ASIGN : '=' ;
PIZQ : '(' ;
PDER : ')' ;
SUMA : '+' ;
MUL : '*' ;
DIV : '/';
RESTA : '-';
DIVMOD : '%';
COMPARACION : '==' ;
DIFERENTE : '!=' ;
MENOR: '<' ;
MENORIGUAL: '<=' ;
MAYOR: '>' ;
MAYORIGUAL: '>=' ;
O : '||';
Y : '&&';
INCRE : '++';
DECRE : '--';
PUNTO : '.';
PCUADRADO_IZQ: '[';
PCUADRADO_DER: ']';
COR_DER: '{';
COR_IZQ: '}';
EXCLAMACION : '!';
EXCLAMACIONA : '¡';
NUMERAL : '#';
DOLAR : '$';
AMPERSON : '&';
INTERROGACION : '?';
ARROBA : '@';
GUIONBAJO : '_';
COMILLADOBLE : '"';
VERTICAL : '|';
COMENTCHAR: '//';
COMENTBLOCKCHAR1: '/*';
COMENTBLOCKCHAR2: '*/';
DOSPUNTOS : ':';
ENE: '~';
BACKQUOTE: '`';
TECHO:'^';
SALTO:'\\n';
RETCARR:'\\r';
TAB:'\\t';
COMILLA: '\'';

STRI
: COMILLADOBLE  (LETTER|DIGIT|PrintableChar|'\r'|' ')* COMILLADOBLE
;



NUMBER
: '1'..'9' (DIGIT)*|'0'
;

FLOAT
: ('1'..'9' (DIGIT)*|'0') '.' DIGIT (DIGIT)*
;

ID
: LETTER (LETTER | DIGIT | '_')*
;


CARACTERESCOMENTBLOCK
: PyCOMA 
| COMA
| ASIGN 
| PIZQ 
| PDER 
| SUMA
| RESTA
| DIVMOD 
| COMPARACION
| DIFERENTE
| MENOR
| MENORIGUAL
| MAYOR
| MAYORIGUAL
| O 
| Y 
| INCRE
| DECRE 
| PUNTO 
| PCUADRADO_IZQ
| PCUADRADO_DER
| COR_DER
| COR_IZQ
| EXCLAMACION 
| EXCLAMACIONA 
| NUMERAL 
| DOLAR
| AMPERSON 
| INTERROGACION 
| ARROBA 
| GUIONBAJO
| VERTICAL
| COMENTCHAR
| DOSPUNTOS 
| ENE
| BACKQUOTE
| TECHO
| SALTO
|RETCARR
|TAB
|COMILLA
;


 CARACTERES
: CARACTERESCOMENTBLOCK
| MUL 
| DIV 
;


CharConst: '\'' (PrintableChar|'\n'|'\r') '\'';

fragment
LETTER: 'a'..'z' | 'A'..'Z';

fragment
DIGIT: '0'..'9';

fragment
PrintableChar: (LETTER|DIGIT|CARACTERES);


